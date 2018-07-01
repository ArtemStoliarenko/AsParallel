using System;

namespace AsParallel
{
	public interface IMessageFormatter
	{
		/// <summary>
		/// Current standard output of the running processes.
		/// </summary>
		string Output { get; }

		/// <summary>
		/// Current error output of the running processes.
		/// </summary>
		string Error { get; }

		/// <summary>
		/// Current combined output of the running processes.
		/// </summary>
		string CombinedOutput { get; }

		/// <summary>
		/// Processes new message for standard and combined output.
		/// </summary>
		/// <param name="message">Message to be processed.</param>
		void AddToOutput(string message);

		/// <summary>
		/// Processes new message for error and combined output.
		/// </summary>
		/// <param name="message">Message to be processed.</param>
		void AddToError(string message);

		/// <summary>
		/// Gets <see cref="RunResults"/> object with current messages state.
		/// </summary>
		/// <returns><see cref="RunResults"/> object with current messages state.</returns>
		RunResults GetRunResults();

		/// <summary>
		/// Clears all messages.
		/// </summary>
		void Clear();

		/// <summary>
		/// Creates new <see cref="IMessageFormatter"/> of the same type, but without already processed messages.
		/// </summary>
		/// <returns>New instance of <see cref="IMessageFormatter"/> of the same type, but without already processed messages.</returns>
		IMessageFormatter GetCopy();
	}
}
