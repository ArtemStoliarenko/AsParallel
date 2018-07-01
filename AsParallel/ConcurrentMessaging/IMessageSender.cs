namespace AsParallel.ConcurrentMessaging
{
	interface IMessageSender
	{
		void SendOutputMessage(string message);

		void SendErrorMessage(string message);
	}
}
