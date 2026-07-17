using Sequence.Core.Board;
using Sequence.Core.Cards;

namespace Sequence.Core.Game;

/// <summary>
/// Enumerates every legal target cell for a card, given the current board state and the
/// player attempting to play it. For an ordinary (non-jack) card there are at most two
/// results, since each card appears on the board exactly twice.
/// </summary>

public static class MoveCandidateFinder{
    public static IReadOnlyList<Cell> FindTargets(GameState state, Guid playerId, Card card)
    {
        IEnumerable<Cell> candidates;

        if (card.IsOneEyedJack)
        {
            candidates = state.Board.AllCells()
            .Where(c => !c.IsFree
            && c.ChipOwnerId is not null
            && c.ChipOwnerId != playerId
            && !c.PartOfCompletedSequence);
        }
        else if (card.IsTwoEyedJack)
        {
            candidates = state.Board.AllOpenNonCornerCells();
        }
        else
        {
            candidates = state.Board.OpenCellsForCard(card);
        }

        return candidates
            .OrderBy(c => c.Row)
            .ThenBy(c => c.Col)
            .ToList();

    }    
}
