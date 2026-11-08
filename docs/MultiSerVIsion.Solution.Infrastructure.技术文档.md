# MultiSerVIsion.Solution.Infrastructure 技术文档

> 状态：骨架（内容随对话增量补充）

## 1. 职责与边界

- **做**：实现 Domain 契约；封装 SDK/IO/持久化细节；对外只暴露统一业务类型。
- **不做**：不承载业务规则（校验/流转属 Domain）。

## 2. 目录与关键类型

| 目录 | 关键类型 | 说明 |
|---|---|---|
| `HiKHardware` | `HikCameraHardwareDriver` | 海康相机驱动（主动取流模式） |
| `HiKHardware` | `HikCameraSession` | 单台相机会话（句柄、模式、取流状态、图源、会话级锁） |
| `HiKHardware` | `HikCameraFrameSource` / `FrameBufferPool` | 图源实现与帧缓冲池 |
| `HiKHardware` | `HikParamName` / `HikDescriptor` | 参数节点名常量与参数描述符（含范围/步长/枚举项） |
| `HiKHardware` | `CameraHardwareRawDto` | SDK 原始信息 → 业务 DTO 中间模型 |
| `Repository` | `DeviceRepository` / `DeviceManager` | JSON 持久化 / 内存聚合根 |

## 3. 依赖方向

- 允许：Infrastructure → Domain / Shared
- 不得被 Domain 反向依赖（Domain 中出现的 `using static ...HikCameraHardwareDriver` 属历史问题）

## 4. 既有约定

- **取流方式**：统一「主动取流」（`StartGrabbing` + `GetImageBuffer` 轮询），不使用 SDK 回调取流，二者互斥。
- **帧缓冲**：转换后**深拷贝到托管数组**，脱离 SDK 生命周期；每帧必须 `FreeImageBuffer`。
- **像素转换（易踩坑）**：`MV_PIXEL_CONVERT_PARAM` 源缓冲字段为 **`pSrcData` / `nSrcDataLen`**（旧版 API 为 `pSrcBuffer`/`nSrcBufferLen`，硬编码会 CS1061）。
- **彩色/单色判定**：SDK 转换成功 → 按目标格式（彩色 BGR8 / 单色 Mono8）；转换失败 → 按**原始数据长度**推断格式（1 或 3 字节/像素），否则上层会因长度校验失败整帧丢弃。
- **默认触发模式**：连接后统一置为**连续采集（Trigger 关闭）**，避免持久化残留导致「等外部触发而无图像」。
- **参数读写**：`ReadParamAsync` 返回「规格 + 当前值」；`SetParamAsync` 按值类型选择 SDK API；基础参数（像素格式/触发）写入前自动暂停并恢复取流。
- **多相机隔离**：以序列号为键，每台相机独立会话与取流线程。
- **持久化**：`AppData\Device.json`，`DeviceEntity` 通过 `[JsonDerivedType("Camera")]` 多态序列化；手工编辑删除 `$type` 会导致加载静默丢设备。

## 5. 已知问题 / 待办

- `DeviceRepository.cs` 仍存被注释的旧实现（死代码）。
- 帧分配仍有每帧 `new byte[]`/`new Bitmap` 的开销（**缓冲池已建立但尚未接入转换路径**）。
- `Login` 中默认参数设置与参数面板存在职责重叠风险。

## 6. 变更记录

| 日期 | 变更 |
|---|---|
| 本次对话 | 建立骨架；登记主动取流、像素转换字段坑、彩色格式推断、默认连续采集、图源与缓冲池约定 |
