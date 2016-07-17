using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace EchoServer
{
    class Server
    {
        private IPEndPoint m_ipEndPoint;
        private Socket m_listenSock;
        private int m_maxClientNum;
        private SessionManager m_sessionManager;
        Thread acceptingThread;

        public Server(int port)
        {
            m_ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            m_listenSock = null;
            m_maxClientNum = 10;
            m_sessionManager = new SessionManager();
        }

        public void ShutDown()
        {
            m_listenSock.Shutdown(SocketShutdown.Both);
            m_listenSock.Close();
        }

        public void StartListen()
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
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public void Start()
        {
            StartListen();
            acceptingThread = new Thread(new ThreadStart(Listen));
            acceptingThread.Start();

            while (true)
            {
                ProcessReadableSession();
            }
        }

        private void Listen()
        {
            while (true)
            {
                if (m_listenSock == null)
                    return;
                if (m_listenSock.Poll(10, SelectMode.SelectRead))
                {
                    Socket newClient = m_listenSock.Accept();
                    Session session = m_sessionManager.AddSession(newClient);
                    Console.WriteLine("Client" + session.id + "(" + session.ip + ")" + " has come");
                }
            }
        }

        private void ProcessReadableSession()
        {
            List<Session> readableSessions;
            readableSessions = m_sessionManager.GetReadableSessions();

            foreach (Session session in readableSessions)
            {
                string data = null;
                bool ret = ReceiveMessage(session, out data);

                if (!ret)
                    continue;

                SendEcho(session, data);
            }

        }

        private void SendEcho(Session session, String message)
        {
            IPAddress ipAddress = session.ip;
            byte[] msg = Encoding.UTF8.GetBytes(message);

            try
            {
                session.socket.Send(msg);
            }
            catch (SocketException)
            {
                m_sessionManager.RemoveSession(session);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                m_sessionManager.RemoveSession(session);
            }
        }

        private bool ReceiveMessage(Session session, out String message)
        {
            message = "";
            Socket socket = session.socket;
            IPAddress ipAddress = session.ip;
            while (true)
            {
                byte[] bytes = new byte[200];
                int bytesRec = 0;
                try
                {
                    bytesRec = socket.Receive(bytes);
                }
                catch (SocketException)
                {
                    m_sessionManager.RemoveSession(session);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    m_sessionManager.RemoveSession(session);
                    break;
                }

                message += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                if (message.IndexOf("\n") > -1)
                {
                    Console.WriteLine("Data from Client" + session.id + " : " + message);
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
    }
}
