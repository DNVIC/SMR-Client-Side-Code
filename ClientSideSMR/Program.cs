using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ClientSideSMR;

namespace ClientSideSMR
{
    
    class Program
    {
        private static string _user;
        private static string _oauth;
        private static string _channel;

        
        public static void assignStrings()
        {
            Console.Write("Insert given username");
            string user = Console.ReadLine();
            Console.Write("Insert given password");
            string pass = Console.ReadLine();
            Console.Write("Insert given channel");
            string chan = Console.ReadLine();


            _user = user;
            _oauth = pass;
            _channel = chan;
        }

        static async Task Main(string[] args)
        {
            assignStrings();
            await ExecuteClient();
        }
        static async Task ExecuteClient()
        {
            // Base socket code taken from https://geeksforgeeks.org/socket-programming-in-c-sharp/
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipaddr = ipHost.AddressList[2];
                IPEndPoint localEndPoint = new IPEndPoint(ipaddr, 16834);
                Console.WriteLine(ipaddr.ToString());
                Socket sender = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //Connect socket to endpoint
                    sender.Connect(localEndPoint);
                    byte[] ByteBuffer = new byte[1024];



                    //Print information that means we are good
                    Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint.ToString());

                    /*
                    //Creating sent message
                    byte[] messageSent = Encoding.ASCII.GetBytes("starttimer\r\n");
                    //Sending
                    int byteSent = sender.Send(messageSent);
                    messageSent = Encoding.ASCII.GetBytes("split\r\n");
                    int byte2Sent = sender.Send(messageSent);
                    */

                    await SplitLevelChecker(sender, ByteBuffer);
                    //sender.Shutdown(SocketShutdown.Both);
                    //sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected Exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void SendCommand(Socket s, string Command)
        {
            byte[] spinx = Encoding.ASCII.GetBytes(Command + "\r\n");
            s.Send(spinx);   
        }

        static string ReceiveCommand(Socket s, byte[] Buffer)
        {
            
            int recv = s.Receive(Buffer);

            


            return Encoding.ASCII.GetString(Buffer, 0, recv);
        }

        static string SendAndReceiveCommand(Socket s, string Command, byte[] Buffer)
        {
            byte[] spinx = Encoding.ASCII.GetBytes(Command + "\r\n");
            s.Send(spinx);
            int recv = s.Receive(Buffer);
            return Encoding.ASCII.GetString(Buffer, 0, recv);
        }


        private static async Task SplitLevelChecker(Socket sender, byte[] ByteBuffer )
        {
            Boolean bowl = true;
            Console.Write("When DNVIC tells you to start, press Enter here. \nIt will automatically start your splits, so only worry about pressing enter and starting the game.");
            Console.ReadLine();
            SendCommand(sender, "starttimer");
            string CurrentProgress = "";
            IrcClient ircClient = new IrcClient("irc.chat.twitch.tv", 6667, _user, _oauth, _channel);

            PingSender ping = new PingSender(ircClient);
            ping.Start();
            while (bowl)
            {


                string ReceivedCommand = SendAndReceiveCommand(sender, "getcurrentsplitname", ByteBuffer);
                string SplitID = ReceivedCommand.Substring(0, 2);
                Console.WriteLine(SplitID);

                

                if(SplitID != "!!")
                {
                    CurrentProgress = SplitID;
                }

                Console.WriteLine("Current Progress" + CurrentProgress);
                ircClient.SendPublicChatMessage(CurrentProgress);

                await Task.Delay(1500);
            }
        }
        
    }
}
