# # 🎌 Daily Anime Discord Bot (.NET)

A Discord bot developed with .NET and Discord.Net that:

- Posts daily anime schedule
- Automatically translates synopsis to Turkish
- Sends rich embed messages with images
- Supports manual command triggering

## 🚀 Features

- 📅 Daily automatic anime schedule (09:00 TR)
- 🌍 English → Turkish translation (Google Translate API)
- 🖼 Large anime images
- ⭐ Score, episode count, broadcast time
- 📢 Channel selection via command

## 🛠 Technologies Used

- .NET
- Discord.Net
- Newtonsoft.Json
- HttpClient
- Jikan API (MyAnimeList)
- Google Translate (Unofficial endpoint)

---

## 📌 Commands

| Command | Description |
|---------|------------|
| `!kanal` | Sets the current channel for daily posts |
| `!anime` | Manually posts today's anime schedule |

---

## ⚙ Setup

### 1️⃣ Install .NET SDK

Install .NET 6 / 7 / 8 from:

https://dotnet.microsoft.com/download

---

### 2️⃣ Set Bot Token (IMPORTANT)

Do NOT hardcode your token.

Instead, set environment variable:

#### Windows (PowerShell):

```
setx DISCORD_TOKEN "YOUR_TOKEN_HERE"
```

Then restart your IDE.

---

### 3️⃣ Run the Bot

```
dotnet restore
dotnet run
```

---

## 🔐 Security Note

Never upload your bot token to GitHub.

If exposed:
1. Go to Discord Developer Portal
2. Reset the token immediately

---

## 📡 APIs Used

- Jikan API (https://api.jikan.moe)
- Google Translate public endpoint

---

## 📄 License

This project is for educational purposes.
