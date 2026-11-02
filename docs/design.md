# QuanChat: design (brief, draft)

Status: first draft, 2 Nov 2026. Nothing agreed or built. Valheim 1.0.21.

## Purpose

A chat mod for the Quanheim server. It starts from what **BetterChat 1.6.4** (Crystal) does, then adds our own ideas
(still to collect, see below).

## Starting point: BetterChat's features

| Feature | Keep? |
|---|---|
| How long the chat window stays open after a message; option to keep it always visible. | Likely |
| Click through the chat window to the UI behind it. | Likely |
| `/` opens chat with the slash already typed. | Likely |
| Shout is the default channel; `/say` for local talk. | Discuss: see questions |
| No forced capitals on shouts, no forced lowercase on whispers. | Likely |
| Option to hide the map ping a shout makes. | Likely |
| Configurable talk and whisper distances. | Likely, server-set |
| Client-side, with optional server-enforced settings. | We'd make it server-synced and version-checked like our other mods |

## Goals

1. Everything above that we keep, with the server setting anything that affects other players (distances, pings).
2. Our own additions (below).
3. Messages say exactly what happened, for example "Whisper to X failed: not online".

## Out of scope (for now)

- Voice chat.
- Changing in-world speech bubbles beyond what the chat changes imply.

## Where I'd like your input

1. **Your "couple of other ideas".** That's the main gap in this draft. Some common ones, to react to:
   - **Timestamps** on messages (local or server time).
   - **Private messages** to a player by name (`/w Name text`), wherever they are, and `/r` to reply.
   - **Chat history** kept between sessions, and scrollback.
   - **Server log of chat** for moderation, with timestamps that match our other logs.
   - **Mentions:** highlight and a sound when someone types your name.
   - **Server announcements:** restart warnings, events, messages from mods (the chunk loader's limits, QuanCompanions' recall results).
   - **Discord bridge:** chat to and from a Discord channel.
   - **Colours** per player or per channel; emotes.
   - **Filters:** hide join/leave or death messages.
2. **Default channel.** Keep vanilla's local talk as the default, or BetterChat's shout default?
3. **Required for everyone?** If chat features change what others see (private messages, announcements), everyone needs it. Purely cosmetic features could work client-only. I'd lean to required, like our other mods.
4. **Server-side part.** Private messages, logging, history and a Discord bridge all need the server. Which do you want?
