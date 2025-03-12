using System.IO;
using QuickType;
using RogueGambit.Handlers.Interface;

namespace RogueGambit.Handlers;

public class SaveHandler : ISaveHandler
{
    private SaveState _saveState;

    public bool SaveCurrentGameState(GameState gameState)
    {
        try
        {
            _saveState = new SaveState
            {
                GameSaveState = new GameSaveState
                {
                    CurrentTurn = gameState.CurrentTurn.ToString(),
                    TurnNumber = gameState.TurnNumber,
                    Pieces = SavePieces(gameState.Pieces),
                    Graveyard = SaveGraveyards(gameState.Graveyard),
                    Board = SaveBoard(gameState.BoardSquares, gameState.BoardShape, gameState.BoardMask),
                    Players = SavePlayers(gameState.Players)
                }
            };
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving game state internally: {e.Message}");
            return false;
        }

        return true;
    }

    public bool SaveGameToFile(string filePath = "../Data/SaveData.json")
    {
        try
        {
            var json = _saveState.ToJson();
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving game state to file: {e.Message}");
            return false;
        }

        return true;
    }


    private SavedBoard SaveBoard(Dictionary<Vector2, BoardSquareModel> boardSquares, Vector2 boardShape, List<List<int>> boardMask)
    {
        var savedBoard = new SavedBoard();

        savedBoard.BoardShape = new SavedVector
        {
            X = (int)boardShape.X,
            Y = (int)boardShape.Y
        };

        savedBoard.MaskedSquares = new List<SavedVector>();
        for (var y = 0; y < boardMask.Count; y++)
        for (var x = 0; x < boardMask[y].Count; x++)
            if (boardMask[y][x] == 0)
                savedBoard.MaskedSquares.Add(new SavedVector
                {
                    X = x,
                    Y = y
                });

        return savedBoard;
    }

    private List<SavedPiece> SavePieces(Dictionary<Vector2, PieceModel> pieces)
    {
        return pieces.Values.Select(BuildSavePiece).ToList();
    }

    private List<SavedGraveyard> SaveGraveyards(List<PieceModel> graveyard)
    {
        var savedGraveyards = new List<SavedGraveyard>();
        var savedGraveyard = new SavedGraveyard
        {
            Pieces = graveyard.Select(BuildSavePiece).ToList()
        };
        savedGraveyards.Add(savedGraveyard);
        return savedGraveyards;
    }

    private static SavedPiece BuildSavePiece(PieceModel piece)
    {
        var savedPiece = new SavedPiece
        {
            Type = piece.Type.ToString(),
            Color = piece.Color.ToString(),
            Owner = piece.Owner.ToString(),
            Rotation = piece.Rotation,
            HasMoved = piece.HasMoved,
            GridPos = new SavedVector
            {
                X = (int)piece.GridPosition.X,
                Y = (int)piece.GridPosition.Y
            }
        };

        var savedMoveSet = new SavedMoveSet
        {
            LoadMoveSet = piece.MoveSet.Name,
            Moves = []
        };

        foreach (var savedMove in piece.MoveSet.Moves.Select(move => new SavedMove
                 {
                     Direction = new SavedVector
                     {
                         X = (int)move.Direction.X,
                         Y = (int)move.Direction.Y
                     },
                     Range = move.Range,
                     SavedChain = new SavedChain
                     {
                         ChainNumber = move.Chain.Item1,
                         ChainPos = move.Chain.Item2
                     },
                     Attributes = move.Attributes.Select(attrib => attrib.ToString()).ToList()
                 }))
            savedMoveSet.Moves.Add(savedMove);

        savedPiece.SavedMoveSet = savedMoveSet;

        return savedPiece;
    }

    private List<SavedPlayer> SavePlayers(List<PlayerModel> players)
    {
        return players.Select(player => new SavedPlayer
        {
            Name = player.Name,
            Color = player.Color.ToString(),
            Controller = player.Controller.ToString()
        }).ToList();
    }
}