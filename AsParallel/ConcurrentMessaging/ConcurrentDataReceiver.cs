using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsParallel.ConcurrentMessaging
{
	sealed class ConcurrentDataReceiver
	{
		private const int threadSleepInterval = 100;

		private readonly IMessageSender messageSender;

		private readonly ConcurrentQueue<ProcessMessage> messageQueue = new ConcurrentQueue<ProcessMessage>();

		public ConcurrentDataReceiver(IMessageSender messageSender)
		{
			if (messageSender == null)
				throw new ArgumentNullException(nameof(messageSender));

			this.messageSender = messageSender;
		}

		public Task Run(CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => ProcessQueue(cancellationToken), TaskCreationOptions.LongRunning);
		}

		public void AddToError(object sender, DataReceivedEventArgs e)
		{
			try
			{
				messageQueue.Enqueue(new ProcessMessage(MessageType.Error, e.Data));
			}
			catch (Exception)
			{
			}
		}

		public void AddToOutput(object sender, DataReceivedEventArgs e)
		{
			try
			{
				messageQueue.Enqueue(new ProcessMessage(MessageType.Standard, e.Data));
			}
			catch (Exception)
			{
			}
		}

		private void ProcessQueue(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				if (!messageQueue.IsEmpty)
					ProcessSingleMessage();
				else
					Thread.Sleep(threadSleepInterval);
			}

			int messagesLeft = messageQueue.Count;
			for (int i = 0; i < messagesLeft; ++i)
				ProcessSingleMessage();
		}

		private void ProcessSingleMessage()
		{
			bool success = messageQueue.TryDequeue(out ProcessMessage processMessage);

			if (success)
			{
				switch (processMessage.MessageType)
				{
					case MessageType.Standard:
						messageSender.SendOutputMessage(processMessage.Message);
						break;
					case MessageType.Error:
						messageSender.SendErrorMessage(processMessage.Message);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(processMessage));
				}
			}
		}
	}
}
