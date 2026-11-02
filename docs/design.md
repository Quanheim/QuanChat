# QuanChat: design (brief, draft)

Status: draft, 2 Nov 2026. Nothing built. Valheim 1.0.21.

## Purpose

A chat mod for the Quanheim server with a feature set close to **BetterChat 1.6.4** (Crystal), fixing what bugs us
most about vanilla chat. Further ideas come later.

## What vanilla does today (from `Chat` and `Terminal`)

- **Three message types:** normal talk (`/say`, reaches players nearby only), shout (`/s`, reaches everyone) and
  whisper (`/w`, quieter local talk; despite its help text it isn't a private message to a named player).
- **The default is normal talk**, so an untagged message only reaches players in range. Far-away players never see
  it. That's most likely why messages get missed: they were never delivered, not hidden.
- **Shouts are forced to capitals** and whispers to lowercase (`Chat`, when it shows a message).
- **New messages already pop the chat window up** for 10 seconds (`Chat.m_hideDelay`), except pings.
- **Tab completion exists:** chat runs on the console's command system, which completes commands and their
  arguments, filtered by what you've typed so far (`Terminal`, `ConsoleCommand` option lists).

## What it does (agreed 2 Nov 2026)

1. **Shout is the default channel**, so chat is server-wide unless you choose otherwise. Typing a message and
   pressing Enter shouts it.
2. **Sticky channels, World of Warcraft style.**
   - `/say hello` sends one message as local talk and leaves your channel as it was.
   - `/say` on its own, then Enter, **switches** your channel: every untagged message after that goes as local talk,
     until you switch again (`/s` back to shout).
   - The current channel shows beside the chat input, so you always know where a message will go.
   - The server sets the default channel each player starts with on login (config, shout by default).
3. **No forced capitals on shouts** (and no forced lowercase on whispers). Messages appear as typed.
4. **You don't miss messages.** Any new message brings the chat window up on every player's screen, without pressing
   Enter, and it stays for **30 seconds** after the latest message (configurable), long enough to read after a
   fight. Optionally it can stay visible all the time.
5. **Scrolling.** Page Up and Page Down scroll back through chat history while the window is showing.
6. **Private messages, nice to have:** `/w <name> <message>` sends a message to one player wherever they are.
   - **Tab completion of names:** after `/w `, Tab cycles through the players online now, and typing the first
     letters narrows the list first. This reuses vanilla's tab completion, with the online player list as the
     options, so it's very doable.
   - This takes over `/w` from vanilla's quiet local whisper, which we don't use. Delivery goes through the server to
     that one player.

From BetterChat we'd also keep, as settings: click-through on the chat window, configurable talk distance, and
hiding the map ping a shout makes.

## How it would work (outline)

- Mostly client-side: channels, capitals, the window's visibility and fading, scrolling and tab completion all
  happen on each player's PC.
- Server side: the default channel at login, talk distance and shout pings (server-synced and admin-locked, as in our
  other mods), and routing private messages to the right player. Required on every client, version-checked like the
  rest of the modpack.

## Later ideas (parked)

Timestamps, `/r` to reply to the last private message, chat history kept between sessions, a server chat log,
mentions with a sound, server announcements, a Discord bridge, colours, and filters for join, leave and death
messages.

## Open

- Exact layout of the channel indicator beside the input.
