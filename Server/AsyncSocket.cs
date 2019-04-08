using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCON
{
    partial class Server
    {
        public void initAsync()
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
                //sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Connected");

                sock.Blocking = false;
                sock.BeginReceive(recvBuf, 0, recvBuf.Length, SocketFlags.None, new AsyncCallback(onReceived), sock);
                Task t = new Task(this.handleBuffer);
                t.Start();
                //sock.BeginConnect(ipep, new AsyncCallback(onConnect), sock);
            }
            catch (Exception e)
            {
                Console.WriteLine("onRecvErr " + e.ToString());
            }

        }
        private void onConnect(IAsyncResult iar)
        {
            Socket sock = (Socket)iar.AsyncState;
            sock.EndConnect(iar);
            if(sock.Connected)
            {
            }
        }
        object recvLock = new object();
        private void onReceived(IAsyncResult iar)
        {
            Console.WriteLine("onRecv");
            Socket sock = (Socket)iar.AsyncState;
            try
            {
                length = sock.EndReceive(iar);
                if (length < 6) // 길이가 0 or 소켓 연결 해제시도 체크해줘야함
                {
                    throw new Exception();
                }
                lock (recvLock)
                    Buffer.AddRange(recvBuf.Take<byte>(length));

                sock.BeginReceive(recvBuf, 0, recvBuf.Length, SocketFlags.None, new AsyncCallback(onReceived), sock);
            } catch(Exception e)
            {
                Console.WriteLine("onRecvErr " + e.ToString());
            }
        }
    }
}
