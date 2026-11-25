using System;

namespace Mirror
{
    // [CardGameDemo addition] A connection with nobody on the other end, for a
    // computer-controlled player that lives on the host (CPU mode). It owns
    // that player's objects like a real client would, but everything sent to
    // it is simply dropped: the CPU reads the game state directly instead.
    // NetworkServer treats it like the host's own local connection when
    // broadcasting (see the BotConnectionToClient checks there), so no
    // messages are handed to the transport for it.
    public class BotConnectionToClient : NetworkConnectionToClient
    {
        public BotConnectionToClient(int networkConnectionId) : base(networkConnectionId) { }

        public override string address => "cpu";

        internal override bool Send(ArraySegment<byte> segment, int channelId = Channels.DefaultReliable) => true;

        // never times out
        internal override bool IsClientAlive() => true;

        public override void Disconnect()
        {
            isReady = false;
            RemoveObservers();
        }
    }
}
