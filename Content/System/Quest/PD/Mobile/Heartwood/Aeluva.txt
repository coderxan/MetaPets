using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Aeluva (The Heartwood)")]
    public class Aeluva : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            /*
             * 1074206 - Excuse me please traveler, might I have a little of your time?
             * 1074207 - Good day to you friend! Allow me to offer you a fabulous opportunity!  Thrills and adventure await!
             */
            MLQuestSystem.Tell(this, pm, Utility.Random(1074206, 2));
        }

        [Constructable]
        public Aeluva()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2.0)
        {
            Name = "Aeluva";
            Title = "the arcanist";
            Race = Race.Elf;
            Female = true;
            Body = 606;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new ElvenShirt());
            AddItem(new Kilt(Utility.RandomNondyedHue())); // Note: OSI hue = 0x1516, typo?
            AddItem(new ElvenBoots());
            AddItem(new Circlet());
        }

        public Aeluva(Serial serial)
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