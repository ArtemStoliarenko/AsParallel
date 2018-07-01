using AsParallel.ConcurrentMessaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace AsParallel
{
	/// <summary>
	/// Encaplsulates process set creation.
	/// </summary>
	sealed class ProcessCreator : ICloneable, IDisposable
	{
		private readonly List<Process> processList = new List<Process>();

		private bool disposed;

		/// <summary>
		/// Process filename.
		/// </summary>
		public string Filename { get; }

		/// <summary>
		/// Arguments that will be used for processes execution.
		/// </summary>
		public ReadOnlyCollection<string> Arguments { get; }

		/// <summary>
		/// True if a new window should be created for every process instance; otherwise, false.
		/// </summary>
		public bool ShowWindow { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessCreator"/> class.
		/// </summary>
		/// <param name="filename">Filename of the executebale to be run.</param>
		/// <param name="arguments">Collection of arguments to start executable with.</param>
		/// <param name="showWindow">True if a new window should be created for every process instance; otherwise, false.</param>
		public ProcessCreator(string filename, IList<string> arguments, bool showWindow)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentException(nameof(filename));

			if (arguments == null)
				throw new ArgumentNullException(nameof(arguments));

			if (arguments.Count == 0 || arguments.Any(parameter => parameter == null))
				throw new ArgumentException(nameof(arguments));

			this.Filename = filename;
			this.Arguments = new ReadOnlyCollection<string>(arguments);
			this.ShowWindow = showWindow;
		}

		/// <summary>
		/// Creates a collection of processes to be executed.
		/// </summary>
		/// <param name="concurrentDataReceiver"><see cref="ConcurrentDataReceiver"/> to handle process output.</param>
		/// <returns>Created collection of processes.</returns>
		public ReadOnlyCollection<Process> CreateProcesses(ConcurrentDataReceiver concurrentDataReceiver = null)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ProcessCreator));

			DisposeProcesses();
			processList.Clear();

			foreach (string arguments in Arguments)
			{
				var process = CreateProcess(arguments, concurrentDataReceiver);
				processList.Add(process);
			}

			return new ReadOnlyCollection<Process>(processList);
		}

		private Process CreateProcess(string arguments, ConcurrentDataReceiver concurrentDataReceiver)
		{
			var process = new Process();

			process.StartInfo.FileName = Filename;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = ShowWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = !ShowWindow;

			if (concurrentDataReceiver != null)
			{
				process.OutputDataReceived += concurrentDataReceiver.AddToOutput;
				process.ErrorDataReceived += concurrentDataReceiver.AddToError;

				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
			}

			process.EnableRaisingEvents = true;

			return process;
		}

		/// <summary>
		/// Clones the current instance of <see cref="ProcessCreator"/>.
		/// </summary>
		/// <returns>Cloned instance of <see cref="ProcessCreator"/></returns>
		public ProcessCreator Clone()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ProcessCreator));

			return new ProcessCreator(Filename, Arguments, ShowWindow);
		}

		/// <summary>
		/// Clones the current instance of <see cref="ProcessCreator"/>.
		/// </summary>
		/// <returns>Cloned instance of <see cref="ProcessCreator"/></returns>
		object ICloneable.Clone() => Clone();

		/// <summary>
		/// Disposes the current instance of <see cref="ProcessCreator"/>.
		/// </summary>
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
