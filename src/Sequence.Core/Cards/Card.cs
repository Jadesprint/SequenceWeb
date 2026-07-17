namespace Sequence.Core.Cards;

public readonly record struct Card(Suit Suit, Rank Rank)
{
    public bool IsJack => Rank == Rank.Jack;

    /// <summary>Two-eyed jacks (clubs, diamonds) are wild: play anywhere on the board.</summary>
    public bool IsTwoEyedJack => Rank == Rank.Jack && (Suit == Suit.Clubs || Suit == Suit.Diamonds);

    /// <summary>One-eyed jacks (hearts, spades) remove an opponent chip instead of placing one.</summary>
    public bool IsOneEyedJack => Rank == Rank.Jack && (Suit == Suit.Hearts || Suit == Suit.Spades);

    public override string ToString() => $"{Rank} of {Suit}";
}
