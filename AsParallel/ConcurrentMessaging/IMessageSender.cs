using System;
using System.Collections.Generic;
using System.Text;

namespace AsParallel.ConcurrentMessaging
{
    interface IMessageSender
    {
		void SendOutputMessage(string message);

		void SendErrorMessage(string message);
    }
}
