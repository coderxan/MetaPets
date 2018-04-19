﻿using System;

using Server;
using Server.Items;

namespace Server.Items
{
    public class ScribeStone : Item
    {
        public override string DefaultName
        {
            get { return "a Scribe Supply Stone"; }
        }

        [Constructable]
        public ScribeStone()
            : base(0xED4)
        {
            Movable = false;
            Hue = 0x105;
        }

        public override void OnDoubleClick(Mobile from)
        {
            ScribeBag scribeBag = new ScribeBag();

            if (!from.AddToBackpack(scribeBag))
                scribeBag.Delete();
        }

        public ScribeStone(Serial serial)
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

    public class ScribeBag : Bag
    {
        public override string DefaultName
        {
            get { return "a Scribe Kit"; }
        }

        [Constructable]
        public ScribeBag()
            : this(1)
        {
            Movable = true;
            Hue = 0x105;
        }

        [Constructable]
        public ScribeBag(int amount)
        {
            DropItem(new BagOfMageryReagents(5000));
            DropItem(new BlankScroll(500));
        }

        public ScribeBag(Serial serial)
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