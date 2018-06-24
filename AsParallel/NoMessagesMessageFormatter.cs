using System;

namespace AsParallel
{
	public class NoMessagesMessageFormatter : IMessageFormatter
	{
		public string Output => string.Empty;

		public string Error => string.Empty;

		public string CombinedOutput => string.Empty;

		public void AddToError(string message)
		{ }

		public void AddToOutput(string message)
		{ }

		public void Clear()
		{ }

		public RunResults GetRunResults() => new RunResults(Output, Error, CombinedOutput);

		object ICloneable.Clone() => new NoMessagesMessageFormatter();
	}
}
