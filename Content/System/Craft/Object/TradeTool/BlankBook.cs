using System;

using Server;

namespace Server.Items
{
    public class BlueBook : BaseBook
    {
        [Constructable]
        public BlueBook()
            : base(0xFF2, 40, true)
        {
        }

        [Constructable]
        public BlueBook(int pageCount, bool writable)
            : base(0xFF2, pageCount, writable)
        {
        }

        [Constructable]
        public BlueBook(string title, string author, int pageCount, bool writable)
            : base(0xFF2, title, author, pageCount, writable)
        {
        }

        // Intended for defined books only
        public BlueBook(bool writable)
            : base(0xFF2, writable)
        {
        }

        public BlueBook(Serial serial)
            : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
    }

    public class RedBook : BaseBook
    {
        [Constructable]
        public RedBook()
            : base(0xFF1)
        {
        }

        [Constructable]
        public RedBook(int pageCount, bool writable)
            : base(0xFF1, pageCount, writable)
        {
        }

        [Constructable]
        public RedBook(string title, string author, int pageCount, bool writable)
            : base(0xFF1, title, author, pageCount, writable)
        {
        }

        // Intended for defined books only
        public RedBook(bool writable)
            : base(0xFF1, writable)
        {
        }

        public RedBook(Serial serial)
            : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
    }

    public class TanBook : BaseBook
    {
        [Constructable]
        public TanBook()
            : base(0xFF0)
        {
        }

        [Constructable]
        public TanBook(int pageCount, bool writable)
            : base(0xFF0, pageCount, writable)
        {
        }

        [Constructable]
        public TanBook(string title, string author, int pageCount, bool writable)
            : base(0xFF0, title, author, pageCount, writable)
        {
        }

        // Intended for defined books only
        public TanBook(bool writable)
            : base(0xFF0, writable)
        {
        }

        public TanBook(Serial serial)
            : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
    }

    public class BrownBook : BaseBook
    {
        [Constructable]
        public BrownBook()
            : base(0xFEF)
        {
        }

        [Constructable]
        public BrownBook(int pageCount, bool writable)
            : base(0xFEF, pageCount, writable)
        {
        }

        [Constructable]
        public BrownBook(string title, string author, int pageCount, bool writable)
            : base(0xFEF, title, author, pageCount, writable)
        {
        }

        // Intended for defined books only
        public BrownBook(bool writable)
            : base(0xFEF, writable)
        {
        }

        public BrownBook(Serial serial)
            : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
    }
}