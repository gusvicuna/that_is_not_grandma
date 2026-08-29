using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class DialogueGraphTests
    {
        private static DialogueNode Linear(int nextIndex)
        {
            return new DialogueNode(nextIndex, Array.Empty<int>());
        }

        private static DialogueNode Choice(params int[] optionTargets)
        {
            return new DialogueNode(DialogueGraph.EndIndex, optionTargets);
        }

        [Test]
        public void Ctor_NullOrEmptyNodes_Throws()
        {
            Assert.Throws<ArgumentException>(() => new DialogueGraph(null));
            Assert.Throws<ArgumentException>(() => new DialogueGraph(Array.Empty<DialogueNode>()));
        }

        [Test]
        public void Ctor_NextIndexOutOfRange_Throws()
        {
            Assert.Throws<ArgumentException>(() => new DialogueGraph(new[]
            {
                Linear(2),
                Linear(DialogueGraph.EndIndex)
            }));
            Assert.Throws<ArgumentException>(() => new DialogueGraph(new[]
            {
                Linear(-2)
            }));
        }

        [Test]
        public void Ctor_OptionTargetOutOfRange_Throws()
        {
            Assert.Throws<ArgumentException>(() => new DialogueGraph(new[]
            {
                Choice(1, 5),
                Linear(DialogueGraph.EndIndex)
            }));
            Assert.Throws<ArgumentException>(() => new DialogueGraph(new[]
            {
                Choice(-2)
            }));
        }

        [Test]
        public void Ctor_ValidGraph_ExposesNodes()
        {
            DialogueNode[] nodes = new[]
            {
                Linear(1),
                Choice(0, DialogueGraph.EndIndex)
            };

            var graph = new DialogueGraph(nodes);

            Assert.That(graph.Count, Is.EqualTo(2));
            Assert.That(graph[0].NextIndex, Is.EqualTo(1));
            Assert.That(graph[0].OptionTargets, Is.Empty);
            Assert.That(graph[1].OptionTargets, Is.EqualTo(new[] { 0, DialogueGraph.EndIndex }));
        }
    }
}
