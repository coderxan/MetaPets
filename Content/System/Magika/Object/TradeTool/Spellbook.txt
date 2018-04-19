using System;

using Server.Network;
using Server.Spells;

namespace Server.Items
{
    /// <summary>
    /// This file contains all the spellbooks on the server, except one:
    /// the Magery spellbook is integrated with the BaseMagic.cs located in the 'Scripts\Engine\Content\System\Magic' directory.
    /// </summary>

    #region Magika Spellbooks

    public class MysticSpellbook : Spellbook
    {
        public override SpellbookType SpellbookType { get { return SpellbookType.Mystic; } }

        public override int BookOffset { get { return 677; } }
        public override int BookCount { get { return 16; } }

        [Constructable]
        public MysticSpellbook()
            : this((ulong)0)
        {
        }

        [Constructable]
        public MysticSpellbook(ulong content)
            : base(content, 0x2D9D)
        {
            Layer = Layer.OneHanded;
        }

        public MysticSpellbook(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class NecromancerSpellbook : Spellbook
    {
        public override SpellbookType SpellbookType { get { return SpellbookType.Necromancer; } }
        public override int BookOffset { get { return 100; } }
        public override int BookCount { get { return ((Core.SE) ? 17 : 16); } }

        [Constructable]
        public NecromancerSpellbook()
            : this((ulong)0)
        {
        }

        [Constructable]
        public NecromancerSpellbook(ulong content)
            : base(content, 0x2253)
        {
            Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
        }

        public NecromancerSpellbook(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Core.ML)
                Layer = Layer.OneHanded;
        }
    }

    public class SpellweavingBook : Spellbook
    {
        public override SpellbookType SpellbookType { get { return SpellbookType.Arcanist; } }
        public override int BookOffset { get { return 600; } }
        public override int BookCount { get { return 16; } }

        [Constructable]
        public SpellweavingBook()
            : this((ulong)0)
        {
        }

        [Constructable]
        public SpellweavingBook(ulong content)
            : base(content, 0x2D50)
        {
            Hue = 0x8A2;

            Layer = Layer.OneHanded;
        }

        public SpellweavingBook(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    #endregion

    #region Combat Spellbooks

    public class BookOfBushido : Spellbook
    {
        public override SpellbookType SpellbookType { get { return SpellbookType.Samurai; } }
        public override int BookOffset { get { return 400; } }
        public override int BookCount { get { return 6; } }

        [Constructable]
        public BookOfBushido()
            : this((ulong)0x3F)
        {
        }

        [Constructable]
        public BookOfBushido(ulong content)
            : base(content, 0x238C)
        {
            Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
        }

        public BookOfBushido(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Core.ML)
                Layer = Layer.OneHanded;
        }
    }

    public class BookOfChivalry : Spellbook
    {
        public override SpellbookType SpellbookType { get { return SpellbookType.Paladin; } }
        public override int BookOffset { get { return 200; } }
        public override int BookCount { get { return 10; } }

        [Constructable]
        public BookOfChivalry()
            : this((ulong)0x3FF)
        {
        }

        [Constructable]
        public BookOfChivalry(ulong content)
            : base(content, 0x2252)
        {
            Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
        }

        public BookOfChivalry(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Core.ML)
                Layer = Layer.OneHanded;
        }
    }

    public class BookOfNinjitsu : Spellbook
    {
        public override SpellbookType SpellbookType { get { return SpellbookType.Ninja; } }
        public override int BookOffset { get { return 500; } }
        public override int BookCount { get { return 8; } }


        [Constructable]
        public BookOfNinjitsu()
            : this((ulong)0xFF)
        {
        }

        [Constructable]
        public BookOfNinjitsu(ulong content)
            : base(content, 0x23A0)
        {
            Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
        }

        public BookOfNinjitsu(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Core.ML)
                Layer = Layer.OneHanded;
        }
    }

    #endregion
}