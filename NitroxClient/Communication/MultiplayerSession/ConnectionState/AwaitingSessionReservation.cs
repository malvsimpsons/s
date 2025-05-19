using System;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Networking.Session;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState;

public class AwaitingSessionReservation : ConnectionNegotiatingState
{
    public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.AWAITING_SESSION_RESERVATION;

    public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        try
        {
            ValidateState(sessionConnectionContext);
            HandleReservation(sessionConnectionContext);
        }
        catch (Exception)
        {
            Disconnect(sessionConnectionContext);
            throw;
        }
        return Task.CompletedTask;
    }

    private static void HandleReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        IMultiplayerSessionConnectionState nextState = sessionConnectionContext.Reservation.ReservationState switch
        {
            SessionReservationState.RESERVED => new SessionReserved(),
            _ => new SessionReservationRejected(),
        };

        sessionConnectionContext.UpdateConnectionState(nextState);
    }

    private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        ReservationIsNotNull(sessionConnectionContext);
    }

    private static void ReservationIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        try
        {
            Validate.NotNull(sessionConnectionContext.Reservation);
        }
        catch (ArgumentNullException ex)
        {
            throw new InvalidOperationException("The context does not have a reservation.", ex);
        }
    }
}
