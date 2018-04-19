using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.Plants;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Quests.Matriarch
{
    public class BlackSolenMatriarch : BaseSolenMatriarch
    {
        public override bool RedSolen { get { return false; } }

        [Constructable]
        public BlackSolenMatriarch()
        {
        }

        public BlackSolenMatriarch(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}