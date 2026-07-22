using Sequence.Core.Cards;

namespace Sequence.Core.Game;

public enum MoveType
{
    PlaceChip,
    RemoveChip,
    DiscardDeadCard
}

public sealed record Move(Guid PlayerId, Card Card, int Row, int Col, MoveType Type);
