using System.Text;
using RogueGambit.Handlers;
using BoardHandler = RogueGambit.Handlers.BoardHandler;

namespace RogueGambit.Models.State;

public class GameState
{
    public GameState()
    {
        BoardSquares = new Dictionary<Vector2, BoardSquareModel>();
        Pieces = new Dictionary<Vector2, PieceModel>();
        PlayerStatus = SelectingPiece;
        GD.Print("...BoardState object ready.");
    }

    public Dictionary<Vector2, BoardSquareModel> BoardSquares { get; set; }
    public Dictionary<Vector2, PieceModel> Pieces { get; set; }
    public List<PieceModel> Graveyard { get; set; } = new();
    public Vector2 BoardShape { get; set; }
    public List<List<int>> BoardMask { get; set; }
    public PieceOwner CurrentTurn { get; set; }
    public PlayerStatus PlayerStatus { get; set; }
    public List<PlayerModel> Players { get; set; }
    public int TurnNumber { get; set; } = 1;

    public void ReadGameStateFromNodes(BoardHandler boardHandler, PieceHandler pieceHandler)
    {
        var boardSquares = boardHandler.GetBoardSquareNodes();
        var pieces = pieceHandler.GetPieceNodes();

        foreach (var square in boardSquares) BoardSquares.Add(square.GridPosition, new BoardSquareModel(square));
        foreach (var piece in pieces) Pieces.Add(piece.GridPosition, new PieceModel(piece));
    }

    public string ToFen()
    {
        var fenBuilder = new StringBuilder();

        var rankGroups = BoardSquares.Keys
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

                if (Pieces.TryGetValue(pos, out var piece))
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
        fenBuilder.Append(CurrentTurn == PieceOwner.Player ? 'w' : 'b');

        // 3. Castling availability
        fenBuilder.Append(" - ");

        // 4. En passant target square
        fenBuilder.Append("- ");

        // 5. Halfmove clock
        fenBuilder.Append("0 ");

        // 6. Fullmove number
        fenBuilder.Append(TurnNumber);

        GD.Print(fenBuilder.ToString());
        return fenBuilder.ToString();
    }

    public void FindOccupiedSquares()
    {
        foreach (var square in BoardSquares.Values)
        {
            square.IsOccupied = Pieces.ContainsKey(square.GridPosition);
            square.UpdateNode();
        }
    }

    public void UpdateBoardNodes()
    {
        foreach (var square in BoardSquares.Values) square.UpdateNode();
    }

    public void UpdatePieceNodes()
    {
        foreach (var piece in Pieces.Values) piece.UpdateNode();
    }

    public void RemoveAll(bool instant = false, bool pieces = true, bool board = true, bool graveyard = true, bool players = true)
    {
        if (pieces)
        {
            foreach (var piece in Pieces.Values) piece.DestroyNode(instant);
            Pieces.Clear();
        }

        if (board)
        {
            foreach (var square in BoardSquares.Values) square.DestroyNode(instant);
            BoardSquares.Clear();
            BoardShape = Vector2.Zero;
            BoardMask = null;
        }

        if (graveyard) Graveyard.Clear();

        if (players)
        {
            Players?.Clear();
            CurrentTurn = PieceOwner.Player;
        }

        if (pieces && board)
        {
            PlayerStatus = SelectingPiece;
            TurnNumber = 1;
        }

        if (pieces && board && graveyard && players)
        {
            BoardSquares = new Dictionary<Vector2, BoardSquareModel>();
            Pieces = new Dictionary<Vector2, PieceModel>();
            Graveyard = new List<PieceModel>();
            Players = new List<PlayerModel>();
            GD.Print("GameState reset.");
        }
    }
    
    public PieceModel GetPieceAtPosition(Vector2 position)
    {
        return Pieces.GetValueOrDefault(position);
    }
}