using Sequence.Core.Cards;
using Xunit;

namespace Sequence.Core.Tests;

public class DeckTests
{
    [Fact]
    public void NewDeck_Has104Cards()
    {
        var deck = new Deck();
        Assert.Equal(104, deck.Count);
    }

    [Fact]
    public void Draw_RemovesOneCardAndDecrementsCount()
    {
        var deck = new Deck();
        deck.Draw();
        Assert.Equal(103, deck.Count);
    }

    [Fact]
    public void Draw_FromEmptyDeck_Throws()
    {
        var deck = new Deck();
        for (var i = 0; i < 104; i++) deck.Draw();
        Assert.Throws<InvalidOperationException>(() => deck.Draw());
    }
}
