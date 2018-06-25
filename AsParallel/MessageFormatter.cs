namespace AsParallel
{
	/// <summary>
	/// Returns the messages formatter to be used in <see cref="ProcessRunner"/>.
	/// </summary>
	public static class MessageFormatter
	{
		/// <summary>
		/// Returns message formatter which append each message to the previous one as a new line.
		/// </summary>
		public static IMessageFormatter AppendLine => new AppendLineMessageFormatter();

		/// <summary>
		/// Returns message formatter which returns only last message.
		/// </summary>
		public static IMessageFormatter LastLine => new LastLineMessageFormatter();

		/// <summary>
		/// Returns message formatter which doesn't return any messages.
		/// </summary>
		public static IMessageFormatter NoMessages => new NoMessagesMessageFormatter();
	}
}
