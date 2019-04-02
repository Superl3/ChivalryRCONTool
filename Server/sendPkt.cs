using System.Net.Sockets;

namespace RCON
{
    partial class Server
    {
        void sendPacket(Packet pkt)
        {
            sock.Send(pkt.encode(), SocketFlags.None);
        }

        void SAY_ALL(string msg)
        {
            sendPacket(new Packet(Packet.msgType.SAY_ALL, msg));
        }

        void SAY_ALL_BIG(string msg)
        {
            sendPacket(new Packet(Packet.msgType.SAY_ALL_BIG, msg));
        }

        void SAY(long guid, string msg)
        {
            sendPacket(new Packet(Packet.msgType.SAY, guid, msg));
        }

        void CHANGE_MAP(string msg)
        {
            sendPacket(new Packet(Packet.msgType.CHANGE_MAP, msg));
        }

        void ROTATE_MAP()
        {
            sendPacket(new Packet(Packet.msgType.ROTATE_MAP));
        }

        void KICK_PLAYER(long guid, string msg)
        {
            sendPacket(new Packet(Packet.msgType.KICK_PLAYER, guid, msg));
        }

        void BAN_PLAYER(long guid, string msg, int duration = 0)
        {
            if(duration == 0)
                sendPacket(new Packet(Packet.msgType.BAN_PLAYER, guid, msg));
            else
                sendPacket(new Packet(Packet.msgType.TEMP_BAN_PLAYER, guid, msg, duration));
        }

        void UNBAN_PLAYER(long guid)
        {
            sendPacket(new Packet(Packet.msgType.UNBAN_PLAYER, guid));
        }
    }
}
