namespace RogueGambit.Handlers.Interface;

public interface IGameStateHandler
{
    GameState GameState { get; set; }
    PlayerStatus PlayerStatus { get; set; }
    void PlacePiece(Vector2 gridPos, PieceType type, PieceColor color, PieceOwner owner, MoveSet moveSet, int rotation = 0, bool hasMoved = false);
    void SelectPiece(PieceModel piece);
    void InitializeGameState();
    void LoadScenes();
    void PlaceBoard(int boardStart, Vector2 boardShape, List<List<int>> boardMask = null);
    void PlacePieces();
    void AssignColorToOwner(PieceColor color, PieceOwner owner);
    void UpdateGameState();
    void SetTurn(PieceOwner owner);
    void ToggleSelectedPiece(PieceModel piece);
    void MovePiece(PieceModel piece, Vector2 targetPosition);
    void CapturePiece(PieceModel attacker, PieceModel targetPiece);
}