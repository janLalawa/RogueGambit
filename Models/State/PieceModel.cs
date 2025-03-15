using RogueGambit.Handlers.Factory;
using RogueGambit.Models.State.Interfaces;
using RogueGambit.Utils;
using static RogueGambit.Utils.BuildMoveSets;

namespace RogueGambit.Models.State;

public class PieceModel : INodeModel
{
    private readonly INodeFactory _nodeFactory;

    public PieceModel(Piece piece)
    {
        GridPosition = piece.GridPosition;
        Color = piece.PieceColor;
        Type = piece.PieceType;
        Instance = piece;
        _nodeFactory = GetNodeFactory<Piece>();
    }

    public PieceModel(Vector2 gridPosition, PieceColor color, PieceType type, int rotation = 0)
    {
        GridPosition = gridPosition;
        Color = color;
        Type = type;
        Rotation = rotation;
        _nodeFactory = GetNodeFactory<Piece>();
    }

    public Vector2 GridPosition { get; set; }
    public PieceColor Color { get; set; }
    public PieceType Type { get; set; }
    public char FenChar => GetFenChar();
    public char FenOverwrite { get; set; }
    public Piece Instance { get; set; }
    public PieceOwner Owner { get; set; } = PieceOwner.None;
    public int Atk { get; set; } = 1;
    public int Def { get; set; } = 1;
    public MoveSet MoveSet { get; set; }
    public int Rotation { get; set; }
    public Vector2 StartPosition { get; set; }
    public bool HasMoved { get; set; }
    public HashSet<Vector2> ValidMoves { get; set; }

    public void UpdateNode(bool create = false)
    {
        if (create || Instance is null) Instance = (Piece)_nodeFactory.CreateNodeForModel(this);

        Instance.GridPosition = GridPosition;
        Instance.PieceType = Type;
        Instance.PieceColor = Color;
        Instance.PieceModel = this;
    }

    public void ReadNode()
    {
        if (Instance is null) return;
        GridPosition = Instance.GridPosition;
        Type = Instance.PieceType;
        Color = Instance.PieceColor;
    }

    public void DestroyNode(bool instant = false)
    {
        if (instant)
            Instance?.Free();
        else
            Instance?.QueueFree();
    }

    public char GetFenChar()
    {
        return FenOverwrite != default ? FenOverwrite : FenUtils.GetFenChar(Type, Color);
    }

    public override string ToString()
    {
        return $"{Owner}'s {Color} {Type} at {GridPosition}";
    }

    public void SetMoveSet(MoveSet moveSet)
    {
        MoveSet = moveSet;
    }

    public void SetDefaultMoveSet()
    {
        var moveSets = GetDefault();
        MoveSet = moveSets.TryGetValue(Type, out var moveSet) ? moveSet.Clone() : throw new Exception($"MoveSet for {Type} not found");
    }
}