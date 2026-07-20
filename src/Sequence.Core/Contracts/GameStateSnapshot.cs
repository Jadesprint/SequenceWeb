using Sequence.Core.Cards;
using Sequence.Core.Game;

namespace Sequence.Core.Contracts;

public sealed record PlayerSummary(Guid Id, string Name, PlayerColor Color, int HandCount, int SequenceCount);

public sealed record CellSnapshot(int Row, int Col, bool IsFree, Card? Card, Guid? ChipOwnerId, bool PartOfCompletedSequence);

/// <summary>
/// A per-recipient view of game state: shared board/roster info everyone sees, plus the
/// recipient's own hand. Other players' hands are never included.
/// </summary>
public sealed record GameStateSnapshot(
    string RoomCode,
    GameStatus Status,
    Guid? CurrentPlayerId,
    Guid? WinnerId,
    int SequencesToWin,
    int DeckCount,
    IReadOnlyList<PlayerSummary> Players,
    IReadOnlyList<CellSnapshot> Board,
    IReadOnlyList<Card> YourHand,
    Guid YourPlayerId
);

public sealed record MoveRequest(Card Card, int Row, int Col, MoveType Type);

public sealed record RoomJoinResult(bool Success, string? Error, string? RoomCode, Guid? PlayerId, string? RejoinToken);