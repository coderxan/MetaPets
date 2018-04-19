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
    public class Kashiel : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Kashiel()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Kashiel";
            Title = "the archer";
            Race = Race.Human;
            BodyValue = 0x191;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new Backpack());

            Item item;

            item = new LeatherChest();
            item.Hue = 0x1BB;
            AddItem(item);

            item = new LeatherLegs();
            item.Hue = 0x901;
            AddItem(item);

            item = new LeatherArms();
            item.Hue = 0x901;
            AddItem(item);

            item = new LeatherGloves();
            item.Hue = 0x1BB;
            AddItem(item);

            AddItem(new Boots(0x1BB));
            AddItem(new CompositeBow());
        }

        public Kashiel(Serial serial)
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