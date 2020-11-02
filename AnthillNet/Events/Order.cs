using System;

namespace AnthillNet.Events
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Order : Attribute
    {
        private Interpreter Interpreter { set; get; }
        public readonly bool CanOrderServer;
        public readonly bool CanOrderClient;
        private Action Action;
        private Action<NetArgs> ActionWithArgument;
        private NetArgs args;

        public Order(bool toServer = true, bool toClient = true)
        {
            this.CanOrderClient = toClient;
            this.CanOrderServer = toServer;
        }

        public Order(Interpreter interpreter) => this.Interpreter = interpreter;
        internal void Invoke()
        {
            if (Action != null)
                this.Action?.Invoke();
            if (ActionWithArgument != null)
                this.ActionWithArgument?.Invoke(args);
        }
        public void Call(Action target)
        {
            Order attribute = (Order)Attribute.GetCustomAttribute(target.Method, typeof(Order));
            if (attribute == null || target == null)
                return;
            attribute.Action = target;
            Interpreter.PrepareOrder(attribute);
        }

        public void Call(Action<NetArgs> target, NetArgs args)
        {
            Order attribute = (Order)Attribute.GetCustomAttribute(target.Method, typeof(Order));
            if (attribute == null || target == null)
                return;
            attribute.ActionWithArgument = target;
            attribute.args = args;
            Interpreter.PrepareOrder(attribute);
        }
    }
}
