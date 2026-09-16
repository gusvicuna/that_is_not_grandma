using System.Collections.Generic;

namespace Game.Domain
{
    public class DialogueNode
    {
        public readonly int NextIndex;
        public readonly IReadOnlyList<int> OptionTargets;

        public DialogueNode(int nextIndex, IReadOnlyList<int> optionTargets)
        {
            NextIndex = nextIndex;
            OptionTargets = optionTargets ?? new List<int>();
        }
    }
}
