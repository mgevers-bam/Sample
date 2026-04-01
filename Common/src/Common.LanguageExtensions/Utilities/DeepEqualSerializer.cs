namespace Common.LanguageExtensions.Utilities;

using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public static class DeepEqualSerializer
{
    public static string SerializeObject<T>(
        T value,
        IReadOnlyCollection<Expression<Func<T, object>>>? blacklistProperties,
        bool includeNonPublicProperties = false)
    {
        blacklistProperties ??= [];

        IContractResolver contractResolver = new BlacklistPropertiesContractResolver<T>(blacklistProperties, includeNonPublicProperties);

        var jsonSerializerSettings = new JsonSerializerSettings() {
            ContractResolver = contractResolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        string rawJson = JsonConvert.SerializeObject(value: value, settings: jsonSerializerSettings);
        return JsonConvert.SerializeObject(NormalizeJToken(JToken.Parse(rawJson)));
    }

    private static JToken NormalizeJToken(JToken jToken)
    {
        return jToken switch
        {
            JObject jObject => NormalizeJObject(jObject),
            JArray jArray => NormalizeJArray(jArray),
            _ => jToken
        };
    }

    private static JObject NormalizeJObject(JObject jObject)
    {
        var result = new JObject();
        foreach (JProperty property in jObject.Properties().OrderBy(p => p.Name))
        {
            result.Add(property.Name, NormalizeJToken(property.Value));
        }
        return result;
    }

    private static JArray NormalizeJArray(JArray jArray)
    {
        var result = new JArray();
        foreach (JToken item in jArray)
        {
            result.Add(NormalizeJToken(item));
        }
        return result;
    }
}
