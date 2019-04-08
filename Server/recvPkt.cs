using System;
using System.Linq;
using System.Threading.Tasks;

namespace RCON
{
    partial class Server
    {
        void handleBuffer()
        {
            Console.WriteLine("handleBuffer");
            while (true)
            {
                int chkLength = Packet.getLength(Buffer);
                if (chkLength <= 0)
                {
                    continue;
                }
                int singleLength = 2 + 4 + chkLength;
                byte[] rawPkt = (Buffer.Take<byte>(singleLength).ToArray());
                lock (Buffer)
                {
                    Buffer.RemoveRange(0, singleLength);
                }
                Task t = new Task(this.handlePacket, (object)new Packet(rawPkt, singleLength));
                t.Start();
            }
        }
        void handlePacket(object param)
        {
            Packet pkt = (Packet)param;
            Packet bkup = new Packet(pkt);

            bool success = false;

            switch (pkt.Type)
            {
                case Packet.msgType.MAP_LIST:
                    success = MAP_LIST(pkt);
                    break;
                case Packet.msgType.MAP_CHANGED:
                    success = MAP_CHANGED(pkt);
                    break;
                case Packet.msgType.ROUND_END:
                    success = ROUND_END(pkt);
                    break;
                case Packet.msgType.PLAYER_CONNECT:
                    success = PLAYER_CONNECT(pkt);
                    break;
                case Packet.msgType.PLAYER_DISCONNECT:
                    success = PLAYER_DISCONNECT(pkt);
                    break;
                case Packet.msgType.PLAYER_CHAT:
                    success = PLAYER_CHAT(pkt);
                    break;
                case Packet.msgType.PING:
                    success = PING(pkt);
                    break;
                case Packet.msgType.NAME_CHANGED:
                    success = NAME_CHANGED(pkt);
                    break;
                case Packet.msgType.TEAM_CHANGED:
                    success = TEAM_CHANGED(pkt);
                    break;
                case Packet.msgType.KILL:
                    success = KILL(pkt);
                    break;
                case Packet.msgType.SUICIDE:
                    success = SUICIDE(pkt);
                    break;
            }
            if (!success) // error occured
            {
                leaveLog("FAILED: " + bkup.ToString());
            }
        }
        bool MAP_LIST(Packet pkt)
        {
            string map = pkt.getStr();
            leaveLog("MAPLIST: " + map);
            mapRotation.Add(map);
            return true;
        }
        bool MAP_CHANGED(Packet pkt) // WIP
        {
            int idx = pkt.getInt();
            string map = pkt.getStr();
            leaveLog("MAPCHANGED: " + map);
            return true;
        }

        bool ROUND_END(Packet pkt)
        {
            int teamWin = pkt.getInt();
            return true;
        } // WIP
        bool PLAYER_CONNECT(Packet pkt)
        {
            Player.addPlayer(pkt.getGUID(), pkt.getStr(), ref playerOnline);
            return true;
        }
        bool PLAYER_DISCONNECT(Packet pkt)
        {
            bool res = playerOnline.Remove(pkt.getGUID());
            return res;
        }
        bool PLAYER_CHAT(Packet pkt)
        {
            Player player = Player.findPlayer(pkt.getGUID(), ref playerOnline);
            string msg = pkt.getStr();
            player.Team = (Player.teamName)pkt.getInt();

            CHAT.Append(player.Name + '\t' + msg);
            return true;
        }
        bool PING(Packet pkt)
        {
            Action<Player, int> pingChk = (p, ping) =>
            {
                if (ping > this.pingLimit)
                    p.PingCnt += 1;
                if (p.PingCnt >= this.pingLimitCount)
                    KICK_PLAYER(p.GUID, strings["kick"]);
            };

            Player player = Player.findPlayer(pkt.getGUID(), ref playerOnline);
            pingChk(player, pkt.getInt());
            return true;
        }
        bool NAME_CHANGED(Packet pkt)
        {
            bool res = true;
            Player player = Player.findPlayer(pkt.getGUID(), pkt.getStr(), ref playerOnline);
            return res;
        }
        bool TEAM_CHANGED(Packet pkt)
        {
            bool res = true;
            int team = pkt.getInt();
            Player player = Player.findPlayer(pkt.getGUID(), ref playerOnline);
            player.Team = (Player.teamName)pkt.getInt();
            return res;
        }
        bool KILL(Packet pkt)
        {
            Func<Player, Player, bool> tkChk = (p1, p2) =>
            {
                if (p1.Team == p2.Team && p1.Team != Player.teamName.NONE)
                    return true;
                return false;
            };

            Player Killer = Player.findPlayer(pkt.getGUID(), ref playerOnline);
            Player Victim = Player.findPlayer(pkt.getGUID(), ref playerOnline);

            if (!tkChk(Killer, Victim))
            {
                Killer.K += 1;
            }
            Victim.D += 1;
            return true;
        }
        bool SUICIDE(Packet pkt)
        {
            Player player = Player.findPlayer(pkt.getGUID(), ref playerOnline);
            player.D += 1;
            return true;
        }

    }
}
