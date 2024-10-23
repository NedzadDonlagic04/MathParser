using System.Collections.ObjectModel;

namespace MathParser.Core
{
    public static class Dictionary
    {
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary
        ) where TKey : notnull => new(dictionary);
    }
}