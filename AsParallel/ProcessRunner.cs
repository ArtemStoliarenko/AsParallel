using AsParallel.ConcurrentMessaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsParallel
{
	/// <summary>
	/// Incapsulates parallel process running logic.
	/// </summary>
	public sealed class ProcessRunner : IMessageSender, ICloneable, IDisposable
	{
		private readonly ProcessCreator processCreator;

		private readonly IMessageFormatter messageFormatter;

		private readonly object locker = new object();

		private bool disposed = false;

		/// <summary>
		/// Returns the task which will we complete after last process finishes.
		/// </summary>
		public Task<RunResults> CurrentTask { get; private set; }

		/// <summary>
		/// Sends an event when <see cref="Output"/> is updated.
		/// </summary>
		public event EventHandler<string> OutputChanged;

		/// <summary>
		/// Sends an event when <see cref="Error"/> is updated.
		/// </summary>
		public event EventHandler<string> ErrorChanged;

		/// <summary>
		/// Sends an event when <see cref="CombinedOutput"/> is updated.
		/// </summary>
		public event EventHandler<string> CombinedOutputChanged;

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="argument">Argument to launch executables with.</param>
		/// <param name="processCount">Amount of processes to be executed simultaneously.</param>
		public ProcessRunner(string filename, string argument, int processCount)
			: this(filename, argument, processCount, false, new AppendLineMessageFormatter())
		{ }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="argument">Argument to launch executables with.</param>
		/// <param name="processCount">Amount of processes to be executed simultaneously.</param>
		/// <param name="showWindow">True if a new window should be created for every process instance; otherwise, false.</param>
		public ProcessRunner(string filename, string argument, int processCount, bool showWindow)
			: this(filename, argument, processCount, showWindow, new AppendLineMessageFormatter())
		{ }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="argument">Argument to launch executables with.</param>
		/// <param name="processCount">Amount of processes to be executed simultaneously.</param>
		/// <param name="messageFormatter"><see cref="IMessageFormatter"/> instance which defines output format.</param>
		public ProcessRunner(string filename, string argument, int processCount, IMessageFormatter messageFormatter)
			: this(filename, argument, processCount, false, messageFormatter)
		{ }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="argument">Argument to launch executables with.</param>
		/// <param name="processCount">Amount of processes to be executed simultaneously.</param>
		/// <param name="showWindow">True if a new window should be created for every process instance; otherwise, false.</param>
		/// <param name="messageFormatter"><see cref="IMessageFormatter"/> instance which defines output format.</param>
		public ProcessRunner(string filename, string argument, int processCount, bool showWindow, IMessageFormatter messageFormatter)
			: this(filename, Enumerable.Repeat(argument, processCount).ToArray(), showWindow, messageFormatter)
		{
			if (processCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(processCount));
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="arguments">Argument to launch executables with.</param>
		public ProcessRunner(string filename, IList<string> arguments)
			: this(filename, arguments, false, new AppendLineMessageFormatter())
		{ }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="arguments">Argument to launch executables with.</param>
		/// <param name="showWindow">True if a new window should be created for every process instance; otherwise, false.</param>
		public ProcessRunner(string filename, IList<string> arguments, bool showWindow)
			: this(filename, arguments, showWindow, new AppendLineMessageFormatter())
		{ }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="arguments">Argument to launch executables with.</param>
		/// <param name="messageFormatter"><see cref="IMessageFormatter"/> instance which defines output format.</param>
		public ProcessRunner(string filename, IList<string> arguments, IMessageFormatter messageFormatter)
			: this(filename, arguments, false, messageFormatter)
		{ }

		/// <summary>
		/// Initializes a new instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <param name="filename">Filename of the executable to be run.</param>
		/// <param name="arguments">Argument to launch executables with.</param>
		/// <param name="showWindow">True if a new window should be created for every process instance; otherwise, false.</param>
		/// <param name="messageFormatter"><see cref="IMessageFormatter"/> instance which defines output format. Default is <see cref="AppendLineMessageFormatter"/>.</param>
		public ProcessRunner(string filename, IList<string> arguments, bool showWindow, IMessageFormatter messageFormatter)
		{
			this.processCreator = new ProcessCreator(filename, arguments, showWindow);

			if (messageFormatter == null)
				throw new ArgumentNullException(nameof(messageFormatter));
			this.messageFormatter = messageFormatter;
		}

		/// <summary>
		/// Executes proccesses and return the <see cref="RunResults"/> instance.
		/// </summary>
		/// <returns><see cref="RunResults"/> instance with the results of the execution.</returns>
		public RunResults Start() => StartAsync().Result;

		/// <summary>
		/// Executes proccesses asynchronously and return the <see cref="RunResults"/> instance.
		/// </summary>
		/// <returns><see cref="RunResults"/> instance with the results of the execution.</returns>
		public Task<RunResults> StartAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ProcessRunner));

			lock (locker)
			{
				if (CurrentTask == null || CurrentTask.IsCompleted)
				{
					messageFormatter.Clear();

					var concurrentDataReceiver = new ConcurrentDataReceiver(this);
					var processCollection = processCreator.CreateProcesses(concurrentDataReceiver);

					var tasks = processCollection.Select(RunProcess);
					var ctrCancellationToken = new CancellationTokenSource();
					var outputDataReceiverTask = concurrentDataReceiver.Run(ctrCancellationToken.Token);

					CurrentTask = Task.WhenAll(tasks).ContinueWith(task => new RunResults(Output, Error, CombinedOutput));
					outputDataReceiverTask.ContinueWith(task => ctrCancellationToken.Dispose());

					return CurrentTask;
				}
				else
				{
					throw new InvalidOperationException(nameof(ProcessRunner));
				}
			}
		}

		/// <summary>
		/// Formats output message and raised events related.
		/// </summary>
		/// <param name="message">Output message to be formatted and set.</param>
		void IMessageSender.SendOutputMessage(string message)
		{
			messageFormatter.AddToOutput(message);

			RaiseOutputChanged(messageFormatter.Output);
			RaiseCombinedOutputChanged(messageFormatter.CombinedOutput);
		}

		/// <summary>
		/// Formats error message and raised events related.
		/// </summary>
		/// <param name="message">Error message to be formatted and set.</param>
		void IMessageSender.SendErrorMessage(string message)
		{
			messageFormatter.AddToError(message);

			RaiseErrorChanged(messageFormatter.Error);
			RaiseCombinedOutputChanged(messageFormatter.CombinedOutput);
		}

		/// <summary>
		/// Clones the current instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <returns>Cloned instance of the <see cref="ProcessRunner"/>.</returns>
		public ProcessRunner Clone()
		{
			return new ProcessRunner(processCreator.Clone(), (IMessageFormatter)messageFormatter.Clone());
		}

		/// <summary>
		/// Clones the current instance of <see cref="ProcessRunner"/>.
		/// </summary>
		/// <returns>Cloned instance of the <see cref="ProcessRunner"/>.</returns>
		object ICloneable.Clone() => Clone();

		/// <summary>
		/// Disposes the current instance of <see cref="ProcessRunner"/> class.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				processCreator?.Dispose();
				disposed = true;
			}
		}

		private void RaiseOutputChanged(string message) => OutputChanged?.Invoke(this, message);

		private void RaiseErrorChanged(string message) => ErrorChanged?.Invoke(this, message);

		private void RaiseCombinedOutputChanged(string message) => CombinedOutputChanged?.Invoke(this, message);

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

		private ProcessRunner(ProcessCreator processCreator, IMessageFormatter messageFormatter)
		{
			this.processCreator = processCreator;
			this.messageFormatter = messageFormatter;
		}
	}
}
