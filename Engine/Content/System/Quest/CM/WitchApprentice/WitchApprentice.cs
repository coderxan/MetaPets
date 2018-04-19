using System;

using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Hag
{
    public class WitchApprenticeQuest : QuestSystem
    {
        private static Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( Hag.FindApprenticeObjective ),
				typeof( Hag.FindGrizeldaAboutMurderObjective ),
				typeof( Hag.KillImpsObjective ),
				typeof( Hag.FindZeefzorpulObjective ),
				typeof( Hag.ReturnRecipeObjective ),
				typeof( Hag.FindIngredientObjective ),
				typeof( Hag.ReturnIngredientsObjective ),
				typeof( Hag.DontOfferConversation ),
				typeof( Hag.AcceptConversation ),
				typeof( Hag.HagDuringCorpseSearchConversation ),
				typeof( Hag.ApprenticeCorpseConversation ),
				typeof( Hag.MurderConversation ),
				typeof( Hag.HagDuringImpSearchConversation ),
				typeof( Hag.ImpDeathConversation ),
				typeof( Hag.ZeefzorpulConversation ),
				typeof( Hag.RecipeConversation ),
				typeof( Hag.HagDuringIngredientsConversation ),
				typeof( Hag.BlackheartFirstConversation ),
				typeof( Hag.BlackheartNoPirateConversation ),
				typeof( Hag.BlackheartPirateConversation ),
				typeof( Hag.EndConversation ),
				typeof( Hag.RecentlyFinishedConversation )
			};

        public override Type[] TypeReferenceTable { get { return m_TypeReferenceTable; } }

        public override object Name
        {
            get
            {
                // "The Witch's Apprentice"
                return 1055042;
            }
        }

        public override object OfferMessage
        {
            get
            {
                /* <I>The ancient, wrinkled hag looks up from her vile-smelling cauldron.
                 * Her single, unblinking eye attempts to focus in on you, but to
                 * little avail.</I><BR><BR>
                 * 
                 * Eh? Who is it? Who's there?  Come to trouble an old woman have you?<BR><BR>
                 * 
                 * I'll split ye open and swallow yer guts!  I'll turn ye into a pile
                 * o' goo, I will!  Bah!  As if I didn't have enough to worry about.  As if I've
                 * not enough trouble as it is!<BR><BR>
                 * 
                 * Another of my blasted apprentices has gone missing!  Foolish children,
                 * think they know everything.  I should turn the lot of them into toads -
                 * if only they'd return with their task complete!  But that's the trouble, innit?
                 * They never return!<BR><BR>
                 * 
                 * But you don't care, do ye?  I suppose you're another one of those meddlesome kids,
                 * come to ask me for something?  Eh?  Is that it?  You want something from me,
                 * expect me to hand it over?  I've enough troubles with my apprentices, and that
                 * vile imp, Zeefzorpul!  Why, I bet it's him who's got the lot of them!  And who
                 * knows what he's done?  Vile little thing.<BR><BR>
                 * 
                 * If you expect me to help you with your silly little desires, you'll be doing
                 * something for me first, eh?  I expect you to go seek out my apprentice.
                 * I sent him along the road west of here up towards Yew's graveyard, but he never
                 * came back. Find him, and bring him back, and I'll give you a little reward that
                 * I'm sure you'll find pleasant.<BR><BR>
                 * 
                 * But I tells ye to watch out for the imp name've Zeefzorpul!  He's a despicable
                 * little beast who likes to fool and fiddle with folk and generally make life
                 * miserable for everyone.  If ye get him on your bad side, you're sure to end up
                 * ruing the day ye were born. As if you didn't already, with an ugly mug
                 * like that!<BR><BR>
                 * 
                 * Well, you little whelp?  Going to help an old hag or not?
                 */
                return 1055001;
            }
        }

        public override TimeSpan RestartDelay { get { return TimeSpan.FromMinutes(5.0); } }
        public override bool IsTutorial { get { return false; } }

        public override int Picture { get { return 0x15D3; } }

        public WitchApprenticeQuest(PlayerMobile from)
            : base(from)
        {
        }

        // Serialization
        public WitchApprenticeQuest()
        {
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }

        private static Point3D[] m_ZeefzorpulLocations = new Point3D[]
			{
				new Point3D( 1226, 1573, 0 ),
				new Point3D( 1929, 1148, 0 ),
				new Point3D( 1366, 2723, 0 ),
				new Point3D( 1675, 2984, 0 ),
				new Point3D( 2177, 3367, 10 ),
				new Point3D( 1171, 3594, 0 ),
				new Point3D( 1010, 2667, 5 ),
				new Point3D( 1591, 2156, 5 ),
				new Point3D( 2592, 464, 60 ),
				new Point3D( 474, 1654, 0 ),
				new Point3D( 897, 2411, 0 ),
				new Point3D( 1471, 2505, 5 ),
				new Point3D( 1257, 872, 16 ),
				new Point3D( 2581, 1118, 0 ),
				new Point3D( 2513, 1102, 0 ),
				new Point3D( 1608, 3371, 0 ),
				new Point3D( 4687, 1179, 0 ),
				new Point3D( 3704, 2196, 20 ),
				new Point3D( 3346, 572, 0 ),
				new Point3D( 569, 1309, 0 )
			};

        public static Point3D RandomZeefzorpulLocation()
        {
            int index = Utility.Random(m_ZeefzorpulLocations.Length);

            return m_ZeefzorpulLocations[index];
        }
    }

    #region Questing Objectives

    public class FindApprenticeObjective : QuestObjective
    {
        private static Point3D[] m_CorpseLocations = new Point3D[]
			{
				new Point3D( 778, 1158, 0 ),
				new Point3D( 698, 1443, 0 ),
				new Point3D( 785, 1548, 0 ),
				new Point3D( 734, 1504, 0 ),
				new Point3D( 819, 1266, 0 )
			};

        private static Point3D RandomCorpseLocation()
        {
            int index = Utility.Random(m_CorpseLocations.Length);

            return m_CorpseLocations[index];
        }

        private Corpse m_Corpse;
        private Point3D m_CorpseLocation;

        public override object Message
        {
            get
            {
                /* To the west of the Hag's house lies the road between Skara Brae
                 * and Yew.  Follow it carefully toward Yew's graveyard, and search for
                 * any sign of the Hag's apprentice along the road.
                 */
                return 1055014;
            }
        }

        public Corpse Corpse { get { return m_Corpse; } }

        public FindApprenticeObjective(bool init)
        {
            if (init)
                m_CorpseLocation = RandomCorpseLocation();
        }

        public FindApprenticeObjective()
        {
        }

        public override void CheckProgress()
        {
            PlayerMobile player = System.From;
            Map map = player.Map;

            if ((m_Corpse == null || m_Corpse.Deleted) && (map == Map.Trammel || map == Map.Felucca) && player.InRange(m_CorpseLocation, 8))
            {
                m_Corpse = new HagApprenticeCorpse();
                m_Corpse.MoveToWorld(m_CorpseLocation, map);

                Effects.SendLocationEffect(m_CorpseLocation, map, 0x3728, 10, 10);
                Effects.PlaySound(m_CorpseLocation, map, 0x1FE);

                Mobile imp = new Zeefzorpul();
                imp.MoveToWorld(m_CorpseLocation, map);

                // * You see a strange imp stealing a scrap of paper from the bloodied corpse *
                m_Corpse.SendLocalizedMessageTo(player, 1055049);

                Timer.DelayCall(TimeSpan.FromSeconds(3.0), new TimerStateCallback(DeleteImp), imp);
            }
        }

        private void DeleteImp(object imp)
        {
            Mobile m = imp as Mobile;

            if (m != null && !m.Deleted)
            {
                Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);
                Effects.PlaySound(m.Location, m.Map, 0x1FE);

                m.Delete();
            }
        }

        public override void OnComplete()
        {
            System.AddConversation(new ApprenticeCorpseConversation());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_CorpseLocation = reader.ReadPoint3D();
                        goto case 0;
                    }
                case 0:
                    {
                        m_Corpse = (Corpse)reader.ReadItem();
                        break;
                    }
            }

            if (version == 0)
                m_CorpseLocation = RandomCorpseLocation();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            if (m_Corpse != null && m_Corpse.Deleted)
                m_Corpse = null;

            writer.WriteEncodedInt((int)1); // version

            writer.Write((Point3D)m_CorpseLocation);
            writer.Write((Item)m_Corpse);
        }
    }

    public class FindGrizeldaAboutMurderObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Return to the Hag to tell her of the vile imp Zeefzorpul's role
                 * in the murder of her Apprentice, and the subsequent theft of a mysterious
                 * scrap of parchment from the corpse.
                 */
                return 1055015;
            }
        }

        public FindGrizeldaAboutMurderObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new MurderConversation());
        }
    }

    public class KillImpsObjective : QuestObjective
    {
        private int m_MaxProgress;

        public override object Message
        {
            get
            {
                /* Search the realm for any imps you can find, and slash, bash, mash,
                 * or fry them with magics until one of them gives up the secret hiding
                 * place of the imp Zeefzorpul.
                 */
                return 1055016;
            }
        }

        public override int MaxProgress { get { return m_MaxProgress; } }

        public KillImpsObjective(bool init)
        {
            if (init)
                m_MaxProgress = Utility.RandomMinMax(1, 4);
        }

        public KillImpsObjective()
        {
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            if (!Completed && from is Imp)
                return true;

            return false;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if (creature is Imp)
                CurProgress++;
        }

        public override void OnComplete()
        {
            PlayerMobile from = System.From;

            Point3D loc = WitchApprenticeQuest.RandomZeefzorpulLocation();

            MapItem mapItem = new MapItem();
            mapItem.SetDisplay(loc.X - 200, loc.Y - 200, loc.X + 200, loc.Y + 200, 200, 200);
            mapItem.AddWorldPin(loc.X, loc.Y);
            from.AddToBackpack(mapItem);

            from.AddToBackpack(new MagicFlute());

            from.SendLocalizedMessage(1055061); // You have received a map and a magic flute.

            System.AddConversation(new ImpDeathConversation(loc));
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_MaxProgress = reader.ReadInt();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((int)m_MaxProgress);
        }
    }

    public class FindZeefzorpulObjective : QuestObjective
    {
        private Point3D m_ImpLocation;

        public override object Message
        {
            get
            {
                /* Find the location shown in the map that the imp gave you. When you
                 * have arrived at the location, play the magic flute he provided,
                 * and the imp Zeefzorpul will be drawn to your presence.
                 */
                return 1055017;
            }
        }

        public Point3D ImpLocation { get { return m_ImpLocation; } }

        public FindZeefzorpulObjective(Point3D impLocation)
        {
            m_ImpLocation = impLocation;
        }

        public FindZeefzorpulObjective()
        {
        }

        public override void OnComplete()
        {
            Mobile from = System.From;
            Map map = from.Map;

            Effects.SendLocationEffect(m_ImpLocation, map, 0x3728, 10, 10);
            Effects.PlaySound(m_ImpLocation, map, 0x1FE);

            Mobile imp = new Zeefzorpul();
            imp.MoveToWorld(m_ImpLocation, map);

            imp.Direction = imp.GetDirectionTo(from);

            Timer.DelayCall(TimeSpan.FromSeconds(3.0), new TimerStateCallback(DeleteImp), imp);
        }

        private void DeleteImp(object imp)
        {
            Mobile m = imp as Mobile;

            if (m != null && !m.Deleted)
            {
                Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);
                Effects.PlaySound(m.Location, m.Map, 0x1FE);

                m.Delete();
            }

            System.From.SendLocalizedMessage(1055062); // You have received the Magic Brew Recipe.

            System.AddConversation(new ZeefzorpulConversation());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_ImpLocation = reader.ReadPoint3D();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((Point3D)m_ImpLocation);
        }
    }

    public class ReturnRecipeObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Return to the old Hag and tell her you have recovered her Magic
                 * Brew Recipe from the bizarre imp named Zeefzorpul.
                 */
                return 1055018;
            }
        }

        public ReturnRecipeObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new RecipeConversation());
        }
    }

    public class FindIngredientObjective : QuestObjective
    {
        private Ingredient[] m_Ingredients;
        private bool m_BlackheartMet;

        public override object Message
        {
            get
            {
                if (!m_BlackheartMet)
                {
                    switch (Step)
                    {
                        case 1:
                            /* You must gather each ingredient on the Hag's list so that she can cook
                             * up her vile Magic Brew.  The first ingredient is :
                             */
                            return 1055019;
                        case 2:
                            /* You must gather each ingredient on the Hag's list so that she can cook
                             * up her vile Magic Brew.  The second ingredient is :
                             */
                            return 1055044;
                        default:
                            /* You must gather each ingredient on the Hag's list so that she can cook
                             * up her vile Magic Brew.  The final ingredient is :
                             */
                            return 1055045;
                    }
                }
                else
                {
                    /* You are still attempting to obtain a jug of Captain Blackheart's
                     * Whiskey, but the drunkard Captain refuses to share his unique brew.
                     * You must prove your worthiness as a pirate to Blackheart before he'll
                     * offer you a jug.
                     */
                    return 1055055;
                }
            }
        }

        public override int MaxProgress
        {
            get
            {
                IngredientInfo info = IngredientInfo.Get(this.Ingredient);

                return info.Quantity;
            }
        }

        public Ingredient[] Ingredients { get { return m_Ingredients; } }
        public Ingredient Ingredient { get { return m_Ingredients[m_Ingredients.Length - 1]; } }
        public int Step { get { return m_Ingredients.Length; } }
        public bool BlackheartMet { get { return m_BlackheartMet; } }

        public FindIngredientObjective(Ingredient[] oldIngredients)
            : this(oldIngredients, false)
        {
        }

        public FindIngredientObjective(Ingredient[] oldIngredients, bool blackheartMet)
        {
            if (!blackheartMet)
            {
                m_Ingredients = new Ingredient[oldIngredients.Length + 1];

                for (int i = 0; i < oldIngredients.Length; i++)
                    m_Ingredients[i] = oldIngredients[i];

                m_Ingredients[m_Ingredients.Length - 1] = IngredientInfo.RandomIngredient(oldIngredients);
            }
            else
            {
                m_Ingredients = new Ingredient[oldIngredients.Length];

                for (int i = 0; i < oldIngredients.Length; i++)
                    m_Ingredients[i] = oldIngredients[i];
            }

            m_BlackheartMet = blackheartMet;
        }

        public FindIngredientObjective()
        {
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                IngredientInfo info = IngredientInfo.Get(this.Ingredient);

                gump.AddHtmlLocalized(70, 260, 270, 100, info.Name, BaseQuestGump.Blue, false, false);
                gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, info.Quantity.ToString());
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            if (Completed)
                return false;

            IngredientInfo info = IngredientInfo.Get(this.Ingredient);
            Type fromType = from.GetType();

            for (int i = 0; i < info.Creatures.Length; i++)
            {
                if (fromType == info.Creatures[i])
                    return true;
            }

            return false;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            IngredientInfo info = IngredientInfo.Get(this.Ingredient);

            for (int i = 0; i < info.Creatures.Length; i++)
            {
                Type type = info.Creatures[i];

                if (creature.GetType() == type)
                {
                    System.From.SendLocalizedMessage(1055043, "#" + info.Name); // You gather a ~1_INGREDIENT_NAME~ from the corpse.

                    CurProgress++;

                    break;
                }
            }
        }

        public override void OnComplete()
        {
            if (this.Ingredient != Ingredient.Whiskey)
            {
                NextStep();
            }
        }

        public void NextStep()
        {
            System.From.SendLocalizedMessage(1055046); // You have completed your current task on the Hag's Magic Brew Recipe list.

            if (Step < 3)
                System.AddObjective(new FindIngredientObjective(m_Ingredients));
            else
                System.AddObjective(new ReturnIngredientsObjective());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Ingredients = new Ingredient[reader.ReadEncodedInt()];
            for (int i = 0; i < m_Ingredients.Length; i++)
                m_Ingredients[i] = (Ingredient)reader.ReadEncodedInt();

            m_BlackheartMet = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.WriteEncodedInt((int)m_Ingredients.Length);
            for (int i = 0; i < m_Ingredients.Length; i++)
                writer.WriteEncodedInt((int)m_Ingredients[i]);

            writer.Write((bool)m_BlackheartMet);
        }
    }

    public class ReturnIngredientsObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have gathered all the ingredients listed in the Hag's Magic Brew
                 * Recipe.  Return to the Hag and tell her you have completed her task.
                 */
                return 1055050;
            }
        }

        public ReturnIngredientsObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new EndConversation());
        }
    }

    #endregion

    #region Mobile Conversation

    public class DontOfferConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The ancient, wrinkled hag looks up from her vile-smelling cauldron.
                 * Her single, unblinking eye attempts to focus in on you, but to
                 * little avail.</I><BR><BR>
                 * 
                 * What's that?  Who's there?  What do you want with me?  I don't have
                 * time for the likes of you.  I have stews to spice and brews to boil.
                 * Too many things to complete to be helping out a stranger.<BR><BR>
                 * 
                 * Besides, it looks as if you've already got yourself a quest that needs
                 * doing.  Perhaps if you finish the task you're on, you can return to me
                 * and I'll help you out.  But until then, leave an old witch alone to her
                 * magics!  Shoo!  Away with ye!<BR><BR>
                 * 
                 * <I>The witch rushes you off with a wave of her decrepit hand and returns
                 * to tending the noxious brew boiling in her cauldron.</I>
                 */
                return 1055000;
            }
        }

        public override bool Logged { get { return false; } }

        public DontOfferConversation()
        {
        }
    }

    public class AcceptConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>Somewhat out of character for the vile old witch, she actually seems
                 * delighted that you've accepted her offer.</I><BR><BR>
                 * 
                 * Ah! That's the spirit! You're not a useless bag of bones after all, are ye?
                 * Well then, best get your hind quarters in gear and head towards the road!
                 * Remember, my young Apprentice could be anywhere along the road heading towards
                 * the Yew Graveyard, so be sure to run the whole course of it, and stay
                 * on track!<BR><BR>
                 * 
                 * And for Gashnak's sake, come back here when you've found something! And remember,
                 * I don't have all day!  And watch out for the imp Zeefzorpul!  And don't return
                 * empty handed!  And pack a warm sweater!  And don't trample my lawn on the
                 * way out!<BR><BR>
                 * 
                 * What are you still doing here?  Get to it!  Shoo!
                 */
                return 1055002;
            }
        }

        public AcceptConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindApprenticeObjective(true));
        }
    }

    public class HagDuringCorpseSearchConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The wrinkled hag looks up at you with venom in her eyes.</I><BR><BR>
                 * 
                 * What're you doing back here?  I thought I told you to go find my lost
                 * Apprentice!  I don't have time for your laziness, you wretched little worm!
                 * Shoo! Away with ye! And don't come back until you've found out what's
                 * happened to my Apprentice!
                 */
                return 1055003;
            }
        }

        public override bool Logged { get { return false; } }

        public HagDuringCorpseSearchConversation()
        {
        }
    }

    public class ApprenticeCorpseConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You inspect the charred and bloodied corpse, recognizing it from the
                 * Hag's description as the lost Apprentice you were tasked to
                 * bring back.<BR><BR>
                 * 
                 * It appears as if he has been scorched by fire and magic, and scratched
                 * at with vicious claws.<BR><BR>
                 * 
                 * You wonder if this horrific act is the work of the vile imp Zeefzorpul
                 * of which the Hag spoke.  You decide you'd best return to the Hag and
                 * report your findings.
                 */
                return 1055004;
            }
        }

        public ApprenticeCorpseConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindGrizeldaAboutMurderObjective());
        }
    }

    public class MurderConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The wrinkled old Hag looks up from her cauldron of boiling
                 * innards.</I><BR><BR>
                 * 
                 * Bah!  Back already?  Can't you see I'm busy with my cooking?  You
                 * wouldn't like to have a little taste of my delicious dragon gizzard soup,
                 * would you?  Haw! I thought as much.<BR><BR>
                 * 
                 * Enough of this jibber-jabber then - what news of my Apprentice?<BR><BR>
                 * 
                 * What's that?  You say that horrible little imp Zeefzorpul was behind his
                 * disappearance!?  What would Zeefzorpul want with my Apprentice?  Probably
                 * just wants to make life more miserable for me than it already is.<BR><BR>
                 * 
                 * Wait! Bah! That must be it! Zeefzorpul must have found out that I sent
                 * my Apprentices out with various Magic Brew Recipes - lists of tasks and
                 * ingredients that needed completing.<BR><BR>
                 * 
                 * That despicable Zeefzorpul knows I need the list of ingredients I gave to
                 * that Apprentice.  I've recipes to mix, stews to boil, magics to cast, and
                 * fortunes to meddle!  I won't let that wretched felchscum spoil my day.
                 * You then, I need you to go find Zeefzorpul and get that scrap of
                 * parchment back!<BR><BR>
                 * 
                 * I'm not sure where he bides his time, but I'm sure if you go find his imp
                 * friends and rough them up, they'll squeal on him in no time!  They all
                 * know each others' secret hiding places.  Go on!  Shoo! Go slay a few imps
                 * until they cough up their secrets!  No mercy for those little nasties!
                 */
                return 1055005;
            }
        }

        public MurderConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new KillImpsObjective(true));
        }
    }

    public class HagDuringImpSearchConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The sickly old hag looks up from her boiling cauldron.</I><BR><BR>
                 * 
                 * Have you found that vile little Zeefzorpul yet?  What!?  You've come
                 * back here without finding out where Zeefzorpul is, and what he's done
                 * with my Magic Brew Recipe?<BR><BR>
                 * 
                 * I told you what needs to be done, you little whelp!  Now away with ye!
                 * And don't you return until you've found my list of ingredients!
                 */
                return 1055006;
            }
        }

        public override bool Logged { get { return false; } }

        public HagDuringImpSearchConversation()
        {
        }
    }

    public class ImpDeathConversation : QuestConversation
    {
        private Point3D m_ImpLocation;

        public override object Message
        {
            get
            {
                /* <I>The wretched imp cries out for mercy.</I><BR><BR>
                 * 
                 * Forgive me! You master! You great warrior, great hooman, great greatest!
                 * Forgive! Forgive! I give up Zeef! He no good any way!  He always smack me
                 * head and hurt me good!  He say I ugly too, even with me pretty teef!<BR><BR>
                 * 
                 * But I knows where he hide!  I follow him flapping to his hidey hole.
                 * He think he so smart but he so wrong!  I make scribble drawing of where he
                 * like to hide!  But you need the whistle blower to make him come!  He no come
                 * without it!  Make with the whistle at his hidey place, and Zeef must come,
                 * he cannot resist!<BR><BR>
                 * 
                 * <I>The frightened imp hands you a crumpled map and a strange flute.</I><BR><BR>
                 * 
                 * You go to where the picture shows and then you play that whistle!  Zeef come,
                 * me promise!  But you make promise that you smack Zeef head good!
                 * Pweese?<BR><BR>
                 * 
                 * <I>With this last request, the miserable little imp falls and breathes no more.</I>
                 */
                return 1055007;
            }
        }

        public ImpDeathConversation(Point3D impLocation)
        {
            m_ImpLocation = impLocation;
        }

        public ImpDeathConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindZeefzorpulObjective(m_ImpLocation));
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_ImpLocation = reader.ReadPoint3D();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((Point3D)m_ImpLocation);
        }
    }

    public class ZeefzorpulConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>In a puff of smoke that smells of brimstone, the imp Zeefzorpul
                 * appears.</I><BR><BR>
                 * 
                 * Wuh-whut!? How did stupid hooman find mighty Zeefzorpul?  This crazy
                 * many times!  This crazy not possible! This big crazy with crazy on top!
                 * But it happening!  How can it be true!?<BR><BR>
                 * 
                 * GAH! Even mighty Zeefzorpul can no resist that crazy music!  Mighty
                 * Zeefzorpul do what you want!  Have you stupid paper back!  Mighty Zeefzorpul
                 * no want it any way.  It dumb.  It super dumb.  Big dumb like stupid dumb
                 * tree with dumb things on it!  So stupid!  So dumb that mighty Zeefzorpul
                 * not even care!  You see me not caring?  You better cause it certainly
                 * happening!  Me not caring one bit!<BR><BR>
                 * 
                 * <I>The strange little imp tosses the piece of parchment at you.  Much
                 * to your surprise, however, he swoops down in a flash of flapping wings
                 * and steals the Magic Flute from your grasp.</I><BR><BR>
                 * 
                 * Hah! So stupid like a hooman!  Mighty Zeefzorpul has defeated stupid
                 * hooman and is greatest ever imp in world!  You serious stupid, mister
                 * hooman.  Big stupid with stupid on top.  Now you no can make trick on me
                 * again with crazy dance music!  Mighty Zeefzorpul fly away to his other
                 * secret home where you never find him again!<BR><BR>
                 * 
                 * Me hope you get eated by a troll!<BR><BR>
                 * 
                 * <I>With that, the imp Zeefzorpul disappears in another puff of rancid smoke.</I>
                 */
                return 1055008;
            }
        }

        public ZeefzorpulConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new ReturnRecipeObjective());
        }
    }

    public class RecipeConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The wart-covered witch looks up from pouring fetid scraps of meat
                 * into her cauldron.</I><BR><BR>
                 * 
                 * You've dealt with that troublesome imp Zeefzorpul?  Good for you, little
                 * one!  You're not as useless as you appear, even to a daft old wench such
                 * as myself!<BR><BR>
                 * 
                 * Now then, I see you've recovered my precious Magic Brew Recipe.  I suppose
                 * you expect a reward?  Well, you can go on expecting, and I can go on being
                 * ugly.  What good is it to me that I have the list, if I don't have an
                 * apprentice to go gather the ingredients and perform the tasks
                 * themselves!<BR><BR>
                 * 
                 * If you want your precious little reward, you'll have to complete the task
                 * I gave to my previous Apprentice.  Now away with you!  Shoo! Shimmy! Skedattle!
                 * I've heads to boil and stews to spice!  Don't you return until you've completed
                 * every item on that list!
                 */
                return 1055009;
            }
        }

        public RecipeConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindIngredientObjective(new Ingredient[0]));
        }
    }

    public class HagDuringIngredientsConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The ancient crone looks up from her bubbling brew, staring you down
                 * with her one good eye.</I><BR><BR>
                 * 
                 * You've returned already have you?  And what of your task?  Have you gathered
                 * all the needed ingredients?<BR><BR>
                 * 
                 * What's that!?  You still haven't finished the simple little task I've set before
                 * you?  Then why come back here and bother me?  I can't get a single brew
                 * concocted if you keep bugging me with your whimpering little diatribes!  Why,
                 * you're worse than my last apprentice - and he was the very king of fools!<BR><BR>
                 * 
                 * Go on with ye!  Away and begone!  I don't want to see hide nor hair of your
                 * whining little face until you've gathered each and every last one of the ingredients
                 * on that list!<BR><BR>
                 * 
                 * <I>With a disgusting hacking noise, the vile witch spits upon the ground and
                 * brushes you off with a wave of her wrinkled old hand.</I>
                 */
                return 1055012;
            }
        }

        public override bool Logged { get { return false; } }

        public HagDuringIngredientsConversation()
        {
        }
    }

    public class BlackheartFirstConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The bawdy old pirate captain looks up from his bottle of Wild Harpy
                 * whiskey, as drunk as any man you've ever seen.<BR><BR>
                 * 
                 * With an excruciatingly slow movement, he pushes back his tricorne hat
                 * and stares you down with red-rimmed eyes.</I><BR><BR>
                 * 
                 * Whut tha blazes do ye want, landlubber?  Some've Captain Blackheart's
                 * fine Whiskey?  Well ye can drown in the seven seas, ya barnacle-covered
                 * bilge rat!<BR><BR>
                 * 
                 * I've cut down pasty-faced runts like yerself for lesser insults!  I've
                 * sailed the seas've this world fer fifty years, and never seen a more
                 * milk-soaked pansy lass than ye come in here for a favor.  Give ye some
                 * of my special Whiskey?  I'd sooner wrestle a sea serpent naked - and I've
                 * done that some twenty times!<BR><BR>
                 * 
                 * Ye see, ol' Captain Blackheart's Whiskey is only for pirate folk.  And ye
                 * don't look like no pirate I've ever seen.  Ye have te have the right cut
                 * of cloth and the right amount of liquor in yer belly te sail on my crew!
                 * And without that, ye might as well go home and cry to yer mommy.  Cause
                 * ye ain't ever gonna share no drink with me!<BR><BR>
                 * 
                 * Now off with ye!<BR><BR>
                 * 
                 * <I>With that, Captain Blackheart goes back to singing his bawdy songs
                 * and drinking his whiskey.  It seems as if you'll have to find some way to
                 * change his mind about your worthiness.</I>
                 */
                return 1055010;
            }
        }

        public BlackheartFirstConversation()
        {
        }

        public override void OnRead()
        {
            FindIngredientObjective obj = System.FindObjective(typeof(FindIngredientObjective)) as FindIngredientObjective;

            if (obj != null)
                System.AddObjective(new FindIngredientObjective(obj.Ingredients, true));
        }
    }

    public class BlackheartNoPirateConversation : QuestConversation
    {
        private bool m_Tricorne;
        private bool m_Drunken;

        public override object Message
        {
            get
            {
                if (m_Tricorne)
                {
                    if (m_Drunken)
                    {
                        /* <I>The filthy Captain flashes a pleased grin at you as he looks you up
                         * and down.</I><BR><BR>Well that's more like it, me little deck swabber!
                         * Ye almost look like ye fit in around here, ready te sail the great seas
                         * of Britannia, sinking boats and slaying sea serpents!<BR><BR>
                         * 
                         * But can ye truly handle yerself?  Ye might think ye can test me meddle
                         * with a sip or two of yer dandy wine, but a real pirate walks the decks
                         * with a belly full of it.  Lookit that, yer not even wobblin'!<BR><BR>
                         * 
                         * Ye've impressed me a bit, ye wee tyke, but it'll take more'n that te
                         * join me crew!<BR><BR><I>Captain Blackheart tips his mug in your direction,
                         * offering up a jolly laugh, but it seems you still haven't impressed him
                         * enough.</I>
                         */
                        return 1055059;
                    }
                    else
                    {
                        /* <I>Captain Blackheart looks up from polishing his cutlass, glaring at
                         * you with red-rimmed eyes.</I><BR><BR>
                         * 
                         * Well, well.  Lookit the wee little deck swabby.  Aren't ye a cute lil'
                         * lassy?  Don't ye look just fancy?  Ye think yer ready te join me pirate
                         * crew?  Ye think I should offer ye some've me special Blackheart brew?<BR><BR>
                         * 
                         * I'll make ye walk the plank, I will!  We'll see how sweet n' darlin' ye
                         * look when the sea serpents get at ye and rip ye te threads!  Won't that be
                         * a pretty picture, eh?<BR><BR>
                         * 
                         * Ye don't have the stomach fer the pirate life, that's plain enough te me.  Ye
                         * prance around here like a wee lil' princess, ye do.  If ye want to join my
                         * crew ye can't just look tha part - ye have to have the stomach fer it, filled
                         * up with rotgut until ye can't see straight.  I don't drink with just any ol'
                         * landlubber!  Ye'd best prove yer mettle before ye talk te me again!<BR><BR>
                         * 
                         * <I>The drunken pirate captain leans back in his chair, taking another gulp of
                         * his drink before he starts in on another bawdy pirate song.</I>
                         */
                        return 1055057;
                    }
                }
                else
                {
                    if (m_Drunken)
                    {
                        /* <I>The inebriated pirate looks up at you with a wry grin.</I><BR><BR>
                         * 
                         * Well hello again, me little matey.  I see ye have a belly full of rotgut
                         * in ye.  I bet ye think you're a right hero, ready te face the world.  But
                         * as I told ye before, bein' a member of my pirate crew means more'n just
                         * being able to hold yer drink.  Ye have te look the part - and frankly, me
                         * little barnacle, ye don't have the cut of cloth te fit in with the crowd I
                         * like te hang around.<BR><BR>
                         * 
                         * So scurry off, ye wee sewer rat, and don't come back round these parts all
                         * liquored up an' three sheets te tha wind, unless yer truly ready te join
                         * me pirate crew!<BR><BR>
                         * 
                         * <I>Captain Blackheart shoves you aside, banging his cutlass against the
                         * table as he calls to the waitress for another round.</I>
                         */
                        return 1055056;
                    }
                    else
                    {
                        /* <I>Captain Blackheart looks up from his drink, almost tipping over
                         * his chair as he looks you up and down.</I><BR><BR>
                         * 
                         * You again?  I thought I told ye te get lost?  Go on with ye!  Ye ain't
                         * no pirate - yer not even fit te clean the barnacles off me rear end!
                         * Don't ye come back babbling te me for any of me Blackheart Whiskey until
                         * ye look and act like a true pirate!<BR><BR>
                         * 
                         * Now shove off, sewer rat - I've got drinkin' te do!<BR><BR>
                         * 
                         * <I>The inebriated pirate bolts back another mug of ale and brushes you
                         * off with a wave of his hand.</I>
                         */
                        return 1055058;
                    }
                }
            }
        }

        public override bool Logged { get { return false; } }

        public BlackheartNoPirateConversation(bool tricorne, bool drunken)
        {
            m_Tricorne = tricorne;
            m_Drunken = drunken;
        }

        public BlackheartNoPirateConversation()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Tricorne = reader.ReadBool();
            m_Drunken = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_Tricorne);
            writer.Write((bool)m_Drunken);
        }
    }

    public class BlackheartPirateConversation : QuestConversation
    {
        private bool m_FirstMet;

        public override object Message
        {
            get
            {
                if (m_FirstMet)
                {
                    /* <I>The bawdy old pirate captain looks up from his bottle of Wild Harpy
                     * whiskey, as drunk as any man you've ever seen.</I><BR><BR>
                     * 
                     * Avast ye, ye loveable pirate!  Just in from sailin' the glorious sea?  Ye
                     * look right ready te fall down on the spot, ye do!<BR><BR>
                     * 
                     * I tell ye what, from the look've ye, ye deserve a belt of better brew than
                     * the slop ye've been drinking, and I've just the thing.<BR><BR>
                     * 
                     * I call it Captain Blackheart's Whiskey, and it'll give ye hairs on yer chest,
                     * that's for sure.  Why, a keg of this stuff once spilled on my ship, and it
                     * ate a hole right through the deck!<BR><BR>Go on, drink up, or use it to clean
                     * the rust off your cutlass - it's the best brew, either way!<BR><BR>
                     * 
                     * <I>Captain Blackheart hands you a jug of his famous Whiskey. You think it best
                     * to return it to the Hag, rather than drink any of the noxious swill.</I>
                     */
                    return 1055054;
                }
                else
                {
                    /* <I>The drunken pirate, Captain Blackheart, looks up from his bottle
                     * of whiskey with a pleased expression.</I><BR><BR>
                     * 
                     * Well looky here!  I didn't think a landlubber like yourself had the pirate
                     * blood in ye!  But look at that!  You certainly look the part now!  Sure
                     * you can still keep on your feet?  Har!<BR><BR>
                     * 
                     * Avast ye, ye loveable pirate!  Ye deserve a belt of better brew than the slop
                     * ye've been drinking, and I've just the thing.<BR><BR>
                     * 
                     * I call it Captain Blackheart's Whiskey, and it'll give ye hairs on yer chest,
                     * that's for sure.  Why, a keg of this stuff once spilled on my ship, and it ate
                     * a hole right through the deck!<BR><BR>
                     * 
                     * Go on, drink up, or use it to clean the rust off your cutlass - it's the best
                     * brew, either way!<BR><BR>
                     * 
                     * <I>Captain Blackheart hands you a jug of his famous Whiskey. You think it best
                     * to return it to the Hag, rather than drink any of the noxious swill.</I>
                     */
                    return 1055011;
                }
            }
        }

        public BlackheartPirateConversation(bool firstMet)
        {
            m_FirstMet = firstMet;
        }

        public BlackheartPirateConversation()
        {
        }

        public override void OnRead()
        {
            FindIngredientObjective obj = System.FindObjective(typeof(FindIngredientObjective)) as FindIngredientObjective;

            if (obj != null)
                obj.NextStep();
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_FirstMet = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_FirstMet);
        }
    }

    public class EndConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The horrible wretch of a witch looks up from her vile experiments
                 * and focuses her one good eye on you.</I><BR><BR>
                 * 
                 * Eh?  What's that?  You say you've gathered the ingredients for my delicious
                 * Magic Brew?<BR><BR>
                 * 
                 * Well, well, I don't know exactly what to say.  I thought for sure you'd
                 * end up dead!  Haw!  Can't blame a lady for wishing, can you?  Even if she
                 * is a bit old and wrinkled.<BR><BR>
                 * 
                 * Well, I promised you a reward for your efforts, and I never lie - leastways
                 * not to someone like you, after the great sacrifices you've made.  You know,
                 * I could use a new Apprentice, in an official capacity as it were.  I couldn't
                 * convince you to stay around and help me out some more could I?  There's always
                 * cauldrons that need cleaning, dung that needs shoveling, newts eye that
                 * needs a proper chewing, and fires that need stoking.<BR><BR>
                 * 
                 * What's that? Not interested?  Well, I suppose you have great things ahead of
                 * you and all that. Feh! Like a puckish little puke like you could ever make
                 * something of themselves in this cold old world!<BR><BR>
                 * 
                 * Nevertheless, I'll give you your blasted reward, and you'd better be happy
                 * with it because it's all you're getting.  Caused me enough trouble as it is.
                 * Here, take it, and be off with you!  It'll be a pleasure to my eye if I
                 * never have to squint to see you again!  And the stench!  Smells like you
                 * washed this very morning!  A great fancy folk you are, with your soaps and
                 * water!  Think you're so great...why, I remember when we didn't even have
                 * soap, and water was made by tiny little fairies and cost a gold piece for
                 * a thimbleful...I could tell you some stories, I could...<BR><BR>
                 * 
                 * <I>Your reward in hand, you decide to leave the old Hag to her mumblings
                 * before she realizes you're still around and puts you back to work.</I>
                 */
                return 1055013;
            }
        }

        public EndConversation()
        {
        }

        public override void OnRead()
        {
            System.Complete();
        }
    }

    public class RecentlyFinishedConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The wrinkled old crone stops stirring her noxious stew, looking up at
                 * you with an annoyed expression on her face.</I><BR><BR>
                 * 
                 * You again? Listen, you little wretch, I'm in no mood for any of your meddlesome
                 * requests. I've work to do, and no time for your whining.<BR><BR>
                 * 
                 * Come back later, and maybe I'll have something for you to do. In the meantime,
                 * get out of my sight - and don't touch anything on your way out!<BR><BR>
                 * 
                 * <I>The vile hag hacks up a gob of phlegm, spitting it on the ground before
                 * returning to her work.</I>
                 */
                return 1055064;
            }
        }

        public RecentlyFinishedConversation()
        {
        }

        public override bool Logged { get { return false; } }
    }

    #endregion

    #region Hag Ingredient Info

    public enum Ingredient
    {
        SheepLiver,
        RabbitsFoot,
        MongbatWing,
        ChickenGizzard,
        RatTail,
        FrogsLeg,
        DeerHeart,
        LizardTongue,
        SlimeOoze,
        SpiritEssence,
        SwampWater,
        RedMushrooms,
        Bones,
        StarChart,
        Whiskey
    }

    public class IngredientInfo
    {
        private static IngredientInfo[] m_Table = new IngredientInfo[]
			{
				// sheep liver
				new IngredientInfo( 1055020, 5, typeof( Sheep ) ),
				// rabbit's foot
				new IngredientInfo( 1055021, 5, typeof( Rabbit ), typeof( JackRabbit ) ),
				// mongbat wing
				new IngredientInfo( 1055022, 5, typeof( Mongbat ), typeof( GreaterMongbat ) ),
				// chicken gizzard
				new IngredientInfo( 1055023, 5, typeof( Chicken ) ),
				// rat tail
				new IngredientInfo( 1055024, 5, typeof( Rat ), typeof( GiantRat ), typeof( Sewerrat ) ),
				// frog's leg
				new IngredientInfo( 1055025, 5, typeof( BullFrog ) ),
				// deer heart
				new IngredientInfo( 1055026, 5, typeof( Hind ), typeof( GreatHart ) ),
				// lizard tongue
				new IngredientInfo( 1055027, 5, typeof( LavaLizard ), typeof( Lizardman ) ),
				// slime ooze
				new IngredientInfo( 1055028, 5, typeof( Slime ) ),
				// spirit essence
				new IngredientInfo( 1055029, 5, typeof( Ghoul ), typeof( Spectre ), typeof( Shade ), typeof( Wraith ), typeof( Bogle ) ),
				// Swamp Water
				new IngredientInfo( 1055030, 1 ),
				// Freshly Cut Red Mushrooms
				new IngredientInfo( 1055031, 1 ),
				// Bones Buried In Hallowed Ground
				new IngredientInfo( 1055032, 1 ),
				// Star Chart
				new IngredientInfo( 1055033, 1 ),
				// Captain Blackheart's Whiskey
				new IngredientInfo( 1055034, 1 )
			};

        public static IngredientInfo Get(Ingredient ingredient)
        {
            int index = (int)ingredient;

            if (index >= 0 && index < m_Table.Length)
                return m_Table[index];
            else
                return m_Table[0];
        }

        public static Ingredient RandomIngredient(Ingredient[] oldIngredients)
        {
            int length = m_Table.Length - oldIngredients.Length;
            Ingredient[] ingredients = new Ingredient[length];

            for (int i = 0, n = 0; i < m_Table.Length && n < ingredients.Length; i++)
            {
                Ingredient currIngredient = (Ingredient)i;

                bool found = false;
                for (int j = 0; !found && j < oldIngredients.Length; j++)
                {
                    if (oldIngredients[j] == currIngredient)
                        found = true;
                }

                if (!found)
                    ingredients[n++] = currIngredient;
            }

            int index = Utility.Random(ingredients.Length);

            return ingredients[index];
        }

        private int m_Name;
        private Type[] m_Creatures;
        private int m_Quantity;

        public int Name { get { return m_Name; } }
        public Type[] Creatures { get { return m_Creatures; } }
        public int Quantity { get { return m_Quantity; } }

        private IngredientInfo(int name, int quantity, params Type[] creatures)
        {
            m_Name = name;
            m_Creatures = creatures;
            m_Quantity = quantity;
        }
    }

    #endregion
}