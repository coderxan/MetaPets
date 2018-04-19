using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Sledge (Buc's Den)")]
    public class Sledge : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074188, // Weakling! You are not up to the task I have.
                1074195  // You there, in the stupid hat!   Come here.
            ));
        }

        [Constructable]
        public Sledge()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Sledge";
            Title = "the Versatile";
            Body = 400;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            AddItem(new Tunic(Utility.RandomNeutralHue()));
            AddItem(new LongPants(Utility.RandomBlueHue()));
            AddItem(new Cloak(Utility.RandomBrightHue()));
            AddItem(new ElvenBoots(Utility.RandomNeutralHue()));
            AddItem(new Backpack());
        }

        public Sledge(Serial serial)
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