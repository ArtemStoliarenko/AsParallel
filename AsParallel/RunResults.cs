namespace AsParallel
{
	/// <summary>
	/// Encaplsulates the results of the process execution.
	/// </summary>
	public sealed class RunResults
	{
		/// <summary>
		/// Standard output string.
		/// </summary>
		public string Output { get; }

		/// <summary>
		/// Error output string.
		/// </summary>
		public string Error { get; }

		/// <summary>
		/// Combined output string.
		/// </summary>
		public string CombinedOutput { get; }

		/// <summary>
		/// Initalizes a new instance of <see cref="RunResults"/> class.
		/// </summary>
		/// <param name="output">Standard output string.</param>
		/// <param name="error">Error output string.</param>
		/// <param name="combinedOutput">Combined output string.</param>
		internal RunResults(string output, string error, string combinedOutput)
		{
			this.Output = output;
			this.Error = error;
			this.CombinedOutput = combinedOutput;
		}
	}
}
