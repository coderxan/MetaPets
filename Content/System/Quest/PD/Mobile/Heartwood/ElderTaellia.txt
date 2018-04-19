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
    public class ElderTaellia : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public ElderTaellia()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Elder Taellia";
            Title = "the wise";
            Race = Race.Elf;
            BodyValue = 0x25E;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new ThighBoots(0x127));
            AddItem(new FemaleElvenRobe(Utility.RandomBrightHue()));
            AddItem(new MagicWand());
            AddItem(new Circlet());

        }

        public ElderTaellia(Serial serial)
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