# MultiSerVIsion.Solution.Shared 技术文档

> 状态：骨架（内容随对话增量补充）

## 1. 职责与边界

- **做**：提供全层可用的基础类型与工具（结果模型、校验结果、日志、常量、扩展）。
- **不做**：不包含业务规则，不依赖任何业务层。

## 2. 目录与关键类型

| 目录 | 关键类型 | 说明 |
|---|---|---|
| `Models` | `OperationResult` / `OperationResult<T>` | 统一操作结果（`Success` / `Message` / `Data`，成功用 `Succes()` 既有拼写） |
| `Models` | `ValidationResult` | 统一校验结果（`Success()` / `Failure(msg)` / `IsValid`） |
| `Helpers` | `LogHelper` | 日志（`Info` / `Error`） |
| `Helpers` | `GlobalConst` | 全局常量（如 `AppDataFolder`） |
| `Extensions` | `CameraMappingExtensions` | 扫描结果 → 实体的映射（命名空间置于 Shared，与目录 Domain\Extensions 不一致） |

## 3. 依赖方向

- 允许：Shared → 无（最底层，只依赖 BCL）

## 4. 既有约定

- **`LogHelper.Error(content, ex)` 的异常参数允许为 null**：内部已做空值处理（`ex?.Message`/`StackTrace`）。此前调用方传 `null` 曾抛 `NullReferenceException` 并掩盖真实失败原因。
- **`ValidationResult` 同名冲突**：Shared 的 `ValidationResult` 与算法模块 `IRecipeValidator` 内定义的同名类型冲突，使用处需明确命名空间。
- **`OperationResult` 成功方法拼写为 `Succes()`**（既有多处调用，勿改）。

## 5. 已知问题 / 待办

- `CameraMappingExtensions` 目录与命名空间不一致，且含被注释的旧映射方法（死代码）。
- 算法模块的 `ValidationResult` 归属未最终确定（继续放 Shared 还是收拢到算法命名空间）。

## 6. 变更记录

| 日期 | 变更 |
|---|---|
| 本次对话 | 建立骨架；登记 LogHelper 空值契约、ValidationResult 同名冲突、OperationResult 既有拼写 |
