using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class DialogueRunnerTests
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
        public void Ctor_NewRunner_StartsAtNodeZeroUnfinished()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Linear(DialogueGraph.EndIndex)
            }));

            Assert.That(runner.IsFinished, Is.False);
            Assert.That(runner.CurrentIndex, Is.EqualTo(0));
            Assert.That(runner.CurrentHasOptions, Is.False);
        }

        [Test]
        public void Ctor_NullGraph_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DialogueRunner(null));
        }

        [Test]
        public void Advance_LinearNode_MovesToNextIndex()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Linear(1),
                Linear(DialogueGraph.EndIndex)
            }));

            runner.Advance();

            Assert.That(runner.CurrentIndex, Is.EqualTo(1));
            Assert.That(runner.IsFinished, Is.False);
        }

        [Test]
        public void Advance_NodeWithEndIndex_Finishes()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Linear(DialogueGraph.EndIndex)
            }));

            runner.Advance();

            Assert.That(runner.IsFinished, Is.True);
            Assert.Throws<InvalidOperationException>(() => _ = runner.CurrentIndex);
        }

        [Test]
        public void Advance_OnChoiceNode_Throws()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Choice(1, 1),
                Linear(DialogueGraph.EndIndex)
            }));

            Assert.Throws<InvalidOperationException>(() => runner.Advance());
        }

        [Test]
        public void Advance_WhenFinished_Throws()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Linear(DialogueGraph.EndIndex)
            }));
            runner.Advance();

            Assert.Throws<InvalidOperationException>(() => runner.Advance());
        }

        [Test]
        public void Choose_ValidOption_JumpsToTarget()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Choice(1, 2),
                Linear(DialogueGraph.EndIndex),
                Linear(DialogueGraph.EndIndex)
            }));

            Assert.That(runner.CurrentHasOptions, Is.True);

            runner.Choose(1);

            Assert.That(runner.CurrentIndex, Is.EqualTo(2));
            Assert.That(runner.IsFinished, Is.False);
        }

        [Test]
        public void Choose_OptionTargetingEnd_Finishes()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Choice(1, DialogueGraph.EndIndex),
                Linear(DialogueGraph.EndIndex)
            }));

            runner.Choose(1);

            Assert.That(runner.IsFinished, Is.True);
        }

        [Test]
        public void Choose_OnLinearNode_Throws()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Linear(DialogueGraph.EndIndex)
            }));

            Assert.Throws<InvalidOperationException>(() => runner.Choose(0));
        }

        [Test]
        public void Choose_OutOfRangeOption_Throws()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Choice(1, 1),
                Linear(DialogueGraph.EndIndex)
            }));

            Assert.Throws<ArgumentOutOfRangeException>(() => runner.Choose(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => runner.Choose(2));
        }

        [Test]
        public void Choose_WhenFinished_Throws()
        {
            var runner = new DialogueRunner(new DialogueGraph(new[]
            {
                Linear(DialogueGraph.EndIndex)
            }));
            runner.Advance();

            Assert.Throws<InvalidOperationException>(() => runner.Choose(0));
        }

        [Test]
        public void Branches_Converge_BothPathsReachSharedNode()
        {
            // Cosmetic-branching shape: 0 = choice, 1/2 = branch lines, 3 = shared closing node.
            var graph = new DialogueGraph(new[]
            {
                Choice(1, 2),
                Linear(3),
                Linear(3),
                Linear(DialogueGraph.EndIndex)
            });

            var runnerA = new DialogueRunner(graph);
            runnerA.Choose(0);
            runnerA.Advance();

            var runnerB = new DialogueRunner(graph);
            runnerB.Choose(1);
            runnerB.Advance();

            Assert.That(runnerA.CurrentIndex, Is.EqualTo(3));
            Assert.That(runnerB.CurrentIndex, Is.EqualTo(3));

            runnerA.Advance();
            runnerB.Advance();

            Assert.That(runnerA.IsFinished, Is.True);
            Assert.That(runnerB.IsFinished, Is.True);
        }
    }
}
