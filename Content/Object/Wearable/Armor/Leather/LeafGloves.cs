﻿using System;

using Server.Items;

namespace Server.Items
{
    [Flipable]
    public class LeafGloves : BaseArmor, IArcaneEquip
    {
        public override Race RequiredRace { get { return Race.Elf; } }
        public override int BasePhysicalResistance { get { return 2; } }
        public override int BaseFireResistance { get { return 3; } }
        public override int BaseColdResistance { get { return 2; } }
        public override int BasePoisonResistance { get { return 4; } }
        public override int BaseEnergyResistance { get { return 4; } }

        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 40; } }

        public override int AosStrReq { get { return 10; } }
        public override int OldStrReq { get { return 10; } }

        public override int ArmorBase { get { return 13; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        [Constructable]
        public LeafGloves()
            : base(0x2FC6)
        {
            Weight = 2.0;
        }

        public LeafGloves(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            if (IsArcane)
            {
                writer.Write(true);
                writer.Write((int)m_CurArcaneCharges);
                writer.Write((int)m_MaxArcaneCharges);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        if (reader.ReadBool())
                        {
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();

                            if (Hue == 2118)
                                Hue = ArcaneGem.DefaultArcaneHue;
                        }

                        break;
                    }
            }
        }

        #region Arcane Impl
        private int m_MaxArcaneCharges, m_CurArcaneCharges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxArcaneCharges
        {
            get { return m_MaxArcaneCharges; }
            set { m_MaxArcaneCharges = value; InvalidateProperties(); Update(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurArcaneCharges
        {
            get { return m_CurArcaneCharges; }
            set { m_CurArcaneCharges = value; InvalidateProperties(); Update(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsArcane
        {
            get { return (m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0); }
        }

        public void Update()
        {
            if (IsArcane)
                ItemID = 0x26B0; // TODO: Check
            else if (ItemID == 0x26B0)
                ItemID = 0x2FC6;

            if (IsArcane && CurArcaneCharges == 0)
                Hue = 0;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (IsArcane)
                list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (IsArcane)
                LabelTo(from, 1061837, String.Format("{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges));
        }

        public void Flip()
        {
            if (ItemID == 0x2FC6)
                ItemID = 0x317C;
            else if (ItemID == 0x317C)
                ItemID = 0x2FC6;
        }
        #endregion
    }
}