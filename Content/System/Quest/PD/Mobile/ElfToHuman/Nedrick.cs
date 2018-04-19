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
    public class Nedrick : DoneQuestCollector
    {
        private static readonly TextDefinition[] m_Offer =
		{
			1074403, // Greetings, traveler and welcome.
			1074404, // Perhaps you have heard of the service I offer?  Perhaps you wish to avail yourself of the opportunity I lay before you.
			1074405, // Elves and humans; we lived together once in peace.  Mighty relics that attest to our friendship remain, of course.  Yet, memories faded when the Gem was shattered and the world torn asunder.  Alone in The Heartwood, our elven brothers and sisters wondered what terrible evil had befallen Sosaria.
			1074406, // Violent change marked the sundering of our ties.  We are different -- elves and humans.  And yet we are much alike.  I can give an elf the chance to walk as a human upon Sosaria.  I can undertake the transformation.
			1074407, // But you must prove yourself to me.  Humans possess a strength of character and back.  Humans are quick-witted and able to pick up a smattering of nearly any talent.  Humans are tough both mentally and physically.  And of course, humans defend their own -- sometimes with their own lives.
			1074408, // Seek Sledge the Versatile and learn about human ingenuity and creativity.  Seek Patricus and demonstrate your integrity and strength.
			1074409, // Seek out a human in need and prove your worth as a defender of humanity.  Seek Belulah in Nu'Jelm and heartily challenge the elements in a display of toughness to rival any human.
			1074411  // Or turn away and embrace your heritage.  It matters not to me.
		};

        private static readonly TextDefinition[] m_Incomplete =
		{
			1074412, // You have made a good start but have more yet to do.
			1074413  // You must yet perform these deeds:
		};

        private static readonly TextDefinition[] m_Complete =
		{
			1074410, // You have proven yourself capable and commited and so I will grant you the transformation you seek.
			1074531, // The first time you were born, you entered the world bare of all possessions and concerns.  So too as you transform to your new life as a human, you must remove all worldly goods from the touch of your flesh.
			1074532  // I call upon all nearby to witness your rebirth!
		};

        private static readonly Type[] m_Needed =
		{
			typeof( Ingenuity ),
			typeof( HeaveHo ),
			typeof( HumanInNeed ),
			typeof( AllSeasonAdventurer )
		};

        public override TextDefinition[] Offer { get { return m_Offer; } }
        public override TextDefinition[] Incomplete { get { return m_Incomplete; } }
        public override TextDefinition[] Complete { get { return m_Complete; } }
        public override Type[] Needed { get { return m_Needed; } }

        [Constructable]
        public Nedrick()
        {
            Name = "Nedrick";
            Title = "the iron worker";
            Race = Race.Human;
            Hue = Race.RandomSkinHue();
            SpeechHue = Utility.RandomDyedHue();

            AddItem(new Boots());
            AddItem(new LongPants(Utility.RandomNondyedHue()));
            AddItem(new FancyShirt(Utility.RandomNondyedHue()));

            Utility.AssignRandomHair(this);
            Utility.AssignRandomFacialHair(this, HairHue);

            SetStr(70, 80);
            SetDex(50, 60);
            SetInt(60, 70); // Verified int
        }

        public override bool CanTalkTo(Mobile from)
        {
            return (from.Race == Race.Elf);
        }

        public override void DenyTalk(Mobile from)
        {
            from.SendLocalizedMessage(1074017); // He's too busy right now, so he ignores you.
        }

        public override void OnComplete(PlayerMobile from)
        {
            from.SendGump(new RaceChangeConfirmGump(this, from, Race.Human));
        }

        public Nedrick(Serial serial)
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