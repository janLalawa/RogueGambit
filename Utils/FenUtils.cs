using System.Text;

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

    public static string GameStateToFen(GameState gamestate)
    {
        var fenBuilder = new StringBuilder();

        var rankGroups = gamestate.BoardSquares.Keys
                                  .GroupBy(pos => pos.Y)
                                  .OrderBy(group => group.Key)
                                  .ToList();

        for (var rankIndex = 0; rankIndex < rankGroups.Count; rankIndex++)
        {
            var rank = rankGroups[rankIndex];

            var sortedSquares = rank.OrderBy(pos => pos.X).ToList();

            var emptyCount = 0;
            var lastX = -1;

            foreach (var pos in sortedSquares)
            {
                var currentX = (int)pos.X;

                if (lastX >= 0 && currentX > lastX + 1)
                {
                    var gap = currentX - lastX - 1;
                    emptyCount += gap;
                }

                if (gamestate.Pieces.TryGetValue(pos, out var piece))
                {
                    if (emptyCount > 0)
                    {
                        fenBuilder.Append(emptyCount);
                        emptyCount = 0;
                    }

                    fenBuilder.Append(piece.FenChar);
                }
                else
                {
                    emptyCount++;
                }

                lastX = currentX;
            }

            if (emptyCount > 0) fenBuilder.Append(emptyCount);

            if (rankIndex < rankGroups.Count - 1) fenBuilder.Append('/');
        }

        // 2. Active color
        fenBuilder.Append(' ');
        fenBuilder.Append(gamestate.CurrentTurn == PieceOwner.Player ? 'w' : 'b');

        // 3. Castling availability
        fenBuilder.Append(" - ");

        // 4. En passant target square
        fenBuilder.Append("- ");

        // 5. Halfmove clock
        fenBuilder.Append("0 ");

        // 6. Fullmove number
        fenBuilder.Append(gamestate.TurnNumber);

        GD.Print(fenBuilder.ToString());
        return fenBuilder.ToString();
    }
}