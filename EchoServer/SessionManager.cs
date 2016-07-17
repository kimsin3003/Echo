﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EchoServer
{
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
            List<Socket> ss = new List<Socket>();

            lock (m_sessions)
            {
                sessions = new List<Session>(m_sessions);
            }
            
            foreach (Session session in sessions)
            {
                Socket socket = session.socket;
                
                if (socket.Poll(10, SelectMode.SelectRead))
                {
                    if (socket.Available == 0)
                    {
                        RemoveSession(session);
                        continue;
                    }
                    if (socket.Available > 0)
                        readableSessions.Add(session);

                }
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
                newSession.id = count;
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
                Console.WriteLine("Client" + session.id + "(" + session.ip + ") has exit");
                m_idCount.Enqueue(session.id);
                session.socket.Close();
                m_sessions.Remove(session);
            }
        }
    }
}
