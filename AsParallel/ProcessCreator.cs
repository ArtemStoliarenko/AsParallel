using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace AsParallel
{
	sealed class ProcessCreator : ICloneable, IDisposable
	{
		private readonly List<Process> processList = new List<Process>();

		private bool disposed;

		public string Filename { get; }

		public ReadOnlyCollection<string> Arguments { get; }

		public bool ShowWindow { get; }

		public ProcessCreator(string filename, IList<string> arguments, bool showWindow)
		{
			if (filename == null)
			{
				throw new ArgumentNullException(nameof(filename));
			}
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentException(nameof(filename));
			}

			if (arguments == null)
			{
				throw new ArgumentNullException(nameof(arguments));
			}
			if (arguments.Count == 0 || arguments.Any(parameter => parameter == null))
			{
				throw new ArgumentException(nameof(arguments));
			}

			this.Filename = filename;
			this.Arguments = new ReadOnlyCollection<string>(arguments);
			this.ShowWindow = showWindow;
		}

		public ReadOnlyCollection<Process> CreateProcesses(IOutputDataReceiver outputDataReceiver = null)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ProcessCreator));

			DisposeProcesses();
			processList.Clear();

			var processWindowStyle = ShowWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
			foreach (string arguments in Arguments)
			{
				var process = CreateProcess(arguments, processWindowStyle, outputDataReceiver);
				processList.Add(process);
			}

			return new ReadOnlyCollection<Process>(processList);
		}

		private Process CreateProcess(string arguments, ProcessWindowStyle processWindowStyle, IOutputDataReceiver outputDataReceiver)
		{
			var process = new Process();

			process.StartInfo.FileName = Filename;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = processWindowStyle;
			if (outputDataReceiver != null)
			{
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.OutputDataReceived += outputDataReceiver.AddToOutput;
				process.ErrorDataReceived += outputDataReceiver.AddToError;
			}
			process.EnableRaisingEvents = true;

			return process;
		}

		public ProcessCreator Clone()
		{
			return new ProcessCreator(Filename, Arguments, ShowWindow);
		}

		object ICloneable.Clone() => Clone();

		public void Dispose()
		{
			if (!disposed)
			{
				DisposeProcesses();
				disposed = true;
			}
		}

		private void DisposeProcesses()
		{
			processList.ForEach(p => p.Dispose());
		}
	}
}
