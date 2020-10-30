using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthillNet.Events
{
    internal class EventHandler
    {
        private readonly Dictionary<Type, INetworkEvent> event_stock = new Dictionary<Type, INetworkEvent>();

        public void HandleEvent<T>(NetArgs args) where T : INetworkEvent
        {
            foreach (INetworkEvent ev in this.event_stock.Where(x => typeof(T).IsAssignableFrom(x.Key)).Select(x => x.Value))
                args.Invoke(ev);
        }

        public void LoadEventHandler(INetworkEvent instance) => event_stock.Add(instance.GetType(), instance);

        public void LoadEventHandler(IEnumerable<INetworkEvent> instances)
        {
            foreach (INetworkEvent instance in instances)
                LoadEventHandler(instance);
        }

        public void ClearEventHandlers() => this.event_stock.Clear();
    }
}
