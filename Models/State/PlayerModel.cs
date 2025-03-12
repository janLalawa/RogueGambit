namespace RogueGambit.Models.State;

public class PlayerModel
{
    public string Name { get; set; }
    public PieceColor Color { get; set; }
    public PieceOwner Controller { get; set; }
}