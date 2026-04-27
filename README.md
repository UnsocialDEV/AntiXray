# AntiXray

AntiXray is a server-side Vintage Story anti-xray mod that hides ore block IDs from clients until the ore is legitimately revealed through gameplay. It is designed to reduce the value of client-side block scanners and overlay mods by ensuring unrevealed ore is sent to clients as ordinary rock instead of real ore.

The mod does not require a client install.

## What Problem This Solves

Client-side tools such as Block Overlay can inspect chunk block IDs that the server sends to the client. If a chunk contains real ore blocks, the client can highlight them even when the player should not know they exist.

AntiXray changes the server world state before the client sees it:

```text
Normal server chunk:
ore blocks -> client receives ore IDs -> overlay can highlight ore

AntiXray chunk:
ore blocks -> replaced with host rock placeholders -> client receives rock IDs -> overlay sees rock
```

The original ore block code is stored server-side. When gameplay legitimately exposes or discovers ore, the server restores the original ore block.

## Current Status

The mod is implemented as a server-side code mod for Vintage Story 1.22.0 and targets .NET 10.

Implemented:

- Server-only ore hiding during chunk generation and existing chunk loading.
- Hidden ore metadata storage in map chunk mod data.
- Chunk-indexed hidden ore lookup.
- Height-gated cave ore hiding.
- Reveal on block break/place exposure.
- Hardened pre-break connected-vein reveal.
- Explosion reveal hardening for exact and nearby hidden ore.
- Player proximity reveal for air-exposed cave ore.
- Optional prospecting pick node-radius reveal.
- Admin commands under `/antixray`.
- Session-only admin chat debug reveal diagnostics.
- Unit test coverage for core services and command formatting.

## Requirements

- Vintage Story server 1.22.0.
- .NET 10 SDK for building from source.
- `VintagestoryAPI.dll`, `VintagestoryServer.dll`, `VintagestoryLib.dll`, available at:

```text
%APPDATA%\Vintagestory\
```

The mod is server-side only. Players do not need to install it.

## Build And Package

From the repository root:

```powershell
dotnet test -c Release
```

The release build automatically creates:

```text
AntiXray/bin/Release/AntiXray-1.0.0.zip
```

Install that zip into the server Mods folder, for example:

```text
%APPDATA%\VintagestoryData\Mods\AntiXray-1.0.0.zip
```

Then restart the Vintage Story server.

## Configuration

The server config is generated at startup as:

```text
AntiXray.json
```

Vintage Story stores mod config through its normal server config path. Restart the server after changing config values.

Default config:

```json
{
  "HideAirExposedOreBelowY": 100,
  "RevealOreOnProspectingPick": false,
  "ProspectingPickRevealRadius": 8,
  "ProspectingPickCodePatterns": ["prospectingpick-*"],
  "BlockBreakRevealDistance": 3,
  "BlockBreakRevealConnectedVein": true,
  "BlockBreakRevealMaxOreBlocks": 128,
  "RevealAirExposedOreNearPlayers": true,
  "PlayerAirExposedOreRevealDistance": 24,
  "PlayerAirExposedOreRevealMaxPerEvent": 64,
  "AdminCommandDefaultRadius": 16,
  "AdminCommandMaxRadius": 64,
  "ConvertExistingChunks": true,
  "OreCodePatterns": ["ore-*"],
  "PlaceholderFallbackBlockCode": "",
  "DebugLoggingEnabled": false
}
```

### Config Reference

| Option | Default | Meaning |
| --- | ---: | --- |
| `HideAirExposedOreBelowY` | `100` | Air-exposed ore below this world Y is hidden during conversion. Air-exposed ore at or above this Y remains visible. Fully enclosed ore is hidden regardless of Y. |
| `RevealOreOnProspectingPick` | `false` | Enables opt-in prospecting pick reveal support. This weakens anti-xray because revealed ore becomes real ore visible to clients. |
| `ProspectingPickRevealRadius` | `6` | Propick reveal radius, clamped by code to `0..8`. |
| `ProspectingPickCodePatterns` | `["prospectingpick-*"]` | Item code patterns treated as prospecting picks. |
| `BlockBreakRevealDistance` | `3` | Pre-break reveal seed distance, clamped to `1..3`. |
| `BlockBreakRevealConnectedVein` | `true` | Reveals the connected same-ore hidden vein when a nearby block is broken. |
| `BlockBreakRevealMaxOreBlocks` | `128` | Maximum connected hidden ore blocks revealed from one block-break trigger. |
| `RevealAirExposedOreNearPlayers` | `true` | Reveals air/fluid-exposed hidden ore when a player joins or changes chunks near it. |
| `PlayerAirExposedOreRevealDistance` | `24` | Player proximity reveal distance, clamped to `8..32`. |
| `PlayerAirExposedOreRevealMaxPerEvent` | `64` | Maximum proximity reveals per join/chunk-transition event. |
| `AdminCommandDefaultRadius` | `16` | Default `/antixray` radius when omitted. |
| `AdminCommandMaxRadius` | `64` | Maximum admin command radius. |
| `ConvertExistingChunks` | `true` | Converts already-existing chunk columns when they load and have not been processed before. |
| `OreCodePatterns` | `["ore-*"]` | Block code path patterns classified as ore. |
| `PlaceholderFallbackBlockCode` | `""` | Optional fallback placeholder block code if host rock cannot be resolved. Empty means no fallback. |
| `DebugLoggingEnabled` | `false` | Enables bounded server log warnings for resolver/debug logging. This is separate from `/antixray debug`. |

## Admin Commands

All admin commands are under:

```text
/antixray
```

The command root requires `Privilege.root` and player execution.

| Command | Description |
| --- | --- |
| `/antixray inspect [radius]` | Summarizes hidden ore metadata around the admin and reports the targeted block when available. |
| `/antixray hidden [radius]` | Lists hidden ore metadata within radius around the admin. Long output is truncated. |
| `/antixray count [radius]` | Counts hidden ore metadata within radius around the admin. |
| `/antixray reveal [radius]` | Force-reveals hidden ore within radius around the admin. |
| `/antixray hide [radius]` | Debug command that re-hides real ore within radius around the admin. This hides all real ore in radius, regardless of normal height/exposure policy. |
| `/antixray debug on` | Enables session-only reveal diagnostics in chat for the executing admin. |
| `/antixray debug off` | Disables session-only reveal diagnostics for the executing admin. |
| `/antixray debug status` | Shows whether chat reveal diagnostics are enabled for the executing admin. |

Command output uses player-facing map coordinates, not large internal world coordinates. Internal coordinates are still used for all lookups, persistence, and block operations.

## Reveal Behavior

Hidden ore is restored to its original ore block only through bounded server-side triggers.

### Chunk Conversion

During world generation or first load of an unprocessed existing chunk column:

1. Ore blocks matching `OreCodePatterns` are detected.
2. A placeholder host rock is resolved from the ore block code.
3. The original ore block code is stored server-side.
4. The world block is replaced with the placeholder block.
5. The column is marked as processed so revealed ore is not re-hidden later.

### Height-Gated Cave Ore Policy

Conversion follows this policy:

- Fully enclosed ore is always hidden.
- Ore adjacent to air/fluid is hidden only when its Y is below `HideAirExposedOreBelowY`.
- Ore adjacent to air/fluid at or above the cutoff remains visible.

This lets servers hide cave-exposed ore underground while still allowing visible ore in mountain sides above the cutoff.

### Block Break Hardening

Before a player breaks a block, the mod scans only the configured bounded radius around the selected block.

If hidden ore is found:

- the selected hidden ore is revealed before drops are calculated, or
- a nearby connected same-ore vein is revealed up to `BlockBreakRevealMaxOreBlocks`.

This prevents one-block-at-a-time reveal behavior and avoids the bug where hidden ore could be mined as a placeholder and then restored afterward.

### Post Break And Placement Exposure

After block break/place, the mod checks neighboring positions and reveals hidden ore that is now exposed to air/fluid.

### Explosion Reveal

Hidden ore explosion handling is attached to rock placeholder blocks.

If an explosion reaches hidden ore directly:

- the hidden ore vein is restored first,
- the placeholder explosion path is prevented,
- the restored ore block receives explosion handling.

If an explosion destroys nearby stone:

- the mod checks a small bounded radius,
- reveals nearby connected hidden ore,
- lets normal non-hidden stone explosion behavior continue.

### Player Proximity Reveal

When enabled, player join and chunk-transition events check nearby hidden ore candidates from chunk-indexed data. Only hidden ore that is within range and currently exposed to air/fluid is revealed.

There are no per-tick movement scans.

### Prospecting Pick Reveal

When `RevealOreOnProspectingPick` is enabled, legitimate server-side prospecting pick use reveals hidden ore in a bounded radius around the prospected block.

This is disabled by default because revealed ore becomes real ore in client-visible chunks and can then be seen by Block Overlay.

## Architecture

The mod is intentionally split into focused services.

Important areas:

```text
AntiXray/
├── Commands/
│   ├── AdminDebugCommandRegistrar.cs
│   ├── AdminDebugCommandHandler.cs
│   ├── AdminOreHideService.cs
│   ├── AdminRadiusQuery.cs
│   ├── OreRevealDebugReporter.cs
│   └── WorldCoordinateDisplayFormatter.cs
├── Config/
│   ├── AntiXrayConfig.cs
│   └── AntiXrayConfigLoader.cs
├── Models/
│   ├── BlockPosKey.cs
│   ├── ChunkPos.cs
│   └── HiddenOreData.cs
├── ModSystem/
│   └── AntiXraySystem.cs
└── Systems/
    ├── ChunkOreConverter.cs
    ├── HiddenOreManager.cs
    ├── HiddenOrePersistence.cs
    ├── HiddenOreVeinCollector.cs
    ├── OreBlockClassifier.cs
    ├── OrePlaceholderResolver.cs
    ├── RevealService.cs
    └── reveal trigger services
```

### Core Services

| Service | Responsibility |
| --- | --- |
| `AntiXraySystem` | Server-side mod entry point and event registration. |
| `ChunkOreConverter` | Converts ore to placeholders during generation/load. |
| `HiddenOreManager` | Single source of truth for hidden ore metadata. |
| `HiddenOrePersistence` | Saves/loads hidden ore metadata through map chunk mod data. |
| `ChunkIndex` | Keeps hidden ore lookup scoped by chunk. |
| `RevealService` | Restores hidden ore to real ore and removes metadata. |
| `BlockBreakRevealService` | Pre-break reveal hardening. |
| `HiddenOreExplosionRevealService` | Explosion exact/near reveal handling. |
| `PlayerProximityRevealService` | Join/chunk-transition cave reveal. |
| `ProspectingRevealService` | Optional propick radius reveal. |
| `AdminOreHideService` | Debug-only re-hide real ore in a bounded admin radius. |

## Persistence And Data Integrity

Hidden ore metadata stores only:

- block position
- original ore block code

The intended invariant is:

```text
hidden ore metadata exists -> world block should be placeholder
hidden ore is revealed -> metadata is removed
processed column with zero hidden ore -> remains marked processed
```

This prevents already-revealed ore from being hidden again after chunk reload or server restart.

## Performance Model

The mod is event-driven and bounded.

It does not:

- scan the entire world,
- run per-tick ore checks,
- inspect client packets,
- require client-side code.

Higher-cost operations are deliberately limited:

- chunk conversion scans one chunk column when generated or first loaded,
- admin commands scan only a clamped radius,
- block-break and explosion hardening scan only small bounded radii,
- proximity reveal queries chunk-indexed candidates only.

## Security And Tradeoffs

AntiXray protects against client-side scanners by removing real ore IDs from client-visible chunks. It does not try to detect client mods.

Important tradeoffs:

- Once ore is revealed, it is real ore again and visible to clients.
- Optional prospecting pick reveal weakens anti-xray reliability by design.
- Admin `/antixray hide` is a debug override and can hide visible ore intentionally.
- Placeholder resolution depends on ore block code conventions or configured fallback behavior.
- Existing world conversion only applies as chunks load; already-loaded or unprocessed areas need live validation.

## Testing

Run unit tests:

```powershell
dotnet test -c Release
```

Current test coverage includes:

- config defaults and clamps,
- ore classification,
- placeholder host rock resolution,
- hidden ore storage and persistence,
- reveal services,
- block-break hardening,
- explosion reveal behavior,
- player proximity reveal,
- prospecting pick reveal,
- admin command formatting and radius queries,
- admin debug chat session behavior.

## Manual Validation Checklist

For live server validation:

1. Start the server with the mod installed.
2. Join with a client running Block Overlay.
3. Confirm underground hidden ore is not visible through Block Overlay.
4. Break blocks near known hidden ore and confirm the connected vein reveals before mining bugs occur.
5. Test cave-exposed ore below and above `HideAirExposedOreBelowY`.
6. Test player proximity reveal by approaching exposed cave-wall hidden ore.
7. Test explosions near hidden ore.
8. Test `/antixray count`, `/antixray hidden`, `/antixray inspect`, `/antixray reveal`, `/antixray hide`.
9. Enable `/antixray debug on` and confirm reveal reasons appear only for the enabling admin.
10. Restart the server and confirm revealed ore stays revealed and hidden ore remains hidden.

See `ProductionReadinessManualTestScript.md` for a more formal validation script.

## Development Rules

This project is performance-sensitive.

Required rules:

- No packet interception.
- No client-side logic.
- No global world scans.
- No per-tick reveal loops.
- Keep all reveal and hide operations bounded.
- Route hidden ore metadata through `HiddenOreManager`.
- Route real ore restoration through `RevealService`.
- Keep classes focused.

## Known Remaining Risks

High-value remaining validation areas:

- live-server stress testing under chunk generation/load,
- thread safety under worldgen pressure,
- engine event coverage for fluids, cave-ins, and falling blocks,
- long-running server persistence behavior.

## License

Apache License 2.0
