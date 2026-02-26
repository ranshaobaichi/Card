---
applyTo: "Assets/**/*.cs"
---

## Unity C# Runtime Code Review Rules

⚠️ 只评审本路径下的 C# 文件。

---

### 一、性能与 GC 重点检查

如果代码位于：

- Update / LateUpdate / FixedUpdate
- 战斗 Tick
- 输入循环
- 高频触发逻辑

必须检查：

- 是否使用 LINQ
- 是否频繁 new List/Dictionary
- 是否字符串拼接
- 是否闭包捕获变量
- 是否装箱拆箱
- 是否不必要的 ToList / ToArray

如发现分配，必须指出。

---

### 二、事件系统安全

- 所有事件订阅必须匹配取消订阅
- 禁止在 OnEnable 重复订阅
- 禁止匿名 lambda 无法解绑
- 检查是否可能重复注册

---

### 三、Unity 生命周期

- 避免在构造函数写逻辑
- Awake / Start / OnEnable 使用是否合理
- 协程是否在对象销毁时停止

---

### 四、日志与调试

- 禁止在高频路径中保留 Debug.Log
- 若存在调试输出，建议使用条件编译