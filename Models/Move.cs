namespace RogueGambit.Models;

public class Move
{
    public PieceModel Piece { get; set; }
    public Vector2 StartPosition { get; set; }
    public Vector2 EndPosition { get; set; }
    public bool IsCapture { get; set; } = false;

    public override string ToString()
    {
        return $"{Piece} to {EndPosition}";
    }
}

public class MoveEval
{
    public Move Move { get; set; }
    public int Score { get; set; }
    public int Depth { get; set; }
}