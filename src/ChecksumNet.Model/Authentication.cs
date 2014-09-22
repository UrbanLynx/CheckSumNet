using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChecksumNet.Model
{
    public class Authentication
    {
        public bool AuthenticationCompare(string inputLogin, string password)
        {
            using (StreamReader sr = File.OpenText("Logins.txt"))
            {
                string str = "";
                string[] masStr;   

                while ((str = sr.ReadLine()) != null)
                {
                    masStr = str.Split('\t');
                    if ((String.Compare(inputLogin, masStr[0]) == 0) && (String.Compare(inputLogin, masStr[1]) == 0))
                    {
                        return true;
                    }
                    str = sr.ReadLine();                 
                }
            }

            return false;
        }












    }
}