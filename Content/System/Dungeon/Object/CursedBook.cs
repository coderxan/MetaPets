using System;

using Server;

namespace Server.Items
{
    /// <summary>
    /// Grimmoch Journal
    /// </summary>
    public class GrimmochJournal1 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day One :",
                    "",
                    "'Tis a grand sight, this",
                    "primeval tomb, I agree",
                    "with Tavara on that.",
                    "And we've a good crew",
                    "here, they've strong",
                    "backs and a good"
                ),
                new BookPageInfo
                (
                    "attitude.  I'm a bit",
                    "concerned by those",
                    "that worked as guides",
                    "for us, however.  All",
                    "seemed well enough",
                    "until we revealed the",
                    "immense stone doors",
                    "of the tomb structure"
                ),
                new BookPageInfo
                (
                    "itself.  Seemed to send",
                    "a shiver up their",
                    "spines and get them all",
                    "stirred up with",
                    "whispering.  I'll",
                    "watch the lot of them",
                    "with a close eye, but",
                    "I'm confident we won't"
                ),
                new BookPageInfo
                (
                    "have any real",
                    "problems on the dig.",
                    "I'm especially proud to",
                    "see Thomas standing",
                    "out - he was a good",
                    "hire, despite the",
                    "warnings from his",
                    "previous employers."
                ),
                new BookPageInfo
                (
                    "He's drummed up the",
                    "workers into a",
                    "furious pace - we've",
                    "nearly halved the",
                    "estimate on the",
                    "timeline for",
                    "excavating the Tomb's",
                    "entrance."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal1()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal1(Serial serial)
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

    public class GrimmochJournal2 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Two :",
                    "",
                    "We managed to dig out",
                    "the last of the",
                    "remaining rubble",
                    "today, revealing the",
                    "entirety of the giant",
                    "stone doors that sealed"
                ),
                new BookPageInfo
                (
                    "ol' Khal Ankur and",
                    "his folk up ages ago.",
                    "Actually getting them",
                    "open was another",
                    "matter altogether,",
                    "however.  As the",
                    "workers set to the",
                    "task with picks and"
                ),
                new BookPageInfo
                (
                    "crowbars, I could have",
                    "sworn I saw Lysander",
                    "Gathenwale fiddling",
                    "with something in that",
                    "musty old tome of his.",
                    " I've no great",
                    "knowledge of things",
                    "magical, but the way"
                ),
                new BookPageInfo
                (
                    "his hand moved over",
                    "that book, and the look",
                    "of concentration on his",
                    "face as he whispered",
                    "something to himself",
                    "looked like every",
                    "description of an",
                    "incantation I've ever"
                ),
                new BookPageInfo
                (
                    "heard.  The strange",
                    "thing is, this set of",
                    "doors that an entire",
                    "crew of excavators",
                    "was laboring over for",
                    "hours, right when",
                    "Gathenwale finishes",
                    "with his mumbling..."
                ),
                new BookPageInfo
                (
                    "well, I swore the doors",
                    "just gave open at the",
                    "exact moment he",
                    "spoke his last bit of",
                    "whisper and shut the",
                    "tome tight in his",
                    "hands.  When he",
                    "looked up, it was"
                ),
                new BookPageInfo
                (
                    "almost as if he was",
                    "expecting the doors to",
                    "be open, rather than",
                    "shocked that they'd",
                    "finally given way."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal2()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal2(Serial serial)
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

    public class GrimmochJournal3 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Three - Day Five:",
                    "",
                    "I might have",
                    "written too hastily in",
                    "my first entry - this",
                    "place doesn't seem too",
                    "bent on giving up any",
                    "secrets.   Though the"
                ),
                new BookPageInfo
                (
                    "main antechamber is",
                    "open to us, the main",
                    "exit hall is blocked by",
                    "yet another pile of",
                    "rubble.  Doesn't look a",
                    "bit like anything",
                    "caused by a quake or",
                    "instability in the"
                ),
                new BookPageInfo
                (
                    "stonework... I swear it",
                    "looks as if someone",
                    "actually piled the",
                    "stones up themselves,",
                    "some time after the",
                    "tomb was built.  The",
                    "stones aren't of the",
                    "same set nor quality"
                ),
                new BookPageInfo
                (
                    "of the carved work",
                    "that surrounds them",
                    "- if anything, they",
                    "resemble the grade of",
                    "common rock we saw",
                    "in great quantities on",
                    "the trip here.  Which",
                    "makes it feel all the"
                ),
                new BookPageInfo
                (
                    "more like someone",
                    "hauled them in and",
                    "deliberately covered",
                    "this passage.  But then",
                    "why not decorate them",
                    "in the same ornate",
                    "manner as the rest of",
                    "the stone in this"
                ),
                new BookPageInfo
                (
                    "place?  Lysander",
                    "wouldn't hear a word",
                    "of what I had to say -",
                    "to him, it was a quake",
                    "some time in the",
                    "history of the tomb,",
                    "and that was it, shut",
                    "up and move on.  So I"
                ),
                new BookPageInfo
                (
                    "shut up, and got back",
                    "to work."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal3()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal3(Serial serial)
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

    public class GrimmochJournal6 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Six :",
                    "",
                    "The camp was",
                    "attacked last night by",
                    "a pack of, well, I don't",
                    "have a clue.  I've never",
                    "seen the like of these",
                    "beasts anywhere."
                ),
                new BookPageInfo
                (
                    "Huge things, with",
                    "fangs the size of your",
                    "forefinger, covered in",
                    "hair and with the",
                    "strangest arched back",
                    "I've ever seen.  And so",
                    "many of them.  We",
                    "were forced back into"
                ),
                new BookPageInfo
                (
                    "the Tomb for the",
                    "night, just to keep our",
                    "hides on us.  And",
                    "today Gathenwale",
                    "practically orders us",
                    "all to move the entire",
                    "exterior camp into the",
                    "Tomb.  Now, I don't"
                ),
                new BookPageInfo
                (
                    "disagree that we'd be",
                    "well off to use the",
                    "place as a point of",
                    "fortification... but I",
                    "don't like it one bit, in",
                    "any case.  I don't like",
                    "the look of this place,",
                    "nor the sound of it."
                ),
                new BookPageInfo
                (
                    "The way the wind",
                    "gets into the",
                    "passageways,",
                    "whistling up the",
                    "strangest noises.",
                    "Deep, sustained echoes",
                    "of the wind, not so",
                    "much flute-like as..."
                ),
                new BookPageInfo
                (
                    "well, it sounds",
                    "ridiculous.  In any",
                    "case, we've set to work",
                    "moving the bulk of the",
                    "exterior camp into the",
                    "main antechamber, so",
                    "there's no use moaning",
                    "about it now."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal6()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal6(Serial serial)
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

    public class GrimmochJournal7 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Seven - Day Ten:",
                    "",
                    "I cannot stand this",
                    "place, I cannot bear it.",
                    "I've got to get out.",
                    "Something evil lurks",
                    "in this ancient place,",
                    "something best left"
                ),
                new BookPageInfo
                (
                    "alone.  I hear them,",
                    "yet none of the others",
                    "do.  And yet they",
                    "must.  Hands, claws,",
                    "scratching at stone,",
                    "the awful scratching",
                    "and the piteous cries",
                    "that sound almost like"
                ),
                new BookPageInfo
                (
                    "laughter.  I can hear",
                    "them above even the",
                    "cracks of the",
                    "workmen's picks, and",
                    "at night they are all I",
                    "can hear.  And yet the",
                    "others hear nothing.",
                    "We must leave this"
                ),
                new BookPageInfo
                (
                    "place, we must.",
                    "Three workers have",
                    "gone missing - Tavara",
                    "expects they've",
                    "abandoned us - and I",
                    "count them lucky if",
                    "they have.  I don't care",
                    "what the others say,"
                ),
                new BookPageInfo
                (
                    "we must leave this",
                    "place.  We must do as",
                    "those before and pile",
                    "up the stones, block all",
                    "access to this primeval",
                    "crypt, seal it up again",
                    "for all eternity."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal7()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal7(Serial serial)
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

    public class GrimmochJournal11 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Eleven - Day",
                    "Thirteen :",
                    "",
                    "Lysander is gone, and",
                    "two more workers",
                    "with him.  Good",
                    "riddance to the first.",
                    "He knows something."
                ),
                new BookPageInfo
                (
                    "He heard them too, I",
                    "know he did - and yet",
                    "he scowled at me",
                    "when I mentioned",
                    "them.  I cannot stop",
                    "the noise in my head,",
                    "the scratching, the",
                    "clawing tears at my"
                ),
                new BookPageInfo
                (
                    "senses.  What is it?",
                    "What does Lysander",
                    "seek that I can only",
                    "turn from?  Where",
                    "has he gone?  The",
                    "only answer to my",
                    "questions comes as",
                    "laughter from behind"
                ),
                new BookPageInfo
                (
                    "the stones."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal11()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal11(Serial serial)
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

    public class GrimmochJournal14 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Fourteen - Day",
                    "Sixteen :",
                    "",
                    "We are lost... we are",
                    "lost... all is lost.  The",
                    "dead are piled up at",
                    "my feet.  Bergen and I",
                    "somehow managed in"
                ),
                new BookPageInfo
                (
                    "the madness to piece",
                    "together a barricade,",
                    "barring access to the",
                    "camp antechamber.",
                    "He knows as well as I",
                    "that we cannot hold it",
                    "forever.  The dead",
                    "come.  They took"
                ),
                new BookPageInfo
                (
                    "Lysander before our",
                    "eyes.  I pity the soul",
                    "of even such a",
                    "madman - no one",
                    "should die in such a",
                    "manner.  And yet so",
                    "many have.  We're",
                    "trapped here in this"
                ),
                new BookPageInfo
                (
                    "horror.  So many have",
                    "died, and for what?",
                    "What curse have we",
                    "stumbled upon?  I",
                    "cannot bear it, the",
                    "moaning, wailing cries",
                    "of the dead.  Poor",
                    "Thomas, cut to pieces"
                ),
                new BookPageInfo
                (
                    "by their blades.  We",
                    "had only an hour to",
                    "properly bury those",
                    "we could, before the",
                    "undead legions struck",
                    "again.  I cannot go on...",
                    "I cannot go on."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal14()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal14(Serial serial)
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

    public class GrimmochJournal17 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Seventeen - Day",
                    "Twenty-Two :",
                    "",
                    "The fighting never",
                    "ceases... the blood",
                    "never stops flowing,",
                    "like a river through",
                    "the bloated corpses of"
                ),
                new BookPageInfo
                (
                    "the dead.  And yet",
                    "there are still more.",
                    "Always more, with",
                    "the red fire gleaming",
                    "in their eyes.  My",
                    "arm aches, I've taken",
                    "to the sword as my",
                    "bow seems to do little"
                ),
                new BookPageInfo
                (
                    "good... the dull ache in",
                    "my arm... so many",
                    "swings, cleaving a",
                    "mountain of decaying",
                    "flesh.  And Thomas...",
                    "he was there, in the",
                    "thick of it... Thomas",
                    "was beside me..."
                ),
                new BookPageInfo
                (
                    "his face cleaved in",
                    "twain - and yet beside",
                    "me, fighting with us",
                    "against the horde until",
                    "he was cut down once",
                    "again.  And I swear I",
                    "see him even now,",
                    "there in the dark"
                ),
                new BookPageInfo
                (
                    "corner of the",
                    "antechamber, his eyes",
                    "flickering in the last",
                    "dying embers of the",
                    "fire... and he stares at",
                    "me, and a scream fills",
                    "the vault - whether",
                    "his or mine, I can no"
                ),
                new BookPageInfo
                (
                    "longer tell."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal17()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal17(Serial serial)
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

    public class GrimmochJournal23 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "The daily journal of Grimmoch Drummel", "Grimmoch",
                new BookPageInfo
                (
                    "Day Twenty-Three :",
                    "",
                    "We no longer bury the",
                    "dead."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public GrimmochJournal23()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public GrimmochJournal23(Serial serial)
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

    /// <summary>
    /// Lysander Notebook
    /// </summary>
    public class LysanderNotebook1 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Lysander's Notebook", "L. Gathenwale",
                new BookPageInfo
                (
                    "Day One :",
                    "",
                    "At last, it stands",
                    "before me.  The doors",
                    "of Thy Sanctum will",
                    "open to me now, after",
                    "all these years of",
                    "searching.  I give"
                ),
                new BookPageInfo
                (
                    "myself unto Thee,",
                    "Khal Ankur, I have",
                    "come for Thy secrets",
                    "and I will kneel",
                    "prostrate before Thee.",
                    " Blessed are the",
                    "Keepers, praise unto",
                    "Thee, a thousand"
                ),
                new BookPageInfo
                (
                    "fortunes in the night."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public LysanderNotebook1()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public LysanderNotebook1(Serial serial)
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

    public class LysanderNotebook2 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Lysander's Notebook", "L. Gathenwale",
                new BookPageInfo
                (
                    "Day Two:",
                    "",
                    "The woman, Tavara",
                    "Sewel, is unbearable.",
                    "Her entire demeanor",
                    "sickens me.  I would",
                    "take her life for Thee",
                    "now, my Lord.  But I"
                ),
                new BookPageInfo
                (
                    "cannot alert the",
                    "others.  Progress is",
                    "made too slowly, I",
                    "cannot stand this",
                    "perpetual waiting.",
                    "Today I knelt down",
                    "with the workers,",
                    "tossing stones and dirt"
                ),
                new BookPageInfo
                (
                    "aside with my very",
                    "hands as they dug at",
                    "the last of the rubble",
                    "covering the entrance",
                    "to Thy Sanctum.  The",
                    "Sewel woman was",
                    "shocked at my",
                    "demeanor, dirtying"
                ),
                new BookPageInfo
                (
                    "my robes, on my",
                    "knees in the muck as I",
                    "clawed at the rocks.",
                    "She thought I did this",
                    "for those sickly",
                    "scholars, or for her,",
                    "or for what she",
                    "laughably calls 'The"
                ),
                new BookPageInfo
                (
                    "Gift of Discovery', of",
                    "learning.  As if I did",
                    "not know what I went",
                    "to find!  I come for",
                    "Thee, Master.  Soon",
                    "shall I receive Thy",
                    "gifts, Thy blessings.",
                    "Patience, eternal"
                ),
                new BookPageInfo
                (
                    "patience.  I must take",
                    "my lessons well.  I",
                    "have learned from",
                    "Thee, Master, I have."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public LysanderNotebook2()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public LysanderNotebook2(Serial serial)
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

    public class LysanderNotebook3 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Lysander's Notebook", "L. Gathenwale",
                new BookPageInfo
                (
                    "Day Three - Day Six:",
                    "",
                    "What are these Beasts",
                    "that dare to defy our",
                    "presence here?  Hast",
                    "Thou sent them,",
                    "Master?  To tear",
                    "apart these foolish"
                ),
                new BookPageInfo
                (
                    "ones that accompany",
                    "me?  That repugnant",
                    "pustule, Drummel, put",
                    "forth his absurd little",
                    "theories as to the",
                    "nature of the Beasts",
                    "that attacked our",
                    "camp, but I'll have"
                ),
                new BookPageInfo
                (
                    "none of his words.  He",
                    "asks too many",
                    "questions.  He is a",
                    "taint upon the grounds",
                    "of Thy Sanctum,",
                    "Master - I will deal",
                    "with him after the",
                    "Sewel woman."
                ),
                new BookPageInfo
                (
                    "Speaking of Sewel, I",
                    "have convinced that",
                    "empty-headed harlot",
                    "that we should move",
                    "our encampment",
                    "within the",
                    "antechamber.  She",
                    "thinks I worry for"
                ),
                new BookPageInfo
                (
                    "her safety.  I come",
                    "for thee, Master.  I",
                    "make my camp in Thy",
                    "chambers.  I sleep",
                    "under Thy roof.  I can",
                    "feel Thine presence",
                    "even now.  Soon,",
                    "Master.  Soon."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public LysanderNotebook3()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public LysanderNotebook3(Serial serial)
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

    public class LysanderNotebook7 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Lysander's Notebook", "L. Gathenwale",
                new BookPageInfo
                (
                    "Day Seven :",
                    "",
                    "The Sewel woman",
                    "pratters on endlessly.",
                    "And she dares to",
                    "speak Thy Name,",
                    "Master!  I wish so",
                    "vehemently to take a"
                ),
                new BookPageInfo
                (
                    "knife to that little",
                    "neck of hers.  She",
                    "struts around the",
                    "chambers of Thy",
                    "Sanctum with her",
                    "repugnant airs, her",
                    "scholarly conjecture",
                    "on this or that.  That I"
                ),
                new BookPageInfo
                (
                    "could peel the skin",
                    "from her face and",
                    "show her how vile and",
                    "ugly she truly is, how",
                    "unworthy of entrance",
                    "to Thy Sanctum.  I",
                    "must take her,",
                    "Master.  I must rend"
                ),
                new BookPageInfo
                (
                    "that little wench to",
                    "pieces.  I ask this gift",
                    "of Thee, that I might",
                    "cleanse Thy Sanctum",
                    "of her presence.  Give",
                    "me the Sewel woman",
                    "and I shall show you",
                    "my mastery of Death,"
                ),
                new BookPageInfo
                (
                    "Master.  I shall cut",
                    "her to bits and scatter",
                    "them before the",
                    "others as a warning.",
                    "I cannot stand her",
                    "presence, I cannot",
                    "abide it.  And",
                    "Drummel!  He is a"
                ),
                new BookPageInfo
                (
                    "pustule that must be",
                    "lanced, a sickness that",
                    "I must cure by blade",
                    "and fire.  Not a trace",
                    "of him will be left",
                    "when I'm done with",
                    "him.  Praises to Thee,",
                    "Master.  I shall honor"
                ),
                new BookPageInfo
                (
                    "Thee with many",
                    "sacrifices, soon",
                    "enough."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public LysanderNotebook7()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public LysanderNotebook7(Serial serial)
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

    public class LysanderNotebook8 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Lysander's Notebook", "L. Gathenwale",
                new BookPageInfo
                (
                    "Day Eight - Day Ten :",
                    "",
                    "Have you taken them,",
                    "Master?  They could",
                    "not have found a way",
                    "past the stones that",
                    "block our path!  The",
                    "three workers, My"
                ),
                new BookPageInfo
                (
                    "Master, where have",
                    "they gone?  Curses",
                    "upon them!  I'll cut",
                    "them all to pieces if",
                    "they show their faces",
                    "again, then burn the",
                    "rest alive upon a pyre,",
                    "for all to see, as a"
                ),
                new BookPageInfo
                (
                    "warning of Thy",
                    "Power.  How could",
                    "they have gotten past",
                    "me?  I sleep against",
                    "the very walls, to",
                    "hear Thy Words, to",
                    "feel Thy Breath.  I",
                    "can find no egress"
                ),
                new BookPageInfo
                (
                    "from the chambers",
                    "that the Sewel woman",
                    "does not know of nor",
                    "have men working at",
                    "excavating.  Where",
                    "have they gone,",
                    "Master?  Have you",
                    "taken them, or do they"
                ),
                new BookPageInfo
                (
                    "truly flee from Thy",
                    "Presence?  I will kill",
                    "them if they show",
                    "their faces again.",
                    "Give me Strength, my",
                    "Master, to let them",
                    "live a while longer,",
                    "until they have"
                ),
                new BookPageInfo
                (
                    "fulfilled their",
                    "purpose and I kneel",
                    "before Thee, covered",
                    "in their blood."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public LysanderNotebook8()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public LysanderNotebook8(Serial serial)
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

    public class LysanderNotebook11 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Lysander's Notebook", "L. Gathenwale",
                new BookPageInfo
                (
                    "Day Eleven - Day",
                    "Thirteen:",
                    "",
                    "I come for Thee, my",
                    "Master.  I come!  The",
                    "way is clear, I have",
                    "found Thy path and",
                    "washed it in the blood"
                ),
                new BookPageInfo
                (
                    "of the two workers",
                    "that caught sight of",
                    "me.  Ah, how sweet it",
                    "was to cut them open,",
                    "to see the blood pour",
                    "out in great torrents, to",
                    "stand in it, to revel in",
                    "it.  If only I had time"
                ),
                new BookPageInfo
                (
                    "for the Sewel woman.",
                    "But there will be time",
                    "enough for her.  I",
                    "have learned Thy",
                    "Patience, Master.  I",
                    "come for Thee.  I walk",
                    "Thy halls in penance,",
                    "my last steps in this"
                ),
                new BookPageInfo
                (
                    "repulsive living",
                    "frame.  I come for",
                    "Thee and Thy Gifts,",
                    "my Master.  Glory",
                    "Unto Thee, Khal",
                    "Ankur, Keeper of the",
                    "Seventh Death,",
                    "Master, Leader of the"
                ),
                new BookPageInfo
                (
                    "Chosen, the Khaldun.",
                    "Praises in Thy",
                    "Name, Master of Life",
                    "and Death, Lord of All.",
                    " Khal Ankur, Master,",
                    "Prophet, I join Thy",
                    "ranks this night, a",
                    "member of the"
                ),
                new BookPageInfo
                (
                    "Khaldun at last!"
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public LysanderNotebook11()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public LysanderNotebook11(Serial serial)
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

    /// <summary>
    /// Tavaras Journal
    /// </summary>
    public class TavarasJournal1 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day One:",
                    "",
                    "The workers continue",
                    "tirelessly in their",
                    "efforts to unload our",
                    "supplies even as light",
                    "fades.  I feel I should",
                    "lend a hand in the"
                ),
                new BookPageInfo
                (
                    "effort, and yet I",
                    "cannot bear to take my",
                    "attention away from",
                    "the magnificent stone",
                    "doors of the tomb.",
                    "Every inch of their",
                    "massive frame is",
                    "covered with"
                ),
                new BookPageInfo
                (
                    "intricately carved",
                    "design work - 'tis",
                    "truly a sight to see.",
                    "I've spent the day",
                    "sketching and",
                    "cataloging what I can",
                    "of them while my",
                    "companions set up our"
                ),
                new BookPageInfo
                (
                    "camp and make",
                    "preparations for",
                    "tomorrow's work.",
                    "Though the stonework",
                    "symbols inspire me to",
                    "new flights of fancy,",
                    "some of the workers",
                    "seem strangely"
                ),
                new BookPageInfo
                (
                    "fearful of them.  I",
                    "cannot wait 'til the",
                    "morrow when those",
                    "ancient works of stone",
                    "shall swing open and",
                    "deliver unto me",
                    "everything I have",
                    "dreamed of for the"
                ),
                new BookPageInfo
                (
                    "last ten years of my",
                    "life."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal1()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal1(Serial serial)
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

    public class TavarasJournal2 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Two:",
                    "",
                    "Everything we'd",
                    "heard and read of the",
                    "tomb has proved",
                    "correct - and yet,",
                    "nothing could prepare",
                    "me for the sight of it"
                ),
                new BookPageInfo
                (
                    "with my own eyes.",
                    "The Tomb of Khal",
                    "Ankur has given up",
                    "its secrets at last!  The",
                    "intricate stonework",
                    "that covered the tomb",
                    "doors seems to",
                    "continue throughout"
                ),
                new BookPageInfo
                (
                    "the entirety of the",
                    "catacombs, each",
                    "hallway and room",
                    "yielding a seemingly",
                    "endless amount of",
                    "information for my",
                    "companions and I to",
                    "record.  It will take"
                ),
                new BookPageInfo
                (
                    "years to catalogue the",
                    "entirety of the Tomb,",
                    "if those legends of its",
                    "massive size prove",
                    "true.  Sadly, a good",
                    "deal of the Tomb's",
                    "interior has been",
                    "damaged or utterly"
                ),
                new BookPageInfo
                (
                    "destroyed, whether",
                    "by seismic activity in",
                    "the surrounding",
                    "mountainside or",
                    "merely the slow",
                    "efforts of Time",
                    "itself, I do not know.",
                    "A good deal of the"
                ),
                new BookPageInfo
                (
                    "stonework has been",
                    "cracked or collapsed",
                    "entirely, especially",
                    "near the entrance",
                    "supports of the main",
                    "hall.  Our passage has",
                    "indeed already been",
                    "entirely blocked in the"
                ),
                new BookPageInfo
                (
                    "first major room",
                    "we've discovered, a",
                    "massive pile of",
                    "boulders and stones",
                    "blocking any exit",
                    "from the",
                    "antechamber.  What",
                    "could have caused"
                ),
                new BookPageInfo
                (
                    "such a localized",
                    "disruption of the",
                    "support structures,",
                    "one can only guess -",
                    "but it will surely take",
                    "an entire afternoon's",
                    "effort to remove even",
                    "a fraction of it.  I look"
                ),
                new BookPageInfo
                (
                    "forward to more",
                    "progress tomorrow",
                    "once the workers have",
                    "set to excavating the",
                    "hall."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal2()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal2(Serial serial)
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

    public class TavarasJournal3 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Three - Day Five:",
                    "",
                    "I do not understand",
                    "this place... not as I",
                    "once thought I did.",
                    "Something palatable",
                    "seems to hinder our",
                    "every attempt to"
                ),
                new BookPageInfo
                (
                    "investigate this",
                    "ancient site.",
                    "Excavation work on",
                    "the first major",
                    "hallway finished only",
                    "yesterday - the",
                    "amount of stone and",
                    "rubble blocking the"
                ),
                new BookPageInfo
                (
                    "egress was",
                    "astounding, it stands",
                    "in immense piles",
                    "outside the Tomb's",
                    "entrance, as if we",
                    "were digging the",
                    "tunnels of this",
                    "abhorred place"
                ),
                new BookPageInfo
                (
                    "ourselves!  The",
                    "satisfaction of",
                    "completing our efforts",
                    "was quickly thwarted,",
                    "however, as we",
                    "discovered the end of",
                    "the hallway we had",
                    "just revealed was"
                ),
                new BookPageInfo
                (
                    "blocked by yet another",
                    "colossal pile of stone.",
                    "I've had a few of the",
                    "workers set up",
                    "primitive scaffolding",
                    "in the main",
                    "antechamber so that I",
                    "can spend my time"
                ),
                new BookPageInfo
                (
                    "pouring over the detail",
                    "work on the stone",
                    "carvings while the",
                    "rest of our crew",
                    "continue excavating",
                    "the inner halls."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal3()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal3(Serial serial)
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

    public class TavarasJournal6 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Six:",
                    "",
                    "Late last night our",
                    "camp was set upon by",
                    "a pack of wild beasts",
                    "- behemoth creatures",
                    "with a speed and",
                    "viciousness I'd n'ere"
                ),
                new BookPageInfo
                (
                    "before seen.  Even",
                    "Grimmoch, well",
                    "versed in all manner",
                    "of wildlife, was",
                    "unsure as to their",
                    "nature - though I lay",
                    "blame upon the",
                    "darkness covering"
                ),
                new BookPageInfo
                (
                    "their movements",
                    "rather than on his",
                    "skill as a huntsman.",
                    "The attacks did not let",
                    "up the entire night,",
                    "and we were",
                    "eventually forced to",
                    "flee into the Tomb"
                ),
                new BookPageInfo
                (
                    "itself to take refuge",
                    "from the ravenous",
                    "creatures - e'en",
                    "Lysander's spells",
                    "could not keep the foul",
                    "things from attacking",
                    "in great numbers.",
                    "The Tomb performed"
                ),
                new BookPageInfo
                (
                    "well as an impromptu",
                    "fortress, and we",
                    "managed to spend the",
                    "night unscathed.",
                    "Morning's light",
                    "seemed to have",
                    "scattered the beasts,",
                    "as not a single one of"
                ),
                new BookPageInfo
                (
                    "them was to be seen as",
                    "we exited the Tomb -",
                    "not even a carcass of",
                    "the few that were",
                    "slain a'fore we fled.",
                    "Lysander set the crew",
                    "to work, moving our",
                    "supplies and gear into"
                ),
                new BookPageInfo
                (
                    "the Tomb, in case the",
                    "creatures did opt to",
                    "return.  Such savage",
                    "fury had the beasts -",
                    "and not a single one",
                    "ever turned to run,",
                    "even in the face of",
                    "certain death."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal6()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal6(Serial serial)
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

    public class TavarasJournal7 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Seven:",
                    "",
                    "T'was written that,",
                    "upon his death, Khal",
                    "Ankur's followers,",
                    "those known as the",
                    "Keepers of the",
                    "Seventh Death, sealed"
                ),
                new BookPageInfo
                (
                    "themselves within the",
                    "Sanctum they had",
                    "carved from the",
                    "mountains in his",
                    "honor.  The Zealots of",
                    "his order entombed",
                    "the lesser followers",
                    "alive, then, when all"
                ),
                new BookPageInfo
                (
                    "but two remained, slit",
                    "their throats and",
                    "joined Khal Ankur in",
                    "death.  Surely this is",
                    "not surprising for a",
                    "Cult that worshipped",
                    "death and sacrifice so",
                    "vehemently as it is"
                ),
                new BookPageInfo
                (
                    "said that the Keepers",
                    "did - and yet, to be in",
                    "this Tomb, to know",
                    "that somewhere in its",
                    "depths hundreds upon",
                    "hundreds of bodies",
                    "lay, sealed alive at",
                    "their own behest..."
                ),
                new BookPageInfo
                (
                    "I must confess that",
                    "the very thought of it",
                    "troubles my dreams at",
                    "night.  I've asked",
                    "Lysander if we might",
                    "reestablish the camp",
                    "outside the Tomb,",
                    "setting up night"
                ),
                new BookPageInfo
                (
                    "watches and some sort",
                    "of fortification, but",
                    "he'll have none of it.  I",
                    "did not press the",
                    "issue, as I suddenly",
                    "felt foolish even at",
                    "my askance."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal7()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal7(Serial serial)
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

    public class TavarasJournal8 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Eight :",
                    "",
                    "Astounding progress",
                    "was made today, and",
                    "my very head spins",
                    "with the excitement",
                    "of it.  Upon full",
                    "excavation of the far"
                ),
                new BookPageInfo
                (
                    "western hall, another",
                    "large antechamber",
                    "was revealed.  By the",
                    "larger, mosaic style of",
                    "the wall carvings and",
                    "their framing, as well",
                    "as the numerous",
                    "vellum scrolls and"
                ),
                new BookPageInfo
                (
                    "tomes held within, the",
                    "room appears to have",
                    "been a great museum",
                    "or library of sorts.",
                    "The sheer amount of",
                    "written information",
                    "encased within this",
                    "room would surely"
                ),
                new BookPageInfo
                (
                    "take me decades to",
                    "study e'en if I could",
                    "immediately decipher",
                    "the strange text with",
                    "which it was written.",
                    "My sheer joy at the",
                    "discovery was quickly",
                    "noted by the brute"
                ),
                new BookPageInfo
                (
                    "known as Morg",
                    "Bergen, who, even in",
                    "his simple way,",
                    "seemed just as",
                    "delighted as I that",
                    "some progress had",
                    "been made.  I must",
                    "confess, upon his"
                ),
                new BookPageInfo
                (
                    "inclusion in our party",
                    "at the beginning of",
                    "this journey I was",
                    "somewhat suspect of",
                    "his nature, but he has",
                    "a startlingly quick wit",
                    "about him for such a",
                    "massive, calloused"
                ),
                new BookPageInfo
                (
                    "warrior.  While",
                    "Lysander and e'en",
                    "Grimmoch always",
                    "seem to investigate the",
                    "tomb with a scowling",
                    "determination, Bergen",
                    "seems to feel the same",
                    "thrill of discovery as"
                ),
                new BookPageInfo
                (
                    "I.  I am proud to now",
                    "count him as a friend,",
                    "and am thankful for",
                    "his laughter as well",
                    "as his strength."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal8()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal8(Serial serial)
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

    public class TavarasJournal9 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Nine - Day Ten:",
                    "",
                    "The excavation of the",
                    "next set of tunnels",
                    "has ceased, as three",
                    "of the workers have",
                    "gone missing in the",
                    "night.  Bergen voiced"
                ),
                new BookPageInfo
                (
                    "the opinion that they",
                    "had most likely",
                    "abandoned our group",
                    "altogether and headed",
                    "back, as they were of",
                    "the number that",
                    "seemed especially",
                    "disturbed by the"
                ),
                new BookPageInfo
                (
                    "Tomb.  Lysander had",
                    "other ideas, however.",
                    "In the middle of our",
                    "discussion on the",
                    "matter, he went into a",
                    "wild tirade on the",
                    "possibility that they",
                    "had somehow"
                ),
                new BookPageInfo
                (
                    "infiltrated the tomb's",
                    "interior without us.",
                    "The pure, hateful",
                    "venom in his voice",
                    "when he spoke of the",
                    "workers shocked me,",
                    "as I had always",
                    "thought him to be a"
                ),
                new BookPageInfo
                (
                    "levelheaded man of",
                    "great learning.  As we",
                    "are still at work",
                    "digging out the rubble",
                    "that blocks all access",
                    "to the inner chambers,",
                    "I cannot help but",
                    "believe the workers"
                ),
                new BookPageInfo
                (
                    "must have fled the",
                    "site altogether, as",
                    "Bergen said."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal9()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal9(Serial serial)
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

    public class TavarasJournal11 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Eleven - Day",
                    "Thirteen:",
                    "",
                    "Two more workers",
                    "have gone missing.",
                    "Even more disturbing",
                    "is the fact that",
                    "Lysander has joined"
                ),
                new BookPageInfo
                (
                    "them. Late last night",
                    "the workers finished",
                    "excavating the next",
                    "main hall, and we",
                    "retired to the main",
                    "antechamber and our",
                    "camp to rest up for",
                    "exploration on the"
                ),
                new BookPageInfo
                (
                    "'morrow.  In the",
                    "middle of the night we",
                    "woke to a strange",
                    "howling sound, and as",
                    "the men prepared",
                    "themselves for",
                    "another onslaught of",
                    "the beasts that had"
                ),
                new BookPageInfo
                (
                    "troubled our outer",
                    "camp, it was noticed",
                    "that Lysander was",
                    "nowhere in our",
                    "number.  I cannot",
                    "fathom where he has",
                    "gone - the newly",
                    "revealed chamber"
                ),
                new BookPageInfo
                (
                    "holds no immediate",
                    "egress, blocked again",
                    "by piles of stone and",
                    "rubble, and I cannot",
                    "believe that Lysander,",
                    "of all people, would",
                    "have fled this site -",
                    "indeed, he had lately"
                ),
                new BookPageInfo
                (
                    "grown almost fanatical",
                    "in his work to",
                    "discover more of the",
                    "secrets barred to us",
                    "by the consistently",
                    "slow progress of",
                    "excavating each new",
                    "hallway.  The men are"
                ),
                new BookPageInfo
                (
                    "at work even now, and",
                    "as the ceaseless",
                    "thumps and cracks of",
                    "their picks",
                    "reverberate",
                    "throughout the",
                    "entirety of the tomb,",
                    "the dust continues to"
                ),
                new BookPageInfo
                (
                    "pour down from the",
                    "ancient stonework",
                    "above us like some",
                    "horrible, eldritch",
                    "curse upon us all."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal11()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal11(Serial serial)
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

    public class TavarasJournal14 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Fourteen - Day",
                    "Fifteen:",
                    "",
                    "Lysander has",
                    "returned... and yet,",
                    "how can I describe the",
                    "horror of it?  He",
                    "stands across the"
                ),
                new BookPageInfo
                (
                    "chamber from me",
                    "even now, a changed",
                    "man.  His hair hangs",
                    "in grimy knots across",
                    "his face, his clothes",
                    "filthy and torn in",
                    "places... and the blood",
                    "- covered in blood, his"
                ),
                new BookPageInfo
                (
                    "skin shining in",
                    "scarlet reflections of",
                    "the torchlight.  He",
                    "will let no one",
                    "approach; a thick,",
                    "rusted dagger in his",
                    "hand warding off any",
                    "attempts to overcome"
                ),
                new BookPageInfo
                (
                    "him.  And the blood,",
                    "which runs down in",
                    "great rivulets from",
                    "his arms and hands -",
                    "it is not his own, and",
                    "this is enough to keep",
                    "us at a wary distance.",
                    "Morg Bergen wishes"
                ),
                new BookPageInfo
                (
                    "to subdue him",
                    "quickly, but there is",
                    "something in",
                    "Lysander's eyes - and",
                    "I remember the power",
                    "of his spells, even as",
                    "he swings the jagged",
                    "dagger back and forth"
                ),
                new BookPageInfo
                (
                    "in a wide swath",
                    "before him.",
                    "Something about the",
                    "sight of it makes my",
                    "stomach churn.",
                    "Something has",
                    "happened, something",
                    "that changes"
                ),
                new BookPageInfo
                (
                    "everything.  Lysander",
                    "has lost his sanity to",
                    "this tomb... or to",
                    "something within it.",
                    "Do we dare approach?",
                    "We must make a",
                    "decision soon."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal14()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal14(Serial serial)
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

    public class TavarasJournal16 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Sixteen:",
                    "",
                    "Why do I write?  I",
                    "must... not so much",
                    "because there must be",
                    "some record of",
                    "this... what's",
                    "happened here... as"
                ),
                new BookPageInfo
                (
                    "for my own sanity.",
                    "The act of putting pen",
                    "to paper calms me,",
                    "focuses me, even in",
                    "this madness.",
                    "Lysander is dead.  So",
                    "many are dead.  And",
                    "we're trapped here,"
                ),
                new BookPageInfo
                (
                    "trapped forever in",
                    "this nightmare.  He",
                    "would not let us pass,",
                    "wild in his psychosis,",
                    "furious, spitting,",
                    "covered in blood, he",
                    "swung the ancient",
                    "dagger at any who"
                ),
                new BookPageInfo
                (
                    "approached.  He",
                    "babbled incoherently,",
                    "cursed at us, the most",
                    "hateful curses,",
                    "prophecy, doom upon",
                    "us.  Bergen would",
                    "have none of it.",
                    "Finally, he leapt at"
                ),
                new BookPageInfo
                (
                    "Lysander, his",
                    "massive axe at his",
                    "side.  But he would not",
                    "be the end of the mad",
                    "mage... no... they",
                    "were... those hands,",
                    "covered in the dirt of",
                    "the grave, maggots,"
                ),
                new BookPageInfo
                (
                    "filth.  They rose up",
                    "behind Lysander.",
                    "That look of curiosity",
                    "on the mage's face as",
                    "Bergen skidded to a",
                    "halt... t'was almost a",
                    "moment of sanity for",
                    "him, surely, to"
                ),
                new BookPageInfo
                (
                    "attempt to comprehend",
                    "what could have",
                    "stopped the warrior in",
                    "his tracks.  And then",
                    "they were upon him.",
                    "Skeletal hands, arms",
                    "and faces with loose,",
                    "corrupted flesh"
                ),
                new BookPageInfo
                (
                    "hanging from yellow",
                    "bone.  Inhuman, yet",
                    "once human,",
                    "staggering towards us",
                    "as their companions",
                    "tore at Lysander,",
                    "coming towards us in",
                    "droves."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal16()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal16(Serial serial)
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

    public class TavarasJournal16b : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Sixteen, Later :",
                    "",
                    "We ran. What could",
                    "we do?  We ran back",
                    "towards the entrance,",
                    "cutting at them when",
                    "we could.  T'was a",
                    "nightmare, and yet"
                ),
                new BookPageInfo
                (
                    "nothing to prepare us",
                    "for what would come.",
                    "We were almost there,",
                    "the entrance to this",
                    "abhorred crypt in",
                    "sight.  Then the earth",
                    "shook with such a",
                    "force that we were"
                ),
                new BookPageInfo
                (
                    "dropped to our hands",
                    "and knees, stumbling",
                    "in the darkness with",
                    "those... those things",
                    "surely behind us.",
                    "The noise of falling",
                    "rock and crumbling",
                    "stone drowned out our"
                ),
                new BookPageInfo
                (
                    "piteous cries.  No sign",
                    "of the entrance",
                    "remained.",
                    "We owe our lives to",
                    "Bergen, whose wits",
                    "returned quickly.",
                    "That he could make us",
                    "hurry back into the"
                ),
                new BookPageInfo
                (
                    "main antechamber...",
                    "actually run back",
                    "towards those eldritch",
                    "dead that stalked us.",
                    "But we did, the",
                    "strength of his",
                    "convictions enough for",
                    "us in the moment."
                ),
                new BookPageInfo
                (
                    "And at our campsite",
                    "we erected our last",
                    "defense, a pitiable",
                    "wall of wood and",
                    "stone, anything at",
                    "hand that might block",
                    "the tide of those",
                    "nightmare creatures."
                ),
                new BookPageInfo
                (
                    "And I sit against it",
                    "even now.  I can hear",
                    "their moans, their",
                    "wailing cries in the",
                    "distance - they'll be",
                    "here soon, even at the",
                    "unhurried pace of the",
                    "shuffling dead."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal16b()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal16b(Serial serial)
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

    public class TavarasJournal17 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Seventeen - Day",
                    "Eighteen :",
                    "",
                    "I cannot go on much",
                    "longer.  I know now",
                    "t'was no work of the",
                    "earth that trapped us",
                    "here - I can feel His"
                ),
                new BookPageInfo
                (
                    "force in it.  It was His",
                    "will, His power that",
                    "has sealed us here in",
                    "this nightmare.  The",
                    "barricade will not be",
                    "enough.  So many of",
                    "them. They come like",
                    "unto the ocean's waves"
                ),
                new BookPageInfo
                (
                    "- ceaseless,",
                    "neverending.  For",
                    "every five we strike",
                    "down, another ten rise",
                    "up against us.  And",
                    "like the sands we",
                    "cannot help but be",
                    "brought down, wasted"
                ),
                new BookPageInfo
                (
                    "away in this ocean of",
                    "blood."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal17()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal17(Serial serial)
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

    public class TavarasJournal19 : BaseBook
    {
        public static readonly BookContent Content = new BookContent
            (
                "Journal: Discovery of the Tomb", "Tavara Sewel",
                new BookPageInfo
                (
                    "Day Nineteen - Day",
                    "Twenty-One :",
                    "",
                    "The barricade won't",
                    "hold - never, and",
                    "they'll come, they",
                    "come even now.  I",
                    "would tear the last of"
                ),
                new BookPageInfo
                (
                    "it down, let them in to",
                    "devour us all, if only",
                    "to stop the screaming",
                    "- the awful, wailing",
                    "cries that fill the tomb",
                    "with their presence.",
                    "May my ancestors",
                    "forgive me, but it"
                ),
                new BookPageInfo
                (
                    "must be done. I must",
                    "end this."
                )
            );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public TavarasJournal19()
            : base(Utility.Random(0xFF1, 2), false)
        {
        }

        public TavarasJournal19(Serial serial)
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