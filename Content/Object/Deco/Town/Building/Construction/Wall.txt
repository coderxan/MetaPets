using System;

namespace Server.Items
{
    public enum DarkWoodWallTypes
    {
        Corner,
        SouthWall,
        EastWall,
        CornerPost,
        EastDoorFrame,
        SouthDoorFrame,
        WestDoorFrame,
        NorthDoorFrame,
        SouthWindow,
        EastWindow,
        CornerMedium,
        EastWallMedium,
        SouthWallMedium,
        CornerPostMedium,
        CornerShort,
        EastWallShort,
        SouthWallShort,
        CornerPostShort,
        SouthWallVShort,
        EastWallVShort
    }

    public class DarkWoodWall : BaseWall
    {
        [Constructable]
        public DarkWoodWall(DarkWoodWallTypes type)
            : base(0x0006 + (int)type)
        {
        }

        public DarkWoodWall(Serial serial)
            : base(serial)
        {
        }

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

    public enum ThinBrickWallTypes
    {
        Corner,
        SouthWall,
        EastWall,
        CornerPost,
        EastDoorFrame,
        SouthDoorFrame,
        WestDoorFrame,
        NorthDoorFrame,
        SouthWindow,
        EastWindow,
        CornerMedium,
        SouthWallMedium,
        EastWallMedium,
        CornerPostMedium,
        CornerShort,
        SouthWallShort,
        EastWallShort,
        CornerPostShort,
        CornerArch,
        SouthArch,
        WestArch,
        EastArch,
        NorthArch,
        SouthCenterArchTall,
        EastCenterArchTall,
        EastCornerArchTall,
        SouthCornerArchTall,
        SouthCornerArch,
        EastCornerArch,
        SouthCenterArch,
        EastCenterArch,
        CornerVVShort,
        SouthWallVVShort,
        EastWallVVShort,
        SouthWallVShort,
        EastWallVShort
    };

    public class ThinBrickWall : BaseWall
    {
        [Constructable]
        public ThinBrickWall(ThinBrickWallTypes type)
            : base(0x0033 + (int)type)
        {
        }

        public ThinBrickWall(Serial serial)
            : base(serial)
        {
        }

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

    public enum WhiteStoneWallTypes
    {
        EastWall,
        SouthWall,
        SECorner,
        NWCornerPost,
        EastArrowLoop,
        SouthArrowLoop,
        EastWindow,
        SouthWindow,
        SouthWallMedium,
        EastWallMedium,
        SECornerMedium,
        NWCornerPostMedium,
        SouthWallShort,
        EastWallShort,
        SECornerShort,
        NWCornerPostShort,
        NECornerPostShort,
        SWCornerPostShort,
        SouthWallVShort,
        EastWallVShort,
        SECornerVShort,
        NWCornerPostVShort,
        SECornerArch,
        SouthArch,
        WestArch,
        EastArch,
        NorthArch,
        EastBattlement,
        SECornerBattlement,
        SouthBattlement,
        NECornerBattlement,
        SWCornerBattlement,
        Column,
        SouthWallVVShort,
        EastWallVVShort
    }

    public class WhiteStoneWall : BaseWall
    {
        [Constructable]
        public WhiteStoneWall(WhiteStoneWallTypes type)
            : base(0x0057 + (int)type)
        {
        }

        public WhiteStoneWall(Serial serial)
            : base(serial)
        {
        }

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

    public enum ThinStoneWallTypes
    {
        Corner,
        EastWall,
        SouthWall,
        CornerPost,
        EastDoorFrame,
        SouthDoorFrame,
        NorthDoorFrame,
        WestDoorFrame,
        SouthWindow,
        EastWindow,
        CornerMedium,
        SouthWallMedium,
        EastWallMedium,
        CornerPostMedium,
        CornerArch,
        EastArch,
        SouthArch,
        NorthArch,
        WestArch,
        CornerShort,
        EastWallShort,
        SouthWallShort,
        CornerPostShort,
        SouthWallShort2,
        EastWallShort2
    }

    public class ThinStoneWall : BaseWall
    {
        [Constructable]
        public ThinStoneWall(ThinStoneWallTypes type)
            : base(0x001A + (int)type)
        {
        }

        public ThinStoneWall(Serial serial)
            : base(serial)
        {
        }

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

    public enum ThickGrayStoneWallTypes
    {
        WestArch,
        NorthArch,
        SouthArchTop,
        EastArchTop,
        EastArch,
        SouthArch,
        Wall1,
        Wall2,
        Wall3,
        SouthWindow,
        Wall4,
        EastWindow,
        WestArch2,
        NorthArch2,
        SouthArchTop2,
        EastArchTop2,
        EastArch2,
        SouthArch2,
        SWArchEdge2,
        SouthWindow2,
        NEArchEdge2,
        EastWindow2
    }

    public class ThickGrayStoneWall : BaseWall
    {
        [Constructable]
        public ThickGrayStoneWall(ThickGrayStoneWallTypes type)
            : base(0x007A + (int)type)
        {
        }

        public ThickGrayStoneWall(Serial serial)
            : base(serial)
        {
        }

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