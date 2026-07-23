using Sequence.Core.Cards;

namespace Sequence.Client;

public static class CardVisuals
{
    public static string RankText(Card card) => card.Rank switch
    {
        Rank.Two => "2",
        Rank.Three => "3",
        Rank.Four => "4",
        Rank.Five => "5",
        Rank.Six => "6",
        Rank.Seven => "7",
        Rank.Eight => "8",
        Rank.Nine => "9",
        Rank.Ten => "10",
        Rank.Jack => "J",
        Rank.Queen => "Q",
        Rank.King => "K",
        Rank.Ace => "A",
        _ => "?"
    };

    public static string SuitGlyph(Suit suit) => suit switch
    {
        Suit.Clubs => "♣",
        Suit.Diamonds => "♦",
        Suit.Hearts => "♥",
        Suit.Spades => "♠",
        _ => "?"
    };

    public static string SuitClass(Suit suit) => suit switch
    {
        Suit.Clubs => "suit-blue",
        Suit.Diamonds => "suit-orange",
        Suit.Hearts => "suit-red",
        Suit.Spades => "suit-black",
        _ => "?"
    };

    public static string DescribeCard(Card card) => $"{RankText(card)}{SuitGlyph(card.Suit)}";

    public static string[] PipPositions(Rank rank) => rank switch
    {
        Rank.Two => new[] { "tc", "bc" },
        Rank.Three => new[] { "tc", "mc", "bc" },
        Rank.Four => new[] { "tl", "tr", "bl", "br" },
        Rank.Five => new[] { "tl", "tr", "mc", "bl", "br" },
        Rank.Six => new[] { "tl", "tr", "ml", "mr", "bl", "br" },
        Rank.Seven => new[] { "tl", "tr", "uc", "ml", "mr", "bl", "br" },
        Rank.Eight => new[] { "tl", "tr", "uc", "ml", "mr", "lc", "bl", "br" },
        Rank.Nine => new[] { "tl", "tr", "ul", "ur", "mc", "ll", "lr", "bl", "br" },
        Rank.Ten => new[] { "tl", "tr", "ul", "ur", "uc", "lc", "ll", "lr", "bl", "br" },
        _ => Array.Empty<string>()
    };

    public static bool IsBottomHalf(string pos) => pos[0] is 'l' or 'b';
}
