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
    [QuesterName("The Ghost of Frederic Smithson")]
    public class Frederic : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Frederic()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "The Ghost of Frederic Smithson";
            BodyValue = 0x1A;
            Hue = 0x455;
            Frozen = true;

            InitStats(100, 100, 25);

        }

        public Frederic(Serial serial)
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