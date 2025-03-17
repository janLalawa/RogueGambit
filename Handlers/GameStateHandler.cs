namespace RogueGambit.Handlers;

public partial class GameStateHandler : Node, IGameStateHandler
{
    [Inject] private readonly IAiHandler _aiHandler = null!;
    [Inject] private readonly IBoardHandler _boardHandler = null!;
    [Inject] private readonly IDebugConsole _debugConsole = null!;
    [Inject] private readonly IInputHandler _inputHandler = null!;
    [Inject] private readonly IMoveHandler _moveHandler = null!;
    [Inject] private readonly IMoveLogic _moveLogic = null!;
    [Inject] private readonly IPieceHandler _pieceHandler = null!;
    [Inject] private readonly ISaveHandler _saveHandler = null!;
    [Inject] private readonly ITurnHandler _turnHandler = null!;

    public PlayerStatus PlayerStatus
    {
        get => GameState.PlayerStatus;
        set => GameState.PlayerStatus = value;
    }

    public GameState GameState { get; set; }

    public void InitializeGameState()
    {
        GameState = new GameState();
    }

    public void LoadScenes()
    {
        _boardHandler.LoadScenes();
        _pieceHandler.LoadScenes();
    }

    public void PlaceBoard(int boardStart, Vector2 boardShape, List<List<int>> boardMask = null)
    {
        GameState.BoardSquares = BoardHandler.BuildBoardSquareModels(boardStart, boardShape, boardMask);
        GameState.BoardSquares.Values.ToList().ForEach(square => square.UpdateNode(true));
        GameState.BoardShape = boardShape;
        GameState.BoardMask = boardMask;
    }

    public void PlacePieces()
    {
        GameState.Pieces = PieceHandler.CreatePieceModelsDefault();
        GameState.Pieces.Values.ToList().ForEach(piece => piece.UpdateNode(true));
    }

    public void UpdateGameState()
    {
        GameState.FindOccupiedSquares();
        GameState.UpdateBoardNodes();
        GameState.UpdatePieceNodes();
    }

    public void AssignColorToOwner(PieceColor color, PieceOwner owner)
    {
        foreach (var piece in GameState.Pieces.Values.Where(piece => piece.Color == color)) piece.Owner = owner;
    }

    public void SetTurn(PieceOwner owner)
    {
        GameState.CurrentTurn = owner;
        _turnHandler.SetTurn(owner);
    }

    public void MovePiece(PieceModel piece, Vector2 targetPosition)
    {
        _moveHandler.MovePiece(piece, targetPosition);
        DeselectPiece();
        UpdateGameState();
        AdvanceTurn();
    }

    public void ToggleSelectedPiece(PieceModel piece)
    {
        _moveHandler.ToggleSelectedPiece(piece);
    }

    public void CapturePiece(PieceModel attacker, PieceModel targetPiece)
    {
        GameState.Graveyard.Add(targetPiece);
        GameState.Pieces.Remove(targetPiece.GridPosition);
        targetPiece.Instance.QueueFree();
        MovePiece(attacker, targetPiece.GridPosition);
    }

    public void SelectPiece(PieceModel piece)
    {
        _moveHandler.SelectPiece(piece);
        piece.ValidMoves = _moveLogic.GetValidMoves(piece);
        foreach (var move in piece.ValidMoves)
        {
            var square = GameState.BoardSquares[move];
            square.Instance.TargetSprite.Visible = true;
        }
    }

    public void PlacePiece(Vector2 gridPos,
                           PieceType type,
                           PieceColor color,
                           PieceOwner owner,
                           MoveSet moveSet,
                           int rotation = 0,
                           bool hasMoved = false)
    {
        _pieceHandler.PlaceSinglePiece(gridPos, type, color, owner, moveSet, rotation, hasMoved);
    }

    public void DeselectPiece()
    {
        _moveHandler.DeselectPiece();
        foreach (var square in GameState.BoardSquares.Values) square.Instance.TargetSprite.Visible = false;
    }

    public void PromotePiece(PieceModel piece, PieceType newType)
    {
        throw new NotImplementedException();
    }


    public override void _Ready()
    {
        GD.Print("...GameStateHandler ready.");
        InjectDependencies(this);
        InitializeGameState();
        LoadScenes();

        PlaceBoard(BoardStartX, new Vector2(8, 8));
        PlacePieces();
        _pieceHandler.SetDefaultMoveSets();
        _inputHandler.Initialize();
        _moveLogic.Initialize();

        AssignColorToOwner(PieceColor.White, PieceOwner.Player);
        AssignColorToOwner(PieceColor.Black, PieceOwner.Ai);

        UpdateGameState();
        SetTurn(PieceOwner.Player);

        RegisterCommands();
    }


    public void AdvanceTurn()
    {
        _turnHandler.AdvanceTurn();
        GameState.TurnNumber++;
        MakeAiMove();
    }

    public void MakeAiMove()
    {
        if (GameState.CurrentTurn != PieceOwner.Ai) return;
        var myMove = _aiHandler.Brain.GetBestMove(GameState).Result;
        GD.Print(myMove);
        if (myMove.IsCapture)
            CapturePiece(myMove.Piece, GameState.GetPieceAtPosition(myMove.EndPosition));
        else
            MovePiece(myMove.Piece, myMove.EndPosition);
    }

    private void RegisterCommands()
    {
        _debugConsole.RegisterCommand("savegame", args => _saveHandler.SaveGame(args));

        _debugConsole.RegisterCommand("loadgame", args => _saveHandler.LoadGame(args));

        _debugConsole.RegisterCommand("clearall", args =>
        {
            GameState.RemoveAll();
            UpdateGameState();
            return "GameState cleared.";
        });

        _debugConsole.RegisterCommand("defaultboard", args =>
        {
            GameState.RemoveAll();
            _Ready();
            return "Default board placed.";
        });

        _debugConsole.RegisterCommand("to_fen", args => GameState.ToFen());
    }
}