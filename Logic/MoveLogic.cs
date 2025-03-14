using RogueGambit.Handlers.Interface;

namespace RogueGambit.Logic;

public class MoveLogic : IMoveLogic
{
    [Inject] private readonly IGameStateHandler _gameStateHandler = null!;

    private bool HitTarget { get; set; }

    public void Initialize()
    {
        InjectDependencies(this);
    }


    public HashSet<Vector2> GetValidMoves(PieceModel piece)
    {
        var validMoves = new HashSet<Vector2>();
        validMoves.UnionWith(GetNormalMoves(piece));
        if (piece.MoveSet.HasChainedMoves) validMoves.UnionWith(GetChainedMoves(piece));
        foreach (var validMove in validMoves) GD.Print(validMove);

        return validMoves;
    }

    private HashSet<Vector2> GetNormalMoves(PieceModel piece)
    {
        var validMoves = new HashSet<Vector2>();
        var normalMoves = piece.MoveSet.Moves.Where(move => !move.IsChained);

        foreach (var move in normalMoves)
        {
            if (move.Attributes.Contains(MoveAttrib.FirstMove) && piece.HasMoved) continue;

            var direction = RotateVector(move.Direction, piece.Rotation);
            var validRay = CastRay(piece.GridPosition, direction, move.Range, move.Attributes, piece.Owner);
            validMoves.UnionWith(validRay);
        }

        return validMoves;
    }

    private HashSet<Vector2> GetChainedMoves(PieceModel piece)
    {
        var validMoves = new HashSet<Vector2>();
        var chainedMoves = GetSortedChainedMoves(piece.MoveSet);

        foreach (var chainGroup in chainedMoves.GroupBy(m => m.Chain.Item1))
        {
            var currentOrigin = piece.GridPosition;
            var chainValidMoves = new HashSet<Vector2>();
            HitTarget = false;

            foreach (var move in chainGroup)
            {
                if (HitTarget) break;

                var direction = RotateVector(move.Direction, piece.Rotation);
                var validRay = CastRay(currentOrigin, direction, move.Range, move.Attributes, piece.Owner);

                if (validRay.Count > 0)
                {
                    chainValidMoves.UnionWith(validRay);
                    currentOrigin = validRay.Last();
                }
                else
                {
                    chainValidMoves.Clear();
                    break;
                }
            }

            validMoves.UnionWith(chainValidMoves);
        }

        return validMoves;
    }

    private IOrderedEnumerable<MoveSet.Move> GetSortedChainedMoves(MoveSet moveSet)
    {
        return moveSet.Moves
                      .Where(m => m.IsChained)
                      .OrderBy(m => m.Chain.Item1) // Chain ID
                      .ThenBy(m => m.Chain.Item2); // Order in chain
    }

    private Vector2 RotateVector(Vector2 vector, int degrees)
    {
        if (degrees == 0) return vector;
        degrees = (degrees % 360 + 360) % 360;

        return degrees switch
        {
            90 => new Vector2(-vector.Y, vector.X),
            180 => new Vector2(-vector.X, -vector.Y),
            270 => new Vector2(vector.Y, -vector.X),
            _ => RotateVectorArbitrary(vector, degrees)
        };
    }

    private Vector2 RotateVectorArbitrary(Vector2 vector, int degrees)
    {
        var radians = degrees * (Mathf.Pi / 180);
        var cos = Mathf.Cos(radians);
        var sin = Mathf.Sin(radians);

        var x = vector.X * cos - vector.Y * sin;
        var y = vector.X * sin + vector.Y * cos;

        return new Vector2(
            Mathf.Abs(x) < 0.001f ? 0 : Mathf.Round(x),
            Mathf.Abs(y) < 0.001f ? 0 : Mathf.Round(y)
        );
    }

    private List<Vector2> CastRay(Vector2 origin,
                                  Vector2 direction,
                                  int distance,
                                  HashSet<MoveAttrib> attributes = null,
                                  PieceOwner owner = PieceOwner.None)
    {
        var pieces = _gameStateHandler.GameState.Pieces;
        var board = _gameStateHandler.GameState.BoardSquares;

        var isAttackOnly = attributes?.Contains(MoveAttrib.AttackOnly) ?? false;
        var isMoveOnly = attributes?.Contains(MoveAttrib.MoveOnly) ?? false;

        var validMoves = new List<Vector2>();
        var rayPos = origin;

        for (var i = 1; i <= distance; i++)
        {
            rayPos += direction;

            if (!board.ContainsKey(rayPos)) break; // Out of bounds, move is invalid and ray stops

            if (pieces.ContainsKey(rayPos))
            {
                if (FriendlyFire || pieces[rayPos].Owner != owner)
                    if (!isMoveOnly)
                    {
                        validMoves.Add(rayPos); // Found an enemy or friendly (with friendly fire), move is valid
                        HitTarget = true;
                    }

                break; // Friendly piece without friendly fire, stop the ray
            }

            // Empty square
            if (!isAttackOnly) validMoves.Add(rayPos);
        }

        return validMoves;
    }
}