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
    [QuesterName("Szandor")]
    public class SkeletonOfSzandor : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public SkeletonOfSzandor()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Skeleton of Szandor";
            Title = "the Late Architect";
            Hue = 0x83F2; // TODO: Random human hue? Why???
            Body = 0x32;
            InitStats(100, 100, 25);
        }

        public SkeletonOfSzandor(Serial serial)
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