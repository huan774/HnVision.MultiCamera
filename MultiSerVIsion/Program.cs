using Microsoft.Extensions.DependencyInjection;
using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Infrastructure.Repository;
using MultiSerVIsion.Solution.Presentation.Views;
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
            // 硬件驱动注册为瞬时：每个相机连接使用独立实例，避免句柄/线程冲突
            // 驱动内部的静态扫描缓存依然全局共享，不影响扫描结果复用
            services.AddSingleton<ICameraHardwareDriver, HikCameraHardwareDriver>();
            services.AddSingleton<ICameraDeviceService, CameraDomainService>();
            services.AddSingleton<ICameraAppService, CameraApplicationoService>();

            // ====================== 装配约定 ======================
            // 容器只注册后台服务；视图与 Presenter 由宿主（Form1）唯一创建并绑定，
            // 不从容器解析视图（避免多实例：Presenter 绑定实例与界面显示实例不一致）。
            // Presenter 构造参数含运行时视图实例，因此也不注册到容器；
            // 纯 new 转发的 Presenter 工厂已删除。
            // ====================== 基础设施层 ======================
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<Solution.Domain.Contexts.IDeviceContext, DeviceContext>();
        }
    }
}
