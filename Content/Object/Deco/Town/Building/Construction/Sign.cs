using System;

using Server;

namespace Server.Items
{
    public enum SignFacing
    {
        North,
        West
    }

    public enum SignType
    {
        Library,
        DarkWoodenPost,
        LightWoodenPost,
        MetalPostC,
        MetalPostB,
        MetalPostA,
        MetalPost,
        Bakery,
        Tailor,
        Tinker,
        Butcher,
        Healer,
        Mage,
        Woodworker,
        Customs,
        Inn,
        Shipwright,
        Stables,
        BarberShop,
        Bard,
        Fletcher,
        Armourer,
        Jeweler,
        Tavern,
        ReagentShop,
        Blacksmith,
        Painter,
        Provisioner,
        Bowyer,
        WoodenSign,
        BrassSign,
        ArmamentsGuild,
        ArmourersGuild,
        BlacksmithsGuild,
        WeaponsGuild,
        BardicGuild,
        BartersGuild,
        ProvisionersGuild,
        TradersGuild,
        CooksGuild,
        HealersGuild,
        MagesGuild,
        SorcerersGuild,
        IllusionistGuild,
        MinersGuild,
        ArchersGuild,
        SeamensGuild,
        FishermensGuild,
        SailorsGuild,
        ShipwrightsGuild,
        TailorsGuild,
        ThievesGuild,
        RoguesGuild,
        AssassinsGuild,
        TinkersGuild,
        WarriorsGuild,
        CavalryGuild,
        FightersGuild,
        MerchantsGuild,
        Bank,
        Theatre
    }

    public class Sign : BaseSign
    {
        [Constructable]
        public Sign(SignType type, SignFacing facing)
            : base((0xB95 + (2 * (int)type)) + (int)facing)
        {
        }

        [Constructable]
        public Sign(int itemID)
            : base(itemID)
        {
        }

        public Sign(Serial serial)
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
        }
    }

    public class LocalizedSign : Sign
    {
        private int m_LabelNumber;

        public override int LabelNumber { get { return m_LabelNumber; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Number { get { return m_LabelNumber; } set { m_LabelNumber = value; InvalidateProperties(); } }

        [Constructable]
        public LocalizedSign(SignType type, SignFacing facing, int labelNumber)
            : base((0xB95 + (2 * (int)type)) + (int)facing)
        {
            m_LabelNumber = labelNumber;
        }

        [Constructable]
        public LocalizedSign(int itemID, int labelNumber)
            : base(itemID)
        {
            m_LabelNumber = labelNumber;
        }

        public LocalizedSign(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write(m_LabelNumber);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_LabelNumber = reader.ReadInt();
                        break;
                    }
            }
        }
    }

    public class SubtextSign : Sign
    {
        private string m_Subtext;

        [CommandProperty(AccessLevel.GameMaster)]
        public string Subtext
        {
            get { return m_Subtext; }
            set { m_Subtext = value; InvalidateProperties(); }
        }

        [Constructable]
        public SubtextSign(SignType type, SignFacing facing, string subtext)
            : base(type, facing)
        {
            m_Subtext = subtext;
        }

        [Constructable]
        public SubtextSign(int itemID, string subtext)
            : base(itemID)
        {
            m_Subtext = subtext;
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (!String.IsNullOrEmpty(m_Subtext))
                LabelTo(from, m_Subtext);
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (!String.IsNullOrEmpty(m_Subtext))
                list.Add(m_Subtext);
        }

        public SubtextSign(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write(m_Subtext);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Subtext = reader.ReadString();
        }
    }
}