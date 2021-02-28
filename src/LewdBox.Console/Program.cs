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
using ImagePuller.Core;
using ImagePusher.Core;
using ImagePuller.Repositories;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog;

namespace LBox.Console
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appconfig.json")
                .AddUserSecrets<Program>()
                .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
            ILoggerFactory loggerFactory = new NLogLoggerFactory();
            var programLogger = loggerFactory.CreateLogger<Program>();

            try
            {
                var imagePuller = GetImagePuller(config, loggerFactory);
                var imagePusher = GetImagePusher(config, loggerFactory);
                var imageRepository = GetImageRepository(config, loggerFactory);

                var lewdBoxApp = new LewdBox(imagePuller, imagePusher, imageRepository);

                var imageCount = int.Parse(config.GetSection("LewdBoxConfig:PicturePerPushCount").Value);

                await lewdBoxApp.PostNewHotPictureAsync(imageCount);
            }
            catch (Exception ex)
            {
                programLogger.LogError(ex, string.Empty);
            }
        }

        private static IImagePuller<HotWebImage> GetImagePuller(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
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

            var apiLogger = loggerFactory.CreateLogger<DanbooruApiClient>();
            var danbooruApiClient = new DanbooruApiClient(danbooruHttpClient, settings, apiLogger);

            var imagePullerLogger = loggerFactory.CreateLogger<DanbooruImagePuller>();
            var imagePuller = new DanbooruImagePuller(danbooruApiClient, memoryCache, imagePullerLogger);

            return imagePuller;
        }

        private static IImagePusher GetImagePusher(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
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

            var discordHookPusherLog = loggerFactory.CreateLogger<DiscrordWebhookImagePusher>();

            var imagePusher = new DiscrordWebhookImagePusher(
                discrordHttpClient,
                discordHookPusherLog,
                swampChannelSettings);

            return imagePusher;
        }

        private static IChosenImageRepository<HotWebImage> GetImageRepository(
            IConfigurationRoot config,
            ILoggerFactory loggerFactory)
        {
            var historyPath = config.GetSection("FileImageRepository:RelativeFileHistoryPath").Value;
            var imageRepository = new FileImageRepository(historyPath);
            return imageRepository;
        }
    }
}
