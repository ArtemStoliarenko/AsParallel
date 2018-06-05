using System;

namespace AsParallel
{
	public sealed class LastLineMessageFormatter : IMessageFormatter
	{
		public string Output { get; private set; } = string.Empty;

		public string Error { get; private set; } = string.Empty;

		public string CombinedOutput { get; private set; } = string.Empty;

		public void AddToError(string message)
		{
			Error = message;
			CombinedOutput = message;
		}

		public void AddToOutput(string message)
		{
			Output = message;
			CombinedOutput = message;
		}

		public RunResults GetRunResults() => new RunResults(Output, Error, CombinedOutput);

		public void Clear()
		{
			Output = string.Empty;
			Error = string.Empty;
			CombinedOutput = string.Empty;
		}

		object ICloneable.Clone() => new LastLineMessageFormatter();
	}
}
