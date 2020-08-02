using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace AnthillNet.API
{
    public abstract class Base
    {
        protected Base() => Clock = new Thread(() =>
        {
            while (!ForceOff)
            {
                int before_tick = DateTime.Now.Millisecond;
                if (!isPause)
                    Tick();
                if (TickRate - (DateTime.Now.Millisecond - before_tick) > 0)
                    Thread.Sleep(1 / (TickRate - (DateTime.Now.Millisecond - before_tick)));
            }
        })
        { IsBackground = true };

        private static readonly BinaryFormatter formatter = new BinaryFormatter();
        private Thread Clock;
        public string hostname { private set; get; }
        public ushort port { private set; get; }
        private bool ForceOff;
        public bool isPause { private set; get; }
        public byte TickRate;
        protected bool Active => Clock.IsAlive;

        //Controlling functionality
        public virtual void Start(string hostname, ushort port)
        {
            this.hostname = hostname;
            this.port = port;
            ForceOff = false;
            isPause = false;
            Clock.Start();
        }
        public virtual void Stop() => ForceOff = true;
        public virtual void ForceStop() => Clock.Interrupt();
        public virtual void Pause() => isPause = true;
        public virtual void Resume() => isPause = false;

        //Delegates
        public delegate void TickHander();
        public delegate void ConnectionStabilizedHandler(Connection connection);

        //Events
        public event TickHander OnTick;
        public event ConnectionStabilizedHandler OnConnectionStabilized;

        //Events Invokers
        protected virtual void Tick() => OnTick?.Invoke();
        protected void ConnectionStabilizedInvoke(Connection connection) => OnConnectionStabilized?.Invoke(connection);

        [Serializable]
        public struct Message
        {
            public byte destiny { private set; get; }
            public object data { private set; get; }

            public Message(byte destiny, object data)
            {
                this.destiny = destiny;
                this.data = data;
            }

            public static Message Deserialize(byte[] raw_data)
            {
                MemoryStream stream = new MemoryStream();
                Message message;
                try
                {
                    message = (Message)formatter.Deserialize(new MemoryStream(raw_data));
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    stream.Close();
                }
                return message;
            }

            public byte[] Serialize()
            {
                MemoryStream stream = new MemoryStream();
                try
                {
                    formatter.Serialize(stream, this);
                }
                catch (SerializationException e)
                {
                    throw e;
                } 
                finally
                {
                    stream.Close();
                }
                return stream.ToArray();
            }
        }
    }
}