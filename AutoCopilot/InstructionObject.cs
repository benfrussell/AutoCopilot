using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCopilot
{
    public interface IInstructionObject : IEnumerable<IInstructionObject>
    {
        int ID { get; }
        string Name { get; }
    }

    public abstract class InstructionObject : IInstructionObject
    {
        static int LastInstructionID = -1;

        public int ID { get; private set; }
        public string Name { get; private set; }

        protected InstructionObject(string name = "")
        {
            ID = ++LastInstructionID;
            Name = name;
        }

        public abstract IEnumerator<IInstructionObject> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
