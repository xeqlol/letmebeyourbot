using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Events.Client;

namespace Letmebeyourbot
{
    public partial class Letmebeyourbot
    {
        public enum Access
        {
            Admin = 2,
            Moderator = 1,
            User = 0
        }

        private string Memod()
        {
            return MeMod ? "/me " : "";
        }

        private string RemoveAtSymbol(string username)
        {
            return username.StartsWith("@") ? username.Replace("@", "").ToLower() : username.ToLower();
        }

        private Access AccessLevel(OnMessageReceivedArgs evnt)
        {
            if(Admins.Contains(evnt.ChatMessage.Username))
            {
                return Access.Admin;
            } else if (evnt.ChatMessage.IsModerator)
            {
                return Access.Moderator;
            } else
            {
                return Access.User;
            }
        } 
    }
}
