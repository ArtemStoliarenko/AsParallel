namespace AsParallel
{
	public sealed class RunResults
    {
		public string Output { get; }

		public string Error { get; }

		public string CombinedOutput { get; }

		internal RunResults(string output, string error, string combinedOutput)
		{
			this.Output = output;
			this.Error = error;
			this.CombinedOutput = combinedOutput;
		}
    }
}
