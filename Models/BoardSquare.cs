namespace RogueGambit.Models;

public partial class BoardSquare : Node2D
{
	[Signal]
	public delegate void SquareClickedEventHandler(BoardSquare square);

	[Signal]
	public delegate void SquareMouseEnteredEventHandler(BoardSquare square);

	[Signal]
	public delegate void SquareMouseExitedEventHandler(BoardSquare square);

	private Vector2 _gridPos;
	private Area2D _squareArea2D;
	private CollisionShape2D _squareCollision2D;
	private ColorRect _squareColorRect;

	[Export] public Color SquareColor { get; set; }
	[Export] public Vector2 SquareSize { get; set; } = SquareSizeVector;

	[Export]
	public Vector2 GridPosition
	{
		get => _gridPos;
		set
		{
			_gridPos = value;
			Position = value * SquareSize;
		}
	}

	public BoardSquareModel BoardSquareModel { get; set; }

	[Export] public bool IsOccupied { get; set; }
	[Export] public Sprite2D OverlaySprite { get; set; }
	[Export] public Sprite2D TargetSprite { get; set; }

	public override void _Ready()
	{
		InitializeNodes();
		SetSquareName();
		ConnectSignals();
		UpdateColorRect();
	}

	private void SetSquareName()
	{
		Name = $"Square_{GridPosition.ToString()}";
	}

	private void InitializeNodes()
	{
		_squareArea2D = GetNode<Area2D>("boardSquareArea2D");
		_squareCollision2D = GetNode<CollisionShape2D>("boardSquareArea2D/boardSquareCollision2D");
		_squareColorRect = GetNode<ColorRect>("boardSquareColorRect");
		OverlaySprite = GetNode<Sprite2D>("boardSquareColorRect/boardSquareOverlay2D");
		OverlaySprite.Visible = false;
		TargetSprite = GetNode<Sprite2D>("boardSquareColorRect/boardSquareOverlayTargets2D");
		TargetSprite.Visible = false;
	}

	private void ConnectSignals()
	{
		_squareArea2D.Connect("input_event", new Callable(this, nameof(OnAreaInputEvent)));
		_squareArea2D.Connect("mouse_entered", new Callable(this, nameof(OnMouseEntered)));
		_squareArea2D.Connect("mouse_exited", new Callable(this, nameof(OnMouseExited)));
	}

	private void OnAreaInputEvent(Node viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left &&
			mouseEvent.IsPressed())
			EmitSignal(nameof(SquareClicked), this);
	}

	private void OnMouseEntered()
	{
		EmitSignal(nameof(SquareMouseEntered), this);
	}

	private void OnMouseExited()
	{
		EmitSignal(nameof(SquareMouseExited), this);
	}

	private void UpdateColorRect()
	{
		if (_squareColorRect == null)
		{
			GD.PrintErr("Square color rect not found.");
			return;
		}

		_squareColorRect.Color = SquareColor;
		_squareColorRect.Size = SquareSize;
	}

	public void UpdateColor(Color color)
	{
		if (_squareColorRect != null) _squareColorRect.Color = color;
	}
}
