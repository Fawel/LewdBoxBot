using ImagePuller.Core;
using LBox.Shared;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ImageDanbooruPuller.DanbooruApiClient;

namespace ImageDanbooruPuller
{
    public class DanbooruImagePuller : IImagePuller<HotWebImage>
    {
        private readonly DanbooruApiClient _danbooruApiClient;
        private readonly IMemoryCache _memoryCache;
        private const int _imageSizeLimit = 7_500_000;

        public DanbooruImagePuller(DanbooruApiClient danbooruApiClient, IMemoryCache memoryCache)
        {
            _danbooruApiClient = danbooruApiClient;
            _memoryCache = memoryCache;
        }

        private readonly static DanbooruSearchTag[] _ignoreTags = new DanbooruSearchTag[]
        {
            new("loli", false),
            new("furry", false)
        };

        public async Task<HotWebImage[]> GetNewHotImageAsync(
            NewHotImageRestrictions restrictions,
            int pictureCount = 1,
            IEnumerable<HotWebImage> imagesToIgnore = default,
            CancellationToken token = default)
        {
            var result = new List<HotWebImage>(pictureCount);

            var localImagesToIgnore = new List<HotWebImage>(imagesToIgnore);

            int currentPage = 1;
            while (result.Count < pictureCount)
            {
                ImageMetadata[] images = await GetImagesAsync(currentPage, token);

                // картинка может быть дочерней от другой, а они в свою очередь могут повторяться
                // поэтому вытаскиваем родителей, если они подходят под условия, то используем их,
                // а не оригинальную пикчу. Если под теги не подходит => оставляем исходно-найденную
                images = await TryGetImagesWithoutParentAsync(imagesToIgnore, images, token);

                // проверяем на ограничения источника

                images = images.Where(x => restrictions.CheckImageParameters(x.ImageSize))
                    .ToArray();

                var chosenImage = ChoseNonIgnoredImages(localImagesToIgnore, images, CurrentPictureNeeds());

                var foundHotWebImage = chosenImage.Select(x => new HotWebImage(x.MD5, x.FileUri))
                    .ToArray();

                localImagesToIgnore.AddRange(foundHotWebImage);

                foreach (var foundImage in foundHotWebImage)
                {
                    result.Add(foundImage);
                }

                if (result.Count < pictureCount)
                {
                    currentPage++;
                }
            }

            return result.ToArray();

            int CurrentPictureNeeds() => pictureCount - result.Count;
        }

        private async Task<ImageMetadata[]> GetImagesAsync(int page, CancellationToken token)
        {
            // сначала лезем в кэш, а уж потом лезет реально в коробку

            if (_memoryCache.TryGetValue(GetPageCacheKey(page), out ImageMetadata[] result))
            {
                return result;
            }

            var searchFilter = new GetImagesFilter(
                                page,
                                _ignoreTags,
                                DanbooruNSFWRating.Explicit,
                                ImageOrder.Rank);

            var images = await _danbooruApiClient.GetPageOfImagesAsync(searchFilter, token);

            _memoryCache.Set(1, images, new TimeSpan(0, 5, 0));

            return images;
        }

        private string GetPageCacheKey(int page) => $"page_{page}";
        private string GetImageCacheKey(int imageId) => $"imageId_{imageId}";

        private async Task<ImageMetadata[]> TryGetImagesWithoutParentAsync(
            IEnumerable<HotWebImage> imagesToIgnore,
            ImageMetadata[] images,
            CancellationToken token = default)
        {
            var imagesWithoutParent = new List<ImageMetadata>(images.Length);
            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];

                if (!image.ParentId.HasValue)
                {
                    imagesWithoutParent.Add(image);
                    continue;
                }

                var parentId = image.ParentId.Value;
                ImageMetadata parentImage = await GetParentImageAsync(parentId, token);

                if (parentImage.Rating != DanbooruNSFWRating.Explicit
                    || parentImage.Tags.Contains("loli")
                    || parentImage.Tags.Contains("furry"))
                {
                    continue;
                }

                var parentHotWebImage = new HotWebImage(parentImage.MD5, parentImage.FileUri);
                if (imagesToIgnore.Contains(parentHotWebImage))
                {
                    continue;
                }

                imagesWithoutParent.Add(parentImage);
            }

            return imagesWithoutParent.ToArray();
        }

        private async Task<ImageMetadata> GetParentImageAsync(int parentId, CancellationToken token = default)
        {
            if (_memoryCache.TryGetValue(GetImageCacheKey(parentId), out ImageMetadata parentImage))
            {
                return parentImage;
            }

            parentImage = await _danbooruApiClient.GetImageAsync(parentId, token);

            _memoryCache.Set(GetImageCacheKey(parentId), parentImage);
            return parentImage;
        }

        private ImageMetadata[] ChoseNonIgnoredImages(
            IEnumerable<HotWebImage> imagesToIgnore,
            ImageMetadata[] images,
            int pictureCountMax)
        {
            if (imagesToIgnore is null || !imagesToIgnore.Any())
            {
                return images.Take(pictureCountMax)
                    .ToArray();
            }

            var result = new List<ImageMetadata>(pictureCountMax);

            for (int i = 0; i < images.Length && pictureCountMax > result.Count; i++)
            {
                var currentImage = images[i];

                if (!imagesToIgnore.Any(x => x.ImageMD5 == currentImage.MD5))
                {
                    result.Add(currentImage);
                }
            }

            return result.ToArray();
        }
    }
}
