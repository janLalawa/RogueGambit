using RogueGambit.Handlers.Factory;
using RogueGambit.Handlers.Interface;
using RogueGambit.Models.State.Interfaces;

namespace RogueGambit.Handlers;

public partial class BoardHandler : Node2D, INodeFactory, IBoardHandler
{
	private PackedScene _boardSquareScene;

	public void LoadScenes()
	{
		_boardSquareScene = GD.Load<PackedScene>(BoardSquareScenePath);
		GD.Print("...Loaded board square scene.");
	}

	public static Dictionary<Vector2, BoardSquareModel> BuildBoardSquareModels(int boardStart, Vector2 boardShape, List<List<int>> boardMask = null)
	{
		var boardSquares = new Dictionary<Vector2, BoardSquareModel>();

		for (var x = boardStart; x < boardShape.X; x++)
		for (var y = boardStart; y < boardShape.Y; y++)
		{
			var boardPosition = new Vector2(x, y);
			var color = (x + y) % 2 == 0 ? LightSquareColor : DarkSquareColor;

			if (boardMask is not null && boardMask[(int)boardPosition.Y][(int)boardPosition.X] == 0)
				continue;

			var boardSquare = new BoardSquareModel(boardPosition, color, false);

			boardSquares.Add(boardPosition, boardSquare);
		}

		return boardSquares;
	}

	public List<BoardSquare> GetBoardSquareNodes()
	{
		var boardSquares = new List<BoardSquare>();
		foreach (var child in GetChildren())
			if (child is BoardSquare square)
				boardSquares.Add(square);

		return boardSquares;
	}

	public Node2D CreateNodeForModel(INodeModel model)
	{
		if (model is not BoardSquareModel boardSquareModel)
		{
			GD.PrintErr("Board handler can only place board square models.");
			return null;
		}

		if (_boardSquareScene is null)
		{
			GD.PrintErr("Board square scene could not be loaded.");
			return null;
		}

		if (_boardSquareScene.Instantiate() is not BoardSquare boardSquare)
		{
			GD.PrintErr("Board square could not be instantiated.");
			return null;
		}

		boardSquare.SquareColor = boardSquareModel.SquareColor;
		boardSquare.GridPosition = boardSquareModel.GridPosition;
		AddChild(boardSquare);
		return boardSquare;
	}

	public override void _Ready()
	{
		GD.Print("...BoardHandler ready.");
	}
}
