using Sequence.Core.Cards;

namespace Sequence.Core.Game;

public sealed class Player
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required PlayerColor Color { get; init; }
    public List<Card> Hand { get; } = new();
}
