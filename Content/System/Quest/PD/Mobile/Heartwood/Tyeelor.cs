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
    public class Tyeelor : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Tyeelor()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Tyeelor";
            Title = "the expeditionist";
            Race = Race.Elf;
            BodyValue = 0x25D;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new ElvenBoots(0x1BB));

            Item item;

            item = new WoodlandLegs();
            item.Hue = 0x236;
            AddItem(item);

            item = new WoodlandChest();
            item.Hue = 0x236;
            AddItem(item);

            item = new WoodlandArms();
            item.Hue = 0x236;
            AddItem(item);

            item = new VultureHelm();
            item.Hue = 0x236;
            AddItem(item);

            item = new WoodlandBelt();
            item.Hue = 0x236;
            AddItem(item);

        }

        public Tyeelor(Serial serial)
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