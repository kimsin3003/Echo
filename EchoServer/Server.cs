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

        public Server()
        {
            m_ipEndPoint = new IPEndPoint(IPAddress.Any, 11000);
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
                ArrayList onRequestSock = new ArrayList(m_sockList);
                Socket.Select(onRequestSock, null, null, 1000);

                for(int i = 0; i < onRequestSock.Count; i++)
                {

                    if(onRequestSock[i] == m_listenSock)
                    {
                        Socket newClient = ((Socket)onRequestSock[i]).Accept();
                        IPAddress clientIP = IPAddress.Parse(((IPEndPoint)newClient.RemoteEndPoint).Address.ToString());
                        Console.WriteLine("Client(" + clientIP + ") has come");
                        m_sockList.Add(newClient);
                    }
                    else
                    {
                        bool isClosed = false;
                        string data = "";
                        Socket connection = ((Socket)onRequestSock[i]);
                        IPAddress ipAddress = IPAddress.Parse(((IPEndPoint)connection.RemoteEndPoint).Address.ToString());
                        while (true)
                        {
                            byte[] bytes = new byte[1024];
                            int bytesRec = 0;
                            try
                            {
                                bytesRec = connection.Receive(bytes);
                            }
                            catch(SocketException e)
                            {
                                Console.WriteLine(ipAddress + " has exit");
                                connection.Close();
                                m_sockList.Remove(connection);
                                isClosed = true;
                                break;
                            }
                            data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                            if (data.IndexOf("\n") > -1)
                            {
                                break;
                            }

                        }
                        if (isClosed)
                            break;

                        byte[] msg = Encoding.UTF8.GetBytes(data);
                        Console.WriteLine("Data from " + ipAddress + " : " + data);
                        try
                        {
                            connection.Send(msg);
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine(ipAddress + " has exit");
                            connection.Close();
                            m_sockList.Remove(connection);
                            break;
                        }

                    }
                }
            }
        }
        
    }
}
