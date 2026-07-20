using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.SignalR.Client;
using Sequence.Core.Cards;
using Sequence.Core.Contracts;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Sequence.Client.Services;

public sealed class GameClient : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    // TODO: move to configuration before deploying anywhere but localhost.
    private const string HubUrl = "http://localhost:5187/hubs/game";
    private const string SessionStorageKey = "sequence.session";

    private HubConnection? _connection;

    public GameClient(IJSRuntime js)
    {
        _js = js;
    }

    private sealed record SavedSession(string RoomCode, Guid PlayerId, string RejoinToken);
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
        _connection.On<GameStateSnapshot>("DisplayStateUpdated", snapshot =>
        {
            State = snapshot;
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
        if (result.Success)
        {
            PlayerId = result.PlayerId;
            await SaveSessionAsync(result.RoomCode!, result.PlayerId!.Value, result.RejoinToken!);
        }
        return result;
    }

    public async Task<RoomJoinResult> JoinRoomAsync(string roomCode, string playerName)
    {
        await EnsureConnectedAsync();
        var result = await _connection!.InvokeAsync<RoomJoinResult>("JoinRoom", roomCode, playerName);
        if (result.Success)
        {
            PlayerId = result.PlayerId;
            await SaveSessionAsync(result.RoomCode!, result.PlayerId!.Value, result.RejoinToken!);
        }
        return result;
    }

    public async Task StartGameAsync(string roomCode)
    {
        await EnsureConnectedAsync();
        await _connection!.InvokeAsync("StartGame", roomCode);
    }

    public async Task WatchRoomAsync(string roomCode)
    {
        await EnsureConnectedAsync();
        await _connection!.InvokeAsync("WatchRoom", roomCode);
    }
    public async Task<bool> TryRejoinAsync(string roomCode)
    {
        var saved = await LoadSessionAsync();
        if (saved is null || !string.Equals(saved.RoomCode, roomCode, StringComparison.OrdinalIgnoreCase))
            return false;

        await EnsureConnectedAsync();
        var result = await _connection!.InvokeAsync<RoomJoinResult>("RejoinRoom", roomCode, saved.RejoinToken);

        if (result.Success)
        {
            PlayerId = result.PlayerId;
            return true;
        }

        await ClearSessionAsync();
        return false;
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
        private async Task SaveSessionAsync(string roomCode, Guid playerId, string rejoinToken)
    {
        var session = new SavedSession(roomCode, playerId, rejoinToken);
        await _js.InvokeVoidAsync("localStorage.setItem", SessionStorageKey, JsonSerializer.Serialize(session));
    }

    private async Task<SavedSession?> LoadSessionAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", SessionStorageKey);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<SavedSession>(json);
    }

    private async Task ClearSessionAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", SessionStorageKey);
    }

}
