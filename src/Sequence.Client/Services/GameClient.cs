using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.SignalR.Client;
using Sequence.Core.Cards;
using Sequence.Core.Contracts;

namespace Sequence.Client.Services;

public sealed class GameClient : IAsyncDisposable
{
    // TODO: move to configuration before deploying anywhere but localhost.
    private const string HubUrl = "http://localhost:5187/hubs/game";

    private HubConnection? _connection;

    public GameStateSnapshot? State { get; private set; }
    public Guid? PlayerId { get; private set; }
    public string? LastError { get; private set; }
    public CardSelection? LatestSelection {get; private set;}

    public event Action? OnChange;

    private async Task EnsureConnectedAsync()
    {
        if (_connection is not null) return;

        _connection = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<GameStateSnapshot>("StateUpdated", snapshot =>
        {
            State = snapshot;
            PlayerId = snapshot.YourPlayerId;
            LastError = null;
            OnChange?.Invoke();
        });

        _connection.On<string>("Error", message =>
        {
            LastError = message;
            OnChange?.Invoke();
        });

        _connection.On<CardSelection>("CardSelected", selection =>
        {
            LatestSelection = selection;
            OnChange?.Invoke();
        });

        _connection.On<Guid>("PlayerDisconnected", _ => OnChange?.Invoke());

        await _connection.StartAsync();
    }

    public async Task<RoomJoinResult> CreateRoomAsync(string playerName)
    {
        await EnsureConnectedAsync();
        var result = await _connection!.InvokeAsync<RoomJoinResult>("CreateAndJoinRoom", playerName);
        if (result.Success) PlayerId = result.PlayerId;
        return result;
    }

    public async Task<RoomJoinResult> JoinRoomAsync(string roomCode, string playerName)
    {
        await EnsureConnectedAsync();
        var result = await _connection!.InvokeAsync<RoomJoinResult>("JoinRoom", roomCode, playerName);
        if (result.Success) PlayerId = result.PlayerId;
        return result;
    }

    public async Task StartGameAsync(string roomCode)
    {
        await EnsureConnectedAsync();
        await _connection!.InvokeAsync("StartGame", roomCode);
    }

    public async Task PlayMoveAsync(string roomCode, MoveRequest move)
    {
        await EnsureConnectedAsync();
        await _connection!.InvokeAsync("PlayMove", roomCode, move);
    }

    public async Task SelectcardAsync(string roomCode, Card card)
    {
        await EnsureConnectedAsync();
        await _connection!.InvokeAsync("SelectCard", roomCode, card);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
