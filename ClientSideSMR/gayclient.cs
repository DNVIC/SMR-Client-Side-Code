using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

//Partially taken from https://codereview.stackexchange.com/questions/142653/simple-irc-bot-in-c


namespace ClientSideSMR
{
    public class gayclient
    {
        // server to connect to (edit at will)
        private readonly string _server;
        // server port (6667 by default)
        private readonly int _port;
        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private readonly string _user;

        // the bot's nickname
        //private readonly string _nick;
        // channel to join
        private readonly string _channel;

        private readonly string _password;

        private readonly int _maxRetries;
        public StreamWriter writer;


        public gayclient(string server, int port, string user, string channel, string password, int maxRetries = 3)
        {
            _server = server;
            _port = port;
            _user = user;
            _channel = channel;
            _maxRetries = maxRetries;
            _password = password;

        }

        public async Task Start()
        {
            var retry = false;
            var retryCount = 0;


            do
            {
                try
                {
                    using (var irc = new TcpClient(_server, _port))
                    using (var stream = irc.GetStream())
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("PASS " + _password);
                        writer.WriteLine("NICK " + _user);
                        writer.WriteLine("USER " + _user + " 8 * :" + _user);
                        writer.Flush();

                        while (true)
                        {
                            string inputLine;
                            while ((inputLine = reader.ReadLine()) != null)
                            {
                                Console.WriteLine("<- " + inputLine);

                                // split the lines sent from the server by spaces (seems to be the easiest way to parse them)
                                string[] splitInput = inputLine.Split(new Char[] { ' ' });

                                if (splitInput[0] == "PING")
                                {
                                    string PongReply = splitInput[1];
                                    //Console.WriteLine("->PONG " + PongReply);
                                    writer.WriteLine("PONG " + PongReply);
                                    writer.Flush();
                                    //continue;
                                }

                                switch (splitInput[1])
                                {
                                    case "001":
                                        writer.WriteLine("JOIN #" + _channel);
                                        writer.Flush();
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                    Console.WriteLine(e.ToString());
                    await Task.Delay(5000);
                    retry = ++retryCount <= _maxRetries;
                }
            } while (retry);
        }

        public void SendIrcMessage(string message, StreamWriter writer)
        {
            try
            {
                writer.WriteLine(message);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void SendPublicChatMessage(string message, StreamWriter writer)
        {
            try
            {
                SendIrcMessage(":" + _user + "!" + _user + "@" + _user + "tmi.twitch.tv PRIVMSG #ldlotbot :" + message, writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
