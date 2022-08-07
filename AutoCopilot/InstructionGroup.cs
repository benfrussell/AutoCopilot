using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCopilot
{
    public interface IInstructionGroup : ICollection<IInstructionObject>
    {

    }

    public class InstructionGroup : InstructionObject, IInstructionGroup
    {
        private LinkedList<IInstructionObject> _instructions;

        public InstructionGroup(string name = "") : base(name)
        {
            _instructions = new LinkedList<IInstructionObject>();
        }

        public int Count => _instructions.Count;

        public bool IsReadOnly => false;

        public void Add(IInstructionObject item)
        {
            _instructions.AddLast(item);
        }

        public void Clear()
        {
            _instructions.Clear();
        }

        public bool Contains(IInstructionObject item)
        {
            return _instructions.Contains(item);
        }

        public void CopyTo(IInstructionObject[] array, int arrayIndex)
        {
            _instructions.CopyTo(array, arrayIndex);
        }

        public override IEnumerator<IInstructionObject> GetEnumerator()
        {
            return _instructions.GetEnumerator();
        }

        public bool Remove(IInstructionObject item)
        {
            return _instructions.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
