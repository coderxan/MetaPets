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
    public class LargePatioHouse : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[] { new Rectangle2D(-7, -7, 15, 14), new Rectangle2D(-5, 7, 4, 1) };

        public override int DefaultPrice { get { return 152800; } }

        public override HousePlacementEntry ConvertEntry { get { return HousePlacementEntry.ThreeStoryFoundations[29]; } }
        public override int ConvertOffsetY { get { return -1; } }

        public override Rectangle2D[] Area { get { return AreaArray; } }
        public override Point3D BaseBanLocation { get { return new Point3D(1, 8, 0); } }

        public LargePatioHouse(Mobile owner)
            : base(0x8C, owner, 1100, 8)
        {
            uint keyValue = CreateKeys(owner);

            AddSouthDoors(-4, 6, 7, keyValue);

            SetSign(1, 8, 16);

            AddEastDoor(1, 4, 7);
            AddEastDoor(1, -4, 7);
            AddSouthDoor(4, -1, 7);
        }

        public LargePatioHouse(Serial serial)
            : base(serial)
        {
        }

        public override HouseDeed GetDeed() { return new LargePatioDeed(); }

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
    public class LargePatioDeed : HouseDeed
    {
        [Constructable]
        public LargePatioDeed()
            : base(0x8C, new Point3D(-4, 7, 0))
        {
        }

        public LargePatioDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new LargePatioHouse(owner);
        }

        public override int LabelNumber { get { return 1041231; } }
        public override Rectangle2D[] Area { get { return LargePatioHouse.AreaArray; } }

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