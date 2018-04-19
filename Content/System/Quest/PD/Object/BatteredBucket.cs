using System;

using Server;
using Server.Engines.MLQuests;
using Server.Engines.MLQuests.Items;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;

namespace Server.Engines.MLQuests.Definitions
{
    public class BatteredBucket : TransientQuestGiverItem
    {
        // Original label, doesn't fit the expiration message well
        //public override int LabelNumber { get { return 1073129; } } // A battered bucket.

        public override string DefaultName { get { return "battered bucket"; } }

        [Constructable]
        public BatteredBucket()
            : base(0x2004, TimeSpan.FromMinutes(10))
        {
            LootType = LootType.Blessed;
        }

        public BatteredBucket(Serial serial)
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