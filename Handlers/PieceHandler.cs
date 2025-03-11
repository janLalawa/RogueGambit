using RogueGambit.Handlers.Factory;
using RogueGambit.Handlers.Interface;
using RogueGambit.Models.State.Interfaces;
using Piece = RogueGambit.Models.Piece;

namespace RogueGambit.Handlers;

public partial class PieceHandler : Node2D, INodeFactory, IPieceHandler
{
    [Inject] private readonly IGameStateHandler _gameStateHandler = null!;
    private PackedScene _pieceScene;

    public Node2D CreateNodeForModel(INodeModel model)
    {
        if (model is not PieceModel pieceModel)
        {
            GD.PrintErr("Piece handler can only place piece models.");
            return null;
        }

        if (_pieceScene is null)
        {
            GD.PrintErr("Piece scene could not be loaded.");
            return null;
        }

        if (_pieceScene.Instantiate() is not Piece piece)
        {
            GD.PrintErr("Piece could not be instantiated.");
            return null;
        }

        piece.PieceColor = pieceModel.Color;
        piece.PieceType = pieceModel.Type;
        piece.GridPosition = pieceModel.GridPosition;
        AddChild(piece);
        return piece;
    }

    public void LoadScenes()
    {
        _pieceScene = GD.Load<PackedScene>(PieceScenePath);
        GD.Print("...Loaded piece scene.");
    }

    public static Dictionary<Vector2, PieceModel> CreatePieceModelsDefault()
    {
        var pieceDictionary = new Dictionary<Vector2, PieceModel>();

        for (var x = 0; x < BoardSize; x++)
        {
            pieceDictionary.Add(new Vector2(x, 1), new PieceModel(new Vector2(x, 1), PieceColor.Black, PieceType.Pawn));
            pieceDictionary.Add(new Vector2(x, 6), new PieceModel(new Vector2(x, 6), PieceColor.White, PieceType.Pawn, 180));
        }

        pieceDictionary.Add(new Vector2(0, 0), new PieceModel(new Vector2(0, 0), PieceColor.Black, PieceType.Rook));
        pieceDictionary.Add(new Vector2(7, 0), new PieceModel(new Vector2(7, 0), PieceColor.Black, PieceType.Rook));
        pieceDictionary.Add(new Vector2(1, 0), new PieceModel(new Vector2(1, 0), PieceColor.Black, PieceType.Knight));
        pieceDictionary.Add(new Vector2(6, 0), new PieceModel(new Vector2(6, 0), PieceColor.Black, PieceType.Knight));
        pieceDictionary.Add(new Vector2(2, 0), new PieceModel(new Vector2(2, 0), PieceColor.Black, PieceType.Bishop));
        pieceDictionary.Add(new Vector2(5, 0), new PieceModel(new Vector2(5, 0), PieceColor.Black, PieceType.Bishop));
        pieceDictionary.Add(new Vector2(3, 0), new PieceModel(new Vector2(3, 0), PieceColor.Black, PieceType.Queen));
        pieceDictionary.Add(new Vector2(4, 0), new PieceModel(new Vector2(4, 0), PieceColor.Black, PieceType.King));

        pieceDictionary.Add(new Vector2(1, 7), new PieceModel(new Vector2(1, 7), PieceColor.White, PieceType.Knight, 180));
        pieceDictionary.Add(new Vector2(6, 7), new PieceModel(new Vector2(6, 7), PieceColor.White, PieceType.Knight, 180));
        pieceDictionary.Add(new Vector2(0, 7), new PieceModel(new Vector2(0, 7), PieceColor.White, PieceType.Rook, 180));
        pieceDictionary.Add(new Vector2(7, 7), new PieceModel(new Vector2(7, 7), PieceColor.White, PieceType.Rook, 180));
        pieceDictionary.Add(new Vector2(2, 7), new PieceModel(new Vector2(2, 7), PieceColor.White, PieceType.Bishop, 180));
        pieceDictionary.Add(new Vector2(5, 7), new PieceModel(new Vector2(5, 7), PieceColor.White, PieceType.Bishop, 180));
        pieceDictionary.Add(new Vector2(3, 7), new PieceModel(new Vector2(3, 7), PieceColor.White, PieceType.Queen, 180));
        pieceDictionary.Add(new Vector2(4, 7), new PieceModel(new Vector2(4, 7), PieceColor.White, PieceType.King, 180));

        return pieceDictionary;
    }

    public List<Piece> GetPieceNodes()
    {
        var pieces = new List<Piece>();

        foreach (var child in GetChildren())
            if (child is Piece piece)
                pieces.Add(piece);

        return pieces;
    }

    public void SetDefaultMoveSets()
    {
        foreach (var pieceModel in _gameStateHandler.GameState.Pieces.Values) pieceModel.SetDefaultMoveSet();
    }

    public override void _Ready()
    {
        GD.Print("...PieceHandler ready.");
        Initialize();
    }

    private void Initialize()
    {
        InjectDependencies(this);
    }

    private void PlacePieceNode(Vector2 boardPosition, PieceColor color, PieceType type)
    {
        var pieceInstance = _pieceScene.Instantiate() as Piece;
        if (pieceInstance is null)
        {
            GD.PrintErr("Failed to instantiate piece.");
            return;
        }

        pieceInstance.PieceColor = color;
        pieceInstance.PieceType = type;
        pieceInstance.GridPosition = boardPosition;
        AddChild(pieceInstance);
    }
}