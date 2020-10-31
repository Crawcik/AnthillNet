﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthillNet.Events
{
    public class EventHandler
    {
        private readonly Dictionary<Type, INetEvent> event_stock = new Dictionary<Type, INetEvent>();
        private Interpreter Interpreter { set; get; }

        public EventHandler(Interpreter interpreter)
        {
            this.Interpreter = interpreter;
            this.Interpreter.OnEventIncoming += OnEventIncoming;
        }

        private void OnEventIncoming(object sender, Message message)
        {
            EventCommand command = ((EventCommand)message.data);
            this.HandleEvent(command.args, command.type);
        }

        public void OrderEvent<T>(NetArgs args) where T : INetEvent
        {
            Interpreter.PrepareEvent(new EventCommand() { type = typeof(T), args = args });
        }

        public void HandleEvent<T>(NetArgs args) where T : INetEvent
        {
            foreach (INetEvent ev in this.event_stock.Where(x => typeof(T).IsAssignableFrom(x.Key)).Select(x => x.Value))
                args.Invoke(ev);
        }

        public void HandleEvent(NetArgs args, Type net_event)
        {
            try
            {
                foreach (INetEvent ev in this.event_stock.Where(x => net_event.IsAssignableFrom(x.Key)).Select(x => x.Value))
                    args.Invoke(ev);
            } 
            catch(Exception ex)
            {
            }
        }

        public void LoadEventHandler(INetEvent instance) => event_stock.Add(instance.GetType(), instance);

        public void LoadEventHandler(IEnumerable<INetEvent> instances)
        {
            foreach (INetEvent instance in instances)
                LoadEventHandler(instance);
        }

        public void ClearEventHandlers() => this.event_stock.Clear();
    }
}