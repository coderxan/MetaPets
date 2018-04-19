using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Jamal (near Blighted Grove)")]
    public class Jamal : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Jamal()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.2, 0.4)
        {
            Name = "Jamal";
            Title = "the Fisherman";
            Body = 400;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this);
            Utility.AssignRandomFacialHair(this, HairHue);

            AddItem(new Shirt(0x1BB));
            AddItem(new ShortPants(Utility.RandomNeutralHue()));
            AddItem(new ThighBoots(Utility.RandomAnimalHue()));
            AddItem(new Backpack());
        }

        public Jamal(Serial serial)
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