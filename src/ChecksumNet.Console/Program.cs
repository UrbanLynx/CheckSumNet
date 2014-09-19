using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChecksumNet.Model;

namespace ChecksumNet.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new NetProvider();
            provider.SetConnection();
            System.Console.ReadLine();
        }
    }
}
