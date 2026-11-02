# QuanChat: design (brief, draft)

Status: draft, 2 Nov 2026. Nothing built. Valheim 1.0.21.

## Purpose

A chat mod for the Quanheim server with a feature set close to **BetterChat 1.6.4** (Crystal), fixing what bugs us
most about vanilla chat. Further ideas come later.

## What it does (agreed 2 Nov 2026)

1. **Shout is the default.** Typing a message and pressing Enter shouts it to everyone, with no `/s` needed each
   time. Other channels use a prefix for that one message: `/say` for local talk, `/w <name>` for a whisper.
2. **No forced capitals on shouts** (and no forced lowercase on whispers). Messages appear as typed.
3. **Quick to start.** One key opens chat ready to shout, and `/` opens it with the slash already typed for a
   command or another channel.
4. **You never miss a message.** When anyone sends a message, the chat window appears on every player's screen
   straight away, without pressing Enter, then fades out after a set time (configurable). New messages bring it back
   and restart the timer. Optionally it can stay visible all the time.
5. **Scrolling.** Page Up and Page Down scroll back through chat history while the window is showing.
6. **Whispers** work as in BetterChat, with configurable talk and whisper distances. Nice to have rather than
   essential.

From BetterChat we'd also keep, as settings: click-through on the chat window, and hiding the map ping a shout makes.

## How it would work (outline)

- Mostly client-side: the default channel, capitals, the window's visibility and fading, and scrolling all happen on
  each player's PC.
- Settings that affect other players (talk and whisper distances, shout pings) are server-synced and admin-locked,
  as in our other mods. Required on every client, version-checked like the rest of the modpack.

## Later ideas (parked)

Timestamps, private messages to anyone online with `/r` to reply, chat history kept between sessions, a server chat
log, mentions with a sound, server announcements, a Discord bridge, colours, and filters for join, leave and death
messages.

## Where I'd like your input

1. **"Quick to start".** I've read point C as: chat opens ready to shout, and people type `/say` or `/w` when they
   want something else. Is that what you meant, or did you mean the input box should open with `/s` already typed?
2. **Fade time.** How long should the window stay up after the last message? (BetterChat's setting is a number of
   seconds; something like 10–15 s is typical.)
