using System;
using System.Net.Http;
using DiscrordHookImagePusher;
using FileChosenImageRepository;
using ImageDanbooruPuller;
using LBox;
using LBox.Application;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MihaZupan;
using Microsoft.Extensions.Configuration;

namespace LBox.Console
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appconfig.json")
                .AddUserSecrets<Program>()
                .Build();

            IOptions<MemoryCacheOptions> options = new MemoryCacheOptions();
            IMemoryCache memoryCache = new MemoryCache(options);

            var vpnIp = config.GetSection("VpnConfigs:Ip").Value;
            var vpnPort = config.GetSection("VpnConfigs:Port").Value;
            var vpnLogin = config.GetSection("VpnConfigs:Login").Value;
            var vpnPassword = config.GetSection("VpnConfigs:Password").Value;

            var proxy = new HttpToSocks5Proxy(vpnIp, int.Parse(vpnPort), vpnLogin, vpnPassword);
            var handler = new HttpClientHandler { Proxy = proxy };
            var danbooruHttpClient = new HttpClient(handler, true);

            var danbooruLogin = config.GetSection("DanbooruAccount:Login").Value;
            var danbooruApiKey = config.GetSection("DanbooruAccount:ApiKey").Value;
            var settings = new DanbooruAuthenticationSettings(danbooruLogin, danbooruApiKey);

            var danbooruApiClient = new DanbooruApiClient(danbooruHttpClient, settings);
            var imagePuller = new DanbooruImagePuller(danbooruApiClient, memoryCache);

            var discrordHttpClient = new HttpClient();

            var discordHookServerName = config.GetSection("DiscordWebHook:ServerName").Value;
            var discordHookChannel = config.GetSection("DiscordWebHook:ChannelName").Value;
            var discordHookId = config.GetSection("DiscordWebHook:Id").Value;
            var discordHookToken = config.GetSection("DiscordWebHook:Token").Value;

            var swampChannelSettings = new DiscordWebHookSettings(
                discordHookServerName,
                discordHookChannel,
                discordHookId,
                discordHookToken);

            var imagePusher = new DiscrordWebhookImagePusher(discrordHttpClient, swampChannelSettings);

            var historyPath = config.GetSection("FileImageRepository:RelativeFileHistoryPath").Value;
            var imageRepository = new FileImageRepository(historyPath);

            var lewdBoxApp = new LewdBox(imagePuller, imagePusher, imageRepository);
            await lewdBoxApp.PostNewHotPictureAsync(5);
        }
    }
}
