using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

class Program
{
    private DiscordSocketClient _client;
    private readonly HttpClient _httpClient = new HttpClient();
    private ulong _selectedChannelId = 0;
    private const string ConfigFile = "config.txt";

    private const string BotToken = "TOKEN"; 

    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        LoadConfig();
        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });

        _client.Log += Log;
        _client.Ready += OnReady;
        _client.MessageReceived += HandleCommandAsync;

        await _client.LoginAsync(TokenType.Bot, BotToken);
        await _client.StartAsync();
        await Task.Delay(-1);
    }

    private async Task OnReady()
    {
        Console.WriteLine($"{_client.CurrentUser.Username} aktif! Büyük resim ve TR çeviri devrede.");
        _ = Task.Run(async () =>
        {
            while (true)
            {
                DateTime trNow = DateTime.UtcNow.AddHours(3);
                DateTime nextRun = trNow.Date.AddHours(9);
                if (trNow >= nextRun) nextRun = nextRun.AddDays(1);
                await Task.Delay(nextRun - trNow);
                await PostDailyAnime();
            }
        });
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        if (message == null || message.Author.IsBot) return;
        if (message.Content.ToLower() == "!kanal")
        {
            _selectedChannelId = message.Channel.Id;
            SaveConfig();
            await message.Channel.SendMessageAsync($"✅ Kanal ayarlandı: <#{_selectedChannelId}>");
        }
        else if (message.Content.ToLower() == "!anime") await PostDailyAnime(message.Channel);
    }

    // --- TÜRKÇE ÇEVİRİ FONKSİYONU ---
    private async Task<string> TranslateToTurkish(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=tr&dt=t&q={HttpUtility.UrlEncode(text)}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JArray.Parse(response);
            return string.Join("", json[0].Select(x => x[0].ToString()));
        }
        catch { return text; } // Çeviri başarısız olursa orijinali döndür
    }

    private async Task PostDailyAnime(ISocketMessageChannel targetChannel = null)
    {
        var channel = targetChannel ?? (_client.GetChannel(_selectedChannelId) as IMessageChannel);
        if (channel == null) return;

        DateTime trTime = DateTime.UtcNow.AddHours(3);
        string day = trTime.DayOfWeek.ToString().ToLower();
        string apiUrl = $"https://api.jikan.moe/v4/schedules?filter={day}";

        try
        {
            var response = await _httpClient.GetStringAsync(apiUrl);
            var data = JObject.Parse(response)["data"];

            await channel.SendMessageAsync($"📢 **{trTime:dd/MM/yyyy} - Bugünün Anime Takvimi (TR Çeviri)**");

            foreach (var anime in data.Take(6)) // Büyük resimler çok yer kaplar, 6 adet idealdir
            {
                string title = anime["title"]?.ToString() ?? "Bilinmiyor";
                string imageUrl = anime["images"]?["jpg"]?["large_image_url"]?.ToString();
                string englishSynopsis = anime["synopsis"]?.ToString() ?? "Açıklama yok";
                
                // Çeviri yapılıyor
                string turkishSynopsis = await TranslateToTurkish(englishSynopsis);
                if (turkishSynopsis.Length > 400) turkishSynopsis = turkishSynopsis.Substring(0, 397) + "...";

                string episodes = string.IsNullOrWhiteSpace(anime["episodes"]?.ToString()) ? "Bilinmiyor" : anime["episodes"].ToString();
                string score = string.IsNullOrWhiteSpace(anime["score"]?.ToString()) ? "N/A" : anime["score"].ToString();

                string trDisplayTime = "Belirtilmemiş";
                string jstTimeStr = anime["broadcast"]?["time"]?.ToString();
                if (!string.IsNullOrEmpty(jstTimeStr))
                {
                    TimeSpan jstTime = TimeSpan.Parse(jstTimeStr);
                    TimeSpan trCalc = jstTime.Subtract(TimeSpan.FromHours(6));
                    if (trCalc.Ticks < 0) trCalc = trCalc.Add(TimeSpan.FromHours(24));
                    trDisplayTime = trCalc.ToString(@"hh\:mm");
                }

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithDescription(turkishSynopsis)
                    .WithImageUrl(imageUrl)
                    .AddField("⏰ TR Saati", trDisplayTime, true)
                    .AddField("📺 Bölüm", episodes, true)
                    .AddField("⭐ Puan", score, true)
                    .WithFooter("Çeviri: Google / Kaynak: MyAnimeList")
                    .WithColor(Color.Blue)
                    .Build();

                await channel.SendMessageAsync(embed: embed);
                await Task.Delay(1500);
            }
        }
        catch (Exception ex) { Console.WriteLine($"Hata: {ex.Message}"); }
    }

    private void SaveConfig() => File.WriteAllText(ConfigFile, _selectedChannelId.ToString());
    private void LoadConfig() { if (File.Exists(ConfigFile)) ulong.TryParse(File.ReadAllText(ConfigFile), out _selectedChannelId); }
    private Task Log(LogMessage msg) { Console.WriteLine(msg.ToString()); return Task.CompletedTask; }
}