using System.IO;
using QuickType;
using RogueGambit.Handlers.Interface;
using RogueGambit.Utils;
using FileAccess = Godot.FileAccess;

namespace RogueGambit.Handlers;

public partial class SaveHandler : Node2D, ISaveHandler
{
    [Inject] private readonly IGameStateHandler _gameStateHandler = null!;
    [Inject] private readonly IInputHandler _inputHandler = null!;
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
                    Board = SaveBoard(gameState.BoardShape, gameState.BoardMask),
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

    public bool SaveGameToFile(string filePath = "user://SaveData.json")
    {
        try
        {
            var json = _saveState.ToJson();

            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                var error = FileAccess.GetOpenError();
                GD.PrintErr($"Could not open file for writing: {error}");
                return false;
            }

            file.StoreString(json);
            file.Close();

            GD.Print($"Successfully saved game to {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving game state to file: {e.Message}");
            return false;
        }

        return true;
    }


    public string SaveGame(string[] args)
    {
        var filePath = "user://SaveData.json"; // Default

        if (args.Length > 0) filePath = args[0];

        var stateSaved = SaveCurrentGameState(_gameStateHandler.GameState);
        if (!stateSaved) return "Failed to save game state.";

        var actualPath = filePath;
        if (filePath.StartsWith("user://"))
        {
            var userDir = OS.GetUserDataDir();
            var relativePath = filePath.Substring(7); // Remove "user://"
            actualPath = Path.Combine(userDir, relativePath);
        }

        var fileSaved = SaveGameToFile(filePath);
        return fileSaved ? $"Game successfully saved to {actualPath}" : $"Failed to save game to {actualPath}";
    }


    public string LoadGame(string[] args)
    {
        var filePath = "user://SaveData.json"; // Default
        if (args.Length > 0) filePath = args[0];
        var fileLoaded = LoadGameFromFile(filePath);
        if (!fileLoaded) return "Failed to load game state.";
        var gameRestored = LoadGameStateToCurrentGame();
        _inputHandler.ConnectSignals();

        return gameRestored ? $"Game successfully restored from {filePath}" : $"Failed to restore game from {filePath}";
    }

    public override void _Ready()
    {
        GD.Print("...SaveHandler ready.");
        Initialize();
    }

    private void Initialize()
    {
        InjectDependencies(this);
    }


    private static SavedBoard SaveBoard(Vector2 boardShape, List<List<int>> boardMask)
    {
        var savedBoard = new SavedBoard();

        savedBoard.BoardShape = new SavedVector
        {
            X = (int)boardShape.X,
            Y = (int)boardShape.Y
        };

        if (boardMask == null) return savedBoard;
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

    private static List<SavedPiece> SavePieces(Dictionary<Vector2, PieceModel> pieces)
    {
        return pieces.Values.Select(BuildSavePiece).ToList();
    }

    private static List<SavedGraveyard> SaveGraveyards(List<PieceModel> graveyard)
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

    private static List<SavedPlayer> SavePlayers(List<PlayerModel> players)
    {
        if (players == null) return new List<SavedPlayer>();

        return players.Select(player => new SavedPlayer
        {
            Name = player.Name,
            Color = player.Color.ToString(),
            Controller = player.Controller.ToString()
        }).ToList();
    }

    public bool LoadGameFromFile(string filePath = "user://SaveData.json")
    {
        try
        {
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                var error = FileAccess.GetOpenError();
                GD.PrintErr($"Could not open file for reading: {error}");
                return false;
            }

            var json = file.GetAsText();
            file.Close();

            _saveState = SaveState.FromJson(json);
            GD.Print($"Successfully loaded game from {filePath}");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading game state from file: {e.Message}");
            return false;
        }
    }

    public bool LoadGameStateToCurrentGame()
    {
        if (_saveState?.GameSaveState == null)
        {
            GD.PrintErr("No save state loaded to restore from");
            return false;
        }

        try
        {
            _gameStateHandler.GameState.RemoveAll(true);

            LoadBoard();
            LoadPieces();
            LoadPlayers();

            _gameStateHandler.SetTurn(Enum.Parse<PieceOwner>(_saveState.GameSaveState.CurrentTurn));
            _gameStateHandler.GameState.TurnNumber = _saveState.GameSaveState.TurnNumber;
            _gameStateHandler.UpdateGameState();

            GD.Print("Game state successfully restored");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error restoring game state: {e.Message}");
            return false;
        }
    }

    private void LoadBoard()
    {
        var boardStart = BoardStartX;
        var boardShape = new Vector2(_saveState.GameSaveState.Board.BoardShape.X, _saveState.GameSaveState.Board.BoardShape.Y);
        var maskPositions = VectorUtils.CreateFromSavedVectors(_saveState.GameSaveState.Board.MaskedSquares);
        var boardMask = VectorUtils.BuildBoardMask(boardShape, maskPositions);

        _gameStateHandler.PlaceBoard(boardStart, boardShape, boardMask);
    }

    private void LoadPieces()
    {
        foreach (var savedPiece in _saveState.GameSaveState.Pieces)
        {
            var gridpos = new Vector2(savedPiece.GridPos.X, savedPiece.GridPos.Y);
            var pieceType = Enum.Parse<PieceType>(savedPiece.Type);
            var pieceColor = Enum.Parse<PieceColor>(savedPiece.Color);
            var pieceOwner = Enum.Parse<PieceOwner>(savedPiece.Owner);
            var moveSet = LoadMoveSet(savedPiece.SavedMoveSet.Moves);
            var rotation = savedPiece.Rotation;
            var hasMoved = savedPiece.HasMoved;

            _gameStateHandler.PlacePiece(gridpos, pieceType, pieceColor, pieceOwner, moveSet, rotation, hasMoved);
        }
    }

    private static MoveSet LoadMoveSet(List<SavedMove> savedMoves)
    {
        var moveSet = new MoveSet();
        foreach (var savedMove in savedMoves)
        {
            var direction = new Vector2(savedMove.Direction.X, savedMove.Direction.Y);
            var range = savedMove.Range;
            var chain = (savedMove.SavedChain.ChainNumber, savedMove.SavedChain.ChainPos);
            var attributes = savedMove.Attributes.Select(Enum.Parse<MoveAttrib>).ToList();
            moveSet.AddMove(direction, range, chain, [..attributes]);
        }

        return moveSet;
    }

    private void LoadPlayers()
    {
        foreach (var player in _saveState.GameSaveState.Players.Select(savedPlayer => new PlayerModel
                 {
                     Name = savedPlayer.Name,
                     Color = Enum.Parse<PieceColor>(savedPlayer.Color),
                     Controller = Enum.Parse<PieceOwner>(savedPlayer.Controller)
                 }))
        {
            _gameStateHandler.GameState.Players.Add(player);
            _gameStateHandler.AssignColorToOwner(player.Color, player.Controller);
        }
    }
}