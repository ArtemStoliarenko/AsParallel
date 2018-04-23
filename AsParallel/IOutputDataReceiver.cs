using System.Diagnostics;

namespace AsParallel
{
	interface IOutputDataReceiver
	{
		void AddToOutput(object sender, DataReceivedEventArgs e);

		void AddToError(object sender, DataReceivedEventArgs e);
	}
}
