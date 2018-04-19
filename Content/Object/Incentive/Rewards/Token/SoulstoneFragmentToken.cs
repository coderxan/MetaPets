using System;

using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class SoulstoneFragmentToken : PromotionalToken
    {
        public override Item CreateItemFor(Mobile from)
        {
            if (from != null && from.Account != null)
                return new SoulstoneFragment(from.Account.ToString());
            else
                return null;
        }

        public override TextDefinition ItemGumpName { get { return 1070999; } }// <center>Soulstone Fragment</center>
        public override TextDefinition ItemName { get { return 1071000; } }//soulstone fragment
        public override TextDefinition ItemReceiveMessage { get { return 1070976; } } // A soulstone fragment has been created in your bank box.

        [Constructable]
        public SoulstoneFragmentToken()
            : base()
        {
        }

        public SoulstoneFragmentToken(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}