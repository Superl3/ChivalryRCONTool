using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON
{
    class Player
    {
        public long GUID { get; private set; }
        public string Name { get; private set; }
        public teamName Team { get; set; }

        public int PingCnt { get; set; }

        public int K { get; set; }
        public int D { get; set; }

        public Player(long guid, string name)
        {
            this.GUID = guid;
            this.Name = name;
            this.PingCnt = 0;
            this.Team = teamName.NONE;
            this.K = this.D = 0;
        }

        static public Player findPlayer(long guid, ref Dictionary<long, Player> DB)
        {
            Player player;
            try
            {
                player = DB[guid];
            }
            catch (KeyNotFoundException e)
            {
                player = new Player(guid, "placeholder");
                DB.Add(guid, player);
            }
            return player;
        }
        static public Player findPlayer(long guid, string name, ref Dictionary<long, Player> DB)
        {

            Player player = findPlayer(guid, ref DB);
            player.Name = name;
            return player;
        }
        static public Player addPlayer(long guid, string name, ref Dictionary<long, Player> DB)
        {
            return findPlayer(guid, name, ref DB);
        }

        public override string ToString()
        {
            return GUID.ToString() + this.Name;
        }
        public enum teamName
        {
            Mason,
            Agatha,
            NONE
        };
    }
}
