using Sequence.Core.Cards;

namespace Sequence.Core.Contracts;

public sealed record LabeledTarget(string Label, int Row, int Col);

///<summary>
/// Broadcast the instant a player selects a card to play, before the move is commited
/// Lets everyone know the label for the legal target cell
/// Not in a persisted state, therefore not in GameStateSnapshot
/// </summary>

public sealed record CardSelection(Guid PlayerId, Card Card, IReadOnlyList<LabeledTarget> Targets);
