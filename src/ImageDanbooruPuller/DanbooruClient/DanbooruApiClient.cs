using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDanbooruPuller
{
    public class DanbooruApiClient
    {
        public readonly HttpClient _httpClient;
        public readonly DanbooruAuthenticationSettings _authSettings;
        public readonly ILogger<DanbooruApiClient> _logger;

        public DanbooruApiClient(
            HttpClient httpClient,
            DanbooruAuthenticationSettings authSettings = null,
            ILogger<DanbooruApiClient> logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authSettings = authSettings;
            _logger = logger;
        }

        public async Task<ImageMetadata[]> GetPageOfImagesAsync(
            GetImagesFilter filter,
            CancellationToken token = default)
        {
            var url = GetSearchQueryBuilder()
                .AddSearchTags(filter.Tags)
                .AddOrderParameter(filter.OrderTag)
                .AddRating(filter.SearchRating)
                .AddPageParameter(filter.Page)
                .Build();

            var images = await GetImagesInnerAsync(url, token);

            _logger?.LogTrace($"Got {images.Length} images");

            return images;
        }

        public async Task<ImageMetadata> GetImageAsync(
            int pictureId,
            CancellationToken token = default)
        {
            var url = GetSearchQueryBuilder()
                .AddIdFilter(pictureId)
                .Build();

            var images = await GetImagesInnerAsync(url, token);

            _logger?.LogTrace($"Image found: {images is not null}");

            return images.FirstOrDefault();
        }

        private DanbooruSearchQueryBuilder GetSearchQueryBuilder() =>
            new DanbooruSearchQueryBuilder(_authSettings);

        private async Task<ImageMetadata[]> GetImagesInnerAsync(
            Uri url,
            CancellationToken token = default)
        {
            var response = await _httpClient.GetAsync(url, token);
            var responseString = await response.Content.ReadAsStringAsync(token);

            var imageMetadatas = JsonConvert.DeserializeObject<ImageMetadata[]>(responseString);

            var imageWithMd5 = imageMetadatas.Where(x => !string.IsNullOrWhiteSpace(x.MD5))
                .ToArray();

            _logger?.LogTrace($"Find {imageMetadatas.Length} images, with MD5 - {imageWithMd5.Length}");

            return imageWithMd5;
        }
    }
}
