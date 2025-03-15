namespace RogueGambit.Logic;

public static class InputLogic
{
    public static bool IsMyTurn(GameState gameState, PieceOwner player)
    {
        return gameState.CurrentTurn == player;
    }

    public static bool IsMyPiece(PieceModel piece, PieceOwner player)
    {
        return piece.Owner == player;
    }

    public static bool DoesPieceInstanceExist(PieceModel piece)
    {
        return piece.Instance != null;
    }

    public static bool IsAttackMove(BoardSquareModel targetSquare, PieceModel selectedPiece, GameState gameState)
    {
        var isOccupied = targetSquare?.IsOccupied ?? false;
        var isMyself = targetSquare != null && gameState.GetPieceAtPosition(targetSquare.GridPosition).GridPosition == selectedPiece.GridPosition;

        return isOccupied && !isMyself;
    }

    public static bool IsNormalMove(BoardSquareModel targetSquare)
    {
        return !targetSquare.IsOccupied;
    }


    public static bool IsMoveValid(PieceModel piece, Vector2 targetPosition)
    {
        return piece.ValidMoves.Contains(targetPosition);
    }


    public static ClickMode GetClickMode(PlayerStatus playerStatus)
    {
        return playerStatus switch
        {
            SelectingPiece => ClickMode.PieceOnly,
            MovingPiece => ClickMode.BoardOnly,
            _ => ClickMode.None
        };
    }
}