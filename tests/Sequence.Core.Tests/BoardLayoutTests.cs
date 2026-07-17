using Sequence.Core.Board;
using Sequence.Core.Cards;
using Xunit;

namespace Sequence.Core.Tests;

public class BoardLayoutTests
{
    [Fact]
    public void Generate_HasFourFreeCorners()
    {
        var layout = BoardLayout.Generate();
        foreach (var (row, col) in BoardLayout.Corners)
            Assert.Null(layout[row, col]);
    }

    [Fact]
    public void Generate_EveryNonJackCardAppearsExactlyTwice()
    {
        var layout = BoardLayout.Generate();
        var counts = new Dictionary<Card, int>();

        for (var row = 0; row < BoardLayout.Size; row++)
        {
            for (var col = 0; col < BoardLayout.Size; col++)
            {
                var card = layout[row, col];
                if (card is null) continue;
                counts[card.Value] = counts.GetValueOrDefault(card.Value) + 1;
            }
        }

        var expectedUniqueCards = 4 * 12; // 4 suits * 12 non-jack ranks
        Assert.Equal(expectedUniqueCards, counts.Count);
        Assert.All(counts.Values, c => Assert.Equal(2, c));
    }

    [Fact]
    public void Generate_IsDeterministicAcrossCalls()
    {
        var first = BoardLayout.Generate();
        var second = BoardLayout.Generate();

        for (var row = 0; row < BoardLayout.Size; row++)
            for (var col = 0; col < BoardLayout.Size; col++)
                Assert.Equal(first[row, col], second[row, col]);
    }
}
