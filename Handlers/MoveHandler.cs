using RogueGambit.Handlers.Interface;

namespace RogueGambit.Handlers;

public partial class MoveHandler : Node2D, IMoveHandler
{
    [Inject] private readonly IGameStateHandler _gameStateHandler = null!;
    public PieceModel SelectedPiece { get; private set; }

    public void SelectPiece(PieceModel piece)
    {
        SelectedPiece = piece;
        piece.Instance.SelectedSprite.Visible = true;
        GD.Print("Selected piece: " + piece);
    }

    public void DeselectPiece()
    {
        if (SelectedPiece == null) return;
        SelectedPiece.Instance.SelectedSprite.Visible = false;
        SelectedPiece = null;
        GD.Print("Deselected piece: " + SelectedPiece);
    }

    public void ToggleSelectedPiece(PieceModel piece)
    {
        if (SelectedPiece == null)
        {
            SelectPiece(piece);
        }
        else if (SelectedPiece == piece)
        {
            DeselectPiece();
        }
        else
        {
            DeselectPiece();
            SelectPiece(piece);
        }
    }

    public void MovePiece(PieceModel piece, Vector2 targetPosition)
    {
        var gameState = _gameStateHandler.GameState;

        gameState.Pieces.Remove(piece.GridPosition);
        gameState.Pieces.Add(targetPosition, piece);
        piece.GridPosition = targetPosition;
        piece.HasMoved = true;
    }

    public override void _Ready()
    {
        GD.Print("...MoveHandler ready.");
        Initialize();
    }

    public void Initialize()
    {
        InjectDependencies(this);
    }
}