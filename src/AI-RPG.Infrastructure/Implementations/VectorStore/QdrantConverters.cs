using Qdrant.Client.Grpc;
using AI_RPG.Infrastructure.Services;

namespace AI_RPG.Infrastructure.Implementations.VectorStore;

/// <summary>
/// Qdrant 类型转换器
/// </summary>
public static class QdrantConverters
{
    /// <summary>
    /// 转换负载数据为Qdrant格式
    /// </summary>
    public static Dictionary<string, Value> ConvertPayload(Dictionary<string, object> payload)
    {
        var result = new Dictionary<string, Value>();
        foreach (var (key, value) in payload)
        {
            result[key] = ConvertToValue(value);
        }
        return result;
    }

    /// <summary>
    /// 将对象转换为Qdrant Value
    /// </summary>
    public static Value ConvertToValue(object? value) => value switch
    {
        null => new Value { NullValue = NullValue.NullValue },
        string s => new Value { StringValue = s },
        int i => new Value { IntegerValue = i },
        long l => new Value { IntegerValue = l },
        float f => new Value { DoubleValue = f },
        double d => new Value { DoubleValue = d },
        bool b => new Value { BoolValue = b },
        IEnumerable<string> strList => new Value
        {
            ListValue = new ListValue { Values = { strList.Select(s => new Value { StringValue = s }) } }
        },
        IEnumerable<int> intList => new Value
        {
            ListValue = new ListValue { Values = { intList.Select(i => new Value { IntegerValue = i }) } }
        },
        IEnumerable<long> longList => new Value
        {
            ListValue = new ListValue { Values = { longList.Select(l => new Value { IntegerValue = l }) } }
        },
        IEnumerable<float> floatList => new Value
        {
            ListValue = new ListValue { Values = { floatList.Select(f => new Value { DoubleValue = f }) } }
        },
        IEnumerable<double> doubleList => new Value
        {
            ListValue = new ListValue { Values = { doubleList.Select(d => new Value { DoubleValue = d }) } }
        },
        _ => new Value { StringValue = value.ToString() ?? string.Empty }
    };

    /// <summary>
    /// 从Qdrant Value转换为对象
    /// </summary>
    public static object ConvertFromValue(Value value) => value.KindCase switch
    {
        Value.KindOneofCase.StringValue => value.StringValue,
        Value.KindOneofCase.IntegerValue => value.IntegerValue,
        Value.KindOneofCase.DoubleValue => value.DoubleValue,
        Value.KindOneofCase.BoolValue => value.BoolValue,
        Value.KindOneofCase.ListValue => value.ListValue.Values.Select(ConvertFromValue).ToList(),
        Value.KindOneofCase.NullValue => null!,
        _ => value.ToString() ?? string.Empty
    };

    /// <summary>
    /// 将Qdrant ScoredPoint转换为VectorPoint
    /// </summary>
    public static VectorPoint ConvertToVectorPoint(ScoredPoint point) => new()
    {
        Id = point.Id.Uuid,
        Vector = point.Vectors?.Vector?.GetDenseVector()?.Data.ToArray() ?? [],
        Payload = point.Payload.ToDictionary(kv => kv.Key, kv => ConvertFromValue(kv.Value))
    };

    /// <summary>
    /// 将Qdrant RetrievedPoint转换为VectorPoint
    /// </summary>
    public static VectorPoint ConvertToVectorPoint(RetrievedPoint point) => new()
    {
        Id = point.Id.Uuid,
        Vector = point.Vectors?.Vector?.GetDenseVector()?.Data.ToArray() ?? [],
        Payload = point.Payload.ToDictionary(kv => kv.Key, kv => ConvertFromValue(kv.Value))
    };

    /// <summary>
    /// 将Qdrant搜索结果转换为SearchResult
    /// </summary>
    public static SearchResult ConvertToSearchResult(ScoredPoint point) => new()
    {
        Id = point.Id.Uuid,
        Score = point.Score,
        Vector = point.Vectors?.Vector?.GetDenseVector()?.Data.ToArray(),
        Payload = point.Payload.ToDictionary(kv => kv.Key, kv => ConvertFromValue(kv.Value))
    };
}
