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

    public PieceModel GetPieceAtPosition(Vector2 position)
    {
        return Pieces.GetValueOrDefault(position);
    }
}