using System;
using System.Collections.Generic;
using Game.Domain;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Game/Data/Dialogue")]
    public class DialogueSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private DialogueNodeData[] _nodes;

        public IReadOnlyList<DialogueNodeData> Nodes => _nodes;

        public DialogueGraph ToGraph()
        {
            var nodes = new DialogueNode[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                DialogueNodeData nodeData = _nodes[i];
                int nextIndex = nodeData.NextIndex;
                int[] optionTargets = new int[nodeData.Options.Length];
                for (int j = 0; j < nodeData.Options.Length; j++)
                {
                    optionTargets[j] = nodeData.Options[j].TargetIndex;
                }
                nodes[i] = new DialogueNode(nextIndex, optionTargets);

            }
            return new DialogueGraph(nodes);
        }
    }
}
