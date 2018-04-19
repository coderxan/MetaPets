using System;

using Server;

namespace Server.Items
{
    public class DrakovsJournal : BlueBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Drakov's Journal", "Drakov",
                new BookPageInfo
                (
                    "My Master",
                    "",
                    "This journal was",
                    "found on one of",
                    "our controllers.  It",
                    "seems he has lost",
                    "faith in you.  Know",
                    "that he has been"
                ),
                new BookPageInfo
                (
                    "dealth with and will",
                    "never again speak",
                    "ill of you or our",
                    "cause.",
                    "          -Galzon"
                ),
                new BookPageInfo
                (
                    "We have completted",
                    "construction of the",
                    "devices needed to",
                    "build the clockwork",
                    "overseers and minions",
                    "as per the request of",
                    "the Master.  The",
                    "gargoyles have been"
                ),
                new BookPageInfo
                (
                    "most useful and their",
                    "knowledge of the",
                    "techniques for the",
                    "construction of these",
                    "creatures will serve",
                    "us well.",
                    "        -----",
                    "I am not one to"
                ),
                new BookPageInfo
                (
                    "criticize the Master,",
                    "but I believe he may",
                    "have erred in his",
                    "decision to destroy",
                    "the wingless ones.",
                    "Already our forces",
                    "are weakened by the",
                    "constant attacks of"
                ),
                new BookPageInfo
                (
                    "the humans  Their",
                    "strength and",
                    "unquestioning",
                    "compliance would",
                    "have made them very",
                    "useful in the fight",
                    "against the humans.",
                    "But the Master felt"
                ),
                new BookPageInfo
                (
                    "their presence to be",
                    "an annoyance and",
                    "a distraction to the",
                    "winged ones.  It was",
                    "not difficult at all",
                    "to remove them from",
                    "this world.  But now",
                    "I fear without more"
                ),
                new BookPageInfo
                (
                    "allies, willing or",
                    "not, we stand",
                    "little chance of",
                    "defeating the foul",
                    "humans from our",
                    "lands.  Perhaps if",
                    "the Master had",
                    "shown a little"
                ),
                new BookPageInfo
                (
                    "mercy and forsight",
                    "we would not be",
                    "in such dire peril."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public DrakovsJournal()
            : base(false)
        {
        }

        public DrakovsJournal(Serial serial)
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

    public class FropozJournal : RedBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal", "Fropoz",
                new BookPageInfo
                (
                    "I have done as my",
                    "Master has",
                    "instructed me.",
                    "",
                    "The painted humans",
                    "have been driven into",
                    "Britannia and are even",
                    "now wreaking havoc"
                ),
                new BookPageInfo
                (
                    "across the land,",
                    "providing us with the",
                    "distraction my Master",
                    "requested.  We",
                    "have provided them",
                    "with the masks",
                    "necessary to defeat",
                    "the orcs, thus"
                ),
                new BookPageInfo
                (
                    "causing even more",
                    "distress for the people",
                    "of Britannia.  The",
                    "unsuspecting fools",
                    "are too busy dealing",
                    "with the orc hordes to",
                    "continue their",
                    "exploration of our"
                ),
                new BookPageInfo
                (
                    "lands.  We are",
                    "safe...for now.",
                    "     ----",
                    "The attacks",
                    "continue exactly as",
                    "planned.  My Master",
                    "is pleased with my",
                    "work and we are"
                ),
                new BookPageInfo
                (
                    "closer to our goals than",
                    "ever before.  The",
                    "gargoyles have proven",
                    "to be more troublesome",
                    "than we first",
                    "anticipated, but I",
                    "believe we can",
                    "subjugate them fully"
                ),
                new BookPageInfo
                (
                    "given enough time.  It's",
                    "unfortunate that we",
                    "did not discover their",
                    "knowledge sooner.",
                    "Even now they",
                    "prepare our armies",
                    "for battle, but not",
                    "without resistance."
                ),
                new BookPageInfo
                (
                    "Now that some of",
                    "them know of the",
                    "other lands and of",
                    "humans, they will",
                    "double their efforts to",
                    "seek help.  This",
                    "cannot be allowed.",
                    "    -----"
                ),
                new BookPageInfo
                (
                    "Damn them!!  The",
                    "humans proved",
                    "more resourcefull than",
                    "we thought them",
                    "capable of.  Already",
                    "their homes are free",
                    "of orcs and savages",
                    "and they once again"
                ),
                new BookPageInfo
                (
                    "are treading in our",
                    "lands.  We may have to",
                    "move sooner than we",
                    "thought.  I will",
                    "prepar my brethern",
                    "and our golems.",
                    "Hopefully, we can",
                    "buy our Master some"
                ),
                new BookPageInfo
                (
                    "more time before the",
                    "humans discover us.",
                    "     -----",
                    "It's too late.  The",
                    "gargoyles whom have",
                    "evaded our capture",
                    "have opened the doors",
                    "to our land."
                ),
                new BookPageInfo
                (
                    "They pray the",
                    "humans will help",
                    "them, despite the",
                    "actions of their",
                    "cousins in Britannia.  I",
                    "fear they are right.",
                    "I must go to warn",
                    "the MastKai Hohiro,"
                ),
                new BookPageInfo
                (
                ),
                new BookPageInfo
                (
                    "10.11.2001",
                    "first one to be here",
                    "",
                    "Congrats. I didn't really",
                    "care to log on earlier,",
                    "nor did I come straight",
                    "here. 2pm, Magus"
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public FropozJournal()
            : base(false)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add("Fropoz's Journal");
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, "Fropoz's Journal");
        }

        public FropozJournal(Serial serial)
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

    public class TranslatedGargoyleJournal : BlueBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Translated Journal", "Velis",
                new BookPageInfo
                (
                    "This text has been",
                    "translated from a",
                    "gargoyle's journal",
                    "following his capture",
                    "and subsequent",
                    "reeducation.",
                    "",
                    "          -Velis"
                ),
                new BookPageInfo
                (
                    "I write this in the",
                    "hopes that someday a",
                    "soul of pure heart and",
                    "mind will read it.  We",
                    "are not the evil beings",
                    "that our cousin",
                    "gargoyles have made",
                    "us out to be.  We"
                ),
                new BookPageInfo
                (
                    "consider them",
                    "uncivilized and they",
                    "have no concept of the",
                    "Principles.  To you",
                    "who reads this, I beg",
                    "for your help in",
                    "saving my brethern",
                    "and preserving my"
                ),
                new BookPageInfo
                (
                    "race.  We stand at the",
                    "edge of destruction as",
                    "does the rest of the",
                    "world.  Once it was",
                    "written law that we",
                    "would not allow the",
                    "knowledge of our",
                    "civilization to spread"
                ),
                new BookPageInfo
                (
                    "into the world, no we",
                    "are left with little",
                    "choice...contact the",
                    "outside world in the hopes",
                    "of finding help to save",
                    "it or becoming the",
                    "unwilling bringers of",
                    "its damnation."
                ),
                new BookPageInfo
                (
                    "   I fear my capture is",
                    "certain, the",
                    "controllers grow ever",
                    "closer to my hiding",
                    "place and I know if",
                    "they discover me, my",
                    "fate will be as that of",
                    "my brothers."
                ),
                new BookPageInfo
                (
                    "Although we resisted",
                    "with all our strength",
                    "it is now clear that we",
                    "must have assistance",
                    "or our people will be",
                    "gone.  And if our",
                    "oppressor achieves",
                    "his goals our race will"
                ),
                new BookPageInfo
                (
                    "surely be joined buy",
                    "others.",
                    "   Those of us who",
                    "have not yet been",
                    "taken hope to open a",
                    "path from the outside",
                    "world into the city.",
                    "We believe we have"
                ),
                new BookPageInfo
                (
                    "found weak areas in",
                    "the mountains that we",
                    "can successfully",
                    "knock through with",
                    "our limited supplies.",
                    "We will have to work",
                    "quickly and the risk",
                    "of being discovered is"
                ),
                new BookPageInfo
                (
                    "great, but no choice",
                    "remains..."
                ),
                new BookPageInfo
                (
                ),
                new BookPageInfo
                (
                ),
                new BookPageInfo
                (
                    "Kai Hohiro, 12pm.",
                    "10.11.2001",
                    "first one to be here"
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TranslatedGargoyleJournal()
            : base(false)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add("Translated Gargoyle Journal");
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, "Translated Gargoyle Journal");
        }

        public TranslatedGargoyleJournal(Serial serial)
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

    public class KaburJournal : RedBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal", "Kabur",
                new BookPageInfo
                (
                    "The campaign to slaughter",
                    "the Meer goes well.",
                    "Although they seem to",
                    "oppose the forces of",
                    "ours at every turn, we",
                    "still defeat them in",
                    "combat.  Spies of the",
                    "Meer have been found and"
                ),
                new BookPageInfo
                (
                    "slain outside of the",
                    "fortress of ours.  The",
                    "fools underestimate us.",
                    "We have the power of",
                    "Lord Exodus behind us.",
                    "Soon they will learn to",
                    "serve the Juka and I",
                    "shall carry the head of"
                ),
                new BookPageInfo
                (
                    "the wench, Dasha, on a",
                    "spike for all the warriors",
                    "of ours to share triumph",
                    "under.",
                    "",
                    "One of the warriors of",
                    "the Juka died today.",
                    "During the training"
                ),
                new BookPageInfo
                (
                    "exercises of ours he",
                    "spoke out in favor of",
                    "the warriors of the",
                    "Meer, saying that they",
                    "were indeed powerful and",
                    "would provide a challenge",
                    "to the Juka.  A Juka in",
                    "fear is no Juka.  I gave"
                ),
                new BookPageInfo
                (
                    "him the death of a",
                    "coward, outside of battle.",
                    "",
                    "More spies of the Meer",
                    "have been found around",
                    "the fortress of ours.",
                    "Many have been seen and",
                    "escaped the wrath of the"
                ),
                new BookPageInfo
                (
                    "warriors of ours.  Those",
                    "who have been captured",
                    "and tortured have",
                    "revealed nothing to us,",
                    "even when subjected to",
                    "the spells of the females.",
                    " I know the Meer must",
                    "have plans against us if"
                ),
                new BookPageInfo
                (
                    "they send so many spies.",
                    " I may send the troops",
                    "of the Juka to invade",
                    "the camps of theirs as a",
                    "warning.",
                    "",
                    "I have met Dasha in",
                    "battle this day.  The"
                ),
                new BookPageInfo
                (
                    "efforts of hers to draw",
                    "me into a Black Duel",
                    "were foolish.   Had we",
                    "not been interrupted in",
                    "the cave I would have",
                    "ended the life of hers",
                    "but I will have to wait",
                    "for another battle.  Lord"
                ),
                new BookPageInfo
                (
                    "Exodus has ordered more",
                    "patrols around the",
                    "fortress of ours.  If",
                    "Dasha is any indication,",
                    "the Meer will strike soon.",
                    "",
                    "More Meer stand outside",
                    "of the fortress of ours"
                ),
                new BookPageInfo
                (
                    "than I have ever seen at",
                    "once.  They must seek",
                    "vengeance for the",
                    "destruction of their",
                    "forest.  Many Juka stand",
                    "ready at the base of the",
                    "mountain to face the",
                    "forces of theirs but"
                ),
                new BookPageInfo
                (
                    "today may be the final",
                    "battle.  Exodus has",
                    "summoned me, I must",
                    "prepare.",
                    "",
                    "Dusk has passed and the",
                    "Juka now live in a new",
                    "age, a later time.  I have"
                ),
                new BookPageInfo
                (
                    "just returned from",
                    "exploring the new world",
                    "that surrounds the",
                    "fortress of the Juka.",
                    "During the attack of the",
                    "Meer the madman",
                    "Adranath tried to destroy",
                    "the fortress of ours"
                ),
                new BookPageInfo
                (
                    "with great magic.  At",
                    "once he was still and",
                    "light surrounded the",
                    "fortress.  Everything",
                    "faded from view.  When I",
                    "regained the senses of",
                    "mine I saw no sign of",
                    "the Meer but Dasha."
                ),
                new BookPageInfo
                (
                    "She has not been found",
                    "since this new being,",
                    "Blackthorn, blasted her",
                    "from the top of the",
                    "fortress.",
                    "The forest was gone, now",
                    "replaced by grasslands.",
                    "In the far distance I"
                ),
                new BookPageInfo
                (
                    "could see humans that",
                    "had covered the bodies of",
                    "theirs in marks.  Even",
                    "Gargoyles populate this",
                    "place.  Exodus has",
                    "explained to me that the",
                    "Juka and the fortress of",
                    "ours have been pulled"
                ),
                new BookPageInfo
                (
                    "forward in time.  The",
                    "world we knew is now",
                    "thousands of years in the",
                    "past.  Lord Exodus say",
                    "he has has saved the",
                    "Juka from extinction.  I",
                    "do not want to believe",
                    "him.  I asked this"
                ),
                new BookPageInfo
                (
                    "stranger about the Meer,",
                    "but he tells me a new",
                    "enemy remains to be",
                    "destroyed.  It seems the",
                    "enemies of ours have",
                    "passed away to dust like",
                    "the forest."
                ),
                new BookPageInfo
                (
                    "I have spoken with other",
                    "Juka and I suspect I have",
                    "been told the truth.  All",
                    "the Juka had powerful",
                    "dreams.  In the dreams",
                    "of ours the Meer invaded",
                    "the fortress of ours and",
                    "a great battle took place."
                ),
                new BookPageInfo
                (
                    " All the Juka and all the",
                    "Meer perished and the",
                    "fortress was destroyed",
                    "from Adranath's spells.  I",
                    "would not like to believe",
                    "that the Meer could ever",
                    "destroy us, but now it",
                    "seems we have seen a"
                ),
                new BookPageInfo
                (
                    "vision of the fate of",
                    "ours now lost in time.  I",
                    "must now wonder if the",
                    "Meer did not die in the",
                    "battle with the Juka, how",
                    "did they die?  And more",
                    "importantly, where is",
                    "Dasha?"
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public KaburJournal()
            : base(false)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add("Khabur's Journal");
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, "Khabur's Journal");
        }

        public KaburJournal(Serial serial)
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
}