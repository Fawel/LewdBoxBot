using ImagePuller.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImagePuller.Repositories
{
    public interface IChosenImageRepository<TComparablePicture>
    {
        Task<bool> IsImageChosenBeforeAsync(TComparablePicture image, CancellationToken token = default);
        Task AddImageToChosenAsync(TComparablePicture image, CancellationToken token = default);
    }
}
