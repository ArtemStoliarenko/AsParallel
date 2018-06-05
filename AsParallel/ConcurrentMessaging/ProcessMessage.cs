using System;

namespace AsParallel.ConcurrentMessaging
{
	sealed class ProcessMessage
	{
		public MessageType MessageType { get; }

		public string Message { get; }

		public ProcessMessage(MessageType messageType, string message)
		{
			if (MessageType == MessageType.None)
				throw new ArgumentOutOfRangeException(nameof(MessageType));

			if (message == null)
				throw new ArgumentNullException(nameof(message));

			this.MessageType = messageType;
			this.Message = message;
		}
	}
}
