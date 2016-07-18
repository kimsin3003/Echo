using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace EchoClient
{
    class Client
    {
        private IPHostEntry m_ipHost;
        private IPAddress m_ipAddr;
        private IPEndPoint m_ipEndPoint;
        private Socket m_sock;

        public Client(string serverHostName, int port)
        {
            m_ipHost = Dns.GetHostEntry(serverHostName);
            m_ipAddr = m_ipHost.AddressList[1];     //In index 0, there's ipv6 address.
            foreach(var el in m_ipHost.AddressList)
                Console.WriteLine(el.ToString());
            m_ipEndPoint = new IPEndPoint(m_ipAddr, port);
        }

        public bool Connect()
        {
            m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_sock.Connect(m_ipEndPoint);
            }
            catch (SocketException)
            {
                Console.WriteLine("Server is Off");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            Console.WriteLine("Connected to server");
            return true;
        }

        private bool Receive()
        {
            string data = "";
            while (true)
            {
                byte[] bytes = new byte[40];
                int bytesRec = 0;
                try
                {
                    bytesRec = m_sock.Receive(bytes);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Connection is closed");
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("\n") > -1)
                {
                    Console.WriteLine("Server Return : " + data);
                    return true;
                }
            }
        }

        private bool Send(string input)
        {
            byte[] msg = Encoding.UTF8.GetBytes(input + "\n");
            try
            {
                m_sock.Send(msg);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Connection is closed");
                return false;
            }
            return true;
        }

        public void Start()
        {
            while(true)
            {
                if (!Connect())
                {
                    Thread.Sleep(1000);
                    continue;
                }

                while (true)
                {
                    Console.Write("Message: ");
                    string input = Console.ReadLine();

                    if (input == "quit")
                        return;

                    if (!Send(input))
                        break;

                    if (!Receive())
                        break;
                }
            }
        }

        public void ShutDown()
        {
            m_sock.Shutdown(SocketShutdown.Both);
            m_sock.Close();
        }

    }
}
