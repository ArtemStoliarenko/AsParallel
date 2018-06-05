using System.Diagnostics;

namespace AsParallel
{
	/// <summary>
	/// Interface for handling console output.
	/// </summary>
	interface IOutputDataReceiver
	{
		/// <summary>
		/// Handles standard output.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event arguments.</param>
		void AddToOutput(object sender, DataReceivedEventArgs e);

		/// <summary>
		/// Handles error output.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event arguments.</param>
		void AddToError(object sender, DataReceivedEventArgs e);
	}
}
