using RogueGambit.Handlers.Interface;

namespace RogueGambit.Handlers;

public partial class TurnHandler : Node2D, ITurnHandler
{
    [Inject] private readonly IGameStateHandler _gameStateHandler = null!;
    private Sprite2D _turnSprite;
    private Dictionary<PieceOwner, string> TextureMap { get; set; }

    public void UpdateTurn()
    {
        UpdateTurnSprite();
    }

    public void AdvanceTurn()
    {
        _gameStateHandler.GameState.CurrentTurn = _gameStateHandler.GameState.CurrentTurn == PieceOwner.Player
            ? PieceOwner.Ai
            : PieceOwner.Player;
        UpdateTurn();
    }

    public void SetTurn(PieceOwner owner)
    {
        _gameStateHandler.GameState.CurrentTurn = owner;
        UpdateTurn();
    }

    public void UpdateTurnSprite()
    {
        _turnSprite.Texture = GD.Load<Texture2D>(TextureMap[_gameStateHandler.GameState.CurrentTurn]);
        _turnSprite.Visible = true;
    }

    public void BuildTextureMap()
    {
        TextureMap = new Dictionary<PieceOwner, string>
        {
            { PieceOwner.Player, "res://Assets/UI/turn-white.png" },
            { PieceOwner.Ai, "res://Assets/UI/turn-black.png" }
        };
    }

    public override void _Ready()
    {
        GD.Print("...TurnHandler ready.");
        Initialize();
    }

    public void Initialize()
    {
        InjectDependencies(this);
        InitializeNodes();
        _turnSprite.Position = new Vector2(50 + BoardEndX, BoardEndY / 2);
        _turnSprite.Visible = false;
        BuildTextureMap();
    }

    private void InitializeNodes()
    {
        _turnSprite = GetNode<Sprite2D>("TurnSprite2D");
    }
}