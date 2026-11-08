# MultiSerVIsion.Solution.Domain 技术文档

> 状态：骨架（内容随对话增量补充）

## 1. 职责与边界

- **做**：纯业务模型、领域枚举、业务规则校验、状态流转；不得依赖 UI 与具体 SDK。
- **不做**：不做持久化实现、不调用硬件 SDK、不处理界面逻辑。

## 2. 目录与关键类型

| 目录 | 关键类型 | 说明 |
|---|---|---|
| `Entities` | `DeviceEntity`（抽象基类）、`CameraEntity` | 组态设备实体；`CameraEntity` 持有 `CameraAllConfig` |
| `Entities\Configs` | `CameraConnectConfig`、`CameraParamConfig`、`ParamSpec`、`ParamValueType`、`ParamGroup`、`CameraParamNames`、`CameraParameterSpec`、`ParameterValueType` | 连接参数、参数值、参数规格（范围/步长/选项） |
| `Enums` | `CameraStatus`、`DeviceConnectionStatue`、`AcquisitionMode` | `AcquisitionMode`：连续采集 / 软触发，二者互斥 |
| `Models` | `CameraFrame`、`PixelFormatEnum`、`CameraDeviceDto`、`ParamReadResult` | `CameraFrame` 为跨层统一帧（托管独立拷贝，无 SDK 依赖） |
| `Sources` | `IFrameSource`、`IFrameSourceProvider`、`IFrameBufferPool`、`FrameSourceState` | 统一图源契约（含拷贝语义） |
| `Services` | `CameraDomainService`、`ICameraDeviceService` | 相机业务规则与状态流转 |
| `Repositories` | `ICameraHardwareDriver`、`IDeviceManager`、`IDeviceRepository` | 面向接口的契约（实现位于 Infrastructure） |
| `Contexts` | `DeviceContext`、`IDeviceContext` | 会话层「当前选中设备 + 连接状态」唯一状态源 |

## 3. 依赖方向

- 允许：Domain → Shared
- **禁止**：Domain → Infrastructure / Presentation（曾存在 `using static ...HikCameraHardwareDriver` 的分层倒置，需持续清理）
- `ICameraHardwareDriver` 位于 Domain 但返回 `HikDescriptor`（Infrastructure 类型），属已知妥协点。

## 4. 既有约定

- **状态职责分工**：`CameraEntity.DetailStatus`（`[JsonIgnore]`，默认 `Idle`）= 相机领域权威状态；`DeviceEntity.ConnectionStatus` = 通用连接标志；`DeviceContext` = 会话层唯一状态源。
- **`CameraStatus.Idle` 语义**：表示「尚未连接过」，领域校验中与 `Disconnected` 同属「可连接」。
- **双层参数校验同源**：UI 限幅与实体校验共用同一份参数规格（`ParamSpec` / `CameraParameterSpec`），避免两处规则分叉。
- **参数规格随实体持久化**：连接成功后回读并写入实体，使未连接相机时也能限幅与校验。
- **采集模式互斥**：`AcquisitionMode` 为连续/软触发二选一，采流中禁止直接切换，须先停流。
- **字段拼写（既有，勿改）**：`DeviceEntity.GroupTage`（T-age）。

## 5. 已知问题 / 待办

- `CameraStatus.Idle` 与 `Disconnected` 语义仍有重叠，`Error`/`Fault`/`Offline` 在 UI 未区分处理。
- `Domain\Extensions\CameraMappingExtensions.cs` 命名空间为 `...Shared.Extensions`，与所在目录不一致。
- 存在多处被注释的死代码（如 `CameraDomainService.CreateEntitiesFromScan`、`ApplyStreamStatus`）。

## 6. 变更记录

| 日期 | 变更 |
|---|---|
| 本次对话 | 建立骨架；登记参数规格持久化、双层校验、图源契约、采集模式互斥等既有约定 |
