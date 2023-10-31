using System.Collections.Generic;

namespace Utilities.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(value, default);
    }
}