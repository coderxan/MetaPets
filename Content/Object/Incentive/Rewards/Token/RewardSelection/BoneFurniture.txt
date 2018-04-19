using System;

namespace Server.Items
{
    /// <summary>
    /// Bone Couch
    /// </summary>
    public class BoneCouchComponent : AddonComponent
    {
        public override int LabelNumber { get { return 1074477; } } // Bone couch

        public BoneCouchComponent(int itemID)
            : base(itemID)
        {
        }

        public BoneCouchComponent(Serial serial)
            : base(serial)
        {
        }

        public override bool OnMoveOver(Mobile m)
        {
            bool allow = base.OnMoveOver(m);

            if (allow && m.Alive && m.Player && (m.AccessLevel == AccessLevel.Player || !m.Hidden))
                Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x547, 0x54A));

            return allow;
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

    [FlipableAddon(Direction.South, Direction.East)]
    public class BoneCouchAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new BoneCouchDeed(); } }

        [Constructable]
        public BoneCouchAddon()
            : base()
        {
            Direction = Direction.South;

            AddComponent(new BoneCouchComponent(0x2A5A), 0, 0, 0);
            AddComponent(new BoneCouchComponent(0x2A5B), -1, 0, 0);
        }

        public BoneCouchAddon(Serial serial)
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

        public virtual void Flip(Mobile from, Direction direction)
        {
            switch (direction)
            {
                case Direction.East:
                    AddComponent(new BoneCouchComponent(0x2A80), 0, 0, 0);
                    AddComponent(new BoneCouchComponent(0x2A7F), 0, 1, 0);
                    break;
                case Direction.South:
                    AddComponent(new BoneCouchComponent(0x2A5A), 0, 0, 0);
                    AddComponent(new BoneCouchComponent(0x2A5B), -1, 0, 0);
                    break;
            }
        }
    }

    public class BoneCouchDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new BoneCouchAddon(); } }
        public override int LabelNumber { get { return 1074477; } } // Bone couch

        [Constructable]
        public BoneCouchDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public BoneCouchDeed(Serial serial)
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

    /// <summary>
    /// Bone Table
    /// </summary>
    public class BoneTableAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new BoneTableDeed(); } }

        [Constructable]
        public BoneTableAddon()
            : base()
        {
            AddComponent(new LocalizedAddonComponent(0x2A5C, 1074478), 0, 0, 0);
        }

        public BoneTableAddon(Serial serial)
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

    public class BoneTableDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new BoneTableAddon(); } }
        public override int LabelNumber { get { return 1074478; } } // Bone table

        [Constructable]
        public BoneTableDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public BoneTableDeed(Serial serial)
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

    /// <summary>
    /// Bone Throne
    /// </summary>
    [Flipable(0x2A58, 0x2A59)]
    public class BoneThroneComponent : AddonComponent
    {
        public override int LabelNumber { get { return 1074476; } } // Bone throne

        public BoneThroneComponent()
            : base(0x2A58)
        {
        }

        public BoneThroneComponent(Serial serial)
            : base(serial)
        {
        }

        public override bool OnMoveOver(Mobile m)
        {
            bool allow = base.OnMoveOver(m);

            if (allow && m.Alive && m.Player && (m.AccessLevel == AccessLevel.Player || !m.Hidden))
                Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x54B, 0x54D));

            return allow;
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

    public class BoneThroneAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new BoneThroneDeed(); } }

        [Constructable]
        public BoneThroneAddon()
            : base()
        {
            AddComponent(new BoneThroneComponent(), 0, 0, 0);
        }

        public BoneThroneAddon(Serial serial)
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

    public class BoneThroneDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new BoneThroneAddon(); } }
        public override int LabelNumber { get { return 1074476; } } // Bone throne

        [Constructable]
        public BoneThroneDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public BoneThroneDeed(Serial serial)
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
}