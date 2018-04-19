using System;

using Server;
using Server.Items;

namespace Server.Items
{
    [FlipableAttribute(0x1451, 0x1456)]
    public class DaemonHelm : BaseArmor
    {
        public override int BasePhysicalResistance { get { return 6; } }
        public override int BaseFireResistance { get { return 6; } }
        public override int BaseColdResistance { get { return 7; } }
        public override int BasePoisonResistance { get { return 5; } }
        public override int BaseEnergyResistance { get { return 7; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int AosStrReq { get { return 20; } }
        public override int OldStrReq { get { return 40; } }

        public override int ArmorBase { get { return 46; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override int LabelNumber { get { return 1041374; } } // daemon bone helmet

        [Constructable]
        public DaemonHelm()
            : base(0x1451)
        {
            Hue = 0x648;
            Weight = 3.0;

            ArmorAttributes.SelfRepair = 1;
        }

        public DaemonHelm(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Weight == 1.0)
                Weight = 3.0;

            if (ArmorAttributes.SelfRepair == 0)
                ArmorAttributes.SelfRepair = 1;
        }
    }

    [FlipableAttribute(0x144f, 0x1454)]
    public class DaemonChest : BaseArmor
    {
        public override int BasePhysicalResistance { get { return 6; } }
        public override int BaseFireResistance { get { return 6; } }
        public override int BaseColdResistance { get { return 7; } }
        public override int BasePoisonResistance { get { return 5; } }
        public override int BaseEnergyResistance { get { return 7; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int AosStrReq { get { return 60; } }
        public override int OldStrReq { get { return 40; } }

        public override int OldDexBonus { get { return -6; } }

        public override int ArmorBase { get { return 46; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override int LabelNumber { get { return 1041372; } } // daemon bone armor

        [Constructable]
        public DaemonChest()
            : base(0x144F)
        {
            Weight = 6.0;
            Hue = 0x648;

            ArmorAttributes.SelfRepair = 1;
        }

        public DaemonChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Weight == 1.0)
                Weight = 6.0;

            if (ArmorAttributes.SelfRepair == 0)
                ArmorAttributes.SelfRepair = 1;
        }
    }

    [FlipableAttribute(0x144e, 0x1453)]
    public class DaemonArms : BaseArmor
    {
        public override int BasePhysicalResistance { get { return 6; } }
        public override int BaseFireResistance { get { return 6; } }
        public override int BaseColdResistance { get { return 7; } }
        public override int BasePoisonResistance { get { return 5; } }
        public override int BaseEnergyResistance { get { return 7; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int AosStrReq { get { return 55; } }
        public override int OldStrReq { get { return 40; } }

        public override int OldDexBonus { get { return -2; } }

        public override int ArmorBase { get { return 46; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override int LabelNumber { get { return 1041371; } } // daemon bone arms

        [Constructable]
        public DaemonArms()
            : base(0x144E)
        {
            Weight = 2.0;
            Hue = 0x648;

            ArmorAttributes.SelfRepair = 1;
        }

        public DaemonArms(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            if (Weight == 1.0)
                Weight = 2.0;
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (ArmorAttributes.SelfRepair == 0)
                ArmorAttributes.SelfRepair = 1;
        }
    }

    [FlipableAttribute(0x1450, 0x1455)]
    public class DaemonGloves : BaseArmor
    {
        public override int BasePhysicalResistance { get { return 6; } }
        public override int BaseFireResistance { get { return 6; } }
        public override int BaseColdResistance { get { return 7; } }
        public override int BasePoisonResistance { get { return 5; } }
        public override int BaseEnergyResistance { get { return 7; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int AosStrReq { get { return 55; } }
        public override int OldStrReq { get { return 40; } }

        public override int OldDexBonus { get { return -1; } }

        public override int ArmorBase { get { return 46; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override int LabelNumber { get { return 1041373; } } // daemon bone gloves

        [Constructable]
        public DaemonGloves()
            : base(0x1450)
        {
            Weight = 2.0;
            Hue = 0x648;

            ArmorAttributes.SelfRepair = 1;
        }

        public DaemonGloves(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Weight == 1.0)
                Weight = 2.0;

            if (ArmorAttributes.SelfRepair == 0)
                ArmorAttributes.SelfRepair = 1;
        }
    }

    [FlipableAttribute(0x1452, 0x1457)]
    public class DaemonLegs : BaseArmor
    {
        public override int BasePhysicalResistance { get { return 6; } }
        public override int BaseFireResistance { get { return 6; } }
        public override int BaseColdResistance { get { return 7; } }
        public override int BasePoisonResistance { get { return 5; } }
        public override int BaseEnergyResistance { get { return 7; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int AosStrReq { get { return 55; } }
        public override int OldStrReq { get { return 40; } }

        public override int OldDexBonus { get { return -4; } }

        public override int ArmorBase { get { return 46; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override int LabelNumber { get { return 1041375; } } // daemon bone leggings

        [Constructable]
        public DaemonLegs()
            : base(0x1452)
        {
            Weight = 3.0;
            Hue = 0x648;

            ArmorAttributes.SelfRepair = 1;
        }

        public DaemonLegs(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (ArmorAttributes.SelfRepair == 0)
                ArmorAttributes.SelfRepair = 1;
        }
    }
}