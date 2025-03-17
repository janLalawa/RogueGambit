namespace RogueGambit.Engine.RogueAi.Bobby;

public class BobbyAi : IRogueAi
{
    [Inject] private readonly IMoveLogic _moveLogic = null!;

    private readonly Dictionary<PieceType, float> _pieceValues = new()
    {
        { PieceType.Pawn, 1.0f },
        { PieceType.Knight, 3.0f },
        { PieceType.Bishop, 3.0f },
        { PieceType.Rook, 5.0f },
        { PieceType.Queen, 9.0f },
        { PieceType.King, 100.0f }
    };

    private int _searchDepth = 1;

    public BobbyAi()
    {
        InjectDependencies(this);
    }

    public Task<Move> GetBestMove(GameState gameState)
    {
        // Get all pieces for the current player
        var playerPieces = gameState.Pieces
                                    .Where(p => p.Value.Owner == gameState.CurrentTurn)
                                    .ToList();

        // If no pieces, return null
        if (!playerPieces.Any()) return Task.FromResult<Move>(null);

        // List to store all possible moves with their scores
        var scoredMoves = new List<MoveEval>();

        // Evaluate each possible move
        foreach (var piece in playerPieces)
        {
            var validMoves = _moveLogic.GetValidMoves(piece.Value);

            foreach (var endPosition in validMoves)
            {
                // Check if this is a capture move
                var isCapture = gameState.Pieces.ContainsKey(endPosition) &&
                                gameState.Pieces[endPosition].Owner != gameState.CurrentTurn;

                // Create a move
                var move = new Move
                {
                    Piece = piece.Value,
                    StartPosition = piece.Value.GridPosition,
                    EndPosition = endPosition,
                    IsCapture = isCapture
                };

                // Score for this move
                var score = EvaluateMove(gameState, move);

                scoredMoves.Add(new MoveEval
                {
                    Move = move,
                    Score = score,
                    Depth = 0
                });
            }
        }

        // If we have scored moves, pick the best one
        if (scoredMoves.Any())
        {
            var bestMoveEval = scoredMoves.OrderByDescending(m => m.Score).First();
            return Task.FromResult(bestMoveEval.Move);
        }

        // Fallback to random move if no scored moves
        var randomPiece = playerPieces[new Random().Next(playerPieces.Count)];
        var randomMoves = _moveLogic.GetValidMoves(randomPiece.Value);

        if (randomMoves.Any())
        {
            var randomEndPosition = randomMoves.ElementAt(new Random().Next(randomMoves.Count));

            // Check if this is a capture move
            var isCapture = gameState.Pieces.ContainsKey(randomEndPosition) &&
                            gameState.Pieces[randomEndPosition].Owner != gameState.CurrentTurn;

            return Task.FromResult(new Move
            {
                Piece = randomPiece.Value,
                StartPosition = randomPiece.Value.GridPosition,
                EndPosition = randomEndPosition,
                IsCapture = isCapture
            });
        }

        // No valid moves found
        return Task.FromResult<Move>(null);
    }

    public Task<int> EvaluatePosition(GameState gameState)
    {
        var score = 0;

        // Material advantage
        foreach (var piece in gameState.Pieces.Values)
        {
            var pieceValue = _pieceValues.ContainsKey(piece.Type) ? _pieceValues[piece.Type] : 1.0f;

            if (piece.Owner == gameState.CurrentTurn)
                score += (int)(pieceValue * 100);
            else
                score -= (int)(pieceValue * 100);
        }

        // Center control bonus
        var centerX = gameState.BoardShape.X / 2;
        var centerY = gameState.BoardShape.Y / 2;

        foreach (var piece in gameState.Pieces.Values)
            if (piece.Owner == gameState.CurrentTurn)
            {
                var distanceToCenter = Math.Abs(piece.GridPosition.X - centerX) +
                                       Math.Abs(piece.GridPosition.Y - centerY);

                var maxDistance = centerX + centerY;
                var centerBonus = 20 * (1 - distanceToCenter / maxDistance);

                score += (int)centerBonus;
            }

        return Task.FromResult(score);
    }

    public Task SetSearchDepth(int depth)
    {
        _searchDepth = Math.Max(1, Math.Min(depth, 3)); // Limit depth between 1 and 3
        return Task.CompletedTask;
    }

    public Task SetEvaluationWeights(Dictionary<PieceType, float> weights)
    {
        foreach (var weight in weights)
            if (_pieceValues.ContainsKey(weight.Key))
                _pieceValues[weight.Key] = weight.Value;

        return Task.CompletedTask;
    }

    private int EvaluateMove(GameState gameState, Move move)
    {
        var score = 0;

        // Check if we're capturing a piece
        if (move.IsCapture)
        {
            var capturedPiece = gameState.Pieces[move.EndPosition];

            // Add score based on the value of the captured piece
            var captureValue = _pieceValues.ContainsKey(capturedPiece.Type)
                ? _pieceValues[capturedPiece.Type]
                : 1.0f;

            score += (int)(captureValue * 100);
        }

        // Bonus for moving toward the center
        var centerX = gameState.BoardShape.X / 2;
        var centerY = gameState.BoardShape.Y / 2;

        var startDistanceToCenter = Math.Abs(move.StartPosition.X - centerX) +
                                    Math.Abs(move.StartPosition.Y - centerY);

        var endDistanceToCenter = Math.Abs(move.EndPosition.X - centerX) +
                                  Math.Abs(move.EndPosition.Y - centerY);

        // If we're moving closer to the center, add a bonus
        if (endDistanceToCenter < startDistanceToCenter) score += 10;

        // Check if the move puts our piece in danger (simple check)
        foreach (var piece in gameState.Pieces.Values)
            if (piece.Owner != gameState.CurrentTurn)
            {
                var opponentMoves = _moveLogic.GetValidMoves(piece);
                if (opponentMoves.Contains(move.EndPosition))
                {
                    // Our piece might be captured next turn
                    var pieceValue = _pieceValues.ContainsKey(move.Piece.Type)
                        ? _pieceValues[move.Piece.Type]
                        : 1.0f;

                    // Penalize moves that put valuable pieces in danger
                    score -= (int)(pieceValue * 50);
                }
            }

        // Add some randomness to avoid predictable play
        score += new Random().Next(5);

        return score;
    }
}