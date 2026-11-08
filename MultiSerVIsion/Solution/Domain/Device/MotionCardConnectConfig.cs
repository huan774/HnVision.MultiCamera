using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Device
{
    public class MotionCardConnectConfig
    {
        public int CardIndex { get; set; } = 0;            
        public int MaxPulseFreq { get; set; } = 100000;     
        public int AccDecTimeMs { get; set; } = 200;       
        public string EncoderMode { get; set; } = "ABPhase";
        public bool LimitEnable { get; set; } = true;       
        public bool AutoHomeOnStart { get; set; } = true;   
        public int PulsePerMm { get; set; } = 1000;

    }
    public class MotionCardRuntimeParam
    {
        public int TempJogSpeed { get; set; }              // 点动临时速度
        public double TempAccScale { get; set; } = 1.0;     // 临时加减速倍率
        public long TempTargetPos { get; set; }            // 临时运动目标位置
        public bool TempIgnoreLimit { get; set; } = false;  // 临时屏蔽限位
        public bool IsJogRunning { get; set; } = false;     // 点动运行状态
        public double TempGearRatio { get; set; } = 1.0;    // 临时电子齿轮比
    }
}
