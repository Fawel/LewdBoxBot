using LBox.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace ImagePusher.Core
{
    public interface IImagePusher
    {
        Task PushImageAsync(WebImage image, CancellationToken token = default);
        NewHotImageRestrictions GetPushSourceRestrictions();
    }
}
