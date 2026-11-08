# 相机品牌接入约定（Common）

本目录用于存放**品牌无关**的相机基础设施约定与说明，不放置具体品牌实现代码。

## 目录结构约定

```
Solution/Infrastructure/Camera/
├─ Common/         品牌无关约定（本目录，仅文档）
├─ HikVision/      海康品牌实现
└─ DaHua/          大华品牌实现（占位，未实现）
```

## 新增一个相机品牌的三步

1. **实现契约**：在 `Solution/Infrastructure/Camera/<品牌>/` 下新建驱动类，
   实现 `MultiSerVIsion.Solution.Domain.Camera.ICameraHardwareDriver`。
   命名约定：`<品牌>CameraDriver`，例如 `DaHuaCameraDriver`。
2. **组合根注册**：在 `Program.cs` 的 `ConfigureServices` 中注册该驱动。
   当前为单一品牌（海康），注册形如：
   ```csharp
   services.AddSingleton<ICameraHardwareDriver, HikCameraHardwareDriver>();
   ```
   引入多品牌后改为按品牌 Key 组织（例如 `AddKeyedSingleton<ICameraHardwareDriver, DaHuaCameraDriver>("DaHua")`），
   **上层应用服务与 UI 无需改动**。
3. **确认能力覆盖**：契约中必须实现的方法清单见
   `ICameraHardwareDriver`——扫描、测试连接、连接、断开、起停流、参数读写与描述、
   采集模式切换、软触发一次、获取图源。

## 必须遵守的边界

- **Domain 层不依赖任何品牌 SDK 类型**；品牌 SDK 只能出现在 `Infrastructure/Camera/<品牌>/` 内。
- 图源通过 `IFrameSource` 对外暴露，**每消费者独立深拷贝**语义不可更改。
- 参数以节点名字符串交互（`GetParamAsync` / `SetParamAsync` / `ReadParamAsync`），
  不向上层泄漏 SDK 枚举值。
