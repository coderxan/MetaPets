using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Iosep (Jhelom)")]
    public class Iosep : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074209, // Hey, could you help me out with something?
                1074215  // Don’t test my patience you sniveling worm!
            ));
        }

        [Constructable]
        public Iosep()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.2, 0.4)
        {
            Name = "Iosep";
            Title = "the Exporter";
            Body = 400;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            AddItem(new FancyShirt(Utility.RandomBlueHue()));
            AddItem(new LongPants(0x1BB));
            AddItem(new Shoes(Utility.RandomNeutralHue()));
            AddItem(new Backpack());
        }

        public Iosep(Serial serial)
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

            reader.ReadInt();
        }
    }
}