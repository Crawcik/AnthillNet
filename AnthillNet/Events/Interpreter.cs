using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthillNet.Events
{
    public class Interpreter
    {
        public Order GetOrderFunction() => new Order(this);
    }
}
