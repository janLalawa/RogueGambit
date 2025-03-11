using RogueGambit.Handlers.Factory;
using RogueGambit.Models.State.Interfaces;

namespace RogueGambit.Models.State;

public class BoardSquareModel : INodeModel
{
    private readonly INodeFactory _nodeFactory;

    public BoardSquareModel(BoardSquare boardSquare)
    {
        GridPosition = boardSquare.GridPosition;
        SquareColor = boardSquare.SquareColor;
        IsOccupied = boardSquare.IsOccupied;
        Instance = boardSquare;
        _nodeFactory = GetNodeFactory<BoardSquare>();
    }

    public BoardSquareModel(Vector2 gridPosition, Color squareColor, bool isOccupied)
    {
        GridPosition = gridPosition;
        SquareColor = squareColor;
        IsOccupied = isOccupied;
        _nodeFactory = GetNodeFactory<BoardSquare>();
    }

    public Vector2 GridPosition { get; set; }
    public Color SquareColor { get; set; }
    public bool IsOccupied { get; set; }
    public BoardSquare Instance { get; set; }

    public void UpdateNode(bool create = false)
    {
        if (create || Instance is null) Instance = (BoardSquare)_nodeFactory.CreateNodeForModel(this);

        Instance.GridPosition = GridPosition;
        Instance.SquareColor = SquareColor;
        Instance.IsOccupied = IsOccupied;
        Instance.UpdateColor(SquareColor);
        Instance.BoardSquareModel = this;
    }

    public void ReadNode()
    {
        if (Instance is null) return;
        GridPosition = Instance.GridPosition;
        SquareColor = Instance.SquareColor;
        IsOccupied = Instance.IsOccupied;
    }

    public void DestroyNode()
    {
        Instance?.QueueFree();
    }
}