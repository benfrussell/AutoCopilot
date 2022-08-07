using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCopilot
{
    public interface IInstruction : IInstructionObject
    {
        public IEnumerable<Action> Actions { get; }
    }

    public class Instruction : InstructionObject, IInstruction
    {
        public IEnumerable<Action> Actions { get; }

        public Instruction() : base()
        {
            Actions = new Action[0];
        }

        public Instruction(params Action[] actions) : base()
        {
            Actions = actions;
        }

        public Instruction(string name, params Action[] actions) : base(name)
        {
            Actions = actions;
        }

        public Instruction(IEnumerable<Action> actionEnum, params Action[] actions) : base()
        {
            Actions = actionEnum.Concat(actions);
        }

        public Instruction(string name, IEnumerable<Action> actionEnum, params Action[] actions) : base(name)
        {
            Actions = actionEnum.Concat(actions);
        }

        public override IEnumerator<IInstructionObject> GetEnumerator()
        {
            return Enumerable.Empty<IInstructionObject>().GetEnumerator();
        }
    }
}
