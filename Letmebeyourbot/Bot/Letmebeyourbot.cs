using System;
using System.Collections.Generic;
using System.Linq;
using IniParser;
using IniParser.Model;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;

namespace Letmebeyourbot
{
    public partial class Letmebeyourbot
    {
        static ConnectionCredentials Credentials;
        static TwitchClient Client;
        static List<Command> CommandList = new List<Command>();
        static MessageLimitHandler MLimitHandler;
        static TwitchLib.Models.API.v5.Channels.Channel Channel;
        static IniData DBConnection;

        static string[] Admins = new string[] { LetmebeyourbotInfo.ChannelName, "xeqlol", "nonameorxeqlol" };

        // if true, adds "/me" at the start of response
        static bool MeMod = false;

        internal void Connect()
        {
            Console.WriteLine($"Letmebeyourbot v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()} by xeqlol\n");
            Console.WriteLine($"Connecting to {LetmebeyourbotInfo.ChannelName} channel...\n");

            TwitchAPI.Settings.ClientId = LetmebeyourbotInfo.ClientId;

            Credentials = new ConnectionCredentials(LetmebeyourbotInfo.BotUsername, LetmebeyourbotInfo.BotToken);
            Client = new TwitchClient(Credentials, LetmebeyourbotInfo.ChannelName, logging: true);
            MLimitHandler = new MessageLimitHandler();
            Channel = TwitchAPI.Channels.v5.GetChannelByIDAsync(TwitchAPI.Users.v5.GetUserByNameAsync(LetmebeyourbotInfo.ChannelName).Result.Matches[0].Id).Result;

            Client.OnLog += Client_OnLog;
            Client.OnConnectionError += Client_OnConnectionError;
            Client.OnMessageReceived += Client_OnMessageReceived;

            SetCommandHandler();

            try
            {
                Client.Connect();
            }
            catch (Exception ex)
            {
                // TODO: add here something with "reconnect in blablabla sec."
                Console.WriteLine($"Something goes wrong: {ex.Message}");
                return;
            }

            Console.WriteLine($"Connected to {LetmebeyourbotInfo.ChannelName} channel.");
            Console.WriteLine(!TwitchAPI.Streams.v5.BroadcasterOnlineAsync(Channel.Id.ToString()).Result
                ? "Status: offline"
                : $"Status: \tonline\nTitle: \t\t{Channel.Status}\nGame: \t\t{Channel.Game}\nUptimet: \t{TwitchAPI.Streams.v5.GetUptimeAsync(Channel.Id.ToString()).Result}");
        }

        internal void Disconnect()
        {
            Client.Disconnect();
        }

        internal void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.StartsWith("!"))
            {
                // reflection => find all functions that have Command attribute, then use attribute.CommandName

                Command command = CommandList.Find(x => x.CommandName == e.ChatMessage.Message.Split().First());
                if (command != null) // no, we cannot join these ifs
                {
                    if (MLimitHandler.ShouldSendMessage() > 0)
                    {
                        try
                        {
                            if (command.CommandAccess <= AccessLevel(e))
                            {
                                command.CommandAction.Invoke(e);
                                MLimitHandler.HandleMessageSent();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception: {ex}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("IRC message limit overflow.");
                    }
                }
            }
        }

        internal void Client_OnLog(object sender, OnLogArgs e)
        {
            //Console.WriteLine(e.Data);
        }

        internal void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error: {e.Error}");
        }
    }

    internal class MessageLimitHandler
    {
        object Sync = new object();
        const int MessageLimit = 90;
        const int TimeSpan = 30;

        LinkedList<DateTime> Messages = new LinkedList<DateTime>();

        public void HandleMessageSent()
        {
            lock (Sync)
            {
                Messages.AddLast(DateTime.Now);
            }
        }

        public int ShouldSendMessage()
        {
            lock (Sync)
            {
                if (Messages.Count == 0)
                    return MessageLimit;
                while(Messages.Count > 0 && (DateTime.Now - Messages.First.Value).TotalSeconds >= TimeSpan)
                    Messages.RemoveFirst();
                return MessageLimit - Messages.Count;
            }
        }
    }
}
