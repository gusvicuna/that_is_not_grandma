using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class NoRepeatPickerTests
    {
        private const int _iterations = 200;

        [Test]
        public void Ctor_CountBelowOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new NoRepeatPicker(0, new Random(1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NoRepeatPicker(-3, new Random(1)));
        }

        [Test]
        public void Ctor_NullRandom_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new NoRepeatPicker(3, null));
        }

        [Test]
        public void Next_SingleClip_AlwaysReturnsZero()
        {
            // A one-clip cue must not hang looking for a different index.
            var picker = new NoRepeatPicker(1, new Random(1));

            for (int i = 0; i < _iterations; i++)
            {
                Assert.That(picker.Next(), Is.EqualTo(0));
            }
        }

        [Test]
        public void Next_NeverRepeatsPreviousIndex()
        {
            var picker = new NoRepeatPicker(2, new Random(1));
            int previous = picker.Next();

            for (int i = 0; i < _iterations; i++)
            {
                int current = picker.Next();

                Assert.That(current, Is.Not.EqualTo(previous), $"index {current} repeated at iteration {i}");
                previous = current;
            }
        }

        [Test]
        public void Next_AlwaysReturnsIndexInRange()
        {
            const int count = 4;
            var picker = new NoRepeatPicker(count, new Random(1));

            for (int i = 0; i < _iterations; i++)
            {
                Assert.That(picker.Next(), Is.InRange(0, count - 1));
            }
        }

        [Test]
        public void Next_SameSeed_ProducesSameSequence()
        {
            // Randomness is injected, so a cue's variation order is reproducible in a bug report.
            var first = new NoRepeatPicker(4, new Random(12345));
            var second = new NoRepeatPicker(4, new Random(12345));

            for (int i = 0; i < _iterations; i++)
            {
                Assert.That(second.Next(), Is.EqualTo(first.Next()));
            }
        }

        [Test]
        public void Next_EventuallyReturnsEveryIndex()
        {
            // No-repeat must not collapse into an A-B-A-B rut that never reaches the other clips.
            const int count = 4;
            var picker = new NoRepeatPicker(count, new Random(7));
            var seen = new bool[count];

            for (int i = 0; i < _iterations; i++)
            {
                seen[picker.Next()] = true;
            }

            for (int index = 0; index < count; index++)
            {
                Assert.That(seen[index], Is.True, $"index {index} never came up in {_iterations} picks");
            }
        }
    }
}
