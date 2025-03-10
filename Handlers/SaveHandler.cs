using QuickType;
using RogueGambit.Handlers.Interface;

namespace RogueGambit.Handlers;

public class SaveHandler : ISaveHandler
{
    private SaveState _saveState;

    public void SaveCurrentGameState(GameState gameState)
    {
        _saveState = new SaveState();
    }

    private SavedBoard SaveBoard(Dictionary<Vector2, BoardSquareModel> boardSquares, Vector2 boardShape, List<List<int>> boardMask)
    {
        var savedBoard = new SavedBoard();

        savedBoard.BoardShape = new SavedVector
        {
            X = (int)boardShape.X,
            Y = (int)boardShape.Y
        };

        return savedBoard; // TODO: Implement this method
    }
}