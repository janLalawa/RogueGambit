using RogueGambit.Models.State.Interfaces;

namespace RogueGambit.Handlers.Interface;

public interface IBoardHandler
{
    Node2D CreateNodeForModel(INodeModel model);
    void LoadScenes();

    static Dictionary<Vector2, BoardSquareModel> BuildBoardSquareModels(int boardStart, Vector2 boardShape, List<List<int>> boardMask = null)
    {
        throw new NotImplementedException();
    }

    List<BoardSquare> GetBoardSquareNodes();
}