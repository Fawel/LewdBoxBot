namespace ImageDanbooruPuller
{
    public class GetImagesFilter
    {
        public readonly int Page;
        public readonly DanbooruSearchTag[] Tags;
        public readonly DanbooruNSFWRating SearchRating;
        public readonly ImageOrder OrderTag;

        private readonly static DanbooruSearchTag[] _emptyTagList =
            new DanbooruSearchTag[0];

        public GetImagesFilter(
            int page,
            DanbooruSearchTag[] tags,
            DanbooruNSFWRating searchRating,
            ImageOrder orderBy)
        {
            Page = page;
            Tags = tags ?? _emptyTagList;
            SearchRating = searchRating;
            OrderTag = orderBy;
        }

        public GetImagesFilter(
            int page,
            DanbooruSearchTag tag,
            DanbooruNSFWRating searchRating,
            ImageOrder orderBy) : this(page, new[] { tag }, searchRating, orderBy)
        {

        }

        public readonly static GetImagesFilter NoFilter =
            new GetImagesFilter(1, _emptyTagList, DanbooruNSFWRating.NoRating, ImageOrder.NoOrder);
    }
}
