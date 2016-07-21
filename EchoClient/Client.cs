using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;

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

        /**make me XML**/
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

        private bool ReceiveMessage()
        {
            Socket socket = m_sock;
            IPAddress ipAddress = m_ipAddr;
            string leftData = null;         //left data that didn't make message shape
            byte[] buf = new byte[1024];
          
            while(true)
            {
                string data = "";
                if (leftData != null)
                {
                    data = leftData;
                    leftData = null;
                }

                int bytesRec = 0;

                try
                {
                    bytesRec = socket.Receive(buf);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Connection is closed");
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }

                data += Encoding.UTF8.GetString(buf, 0, bytesRec);

                int index = -1;
                while ((index = data.IndexOf("<eom>")) > -1)     //If data has end of message,
                {
                    string messageFragment = data.Substring(0, index);
                    Console.WriteLine("Server Return : " + messageFragment);

                    int lengthOfEOM = 5;
                    int lengthOfRealData = index + 1;
                    if (data.Length > lengthOfRealData + lengthOfEOM)  //If more data has come, 
                        data = data.Substring(index + lengthOfEOM);       //erase already printed data.
                    else
                        return true;
                }

                //left data that doesn't have any end of message
                leftData = data;
            }
        }

        private bool Send(string input)
        {
            byte[] msg = Encoding.UTF8.GetBytes(input + "<eom>");
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

                    while(m_sock.Poll(10000, SelectMode.SelectRead))
                    {
                        if (!ReceiveMessage())
                            break;
                    }
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
