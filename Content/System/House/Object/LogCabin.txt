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
    public class LogCabin : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[] { new Rectangle2D(-3, -6, 8, 13) };

        public override int DefaultPrice { get { return 97800; } }

        public override HousePlacementEntry ConvertEntry { get { return HousePlacementEntry.TwoStoryFoundations[12]; } }

        public override Rectangle2D[] Area { get { return AreaArray; } }
        public override Point3D BaseBanLocation { get { return new Point3D(5, 8, 0); } }

        public LogCabin(Mobile owner)
            : base(0x9A, owner, 1100, 8)
        {
            uint keyValue = CreateKeys(owner);

            AddSouthDoor(1, 4, 8, keyValue);

            SetSign(5, 8, 20);

            AddSouthDoor(1, 0, 29);
        }

        public LogCabin(Serial serial)
            : base(serial)
        {
        }

        public override HouseDeed GetDeed() { return new LogCabinDeed(); }

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
    public class LogCabinDeed : HouseDeed
    {
        [Constructable]
        public LogCabinDeed()
            : base(0x9A, new Point3D(1, 6, 0))
        {
        }

        public LogCabinDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new LogCabin(owner);
        }

        public override int LabelNumber { get { return 1041238; } }
        public override Rectangle2D[] Area { get { return LogCabin.AreaArray; } }

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