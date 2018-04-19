using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class Lissbet : BaseEscortable
    {
        public override bool StaticMLQuester { get { return true; } }
        public override bool InitialInnocent { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074204, // Greetings seeker.  I have an urgent matter for you, if you are willing.
                1074222  // Could I trouble you for some assistance?
            ));
        }

        [Constructable]
        public Lissbet()
        {
        }

        public override void InitBody()
        {
            SetStr(40, 50);
            SetDex(70, 80);
            SetInt(80, 90);

            Hue = Utility.RandomSkinHue();
            Female = true;
            Body = 401;
            Name = "Lissbet";
            Title = "the flower girl";

            HairItemID = 0x203D;
            HairHue = 0x1BB;
        }

        public override void InitOutfit()
        {
            AddItem(new Kilt(Utility.RandomYellowHue()));
            AddItem(new FancyShirt(Utility.RandomYellowHue()));
            AddItem(new Sandals());
        }

        public Lissbet(Serial serial)
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