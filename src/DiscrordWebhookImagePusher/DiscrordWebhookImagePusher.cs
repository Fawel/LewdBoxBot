using ImagePusher.Core;
using LBox.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiscrordHookImagePusher
{
    public class DiscrordWebhookImagePusher : IImagePusher
    {
        private readonly HttpClient _httpClient;
        private DiscordWebHookSettings[] _webhooks;
        private readonly int _sendDelay;
        private readonly string _baseDiscordUrlHook = "https://discord.com/api/webhooks";
        private readonly ILogger<DiscrordWebhookImagePusher> _logger;

        public DiscrordWebhookImagePusher(
            HttpClient httpClient,
            int sendDelay,
            ILogger<DiscrordWebhookImagePusher> logger, 
            params DiscordWebHookSettings[] webhooks)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _sendDelay = sendDelay <= 0 ? 1000 : sendDelay;
            _logger = logger;
            _webhooks = webhooks ?? throw new ArgumentNullException(nameof(webhooks));
            if (webhooks.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(webhooks), "Нужно предоставить хотя бы один вебхук");
            }
        }

        public async Task PushImageAsync(WebImage image, CancellationToken token = default)
        {
            foreach (var hook in _webhooks)
            {
                _logger.LogTrace($"Sending image to discord channel {hook.ChannelName}, server {hook.ServerName}");
                var imageToSendContent = new { content = image.Uri.ToString() };
                var json = JsonConvert.SerializeObject(imageToSendContent);
                var httpContent = new StringContent(json);
                httpContent.Headers.ContentType.MediaType = "application/json";

                var url = $"{_baseDiscordUrlHook}/{hook.Id}/{hook.Token}";

                var response = await _httpClient.PostAsync(url, httpContent, token);

                if(!response.IsSuccessStatusCode)
                {
                    var failedRequestMessage = await response.Content.ReadAsStringAsync();

                    _logger.LogError($"Failed to send image to hook, " +
                        $"discord channel {hook.ChannelName}, server {hook.ServerName}. " +
                        $"Status code: {response.StatusCode}," +
                        $"Message: {failedRequestMessage}");
                }

                await Task.Delay(_sendDelay, token);
            }
        }

        public NewHotImageRestrictions GetPushSourceRestrictions()
            => new NewHotImageRestrictions(
                IntNaturalRange.CreateRangeWithMaximumLimit(7_500_000),
                IntNaturalRange.CreateEmptyRange(),
                IntNaturalRange.CreateEmptyRange());
    }

    public class DiscordWebHookSettings : IEquatable<DiscordWebHookSettings>
    {
        public readonly string ServerName;
        public readonly string ChannelName;
        public readonly string Id;
        public readonly string Token;

        public DiscordWebHookSettings(
            string serverName,
            string channelName,
            string id,
            string token)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentException($"Название сервера должно быть указано");
            }

            if (string.IsNullOrWhiteSpace(channelName))
            {
                throw new ArgumentException($"Необходимо имя канала", nameof(channelName));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"Нужно id вебхука дискорда", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException($"Нужен токен вебхука дискорда", nameof(token));
            }
            ServerName = serverName;
            ChannelName = channelName;
            Id = id;
            Token = token;
        }

        public bool Equals(DiscordWebHookSettings other)
        {
            if (this is null || other is null)
            {
                return false;
            }

            return this.ChannelName == other.ChannelName
                && this.Id == other.Id
                && this.Token == other.Token;
        }
    }
}
