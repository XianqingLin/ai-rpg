# AI-RPG.Domain

AI-RPG 的领域层，包含核心业务规则和领域模型。

## 项目定位

位于 **AICapabilities 层** 之上，是业务逻辑的核心，不依赖任何外部框架。

```
┌─────────────────────────────────────┐
│         Application Layer           │
│    (AI-RPG.Application)             │
├─────────────────────────────────────┤
│         AI-RPG.Domain               │  ← 本层
│  - 领域实体、值对象、领域服务        │
│  - 领域事件、仓储接口                │
├─────────────────────────────────────┤
│      AI-RPG.AICapabilities          │
├─────────────────────────────────────┤
│         Infrastructure Layer        │
└─────────────────────────────────────┘
```

## 项目结构

```
AI-RPG.Domain/
├── Entities/                        # 领域实体
│   ├── Participant.cs              # 参与者基类
│   ├── Player.cs                   # 玩家
│   ├── NPC.cs                      # NPC
│   ├── NPCProfile.cs               # NPC设定
│   ├── GameMaster.cs               # 主持人
│   ├── Session.cs                  # 会话聚合根
│   └── Enums.cs                    # 枚举定义
├── ValueObjects/                    # 值对象
│   ├── ValueObject.cs              # 值对象基类
│   ├── SessionId.cs                # 会话ID
│   ├── ParticipantId.cs            # 参与者ID
│   ├── GameSetting.cs              # 游戏设定
│   ├── Scene.cs                    # 场景
│   └── DialogueTurn.cs             # 对话回合
├── Events/                          # 领域事件
│   ├── IDomainEvent.cs             # 事件接口
│   ├── SessionEvents.cs            # 会话事件
│   └── DialogueEvents.cs           # 对话事件
├── Services/                        # 领域服务接口
│   └── IDialogueService.cs         # 对话服务
└── Repositories/                    # 仓储接口
    └── ISessionRepository.cs       # 会话仓储
```

## 核心概念

### 会话（Session）

会话是核心聚合根，代表一次完整的跑团游戏：

```csharp
// 创建会话
var session = Session.Create(
    title: "龙之谷的冒险",
    setting: new GameSetting("奇幻", "史诗冒险", "一个充满魔法的世界..."),
    initialScene: new Scene("村口", "你站在一个宁静的小村庄入口...")
);

// 添加参与者
session.AddPlayer(Player.Create("勇者", "user-001"));
session.AddNPC(NPC.Create("村长", new NPCProfile("白发老人", "慈祥", "村子的领导者")));

// 开始会话
session.Start();

// 记录对话
session.RecordDialogue(player.Id, "我想和村长说话", DialogueType.Speech);
```

### 参与者（Participant）

三种类型的参与者：
- **Player**：真实玩家，关联外部用户系统
- **NPC**：AI控制的角色，有角色设定
- **GameMaster**：AI主持人，推动剧情

### 领域事件

重要业务变更会触发领域事件：
- `SessionStarted` / `SessionEnded`
- `PlayerJoined` / `PlayerLeft`
- `NPCEnteredScene` / `NPCLeftScene`
- `DialogueSpoken`

## 设计原则

1. **聚合根边界**：Session 是唯一的聚合根，所有修改通过它进行
2. **值对象不可变**：所有值对象创建后不可修改
3. **领域服务**：复杂业务逻辑通过领域服务接口定义
4. **仓储抽象**：数据持久化通过仓储接口抽象

## 依赖

- 仅依赖 .NET 基础类库
- 无外部框架依赖
