using System;
using System.Linq;
using System.Text;

namespace ImageDanbooruPuller
{
    internal class DanbooruSearchQueryBuilder
    {
        private static readonly string _baseUrl = "http://danbooru.donmai.us/posts.json";

        private static readonly string _tagsQueryKey = "tags";
        private static readonly string _pageQueryKey = "page";
        private static readonly string _ratingTagQueryKey = "rating";
        private static readonly string _orderTagQueryKey = "order";
        private static readonly string _idTagQueryKey = "id";

        private static readonly string _loginQueryKey = "login";
        private static readonly string _apiKeyQueryKey = "api_key";

        private UriBuilder _currentUrl;

        public DanbooruSearchQueryBuilder(DanbooruAuthenticationSettings authSettings = null)
        {
            _currentUrl = new UriBuilder(_baseUrl);

            AddAuthToBaseUrl(authSettings);
        }

        public Uri Build() => _currentUrl.Uri;

        public DanbooruSearchQueryBuilder AddSearchTags(params DanbooruSearchTag[] tags)
        {
            var tagValue = new StringBuilder();
            foreach (var tag in tags)
            {
                var value = $"{tag.Value} ";

                if (!tag.IncludedTagInSearch)
                {
                    value = $"-{value}";
                }

                tagValue.Append(value);
            }

            AddValueToTag(_tagsQueryKey, tagValue.ToString(), _currentUrl);

            return this;
        }
        public DanbooruSearchQueryBuilder AddIdFilter(int id, bool isInclude = true)
        {
            if(id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id картинки с данбору - положительное число");
            }

            var searchTagValue = $"{_idTagQueryKey}:" + id.ToString();

            var searchTag = new DanbooruSearchTag(searchTagValue, isInclude);
            return AddSearchTags(searchTag);
        }

        public DanbooruSearchQueryBuilder AddRating(DanbooruNSFWRating rating, bool isInclude = true)
        {
            if (rating == DanbooruNSFWRating.NoRating)
            {
                return this;
            }

            var searchTagValue = $"{_ratingTagQueryKey}:" + rating switch
            {
                DanbooruNSFWRating.Safe => "s",
                DanbooruNSFWRating.Questionable => "q",
                DanbooruNSFWRating.Explicit => "e",
                _ => throw new NotSupportedException($"Значение {rating} не поддерживается")
            };

            var searchTag = new DanbooruSearchTag(searchTagValue, isInclude);
            return AddSearchTags(searchTag);
        }

        public DanbooruSearchQueryBuilder AddPageParameter(int page)
        {
            if (IsHaveParameter(_pageQueryKey, _currentUrl))
            {
                throw new ArgumentException($"Параметр page уже добавлен в ссылку");
            }

            page = page < 1 ? 1 : page;
            AddValueToTag(_pageQueryKey, page.ToString(), _currentUrl);
            return this;
        }

        public DanbooruSearchQueryBuilder AddOrderParameter(ImageOrder order, bool isInclude = true)
        {
            if (order == ImageOrder.NoOrder)
            {
                return this;
            }

            var orderTagValue = $"{_orderTagQueryKey}:" + order switch
            {
                ImageOrder.Rank => "rank",
                _ => throw new NotSupportedException($"Неподдерживаемая сортировка {order}")
            };

            var orderTag = new DanbooruSearchTag(orderTagValue, isInclude);
            return AddSearchTags(orderTag);
        }

        private void AddValueToTag(string parameter, string tagValue, UriBuilder uriBuilder)
        {
            var queryBuilder = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            if (queryBuilder.AllKeys.Contains(parameter))
            {
                queryBuilder[parameter] += tagValue.ToString();
            }
            else
            {
                queryBuilder.Add(parameter, tagValue.ToString());
            }

            uriBuilder.Query = queryBuilder.ToString();
        }

        private void AddAuthToBaseUrl(DanbooruAuthenticationSettings authSettings)
        {
            if(authSettings is null)
            {
                return;
            }

            AddValueToTag(_loginQueryKey, authSettings.Login, _currentUrl);
            AddValueToTag(_apiKeyQueryKey, authSettings.ApiKey, _currentUrl);
        }

        private bool IsHaveParameter(string parameterName, UriBuilder uriBuilder)
        {
            var queryBuilder = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            return queryBuilder.AllKeys.Contains(parameterName);
        }
    }
}
