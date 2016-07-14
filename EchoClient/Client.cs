﻿using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace EchoClient
{
    class Client
    {
        private IPHostEntry m_ipHost;
        private IPAddress m_ipAddr;
        private IPEndPoint m_ipEndPoint;
        private Socket m_sock;

        public Client()
        {
            m_ipHost = Dns.Resolve("localhost");
            m_ipAddr = m_ipHost.AddressList[0];
            m_ipEndPoint = new IPEndPoint(m_ipAddr, 11000);
        }

        public void MakeConnection()
        {
            m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_sock.Connect(m_ipEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        public void Start()
        {
            while(true)
            {
                Console.Write("Message: ");
                string input = Console.ReadLine();

                if (input == "quit")
                    return;

                byte[] msg = Encoding.UTF8.GetBytes(input + "\n");
                try
                {
                    m_sock.Send(msg);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Connection is closed");
                    m_sock.Close();
                    break;
                }

                string data = "";
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = 0;
                    try
                    {
                        bytesRec = m_sock.Receive(bytes);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Connection is closed");
                        m_sock.Close();
                        return;
                    }
                    
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("\n") > -1)
                    {
                        break;
                    }

                }

                Console.WriteLine("Server Return : " + data);
            }
        }

    }
}