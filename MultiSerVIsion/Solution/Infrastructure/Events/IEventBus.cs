using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.Events
{
    public interface IEventBus
    {
        /// <summary>订阅事件</summary>
        void Subscribe<TEvent>(Action<TEvent> handler);

        /// <summary>取消订阅（释放时必须调用，避免内存泄漏）</summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        /// <summary>发布事件</summary>
        void Publish<TEvent>(TEvent eventArgs);
    }
}
