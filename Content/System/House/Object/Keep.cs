using System;
using System.Collections;

using Server;
using Server.Items;
using Server.Multis;
using Server.Multis.Deeds;
using Server.Regions;
using Server.Targeting;

namespace Server.Multis
{
    public class Keep : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[] { new Rectangle2D(-11, -11, 7, 8), new Rectangle2D(-11, 5, 7, 8), new Rectangle2D(6, -11, 7, 8), new Rectangle2D(6, 5, 7, 8), new Rectangle2D(-9, -3, 5, 8), new Rectangle2D(6, -3, 5, 8), new Rectangle2D(-4, -9, 10, 20), new Rectangle2D(-1, 11, 4, 1) };

        public override int DefaultPrice { get { return 665200; } }

        public override Rectangle2D[] Area { get { return AreaArray; } }
        public override Point3D BaseBanLocation { get { return new Point3D(5, 13, 0); } }

        public Keep(Mobile owner)
            : base(0x7C, owner, 2625, 18)
        {
            uint keyValue = CreateKeys(owner);

            AddSouthDoors(false, 0, 10, 6, keyValue);

            SetSign(5, 12, 16);
        }

        public Keep(Serial serial)
            : base(serial)
        {
        }

        public override HouseDeed GetDeed() { return new KeepDeed(); }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);//version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}

namespace Server.Multis.Deeds
{
    public class KeepDeed : HouseDeed
    {
        [Constructable]
        public KeepDeed()
            : base(0x7C, new Point3D(0, 11, 0))
        {
        }

        public KeepDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new Keep(owner);
        }

        public override int LabelNumber { get { return 1041223; } }
        public override Rectangle2D[] Area { get { return Keep.AreaArray; } }

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