using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace EchoServer
{
    class Server
    {
        private IPEndPoint m_ipEndPoint;
        private Socket m_listenSock;
        private int m_maxClientNum;
        private ArrayList m_sockList;

        public Server(int port)
        {
            m_ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            m_listenSock = null;
            m_sockList = new ArrayList();
            m_maxClientNum = 10;
        }
        public void MakeListener()
        {
            m_listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_listenSock.Bind(m_ipEndPoint);

                try
                {
                    m_listenSock.Listen(m_maxClientNum);
                    Console.WriteLine("Start Listening");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                m_sockList.Add(m_listenSock);
         
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public void Start()
        {
            while (true)
            {
                ArrayList onRequestSocks = new ArrayList(m_sockList);
                Socket.Select(onRequestSocks, null, null, 1000);

                for (int i = 0; i < onRequestSocks.Count; i++)
                {

                    ProcessSocket((Socket)onRequestSocks[i]);
                }
            }
        }

        private void ProcessSocket(Socket socket)
        {
            if (socket == m_listenSock)
            {
                Socket newClient = socket.Accept();
                IPAddress clientIP = IPAddress.Parse(((IPEndPoint)newClient.RemoteEndPoint).Address.ToString());
                Console.WriteLine("Client(" + clientIP + ") has come");
                m_sockList.Add(newClient);
            }
            else
            {
                string data = "";
                
                if (!ReceiveMessage(socket, ref data))
                    return;

                IPAddress ipAddress = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
                byte[] msg = Encoding.UTF8.GetBytes(data);
                Console.WriteLine("Data from " + ipAddress + " : " + data);
                try
                {
                    socket.Send(msg);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(ipAddress + " has exit");
                    CloseSocket(socket);
                    m_sockList.Remove(socket);
                    return;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    CloseSocket(socket);
                    m_sockList.Remove(socket);
                }

            }
        }

        private bool ReceiveMessage(Socket socket,ref String message)
        {
            IPAddress ipAddress = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
            while (true)
            {
                byte[] bytes = new byte[200];
                int bytesRec = 0;
                try
                {
                    bytesRec = socket.Receive(bytes);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(ipAddress + " has exit");
                    CloseSocket(socket);
                    m_sockList.Remove(socket);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    CloseSocket(socket);
                    m_sockList.Remove(socket);
                    break;
                }

                message += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                if (message.IndexOf("\n") > -1)
                {
                    return true;
                }

            }

            return false;

        }

        private void CloseSocket(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public void ShutDown()
        {
            m_listenSock.Shutdown(SocketShutdown.Both);
            m_listenSock.Close();
        }

    }
}
