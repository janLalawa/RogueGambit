using RogueGambit.Engine.RogueAi.Bobby;

namespace RogueGambit.Handlers;

public partial class AiHandler : Node2D, IAiHandler
{
    public IRogueAi Brain { get; set; }

    public override void _Ready()
    {
        GD.Print("...AiHandler ready.");
        Initialize();
    }

    public void Initialize()
    {
        InjectDependencies(this);
        Brain = new BobbyAi();
    }
}