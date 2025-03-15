namespace RogueGambit.Handlers;

public partial class UciHandler : Node2D, IUciHandler
{
	public override void _Ready()
	{
		GD.Print("...UciHandler ready.");
		Initialize();
	}

	private void Initialize()
	{
		InjectDependencies(this);
	}
}
