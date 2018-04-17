using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsParallel
{
	public sealed class AsParallel : IOutputDataReceiver, ICloneable, IDisposable
    {
		private readonly ProcessCreator processCreator;

		private readonly object locker = new object();

		private bool disposed = false;

		public bool IsLongRunning { get; }
		
		public Task<RunResults> CurrentTask { get; private set; }

		private readonly StringBuilder output = new StringBuilder();
		public string Output { get => output.ToString(); }
		public event EventHandler<string> OutputChanged;

		private readonly StringBuilder error = new StringBuilder();
		public string Error { get => error.ToString(); }
		public event EventHandler<string> ErrorChanged;

		private readonly StringBuilder combinedOutput = new StringBuilder();
		public string CombinedOutput { get => combinedOutput.ToString(); }
		public event EventHandler<string> CombinedOutputChanged;

		public AsParallel(string filename, string argument, int processCount, bool isLongRunning = false, bool showWindow = false)
			: this(filename, Enumerable.Repeat(argument, processCount).ToArray(), isLongRunning, showWindow)
		{
			if (processCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(processCount));
		}

		public AsParallel(string filename, IList<string> arguments, bool isLongRunning = false, bool showWindow = false)
		{
			this.processCreator = new ProcessCreator(filename, arguments, showWindow);
			this.IsLongRunning = isLongRunning;
		}

		public RunResults Start()
		{
			return StartAsync().Result;
		}
		
		public Task<RunResults> StartAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(AsParallel));

			lock (locker)
			{
				if (CurrentTask == null || CurrentTask.IsCompleted)
				{
					var processCollection = processCreator.CreateProcesses(this);
					var tasks = processCollection.Select(RunProcess);
					CurrentTask = Task.WhenAll(tasks).ContinueWith(task => new RunResults(Output, Error, CombinedOutput));

					return CurrentTask;
				}
				else
				{
					throw new InvalidOperationException(nameof(AsParallel));
				}
			}
		}

		public AsParallel Clone()
		{
			return new AsParallel(processCreator.Clone(), IsLongRunning);
		}

		object ICloneable.Clone() => Clone();

		public void Dispose()
		{
			if (!disposed)
			{
				processCreator?.Dispose();
				disposed = true;
			}
		}

		public void AddToOutput(object sender, DataReceivedEventArgs e)
		{
			lock(locker)
			{
				output.AppendLine(e.Data);
				combinedOutput.AppendLine(e.Data);

				RaiseOutputChanged();
				RaiseCombinedOutputChanged();
			}
		}

		public void AddToError(object sender, DataReceivedEventArgs e)
		{
			lock(locker)
			{
				error.AppendLine(e.Data);
				combinedOutput.AppendLine(e.Data);

				RaiseErrorChanged();
				RaiseCombinedOutputChanged();
			}
		}

		private void RaiseOutputChanged() => OutputChanged?.Invoke(this, Output);

		private void RaiseErrorChanged() => ErrorChanged?.Invoke(this, Error);

		private void RaiseCombinedOutputChanged() => CombinedOutputChanged?.Invoke(this, CombinedOutput);

		private Task RunProcess(Process process)
		{
			var tcs = new TaskCompletionSource<bool>();

			process.Exited += (sender, args) =>
			{
				tcs.TrySetResult(true);
				process.Dispose();
			};
			process.Start();

			return tcs.Task;
		}

		private AsParallel(ProcessCreator processCreator, bool isLongRunning)
		{
			this.processCreator = processCreator;
			this.IsLongRunning = isLongRunning;
		}
	}
}
