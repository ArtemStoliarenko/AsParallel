using System;
using System.Text;

namespace AsParallel
{
	sealed class AppendLineMessageFormatter : IMessageFormatter
	{
		private readonly StringBuilder outputStringBuilder = new StringBuilder();
		private readonly StringBuilder errorStringBuilder = new StringBuilder();
		private readonly StringBuilder combinedStringBuilder = new StringBuilder();

		/// <summary>
		/// Current standard output of the running processes.
		/// </summary>
		public string Output => outputStringBuilder.ToString();

		/// <summary>
		/// Current error output of the running processes.
		/// </summary>
		public string Error => errorStringBuilder.ToString();

		/// <summary>
		/// Current combined output of the running processes.
		/// </summary>
		public string CombinedOutput => combinedStringBuilder.ToString();

		/// <summary>
		/// Processes new message for standard and combined output.
		/// </summary>
		/// <param name="message">Message to be processed.</param>
		public void AddToOutput(string message)
		{
			outputStringBuilder.AppendLine(message);
			combinedStringBuilder.AppendLine(message);
		}

		/// <summary>
		/// Processes new message for error and combined output.
		/// </summary>
		/// <param name="message">Message to be processed.</param>
		public void AddToError(string message)
		{
			errorStringBuilder.AppendLine(message);
			combinedStringBuilder.AppendLine(message);
		}

		/// <summary>
		/// Gets <see cref="RunResults"/> object with current messages state.
		/// </summary>
		/// <returns><see cref="RunResults"/> object with current messages state.</returns>
		public RunResults GetRunResults() => new RunResults(Output, Error, CombinedOutput);

		/// <summary>
		/// Clears all messages.
		/// </summary>
		public void Clear()
		{
			outputStringBuilder.Clear();
			errorStringBuilder.Clear();
			combinedStringBuilder.Clear();
		}

		/// <summary>
		/// Creates new instance of <see cref="AppendLineMessageFormatter"/> without already processed messages.
		/// </summary>
		/// <returns>New instance of <see cref="AppendLineMessageFormatter"/> without already processed messages.</returns>
		public IMessageFormatter GetCopy() => new AppendLineMessageFormatter();
	}
}
