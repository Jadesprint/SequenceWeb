# Sequence

A browser-based, online multiplayer version of the board game Sequence, built on .NET.

## Stack

- **Sequence.Core** — Game logic solely based on C#
- **Sequence.Server** — ASP.NET Core host with a SignalR hub (`GameHub`)
- **Sequence.Client** — Blazor WebAssembly frontend. References `Sequence.Core`
- **Sequence.Core.Tests** — xUnit tests for the engine

## Project layout

```
Sequence.slnx
src/
  Sequence.Core/      game engine (cards, board, rules, contracts shared with the client)
  Sequence.Server/     SignalR hub + room management
  Sequence.Client/     Blazor WebAssembly UI
tests/
  Sequence.Core.Tests/ engine unit tests
```

## Running locally

You need the .NET SDK (10.0+) installed.

**1. Start the server** (in one terminal):
```bash
dotnet run --project src/Sequence.Server/Sequence.Server.csproj --launch-profile http --urls http://localhost:5187
```

**2. Start the client** (in another terminal):
```bash
dotnet run --project src/Sequence.Client/Sequence.Client.csproj --launch-profile http --urls http://localhost:5216
```

**3. Open [http://localhost:5216](http://localhost:5216)** in two browser tabs (or one normal + one incognito) to play against yourself: create a room in one tab, join with the room code in the other.

> The client currently has the hub URL hardcoded to `http://localhost:5187` in `Sequence.Client/Services/GameClient.cs` — this needs to move to configuration before deploying anywhere but localhost.

## Running tests

```bash
dotnet test
```

## Game rules implemented

- Standard two-deck (104 card) Sequence deck; 7-card hands for 2 players.
- Two-eyed jacks (clubs, diamonds) are wild and can be played on any open space.
- One-eyed jacks (hearts, spades) remove an opponent's chip, as long as it isn't part of a completed sequence.
- Sequence detection across all 4 directions (horizontal, vertical, both diagonals), with the four free corners acting as wild for every player.
- Win condition: 2 sequences to win in a 2-player game (standard Sequence rules).

**Known simplification:** the official rules allow a single chip to be shared between two different completed sequences. This implementation currently does not allow that overlap — each chip counts toward at most one sequence. Worth revisiting if it matters for your games.

The board's card layout is generated deterministically in code (`BoardLayout.cs`) rather than transcribed from the physical retail board — every non-jack card still appears exactly twice among the 96 non-corner cells, and the arrangement is identical for every game, but the specific card-to-cell positions differ cosmetically from the physical board.

## Status / not yet done

- Hub URL / server address is not yet configurable per environment.
- Not yet deployed anywhere (see stack notes below for the intended path).
- No reconnect-after-disconnect handling — if a player's connection drops mid-game, their room state is lost (rooms are in-memory only).
- No animations/polish on the board UI yet.

## Deployment plan

- **Client** (static Blazor WASM output): Vercel, Netlify, or Cloudflare Pages.
- **Server** (stateful SignalR host, needs persistent WebSocket connections): Fly.io or Railway — not a platform that only supports short-lived serverless functions.
