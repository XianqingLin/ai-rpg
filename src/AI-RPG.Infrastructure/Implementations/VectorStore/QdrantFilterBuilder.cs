using Qdrant.Client.Grpc;

namespace AI_RPG.Infrastructure.Implementations.VectorStore;

/// <summary>
/// Qdrant 过滤条件构建器
/// </summary>
public static class QdrantFilterBuilder
{
    /// <summary>
    /// 构建Qdrant过滤条件
    /// </summary>
    public static Filter BuildFilter(Dictionary<string, object> filter)
    {
        var conditions = new Filter();

        foreach (var (key, value) in filter)
        {
            conditions.Must.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = key,
                    Match = CreateMatch(value)
                }
            });
        }

        return conditions;
    }

    /// <summary>
    /// 创建匹配条件
    /// </summary>
    private static Match CreateMatch(object value) => value switch
    {
        string s => new Match { Keyword = s },
        int i => new Match { Integer = i },
        long l => new Match { Integer = l },
        bool b => new Match { Boolean = b },
        _ => new Match { Keyword = value.ToString() ?? string.Empty }
    };
}
