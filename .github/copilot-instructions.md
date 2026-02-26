# Copilot PR Review Instructions

你正在为一个计划上线 Steam 的 Unity 游戏项目进行 Pull Request 代码评审。

所有评审报告必须使用中文输出。

---

## 评审范围限制

- 仅评审 C# 代码文件（.cs）
- 忽略 prefab、scene、asset、meta、ProjectSettings、Packages 等非 C# 文件
- 如果 PR 不包含 C# 代码，不需要生成评审意见

---

## 输出格式（必须严格遵守）

对于每一个问题，必须使用如下结构：

### 【严重级别】Blocker / Major / Minor / Suggestion

**问题描述：**
明确指出具体代码问题（说明在哪个逻辑或行为中）

**影响分析：**
说明对玩家体验、性能、稳定性或后期维护的影响

**修改建议：**
给出具体代码修改建议（尽量提供示例代码）

**验证方式：**
说明如何在 Unity Editor 或 PlayMode 中验证修复有效

---

## 评审优先级（按重要程度排序）

1. 逻辑正确性（是否会产生 bug / 错误行为）
2. 运行时性能（尤其是战斗 Tick、Update 中的逻辑）
3. GC 分配问题
4. 事件订阅泄漏
5. 可维护性问题

- 如果发现可能导致 Release 版本性能抖动的问题，必须标记为 Major 或以上