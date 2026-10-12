using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Presentation.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Domain.Enums.NodeEnums;

namespace MultiSerVIsion.Solution.Presentation.Views
{
    public interface IAlgorithmRecipeView
    {
        // ================= 事件：View 通知 Presenter（用户操作） =================
        event EventHandler<string> NodeSelected;              // 节点被选中
        event EventHandler<NodeDropEventArgs> NodeDropped;    // 算子被拖入
        event EventHandler<string> NodeDeleted;               // 节点被删除
        event EventHandler<NodeRenameEventArgs> NodeRenamed;  // 节点重命名
        event EventHandler<string> BreakpointToggled;         // 断点切换

        event EventHandler RunRequested;                      // 点击运行
        event EventHandler StopRequested;                     // 点击停止
        event EventHandler StepRequested;                     // 点击单步
        event EventHandler RunToSelectedRequested;            // 执行到选中
        event EventHandler RunFromSelectedRequested;          // 从选中开始

        // ================= 状态：View 暴露给 Presenter 读取 =================
        string SelectedNodeId { get; }
        bool IsRunning { get; }

        // ================= 方法：Presenter 调用 View 刷新（被动渲染） =================
        void LoadToolbox(IEnumerable<OperatorMetadata> operators);
        void LoadFlowTree(FlowNodeModel root);

        // 状态与校验
        void SetNodeState(string nodeId, NodeRunState state, long elapsedMs);
        void SetNodeValidation(string nodeId, ValidationLevel level, string message);
        void SetDragPreview(string nodeId, bool hasError); // 拖拽红框

        // 属性面板与变量
        void ShowParameters(object parameterObject);
        void UpdateVariable(string name, string type, string value, string source);

        // 日志与状态栏
        void AppendLog(string message);
        void UpdateStatus(string status, string currentNode, long elapsedMs);

        // 通用交互
        void ShowMessage(string title, string message);
        string PromptInput(string title, string defaultValue);
    }
}
