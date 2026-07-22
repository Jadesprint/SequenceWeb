namespace Sequence.Core.Game;

public static class GameEngine
{
    public static void StartGame(GameState state)
    {
        if (state.Players.Count < 2)
            throw new InvalidOperationException("At least two players are required to start.");

        foreach (var player in state.Players)
        {
            for (var i = 0; i < state.HandSize; i++)
                player.Hand.Add(state.Deck.Draw());
        }

        state.CurrentPlayerIndex = 0;
        state.Status = GameStatus.InProgress;
    }

    public static void ApplyMove(GameState state, Move move)
    {
        if (!MoveValidator.TryValidate(state, move, out var error))
            throw new InvalidOperationException(error);

        var player = state.GetPlayer(move.PlayerId)!;

        if (move.Type == MoveType.DiscardDeadCard)
        {
            player.Hand.Remove(move.Card);
            state.DiscardPile.Add(move.Card);

            if (state.Deck.Count > 0)
                player.Hand.Add(state.Deck.Draw());

            state.AdvanceTurn();
            return;
        }

        var cell = state.Board.GetCell(move.Row, move.Col);


        if (move.Type == MoveType.RemoveChip)
        {
            cell.ChipOwnerId = null;
        }
        else
        {
            cell.ChipOwnerId = player.Id;
        }

        player.Hand.Remove(move.Card);
        state.DiscardPile.Add(move.Card);

        if (state.Deck.Count > 0)
            player.Hand.Add(state.Deck.Draw());

        SequenceDetector.Recompute(state);

        if (state.SequenceCounts.TryGetValue(player.Id, out var sequences) && sequences >= state.SequencesToWin)
        {
            state.Status = GameStatus.Completed;
            state.WinnerId = player.Id;
            return;
        }

        state.AdvanceTurn();
    }
}
