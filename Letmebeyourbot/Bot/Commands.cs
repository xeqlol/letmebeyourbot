using System;
using System.Linq;
using System.Net;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.API.v5.Users;

namespace Letmebeyourbot
{
    public partial class Letmebeyourbot
    {
        public class Command : IEquatable<Command>
        {
            public string CommandName;
            public string CommandInfo;
            public Access CommandAccess;
            public Action<OnMessageReceivedArgs> CommandAction;

            public Command(string name, string info, Access access, Action<OnMessageReceivedArgs> action)
            {
                CommandName = name;
                CommandInfo = info;
                CommandAccess = access;
                CommandAction = action;
            }

            public bool Equals(Command other)
            {
                if (other != null && CommandName == other.CommandName)
                    return true;
                return false;
            }
        }

        // TODO: add some commands descriptions

        internal void SetCommandHandler()
        {
            // !commands
            CommandList.Add(new Command("!commands",
                                        "Команда !commands показывает список доступных команд.",
                                        Access.User,
                                        (arg) =>
            {
                Client.SendMessage($"{Memod()}Список команд: {CommandList.Select(x => x.CommandName).Aggregate((a, b) => $"{a}, {b}")}");
            }));

            // !commandinfo 
            CommandList.Add(new Command("!commandinfo",
                                        "Команда !commandinfo {command} показывает информацию о команде {command}. Например, !commandinfo !lalka.",
                                        Access.User,
                                        (arg) =>
            {
                string message = arg.ChatMessage.Message;
                if (message.Split().Length == 1)
                {
                    Client.SendMessage($"{Memod()}Пожалуйста, укажите название команды. Например, !commandinfo !lalka.");
                    return;
                }
                Command info = CommandList.Find(x => x.CommandName == message.Split(' ')[1]);
                if (info == null)
                {
                    Client.SendMessage($"{Memod()}Команда {message.Split(' ')[1]} не найдена. Возможно, Вы забыли знак ! перед названием команды. Например, !commandinfo !lalka.");
                    return;
                }
                Client.SendMessage($"{Memod()}{info.CommandInfo}");
            }));

            // !followage
            CommandList.Add(new Command("!followage",
                                        "Команда !followage {username} показывает сколько времени пользователь подписан на канал. Если {username} не указан, применяется к вызвавшему команду.",
                                        Access.User,
                                        (arg) =>
            {
                if (arg == null)
                    return;
                string username = arg.ChatMessage.DisplayName;
                if (arg.ChatMessage.Message.Split(' ').Length != 1)
                {
                    username = RemoveAtSymbol(arg.ChatMessage.Message.Split(' ')[1]);
                }
                if (username == LetmebeyourbotInfo.ChannelName || (arg.ChatMessage.DisplayName == LetmebeyourbotInfo.ChannelName && arg.ChatMessage.Message.Split(' ').Length == 1))
                {
                    Client.SendMessage($"{Memod()}{LetmebeyourbotInfo.ChannelName} владелец канала, ало. DansGame");
                    return;
                }
                using (WebClient client = new WebClient())
                {
                    string followstart = client.DownloadString($@"http://api.newtimenow.com/follow-length/?channel={LetmebeyourbotInfo.ChannelName}&user={username}");
                    if (followstart.StartsWith("Not following..."))
                    {
                        Client.SendMessage($"{Memod()}{username} не подписан на канал.");
                        return;
                    }
                    DateTime date2 = DateTime.Now;
                    DateTime date1 = DateTime.Parse(followstart).AddHours(12);

                    int oldMonth = date2.Month;
                    while (oldMonth == date2.Month)
                    {
                        date1 = date1.AddDays(-1);
                        date2 = date2.AddDays(-1);
                    }
                    int years = 0, months = 0, days = 0;
                    // getting number of years
                    while (date2.CompareTo(date1) >= 0)
                    {
                        years++;
                        date2 = date2.AddYears(-1);
                    }
                    date2 = date2.AddYears(1);
                    years--;
                    // getting number of months and days
                    oldMonth = date2.Month;
                    while (date2.CompareTo(date1) >= 0)
                    {
                        days++;
                        date2 = date2.AddDays(-1);
                        if ((date2.CompareTo(date1) >= 0) && (oldMonth != date2.Month))
                        {
                            months++;
                            days = 0;
                            oldMonth = date2.Month;
                        }
                    }
                    date2 = date2.AddDays(1);
                    days--;
                    TimeSpan difference = date2.Subtract(date1);
                    string formatted = string.Format("{0}{1}{2}{3}{4}{5}",
                        years > 0 ? string.Format("{0:0}г ", years.ToString()) : string.Empty,
                        months > 0 ? string.Format("{0:0}м ", months.ToString()) : string.Empty,
                        days > 0 ? string.Format("{0:0}д ", days.ToString()) : string.Empty,
                        difference.Hours > 0 ? string.Format("{0:0}ч ", difference.Hours.ToString()) : string.Empty,
                        difference.Minutes > 0 ? string.Format("{0:0}м ", difference.Minutes.ToString()) : string.Empty,
                        difference.Seconds > 0 ? string.Format("{0:0}с", difference.Seconds.ToString()) : string.Empty);
                    if (string.IsNullOrEmpty(formatted))
                        formatted = "0 секунд";
                    Client.SendMessage($"{Memod()}{username} с нами уже {formatted}.");
                }
            }));

            // !uptime
            CommandList.Add(new Command("!uptime",
                                        "Команда !uptime показывает длительность текущей трансляции.",
                                        Access.User,
                                        (arg) =>
            {
                string userId = null;
                try
                {
                    User[] userList = TwitchAPI.Users.v5.GetUserByNameAsync(LetmebeyourbotInfo.ChannelName).Result.Matches;
                    if (userList.Length == 0 || userList == null)
                        return;
                    userId = userList[0].Id;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                if (userId == null)
                    return;
                TimeSpan? time = TwitchAPI.Streams.v5.GetUptimeAsync(userId).Result;
                if (time != null)
                {
                    TimeSpan wrapper = time ?? TimeSpan.FromMilliseconds(0);
                    string formatted = string.Format("{0}{1}{2}{3}",
                        wrapper.Duration().Days > 0 ? string.Format("{0:0}д ", wrapper.Days) : string.Empty,
                        wrapper.Duration().Hours > 0 ? string.Format("{0:0}ч ", wrapper.Hours) : string.Empty,
                        wrapper.Duration().Minutes > 0 ? string.Format("{0:0}м ", wrapper.Minutes) : string.Empty,
                        wrapper.Duration().Seconds > 0 ? string.Format("{0:0}с", wrapper.Seconds) : string.Empty);

                    if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

                    if (string.IsNullOrEmpty(formatted)) formatted = "0с.";

                    Client.SendMessage($"{Memod()}Длительность стрима: {formatted}.");
                }
                else
                {
                    Client.SendMessage($"{Memod()}Стрим оффлайн.");
                }
            }));

            // !lalka
            CommandList.Add(new Command("!lalka",
                                        "Команда !lalka {username} показывает насколько пользователь {username} лалка. Если {username} не указан, применяется к вызвавшему команду.",
                                        Access.User,
                                        (arg) =>
            {
                if (arg == null)
                    return;
                if (arg.ChatMessage.Message.Split(' ').Length == 1)
                {
                    Client.SendMessage($"{Memod()}{arg.ChatMessage.Username} лалка на {new Random().Next(0, 100)}% Kappa");
                }
                else
                {
                    string input = RemoveAtSymbol(arg.ChatMessage.Message.Split(' ')[1]);
                    if (input == LetmebeyourbotInfo.ChannelName || input == $"@{LetmebeyourbotInfo.ChannelName}")
                    {
                        Client.SendMessage($"{Memod()}{LetmebeyourbotInfo.ChannelName} абсолютная лалка. 4Head");
                        return;
                    }
                    Client.SendMessage($"{Memod()}{input} лалка на {new Random().Next(0, 100)}%. Kappa");
                }
            }));

            // !vodka
            CommandList.Add(new Command("!vodka",
                                        "VODKA, VODKA, VODKA! (phychonaut 4 - sweed decadence)",
                                        Access.User,
                                        (arg) =>
            {
                Client.SendMessage($"{Memod()}VODKA, VODKA, VODKA! SwiftRage");
            }));

            // !rating
            CommandList.Add(new Command("!rating",
                                        "Команда !rating показывает текущий рейтинг на мейн персонаже.",
                                        Access.User,
                                        (arg) =>
            {
                using (WebClient client = new WebClient())
                {
                    Newtonsoft.Json.Linq.JObject j_object = Newtonsoft.Json.Linq.JObject.Parse(client.DownloadString($"https://eu.api.battle.net/wow/character/BlackScar/Якушева?fields=pvp&locale=en_GB&apikey={LetmebeyourbotInfo.BlizzardAPIKey}"));
                    Client.SendMessage($"{Memod()}Рейтинг на мейне (Якушева): 2x2 - {j_object["pvp"]["brackets"]["ARENA_BRACKET_2v2"]["rating"]}, " +
                                                                $"3x3 - {j_object["pvp"]["brackets"]["ARENA_BRACKET_3v3"]["rating"]}, " +
                                                                $"RBG - {j_object["pvp"]["brackets"]["ARENA_BRACKET_RBG"]["rating"]}.");
                }
            }));

            // !roll
            CommandList.Add(new Command("!roll",
                                        "Команда !roll выбирает случайное число в промежутке от 1 до 100.",
                                        Access.User,
                                        (arg) =>
            {
                Client.SendMessage($"{Memod()}{arg.ChatMessage.Username} выбрасывает {new Random().Next(1, 100)} (1-100).");
            }));

            // !duel
            CommandList.Add(new Command("!duel",
                                        "Команда !duel вызывает {username} на дуэль.",
                                        Access.User,
                                        (arg) =>
            {
                using (WebClient client = new WebClient())
                {
                    string[] splittedMessage = arg.ChatMessage.Message.Split(' ');
                    if (splittedMessage.Length == 1)
                        Client.SendMessage($"{Memod()}Выберите противника для дуэли. Например, !duel jiberjaber1");

                    Client.SendMessage($"{Memod()}{arg.ChatMessage.Username} вызывает на дуэль {RemoveAtSymbol(splittedMessage[1])}. {(new Random().NextDouble() >= 0.5 ? arg.ChatMessage.Username : RemoveAtSymbol(splittedMessage[1]))} выходит победителем!");
                }
            }));

            // !memod
            CommandList.Add(new Command("!memod",
                                        "Команда !memod {true/false} устанавливает режим отображения (/me) сообщений бота.",
                                        Access.Admin,
                                        (arg) =>
            {
                if (bool.TryParse(arg.ChatMessage.Message.Split(' ')[1], out MeMod))
                {
                    Client.SendMessage(Memod() + (MeMod ? "Режим /me включен." : "Режим /me отключен."));
                }
                else
                {
                    Client.SendMessage($"{Memod()}Неправильный аргумент команды, попробуйте !memod true или !memod false");
                }

            }));
        }


        /* COINS COMMANDS */


    }
}
