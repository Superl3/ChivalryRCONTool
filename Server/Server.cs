using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RCON
{
    partial class Server
    {
        IPEndPoint ipep;
        String passwd;
        int queryPort;

        StringBuilder LOG = new StringBuilder();
        StringBuilder CHAT = new StringBuilder();

        List<string> mapRotation = new List<string>();
        Dictionary<long, Player> playerOnline = new Dictionary<long, Player>();

        Dictionary<string, string> strings = new Dictionary<string, string>();

        int pingLimit = 100;
        int pingLimitCount = 5;

        byte[] recvBuf = new byte[8192];
        List<byte> Buffer = new List<byte>();
        int length;

        public Server(string ip = "127.0.0.1", int rconport = 7777, int queryport = 27015, string adminpw = "l3repus")
        {
            this.ipep = new IPEndPoint(IPAddress.Parse(ip), rconport);
            this.passwd = adminpw;
            this.queryPort = queryport;

            initData();
            setStrings();
        }
        void initData()
        {
            mapRotation.Clear(); // 데이터 초기화;
            playerOnline.Clear();

        }
        void setStrings()
        {
            strings.Add("kick_ping", "당신은 핑이 높기 때문에 추방되었습니다.");
        }

        void leaveLog(string str) // LOG: 정기적 + 로그 값이 일정 이상일경우 작성 후 flush 할때 threadsafe 유의
        {
            str = DateTime.Now + "\t" + str;
            Console.WriteLine(str);
            LOG.Append(str);
        }
      
    }
}
