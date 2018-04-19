﻿using System;
using System.Collections;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Mobiles
{
    public class GypsyAnimalTrainer : AnimalTrainer
    {
        [Constructable]
        public GypsyAnimalTrainer()
        {
            if (Utility.RandomBool())
                Title = "the gypsy animal trainer";
            else
                Title = "the gypsy animal herder";
        }

        public override VendorShoeType ShoeType
        {
            get { return Female ? VendorShoeType.ThighBoots : VendorShoeType.Boots; }
        }

        public override int GetShoeHue()
        {
            return 0;
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            Item item = FindItemOnLayer(Layer.Pants);

            if (item != null)
                item.Hue = Utility.RandomBrightHue();

            item = FindItemOnLayer(Layer.OuterLegs);

            if (item != null)
                item.Hue = Utility.RandomBrightHue();

            item = FindItemOnLayer(Layer.InnerLegs);

            if (item != null)
                item.Hue = Utility.RandomBrightHue();

            item = FindItemOnLayer(Layer.OuterTorso);

            if (item != null)
                item.Hue = Utility.RandomBrightHue();

            item = FindItemOnLayer(Layer.InnerTorso);

            if (item != null)
                item.Hue = Utility.RandomBrightHue();

            item = FindItemOnLayer(Layer.Shirt);

            if (item != null)
                item.Hue = Utility.RandomBrightHue();
        }

        public GypsyAnimalTrainer(Serial serial)
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