using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class ExchangeLogTests
    {
        private ExchangeLog _log;

        private static NpcProfile Npc(string id, bool leaks, ExchangeTable table = null)
        {
            return new NpcProfile(id, leaks, table ?? new ExchangeTable(new Dictionary<string, string>()));
        }

        private static ExchangeTable Table(string given, string returned, string fallback = null)
        {
            return new ExchangeTable(new Dictionary<string, string> { { given, returned } }, fallback);
        }

        [SetUp]
        public void SetUp()
        {
            _log = new ExchangeLog();
        }

        [Test]
        public void LeakedRooms_NewLog_IsEmpty()
        {
            Assert.That(_log.LeakedRooms, Is.Empty);
        }

        [Test]
        public void Share_FirstTime_AcceptedWithMappedReturn()
        {
            NpcProfile cousin = Npc("npc_cousin", false, Table("clue_uncle_01", "clue_evidence_01"));

            ShareResult result = _log.Share(cousin, "clue_uncle_01", RoomId.LivingRoom);

            Assert.That(result.Outcome, Is.EqualTo(ShareOutcome.Accepted));
            Assert.That(result.ReturnedClueId, Is.EqualTo("clue_evidence_01"));
        }

        [Test]
        public void Share_UnmappedClue_AcceptedWithFallbackReturn()
        {
            NpcProfile mother = Npc("npc_mother", false, Table("clue_kitchen_01", "clue_livingroom_01", "clue_useless_01"));

            ShareResult result = _log.Share(mother, "clue_bedroom_01", RoomId.Bedroom);

            Assert.That(result.Outcome, Is.EqualTo(ShareOutcome.Accepted));
            Assert.That(result.ReturnedClueId, Is.EqualTo("clue_useless_01"));
        }

        [Test]
        public void Share_UnmappedClueWithoutFallback_AcceptedWithNoReturn()
        {
            NpcProfile mother = Npc("npc_mother", false, Table("clue_kitchen_01", "clue_livingroom_01"));

            ShareResult result = _log.Share(mother, "clue_bedroom_01", RoomId.Bedroom);

            Assert.That(result.Outcome, Is.EqualTo(ShareOutcome.Accepted));
            Assert.That(result.ReturnedClueId, Is.Null);
        }

        [Test]
        public void Share_SameClueSameNpc_AlreadySharedAndStateUnchanged()
        {
            NpcProfile uncle = Npc("npc_uncle", true, Table("clue_kitchen_01", "clue_livingroom_01"));
            _log.Share(uncle, "clue_kitchen_01", RoomId.Kitchen);

            ShareResult second = _log.Share(uncle, "clue_kitchen_01", RoomId.Kitchen);

            Assert.That(second.Outcome, Is.EqualTo(ShareOutcome.AlreadyShared));
            Assert.That(second.ReturnedClueId, Is.Null);
            Assert.That(second.LeakedNewRoom, Is.False);
            Assert.That(_log.LeakedRooms, Has.Count.EqualTo(1));
        }

        [Test]
        public void Share_SameClueDifferentNpc_Accepted()
        {
            NpcProfile mother = Npc("npc_mother", false);
            NpcProfile cousin = Npc("npc_cousin", false);
            _log.Share(mother, "clue_kitchen_01", RoomId.Kitchen);

            ShareResult result = _log.Share(cousin, "clue_kitchen_01", RoomId.Kitchen);

            Assert.That(result.Outcome, Is.EqualTo(ShareOutcome.Accepted));
        }

        [Test]
        public void Share_WithLeakerNpc_LeaksClueRoom()
        {
            NpcProfile uncle = Npc("npc_uncle", true);

            ShareResult result = _log.Share(uncle, "clue_kitchen_01", RoomId.Kitchen);

            Assert.That(result.LeakedNewRoom, Is.True);
            Assert.That(result.LeakedRoom, Is.EqualTo(RoomId.Kitchen));
            Assert.That(_log.LeakedRooms, Is.EquivalentTo(new[] { RoomId.Kitchen }));
        }

        [Test]
        public void Share_WithLoyalNpc_DoesNotLeak()
        {
            NpcProfile cousin = Npc("npc_cousin", false);

            ShareResult result = _log.Share(cousin, "clue_kitchen_01", RoomId.Kitchen);

            Assert.That(result.LeakedNewRoom, Is.False);
            Assert.That(_log.LeakedRooms, Is.Empty);
        }

        [Test]
        public void Share_SecondClueFromSameRoomWithLeaker_ReportsNoNewLeak()
        {
            NpcProfile uncle = Npc("npc_uncle", true);
            _log.Share(uncle, "clue_kitchen_01", RoomId.Kitchen);

            ShareResult result = _log.Share(uncle, "clue_kitchen_02", RoomId.Kitchen);

            Assert.That(result.Outcome, Is.EqualTo(ShareOutcome.Accepted));
            Assert.That(result.LeakedNewRoom, Is.False);
            Assert.That(_log.LeakedRooms, Has.Count.EqualTo(1));
        }

        [Test]
        public void Share_NullNpc_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _log.Share(null, "clue_kitchen_01", RoomId.Kitchen));
        }

        [Test]
        public void Share_NullOrEmptyClueId_Throws()
        {
            NpcProfile mother = Npc("npc_mother", false);

            Assert.Throws<ArgumentException>(() => _log.Share(mother, null, RoomId.Kitchen));
            Assert.Throws<ArgumentException>(() => _log.Share(mother, string.Empty, RoomId.Kitchen));
            Assert.Throws<ArgumentException>(() => _log.Share(mother, "   ", RoomId.Kitchen));
        }

        [Test]
        public void HasShared_ReflectsShareHistory()
        {
            NpcProfile mother = Npc("npc_mother", false);
            NpcProfile cousin = Npc("npc_cousin", false);

            _log.Share(mother, "clue_kitchen_01", RoomId.Kitchen);

            Assert.That(_log.HasShared("npc_mother", "clue_kitchen_01"), Is.True);
            Assert.That(_log.HasShared("npc_cousin", "clue_kitchen_01"), Is.False);
            Assert.That(_log.HasShared("npc_mother", "clue_bedroom_01"), Is.False);
        }
    }
}
