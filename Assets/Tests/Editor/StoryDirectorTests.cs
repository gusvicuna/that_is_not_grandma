using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class StoryDirectorTests
    {
        private const string ClueKitchen = "clue_kitchen_01";
        private const string ClueBedroom = "clue_bedroom_01";
        private const string NpcUncle = "npc_uncle";
        private const string NpcCousin = "npc_cousin";

        private static StoryBeat Beat(
            string id,
            StoryTrigger trigger,
            string primary = null,
            string secondary = null,
            int number = -1,
            StoryCondition condition = null,
            bool repeatable = false)
        {
            return new StoryBeat(id, trigger, primary, secondary, number, condition, repeatable);
        }

        private static StoryDirector Director(params StoryBeat[] beats)
        {
            return new StoryDirector(beats);
        }

        [Test]
        public void Ctor_NullBeats_Throws()
        {
            Assert.That(() => new StoryDirector(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Ctor_DuplicateBeatIds_Throws()
        {
            var beats = new List<StoryBeat>
            {
                Beat("beat_intro", StoryTrigger.ClueCollected, ClueKitchen),
                Beat("beat_intro", StoryTrigger.ItemInspected, "item_sink")
            };

            Assert.That(() => new StoryDirector(beats), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Notify_MatchingTriggerAndId_FiresBeat()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.EqualTo(new[] { "beat_a" }));
        }

        [Test]
        public void Notify_DifferentTrigger_DoesNotFire()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ItemInspected, ClueKitchen));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_DifferentPrimaryId_DoesNotFire()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueBedroom));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_WildcardPrimaryId_FiresForAnyPayload()
        {
            StoryDirector director = Director(Beat("beat_any_clue", StoryTrigger.ClueCollected));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueBedroom));

            Assert.That(fired, Is.EqualTo(new[] { "beat_any_clue" }));
        }

        [Test]
        public void Notify_ClueSharedWithMatchingNpc_FiresBeat()
        {
            StoryDirector director = Director(
                Beat("beat_uncle_reacts", StoryTrigger.ClueShared, secondary: NpcUncle));

            IReadOnlyList<string> fired = director.Notify(
                new StoryEvent(StoryTrigger.ClueShared, ClueKitchen, NpcUncle));

            Assert.That(fired, Is.EqualTo(new[] { "beat_uncle_reacts" }));
        }

        [Test]
        public void Notify_ClueSharedWithOtherNpc_DoesNotFire()
        {
            StoryDirector director = Director(
                Beat("beat_uncle_reacts", StoryTrigger.ClueShared, secondary: NpcUncle));

            IReadOnlyList<string> fired = director.Notify(
                new StoryEvent(StoryTrigger.ClueShared, ClueKitchen, NpcCousin));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_BeatAlreadyFired_DoesNotFireAgain()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));
            director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.Empty);
            Assert.That(director.HasFired("beat_a"), Is.True);
        }

        [Test]
        public void Notify_RepeatableBeat_FiresEveryTime()
        {
            StoryDirector director = Director(
                Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen, repeatable: true));
            director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.EqualTo(new[] { "beat_a" }));
        }

        [Test]
        public void Notify_MissingRequiredFlag_DoesNotFire()
        {
            var condition = new StoryCondition(requiredFlags: new[] { "met_cousin" });
            StoryDirector director = Director(
                Beat("beat_gated", StoryTrigger.ClueCollected, ClueKitchen, condition: condition));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void SetFlag_ThenNotify_FiresGatedBeat()
        {
            var condition = new StoryCondition(requiredFlags: new[] { "met_cousin" });
            StoryDirector director = Director(
                Beat("beat_gated", StoryTrigger.ClueCollected, ClueKitchen, condition: condition));

            director.SetFlag("met_cousin");
            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(director.HasFlag("met_cousin"), Is.True);
            Assert.That(fired, Is.EqualTo(new[] { "beat_gated" }));
        }

        [Test]
        public void Notify_ForbiddenFlagSet_DoesNotFire()
        {
            var condition = new StoryCondition(forbiddenFlags: new[] { "grandma_arrived" });
            StoryDirector director = Director(
                Beat("beat_before_arrival", StoryTrigger.ClueCollected, ClueKitchen, condition: condition));
            director.SetFlag("grandma_arrived");

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_DayStarted_UpdatesCurrentDay()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));

            director.Notify(new StoryEvent(StoryTrigger.DayStarted, number: 3));

            Assert.That(director.CurrentDay, Is.EqualTo(3));
        }

        [Test]
        public void Notify_BelowMinDay_DoesNotFire()
        {
            var condition = new StoryCondition(minDay: 2);
            StoryDirector director = Director(
                Beat("beat_late", StoryTrigger.ClueCollected, ClueKitchen, condition: condition));
            director.Notify(new StoryEvent(StoryTrigger.DayStarted, number: 1));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_DayStartedAtMinDay_FiresOnTheSameEvent()
        {
            var condition = new StoryCondition(minDay: 2);
            StoryDirector director = Director(
                Beat("beat_phone_appears", StoryTrigger.DayStarted, number: 2, condition: condition));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.DayStarted, number: 2));

            Assert.That(fired, Is.EqualTo(new[] { "beat_phone_appears" }));
        }

        [Test]
        public void Notify_RoomEnteredMatchingRoom_FiresBeat()
        {
            StoryDirector director = Director(
                Beat("beat_bedroom", StoryTrigger.RoomEntered, number: (int)RoomId.Bedroom));

            IReadOnlyList<string> fired = director.Notify(
                new StoryEvent(StoryTrigger.RoomEntered, number: (int)RoomId.Bedroom));

            Assert.That(fired, Is.EqualTo(new[] { "beat_bedroom" }));
        }

        [Test]
        public void Notify_RoomEnteredOtherRoom_DoesNotFire()
        {
            StoryDirector director = Director(
                Beat("beat_bedroom", StoryTrigger.RoomEntered, number: (int)RoomId.Bedroom));

            IReadOnlyList<string> fired = director.Notify(
                new StoryEvent(StoryTrigger.RoomEntered, number: (int)RoomId.Kitchen));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_PoliceCallResolvedWithMatchingOutcome_FiresBeat()
        {
            StoryDirector director = Director(
                Beat("beat_police_wrong", StoryTrigger.PoliceCallResolved,
                    number: (int)PoliceCallOutcome.WrongEvidence));

            IReadOnlyList<string> fired = director.Notify(
                new StoryEvent(StoryTrigger.PoliceCallResolved, number: (int)PoliceCallOutcome.WrongEvidence));

            Assert.That(fired, Is.EqualTo(new[] { "beat_police_wrong" }));
        }

        [Test]
        public void Notify_PoliceCallResolvedWithOtherOutcome_DoesNotFire()
        {
            StoryDirector director = Director(
                Beat("beat_police_wrong", StoryTrigger.PoliceCallResolved,
                    number: (int)PoliceCallOutcome.WrongEvidence));

            IReadOnlyList<string> fired = director.Notify(
                new StoryEvent(StoryTrigger.PoliceCallResolved, number: (int)PoliceCallOutcome.Unavailable));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void Notify_SeveralMatchingBeats_FiresInDeclarationOrder()
        {
            StoryDirector director = Director(
                Beat("beat_first", StoryTrigger.ClueCollected, ClueKitchen),
                Beat("beat_second", StoryTrigger.ClueCollected),
                Beat("beat_third", StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(fired, Is.EqualTo(new[] { "beat_first", "beat_second", "beat_third" }));
        }

        [Test]
        public void Notify_NoMatchingBeat_ReturnsEmpty()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));

            IReadOnlyList<string> fired = director.Notify(new StoryEvent(StoryTrigger.RoomEntered, number: 0));

            Assert.That(fired, Is.Empty);
        }

        [Test]
        public void HasFired_BeatNeverTriggered_IsFalse()
        {
            StoryDirector director = Director(Beat("beat_a", StoryTrigger.ClueCollected, ClueKitchen));

            Assert.That(director.HasFired("beat_a"), Is.False);
        }
    }
}
