using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Aulan (The Heartwood)")]
    public class Aulan : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074188, // Weakling! You are not up to the task I have.
                1074191, // Just keep walking away!  I thought so. Coward!  I’ll bite your legs off!
                1074195 // You there, in the stupid hat!   Come here.
            ));
        }

        [Constructable]
        public Aulan()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Aulan";
            Title = "the expeditionist";
            Race = Race.Elf;
            BodyValue = 0x25D;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            Item item;

            item = new ElvenBoots();
            item.Hue = Utility.RandomYellowHue();
            AddItem(item);

            AddItem(new ElvenPants(Utility.RandomGreenHue()));
            AddItem(new Cloak(Utility.RandomGreenHue()));
            AddItem(new Circlet());

            item = new HideChest();
            item.Hue = Utility.RandomYellowHue();
            AddItem(item);

            item = new HideGloves();
            item.Hue = Utility.RandomYellowHue();
            AddItem(item);
        }

        public Aulan(Serial serial)
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