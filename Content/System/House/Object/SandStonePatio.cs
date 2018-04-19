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
    public class SandStonePatio : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[] { new Rectangle2D(-5, -4, 12, 8), new Rectangle2D(-2, 4, 3, 1) };

        public override int DefaultPrice { get { return 90900; } }

        public override HousePlacementEntry ConvertEntry { get { return HousePlacementEntry.TwoStoryFoundations[35]; } }
        public override int ConvertOffsetY { get { return -1; } }

        public override Rectangle2D[] Area { get { return AreaArray; } }
        public override Point3D BaseBanLocation { get { return new Point3D(4, 6, 0); } }

        public SandStonePatio(Mobile owner)
            : base(0x9C, owner, 850, 6)
        {
            uint keyValue = CreateKeys(owner);

            AddSouthDoor(-1, 3, 6, keyValue);

            SetSign(4, 6, 24);
        }

        public SandStonePatio(Serial serial)
            : base(serial)
        {
        }

        public override HouseDeed GetDeed() { return new SandstonePatioDeed(); }

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
    public class SandstonePatioDeed : HouseDeed
    {
        [Constructable]
        public SandstonePatioDeed()
            : base(0x9C, new Point3D(-1, 4, 0))
        {
        }

        public SandstonePatioDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SandStonePatio(owner);
        }

        public override int LabelNumber { get { return 1041239; } }
        public override Rectangle2D[] Area { get { return SandStonePatio.AreaArray; } }

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