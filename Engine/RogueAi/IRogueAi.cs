namespace RogueGambit.Engine.RogueAi;

public interface IRogueAi
{
    Move GetBestMove(GameState gameState);
    int EvaluatePosition(GameState gameState);
    void SetSearchDepth(int depth);
    void SetEvaluationWeights(Dictionary<PieceType, float> weights);
}