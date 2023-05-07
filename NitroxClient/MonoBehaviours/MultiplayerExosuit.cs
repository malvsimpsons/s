using NitroxClient.GameLogic.FMOD;
using NitroxClient.Unity.Smoothing;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MultiplayerExosuit : MultiplayerVehicleControl
{
    private float jetLoopingSoundDistance;

    private bool lastThrottle;
    private float timeJetsChanged;
    private Exosuit exosuit;

    protected override void Awake()
    {
        exosuit = GetComponent<Exosuit>();
        WheelYawSetter = value => exosuit.steeringWheelYaw = value;
        WheelPitchSetter = value => exosuit.steeringWheelPitch = value;
        base.Awake();
        SmoothRotation = new ExosuitSmoothRotation(gameObject.transform.rotation);
#if SUBNAUTICA
        this.Resolve<FMODWhitelist>().TryGetSoundData(exosuit.loopingJetSound.asset.path, out SoundData jetSoundData);
#elif BELOWZERO
        exosuit.boostSound.EventDescription.getPath(out var path);
        this.Resolve<FMODWhitelist>().TryGetSoundData(path, out SoundData jetSoundData);
#endif
        jetLoopingSoundDistance = jetSoundData.Radius;
    }

    internal override void Enter()
    {
        GetComponent<Rigidbody>().freezeRotation = false;
        exosuit.SetIKEnabled(true);
        exosuit.thrustIntensity = 0;
        base.Enter();
    }

    public override void Exit()
    {
        GetComponent<Rigidbody>().freezeRotation = true;
        exosuit.SetIKEnabled(false);
#if SUBNAUTICA
        exosuit.loopingJetSound.Stop();
#elif BELOWZERO
        //TODO: Check which of these or both we need to activate
        exosuit.jumpJetsSound.Stop();
        exosuit.boostSound.Stop();
#endif
        exosuit.fxcontrol.Stop(0);
        base.Exit();
    }

    internal override void SetThrottle(bool isOn)
    {
        if (timeJetsChanged + 0.3f <= Time.time && lastThrottle != isOn)
        {
            timeJetsChanged = Time.time;
            lastThrottle = isOn;
            if (isOn)
            {
#if SUBNAUTICA
                exosuit.loopingJetSound.Play();
#elif BELOWZERO
                exosuit.boostSound.Play();
#endif
                exosuit.fxcontrol.Play(0);
                exosuit.areFXPlaying = true;
            }
            else
            {
#if SUBNAUTICA
                exosuit.loopingJetSound.Play();
#elif BELOWZERO
                exosuit.boostSound.Play();
#endif
                exosuit.fxcontrol.Stop(0);
                exosuit.areFXPlaying = false;
            }
        }
    }

    private void Update()
    {
#if SUBNAUTICA
        if (exosuit.loopingJetSound.playing)
        {
            if (exosuit.loopingJetSound.evt.hasHandle())
            {
                float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
                exosuit.loopingJetSound.evt.setVolume(volume);
            }
        }
        else
        {
            if (exosuit.loopingJetSound.evtStop.hasHandle())
            {
                float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
                exosuit.loopingJetSound.evtStop.setVolume(volume);
            }
        }
#elif BELOWZERO
        if (exosuit.boostSound.IsPlaying())
        {
            if (exosuit.boostSound.EventInstance.hasHandle())
            {
                float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
                exosuit.boostSound.EventInstance.setVolume(volume);
            }
        }
        else
        {
            if (exosuit.boostSound.EventInstance.hasHandle())
            {
                float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
                exosuit.boostSound.EventInstance.setVolume(volume);
            }
        }
#endif
    }

    internal override void SetArmPositions(Vector3 leftArmPosition, Vector3 rightArmPosition)
    {
        base.SetArmPositions(leftArmPosition, rightArmPosition);
        Transform leftAim = exosuit.aimTargetLeft;
        Transform rightAim = exosuit.aimTargetRight;
        if (leftAim)
        {
            Vector3 leftAimPosition = leftAim.localPosition;
            leftAimPosition = new Vector3(leftAimPosition.x, leftArmPosition.y, leftAimPosition.z);
            leftAim.localPosition = leftAimPosition;
        }

        if (rightAim)
        {
            Vector3 rightAimPosition = rightAim.localPosition;
            rightAimPosition = new Vector3(rightAimPosition.x, rightArmPosition.y, rightAimPosition.z);
            rightAim.localPosition = rightAimPosition;
        }
    }
}
