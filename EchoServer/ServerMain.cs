using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    class ServerMain
    {
        static void Main(string[] args)
        {

            Server server = new Server(11000);
            server.MakeListener();
            server.Start();
            
        }
    }
}
