using System.Security.Cryptography.X509Certificates;
using Sequence.Core.Cards;
using Sequence.Core.Game;
using Xunit;

namespace Sequence.Core.Tests;

public class MoveCandidateFinderTest
{
    private static GameState NewStateWithTwoPlayers(out Player p1, out Player p2)
    {
        var state = new GameState {RoomCode = "TEST"};
        p1 = new Player {Id = Guid.NewGuid(), Name = "P1", Color = PlayerColor.Blue};
        p2 = new Player {Id = Guid.NewGuid(), Name = "P2", Color = PlayerColor.Green};
        state.Players.Add(p1);
        state.Players.Add(p2);
        return state;
    }

    [Fact]
    public void NormalCard_BothCopiesOpen_ReturnsTwoCandidates()
    {
        var state = NewStateWithTwoPlayers(out var p1, out _);
        var card = state.Board.AllCells().First(c => !c.IsFree).Card!.Value;

        var targets = MoveCandidateFinder.FindTargets(state, p1.Id, card);

        Assert.Equal(2, targets.Count);
    }

    [Fact]
    public void NormalCard_OneCopyOccupied_ReturnsOneCandidate()
    {
        var state = NewStateWithTwoPlayers(out var p1, out var p2);
        var card = state.Board.AllCells().First(c => !c.IsFree).Card!.Value;
        var cells = state.Board.CellsForCard(card).ToList();
        cells[0].ChipOwnerId = p2.Id;

        var targets = MoveCandidateFinder.FindTargets(state, p1.Id, card);

        Assert.Single(targets);
        Assert.Equal(cells[1].Row, targets[0].Row);
        Assert.Equal(cells[1].Col, targets[0].Col);
    }

    [Fact]
    public void NormalCard_BothCopesOccupied_ReturnsNoCandidates_DeadCard()
    {
        var state = NewStateWithTwoPlayers(out var p1, out var p2);
        var card = state.Board.AllCells().First(c => !c.IsFree).Card!.Value;
        foreach(var cell in state.Board.CellsForCard(card))
            cell.ChipOwnerId = p2.Id;

        var targets = MoveCandidateFinder.FindTargets(state, p1.Id, card);

        Assert.Empty(targets);
    }

    [Fact]
    public void TwoEyedJack_ReturnsEveryOpenNonCornerCell()
    {
        var state = NewStateWithTwoPlayers(out var p1, out _);
        var jack = new Card(Suit.Clubs, Rank.Jack);

        var targets = MoveCandidateFinder.FindTargets(state, p1.Id, jack);

        Assert.Equal(96, targets.Count); //These are the cells minus the corners, assuming nothing has been placed
    }

    [Fact]
    public void OneEyedJack_OnlyReturnsUnprotectedOpponentChips()
    {
        var state = NewStateWithTwoPlayers(out var p1 , out var p2);
        var oneEyedJack = new Card(Suit.Hearts, Rank.Jack);

        var opponentCell = state.Board.AllCells().First(c => !c.IsFree);
        opponentCell.ChipOwnerId = p2.Id;

        var ownCell = state.Board.AllCells().First(c => !c.IsFree && c != opponentCell);
        ownCell.ChipOwnerId = p1.Id;

        var protectedCell = state.Board.AllCells().First(c => !c.IsFree && c != opponentCell && c != ownCell);
        protectedCell.ChipOwnerId = p2.Id;
        protectedCell.PartOfCompletedSequence = true;

        var targets = MoveCandidateFinder.FindTargets(state, p1.Id, oneEyedJack);

        Assert.Contains(targets, c => c.Row == opponentCell.Row && c.Col == opponentCell.Col);
        Assert.DoesNotContain(targets, c => c.Row == ownCell.Row && c.Col == ownCell.Col);
        Assert.DoesNotContain(targets, c => c.Row == protectedCell.Row && c.Col == protectedCell.Col);
    }
}

