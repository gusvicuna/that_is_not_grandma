using System.Collections.Generic;

namespace Game.Domain
{
    public class DialogueGraph
    {
        public const int EndIndex = -1;
        private IReadOnlyList<DialogueNode> _nodes;

        public DialogueGraph(IReadOnlyList<DialogueNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                throw new System.ArgumentException("Dialogue graph must contain at least one node.");
            }
            _nodes = new List<DialogueNode>(nodes);
            foreach (var node in _nodes)
            {
                if (node.NextIndex != EndIndex && (node.NextIndex < 0 || node.NextIndex >= _nodes.Count))
                {
                    throw new System.ArgumentException($"Node has invalid NextIndex: {node.NextIndex}");
                }
                foreach (var optionTarget in node.OptionTargets)
                {
                    if (optionTarget != EndIndex && (optionTarget < 0 || optionTarget >= _nodes.Count))
                    {
                        throw new System.ArgumentException($"Node has invalid OptionTarget: {optionTarget}");
                    }
                }
            }
        }

        public int Count => _nodes.Count;

        public DialogueNode this[int index] => _nodes[index];
    }
}
