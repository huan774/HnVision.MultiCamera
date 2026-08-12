using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.Events
{
    public class EventBus:IEventBus
    {
        public readonly ConcurrentDictionary<Type,Delegate> _handlerDict = new ConcurrentDictionary<Type,Delegate>();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType=typeof(TEvent);
            _handlerDict.AddOrUpdate(
                eventType,
                handler,
                (_,existing)=>Delegate.Combine(existing,handler));
        }
        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType=typeof(TEvent);
            if (!_handlerDict.TryGetValue(eventType, out var existing)) return;

            var newHandler = Delegate.Remove(existing, handler);
            if(newHandler==null)
                _handlerDict.TryRemove(eventType, out _);
            else
                _handlerDict[eventType]= newHandler;
        }
        public void Publish<TEvent>(TEvent evetArgs)
        {
            if(_handlerDict.TryGetValue(typeof(TEvent), out var handler))
            {
               (handler as Action<TEvent>)?.Invoke(evetArgs);
            }
        }
    }
}
