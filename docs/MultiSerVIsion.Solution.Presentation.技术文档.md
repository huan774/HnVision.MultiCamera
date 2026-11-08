# MultiSerVIsion.Solution.Presentation 技术文档

> 状态：骨架（内容随对话增量补充）

## 1. 职责与边界

- **做**：视图渲染、用户交互采集、把动作转成事件交给 Presenter；Presenter 编排并回写视图。
- **不做**：View 不引用任何后台具体类（只依赖 `I*View` 契约）；Presenter 不直接操作控件。

## 2. 目录与关键类型

| 目录 | 关键类型 | 说明 |
|---|---|---|
| `Views` | `IVisionView`、`IDeviceDatailView`、`IDeviceTreeView`、`IDeviceInfoParamView`、`IAlgorithmRecipeView` | 视图契约 |
| `UserControls` | `BaseViewUc`、`UCVisionView`、`CameraDateilUC`、`DeviceInfoUC`、`DeviceTreeUC`、`UCMonitorView`、`UCAlgorithmView`(5 partial) | 视图实现 |
| `Presenter` | `VisionPreseter`(既有拼写)、`DeviceInfoPresenter`、`DeviceTressPresenter`、`DeviceDetailPresenter`、`AlgorithmRecipePresenter`(空壳) | 表现逻辑 |
| `Winforms` | `Form1` | 唯一宿主：装配视图与 Presenter、TabPage 懒创建与切换 |
| `Events` | 各类事件参数（如 `DeviceConnectionChangedEveent`，既有拼写双 e） | 事件总线载荷 |
| `Factor` | 各 Presenter 工厂 | **已判定为冗余，待收敛删除** |

## 3. 依赖方向

- 允许：Presentation → Application / Domain / Shared
- 现存倒置：Presentation 通过 `using static ...HikCameraHardwareDriver` 直接引用 Infrastructure（待清理）

## 4. 既有约定

- **视图实例唯一（重要踩坑）**：Presenter 绑定的视图实例必须与界面挂载的实例**同一个**；严禁在事件回调里 `new` 第二个视图，否则出现「属性已赋值但 UI 不同步」。`Form1` 用字段唯一持有视图 + Presenter。
- **UI 线程**：回填可能在后台线程，视图 setter 内需 `InvokeRequired` 判断；ComboBox 回填枚举符号名须先 `Items.Add` 补齐再选中，直接赋 `SelectedItem` 会被静默丢弃。
- **装配范式（已确认取舍）**：**容器只装服务；视图由宿主唯一 `new` 并构造注入 Presenter。** 工厂仅在「需按类型创建多实例」时才建——纯 `new` 转发的工厂应删除，避免与 DI 形成双轨装配。
- **导航栏极简**：视觉页仅「启动 / 暂停 / 抓拍」三按钮；采集模式由参数配置区（触发模式）决定，不在导航栏叠加下拉与复杂联动。
- **算法页**：单 UC 拆 partial 属**文件切分**而非模块划分；沿用构造注入与唯一实例装配，不新增工厂。
- **Designer 约定**：`UCAlgorithmView` 需 `[DesignerCategory("Code")]`；`.csproj` 中 partial 保持 `<SubType>Code</SubType>`；`OwnerDrawText` 模式必须挂 `DrawNode`，否则节点文字不显示。

## 5. 已知问题 / 待办

- `Form1.SwitchTabView` 显示新视图时误调 `OnViewHide()`，应为 `OnViewShow()`。
- `Form1_FormClosed` 仅释放 `_viewCache`，未释放 `_presenterCache` → 事件订阅泄漏；双缓存待合并。
- `BaseViewUc.OnViewDestroy()`（含 `Controls.Clear()`）无任何调用点，属死代码。
- `Program.cs` 中 4 条视图 Transient 注册与 `Form1` 手工 `new` 并存 → 多实例隐患。
- 算法页契约漂移：`UCAlgorithmView` 仅实现部分 `IAlgorithmRecipeView` 成员（详见算法模块笔记）。

## 6. 变更记录

| 日期 | 变更 |
|---|---|
| 本次对话 | 建立骨架；登记 视图实例唯一约定、装配范式取舍（容器装服务/宿主管视图）、导航栏极简原则、算法页 partial 说明 |
