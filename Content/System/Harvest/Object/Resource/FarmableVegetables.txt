using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class FarmableCabbage : FarmableCrop
    {
        public static int GetCropID()
        {
            return 3254;
        }

        public override Item GetCropObject()
        {
            Cabbage cabbage = new Cabbage();

            cabbage.ItemID = Utility.Random(3195, 2);

            return cabbage;
        }

        public override int GetPickedID()
        {
            return 3254;
        }

        [Constructable]
        public FarmableCabbage()
            : base(GetCropID())
        {
        }

        public FarmableCabbage(Serial serial)
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

    public class FarmableCarrot : FarmableCrop
    {
        public static int GetCropID()
        {
            return 3190;
        }

        public override Item GetCropObject()
        {
            Carrot carrot = new Carrot();

            carrot.ItemID = Utility.Random(3191, 2);

            return carrot;
        }

        public override int GetPickedID()
        {
            return 3254;
        }

        [Constructable]
        public FarmableCarrot()
            : base(GetCropID())
        {
        }

        public FarmableCarrot(Serial serial)
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

    public class FarmableLettuce : FarmableCrop
    {
        public static int GetCropID()
        {
            return 3254;
        }

        public override Item GetCropObject()
        {
            Lettuce lettuce = new Lettuce();

            lettuce.ItemID = Utility.Random(3184, 2);

            return lettuce;
        }

        public override int GetPickedID()
        {
            return 3254;
        }

        [Constructable]
        public FarmableLettuce()
            : base(GetCropID())
        {
        }

        public FarmableLettuce(Serial serial)
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

    public class FarmableOnion : FarmableCrop
    {
        public static int GetCropID()
        {
            return 3183;
        }

        public override Item GetCropObject()
        {
            Onion onion = new Onion();

            onion.ItemID = Utility.Random(3181, 2);

            return onion;
        }

        public override int GetPickedID()
        {
            return 3254;
        }

        [Constructable]
        public FarmableOnion()
            : base(GetCropID())
        {
        }

        public FarmableOnion(Serial serial)
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

    public class FarmablePumpkin : FarmableCrop
    {
        public static int GetCropID()
        {
            return Utility.Random(3166, 3);
        }

        public override Item GetCropObject()
        {
            Pumpkin pumpkin = new Pumpkin();

            pumpkin.ItemID = Utility.Random(3178, 3);

            return pumpkin;
        }

        public override int GetPickedID()
        {
            return Utility.Random(3166, 3);
        }

        [Constructable]
        public FarmablePumpkin()
            : base(GetCropID())
        {
        }

        public FarmablePumpkin(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class FarmableTurnip : FarmableCrop
    {
        public static int GetCropID()
        {
            return Utility.Random(3169, 3);
        }

        public override Item GetCropObject()
        {
            Turnip turnip = new Turnip();

            turnip.ItemID = Utility.Random(3385, 2);

            return turnip;
        }

        public override int GetPickedID()
        {
            return 3254;
        }

        [Constructable]
        public FarmableTurnip()
            : base(GetCropID())
        {
        }

        public FarmableTurnip(Serial serial)
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