using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Synaeva (The Heartwood)")]
    public class Synaeva : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1074223); // Have you done it yet?  Oh, I haven’t told you, have I?
        }

        [Constructable]
        public Synaeva()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2.0)
        {
            Name = "Synaeva";
            Title = "the arcanist";
            Race = Race.Elf;
            Female = true;
            Body = 606;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            Item item = new RavenHelm();
            item.Hue = Utility.RandomGreenHue();
            AddItem(item);

            AddItem(new FemaleLeafChest());
            AddItem(new LeafArms());
            AddItem(new LeafTonlet());
            AddItem(new ElvenBoots());
            AddItem(new WildStaff());
        }

        public Synaeva(Serial serial)
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