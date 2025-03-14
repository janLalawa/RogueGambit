using QuickType;

namespace RogueGambit.Utils;

public static class VectorUtils
{
    public static Vector2 GetGridSize(List<Vector2> gridPositions)
    {
        var maxX = gridPositions.Max(v => v.X);
        var maxY = gridPositions.Max(v => v.Y);
        return new Vector2(maxX, maxY);
    }

    public static List<Vector2> GetGridMasks(Vector2 gridSize, List<Vector2> gridPositions)
    {
        var missingVectors = new List<Vector2>();
        for (var x = 0; x < gridSize.X; x++)
        for (var y = 0; y < gridSize.Y; y++)
        {
            var gridPosition = new Vector2(x, y);
            if (!gridPositions.Contains(gridPosition))
                missingVectors.Add(gridPosition);
        }

        return missingVectors;
    }

    public static List<List<int>> BuildBoardMask(Vector2 boardShape, HashSet<Vector2> maskPositions = null)
    {
        var rows = (int)boardShape.Y;
        var cols = (int)boardShape.X;
        var boardMask = new List<List<int>>();

        for (var i = 0; i < rows; i++) boardMask.Add(new List<int>(new int[cols].Select(_ => 1)));

        if (maskPositions is null) return boardMask;

        foreach (var maskPosition in maskPositions)
        {
            var row = (int)maskPosition.Y;
            var col = (int)maskPosition.X;
            boardMask[row][col] = 0;
        }

        return boardMask;
    }

    public static HashSet<Vector2> CreateFromSavedVectors(List<SavedVector> savedVectors)
    {
        if (savedVectors is null) return null;

        var vectors = new HashSet<Vector2>();
        foreach (var savedVector in savedVectors)
            vectors.Add(new Vector2(savedVector.X, savedVector.Y));

        return vectors;
    }
}