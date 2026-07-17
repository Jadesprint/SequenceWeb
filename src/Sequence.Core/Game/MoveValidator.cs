using Sequence.Core.Board;

namespace Sequence.Core.Game;

public static class MoveValidator
{
    public static bool TryValidate(GameState state, Move move, out string error)
    {
        error = string.Empty;

        if (state.Status != GameStatus.InProgress)
        {
            error = "Game is not in progress.";
            return false;
        }

        if (state.CurrentPlayer.Id != move.PlayerId)
        {
            error = "It is not your turn.";
            return false;
        }

        var player = state.GetPlayer(move.PlayerId);
        if (player is null || !player.Hand.Contains(move.Card))
        {
            error = "You do not hold that card.";
            return false;
        }

        if (move.Row < 0 || move.Row >= state.Board.Size || move.Col < 0 || move.Col >= state.Board.Size)
        {
            error = "Cell is off the board.";
            return false;
        }

        var cell = state.Board.GetCell(move.Row, move.Col);

        if (cell.IsFree)
        {
            error = "Free corner cells cannot be played on.";
            return false;
        }

        if (move.Card.IsOneEyedJack)
        {
            return ValidateRemoval(move, player, cell, out error);
        }

        return ValidatePlacement(move, player, cell, out error);
    }

    private static bool ValidatePlacement(Move move, Player player, Cell cell, out string error)
    {
        error = string.Empty;

        if (cell.ChipOwnerId is not null)
        {
            error = "Cell is already occupied.";
            return false;
        }

        if (!move.Card.IsTwoEyedJack && cell.Card != move.Card)
        {
            error = "Card does not match this cell.";
            return false;
        }

        return true;
    }

    private static bool ValidateRemoval(Move move, Player player, Cell cell, out string error)
    {
        error = string.Empty;

        if (cell.ChipOwnerId is null)
        {
            error = "Cell is empty; nothing to remove.";
            return false;
        }

        if (cell.ChipOwnerId == player.Id)
        {
            error = "You cannot remove your own chip.";
            return false;
        }

        if (cell.PartOfCompletedSequence)
        {
            error = "Chip is part of a completed sequence and cannot be removed.";
            return false;
        }

        return true;
    }
}
