namespace Sequence.Core.Cards;

/// <summary>Two standard 52-card decks (104 cards total), as used in Sequence.</summary>
public sealed class Deck
{
    private readonly List<Card> _cards;
    private readonly Random _random;

    public Deck(Random? random = null)
    {
        _random = random ?? new Random();
        _cards = BuildDoubleDeck();
        Shuffle();
    }

    public int Count => _cards.Count;

    public Card Draw()
    {
        if (_cards.Count == 0)
            throw new InvalidOperationException("Deck is empty.");

        var card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }

    public void Shuffle()
    {
        for (var i = _cards.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    private static List<Card> BuildDoubleDeck()
    {
        var cards = new List<Card>(104);
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                cards.Add(new Card(suit, rank));
                cards.Add(new Card(suit, rank));
            }
        }
        return cards;
    }
}
