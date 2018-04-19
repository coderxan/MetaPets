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
    public class SmallOldHouse : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[] { new Rectangle2D(-3, -3, 7, 7), new Rectangle2D(-1, 4, 3, 1) };

        public override Rectangle2D[] Area { get { return AreaArray; } }
        public override Point3D BaseBanLocation { get { return new Point3D(2, 4, 0); } }

        public override int DefaultPrice { get { return 43800; } }

        public override HousePlacementEntry ConvertEntry { get { return HousePlacementEntry.TwoStoryFoundations[0]; } }

        public SmallOldHouse(Mobile owner, int id)
            : base(id, owner, 425, 3)
        {
            uint keyValue = CreateKeys(owner);

            AddSouthDoor(0, 3, 7, keyValue);

            SetSign(2, 4, 5);
        }

        public SmallOldHouse(Serial serial)
            : base(serial)
        {
        }

        public override HouseDeed GetDeed()
        {
            switch (ItemID)
            {
                case 0x64: return new StonePlasterHouseDeed();
                case 0x66: return new FieldStoneHouseDeed();
                case 0x68: return new SmallBrickHouseDeed();
                case 0x6A: return new WoodHouseDeed();
                case 0x6C: return new WoodPlasterHouseDeed();
                case 0x6E:
                default: return new ThatchedRoofCottageDeed();
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
    public class StonePlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public StonePlasterHouseDeed()
            : base(0x64, new Point3D(0, 4, 0))
        {
        }

        public StonePlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x64);
        }

        public override int LabelNumber { get { return 1041211; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

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

    public class FieldStoneHouseDeed : HouseDeed
    {
        [Constructable]
        public FieldStoneHouseDeed()
            : base(0x66, new Point3D(0, 4, 0))
        {
        }

        public FieldStoneHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x66);
        }

        public override int LabelNumber { get { return 1041212; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

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

    public class SmallBrickHouseDeed : HouseDeed
    {
        [Constructable]
        public SmallBrickHouseDeed()
            : base(0x68, new Point3D(0, 4, 0))
        {
        }

        public SmallBrickHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x68);
        }

        public override int LabelNumber { get { return 1041213; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

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

    public class WoodHouseDeed : HouseDeed
    {
        [Constructable]
        public WoodHouseDeed()
            : base(0x6A, new Point3D(0, 4, 0))
        {
        }

        public WoodHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x6A);
        }

        public override int LabelNumber { get { return 1041214; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

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

    public class WoodPlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public WoodPlasterHouseDeed()
            : base(0x6C, new Point3D(0, 4, 0))
        {
        }

        public WoodPlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x6C);
        }

        public override int LabelNumber { get { return 1041215; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

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

    public class ThatchedRoofCottageDeed : HouseDeed
    {
        [Constructable]
        public ThatchedRoofCottageDeed()
            : base(0x6E, new Point3D(0, 4, 0))
        {
        }

        public ThatchedRoofCottageDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x6E);
        }

        public override int LabelNumber { get { return 1041216; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

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