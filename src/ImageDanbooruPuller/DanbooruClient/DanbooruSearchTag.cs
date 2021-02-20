using System;
using System.Diagnostics.CodeAnalysis;

namespace ImageDanbooruPuller
{
    public class DanbooruSearchTag : System.Collections.Generic.IEqualityComparer<DanbooruSearchTag>
    {
        public readonly string Value;
        public readonly bool IncludedTagInSearch;

        public DanbooruSearchTag(string value, bool includedTagInSearch = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Нужно предоставить значение тэга");
            }

            Value = value;
            IncludedTagInSearch = includedTagInSearch;
        }

        public bool Equals(DanbooruSearchTag x, DanbooruSearchTag y)
        {
            if (x is null || y is null)
            {
                return false;
            }

            return x.IncludedTagInSearch == y.IncludedTagInSearch;
        }

        public int GetHashCode([DisallowNull] DanbooruSearchTag obj) => obj.Value.GetHashCode();

    }
}
