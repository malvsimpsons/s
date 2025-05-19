using System;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState;

public class EstablishingSessionPolicy : ConnectionNegotiatingState
{
    public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY;

    public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        try
        {
            ValidateState(sessionConnectionContext);
            AwaitReservationCredentials(sessionConnectionContext);
        }
        catch (Exception)
        {
            Disconnect(sessionConnectionContext);
            throw;
        }
        return Task.CompletedTask;
    }

    private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        SessionPolicyIsNotNull(sessionConnectionContext);
    }

    private static void SessionPolicyIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        try
        {
            Validate.NotNull(sessionConnectionContext.SessionPolicy);
        }
        catch (ArgumentNullException ex)
        {
            throw new InvalidOperationException("The context is missing a session policy.", ex);
        }
    }

    private void AwaitReservationCredentials(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        sessionConnectionContext.UpdateConnectionState(new AwaitingReservationCredentials());
    }
}
