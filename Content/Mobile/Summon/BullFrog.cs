﻿using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Items;
using Server.Regions;

using BunnyHole = Server.Mobiles.VorpalBunny.BunnyHole;

namespace Server.Mobiles
{
    public class SummonedBullFrog : BaseTalismanSummon
    {
        [Constructable]
        public SummonedBullFrog()
            : base()
        {
            Name = "a bull frog";
            Body = 81;
            Hue = Utility.RandomList(0x5AC, 0x5A3, 0x59A, 0x591, 0x588, 0x57F);
            BaseSoundID = 0x266;
        }

        public SummonedBullFrog(Serial serial)
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