﻿using System;
using System.Reflection;

namespace AnthillNet.Events
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Order : Attribute
    {
        private Interpreter Interpreter { set; get; }
        public readonly bool CanOrderServer;
        public readonly bool CanOrderClient;
        public string target { private set; get; }
        private Action Action;
        private Action<object> ActionWithArgument;
        private object arg;
       

        public Order(bool toServer = true, bool toClient = true)
        {
            this.CanOrderClient = toClient;
            this.CanOrderServer = toServer;
        }

        public Order(Interpreter interpreter) => this.Interpreter = interpreter;
        internal void Invoke(bool server, bool client)
        {
            MethodInfo info = null;
            if (Action != null)
                info= Action.Method;
            else if( ActionWithArgument != null)
                info = ActionWithArgument.Method;
            if (info == null)
                return;
            Order safeOrder = (Order)Order.GetCustomAttribute(info, typeof(Order));
            if (!((safeOrder.CanOrderServer && server) || (safeOrder.CanOrderClient && client)))
                return;
            if (Action != null)
                this.Action?.Invoke();
            else if (ActionWithArgument != null)
                this.ActionWithArgument?.Invoke(arg);
        }
        public void Call(Action target)
        {
            Order attribute = (Order)Order.GetCustomAttribute(target.Method, typeof(Order));
            if (attribute == null || target == null)
                return;
            attribute.Action = target;
            Interpreter.PrepareOrder(attribute);
        }

        public void Call(Action<object> target, object arg)
        {
            Order attribute = (Order)Order.GetCustomAttribute(target.Method, typeof(Order));
            if (attribute == null || target == null)
                return;
            attribute.ActionWithArgument = target;
            attribute.arg = arg;
            Interpreter.PrepareOrder(attribute);
        }

        public void CallTo(Action<object> method, object arg, string target)
        {
            Order attribute = (Order)Order.GetCustomAttribute(method.Method, typeof(Order));
            if (attribute == null || method == null)
                return;
            attribute.ActionWithArgument = method;
            attribute.arg = arg;
            attribute.target = target;
            Interpreter.PrepareOrder(attribute);
        }

        public void CallTo(Action method, string target)
        {
            Order attribute = (Order)Order.GetCustomAttribute(method.Method, typeof(Order));
            if (attribute == null || method == null)
                return;
            attribute.Action = method;
            attribute.target = target;
            Interpreter.PrepareOrder(attribute);
        }
    }
}
