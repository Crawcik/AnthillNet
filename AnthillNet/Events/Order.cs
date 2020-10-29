using AnthillNet.Core;
using System;

namespace AnthillNet.Events
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [Serializable]
    public class Order : Attribute
    {
        private Interpreter Interpreter { set; get; }
        public readonly bool CanOrderServer;
        public readonly bool CanOrderClient;
        private Action Action;

        public Order(bool toServer = true, bool toClient = true)
        {
            this.CanOrderClient = toClient;
            this.CanOrderServer = toServer;
        }

        public Order(Interpreter interpreter) => this.Interpreter = interpreter;
        internal void Invoke() => this.Action.Invoke();

        public void Call(Action target)
        {
            Order attribute = (Order)Attribute.GetCustomAttribute(target.Method, typeof(Order));
            if (attribute == null || target == null)
                return;
            attribute.Action = target;
            Interpreter.PrepareOrder(attribute);
        }
    }
}
