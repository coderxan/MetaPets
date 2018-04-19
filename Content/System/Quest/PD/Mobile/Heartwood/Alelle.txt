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
    public class Alelle : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Alelle()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Alelle";
            Title = "the arborist";
            Race = Race.Elf;
            BodyValue = 0x25E;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new ElvenBoots(0x1BB));

            Item item;

            item = new FemaleLeafChest();
            item.Hue = 0x3A;
            AddItem(item);

            item = new LeafLegs();
            item.Hue = 0x74C;
            AddItem(item);

            item = new LeafGloves();
            item.Hue = 0x1BB;
            AddItem(item);

        }

        public Alelle(Serial serial)
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