using AI_RPG.Application.DTOs;

namespace AI_RPG.Blazor.Data;

/// <summary>
/// 默认跑团剧本 - 迷雾小镇
/// </summary>
public static class DefaultScenario
{
    /// <summary>
    /// 剧本背景设定
    /// </summary>
    public static class Setting
    {
        public const string Title = "迷雾小镇";
        public const string Genre = "悬疑恐怖";
        public const string Theme = "调查与生存";
        public const string WorldDescription = "你来到了一个被永恒迷雾笼罩的偏僻小镇。这里的时间仿佛停滞在19世纪末，煤气灯在雾中摇曳，街道上几乎看不到行人。最近，镇上发生了几起离奇的失踪案，而当你踏入这片土地时，发现迷雾似乎有自己的意志，它在观察你，等待你...";
    }

    /// <summary>
    /// 初始场景
    /// </summary>
    public static SceneDto InitialScene => new()
    {
        Name = "小镇入口",
        Description = "你站在小镇的入口处，一块锈迹斑斑的路牌上写着\"威尔镇\"。迷雾从四面八方涌来，几乎看不清十米外的景物。远处传来若有若无的钟声，还有某种你无法辨认的低语。"
    };

    /// <summary>
    /// 预设NPC列表
    /// </summary>
    public static List<AddNPCRequest> DefaultNPCs => new()
    {
        new AddNPCRequest
        {
            Name = "老镇长·亨利",
            Appearance = "一位白发苍苍的老者，穿着过时的黑色燕尾服，拄着一根雕花木杖。他的眼睛深陷，目光却异常锐利，仿佛能看透人心。",
            Personality = "表面上和蔼可亲，说话慢条斯理，但总是避重就轻。对小镇的秘密守口如瓶，却又似乎在暗示什么。在关键时刻会变得异常紧张。",
            Background = "威尔镇的镇长，据说已经在这个位置上坐了四十年。他见证了小镇的兴衰，也知晓迷雾的秘密。有传言说他与失踪案有关，但从未有人找到证据。"
        },
        new AddNPCRequest
        {
            Name = "神秘女子·艾拉",
            Appearance = "一位年轻女子，穿着深灰色的斗篷，半张脸隐藏在兜帽阴影中。她的眼睛呈现出不自然的银灰色，在黑暗中似乎会微微发光。",
            Personality = "冷漠疏离，说话简短而神秘。她似乎对迷雾有着独特的了解，偶尔会在关键时刻给出隐晦的警告。对陌生人充满戒心，但如果获得她的信任，可能会得到重要线索。",
            Background = "没有人知道她从哪里来，她五年前突然出现在小镇，住在镇外的废弃教堂里。有人说她能听见迷雾中的声音，有人说她就是迷雾的化身。她似乎一直在寻找某个特定的人。"
        }
    };

    /// <summary>
    /// AI主持人说明（由后端自动创建，不是NPC）
    /// </summary>
    public static class AIHost
    {
        public const string Name = "守密人";
        public const string Description = "AI主持人由系统自动创建，负责推动剧情发展，描述环境和NPC反应。它是迷雾的化身，知晓所有秘密但只会逐步揭示。";
    }

    /// <summary>
    /// 创建默认会话请求
    /// </summary>
    public static CreateSessionRequest CreateSessionRequest() => new()
    {
        Title = Setting.Title,
        Genre = Setting.Genre,
        Theme = Setting.Theme,
        WorldDescription = Setting.WorldDescription,
        InitialScene = InitialScene
    };
}
