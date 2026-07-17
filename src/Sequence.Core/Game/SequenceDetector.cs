using Sequence.Core.Board;

namespace Sequence.Core.Game;

/// <summary>
/// Finds completed 5-in-a-row sequences (horizontal, vertical, diagonal) where each cell is
/// either owned by the given player or is a free corner. Simplification vs. the physical game:
/// each non-corner chip may be part of at most one counted sequence (the official rules allow a
/// single shared chip between two sequences; we skip that edge case for a first playable version).
/// Free corners, being wild for everyone, may be reused by any number of sequences.
/// </summary>
public static class SequenceDetector
{
    private const int RunLength = 5;

    private static readonly (int DRow, int DCol)[] Directions =
    {
        (0, 1),   // horizontal
        (1, 0),   // vertical
        (1, 1),   // diagonal down-right
        (1, -1)   // diagonal down-left
    };

    /// <summary>Recomputes sequence counts and completed-sequence flags for every player, mutating board cells and state.SequenceCounts.</summary>
    public static void Recompute(GameState state)
    {
        foreach (var cell in state.Board.AllCells())
            cell.PartOfCompletedSequence = false;

        foreach (var player in state.Players)
        {
            var usedNonCornerCells = new HashSet<(int, int)>();
            var count = 0;

            foreach (var window in FindWindowsForPlayer(state.Board, player.Id))
            {
                if (window.Any(c => !c.IsFree && usedNonCornerCells.Contains((c.Row, c.Col))))
                    continue;

                foreach (var c in window)
                {
                    if (!c.IsFree)
                        usedNonCornerCells.Add((c.Row, c.Col));
                    c.PartOfCompletedSequence = true;
                }

                count++;
            }

            state.SequenceCounts[player.Id] = count;
        }
    }

    private static IEnumerable<Cell[]> FindWindowsForPlayer(GameBoard board, Guid playerId)
    {
        var size = board.Size;

        foreach (var (dRow, dCol) in Directions)
        {
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var endRow = row + dRow * (RunLength - 1);
                    var endCol = col + dCol * (RunLength - 1);
                    if (endRow < 0 || endRow >= size || endCol < 0 || endCol >= size)
                        continue;

                    var window = new Cell[RunLength];
                    var valid = true;
                    for (var i = 0; i < RunLength; i++)
                    {
                        var cell = board.GetCell(row + dRow * i, col + dCol * i);
                        if (!cell.IsFree && cell.ChipOwnerId != playerId)
                        {
                            valid = false;
                            break;
                        }
                        window[i] = cell;
                    }

                    if (valid)
                        yield return window;
                }
            }
        }
    }
}
