using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Repositories
{
    public interface ICameraHardwareDriver
    {
        Task<List<CameraHardwareRawDto>> ScanAllCameraAsync();

        /* int Login(CameraConnectConfig connectParam, out int sdkHandle);*/
        Task<(int errorCode, MyCamera cameraObj)> LoginAsync(CameraConnectConfig connectParam);
        Task<int> OpenStreamAsync(MyCamera cameraObj);
        Task<int> CloseStreamAsync(MyCamera cameraObj);
        Task<int> LogoutAsync(MyCamera cameraObj);
        void Relese();
    }
}
