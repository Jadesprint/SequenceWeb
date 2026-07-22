using Sequence.Core.Cards;
using Sequence.Core.Game;
using Xunit;

namespace Sequence.Core.Tests;

public class DiscardDeadCardTests
{
    private static GameState NewInProgressStateWithTwoPlayers(out Player p1, out Player p2)
    {
        var state = new GameState { RoomCode = "TEST" };
        p1 = new Player { Id = Guid.NewGuid(), Name = "P1", Color = PlayerColor.Blue };
        p2 = new Player { Id = Guid.NewGuid(), Name = "P2", Color = PlayerColor.Green };
        state.Players.Add(p1);
        state.Players.Add(p2);
        GameEngine.StartGame(state);
        return state;
    }

    [Fact]
    public void DiscardingATrulyDeadCard_DrawsReplacementAndAdvancesTurn()
    {
        var state = NewInProgressStateWithTwoPlayers(out var p1, out var p2);

        var deadCard = state.Board.AllCells().First(c => !c.IsFree).Card!.Value;
        foreach (var cell in state.Board.CellsForCard(deadCard))
            cell.ChipOwnerId = p2.Id;

        p1.Hand.Add(deadCard);
        var handSizeBefore = p1.Hand.Count;
        var deckCountBefore = state.Deck.Count;

        var move = new Move(p1.Id, deadCard, -1, -1, MoveType.DiscardDeadCard);
        GameEngine.ApplyMove(state, move);

        Assert.DoesNotContain(deadCard, p1.Hand);
        Assert.Equal(handSizeBefore, p1.Hand.Count);
        Assert.Equal(deckCountBefore - 1, state.Deck.Count);
        Assert.Contains(deadCard, state.DiscardPile);
        Assert.Equal(p2.Id, state.CurrentPlayer.Id);
    }

    [Fact]
    public void DiscardingACardThatStillHasLegalMoves_Throws()
    {
        var state = NewInProgressStateWithTwoPlayers(out var p1, out _);

        var liveCard = state.Board.AllCells().First(c => !c.IsFree).Card!.Value;
        p1.Hand.Add(liveCard);

        var move = new Move(p1.Id, liveCard, -1, -1, MoveType.DiscardDeadCard);

        Assert.Throws<InvalidOperationException>(() => GameEngine.ApplyMove(state, move));
    }
}
