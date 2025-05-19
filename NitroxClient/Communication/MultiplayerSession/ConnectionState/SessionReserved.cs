using System;
using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class SessionReserved : ConnectionNegotiatedState
    {
        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.SESSION_RESERVED;

        public override void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                ValidateState(sessionConnectionContext);
                EnterMultiplayerSession(sessionConnectionContext);
                ChangeState(sessionConnectionContext);
            }
            catch (Exception)
            {
                Disconnect(sessionConnectionContext);
                throw;
            }
        }

        private static void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            if (!sessionConnectionContext.Client.IsConnected)
            {
                throw new InvalidOperationException("The client is not connected.");
            }
        }

        private void EnterMultiplayerSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            IClient client = sessionConnectionContext.Client;
            SessionReservation reservation = sessionConnectionContext.Reservation;
            string reservationKey = reservation.ReservationKey;

            client.Send(new PlayerJoiningMultiplayerSession(reservationKey));
        }

        private void ChangeState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            sessionConnectionContext.UpdateConnectionState(new SessionJoined());
        }
    }
}
