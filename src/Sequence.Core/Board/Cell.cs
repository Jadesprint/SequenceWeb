using Sequence.Core.Cards;

namespace Sequence.Core.Board;

public sealed class Cell
{
    public required int Row { get; init; }
    public required int Col { get; init; }

    /// <summary>Null only for the four free corner cells.</summary>
    public Card? Card { get; init; }

    public bool IsFree => Card is null;

    /// <summary>Player id occupying this cell, or null if empty. Free corners are always considered occupied by every player for sequence purposes.</summary>
    public Guid? ChipOwnerId { get; set; }

    /// <summary>Set by the sequence detector; protects the chip from one-eyed jack removal.</summary>
    public bool PartOfCompletedSequence { get; set; }
}
