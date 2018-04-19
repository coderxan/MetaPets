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
    public class ElderJothan : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public ElderJothan()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Elder Jothan";
            Title = "the wise";
            Race = Race.Elf;
            BodyValue = 0x25D;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new ThighBoots());
            AddItem(new ElvenPants(0x58D));
            AddItem(new ElvenShirt(Utility.RandomYellowHue()));
            AddItem(new Cloak(Utility.RandomBrightHue()));
            AddItem(new Circlet());

        }

        public ElderJothan(Serial serial)
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