using Sequence.Core.Game;
using Xunit;

namespace Sequence.Core.Tests;

public class SequenceDetectorTests
{
    private static (GameState state, Player player) NewStateWithOnePlayer()
    {
        var state = new GameState { RoomCode = "TEST" };
        var player = new Player { Id = Guid.NewGuid(), Name = "P1", Color = PlayerColor.Blue };
        state.Players.Add(player);
        return (state, player);
    }

    [Fact]
    public void FiveInARow_Horizontal_CountsAsOneSequence()
    {
        var (state, player) = NewStateWithOnePlayer();

        // Row 1, columns 1..5 (avoids the free corners at row 0 / row 9).
        for (var col = 1; col <= 5; col++)
            state.Board.GetCell(1, col).ChipOwnerId = player.Id;

        SequenceDetector.Recompute(state);

        Assert.Equal(1, state.SequenceCounts[player.Id]);
    }

    [Fact]
    public void FourChipsPlusFreeCorner_CountsAsOneSequence()
    {
        var (state, player) = NewStateWithOnePlayer();

        // Row 0 is the top edge; (0,0) is a free corner. Columns 1..4 complete the run of 5.
        for (var col = 1; col <= 4; col++)
            state.Board.GetCell(0, col).ChipOwnerId = player.Id;

        SequenceDetector.Recompute(state);

        Assert.Equal(1, state.SequenceCounts[player.Id]);
    }

    [Fact]
    public void NoChips_CountsAsZeroSequences()
    {
        var (state, player) = NewStateWithOnePlayer();

        SequenceDetector.Recompute(state);

        Assert.Equal(0, state.SequenceCounts[player.Id]);
    }

    [Fact]
    public void TenInARow_CountsAsTwoSequencesNotSix()
    {
        var (state, player) = NewStateWithOnePlayer();

        for (var col = 0; col < 10; col++)
            state.Board.GetCell(2, col).ChipOwnerId = player.Id;

        SequenceDetector.Recompute(state);

        Assert.Equal(2, state.SequenceCounts[player.Id]);
    }
}
