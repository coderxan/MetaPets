using System;

using Server;
using Server.Items;

namespace Server.Items
{
    /// <summary>
    /// This stone offers up a bag of Magery Reagents
    /// </summary>
    public class MageryReagentStone : Item
    {
        public override string DefaultName
        {
            get { return "a reagent stone"; }
        }

        [Constructable]
        public MageryReagentStone()
            : base(0xED4)
        {
            Movable = false;
            Hue = 0x2D1;
            Name = "a mage reagent stone";
        }

        public override void OnDoubleClick(Mobile from)
        {
            BagOfMageryReagents regBag = new BagOfMageryReagents(50);

            if (!from.AddToBackpack(regBag))
                regBag.Delete();
        }

        public MageryReagentStone(Serial serial)
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

    public class BagOfMageryReagents : Bag
    {
        [Constructable]
        public BagOfMageryReagents()
            : this(50)
        {
        }

        [Constructable]
        public BagOfMageryReagents(int amount)
        {
            DropItem(new BlackPearl(amount));
            DropItem(new Bloodmoss(amount));
            DropItem(new Garlic(amount));
            DropItem(new Ginseng(amount));
            DropItem(new MandrakeRoot(amount));
            DropItem(new Nightshade(amount));
            DropItem(new SulfurousAsh(amount));
            DropItem(new SpidersSilk(amount));
        }

        public BagOfMageryReagents(Serial serial)
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
    /// This stone offers up a bag of Necromancy Reagents
    /// </summary>
    public class NecromancyReagentStone : Item
    {
        public override string DefaultName
        {
            get { return "a reagent stone"; }
        }

        [Constructable]
        public NecromancyReagentStone()
            : base(0xED4)
        {
            Movable = false;
            Hue = 0x2D1;
            Name = "a necromancer reagent stone";
        }

        public override void OnDoubleClick(Mobile from)
        {
            BagOfNecromancyReagents necromancyReagents = new BagOfNecromancyReagents(50);

            if (!from.AddToBackpack(necromancyReagents))
                necromancyReagents.Delete();
        }

        public NecromancyReagentStone(Serial serial)
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

    public class BagOfNecromancyReagents : Bag
    {
        [Constructable]
        public BagOfNecromancyReagents()
            : this(50)
        {
        }

        [Constructable]
        public BagOfNecromancyReagents(int amount)
        {
            DropItem(new BatWing(amount));
            DropItem(new GraveDust(amount));
            DropItem(new DaemonBlood(amount));
            DropItem(new NoxCrystal(amount));
            DropItem(new PigIron(amount));
        }

        public BagOfNecromancyReagents(Serial serial)
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
    /// This stone offers up a bag of ALL the Reagents
    /// </summary>
    public class ReagentStone : Item
    {
        public override string DefaultName
        {
            get { return "a reagent stone"; }
        }

        [Constructable]
        public ReagentStone()
            : base(0xED4)
        {
            Movable = false;
            Hue = 0x2D1;
            Name = "a reagent stone";
        }

        public override void OnDoubleClick(Mobile from)
        {
            BagOfAllReagents allReagents = new BagOfAllReagents(50);

            if (!from.AddToBackpack(allReagents))
                allReagents.Delete();
        }

        public ReagentStone(Serial serial)
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

    public class BagOfAllReagents : Bag
    {
        [Constructable]
        public BagOfAllReagents()
            : this(50)
        {
        }

        [Constructable]
        public BagOfAllReagents(int amount)
        {
            DropItem(new BlackPearl(amount));
            DropItem(new Bloodmoss(amount));
            DropItem(new Garlic(amount));
            DropItem(new Ginseng(amount));
            DropItem(new MandrakeRoot(amount));
            DropItem(new Nightshade(amount));
            DropItem(new SulfurousAsh(amount));
            DropItem(new SpidersSilk(amount));
            DropItem(new BatWing(amount));
            DropItem(new GraveDust(amount));
            DropItem(new DaemonBlood(amount));
            DropItem(new NoxCrystal(amount));
            DropItem(new PigIron(amount));
        }

        public BagOfAllReagents(Serial serial)
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