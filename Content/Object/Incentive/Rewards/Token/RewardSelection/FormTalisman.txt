using System;

using Server.Mobiles;
using Server.Spells.Ninjitsu;

namespace Server.Items
{
    /// <summary>
    /// Ferret FormTalisman
    /// </summary>
    public class FerretFormTalisman : BaseFormTalisman
    {
        public override TalismanForm Form { get { return TalismanForm.Ferret; } }

        [Constructable]
        public FerretFormTalisman()
            : base()
        {
        }

        public FerretFormTalisman(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); //version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    /// <summary>
    /// Squirrel FormTalisman
    /// </summary>
    public class SquirrelFormTalisman : BaseFormTalisman
    {
        public override TalismanForm Form { get { return TalismanForm.Squirrel; } }

        [Constructable]
        public SquirrelFormTalisman()
            : base()
        {
        }

        public SquirrelFormTalisman(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); //version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    /// <summary>
    /// CuSidhe FormTalisman
    /// </summary>
    public class CuSidheFormTalisman : BaseFormTalisman
    {
        public override TalismanForm Form { get { return TalismanForm.CuSidhe; } }

        [Constructable]
        public CuSidheFormTalisman()
            : base()
        {
        }

        public CuSidheFormTalisman(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); //version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    /// <summary>
    /// Reptalon FormTalisman
    /// </summary>
    public class ReptalonFormTalisman : BaseFormTalisman
    {
        public override TalismanForm Form { get { return TalismanForm.Reptalon; } }

        [Constructable]
        public ReptalonFormTalisman()
            : base()
        {
        }

        public ReptalonFormTalisman(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); //version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}