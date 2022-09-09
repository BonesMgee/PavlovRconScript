//Minimalistic Pavlov Rcon Tool
//Created By Bones M'gee {1/09/2022}
//Credit to Tom Janssens For The Framework On Which This Is Built

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace PavlovRcon
{
    enum Verbs {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    public class PlistModel
    {
        public string[]? PlayerList { get; set; }
        public bool Successful { get; set; }
    }

    public class IlistModel
    {
        public string[]? ItemList { get; set; }
        public bool Successful { get; set; }
    }

    class RConnection
    {
        TcpClient tcpSocket;

        int TimeOutMs = 100;

        public RConnection(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(Hostname, Port);

        }



        public void Login(string MD5SUM)
        {
            if (!tcpSocket.Connected) Console.WriteLine("Not Connected");
            if (!tcpSocket.Connected) return;
            Command(MD5SUM);
            string auth = Read();
            Console.WriteLine(auth);
            char[] authsucc = auth.ToCharArray();
            if (authsucc[24] != '1')
            {
                Console.WriteLine("Failed To Authenticate");
                throw new Exception("Authentication Failiure");
            }

        }

        public string[] PlayerList()
        {
            string[] NP = { "No Players Connected"};
            if (!tcpSocket.Connected) Console.WriteLine("Not Connected");
            if (!tcpSocket.Connected) throw new Exception("Connection Failure");
            Command("RefreshList");
            string PList = Read();
            //Console.WriteLine(PList);
            PlistModel p = JsonConvert.DeserializeObject<PlistModel>(PList);
            return p.PlayerList;
        }
        
        public string[] ItemList()
        {
            if (!tcpSocket.Connected) Console.WriteLine("Not Connected");
            if (!tcpSocket.Connected) throw new Exception("Connection Failure");
            Command("ItemList");
            string IList = Read();
            //Console.WriteLine(IList);
            IlistModel i = JsonConvert.DeserializeObject<IlistModel>(IList);
            return i.ItemList;

        }
        
        public void Command(string cmd)
        {
            Write(cmd + "\n");
        }

        public void Write(string cmd)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF","\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public string Read()
        {
            if (!tcpSocket.Connected) return null;
            StringBuilder sb=new StringBuilder();
            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (tcpSocket.Available > 0);
            return sb.ToString();
        }

        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1 :
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC: 
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO: 
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA )
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL:(byte)Verbs.DO); 
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT); 
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append( (char)input );
                        break;
                }
            }
        }
    }
}
