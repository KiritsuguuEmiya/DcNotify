# DcNotify for FINAL FANTASY XIV

A Dalamud plugin to send a Discord Webhook message whenever a Party Finder event or duty pop happens with an AFK client.

Fork of [PushyFinder](https://github.com/snightshade/PushyFinder).

## Install via Plugin Installer

1. In game, open `/xlplugins`
2. Click the **Settings** gear → **Custom Plugin Repositories**
3. Add this URL:
   ```
   https://raw.githubusercontent.com/Kiri12121212/DalamudPlugins/main/pluginmaster.json
   ```
4. Click **Save**, then refresh the plugin list
5. Install **DcNotify**

Shared custom repo: [Kiri12121212/DalamudPlugins](https://github.com/Kiri12121212/DalamudPlugins) (also lists HuntTrainAuto).

## Discord Setup

1. Open your Discord text channel → **Edit Channel** → **Integrations** → **Webhooks**
2. Create a webhook and copy the **Webhook URL**

## In Game

Configure with `/dcn`:

- Paste your **Webhook URL**
- Toggle duty pop notifications and AFK behavior as needed
- Click **Save and close**

Notifications are sent when:

- A player joins your party finder group
- The party becomes full (8/8)
- A player leaves (optional)
- A duty pops (if enabled)

By default, notifications only send while you are **AFK** (`/afk`).

## Dev Install

Build:

```powershell
dotnet build DcNotify/DcNotify.csproj -c Release
```

Add as a dev plugin in XIVLauncher:

```
<PATH_TO_DC_NOTIFY>\DcNotify\bin\Release\DcNotify.dll
```

## Releases

Tagged releases (`v*`) are built automatically via GitHub Actions and published to this repo.
