using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Items;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Gorrow (Luna)")]
    public class Gorrow : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074200, // Thank goodness you are here, there’s no time to lose.
                1074203 // Hello friend. I realize you are busy but if you would be willing to render me a service I can assure you that you will be judiciously renumerated.
            ));
        }

        [Constructable]
        public Gorrow()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Gorrow";
            Title = "the Mayor";
            Race = Race.Human;
            BodyValue = 0x190;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            AddItem(new Backpack());
            AddItem(new Shoes(0x1BB));
            AddItem(new Tunic(Utility.RandomNeutralHue()));
            AddItem(new LongPants(0x901));
            AddItem(new Cloak(Utility.RandomRedHue()));
        }

        public Gorrow(Serial serial)
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