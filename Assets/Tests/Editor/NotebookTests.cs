using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class NotebookTests
    {
        private Notebook _notebook;

        [SetUp]
        public void SetUp()
        {
            _notebook = new Notebook();
        }

        [Test]
        public void Count_EmptyNotebook_IsZero()
        {
            Assert.That(_notebook.Count, Is.EqualTo(0));
        }

        [Test]
        public void Collect_NewClue_ReturnsTrueAndAddsIt()
        {
            bool added = _notebook.Collect("clue_kitchen_01");

            Assert.That(added, Is.True);
            Assert.That(_notebook.Contains("clue_kitchen_01"), Is.True);
            Assert.That(_notebook.Count, Is.EqualTo(1));
        }

        [Test]
        public void Collect_DuplicateClue_ReturnsFalseAndCountUnchanged()
        {
            _notebook.Collect("clue_kitchen_01");

            bool addedAgain = _notebook.Collect("clue_kitchen_01");

            Assert.That(addedAgain, Is.False);
            Assert.That(_notebook.Count, Is.EqualTo(1));
        }

        [Test]
        public void Collect_MultipleClues_PreservesInsertionOrder()
        {
            _notebook.Collect("clue_bathroom_01");
            _notebook.Collect("clue_kitchen_01");
            _notebook.Collect("clue_bedroom_01");

            Assert.That(_notebook.CollectedIds, Is.EqualTo(new[]
            {
                "clue_bathroom_01",
                "clue_kitchen_01",
                "clue_bedroom_01"
            }));
        }

        [Test]
        public void Contains_UncollectedClue_ReturnsFalse()
        {
            _notebook.Collect("clue_kitchen_01");

            Assert.That(_notebook.Contains("clue_bathroom_01"), Is.False);
        }

        [Test]
        public void Collect_NullOrEmptyId_Throws()
        {
            Assert.Throws<ArgumentException>(() => _notebook.Collect(null));
            Assert.Throws<ArgumentException>(() => _notebook.Collect(string.Empty));
            Assert.Throws<ArgumentException>(() => _notebook.Collect("   "));
        }
    }
}
