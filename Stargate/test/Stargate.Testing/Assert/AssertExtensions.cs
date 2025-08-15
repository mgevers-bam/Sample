using System.Linq.Expressions;

namespace Stargate.Testing.Assert;

public static class AssertExtensions
{
    public static void DeepEqual<T>(T expected, T actual, bool includeNonPublicProperties = false)
    {
        DeepEqualWithBlacklist(
            expected: expected,
            actual: actual,
            includeNonPublicProperties: includeNonPublicProperties);
    }

    public static void DeepEqualWithBlacklist<T>(
        T expected,
        T actual,
        params Expression<Func<T, object>>[] blacklistProperties)
    {
        DeepEqualWithBlacklist(
            expected: expected,
            actual: actual,
            includeNonPublicProperties: false,
            blacklistProperties: blacklistProperties);
    }

    public static void DeepEqualWithBlacklist<T>(
        T expected,
        T actual,
        bool includeNonPublicProperties,
        params Expression<Func<T, object>>[] blacklistProperties)
    {
        string expectedJson = DeepEqualSerializer.SerializeObject(
            value: expected,
            blacklistProperties: blacklistProperties,
            includeNonPublicProperties: includeNonPublicProperties);

        string actualJson = DeepEqualSerializer.SerializeObject(
            value: actual,
            blacklistProperties: blacklistProperties,
            includeNonPublicProperties: includeNonPublicProperties);

        Xunit.Assert.Equal(expected: expectedJson, actual: actualJson);
    }
}
