using System;

namespace AsParallel.ConcurrentMessaging
{
	struct ProcessMessage
	{
		public MessageType MessageType { get; }

		public string Message { get; }

		public ProcessMessage(MessageType messageType, string message)
		{
			if (messageType == MessageType.None)
				throw new ArgumentOutOfRangeException(nameof(messageType));

			if (message == null)
				throw new ArgumentNullException(nameof(message));

			this.MessageType = messageType;
			this.Message = message;
		}
	}
}
