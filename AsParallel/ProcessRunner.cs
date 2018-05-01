﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsParallel
{
	public sealed class ProcessRunner : IOutputDataReceiver, ICloneable, IDisposable
	{
		private readonly ProcessCreator processCreator;

		private readonly object locker = new object();

		private bool disposed = false;

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

		public ProcessRunner(string filename, string argument, int processCount, bool showWindow = false)
			: this(filename, Enumerable.Repeat(argument, processCount).ToArray(), showWindow)
		{
			if (processCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(processCount));
		}

		public ProcessRunner(string filename, IList<string> arguments, bool showWindow = false)
		{
			this.processCreator = new ProcessCreator(filename, arguments, showWindow);
		}

		public RunResults Start()
		{
			return StartAsync().Result;
		}

		public Task<RunResults> StartAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ProcessRunner));

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
					throw new InvalidOperationException(nameof(ProcessRunner));
				}
			}
		}

		public ProcessRunner Clone()
		{
			return new ProcessRunner(processCreator.Clone());
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

		void IOutputDataReceiver.AddToOutput(object sender, DataReceivedEventArgs e)
		{
			lock (locker)
			{
				output.AppendLine(e.Data);
				combinedOutput.AppendLine(e.Data);

				RaiseOutputChanged();
				RaiseCombinedOutputChanged();
			}
		}

		void IOutputDataReceiver.AddToError(object sender, DataReceivedEventArgs e)
		{
			lock (locker)
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

		private ProcessRunner(ProcessCreator processCreator)
		{
			this.processCreator = processCreator;
		}
	}
}
