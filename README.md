# QuanChat

Valheim chat mod for the Quanheim server: shout by default, sticky `/s` and `/say` channels, messages as typed, a
chat window that fades instead of vanishing (or stays on screen), scrolling without typing, hanging indent, and no
shout map pings.

- What it does and its settings: [Package/README.md](Package/README.md) (the Thunderstore page).
- **What's agreed, what's next, and the test script: [docs/design.md](docs/design.md).**
- Building and releasing: [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md).
- Game code it relies on: [docs/valheim-dependencies.md](docs/valheim-dependencies.md).

## Code

| File | What it does |
|---|---|
| [Plugin.cs](Plugin.cs) | Start-up: skips everything on a dedicated server, otherwise settings and Harmony. |
| [Settings.cs](Settings.cs) | The settings, all personal. |
| [Channels.cs](Channels.cs) | Default and sticky channels, and the channel shown in the chat input. |
| [ChatWindow.cs](ChatWindow.cs) | Window fade, idle opacity, hiding, click-through, scrolling without typing. |
| [WindowLayout.cs](WindowLayout.cs) | Window size and position, even background, font size. |
| [tools/render_package_icon.py](tools/render_package_icon.py) | Draws `Package/icon.png`. Run with `python tools/render_package_icon.py`. |
| [MessageLayout.cs](MessageLayout.cs) | Hanging indent, the gap between messages, fitting messages to the window. |
| [Messages.cs](Messages.cs) | Messages as typed, jump to the newest line, no shout map pings. |
