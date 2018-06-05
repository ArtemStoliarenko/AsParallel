using System;

namespace AsParallel
{
	public interface IMessageFormatter : ICloneable
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

		void AddToOutput(string message);

		void AddToError(string message);

		RunResults GetRunResults();

		void Clear();
	}
}
