﻿using System;
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
    public class Daelas : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Daelas()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Daelas";
            Title = "the arborist";
            Race = Race.Elf;
            BodyValue = 0x25D;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new ElvenBoots(0x901));
            AddItem(new ElvenPants(0x8AB));

            Item item;

            item = new LeafChest();
            item.Hue = 0x8B0;
            AddItem(item);

            item = new LeafGloves();
            item.Hue = 0x1BB;
            AddItem(item);

        }

        public Daelas(Serial serial)
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