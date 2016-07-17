using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace EchoServer
{
    class Session
    {
        public Socket socket;
        public string id;
        public IPAddress ip;
        public Thread thread;
        public Session()
        {
            socket = null;
            id = null;
            ip = null;
            thread = null;
        }

        public Session(Session session)
        {
            socket = session.socket;
            id = "" + session.id;
            ip = new IPAddress(session.ip.Address);
            thread = session.thread;
        }
    }

    class SessionManager
    {
        private List<Session> m_sessions;
        private Queue<int> m_idCount;

        public SessionManager()
        {
            m_sessions = new List<Session>();
            m_idCount = new Queue<int>();
            m_idCount.Enqueue(1);

        }

        public List<Session> GetReadableSessions()
        {
            List<Session> readableSessions = new List<Session>();
            List<Session> closedSessions = new List<Session>();
            List<Session> sessions;

            lock (m_sessions)
            {
                sessions = m_sessions;
            }

            foreach(Session session in sessions)
            {
                if(!session.socket.Connected)
                {
                    closedSessions.Add(session);
                    continue;
                }

                if (session.socket.Available > 0)
                {
                    readableSessions.Add(session);
                }
            }

            foreach (Session session in closedSessions)
            {
                RemoveSession(session);
            }
            return readableSessions;
        }

        public Session AddSession(Socket socket)
        {
            Session newSession = new Session();

            newSession.socket = socket;
            lock (m_sessions)
            {
                int count = m_idCount.Dequeue();
                newSession.id = "Client" + count;
                m_idCount.Enqueue(count + 1);
                newSession.ip = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
           
                m_sessions.Add(newSession);
            }

            return newSession;
        }

        public void RemoveSession(Session session)
        {
            lock(m_sessions)
            {
                Console.WriteLine(session.id + " has exit");
                session.socket.Close();
                m_sessions.Remove(session);
            }
        }
    }
}
