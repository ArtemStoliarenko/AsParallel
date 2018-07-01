using System;

namespace AsParallel
{
	sealed class NoMessagesMessageFormatter : IMessageFormatter
	{
		/// <summary>
		/// Current standard output of the running processes.
		/// </summary>
		public string Output => string.Empty;

		/// <summary>
		/// Current error output of the running processes.
		/// </summary>
		public string Error => string.Empty;

		/// <summary>
		/// Current combined output of the running processes.
		/// </summary>
		public string CombinedOutput => string.Empty;

		/// <summary>
		/// Processes new message for standard and combined output.
		/// </summary>
		/// <param name="message">Message to be processed.</param>
		public void AddToOutput(string message)
		{ }

		/// <summary>
		/// Processes new message for error and combined output.
		/// </summary>
		/// <param name="message">Message to be processed.</param>
		public void AddToError(string message)
		{ }

		/// <summary>
		/// Gets <see cref="RunResults"/> object with current messages state.
		/// </summary>
		/// <returns><see cref="RunResults"/> object with current messages state.</returns>
		public RunResults GetRunResults() => new RunResults(Output, Error, CombinedOutput);

		/// <summary>
		/// Clears all messages.
		/// </summary>
		public void Clear()
		{ }

		/// <summary>
		/// Creates new instance of <see cref="NoMessagesMessageFormatter"/> without already processed messages.
		/// </summary>
		/// <returns>New instance of <see cref="NoMessagesMessageFormatter"/> without already processed messages.</returns>
		public IMessageFormatter GetCopy() => new NoMessagesMessageFormatter();
	}
}
