using System.Runtime.CompilerServices;
using RogueGambit.Handlers;
using RogueGambit.Logic;
using BoardHandler = RogueGambit.Handlers.BoardHandler;

[assembly: InternalsVisibleTo("RogueGambit.Tests")]

public partial class MainScene : Node2D
{
	public override void _Ready()
	{
		GD.Print("...MainScene ready.");
	}

	public override void _EnterTree()
	{
		GD.Print("...MainScene entered tree.");
		RegisterServices();
	}

	private void RegisterServices()
	{
		RegisterService<IGameStateHandler>(GetNode<GameStateHandler>("/root/MainScene/GameStateHandler"));
		RegisterService<IBoardHandler>(GetNode<BoardHandler>("/root/MainScene/BoardHandler"));
		RegisterService<IInputHandler>(GetNode<InputHandler>("/root/MainScene/InputHandler"));
		RegisterService<IMoveHandler>(GetNode<MoveHandler>("/root/MainScene/MoveHandler"));
		RegisterService<IPieceHandler>(GetNode<PieceHandler>("/root/MainScene/PieceHandler"));
		RegisterService<ITurnHandler>(GetNode<TurnHandler>("/root/MainScene/TurnHandler"));
		RegisterService<IDebugConsole>(GetNode<DebugConsole>("/root/MainScene/DebugConsole"));
		RegisterService<ISaveHandler>(GetNode<SaveHandler>("/root/MainScene/SaveHandler"));
		RegisterService<IUciHandler>(GetNode<UciHandler>("/root/MainScene/UciHandler"));

		RegisterService<IMoveLogic>(new MoveLogic());

		RegisterNodeFactory<BoardSquare>((BoardHandler)GetService<IBoardHandler>());
		RegisterNodeFactory<Piece>((PieceHandler)GetService<IPieceHandler>());
	}
}
