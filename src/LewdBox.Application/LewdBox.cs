using ImagePuller.Core;
using ImagePuller.Repositories;
using ImagePusher.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LBox.Shared;

namespace LBox.Application
{
    public class LewdBox
    {
        private readonly IImagePuller<HotWebImage> _imagePuller;
        private readonly IImagePusher _imagePusher;
        private readonly IChosenImageRepository<HotWebImage> _chosenImageRepository;

        private readonly NewHotImageRestrictions _imageRestrictions;

        public LewdBox(
            IImagePuller<HotWebImage> imagePuller,
            IImagePusher imagePusher,
            IChosenImageRepository<HotWebImage> chosenImageRepository)
        {
            _imagePuller = imagePuller ?? throw new ArgumentNullException(nameof(imagePuller));
            _imagePusher = imagePusher ?? throw new ArgumentNullException(nameof(imagePusher));
            _chosenImageRepository = chosenImageRepository ?? 
                throw new ArgumentNullException(nameof(chosenImageRepository));

            _imageRestrictions = _imagePusher.GetPushSourceRestrictions();
        }

        /// <summary>
        /// Ищем новую хотную пикчу и стараемся её отослать
        /// </summary>
        public async Task PostNewHotPictureAsync(
            int picturesToPost, 
            CancellationToken token = default)
        {
            if(picturesToPost < 1)
            {
                return;
            }

            var ignoredImages = new List<HotWebImage>();
            var chosenImages = new List<HotWebImage>();

            while (IsMustPostMore(chosenImages.Count, picturesToPost))
            {
                var imageNeeded = picturesToPost - chosenImages.Count;

                // Дабы не тягать по много раз из источника, постараемся вытянуть побольше пикчей, 
                // чтобы шанс того, что они не повторялись был больше
                var images = await _imagePuller.GetNewHotImageAsync(
                    _imageRestrictions,
                    ImageSizeToGetFromPool(imageNeeded), 
                    ignoredImages, 
                    token);

                foreach(var image in images)
                {
                    ignoredImages.Add(image);

                    if (await _chosenImageRepository.IsImageChosenBeforeAsync(image, token))
                    {
                        continue;
                    }

                    chosenImages.Add(image);
                    await _chosenImageRepository.AddImageToChosenAsync(image, token);

                    var imageToPush = new WebImage(image.ImageUrl);
                    await _imagePusher.PushImageAsync(imageToPush);

                    if(!IsMustPostMore(chosenImages.Count, picturesToPost))
                    {
                        break;
                    }
                }
            }

            static bool IsMustPostMore(int postedPictureCount, int targetPictureCount)
                => targetPictureCount > postedPictureCount;

            static int ImageSizeToGetFromPool(int pictureCount)
                => pictureCount switch
                {
                    < 10 => 10,
                    >= 10 and <= 50 => pictureCount * 2,
                    > 50 and <= 100 => (int)Math.Floor(pictureCount * 1.5),
                    >= 100 => (int)Math.Floor(pictureCount * 1.2)
                };
        }
    }
}
