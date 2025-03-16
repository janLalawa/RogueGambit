namespace RogueGambit.Engine.Uci;

public interface IUciHandler
{
    Task<bool> ConnectToEngine(string enginePath);
    void DisconnectEngine();
    Task InitializeUci();
    Task<bool> IsEngineReady();
    void SetPosition(string fen = "startpos", List<string> moves = null);
    Task<string> GetBestMove(int depth = 0, int timeMs = 0);
    void StopCalculation();
    void SetOption(string name, string value);
    void NewGame();

    event Action<string> BestMoveFound;
    event Action<EngineAnalysisInfo> AnalysisUpdated;
}