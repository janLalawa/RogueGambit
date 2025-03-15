namespace RogueGambit.Utils;

public static class FenUtils
{
    public static char GetFenChar(PieceType type, PieceColor color)
    {
        return type switch
        {
            PieceType.Pawn => color == PieceColor.White ? 'P' : 'p',
            PieceType.Rook => color == PieceColor.White ? 'R' : 'r',
            PieceType.Knight => color == PieceColor.White ? 'N' : 'n',
            PieceType.Bishop => color == PieceColor.White ? 'B' : 'b',
            PieceType.Queen => color == PieceColor.White ? 'Q' : 'q',
            PieceType.King => color == PieceColor.White ? 'K' : 'k',
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}