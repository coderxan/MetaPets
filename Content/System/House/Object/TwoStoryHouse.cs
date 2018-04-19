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
    public class TwoStoryHouse : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[] { new Rectangle2D(-7, 0, 14, 7), new Rectangle2D(-7, -7, 9, 7), new Rectangle2D(-4, 7, 4, 1) };

        public override Rectangle2D[] Area { get { return AreaArray; } }
        public override Point3D BaseBanLocation { get { return new Point3D(2, 8, 0); } }

        public override int DefaultPrice { get { return 192400; } }

        public TwoStoryHouse(Mobile owner, int id)
            : base(id, owner, 1370, 10)
        {
            uint keyValue = CreateKeys(owner);

            AddSouthDoors(-3, 6, 7, keyValue);

            SetSign(2, 8, 16);

            AddSouthDoor(-3, 0, 7);
            AddSouthDoor(id == 0x76 ? -2 : -3, 0, 27);
        }

        public TwoStoryHouse(Serial serial)
            : base(serial)
        {
        }

        public override HouseDeed GetDeed()
        {
            switch (ItemID)
            {
                case 0x76: return new TwoStoryWoodPlasterHouseDeed();
                case 0x78:
                default: return new TwoStoryStonePlasterHouseDeed();
            }
        }

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
    public class TwoStoryWoodPlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public TwoStoryWoodPlasterHouseDeed()
            : base(0x76, new Point3D(-3, 7, 0))
        {
        }

        public TwoStoryWoodPlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new TwoStoryHouse(owner, 0x76);
        }

        public override int LabelNumber { get { return 1041220; } }
        public override Rectangle2D[] Area { get { return TwoStoryHouse.AreaArray; } }

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

    public class TwoStoryStonePlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public TwoStoryStonePlasterHouseDeed()
            : base(0x78, new Point3D(-3, 7, 0))
        {
        }

        public TwoStoryStonePlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new TwoStoryHouse(owner, 0x78);
        }

        public override int LabelNumber { get { return 1041221; } }
        public override Rectangle2D[] Area { get { return TwoStoryHouse.AreaArray; } }

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