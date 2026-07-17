using Sequence.Core.Board;
using Sequence.Core.Cards;

namespace Sequence.Core.Game;

public sealed class GameState
{
    public required string RoomCode { get; init; }
    public GameBoard Board { get; } = new();
    public Deck Deck { get; } = new();
    public List<Card> DiscardPile { get; } = new();
    public List<Player> Players { get; } = new();
    public int CurrentPlayerIndex { get; set; }
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    public Guid? WinnerId { get; set; }
    public Dictionary<Guid, int> SequenceCounts { get; } = new();

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    /// <summary>Standard Sequence rule: 2 players/teams need 2 sequences, 3 need 1.</summary>
    public int SequencesToWin => Players.Count >= 3 ? 1 : 2;

    public const int HandSizeForTwoPlayers = 7;
    public const int HandSizeForThreePlayers = 6;

    public int HandSize => Players.Count >= 3 ? HandSizeForThreePlayers : HandSizeForTwoPlayers;

    public Player? GetPlayer(Guid playerId) => Players.FirstOrDefault(p => p.Id == playerId);

    public void AdvanceTurn()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    }
}
