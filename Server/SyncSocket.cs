using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCON
{
    partial class Server { 
        Socket sock;
        public void initConnect()
        {
            try
            {
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
                Task t1 = new Task(this.getPacket);
                Task t2 = new Task(this.handleBuffer);
                t1.Start();
                t2.Start();

            }
            catch (Exception e) // 익셉션 발견시 재연결 시도
            {
                if (sock.Connected)
                    sock.Close();
                initConnect();
            }
        }
        void getPacket()
        {
            Console.WriteLine("getPacket");
            while (true)
            {
                try
                {
                    length = sock.Receive(recvBuf);
                    if (length < 6) // 길이가 0 or 소켓 연결 해제시도 체크해줘야함
                    {
                        throw new Exception();
                    }

                    Buffer.AddRange(recvBuf.Take<byte>(length));

                }
                catch (Exception e)
                {
                    break;
                }
            }
            if (sock.Connected)
                sock.Close();
            //initConnect();
        }

    }
}
