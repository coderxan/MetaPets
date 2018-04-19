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
    [QuesterName("Maul")]
    public class MaulTheBear : GrizzlyBear
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public MaulTheBear()
        {
            Name = "Maul";
            AI = AIType.AI_Vendor;
            FightMode = FightMode.None;
            Tamable = false;
        }

        public MaulTheBear(Serial serial)
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