using System;
using System.Collections.Generic;
using System.Linq;
    using System.Text;

namespace RCON
{
    class Packet
    {
        public msgType Type { get; private set; }
        private int Size = 0;
        private List<byte> Data = new List<byte>();

        public Packet(Packet pkt)
        {
            this.Type = pkt.Type;
            this.Size = pkt.Size;
            this.Data = pkt.Data;
        }
        public Packet(byte[] data, int length)
        {
            byte[] type = new byte[2];
            Array.Copy(data, type, 2);
            Array.Reverse(type);

            byte[] size = new byte[4];
            Array.Copy(data, 2, size, 0, 4);
            Array.Reverse(size);

            byte[] raw = new byte[length - 6];
            Array.Copy(data, 6, raw, 0, length - 6);

            Type = (msgType)BitConverter.ToInt16(type, 0);
            Size = BitConverter.ToInt32(size, 0);
            Data = raw.ToList();
        }
        
        public Packet(msgType type)
        {
            this.Type = type;
        }
        public Packet(msgType type, long guid)
        {
            this.Type = type;
            addGUID(guid);
        }
        public Packet(msgType type, string str)
        {
            this.Type = type;
            addStr(str);
        }
        public Packet(msgType type, long guid, string str)
        {
            this.Type = type;
            addGUID(guid);
            addStr(str);
        }
        public Packet(msgType type, long guid, string str, int val)
        {
            this.Type = type;
            addGUID(guid);
            addStr(str);
            addInt(val);
        }

        public byte[] encode()
        {
            List<byte> res = new List<byte>(BitConverter.GetBytes((short)this.Type).Reverse());
            res.AddRange(BitConverter.GetBytes(Data.Count()).Reverse());
            res.AddRange(Data);
            return res.ToArray();
        }

        public void addInt(int val)
        {
            Data.AddRange(BitConverter.GetBytes(val).Reverse());
        }
        public void addGUID(long guid)
        {
            Data.AddRange(BitConverter.GetBytes(guid).Reverse());
        }
        public void addStr(string str)
        {
            byte[] raw = Encoding.UTF8.GetBytes(str);

            Data.AddRange(BitConverter.GetBytes(raw.Length).Reverse());
            Data.AddRange(raw);
        }

        static public int getLength(List<byte> input)
        {
            int res;
            try
            {
                res = BitConverter.ToInt32(input.Skip<byte>(2).Take<byte>(4).Reverse().ToArray(), 0);
            }
            catch (ArgumentOutOfRangeException e)
            {
                res = -1;
            }
            return res;
        }
        public int getInt()
        {
            int res = BitConverter.ToInt32(Data.Take<byte>(4).Reverse().ToArray(), 0);
            Data.RemoveRange(0, 4);
            return res;
        }
        public long getGUID()
        {
            long guid = BitConverter.ToInt64(Data.Take<byte>(8).Reverse().ToArray(), 0);
            Data.RemoveRange(0, 8);
            return guid;
        }
        public string getStr()
        {
            int length = getInt();
            string str = Encoding.UTF8.GetString(Data.GetRange(0, length).ToArray());
            Data.RemoveRange(0, length);
            return str;
        }

        public override string ToString()
        {
            return this.Type.ToString() + '\t' + this.Size + '\t' + this.Data;
        }

        public enum msgType
        {
            SERVER_CONNECT,
            SERVER_CONNECT_SUCCESS,
            PASSWORD,
            PLAYER_CHAT,
            PLAYER_CONNECT,
            PLAYER_DISCONNECT,
            SAY_ALL,
            SAY_ALL_BIG,
            SAY,
            MAP_CHANGED,
            MAP_LIST,
            CHANGE_MAP,
            ROTATE_MAP,
            TEAM_CHANGED,
            NAME_CHANGED,
            KILL,
            SUICIDE,
            KICK_PLAYER,
            TEMP_BAN_PLAYER,
            BAN_PLAYER,
            UNBAN_PLAYER,
            ROUND_END,
            PING
        }

    }
}