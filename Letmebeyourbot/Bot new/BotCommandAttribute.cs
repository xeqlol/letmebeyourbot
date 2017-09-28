using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Letmebeyourbot.Bot_new
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BotCommandAttribute : Attribute
    {
        public string[] Commands { get; set; }
        public AccessLevel AccessRequired { get; set; }

        public BotCommandAttribute(AccessLevel accessRequired, params string[] commands)
        {
            Commands = commands;
            AccessRequired = accessRequired;
        }
    }
}
