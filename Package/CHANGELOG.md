# Changelog

## 0.2.7 - 2026-11-08

- `/shout` works like `/s`: on its own it switches you to shout; with text it sends one shout.

## 0.2.6 - 2026-11-08

- New defaults: *Always show* on, *Idle opacity* 36, *Width* 130, *Height* 115, *Space between messages* off.
  They apply to new config files; an existing one keeps its values (use each setting's Reset to take the default).

## 0.2.5 - 2026-11-08

- Page Up and Page Down scroll 3 messages at a time, typing or not (the game: 1, and only while typing).

## 0.2.4 - 2026-11-07

- Messages can be up to 200 characters (the game stops at 127).

## 0.2.3 - 2026-11-07

- The chat stays on screen while the mod settings window (Configuration Manager) is open, even over the Esc menu,
  so setting changes show as you make them.
- Typed text in the chat input keeps the same side padding as the messages.

## 0.2.2 - 2026-11-07

- Fixed: in 0.2.1 the chat box sat half off the bottom-right of the screen. It now sits in the corner with an even
  gap.

## 0.2.1 - 2026-11-07

- Fixed: in 0.2.0 the chat window filled the whole width of the screen. It's back to the game's size by default,
  in the bottom-right corner.

## 0.2.0 - 2026-11-07

- The chat window's background is one even shade instead of a gradient, so it reads the same at idle opacity.
- New *Width* and *Height* settings (percent of the game's size); the window stays anchored at the bottom right,
  now with an even gap below and to the right.
- Messages fit the window: the oldest drop off the top instead of spilling above it. A taller window shows more.
- New *Font size* setting; the input box grows with it.

## 0.1.0 - 2026-11-07

- Proof of concept, for testing on the Quanheim server.
- Shout is the default channel; `/s` and `/say` on their own switch channel, with text they send one message. The
  chat input shows your channel. Personal *Starting channel* setting.
- Messages appear as typed: no forced capitals on shouts or lowercase on whispers.
- The chat window fades after 30 seconds instead of vanishing after 10, or stays at an idle opacity with *Always
  show*. Hidden with the HUD, in the menu and under the big map. Click-through while not typing.
- Page Up / Page Down scroll chat without opening it.
- Hanging indent and a small gap between messages.
- Shouts no longer flash a pin on the map (setting, on by default).
- Does nothing on a dedicated server beyond the version check, and says so in the server log.
