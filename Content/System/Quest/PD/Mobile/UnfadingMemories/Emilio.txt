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
    [QuesterName("Emilio (Britain)")] // OSI's description is "Artist", not very helpful
    public class Emilio : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Emilio()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Emilio";
            Title = "the Tortured Artist";
            Race = Race.Human;
            BodyValue = 0x190;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new Backpack());
            AddItem(new Sandals(0x72B));
            AddItem(new LongPants(0x525));
            AddItem(new FancyShirt(0x53F));
            AddItem(new FloppyHat(0x58C));
            AddItem(new BodySash(0x1C));

        }

        public Emilio(Serial serial)
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