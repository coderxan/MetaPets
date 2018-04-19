using System;

using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Ambitious
{
    public class BlackAmbitiousSolenQueen : BaseAmbitiousSolenQueen
    {
        public override bool RedSolen { get { return false; } }

        [Constructable]
        public BlackAmbitiousSolenQueen()
        {
        }

        public BlackAmbitiousSolenQueen(Serial serial)
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