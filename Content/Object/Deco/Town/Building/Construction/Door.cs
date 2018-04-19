using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;

namespace Server.Items
{
    public enum DoorFacing
    {
        WestCW,
        EastCCW,
        WestCCW,
        EastCW,
        SouthCW,
        NorthCCW,
        SouthCCW,
        NorthCW,
        //Sliding Doors
        SouthSW,
        SouthSE,
        WestSS,
        WestSN
    }

    /// <summary>
    /// Town Doors
    /// </summary>
    public class LightWoodDoor : BaseDoor
    {
        [Constructable]
        public LightWoodDoor(DoorFacing facing)
            : base(0x6D5 + (2 * (int)facing), 0x6D6 + (2 * (int)facing), 0xEA, 0xF1, BaseDoor.GetOffset(facing))
        {
        }

        public LightWoodDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class StrongWoodDoor : BaseDoor
    {
        [Constructable]
        public StrongWoodDoor(DoorFacing facing)
            : base(0x6E5 + (2 * (int)facing), 0x6E6 + (2 * (int)facing), 0xEA, 0xF1, BaseDoor.GetOffset(facing))
        {
        }

        public StrongWoodDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class DarkWoodDoor : BaseDoor
    {
        [Constructable]
        public DarkWoodDoor(DoorFacing facing)
            : base(0x6A5 + (2 * (int)facing), 0x6A6 + (2 * (int)facing), 0xEA, 0xF1, BaseDoor.GetOffset(facing))
        {
        }

        public DarkWoodDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class MediumWoodDoor : BaseDoor
    {
        [Constructable]
        public MediumWoodDoor(DoorFacing facing)
            : base(0x6B5 + (2 * (int)facing), 0x6B6 + (2 * (int)facing), 0xEA, 0xF1, BaseDoor.GetOffset(facing))
        {
        }

        public MediumWoodDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class IronGateShort : BaseDoor
    {
        [Constructable]
        public IronGateShort(DoorFacing facing)
            : base(0x84c + (2 * (int)facing), 0x84d + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public IronGateShort(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class LightWoodGate : BaseDoor
    {
        [Constructable]
        public LightWoodGate(DoorFacing facing)
            : base(0x839 + (2 * (int)facing), 0x83A + (2 * (int)facing), 0xEB, 0xF2, BaseDoor.GetOffset(facing))
        {
        }

        public LightWoodGate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class DarkWoodGate : BaseDoor
    {
        [Constructable]
        public DarkWoodGate(DoorFacing facing)
            : base(0x866 + (2 * (int)facing), 0x867 + (2 * (int)facing), 0xEB, 0xF2, BaseDoor.GetOffset(facing))
        {
        }

        public DarkWoodGate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class IronGate : BaseDoor
    {
        [Constructable]
        public IronGate(DoorFacing facing)
            : base(0x824 + (2 * (int)facing), 0x825 + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public IronGate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class MetalDoor : BaseDoor
    {
        [Constructable]
        public MetalDoor(DoorFacing facing)
            : base(0x675 + (2 * (int)facing), 0x676 + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public MetalDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class BarredMetalDoor : BaseDoor
    {
        [Constructable]
        public BarredMetalDoor(DoorFacing facing)
            : base(0x685 + (2 * (int)facing), 0x686 + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public BarredMetalDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class BarredMetalDoor2 : BaseDoor
    {
        [Constructable]
        public BarredMetalDoor2(DoorFacing facing)
            : base(0x1FED + (2 * (int)facing), 0x1FEE + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public BarredMetalDoor2(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class RattanDoor : BaseDoor
    {
        [Constructable]
        public RattanDoor(DoorFacing facing)
            : base(0x695 + (2 * (int)facing), 0x696 + (2 * (int)facing), 0xEB, 0xF2, BaseDoor.GetOffset(facing))
        {
        }

        public RattanDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class MetalDoor2 : BaseDoor
    {
        [Constructable]
        public MetalDoor2(DoorFacing facing)
            : base(0x6C5 + (2 * (int)facing), 0x6C6 + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public MetalDoor2(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    /// <summary>
    /// House Doors
    /// </summary>
    public class MetalHouseDoor : BaseHouseDoor
    {
        [Constructable]
        public MetalHouseDoor(DoorFacing facing)
            : base(facing, 0x675 + (2 * (int)facing), 0x676 + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset(facing))
        {
        }

        public MetalHouseDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class DarkWoodHouseDoor : BaseHouseDoor
    {
        [Constructable]
        public DarkWoodHouseDoor(DoorFacing facing)
            : base(facing, 0x6A5 + (2 * (int)facing), 0x6A6 + (2 * (int)facing), 0xEA, 0xF1, BaseDoor.GetOffset(facing))
        {
        }

        public DarkWoodHouseDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class GenericHouseDoor : BaseHouseDoor
    {
        [Constructable]
        public GenericHouseDoor(DoorFacing facing, int baseItemID, int openedSound, int closedSound)
            : this(facing, baseItemID, openedSound, closedSound, true)
        {
        }

        [Constructable]
        public GenericHouseDoor(DoorFacing facing, int baseItemID, int openedSound, int closedSound, bool autoAdjust)
            : base(facing, baseItemID + (autoAdjust ? (2 * (int)facing) : 0), baseItemID + 1 + (autoAdjust ? (2 * (int)facing) : 0), openedSound, closedSound, BaseDoor.GetOffset(facing))
        {
        }

        public GenericHouseDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    /// <summary>
    /// Castle Gate
    /// </summary>
    public class PortcullisNS : BaseDoor
    {
        public override bool UseChainedFunctionality { get { return true; } }

        [Constructable]
        public PortcullisNS()
            : base(0x6F5, 0x6F5, 0xF0, 0xEF, new Point3D(0, 0, 20))
        {
        }

        public PortcullisNS(Serial serial)
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

    public class PortcullisEW : BaseDoor
    {
        public override bool UseChainedFunctionality { get { return true; } }

        [Constructable]
        public PortcullisEW()
            : base(0x6F6, 0x6F6, 0xF0, 0xEF, new Point3D(0, 0, 20))
        {
        }

        public PortcullisEW(Serial serial)
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

    /// <summary>
    /// Secret Doors
    /// </summary>
    public class SecretStoneDoor1 : BaseDoor
    {
        [Constructable]
        public SecretStoneDoor1(DoorFacing facing)
            : base(0xE8 + (2 * (int)facing), 0xE9 + (2 * (int)facing), 0xED, 0xF4, BaseDoor.GetOffset(facing))
        {
        }

        public SecretStoneDoor1(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SecretDungeonDoor : BaseDoor
    {
        [Constructable]
        public SecretDungeonDoor(DoorFacing facing)
            : base(0x314 + (2 * (int)facing), 0x315 + (2 * (int)facing), 0xED, 0xF4, BaseDoor.GetOffset(facing))
        {
        }

        public SecretDungeonDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SecretStoneDoor2 : BaseDoor
    {
        [Constructable]
        public SecretStoneDoor2(DoorFacing facing)
            : base(0x324 + (2 * (int)facing), 0x325 + (2 * (int)facing), 0xED, 0xF4, BaseDoor.GetOffset(facing))
        {
        }

        public SecretStoneDoor2(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SecretWoodenDoor : BaseDoor
    {
        [Constructable]
        public SecretWoodenDoor(DoorFacing facing)
            : base(0x334 + (2 * (int)facing), 0x335 + (2 * (int)facing), 0xED, 0xF4, BaseDoor.GetOffset(facing))
        {
        }

        public SecretWoodenDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SecretLightWoodDoor : BaseDoor
    {
        [Constructable]
        public SecretLightWoodDoor(DoorFacing facing)
            : base(0x344 + (2 * (int)facing), 0x345 + (2 * (int)facing), 0xED, 0xF4, BaseDoor.GetOffset(facing))
        {
        }

        public SecretLightWoodDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SecretStoneDoor3 : BaseDoor
    {
        [Constructable]
        public SecretStoneDoor3(DoorFacing facing)
            : base(0x354 + (2 * (int)facing), 0x355 + (2 * (int)facing), 0xED, 0xF4, BaseDoor.GetOffset(facing))
        {
        }

        public SecretStoneDoor3(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // Default Serialize method
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader) // Default Deserialize method
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}