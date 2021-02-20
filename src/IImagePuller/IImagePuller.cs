using LBox.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImagePuller.Core
{
    /// <summary>
    /// Интерфейс для получения картинок из источника
    /// </summary>
    public interface IImagePuller<TComparablePicture>
        where TComparablePicture : BasicComparablePicture
    {
        /// <summary>
        /// Находит свежую картинку из источника
        /// Картинка должна быть уникальна и никогда не передаваться ранее
        /// </summary>
        Task<TComparablePicture[]> GetNewHotImageAsync(
            NewHotImageRestrictions restrictions,
            int pictureCount = 1,
            IEnumerable<TComparablePicture> imagesToIgnore = default,
            CancellationToken token = default);
    }

    public abstract class BasicComparablePicture
    {
        public readonly string ImageMD5;

        protected BasicComparablePicture(string imageMD5)
        {
            if (string.IsNullOrWhiteSpace(imageMD5))
            {
                throw new ArgumentException($"MD5 картинки должен быть высчитан");
            }

            ImageMD5 = imageMD5;
        }

        public virtual bool IsSamePicture(BasicComparablePicture otherPicture)
        {
            if (this is null
                || otherPicture is null
                || string.IsNullOrWhiteSpace(this.ImageMD5)
                || string.IsNullOrWhiteSpace(otherPicture.ImageMD5))
            {
                return false;
            }

            return this.ImageMD5 == otherPicture.ImageMD5;
        }
    }

    public class HotWebImage : BasicComparablePicture
    {
        public readonly Uri ImageUrl;

        public HotWebImage(string imageMD5, Uri imageUrl) : base(imageMD5)
        {
            ImageUrl = imageUrl;
        }
    }
}
