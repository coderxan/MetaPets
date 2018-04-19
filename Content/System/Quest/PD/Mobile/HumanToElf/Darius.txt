using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Definitions;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MLQuests.Mobiles
{
    public class Darius : DoneQuestCollector
    {
        private static readonly TextDefinition[] m_Offer =
		{
			1073998, // Blessings of Sosaria to you and merry met, friend.
			1073999, // I am glad for your company and wonder if you seek the heritage of your people?  I sense within you an elven bloodline -- the purity of which was lost when our brothers and sisters were exiled here in the Rupture.
			1074000, // If it is your desire to reclaim your place amongst the people, you must demonstrate that you understand and embrace the responsibilities expected of you as an elf.
			1074001, // The most basic lessons of our Sosaria are taught by her humblest children.  Seek Maul, the great bear, who understands instictively the seasons.
			1074398, // Seek Strongroot, the great treefellow, whose very roots reach to the heart of the world.  Seek Enigma, whose wisdom can only be conveyed in riddles and rhymes.  Seek Bravehorn, the great hart, who exemplifies the fierce dedication of a protector of his people.
			1074399, // Seek the Huntsman, the centuar tasked with maintaining the balance.  And lastly seek Arielle, the pixie, who has perhaps the most important lesson -- not to take yourself too seriously.
			1074400  // Or do none of these things.  You must choose your own path in the world, and what use you'll make of your existence.
		};

        private static readonly TextDefinition[] m_Incomplete =
		{
			1074002, // You have begun to walk the path of reclaiming your heritage, but you have not learned all the lessons before you.
			1074003  // You yet must perform these services:
		};

        private static readonly TextDefinition[] m_Complete =
		{
			1074004, // You have carved a path in history, sought to understand the way from our sage companions.
			1074005, // And now you have returned full circle to the place of your origin within the arms of Mother Sosaria. There is but one thing left to do if you truly wish to embrace your elven heritage.
			1074006, // To be born once more an elf, you must strip of all worldly possessions. Nothing of man or beast much touch your skin.
			1074007  // Then you may step forth into history.
		};

        private static readonly Type[] m_Needed =
		{
			typeof( Seasons ),
			typeof( CaretakerOfTheLand ),
			typeof( WisdomOfTheSphynx ),
			typeof( DefendingTheHerd ),
			typeof( TheBalanceOfNature ),
			typeof( TheJoysOfLife )
		};

        public override TextDefinition[] Offer { get { return m_Offer; } }
        public override TextDefinition[] Incomplete { get { return m_Incomplete; } }
        public override TextDefinition[] Complete { get { return m_Complete; } }
        public override Type[] Needed { get { return m_Needed; } }

        [Constructable]
        public Darius()
        {
            Name = "Darius";
            Title = "the wise";
            Race = Race.Elf;
            Hue = Race.RandomSkinHue();
            SpeechHue = Utility.RandomDyedHue();

            AddItem(new WildStaff());
            AddItem(new Sandals(0x1BB));
            AddItem(new GemmedCirclet());
            AddItem(new Tunic(Utility.RandomBrightHue()));

            Utility.AssignRandomHair(this);

            SetStr(40, 50);
            SetDex(60, 70);
            SetInt(90, 100); // Verified int
        }

        public override bool CanTalkTo(Mobile from)
        {
            return (from.Race == Race.Human);
        }

        public override void DenyTalk(Mobile from)
        {
            from.SendLocalizedMessage(1074017); // He's too busy right now, so he ignores you.
        }

        public override void OnComplete(PlayerMobile from)
        {
            from.SendGump(new RaceChangeConfirmGump(this, from, Race.Elf));
        }

        public Darius(Serial serial)
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