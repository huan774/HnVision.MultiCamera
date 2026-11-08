using Microsoft.Extensions.DependencyInjection;
using MultiSerVIsion.Solution.Application.Device;
using MultiSerVIsion.Solution.Application.Camera;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.Camera.HikVision;
using MultiSerVIsion.Solution.Infrastructure.Device;
using MultiSerVIsion.Solution.Presentation.DevicePanel;
using MultiSerVIsion.Solution.Presentation.CameraPreview;
using MultiSerVIsion.Solution.Presentation.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MultiSerVIsion
{
    internal static class Program
    {
        public static IServiceProvider Services { get; private set; }
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. 注册所有服务与依赖
            var services = new ServiceCollection();
            ConfigureServices(services);

            // 2. 构建依赖注入容器
            Services = services.BuildServiceProvider();

            // 3. 从容器获取主窗体，自动注入其所有依赖
            var mainForm = Services.GetRequiredService<Form1>();

            Application.Run(mainForm);
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            // ====================== 主窗体 ======================
            // 注册为瞬时：每次获取都是新实例，匹配窗体生命周期
            services.AddTransient<Form1>();

            // ====================== 设备领域层 ======================
            services.AddSingleton<IDeviceRepository, DeviceRepository>();
            services.AddSingleton<IDeviceDomainSerivce, DeviceDomainService>();
            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<IDeviceAppService, DeviceAppService>();

            // ====================== 相机业务层 ======================
            // 【多品牌扩展点】新增品牌时在此按品牌 Key 追加 ICameraHardwareDriver 注册，
            // 例如 DaHua：services.AddKeyedSingleton<ICameraHardwareDriver, DaHuaCameraDriver>("DaHua");
            // UI、应用服务与领域契约均无需改动。
            services.AddSingleton<ICameraHardwareDriver, HikCameraHardwareDriver>();
            services.AddSingleton<ICameraDeviceService, CameraDomainService>();
            services.AddSingleton<ICameraAppService, CameraApplicationoService>();

            // ====================== 基础设施层 ======================
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<Solution.Domain.Contexts.IDeviceContext, DeviceContext>();

            // ====================== 装配约定 ======================
            // 容器只注册后台服务；视图与 Presenter 由宿主（Form1）唯一创建并绑定，
            // 不从容器解析视图（避免多实例：Presenter 绑定实例与界面显示实例不一致）。
            // Presenter 构造参数含运行时视图实例，因此也不注册到容器；
            // 纯 new 转发的 Presenter 工厂已删除。
        }
    }
}
