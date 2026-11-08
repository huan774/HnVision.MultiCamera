# MultiSerVIsion.Solution.Application 技术文档

> 状态：骨架（内容随对话增量补充）

## 1. 职责与边界

- **做**：编排用例（扫描、组态、连接、断开、取流、参数读写）、做跨层映射、发布领域事件。
- **不做**：不直接操作 SDK，不处理 UI。

## 2. 目录与关键类型

| 目录 | 关键类型 | 说明 |
|---|---|---|
| `Services` | `ICameraAppService` / `CameraApplicationoService` | 相机用例编排（注意实现类名既有拼写 `CameraApplicationoService`） |
| `Services` | `IDeviceAppService` / `DeviceAppService` | 设备组态用例（增删改查、工程存取） |
| `Services` | `AlgorithmAppService` | 算法配方服务（**当前为空壳**） |
| `Interfaces` | `IOperatorRegistry`、`IRecipeValidator` | 算法模块契约 |
| `Dtos` | `CameraDeviceDto` 等 | 应用层数据传输对象 |
| `DeviceAppService` | `SaveProject` / `LoadProject` | 调用 `IDeviceManager.SaveToStorage/LoadFromStorage` 触发落盘/加载 |

## 3. 依赖方向

- 允许：Application → Domain / Shared
- **禁止**：Application → Presentation（现存 `DeviceConnectionChangedEveent` 等事件类位于 Presentation 并被 Application 引用，属已知倒置，需择机迁移）

## 4. 既有约定

- **参数下发链路**：UI 限幅 → `CameraDomainService.ValidateParameter` 二次校验 → 驱动下发 → 成功后写实体并 `_deviceManager.Update` 落盘（自动保存）。
- **参数持久化策略**：首次连接以相机为准回读并落盘；已有持久化配置（`ParamSpecs` 非空）时以实体值为准下发，避免每次连接被 SDK 默认值覆盖。
- **在线设备 vs 组态设备职责分离**：在线设备仅做「测试连接」（登录即释放，不占句柄）；组态设备才做正式长连接。
- **参数读取返回规格+值**：`ReadParamAsync` 返回 `ParamReadResult`（规格 + 当前值），避免只回值而丢失范围/可选项。
- **接口参数名**：`ICameraAppService` 参数形参名为 `deviceId`（组态设备 ID），非序列号。

## 5. 已知问题 / 待办

- `ConnectOnlineCamera` 已无调用方，与 `TestConnectAsync` 语义重复。
- `ConnectCamera` 发布 `DeviceConnectionChangedEveent`，而 `DisconnectCamera` 未发布 → 事件不对称。
- `UpdataDevice` 未做校验，且仅改内存/缺落盘（与 `CreateDevice` 不一致）。
- `AlgorithmAppService` 等算法侧服务为骨架，未接入。

## 6. 变更记录

| 日期 | 变更 |
|---|---|
| 本次对话 | 建立骨架；登记参数下发与持久化策略、在线/组态职责分离、已知事件不对称问题 |
