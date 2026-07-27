using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
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
      
        int Login(CameraConnectConfig connectParam);
        void OpenStream(int sdkHandle);
        void CloseStream(int sdkHandle);
        void Logout(int sdkHandle);
        void Relese();
    }
}
