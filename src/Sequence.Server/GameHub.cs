using Microsoft.AspNetCore.SignalR;
using Sequence.Core.Contracts;
using Sequence.Core.Game;
using Sequence.Core.Cards;

namespace Sequence.Server;

public sealed class GameHub : Hub
{
    private const int MaxPlayers = 3;

    private readonly RoomManager _rooms;

    public GameHub(RoomManager rooms)
    {
        _rooms = rooms;
    }

    public async Task<RoomJoinResult> CreateAndJoinRoom(string playerName)
    {
        var state = _rooms.CreateRoom();
        return await JoinRoomCore(state, playerName);
    }

    public async Task<RoomJoinResult> JoinRoom(string roomCode, string playerName)
    {
        if (!_rooms.TryGetRoom(roomCode, out var state))
            return new RoomJoinResult(false, "Room not found.", null, null);

        return await JoinRoomCore(state, playerName);
    }

    private async Task<RoomJoinResult> JoinRoomCore(GameState state, string playerName)
    {
        if (state.Status != GameStatus.WaitingForPlayers)
            return new RoomJoinResult(false, "Game has already started.", null, null);

        if (state.Players.Count >= MaxPlayers)
            return new RoomJoinResult(false, "Room is full.", null, null);

        if (string.IsNullOrWhiteSpace(playerName))
            return new RoomJoinResult(false, "Name is required.", null, null);

        var color = (PlayerColor)state.Players.Count;
        var player = new Player { Id = Guid.NewGuid(), Name = playerName.Trim(), Color = color };
        state.Players.Add(player);

        await Groups.AddToGroupAsync(Context.ConnectionId, state.RoomCode);
        _rooms.TrackConnection(Context.ConnectionId, state.RoomCode, player.Id);

        await BroadcastState(state);

        return new RoomJoinResult(true, null, state.RoomCode, player.Id);
    }

    public async Task WatchRoom(string roomCode)
    {
        if (!_rooms.TryGetRoom(roomCode, out var state)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, state.RoomCode);

        var snapshot = GameStateSnapshotFactory.Build(state, Guid.Empty);
        await Clients.Caller.SendAsync("DisplayStateUpdated", snapshot);
    }

    public async Task StartGame(string roomCode)
    {
        if (!_rooms.TryGetRoom(roomCode, out var state)) return;

        try
        {
            GameEngine.StartGame(state);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
            return;
        }

        await BroadcastState(state);
    }

    public async Task PlayMove(string roomCode, MoveRequest move)
    {
        if (!_rooms.TryGetRoom(roomCode, out var state)) return;
        if (!_rooms.TryGetConnection(Context.ConnectionId, out var conn)) return;

        var domainMove = new Move(conn.PlayerId, move.Card, move.Row, move.Col, move.Type);

        try
        {
            GameEngine.ApplyMove(state, domainMove);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
            return;
        }

        await BroadcastState(state);
    }

    public async Task SelectCard(string roomCode, Card card)
    {
        if(!_rooms.TryGetRoom(roomCode, out var state)) return;
        if(!_rooms.TryGetConnection(Context.ConnectionId, out var conn)) return;

        if (state.Status != GameStatus.InProgress || state.CurrentPlayer.Id != conn.PlayerId)
        return;

        var player = state.GetPlayer(conn.PlayerId);
        if (player is null || !player.Hand.Contains(card)) return;

        var targets = MoveCandidateFinder.FindTargets(state, conn.PlayerId, card);
        var labeled = targets.Select((cell, i) => new LabeledTarget(LabelFor(i), cell.Row, cell.Col)).ToList();

        var selection = new CardSelection(conn.PlayerId, card, labeled);
        await Clients.Group(roomCode).SendAsync("CardSelected", selection);
    }

    private static string LabelFor(int index) =>
    index < 26 ? ((char)('A'+index)).ToString() : (index + 1).ToString();
    

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_rooms.TryGetConnection(Context.ConnectionId, out var conn))
        {
            _rooms.RemoveConnection(Context.ConnectionId);
            if (_rooms.TryGetRoom(conn.RoomCode, out var state))
            {
                await Clients.Group(conn.RoomCode).SendAsync("PlayerDisconnected", conn.PlayerId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastState(GameState state)
    {
        foreach (var player in state.Players)
        {
            var snapshot = GameStateSnapshotFactory.Build(state, player.Id);
            var connectionId = _rooms.TryGetConnectionId(state.RoomCode, player.Id);
            if (connectionId is not null)
                await Clients.Client(connectionId).SendAsync("StateUpdated", snapshot);
        }

        var displaySnapshot = GameStateSnapshotFactory.Build(state, Guid.Empty);
        await Clients.Group(state.RoomCode).SendAsync("DisplayStateUpdated", displaySnapshot);
    }
}
