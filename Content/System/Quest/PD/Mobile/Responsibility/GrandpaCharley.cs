using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class GrandpaCharley : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        [Constructable]
        public GrandpaCharley()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Grandpa Charley";
            Title = "the farmer";
            Body = 400;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            int hairHue = 0x3B2 + Utility.Random(2);
            Utility.AssignRandomHair(this, hairHue);

            FacialHairItemID = 0x203E; // Long Beard
            FacialHairHue = hairHue;

            SetSkill(SkillName.ItemID, 80, 90);

            AddItem(new WideBrimHat(Utility.RandomNondyedHue()));
            AddItem(new FancyShirt(Utility.RandomNondyedHue()));
            AddItem(new LongPants(Utility.RandomNondyedHue()));
            AddItem(new Sandals(Utility.RandomNeutralHue()));
            AddItem(new ShepherdsCrook());
            AddItem(new Backpack());
        }

        public GrandpaCharley(Serial serial)
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