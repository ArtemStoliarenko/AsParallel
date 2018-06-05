using System;
using System.Text;

namespace AsParallel
{
	public sealed class AppendLineMessageFormatter : IMessageFormatter
	{
		private readonly StringBuilder outputStringBuilder = new StringBuilder();
		private readonly StringBuilder errorStringBuilder = new StringBuilder();
		private readonly StringBuilder combinedStringBuilder = new StringBuilder();

		public string Output => outputStringBuilder.ToString();

		public string Error => errorStringBuilder.ToString();

		public string CombinedOutput => combinedStringBuilder.ToString();

		public void AddToError(string message)
		{
			errorStringBuilder.AppendLine(message);
			combinedStringBuilder.AppendLine(message);
		}

		public void AddToOutput(string message)
		{
			outputStringBuilder.AppendLine(message);
			combinedStringBuilder.AppendLine(message);
		}

		public RunResults GetRunResults() => new RunResults(Output, Error, CombinedOutput);

		public void Clear()
		{
			outputStringBuilder.Clear();
			errorStringBuilder.Clear();
			combinedStringBuilder.Clear();
		}

		object ICloneable.Clone() => new AppendLineMessageFormatter();
	}
}
