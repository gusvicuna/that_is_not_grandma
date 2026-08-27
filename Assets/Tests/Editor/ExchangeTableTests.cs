using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class ExchangeTableTests
    {
        [Test]
        public void Ctor_NullPairs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ExchangeTable(null));
        }

        [Test]
        public void Ctor_NullOrEmptyKeyOrValue_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ExchangeTable(new Dictionary<string, string>
            {
                { "", "clue_livingroom_01" }
            }));
            Assert.Throws<ArgumentException>(() => new ExchangeTable(new Dictionary<string, string>
            {
                { "clue_kitchen_01", null }
            }));
            Assert.Throws<ArgumentException>(() => new ExchangeTable(new Dictionary<string, string>
            {
                { "clue_kitchen_01", "   " }
            }));
        }

        [Test]
        public void TryGetReturn_MappedClue_ReturnsMappedClue()
        {
            var table = new ExchangeTable(new Dictionary<string, string>
            {
                { "clue_kitchen_01", "clue_livingroom_01" }
            });

            bool found = table.TryGetReturn("clue_kitchen_01", out string returned);

            Assert.That(found, Is.True);
            Assert.That(returned, Is.EqualTo("clue_livingroom_01"));
        }

        [Test]
        public void TryGetReturn_UnmappedClue_ReturnsFallback()
        {
            var table = new ExchangeTable(new Dictionary<string, string>
            {
                { "clue_kitchen_01", "clue_livingroom_01" }
            }, "clue_useless_01");

            bool found = table.TryGetReturn("clue_bedroom_01", out string returned);

            Assert.That(found, Is.True);
            Assert.That(returned, Is.EqualTo("clue_useless_01"));
        }

        [Test]
        public void TryGetReturn_UnmappedClueWithoutFallback_ReturnsFalse()
        {
            var table = new ExchangeTable(new Dictionary<string, string>
            {
                { "clue_kitchen_01", "clue_livingroom_01" }
            });

            bool found = table.TryGetReturn("clue_bedroom_01", out string returned);

            Assert.That(found, Is.False);
            Assert.That(returned, Is.Null);
        }
    }
}
