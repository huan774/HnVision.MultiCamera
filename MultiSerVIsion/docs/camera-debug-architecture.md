# 相机调试专用化架构说明

本项目已收敛为**相机调试专用**工程：剥离算法配方、运行监控、运动轴等非相机模块，
按业务聚合重组目录，并为多品牌相机 SDK 预留扩展点。**功能行为与重构前完全一致。**

## 1. 目标目录结构

```
MultiSerVIsion/
├─ MultiSerVIsion.csproj
├─ Program.cs                         组合根（DI 注册 9 项）
├─ docs/
│  └─ camera-debug-architecture.md    本文档
└─ Solution/
   ├─ Domain/                          领域层（不依赖任何 SDK 与 UI）
   │  ├─ Camera/                       品牌无关相机领域
   │  │   ICameraHardwareDriver.cs      ★ 多品牌唯一扩展契约
   │  │   ICameraDeviceService.cs
   │  │   CameraDomainService.cs
   │  │   CameraEntity.cs
   │  │   CameraConnectConfig.cs
   │  │   CameraParameterSpec.cs
   │  │   ParamSpec.cs
   │  │   ParamReadResult.cs
   │  │   CameraFrame.cs
   │  │   CameraDeviceDto.cs
   │  │   CameraStatus.cs
   │  │   AcquisitionMode.cs
   │  │   CameraMappingExtensions.cs
   │  │   IFrameSource.cs / IFrameSourceProvider.cs
   │  │   IFrameBufferPool.cs / FrameSourceState.cs
   │  ├─ Device/                       设备通用领域
   │  │   DeviceEntity.cs / DeviceFactory.cs / DeviceConnectionStatue.cs
   │  │   IDeviceManager.cs / IDeviceRepository.cs
   │  │   DeviceDomainService.cs / IDeviceDomainSerivce.cs
   │  │   MotionCardConnectConfig.cs
   │  ├─ Contexts/                     DeviceContext / IDeviceContext（会话状态）
   │  └─ Enums/NodeEnums.cs            仅保留 ValidationLevel
   ├─ Application/                     应用层（编排用例）
   │  ├─ Camera/                       ICameraAppService / CameraAppService
   │  └─ Device/                       IDeviceAppService / DeviceAppService
   │     └─ Dtos/                      DeviceCreateInput / DeviceUpdateIput
   ├─ Infrastructure/                  基础设施层（品牌 SDK 只允许出现在此）
   │  ├─ Camera/
   │  │  ├─ Common/README.md           多品牌接入约定
   │  │  ├─ HikVision/                 海康实现（驱动/会话/图源/帧池/描述符/参数名）
   │  │  └─ DaHua/README.md            大华占位（未实现）
   │  ├─ Device/                       DeviceManager / DeviceRepository
   │  └─ Events/                       EventBus / IEventBus
   ├─ Presentation/                    表现层（按页面聚合）
   │  ├─ Shell/                        Form1、BaseViewUc
   │  ├─ DevicePanel/                  设备树、设备信息、参数面板（UC + 契约 + Presenter）
   │  ├─ CameraPreview/                视觉预览页（UCVisionView + IVisionView + VisionPreseter）
   │  ├─ Common/                       BasePresenter
   │  ├─ Events/                       4 个相机选择/连接事件
   │  └─ Winforms/                     FrmAddDevice（手动添加设备对话框）
   └─ Shared/                          GlobalConst、Helpers、Models（OperationResult 等）
```

## 2. 命名空间 ↔ 文件夹对照表

| 文件夹 | 命名空间 |
|---|---|
| `Domain\Camera\` | `MultiSerVIsion.Solution.Domain.Camera` |
| `Domain\Device\` | `MultiSerVIsion.Solution.Domain.Device` |
| `Domain\Contexts\` | `MultiSerVIsion.Solution.Domain.Contexts` |
| `Domain\Enums\` | `MultiSerVIsion.Solution.Domain.Enums` |
| `Application\Camera\` | `MultiSerVIsion.Solution.Application.Camera` |
| `Application\Device\` | `MultiSerVIsion.Solution.Application.Device` |
| `Application\Device\Dtos\` | `MultiSerVIsion.Solution.Application.Device.Dtos` |
| `Infrastructure\Camera\HikVision\` | `MultiSerVIsion.Solution.Infrastructure.Camera.HikVision` |
| `Infrastructure\Device\` | `MultiSerVIsion.Solution.Infrastructure.Device` |
| `Infrastructure\Events\` | `MultiSerVIsion.Solution.Infrastructure.Events` |
| `Presentation\Shell\` | `MultiSerVIsion.Solution.Presentation.Shell` |
| `Presentation\DevicePanel\` | `MultiSerVIsion.Solution.Presentation.DevicePanel` |
| `Presentation\CameraPreview\` | `MultiSerVIsion.Solution.Presentation.CameraPreview` |
| `Presentation\Common\` | `MultiSerVIsion.Solution.Presentation.Common` |
| `Presentation\Events\` | `MultiSerVIsion.Solution.Presentation.Events` |
| `Presentation\Winforms\` | `MultiSerVIsion.Solution.Presentation.Winforms` |
| `Shared\`（各子目录） | `MultiSerVIsion.Solution.Shared.*` |

## 3. 新增品牌 SDK 接入三步

1. **新建实现**
   在 `Solution\Infrastructure\Camera\<Brand>\` 下新建类，实现
   `MultiSerVIsion.Solution.Domain.Camera.ICameraHardwareDriver`，
   类名约定 `<Brand>CameraDriver`（例如 `DaHuaCameraDriver`）。
   必须覆盖：扫描、测试连接、连接、断开、起停流、参数读写与描述读取、
   采集模式切换、软触发一次、获取图源。

2. **组合根注册**
   在 `Program.ConfigureServices` 的「相机业务层」按品牌 Key 追加注册，例如：
   `services.AddKeyedSingleton<ICameraHardwareDriver, DaHuaCameraDriver>("DaHua");`
   上层应用服务与 UI **无需任何改动**。

3. **登记品牌差异**
   在 `Solution\Infrastructure\Camera\Common\README.md` 登记品牌清单与能力差异
   （参数节点名、像素格式支持、句柄生命周期语义等）。

## 4. 禁止项

- **不得在 UI 层出现品牌名**：`Presentation` 与 `Domain` 不得出现 `Hik`/`DaHua` 等品牌标识。
- **不得让 Domain 引用 Infrastructure**：领域层不得出现 SDK 类型（`MyCamera`、`MvCamCtrl` 等）。
- **品牌 SDK 只能出现在** `Infrastructure\Camera\<Brand>\` 内。
- **不得改变图源拷贝语义**：`IFrameSource` 对每个消费者独立深拷贝，消费者拥有该帧且无需释放。
- **不得把品牌枚举值泄漏到上层**：参数以节点名字符串交互，像素格式统一经 `CameraFrame.PixelFormat`。
