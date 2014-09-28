using System;
using System.Collections.Generic;
using System.IO;
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
            var manager = new ModelManager();
            manager.RefreshHosts();
            //manager.StartListening();

            string path = @"D:\aa.txt";
            if(File.Exists(path))
                manager.NewChecksum(path);
            System.Console.ReadLine();
        }
    }
}
