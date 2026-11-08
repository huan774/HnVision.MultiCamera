# 大华相机驱动（DaHua）— 占位目录

本目录为**多品牌扩展预留位**，当前无实现代码。

## 计划内容

新增大华品牌时，在本目录下实现：

```
Solution/Infrastructure/Camera/DaHua/
└─ DaHuaCameraDriver.cs     实现 MultiSerVIsion.Solution.Domain.Camera.ICameraHardwareDriver
```

## 接入步骤（见 ../Common/README.md）

1. 新建 `DaHuaCameraDriver : ICameraHardwareDriver`，覆盖契约中的全部方法：
   扫描、测试连接、连接、断开、起停流、参数读写/描述读取、采集模式切换、软触发一次、获取图源。
2. 在 `Program.cs` 组合根按品牌注册（多品牌场景下使用品牌 Key）。
3. 上层 `CameraAppService` 与 UI **不做任何修改**。

## 待确认事项（实现前需明确）

- 大华 SDK 的**句柄生命周期**语义（是否与海康一样按序列号缓存会话）。
- 像素格式枚举与海康的对应关系，确保 `CameraFrame.PixelFormat` 的取值语义一致。
- 参数节点的命名是否与海康一致；若不一致，需要在驱动内部做节点名映射，
  **不得把品牌差异泄漏到 Domain / Application 层**。
