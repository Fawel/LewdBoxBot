using ImagePuller.Core;
using ImagePuller.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileChosenImageRepository
{
    public class FileImageRepository : IChosenImageRepository<HotWebImage>
    {
        private readonly string _absoluteFileHistoryPath;
        private readonly Dictionary<char, ICollection<string>> _imageHistoryCache =
            new Dictionary<char, ICollection<string>>();

        public FileImageRepository(string relativeFileHistoryPath)
        {
            if (string.IsNullOrWhiteSpace(relativeFileHistoryPath))
            {
                throw new ArgumentException("Необходимо предоставить путь к файлу с историей избранных пикч");
            }

            var absoulteFilePath = Path.GetFullPath(relativeFileHistoryPath);

            if (!File.Exists(absoulteFilePath))
            {
                throw new ArgumentNullException(
                    nameof(absoulteFilePath),
                    $"Файл с историей не найден по указанному пути");
            }

            _absoluteFileHistoryPath = absoulteFilePath;

            InitCache(_absoluteFileHistoryPath);
        }

        private void InitCache(string filePath)
        {
            using var reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                var md5 = reader.ReadLine();
                AddMd5ToCache(md5);
            }
        }

        private void AddMd5ToCache(string md5)
        {
            var firstChar = md5[0];
            if (!_imageHistoryCache.ContainsKey(firstChar))
            {
                _imageHistoryCache.Add(firstChar, new List<string>());
            }

            _imageHistoryCache[firstChar].Add(md5);
        }

        public async Task AddImageToChosenAsync(HotWebImage image, CancellationToken token = default)
        {
            using StreamWriter writer = new StreamWriter(_absoluteFileHistoryPath, true);
            await writer.WriteLineAsync(image.ImageMD5.AsMemory(), token);

            AddMd5ToCache(image.ImageMD5);
        }

        public Task<bool> IsImageChosenBeforeAsync(HotWebImage image, CancellationToken token = default)
        {
            var firstMd5Char = image.ImageMD5[0];
            if (!_imageHistoryCache.TryGetValue(firstMd5Char, out var collection))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(collection.Any(x => x == image.ImageMD5));
        }
    }
}
