using AnthillNet.Core;
using System;
using System.Reflection;

namespace AnthillNet.Events
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Order : Attribute
    {
        public readonly Interpreter Interpreter;
        public bool CanOrderServer { private set; get; }
        public bool CanOrderClient { private set; get; }

        public Order(bool toServer, bool toClient)
        {
            this.CanOrderClient = toClient;
            this.CanOrderServer = toServer;
        }

        internal Order(Interpreter interpreter) => this.Interpreter = interpreter;

        public void Invoke(Action target)
        {
            
        }

        public void Invoke(Action target, Connection connection)
        {

        }
    }
}
