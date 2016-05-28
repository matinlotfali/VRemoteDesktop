using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VRemoteDesktop
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient client = new UdpClient(7711, AddressFamily.InterNetwork);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 7711);
            for (;;)
            {
                byte[] bytes = client.Receive(ref ip);
                string str = Encoding.UTF8.GetString(bytes);
                Console.WriteLine(str);
            }
        }
    }
}
