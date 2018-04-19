using System;

using Server;

namespace Server.Items
{
    public abstract class BaseSuit : Item
    {
        private AccessLevel m_AccessLevel;

        [CommandProperty(AccessLevel.Administrator)]
        public AccessLevel AccessLevel { get { return m_AccessLevel; } set { m_AccessLevel = value; } }

        public BaseSuit(AccessLevel level, int hue, int itemID)
            : base(itemID)
        {
            Hue = hue;
            Weight = 1.0;
            Movable = false;
            LootType = LootType.Newbied;
            Layer = Layer.OuterTorso;

            m_AccessLevel = level;
        }

        public BaseSuit(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_AccessLevel);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_AccessLevel = (AccessLevel)reader.ReadInt();
                        break;
                    }
            }
        }

        public bool Validate()
        {
            object root = RootParent;

            if (root is Mobile && ((Mobile)root).AccessLevel < m_AccessLevel)
            {
                Delete();
                return false;
            }

            return true;
        }

        public override void OnSingleClick(Mobile from)
        {
            if (Validate())
                base.OnSingleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Validate())
                base.OnDoubleClick(from);
        }

        public override bool VerifyMove(Mobile from)
        {
            return (from.AccessLevel >= m_AccessLevel);
        }

        public override bool OnEquip(Mobile from)
        {
            if (from.AccessLevel < m_AccessLevel)
                from.SendMessage("You may not wear this.");

            return (from.AccessLevel >= m_AccessLevel);
        }
    }

    #region Server Royal

    public class LordBritishSuit : BaseSuit
    {
        [Constructable]
        public LordBritishSuit()
            : base(AccessLevel.GameMaster, 0x0, 0x2042)
        {
        }

        public LordBritishSuit(Serial serial)
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

    public class LordBlackthorneSuit : BaseSuit
    {
        [Constructable]
        public LordBlackthorneSuit()
            : base(AccessLevel.GameMaster, 0x0, 0x2043)
        {
        }

        public LordBlackthorneSuit(Serial serial)
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

    public class DupreSuit : BaseSuit
    {
        [Constructable]
        public DupreSuit()
            : base(AccessLevel.GameMaster, 0x0, 0x2050)
        {
        }

        public DupreSuit(Serial serial)
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

    #endregion

    #region Server Admin

    public class AdminRobe : BaseSuit
    {
        [Constructable]
        public AdminRobe()
            : base(AccessLevel.Administrator, 0x0, 0x204F) // Blank hue
        {
        }

        public AdminRobe(Serial serial)
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

    public class GMRobe : BaseSuit
    {
        [Constructable]
        public GMRobe()
            : base(AccessLevel.GameMaster, 0x26, 0x204F)
        {
        }

        public GMRobe(Serial serial)
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

    public class SeerRobe : BaseSuit
    {
        [Constructable]
        public SeerRobe()
            : base(AccessLevel.Seer, 0x1D3, 0x204F)
        {
        }

        public SeerRobe(Serial serial)
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

    public class CounselorRobe : BaseSuit
    {
        [Constructable]
        public CounselorRobe()
            : base(AccessLevel.Counselor, 0x3, 0x204F)
        {
        }

        public CounselorRobe(Serial serial)
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

    #endregion
}