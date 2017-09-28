using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Letmebeyourbot.Bot_new
{
    internal class MessageLimiter
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
                while (Messages.Count > 0 && (DateTime.Now - Messages.First.Value).TotalSeconds >= TimeSpan)
                    Messages.RemoveFirst();
                return MessageLimit - Messages.Count;
            }
        }
    }
}
