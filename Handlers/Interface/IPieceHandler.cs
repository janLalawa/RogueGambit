using RogueGambit.Models.State.Interfaces;

namespace RogueGambit.Handlers.Interface;

public interface IPieceHandler
{
    Node2D CreateNodeForModel(INodeModel model);
    void LoadScenes();

    static Dictionary<Vector2, PieceModel> CreatePieceModelsDefault()
    {
        throw new NotImplementedException();
    }

    List<Piece> GetPieceNodes();

    void SetDefaultMoveSets();

    void PlaceSinglePiece(Vector2 boardPosition,
                          PieceType type,
                          PieceColor color,
                          PieceOwner owner = PieceOwner.Player,
                          MoveSet moveSet = null,
                          int rotation = 0,
                          bool hasMoved = false);
}