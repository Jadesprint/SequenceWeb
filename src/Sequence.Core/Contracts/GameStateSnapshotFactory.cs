using Sequence.Core.Game;

namespace Sequence.Core.Contracts;

public static class GameStateSnapshotFactory
{
    public static GameStateSnapshot Build(GameState state, Guid recipientPlayerId)
    {
        var players = state.Players
            .Select(p => new PlayerSummary(
                p.Id,
                p.Name,
                p.Color,
                p.Hand.Count,
                state.SequenceCounts.GetValueOrDefault(p.Id)))
            .ToList();

        var board = state.Board.AllCells()
            .Select(c => new CellSnapshot(c.Row, c.Col, c.IsFree, c.Card, c.ChipOwnerId, c.PartOfCompletedSequence))
            .ToList();

        var recipient = state.GetPlayer(recipientPlayerId);

        return new GameStateSnapshot(
            state.RoomCode,
            state.Status,
            state.Status == GameStatus.InProgress ? state.CurrentPlayer.Id : null,
            state.WinnerId,
            state.SequencesToWin,
            state.Deck.Count,
            players,
            board,
            recipient?.Hand.ToList() ?? new(),
            recipientPlayerId
        );
    }
}
