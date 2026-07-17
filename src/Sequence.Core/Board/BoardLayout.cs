using Sequence.Core.Cards;

namespace Sequence.Core.Board;

/// <summary>
/// Generates the fixed 10x10 card layout used by every game. The four corners are free spaces;
/// the remaining 96 cells hold each of the 48 non-jack cards exactly twice. The arrangement is
/// deterministic (fixed seed) so every player sees the same board, but is generated rather than
/// transcribed from the physical retail board art.
/// </summary>
public static class BoardLayout
{
    public const int Size = 10;
    private const int LayoutSeed = 20260717;

    public static readonly IReadOnlyList<(int Row, int Col)> Corners = new[]
    {
        (0, 0), (0, Size - 1), (Size - 1, 0), (Size - 1, Size - 1)
    };

    public static Card?[,] Generate()
    {
        var nonJackCards = new List<Card>(96);
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                if (rank == Rank.Jack) continue;
                nonJackCards.Add(new Card(suit, rank));
                nonJackCards.Add(new Card(suit, rank));
            }
        }

        var random = new Random(LayoutSeed);
        for (var i = nonJackCards.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (nonJackCards[i], nonJackCards[j]) = (nonJackCards[j], nonJackCards[i]);
        }

        var layout = new Card?[Size, Size];
        var cardIndex = 0;
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                if (IsCorner(row, col))
                {
                    layout[row, col] = null;
                }
                else
                {
                    layout[row, col] = nonJackCards[cardIndex++];
                }
            }
        }

        return layout;
    }

    public static bool IsCorner(int row, int col) =>
        (row == 0 || row == Size - 1) && (col == 0 || col == Size - 1);
}
