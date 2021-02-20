using System;

namespace ImagePusher.Core
{
    /// <summary>
    /// Картинка, размещённая в дерях Интернета
    /// </summary>
    public class WebImage
    {
        public WebImage(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public WebImage(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentException($"Нужно предоставить ссылку на изображение");
            }

            if (!Uri.TryCreate(uri, UriKind.Absolute, out var result))
            {
                throw new ArgumentException($"Не является ссылкой - {uri}");
            }

            Uri = result;
        }

        public Uri Uri { get; set; }
    }
}
