using ImageDanbooruPuller.Converters;
using Newtonsoft.Json;
using System;

namespace ImageDanbooruPuller
{
    public class ImageMetadata
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("md5")]
        public string MD5 { get; set; }

        [JsonProperty("file_size")]
        public int ImageSize { get; set; }

        [JsonProperty("file_url")]
        protected Uri CommonFileUri { get; set; }

        [JsonProperty("large_file_url")]
        protected Uri LargeFileUri { get; set; }

        public Uri FileUri
        {
            get
            {
                if(Extensions == "zip" && LargeFileUri.OriginalString.EndsWith("webm"))
                {
                    return LargeFileUri;
                }
                else
                {
                    return CommonFileUri;
                }
            }
        }


        [JsonProperty("file_ext")]
        public string Extensions { get; set; }

        [JsonProperty("tag_string_general"), JsonConverter(typeof(StringToStringArrayConverter), " ")]
        public string[] Tags { get; set; }

        [JsonProperty("tag_string_character"), JsonConverter(typeof(StringToStringArrayConverter), " ")]
        public string[] Characters { get; set; }

        [JsonProperty("tag_string_artist"), JsonConverter(typeof(StringToStringArrayConverter), " ")]
        public string[] Author { get; set; }

        [JsonProperty("rating"), JsonConverter(typeof(StringRatingConverter))]
        public DanbooruNSFWRating Rating { get; set; }

        [JsonProperty("parent_id")]
        public int? ParentId { get; set; }

        [JsonProperty("has_children")]
        public bool HasChildren { get; set; }
    }
}
