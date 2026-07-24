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

**3. Local deploy** (Port forwarding using any method of your liking)  
After compiling and running (assuming you use VSCode to do so) you can also forward your port using dev tunnels.  
Remember to change port privacy to public on both ports server and client.  
After that you can copy the url and share it to anyone. Keep in mind all security risks regarding port forwarding with public privacy.  
You can then create a room and navigate to `YOUR_DEV_TUNNEL_URL/display/YOUR_ROOM_CODE` in another tab or device to access the game board where a QR Code will generate for others to join.  
Then done, you can play!  


> The game hub URL configuration can be edited at `Sequence.Server/Properties/appsettings.Development.json` (for Client url pid 5216) and `Sequence.Client/wwwroot/appsettings.json` (for Server instance pid 5187). Here you can paste the urls from the port forwarding integration at VSCode Console  

**4. Gameplay loop and tips** 
- No need to setup anything else, game rules such as teams, cards for players and sequences needed to win are automatically configured based on player count
- You can play this from your phone using your pc or any other device with web browser as display for the game board (Kahoot style!)
- Select a card to play and then choose A or B positions (highlighted on board)
- When selecting a Joker you will be prompted a different set of options, this time Battleship style. Choose column and row to confirm selection.
- When playing a dead card the game will let you know and follow the official set of rules for Sequence
- Currently looking forward to add the overlaping chip rule. Keep in mind this is not a thing when playing WebSequence!

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

**Known simplification:**   
the official rules allow a single chip to be shared between two different completed sequences.    
This implementation currently does not allow that overlap — each chip counts toward at most one sequence.  

The board's card layout is generated deterministically in code (`BoardLayout.cs`) — every non-jack card still appears exactly twice among the 96 non-corner cells, and the arrangement is identical for every game, tho it differs cosmetically from the physical board. Feel free to remove the hardcoded seed to make it random.  

Also, there is no drawing within your turn rule and punishment.  
Check Status/Roadmap for more details.  

## Status / not yet done

- Not yet deployed anywhere (see stack notes below for the intended path).
- No animations/polish on the board UI yet.
- Sequence has a punishment rule for not drawing a card within your turn, looking forward to add this as a mechanic but it would require quite the effort.  

## Deployment plan

- **Client** (static Blazor WASM output): Vercel, Netlify, or Cloudflare Pages.
- **Server** (stateful SignalR host, needs persistent WebSocket connections): Fly.io or Railway 

## Special thanks  
Thanks to @selfthinker for the CSS-Playing-Cards repo, very much inspiring to build my own css file
