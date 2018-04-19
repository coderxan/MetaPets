using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Belulah (Nujel'm)")] // On OSI it's "Belulah (Nu'Jelm)" (incorrect spelling)
    public class Belulah : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            /*
             * 1074205 - Oh great adventurer, would you please assist a weak soul in need of aid?
             * 1074206 - Excuse me please traveler, might I have a little of your time?
             */
            MLQuestSystem.Tell(this, pm, Utility.Random(1074205, 2));
        }

        [Constructable]
        public Belulah()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Belulah";
            Title = "the scorned";
            Female = true;
            Body = 401;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new FancyShirt(Utility.RandomBlueHue()));
            AddItem(new LongPants(Utility.RandomNondyedHue()));
            AddItem(new Boots());
        }

        public Belulah(Serial serial)
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