namespace RogueGambit.Static.Constants;

public static class GameConstants
{
    public enum ClickMode
    {
        All,
        BoardOnly,
        PieceOnly,
        UiOnly,
        None
    }

    public enum PlayerStatus
    {
        WaitingForTurn,
        SelectingPiece,
        MovingPiece,
        PickingAttack,
        PromotingPawn,
        Castling,
        GameOver,
        InMenu
    }

    public static bool FriendlyFire { get; set; } = false;
}