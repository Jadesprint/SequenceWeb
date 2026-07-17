using System.Collections.Concurrent;
using Sequence.Core.Game;

namespace Sequence.Server;

public sealed class RoomManager
{
    private static readonly char[] CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private readonly ConcurrentDictionary<string, GameState> _rooms = new();
    private readonly Random _random = new();

    public GameState CreateRoom()
    {
        string code;
        do
        {
            code = GenerateCode();
        } while (!_rooms.TryAdd(code, new GameState { RoomCode = code }));

        return _rooms[code];
    }

    public bool TryGetRoom(string roomCode, out GameState state) =>
        _rooms.TryGetValue(roomCode.ToUpperInvariant(), out state!);

    public void RemoveRoom(string roomCode) => _rooms.TryRemove(roomCode.ToUpperInvariant(), out _);

    private readonly ConcurrentDictionary<string, (string RoomCode, Guid PlayerId)> _connectionToPlayer = new();
    private readonly ConcurrentDictionary<(string RoomCode, Guid PlayerId), string> _playerToConnection = new();

    public void TrackConnection(string connectionId, string roomCode, Guid playerId)
    {
        var code = roomCode.ToUpperInvariant();
        _connectionToPlayer[connectionId] = (code, playerId);
        _playerToConnection[(code, playerId)] = connectionId;
    }

    public bool TryGetConnection(string connectionId, out (string RoomCode, Guid PlayerId) info) =>
        _connectionToPlayer.TryGetValue(connectionId, out info);

    public string? TryGetConnectionId(string roomCode, Guid playerId) =>
        _playerToConnection.TryGetValue((roomCode.ToUpperInvariant(), playerId), out var id) ? id : null;

    public void RemoveConnection(string connectionId)
    {
        if (_connectionToPlayer.TryRemove(connectionId, out var info))
            _playerToConnection.TryRemove(info, out _);
    }

    private string GenerateCode()
    {
        Span<char> buffer = stackalloc char[5];
        for (var i = 0; i < buffer.Length; i++)
            buffer[i] = CodeAlphabet[_random.Next(CodeAlphabet.Length)];
        return new string(buffer);
    }
}
