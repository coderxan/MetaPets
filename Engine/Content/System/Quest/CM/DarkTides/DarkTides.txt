using System;

using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Necro
{
    public class DarkTidesQuest : QuestSystem
    {
        private static Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( Necro.AcceptConversation ),
				typeof( Necro.AnimateMaabusCorpseObjective ),
				typeof( Necro.BankerConversation ),
				typeof( Necro.CashBankCheckObjective ),
				typeof( Necro.FetchAbraxusScrollObjective ),
				typeof( Necro.FindBankObjective ),
				typeof( Necro.FindCallingScrollObjective ),
				typeof( Necro.FindCityOfLightObjective ),
				typeof( Necro.FindCrystalCaveObjective ),
				typeof( Necro.FindMaabusCorpseObjective ),
				typeof( Necro.FindMaabusTombObjective ),
				typeof( Necro.FindMardothAboutKronusObjective ),
				typeof( Necro.FindMardothAboutVaultObjective ),
				typeof( Necro.FindMardothEndObjective ),
				typeof( Necro.FindVaultOfSecretsObjective ),
				typeof( Necro.FindWellOfTearsObjective ),
				typeof( Necro.HorusConversation ),
				typeof( Necro.LostCallingScrollConversation ),
				typeof( Necro.MaabasConversation ),
				typeof( Necro.MardothEndConversation ),
				typeof( Necro.MardothKronusConversation ),
				typeof( Necro.MardothVaultConversation ),
				typeof( Necro.RadarConversation ),
				typeof( Necro.ReadAbraxusScrollConversation ),
				typeof( Necro.ReadAbraxusScrollObjective ),
				typeof( Necro.ReanimateMaabusConversation ),
				typeof( Necro.RetrieveAbraxusScrollObjective ),
				typeof( Necro.ReturnToCrystalCaveObjective ),
				typeof( Necro.SecondHorusConversation ),
				typeof( Necro.SpeakCavePasswordObjective ),
				typeof( Necro.UseCallingScrollObjective ),
				typeof( Necro.VaultOfSecretsConversation ),
				typeof( Necro.FindHorusAboutRewardObjective ),
				typeof( Necro.HealConversation ),
				typeof( Necro.HorusRewardConversation )
			};

        public override Type[] TypeReferenceTable { get { return m_TypeReferenceTable; } }

        public override object Name
        {
            get
            {
                // Dark Tides
                return 1060095;
            }
        }

        public override object OfferMessage
        {
            get
            {
                /* <I>An old man who looks to be 200 years old from the looks
                 * of his translucently pale and heavily wrinkled skin, turns
                 * to you and gives you a half-cocked grin that makes you
                 * feel somewhat uneasy.<BR><BR>
                 * 
                 * After a short pause, he begins to speak to you...</I><BR><BR>
                 * 
                 * Hmm. What's this?  Another budding Necromancer to join the
                 * ranks of Evil?  Here... let me take a look at you...  Ah
                 * yes...  Very Good! I sense the forces of evil are strong
                 * within you, child – but you need training so that you can
                 * learn to focus your skills against those aligned against
                 * our cause.  You are destined to become a legendary
                 * Necromancer - with the proper training, that only I can
                 * give you.<BR><BR>
                 * 
                 * <I>Mardoth pauses just long enough to give you a wide,
                 * skin-crawling grin.</I><BR><BR>
                 * 
                 * Let me introduce myself. I am Mardoth, the guildmaster of
                 * the Necromantic Brotherhood.  I have taken it upon myself
                 * to train anyone willing to learn the dark arts of Necromancy.
                 * The path of destruction, decay and obliteration is not an
                 * easy one.  Only the most evil and the most dedicated can
                 * hope to master the sinister art of death.<BR><BR>
                 * 
                 * I can lend you training and help supply you with equipment –
                 * in exchange for a few services rendered by you, of course.
                 * Nothing major, just a little death and destruction here and
                 * there - the tasks should be easy as a tasty meat pie for one
                 * as treacherous and evil as yourself.<BR><BR>
                 * 
                 * What do you say?  Do we have a deal?
                 */
                return 1060094;
            }
        }

        public override TimeSpan RestartDelay { get { return TimeSpan.MaxValue; } }
        public override bool IsTutorial { get { return true; } }

        public override int Picture { get { return 0x15B5; } }

        public DarkTidesQuest(PlayerMobile from)
            : base(from)
        {
        }

        // Serialization
        public DarkTidesQuest()
        {
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            if (from is SummonedPaladin)
                return true;

            return base.IgnoreYoungProtection(from);
        }

        public static bool HasLostCallingScroll(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return false;

            QuestSystem qs = pm.Quest;

            if (qs is DarkTidesQuest)
            {
                if (qs.IsObjectiveInProgress(typeof(FindMardothAboutKronusObjective)) || qs.IsObjectiveInProgress(typeof(FindWellOfTearsObjective)) || qs.IsObjectiveInProgress(typeof(UseCallingScrollObjective)))
                {
                    Container pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof(KronusScroll)) == null);
                }
            }

            return false;
        }
    }

    #region Questing Objectives

    public class AnimateMaabusCorpseObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Re-animate the corpse of Maabus using your <a href="?ForceTopic112">Animate Dead</a>
                 * spell and question him about the Kronus rituals.
                 */
                return 1060102;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023643, 8787 ) // spellbook
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public AnimateMaabusCorpseObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new MaabasConversation());
        }
    }

    public class FindCrystalCaveObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Take the teleporter in the corner of Maabus' tomb to
                 * the crystal cave where the calling scroll is kept.
                 */
                return 1060104;
            }
        }

        public FindCrystalCaveObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new HorusConversation());
        }
    }

    public class FindMardothAboutVaultObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Infiltrate the city of the Paladins and figure out a way into
                 * the Vault. See Mardoth for help with this objective.
                 */
                return 1060106;
            }
        }

        public FindMardothAboutVaultObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new MardothVaultConversation());
        }
    }

    public class FindMaabusTombObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Step onto the teleporter near Mardoth and follow the path
                 * of glowing runes to the tomb of Maabus.
                 */
                return 1060124;
            }
        }

        public FindMaabusTombObjective()
        {
        }

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(2024, 1240, -90), 3))
                Complete();
        }

        public override void OnComplete()
        {
            System.AddObjective(new FindMaabusCorpseObjective());
        }
    }

    public class FindMaabusCorpseObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* This is the tomb of Maabus.  Enter within and find
                 * the corpse of the ancient necromancer.
                 */
                return 1061142;
            }
        }

        public FindMaabusCorpseObjective()
        {
        }

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(2024, 1223, -90), 3))
                Complete();
        }

        public override void OnComplete()
        {
            System.AddObjective(new AnimateMaabusCorpseObjective());
        }
    }

    public class FindCityOfLightObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Use the teleporter near Mardoth to be transported
                 * to the Paladin City of Light.
                 */
                return 1060108;
            }
        }

        public FindCityOfLightObjective()
        {
        }

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(1076, 519, -90), 5))
                Complete();
        }

        public override void OnComplete()
        {
            System.AddObjective(new FindVaultOfSecretsObjective());
        }
    }

    public class FindVaultOfSecretsObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Follow the road paved with glowing runes to
                 * find the Vault of Secrets.  Be careful not
                 * to give yourself away as a Necromancer while
                 * in the city.
                 */
                return 1060109;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023676, 3679 ) // glowing rune
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public FindVaultOfSecretsObjective()
        {
        }

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(1072, 455, -90), 1))
                Complete();
        }

        public override void OnComplete()
        {
            System.AddConversation(new VaultOfSecretsConversation());
        }
    }

    public class FetchAbraxusScrollObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // <a href="?ForceTopic127">Summon your Horde Minion familiar</a> to fetch the scroll for you.
                return 1060196;
            }
        }

        public FetchAbraxusScrollObjective()
        {
        }

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(1076, 450, -84), 5))
            {
                HordeMinionFamiliar hmf = Spells.Necromancy.SummonFamiliarSpell.Table[System.From] as HordeMinionFamiliar;

                if (hmf != null && hmf.InRange(System.From, 5) && hmf.TargetLocation == null)
                {
                    System.From.SendLocalizedMessage(1060113); // You instinctively will your familiar to fetch the scroll for you.
                    hmf.TargetLocation = new Point2D(1076, 450);
                }
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new RetrieveAbraxusScrollObjective());
        }
    }

    public class RetrieveAbraxusScrollObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Double click your Horde Minion to open his pack and retrieve
                 * the Scroll of Abraxus that he looted for you.
                 */
                return 1060199;
            }
        }

        public RetrieveAbraxusScrollObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new ReadAbraxusScrollConversation());
        }
    }

    public class ReadAbraxusScrollObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Find the Crystal Cave password by reading (double click)
                 * the golden scroll entitled "Scroll of Abraxus" that you
                 * got from your familiar..
                 */
                return 1060125;
            }
        }

        public ReadAbraxusScrollObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnToCrystalCaveObjective());
        }
    }

    public class ReturnToCrystalCaveObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Now that you have the password, return to the Crystal Cave
                 * to speak with the guard there.  Use the teleporter outside
                 * of the vault to get there if necessary.
                 */
                return 1060115;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1026153, 6178 ) // teleporter
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public ReturnToCrystalCaveObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddObjective(new SpeakCavePasswordObjective());
        }
    }

    public class SpeakCavePasswordObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Speak the secret word that you read in the scroll
                 * stolen from the Vault to Horus the guard, using
                 * his <a href="?ForceTopic90">context menu</a>.
                 */
                return 1060117;
            }
        }

        public SpeakCavePasswordObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new SecondHorusConversation());
        }
    }

    public class FindCallingScrollObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Enter the Crystal Cave and find the Scroll of Calling.
                 * The barrier will now allow you to pass.
                 */
                return 1060119;
            }
        }

        private int m_SkitteringHoppersKilled;
        private bool m_HealConversationShown;
        private bool m_SkitteringHoppersDisposed;

        public FindCallingScrollObjective()
        {
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            return !m_SkitteringHoppersDisposed && from is SkitteringHopper;
        }

        public override bool GetKillEvent(BaseCreature creature, Container corpse)
        {
            return !m_SkitteringHoppersDisposed;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if (creature is SkitteringHopper)
            {
                if (!m_HealConversationShown)
                {
                    m_HealConversationShown = true;
                    System.AddConversation(new HealConversation());
                }

                if (++m_SkitteringHoppersKilled >= 5)
                {
                    m_SkitteringHoppersDisposed = true;
                    System.AddObjective(new FindHorusAboutRewardObjective());
                }
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new FindMardothAboutKronusObjective());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_SkitteringHoppersKilled = reader.ReadEncodedInt();
            m_HealConversationShown = reader.ReadBool();
            m_SkitteringHoppersDisposed = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.WriteEncodedInt((int)m_SkitteringHoppersKilled);
            writer.Write((bool)m_HealConversationShown);
            writer.Write((bool)m_SkitteringHoppersDisposed);
        }
    }

    public class FindHorusAboutRewardObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have disposed of the creatures as Horus has asked.
                 * See him on your way out of the Crystal Cave to claim your reward.
                 */
                return 1060126;
            }
        }

        public FindHorusAboutRewardObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new HorusRewardConversation());
        }
    }

    public class FindMardothAboutKronusObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have obtained the scroll of calling. See Mardoth
                 * for further instructions.
                 */
                return 1060127;
            }
        }

        public FindMardothAboutKronusObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new MardothKronusConversation());
        }
    }

    public class FindWellOfTearsObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Follow the red lanterns to the Well of Tears where
                 * you will perform the calling of Kronus.
                 */
                return 1060128;
            }
        }

        public FindWellOfTearsObjective()
        {
        }

        private static readonly Rectangle2D m_WellOfTearsArea = new Rectangle2D(2080, 1346, 10, 10);

        private bool m_Inside;

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && m_WellOfTearsArea.Contains(System.From.Location))
            {
                if (DarkTidesQuest.HasLostCallingScroll(System.From))
                {
                    if (!m_Inside)
                        System.AddConversation(new LostCallingScrollConversation(false));
                }
                else
                {
                    Complete();
                }

                m_Inside = true;
            }
            else
            {
                m_Inside = false;
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new UseCallingScrollObjective());
        }
    }

    public class UseCallingScrollObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Use the Scroll of Calling (double click) near the
                 * Well of Tears to charge the waters for the arrival
                 * of Kronus.
                 */
                return 1060130;
            }
        }

        public UseCallingScrollObjective()
        {
        }
    }

    public class FindMardothEndObjective : QuestObjective
    {
        private bool m_Victory;

        public override object Message
        {
            get
            {
                if (m_Victory)
                {
                    /* Victory! You have done as Mardoth has asked of you.
                     * Take as much of your foe's loot as you can carry
                     * and return to Mardoth for your reward.
                     */
                    return 1060131;
                }
                else
                {
                    /* Although you were slain by the cowardly paladin,
                     * you managed to complete the rite of calling as
                     * instructed. Return to Mardoth.
                     */
                    return 1060132;
                }
            }
        }

        public FindMardothEndObjective(bool victory)
        {
            m_Victory = victory;
        }

        // Serialization
        public FindMardothEndObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new MardothEndConversation());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Victory = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_Victory);
        }
    }

    public class FindBankObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Use the enchanted sextant in your pack to locate
                 * the nearest bank.  Go there and speak with the
                 * Banker.
                 */
                return 1060134;
            }
        }

        public FindBankObjective()
        {
        }

        public override void CheckProgress()
        {
            if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(2048, 1345, -84), 5))
                Complete();
        }

        public override void OnComplete()
        {
            System.AddObjective(new CashBankCheckObjective());
        }
    }

    public class CashBankCheckObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have arrived at the Bank. <a href="?ForceTopic38">Open your bank box</a>
                 * and then <a href="?ForceTopic86">cash the check</a> that Mardoth gave you.
                 */
                return 1060644;
            }
        }

        public CashBankCheckObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new BankerConversation());
        }
    }

    #endregion

    #region Mobile Conversation

    public class AcceptConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I><U>Important Quest Information</U></I><BR><BR>
                 * 
                 * During your quest, any important information that a
                 * <a href = "?ForceTopic31">NPC</a> gives you, will appear in a
                 * window such as this one.  You can review the information at
                 * any time during your quest.<BR><BR><U>Getting Help</U><BR><BR>
                 * 
                 * Some of the text you will come across during your quest, will
                 * be underlined <a href = "?ForceTopic73">links to the codex of
                 * wisdom</a>, or online help system.  You can click on the text
                 * to get detailed information on the underlined subject.  You
                 * may also access the Codex Of Wisdom by pressing "F1" or by
                 * clicking on the "?" on the toolbar at the top of your screen.<BR><BR>
                 * 
                 * <U>Context Menus</U><BR><BR>Context menus can be called up by
                 * single left-clicking (or Shift + single left-click, if you
                 * changed it) on most objects or NPCs in the world.  Nearly
                 * everything, including your own avatar will have context menus
                 * available.  Bringing up your avatar's context menu will give
                 * you options to cancel your quest and review various quest
                 * information.<BR><BR>
                 */
                return 1049092;
            }
        }

        public AcceptConversation()
        {
        }

        public override void OnRead()
        {
            Container bag = Mardoth.GetNewContainer();

            bag.DropItem(new DarkTidesHorn());

            System.From.AddToBackpack(bag);

            System.AddConversation(new ReanimateMaabusConversation());
        }
    }

    public class ReanimateMaabusConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* Excellent choice, young apprentice of evil!<BR><BR>
                 * 
                 * I will not waste our time with pleasantries.  There is much work
                 * to be done – especially in light of the recent Paladin ambushes
                 * that we have suffered.  The necromantic brotherhood is working
                 * towards the summoning of the elder daemon Kronus, who will rise
                 * from the Well of Tears to help us finally crush the Paladin forces
                 * that have plagued our lands for so long now.<BR><BR>
                 * 
                 * To summon Kronus, we must energize the Well of Tears with a series
                 * of dark rituals.  Unfortunately the rituals needed to sufficiently
                 * energize the Well of Tears have been lost to us.  Your task will be
                 * to recover one of the ritual scrolls needed for the summoning.<BR><BR>
                 *  
                 * You will need to find the corpse of the Arch Necromancer Maabus, which
                 * has been laid to rest in the tomb of elders.  We believe his spirit may
                 * have memory of where we may find the scrolls needed for the summoning.
                 * You will need to awaken him from the slumber of death, using your
                 * Animate Dead spell, of course.<BR><BR>
                 * 
                 * To reach the tomb, step onto the magical teleporter just to the
                 * <a href = "?ForceTopic13">West</a> of where I am standing.<BR><BR>
                 * 
                 * Once you have been teleported, follow the path, which will lead you to
                 * the tomb of Maabus.<BR><BR>One more thing before you go:<BR><BR>
                 * 
                 * Should you get into trouble out there or should you lose your way, do
                 * not worry. I have also given you a magical horn - a <I>Horn of Retreat</I>.
                 * Play the horn at any time to open a magical gateway that leads back to this
                 * tower.<BR><BR>
                 * 
                 * Should your horn run out of <a href = "?ForceTopic83">charges</a>, simply
                 * hand me the horn to have it recharged.<BR><BR>
                 * 
                 * Good luck friend.
                 */
                return 1060099;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1026153, 6178 ), // teleporter
				new QuestItemInfo( 1049117, 4036 ), // Horn of Retreat
				new QuestItemInfo( 1048032, 3702 )  // a bag
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public ReanimateMaabusConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindMaabusTombObjective());
        }
    }

    public class MaabasConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>Maabus emits an ear-crawling screech as his body reanimates.
                 * He turns and angrily screams at you</I>:<BR><BR>
                 * 
                 * YOU INFIDEL!  HOW DARE YOU AWAKEN MAABUS!?!<BR><BR>
                 * 
                 * <I>Maabus continues to scream at you angrily for some time.
                 * As he settles down, you explain to him the purpose of your visit.
                 * Once you explain that you are on a quest to summon the elder daemon
                 * Kronus, Maabus begins to cooperate, and begins to speak in a more
                 * reasonable tone</I>:<BR><BR>
                 * 
                 * Well, why didn’t you say so?  If you’re going to raise Kronus from
                 * the Well of Tears, you must first complete a long series of dark
                 * rituals.  I once owned one of the scrolls needed for the summoning,
                 * but alas it was lost to me when I lost my life to a cowardly Paladin
                 * ambush near the Paladin city of Light.  They would have probably
                 * hidden the scroll in their precious crystal cave near the city.<BR><BR>
                 * 
                 * There is a teleporter in the corner of this tomb.  It will transport
                 * you near the crystal cave at which I believe one of the calling scrolls
                 * is hidden.  Good luck.<BR><BR>
                 * 
                 * <I>Maabus' body slumps back into the coffinas your magic expires</I>.
                 */
                return 1060103;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1026153, 6178 ) // teleporter
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public MaabasConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindCrystalCaveObjective());
        }
    }

    public class HorusConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>An old man, dressed in slightly tattered armor, whom you recognize
                 * to be a Paladin stands before the Crystal Cave staring blankly into
                 * the space in front of him.  As he begins to speak to you, you realize
                 * this man is blind.  You attempt to persuade the blind man that you are
                 * a Paladin seeking to inspect the scroll of calling...</I><BR><BR>
                 * 
                 * Greetings traveler!<BR><BR>
                 * 
                 * You seek entrance to the Crystal Cave, home of the Calling Scroll?  Hmm.
                 * You reak of death and decay, brother.  You reak of death like a Necromancer,
                 * but yet you claim to be a Paladin in hopes that I will grant thee passage
                 * into the cave?<BR><BR>
                 * 
                 * Please don’t think ill of me for this, but I’m just a blind, old man looking
                 * to keep the brotherhood of Paladins safe from the clutches of the elder daemon
                 * Kronus.  The Necromancers have been after this particular scroll for quite some
                 * time, so we must take all the security precautions we can.<BR><BR>
                 * 
                 * Before I can let you pass into the Crystal Cave, you must speak to me the secret
                 * word that is kept in the Scroll of Abraxus in the Vault of Secrets at the Paladin
                 * city of Light.  It’s the only way that I can be sure you are who you claim to be,
                 * since Necromancers cannot enter the Vault due to powerful protective magic that
                 * the brotherhood has blessed the vault with.
                 */
                return 1060105;
            }
        }

        public HorusConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindMardothAboutVaultObjective());
        }
    }

    public class MardothVaultConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>Mardoth looks at you expectantly until you tell him that you failed
                 * to retrieve the scroll...</I><BR><BR>
                 * 
                 * You failed?  Very unfortunate...  So now you must find your way into
                 * the paladin’s Vault of Secrets, eh?  Well, you won't be able to get in
                 * – there is a powerful magic aura that protects the Vault from all
                 * Necromancers.  We simply cannot enter.  However, that's not to say you
                 * familiar spirit can't.<BR><BR>
                 * 
                 * <I>Mardoth grins with obvious satisfaction
                 * as he explains the details of the <a href="?ForceTopic127">Summon
                 * Familiar</a> spell to you...</I>, which will allow you to summon a
                 * scavenging Horde Minion to steal the scroll.<BR><BR>
                 * 
                 * Very well.  You are prepared to go.  Take the teleporter just to the
                 * <a href = "?ForceTopic13">West</a> of where I am standing to transport
                 * to the Paladin city of Light.  Once you have arrived in the city, follow
                 * the road of glowing runes to the Vault of Secrets.  You know what to do.
                 */
                return 1060107;
            }
        }

        public MardothVaultConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindCityOfLightObjective());
        }
    }

    public class VaultOfSecretsConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You have arrived in the Vault of Secrets.  You can feel the
                 * protective magic in this place restricting you, making you
                 * feel nearly claustrophobic.<BR><BR>
                 * 
                 * Just ahead of you and out of your reach, you see a collection
                 * of scrolls and books, one of them being entitled
                 * 'Scroll of Abraxus' .  You can only assume that this scroll
                 * holds the current password required to enter the Crystal Cave.<BR><BR>
                 * 
                 * This would be a good opportunity to <a href="?ForceTopic127">summon
                 * your familiar</a>.  Since your familiar is not a Necromancer, it
                 * will not be affected by the anti-magic aura that surrounds the Vault.<BR><BR>
                 * 
                 * Summon your familiar with the <a href="?ForceTopic127">Summon Familiar</a> spell.
                 */
                return 1060110;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023643, 8787 ) // spellbook
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public VaultOfSecretsConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FetchAbraxusScrollObjective());
        }
    }

    public class ReadAbraxusScrollConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You have obtained the Scroll of Abraxus, which contains the secret
                 * password needed to gain passage into the Crystal Cave where the
                 * Scroll of Calling is kept.  Read the scroll (double click) and
                 * figure out the password.<BR><BR>
                 * 
                 * Once you have the password, return to the Crystal Cave and speak
                 * the password to the guard.<BR><BR>
                 * 
                 * If you do not know the way to the Crystal Cave from the Paladin City,
                 * you can use the magic teleporter located just outside of the vault.
                 */
                return 1060114;
            }
        }

        public ReadAbraxusScrollConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new ReadAbraxusScrollObjective());
        }
    }

    public class SecondHorusConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* Very well Paladin, you have proven to me your identity.
                 * I grant thee passage.<BR><BR>
                 * 
                 * Be careful, however – I’ve heard that the cave has been
                 * infested with a vermin of some sort.  Our High Lord
                 * Melkeer was supposed to send some troops to clear the
                 * vermin out of the cave, but that was last week already.
                 * I fear that he forgot.<BR><BR>
                 * 
                 * If you can find it in your goodness to dispose of at
                 * least 5 of those vermin in there, I shall reward your
                 * efforts.  If however you are too busy, and I would
                 * understand if you were, don’t bother with the vermin.<BR><BR>
                 * 
                 * You may now pass through the energy barrier to enter the
                 * Crystal Cave.   Take care honorable Paladin soul.
                 * Walk in the light my friend.
                 */
                return 1060118;
            }
        }

        public SecondHorusConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindCallingScrollObjective());
        }
    }

    public class HealConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You've just slain a creature.  Now is a good time to learn how
                 * to heal yourself as a Necromancer.<BR><BR>
                 * 
                 * As a follower of the dark path, you are able to recover lost
                 * hitpoints by communing with the spirit world via the skill
                 * <a href="?ForceTopic133">Spirit Speak</a>.  Learn more about it now,
                 * <a href="?ForceTopic73">in the codex of Wisdom</a>.
                 */
                return 1061610;
            }
        }

        public HealConversation()
        {
        }
    }

    public class HorusRewardConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* I thank you for going out of your way to clean out some
                 * of the vermin in that cave – here is your reward: a bag
                 * containing 500 gold coins plus a strange and magical artifact
                 * that should come in handy in your travels.<BR><BR>
                 * 
                 * Take care young Paladin!
                 */
                return 1060717;
            }
        }

        public override bool Logged { get { return false; } }

        public HorusRewardConversation()
        {
        }
    }

    public class LostCallingScrollConversation : QuestConversation
    {
        private bool m_FromMardoth;

        public override object Message
        {
            get
            {
                if (m_FromMardoth)
                {
                    /* You return without the scroll of Calling?  I'm afraid that
                     * won't do.  You must return to the Crystal Cave and fetch
                     * another scroll.  Use the teleporter to the West of me to
                     * get there.  Return here when you have the scroll.  Do not 
                     * fail me this time, young apprentice of evil.
                     */
                    return 1062058;
                }
                else // from well of tears
                {
                    /* You have arrived at the well, but no longer have the scroll
                     * of calling.  Use Mardoth's teleporter to return to the
                     * Crystal Cave and fetch another scroll from the box.
                     */
                    return 1060129;
                }
            }
        }

        public override bool Logged { get { return false; } }

        public LostCallingScrollConversation(bool fromMardoth)
        {
            m_FromMardoth = fromMardoth;
        }

        // Serialization
        public LostCallingScrollConversation()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_FromMardoth = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_FromMardoth);
        }
    }

    public class MardothKronusConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You have returned with the scroll!  I knew I could count on you.
                 * You can now perform the rite of calling at the Well of Tears.
                 * This ritual will help charge the Well to prepare for the coming
                 * of Kronus.   You are prepared to do your part young Necromancer!<BR><BR>
                 * 
                 * Just outside of this tower, you will find a path lined with red
                 * lanterns.  Follow this path to get to the Well of Tears.  Once
                 * you have arrived at the Well, use the scroll to perform the
                 * ritual of calling.  Performing the rite will empower the well
                 * and bring us that much closer to the arrival of Kronus.<BR><BR>
                 * 
                 * Once you have completed the ritual, return here for your
                 * promised reward.
                 */
                return 1060121;
            }
        }

        public MardothKronusConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindWellOfTearsObjective());
        }
    }

    public class MardothEndConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You have done as I asked... I knew I could count on you from
                 * the moment you walked in here!<BR><BR>
                 * 
                 * The forces of evil are strong within you.  You will become
                 * a great Necromancer in this life - perhaps even the greatest.<BR><BR>
                 * 
                 * My work for you is done here.  I release you from my service
                 * to go into the world and fight for our cause...<BR><BR>
                 * 
                 * Oh...I almost forgot - your reward.  Here is a magical
                 * weapon and 2000 gold for you, in the form of a check. Don't
                 * spend it all in one place though, eh?<BR><BR>
                 * 
                 * Actually, before you can spend any of it at all, you will
                 * have to <a href="?ForceTopic86">cash the check</a> at the
                 * nearest bank.  Shopkeepers never accept checks for payment,
                 * they require cash.<BR><BR>
                 * 
                 * In your pack, you will find an enchanted sextant.  Use this
                 * sextant to guide you to the nearest bank.<BR><BR>
                 * 
                 * Farewell, and stay true to the ways of the shadow...
                 */
                return 1060133;
            }
        }

        public MardothEndConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindBankObjective());
        }
    }

    public class BankerConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The banker smiles at you and greets you in a loud and robust voice...</I><BR><BR>
                 * 
                 * Well hello there adventurer! I see you've learned how to cash checks. Wonderful!
                 * Let me tell you a bit about the banks in this world...<BR><BR>
                 * 
                 * Anything that you place into any bank box, can be retrieved from any other
                 * bank box in the land. For instance, if you place an item into a bank box in
                 * Britain, it can be retrieved from your bank box in Moonglow or any other city.<BR><BR>
                 * 
                 * Bank boxes are very secure. So secure, in fact, that no one can ever get into
                 * your bank box except for yourself. Security is hard to come by these days,
                 * but you can trust in the banking system of Britannia! We shall not let you down!<BR><BR>
                 * 
                 * I hope to be seeing much more of you as your riches grow! May your bank box overflow
                 * with the spoils of your adventures.<BR><BR>Farewell adventurer, you are now free to
                 * explore the world on your own.
                 */
                return 1060137;
            }
        }

        public BankerConversation()
        {
        }

        public override void OnRead()
        {
            System.Complete();
        }
    }

    public class RadarConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* If you are leaving the tower, you should learn about the Radar Map.<BR><BR>
                 * 
                 * The Radar Map (or Overhead View) can be opened by pressing 'ALT-R'
                 * on your keyboard. It shows your immediate surroundings from a bird's
                 * eye view.<BR><BR>Pressing ALT-R twice, will enlarge the Radar Map a
                 * little. Use the Radar Map often as you travel throughout the world
                 * to familiarize yourself with your surroundings.
                 */
                return 1061692;
            }
        }

        public override bool Logged { get { return false; } }

        public RadarConversation()
        {
        }
    }

    #endregion
}