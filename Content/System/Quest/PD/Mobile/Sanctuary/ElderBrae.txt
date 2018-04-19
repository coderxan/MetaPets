using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Elder Brae (Sanctuary)")]
    public class ElderBrae : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074215, // Don’t test my patience you sniveling worm!
                1074218  // Hey!  I want to talk to you, now.
            ));
        }

        [Constructable]
        public ElderBrae()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2.0)
        {
            Name = "Elder Brae";
            Title = "the wise";
            Race = Race.Elf;
            Female = true;
            Body = 606;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new GemmedCirclet());
            AddItem(new FemaleElvenRobe(Utility.RandomBrightHue()));
            AddItem(new ElvenBoots(Utility.RandomAnimalHue()));
        }

        public ElderBrae(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}