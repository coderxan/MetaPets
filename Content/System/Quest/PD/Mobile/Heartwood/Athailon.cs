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
    public class Athailon : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Athailon()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Athailon";
            Title = "the expeditionist";
            Race = Race.Elf;
            BodyValue = 0x25E;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new ElvenBoots(0x901));
            AddItem(new WoodlandBelt());
            AddItem(new DiamondMace());

            Item item;

            item = new WoodlandLegs();
            item.Hue = 0x3B2;
            AddItem(item);

            item = new FemaleElvenPlateChest();
            item.Hue = 0x3B2;
            AddItem(item);

            item = new WoodlandArms();
            item.Hue = 0x3B2;
            AddItem(item);

            item = new WingedHelm();
            item.Hue = 0x3B2;
            AddItem(item);

        }

        public Athailon(Serial serial)
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