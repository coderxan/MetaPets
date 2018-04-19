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
    [QuesterName("Salaenih (The Heartwood)")]
    public class Salaenih : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074207, // Good day to you friend! Allow me to offer you a fabulous opportunity!  Thrills and adventure await!
                1074209 // Hey, could you help me out with something?
            ));
        }

        [Constructable]
        public Salaenih()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Salaenih";
            Title = "the expeditionist";
            Race = Race.Elf;
            BodyValue = 0x25E;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new ElvenBoots());
            AddItem(new WarCleaver());

            Item item;

            item = new WoodlandBelt();
            item.Hue = 0x597;
            AddItem(item);

            item = new VultureHelm();
            item.Hue = 0x1BB;
            AddItem(item);

            item = new WoodlandLegs();
            item.Hue = 0x1BB;
            AddItem(item);

            item = new WoodlandChest();
            item.Hue = 0x1BB;
            AddItem(item);

            item = new WoodlandArms();
            item.Hue = 0x1BB;
            AddItem(item);
        }

        public Salaenih(Serial serial)
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