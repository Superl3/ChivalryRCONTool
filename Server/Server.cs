using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace RCON
{
    partial class Server
    {
        IPEndPoint ipep;
        String passwd;
        int queryPort;
        Socket sock;
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

            setStrings();
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

        public void initConnect()
        {
            try
            {
                mapRotation.Clear(); // 데이터 초기화;
                playerOnline.Clear();
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(ipep);

                length = sock.Receive(recvBuf); // SERVER_CONNECT with custom strs
                Packet pkt = new Packet(recvBuf, length);

                byte[] encryptedPasswd = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(passwd + pkt.getStr()));
                pkt = new Packet(Packet.msgType.PASSWORD, BitConverter.ToString(encryptedPasswd).Replace("-", ""));
                
                sock.Send(pkt.encode(), SocketFlags.None);
                length = sock.Receive(recvBuf);
                if (length < 6) // 길이가 0 or 소켓 연결 해제시도 체크해줘야함
                {
                    throw new Exception();
                }

                pkt = new Packet(recvBuf, length);
                ThreadStart getData = new ThreadStart(this.getPacket);
                ThreadStart handleData = new ThreadStart(this.handleBuffer);
                Thread t1 = new Thread(getData);
                Thread t2 = new Thread(handleData);
                t1.Start();
                t2.Start();
            } catch(Exception e) // 익셉션 발견시 재연결 시도
            {
                if(sock.Connected)
                    sock.Close();
                initConnect();
            }
        }

        void getPacket()
        {
            Console.WriteLine("getPacket");
            while(true)
            {
                try
                {
                    length = sock.Receive(recvBuf);
                    if (length < 6) // 길이가 0 or 소켓 연결 해제시도 체크해줘야함
                    {
                        throw new Exception();
                    }

                    Buffer.AddRange(recvBuf.Take<byte>(length));

                } catch (Exception e) {
                    break;
                }
            }
            if (sock.Connected)
                sock.Close();
            //initConnect();
        }
        
    }
}
