namespace RogueGambit.Engine.RogueAi;

public interface IRogueAi
{
    Task<Move> GetBestMove(GameState gameState);
    Task<int> EvaluatePosition(GameState gameState);
    Task SetSearchDepth(int depth);
    Task SetEvaluationWeights(Dictionary<PieceType, float> weights);
}