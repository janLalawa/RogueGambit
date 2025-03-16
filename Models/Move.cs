namespace RogueGambit.Models;

public class Move
{
    public PieceModel Piece { get; set; }
    public Vector2 StartPosition { get; set; }
    public Vector2 EndPosition { get; set; }
}