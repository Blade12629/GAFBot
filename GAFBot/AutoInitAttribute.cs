using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot
{
    public class AutoInitAttribute : Attribute
    {
        public int Priority { get; private set; }

        public AutoInitAttribute() : base()
        {
            Priority = int.MaxValue;
        }

        public AutoInitAttribute(int priority) : base()
        {
            Priority = priority;
        }
    }
}
