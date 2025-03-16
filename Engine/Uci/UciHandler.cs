namespace RogueGambit.Engine.Uci;

public partial class UciHandler : Node2D, IUciHandler
{
	private readonly Queue<string> _commandQueue = new();
	private readonly Dictionary<string, string> _engineOptions = new();
	[Inject] private readonly IGameStateHandler _gameStateHandler = null!;
	private CancellationTokenSource _cts;
	private StreamWriter _engineInput;
	private StreamReader _engineOutput;

	private Process _engineProcess;

	private bool _engineReady;
	private bool _isConnected;


	public async Task<bool> ConnectToEngine(string enginePath)
	{
		try
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = enginePath,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			_engineProcess = new Process { StartInfo = startInfo };
			_engineProcess.OutputDataReceived += (sender, args) =>
			{
				if (args.Data != null)
					OnEngineOutput(args.Data);
			};

			_engineProcess.Start();
			_engineProcess.BeginOutputReadLine();

			_engineInput = _engineProcess.StandardInput;
			_engineOutput = _engineProcess.StandardOutput;
			_cts = new CancellationTokenSource();

			await InitializeUci();

			var isReady = await IsEngineReady();
			_isConnected = isReady;

			GD.Print(
				isReady
					? $"Successfully connected to engine: {enginePath}"
					: $"Engine connected but not responding properly: {enginePath}"
			);

			return isReady;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to connect to engine: {ex.Message}");
			DisconnectEngine();
			return false;
		}
	}

	public void DisconnectEngine()
	{
		try
		{
			_cts?.Cancel();

			if (_isConnected && _engineInput != null)
			{
				SendCommand("quit");
				_engineInput.Close();
			}

			if (_engineProcess != null && !_engineProcess.HasExited)
			{
				_engineProcess.Kill();
				_engineProcess.Dispose();
			}

			_engineInput = null;
			_engineOutput = null;
			_engineProcess = null;
			_isConnected = false;
			_engineReady = false;

			GD.Print("Engine disconnected");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error disconnecting engine: {ex.Message}");
		}
	}

	public async Task InitializeUci()
	{
		SendCommand("uci");

		try
		{
			await WaitForResponse("uciok");
			GD.Print("UCI mode initialized");
		}
		catch (TimeoutException)
		{
			GD.PrintErr("Engine did not respond with uciok");
		}
	}

	public async Task<bool> IsEngineReady()
	{
		SendCommand("isready");

		try
		{
			await WaitForResponse("readyok", 3000);
			return true;
		}
		catch (TimeoutException)
		{
			GD.PrintErr("Engine did not respond with readyok in time");
			return false;
		}
	}

	public void SetPosition(string fen = "startpos", List<string> moves = null)
	{
		if (!_isConnected) return;

		var command = $"position {fen}";
		if (moves is { Count: > 0 }) command += " moves " + string.Join(" ", moves);

		SendCommand(command);
	}


	public async Task<string> GetBestMove(int depth = 0, int timeMs = 0)
	{
		if (!_isConnected) return string.Empty;

		var command = "go";

		if (depth > 0) command += $" depth {depth}";

		if (timeMs > 0) command += $" movetime {timeMs}";

		SendCommand(command);

		try
		{
			var response = await WaitForResponse("bestmove", 30000);
			var parts = response.Split(' ');

			if (parts.Length < 2) return string.Empty;
			var bestMove = parts[1];
			BestMoveFound?.Invoke(bestMove);
			return bestMove;
		}
		catch (TimeoutException)
		{
			GD.PrintErr("Timeout waiting for bestmove");
			StopCalculation();
			return string.Empty;
		}
	}

	public void StopCalculation()
	{
		if (_isConnected) SendCommand("stop");
	}

	public void SetOption(string name, string value)
	{
		if (!_isConnected) return;

		SendCommand($"setoption name {name} value {value}");
		_engineOptions[name] = value;
	}

	public void NewGame()
	{
		if (!_isConnected) return;

		SendCommand("ucinewgame");
		_engineReady = false;
	}

	public event Action<string> BestMoveFound;
	public event Action<EngineAnalysisInfo> AnalysisUpdated;

	private void SendCommand(string command)
	{
		if (!_isConnected || _engineOutput is null)
		{
			GD.PrintErr("Engine not connected or output stream not available.");
			return;
		}

		GD.Print($"→ {command}");
		_engineInput.WriteLine(command);
		_engineInput.Flush();
	}

	private async Task<string> WaitForResponse(string expectedPrefix, int timeoutMs = 5000)
	{
		var timeoutTask = Task.Delay(timeoutMs, _cts.Token);
		var responseTask = Task.Run(async () =>
		{
			while (!_cts.Token.IsCancellationRequested)
			{
				if (_commandQueue.Count > 0)
				{
					var response = _commandQueue.Dequeue();
					if (response.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)) return response;
				}

				await Task.Delay(10, _cts.Token);
			}

			return null;
		}, _cts.Token);

		var completedTask = await Task.WhenAny(responseTask, timeoutTask);

		if (completedTask == timeoutTask) throw new TimeoutException($"Timeout waiting for response starting with '{expectedPrefix}'");

		var result = await responseTask;
		return result ?? string.Empty;
	}

	private void OnEngineOutput(string output)
	{
		if (string.IsNullOrWhiteSpace(output)) return;

		GD.Print($"← {output}");

		_commandQueue.Enqueue(output);

		if (output.StartsWith("info", StringComparison.OrdinalIgnoreCase))
		{
			ProcessInfoLine(output);
		}
		else if (output.StartsWith("bestmove", StringComparison.OrdinalIgnoreCase))
		{
			var parts = output.Split(' ');
			if (parts.Length >= 2) BestMoveFound?.Invoke(parts[1]);
		}
		else if (output.StartsWith("readyok", StringComparison.OrdinalIgnoreCase))
		{
			_engineReady = true;
		}
	}

	private void ProcessInfoLine(string info)
	{
		var parts = info.Split(' ');
		var analysisInfo = new EngineAnalysisInfo();

		for (var i = 1; i < parts.Length; i++)
			switch (parts[i])
			{
				case "depth":
					if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out var depth))
					{
						analysisInfo.Depth = depth;
						i++;
					}

					break;

				case "score":
					if (i + 2 < parts.Length)
					{
						if (parts[i + 1] == "cp" && int.TryParse(parts[i + 2], out var score))
						{
							analysisInfo.Score = score;
							analysisInfo.IsMate = false;
							i += 2;
						}
						else if (parts[i + 1] == "mate" && int.TryParse(parts[i + 2], out var mateIn))
						{
							analysisInfo.Score = mateIn;
							analysisInfo.IsMate = true;
							i += 2;
						}
					}

					break;

				case "pv":
					var pvMoves = new List<string>();
					i++;
					while (i < parts.Length && !parts[i].Contains("bmc") && !parts[i].Contains("tb"))
					{
						pvMoves.Add(parts[i]);
						i++;
					}

					i--;
					analysisInfo.PrincipalVariation = pvMoves;
					break;

				case "multipv":
				case "nodes":
				case "nps":
				case "hashfull":
				case "tbhits":
				case "time":
					if (i + 1 < parts.Length)
					{
						analysisInfo.AdditionalInfo[parts[i]] = parts[i + 1];
						i++;
					}

					break;
			}

		AnalysisUpdated?.Invoke(analysisInfo);
	}

	public override void _Ready()
	{
		GD.Print("...UciHandler ready.");
		Initialize();
	}

	public override void _Process(double delta)
	{
	}

	private void Initialize()
	{
		InjectDependencies(this);
	}
}

public class EngineAnalysisInfo
{
	public int Depth { get; set; }
	public int Score { get; set; }
	public bool IsMate { get; set; }
	public List<string> PrincipalVariation { get; set; } = new();
	public Dictionary<string, string> AdditionalInfo { get; set; } = new();
}
