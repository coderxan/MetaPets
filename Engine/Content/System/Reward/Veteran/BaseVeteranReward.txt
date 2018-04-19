using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Accounting;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Engines.VeteranRewards
{
    public class RewardSystem
    {
        private static RewardCategory[] m_Categories;
        private static RewardList[] m_Lists;

        public static RewardCategory[] Categories
        {
            get
            {
                if (m_Categories == null)
                    SetupRewardTables();

                return m_Categories;
            }
        }

        public static RewardList[] Lists
        {
            get
            {
                if (m_Lists == null)
                    SetupRewardTables();

                return m_Lists;
            }
        }

        public static bool Enabled = true; // change to true to enable vet rewards
        public static bool SkillCapRewards = true; // assuming vet rewards are enabled, should total skill cap bonuses be awarded? (720 skills total at 4th level)
        public static TimeSpan RewardInterval = TimeSpan.FromDays(30.0);

        public static bool HasAccess(Mobile mob, RewardCategory category)
        {
            List<RewardEntry> entries = category.Entries;

            for (int j = 0; j < entries.Count; ++j)
            {
                //RewardEntry entry = entries[j];
                if (RewardSystem.HasAccess(mob, entries[j]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasAccess(Mobile mob, RewardEntry entry)
        {
            if (Core.Expansion < entry.RequiredExpansion)
            {
                return false;
            }

            TimeSpan ts;
            return HasAccess(mob, entry.List, out ts);
        }

        public static bool HasAccess(Mobile mob, RewardList list, out TimeSpan ts)
        {
            if (list == null)
            {
                ts = TimeSpan.Zero;
                return false;
            }

            Account acct = mob.Account as Account;

            if (acct == null)
            {
                ts = TimeSpan.Zero;
                return false;
            }

            TimeSpan totalTime = (DateTime.UtcNow - acct.Created);

            ts = (list.Age - totalTime);

            if (ts <= TimeSpan.Zero)
                return true;

            return false;
        }

        public static int GetRewardLevel(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
                return 0;

            return GetRewardLevel(acct);
        }

        public static int GetRewardLevel(Account acct)
        {
            TimeSpan totalTime = (DateTime.UtcNow - acct.Created);

            int level = (int)(totalTime.TotalDays / RewardInterval.TotalDays);

            if (level < 0)
                level = 0;

            return level;
        }

        public static bool HasHalfLevel(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
                return false;

            return HasHalfLevel(acct);
        }

        public static bool HasHalfLevel(Account acct)
        {
            TimeSpan totalTime = (DateTime.UtcNow - acct.Created);

            Double level = (totalTime.TotalDays / RewardInterval.TotalDays);

            return level >= 0.5;
        }

        public static bool ConsumeRewardPoint(Mobile mob)
        {
            int cur, max;

            ComputeRewardInfo(mob, out cur, out max);

            if (cur >= max)
                return false;

            Account acct = mob.Account as Account;

            if (acct == null)
                return false;

            //if ( mob.AccessLevel < AccessLevel.GameMaster )
            acct.SetTag("numRewardsChosen", (cur + 1).ToString());

            return true;
        }

        public static void ComputeRewardInfo(Mobile mob, out int cur, out int max)
        {
            int level;

            ComputeRewardInfo(mob, out cur, out max, out level);
        }

        public static void ComputeRewardInfo(Mobile mob, out int cur, out int max, out int level)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
            {
                cur = max = level = 0;
                return;
            }

            level = GetRewardLevel(acct);

            if (level == 0)
            {
                cur = max = 0;
                return;
            }

            string tag = acct.GetTag("numRewardsChosen");

            if (String.IsNullOrEmpty(tag))
                cur = 0;
            else
                cur = Utility.ToInt32(tag);

            if (level >= 6)
                max = 9 + ((level - 6) * 2);
            else
                max = 2 + level;
        }

        public static bool CheckIsUsableBy(Mobile from, Item item, object[] args)
        {
            if (m_Lists == null)
                SetupRewardTables();

            bool isRelaxedRules = (item is DyeTub || item is MonsterStatuette);

            Type type = item.GetType();

            for (int i = 0; i < m_Lists.Length; ++i)
            {
                RewardList list = m_Lists[i];
                RewardEntry[] entries = list.Entries;
                TimeSpan ts;

                for (int j = 0; j < entries.Length; ++j)
                {
                    if (entries[j].ItemType == type)
                    {
                        if (args == null && entries[j].Args.Length == 0)
                        {
                            if ((!isRelaxedRules || i > 0) && !HasAccess(from, list, out ts))
                            {
                                from.SendLocalizedMessage(1008126, true, Math.Ceiling(ts.TotalDays / 30.0).ToString()); // Your account is not old enough to use this item. Months until you can use this item : 
                                return false;
                            }

                            return true;
                        }

                        if (args.Length == entries[j].Args.Length)
                        {
                            bool match = true;

                            for (int k = 0; match && k < args.Length; ++k)
                                match = (args[k].Equals(entries[j].Args[k]));

                            if (match)
                            {
                                if ((!isRelaxedRules || i > 0) && !HasAccess(from, list, out ts))
                                {
                                    from.SendLocalizedMessage(1008126, true, Math.Ceiling(ts.TotalDays / 30.0).ToString()); // Your account is not old enough to use this item. Months until you can use this item : 
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            // no entry?
            return true;
        }

        public static int GetRewardYearLabel(Item item, object[] args)
        {
            int level = GetRewardYear(item, args);

            return 1076216 + ((level < 10) ? level : (level < 12) ? ((level - 9) + 4240) : ((level - 11) + 37585));
        }

        public static int GetRewardYear(Item item, object[] args)
        {
            if (m_Lists == null)
                SetupRewardTables();

            Type type = item.GetType();

            for (int i = 0; i < m_Lists.Length; ++i)
            {
                RewardList list = m_Lists[i];
                RewardEntry[] entries = list.Entries;

                for (int j = 0; j < entries.Length; ++j)
                {
                    if (entries[j].ItemType == type)
                    {
                        if (args == null && entries[j].Args.Length == 0)
                            return i + 1;

                        if (args.Length == entries[j].Args.Length)
                        {
                            bool match = true;

                            for (int k = 0; match && k < args.Length; ++k)
                                match = (args[k].Equals(entries[j].Args[k]));

                            if (match)
                                return i + 1;
                        }
                    }
                }
            }

            // no entry?
            return 0;
        }

        public static void SetupRewardTables()
        {
            RewardCategory monsterStatues = new RewardCategory(1049750);
            RewardCategory cloaksAndRobes = new RewardCategory(1049752);
            RewardCategory etherealSteeds = new RewardCategory(1049751);
            RewardCategory specialDyeTubs = new RewardCategory(1049753);
            RewardCategory houseAddOns = new RewardCategory(1049754);
            RewardCategory miscellaneous = new RewardCategory(1078596);

            m_Categories = new RewardCategory[]
				{
					monsterStatues,
					cloaksAndRobes,
					etherealSteeds,
					specialDyeTubs,
					houseAddOns,
					miscellaneous
				};

            const int Bronze = 0x972;
            const int Copper = 0x96D;
            const int Golden = 0x8A5;
            const int Agapite = 0x979;
            const int Verite = 0x89F;
            const int Valorite = 0x8AB;
            const int IceGreen = 0x47F;
            const int IceBlue = 0x482;
            const int DarkGray = 0x497;
            const int Fire = 0x489;
            const int IceWhite = 0x47E;
            const int JetBlack = 0x001;
            const int Pink = 0x490;
            const int Crimson = 0x485;

            m_Lists = new RewardList[]
			{
					new RewardList( RewardInterval, 1, new RewardEntry[]
					{
						new RewardEntry( specialDyeTubs, 1006008, typeof( RewardBlackDyeTub ) ),
						new RewardEntry( specialDyeTubs, 1006013, typeof( FurnitureDyeTub ) ),
						new RewardEntry( specialDyeTubs, 1006047, typeof( SpecialDyeTub ) ),
						new RewardEntry( cloaksAndRobes, 1006009, typeof( RewardCloak ), Bronze, 1041286 ),
						new RewardEntry( cloaksAndRobes, 1006010, typeof( RewardRobe ), Bronze, 1041287 ),
						new RewardEntry( cloaksAndRobes, 1080366, typeof( RewardDress ), Expansion.ML, Bronze, 1080366 ),
						new RewardEntry( cloaksAndRobes, 1006011, typeof( RewardCloak ), Copper, 1041288 ),
						new RewardEntry( cloaksAndRobes, 1006012, typeof( RewardRobe ), Copper, 1041289 ),
						new RewardEntry( cloaksAndRobes, 1080367, typeof( RewardDress ), Expansion.ML, Copper, 1080367 ),
						new RewardEntry( monsterStatues, 1006024, typeof( MonsterStatuette ), MonsterStatuetteType.Crocodile ),
						new RewardEntry( monsterStatues, 1006025, typeof( MonsterStatuette ), MonsterStatuetteType.Daemon ),
						new RewardEntry( monsterStatues, 1006026, typeof( MonsterStatuette ), MonsterStatuetteType.Dragon ),
						new RewardEntry( monsterStatues, 1006027, typeof( MonsterStatuette ), MonsterStatuetteType.EarthElemental ),
						new RewardEntry( monsterStatues, 1006028, typeof( MonsterStatuette ), MonsterStatuetteType.Ettin ),
						new RewardEntry( monsterStatues, 1006029, typeof( MonsterStatuette ), MonsterStatuetteType.Gargoyle ),
						new RewardEntry( monsterStatues, 1006030, typeof( MonsterStatuette ), MonsterStatuetteType.Gorilla ),
						new RewardEntry( monsterStatues, 1006031, typeof( MonsterStatuette ), MonsterStatuetteType.Lich ),
						new RewardEntry( monsterStatues, 1006032, typeof( MonsterStatuette ), MonsterStatuetteType.Lizardman ),
						new RewardEntry( monsterStatues, 1006033, typeof( MonsterStatuette ), MonsterStatuetteType.Ogre ),
						new RewardEntry( monsterStatues, 1006034, typeof( MonsterStatuette ), MonsterStatuetteType.Orc ),
						new RewardEntry( monsterStatues, 1006035, typeof( MonsterStatuette ), MonsterStatuetteType.Ratman ),
						new RewardEntry( monsterStatues, 1006036, typeof( MonsterStatuette ), MonsterStatuetteType.Skeleton ),
						new RewardEntry( monsterStatues, 1006037, typeof( MonsterStatuette ), MonsterStatuetteType.Troll ),
						new RewardEntry( houseAddOns,    1062692, typeof( ContestMiniHouseDeed ), Expansion.AOS, MiniHouseType.MalasMountainPass ),
						new RewardEntry( houseAddOns,    1072216, typeof( ContestMiniHouseDeed ), Expansion.SE, MiniHouseType.ChurchAtNight ),
						new RewardEntry( miscellaneous,  1076155, typeof( RedSoulstone ), Expansion.ML ),
						new RewardEntry( miscellaneous,  1080523, typeof( CommodityDeedBox ), Expansion.ML ),
					} ),
					new RewardList( RewardInterval, 2, new RewardEntry[]
					{
						new RewardEntry( specialDyeTubs, 1006052, typeof( LeatherDyeTub ) ),
						new RewardEntry( cloaksAndRobes, 1006014, typeof( RewardCloak ), Agapite, 1041290 ),
						new RewardEntry( cloaksAndRobes, 1006015, typeof( RewardRobe ), Agapite, 1041291 ),
						new RewardEntry( cloaksAndRobes, 1080369, typeof( RewardDress ), Expansion.ML, Agapite, 1080369 ),
						new RewardEntry( cloaksAndRobes, 1006016, typeof( RewardCloak ), Golden, 1041292 ),
						new RewardEntry( cloaksAndRobes, 1006017, typeof( RewardRobe ), Golden, 1041293 ),
						new RewardEntry( cloaksAndRobes, 1080368, typeof( RewardDress ), Expansion.ML, Golden, 1080368 ),
						new RewardEntry( houseAddOns,    1006048, typeof( BannerDeed ) ),
						new RewardEntry( houseAddOns, 	 1006049, typeof( FlamingHeadDeed ) ),
						new RewardEntry( houseAddOns, 	 1080409, typeof( MinotaurStatueDeed ), Expansion.ML )
					} ),
					new RewardList( RewardInterval, 3, new RewardEntry[]
					{
						new RewardEntry( cloaksAndRobes, 1006020, typeof( RewardCloak ), Verite, 1041294 ),
						new RewardEntry( cloaksAndRobes, 1006021, typeof( RewardRobe ), Verite, 1041295 ),
						new RewardEntry( cloaksAndRobes, 1080370, typeof( RewardDress ), Expansion.ML, Verite, 1080370 ),
						new RewardEntry( cloaksAndRobes, 1006022, typeof( RewardCloak ), Valorite, 1041296 ),
						new RewardEntry( cloaksAndRobes, 1006023, typeof( RewardRobe ), Valorite, 1041297 ),
						new RewardEntry( cloaksAndRobes, 1080371, typeof( RewardDress ), Expansion.ML, Valorite, 1080371 ),
						new RewardEntry( monsterStatues, 1006038, typeof( MonsterStatuette ), MonsterStatuetteType.Cow ),
						new RewardEntry( monsterStatues, 1006039, typeof( MonsterStatuette ), MonsterStatuetteType.Zombie ),
						new RewardEntry( monsterStatues, 1006040, typeof( MonsterStatuette ), MonsterStatuetteType.Llama ),
						new RewardEntry( etherealSteeds, 1006019, typeof( EtherealHorse ) ),
						new RewardEntry( etherealSteeds, 1006050, typeof( EtherealOstard ) ),
						new RewardEntry( etherealSteeds, 1006051, typeof( EtherealLlama ) ),
						new RewardEntry( houseAddOns,	 1080407, typeof( PottedCactusDeed ), Expansion.ML )

					} ),
					new RewardList( RewardInterval, 4, new RewardEntry[]
					{
						new RewardEntry( specialDyeTubs, 1049740, typeof( RunebookDyeTub ) ),
						new RewardEntry( cloaksAndRobes, 1049725, typeof( RewardCloak ), DarkGray, 1049757 ),
						new RewardEntry( cloaksAndRobes, 1049726, typeof( RewardRobe ), DarkGray, 1049756 ),
						new RewardEntry( cloaksAndRobes, 1080374, typeof( RewardDress ), Expansion.ML, DarkGray, 1080374 ),
						new RewardEntry( cloaksAndRobes, 1049727, typeof( RewardCloak ), IceGreen, 1049759 ),
						new RewardEntry( cloaksAndRobes, 1049728, typeof( RewardRobe ), IceGreen, 1049758 ),
						new RewardEntry( cloaksAndRobes, 1080372, typeof( RewardDress ), Expansion.ML, IceGreen, 1080372 ),

						new RewardEntry( cloaksAndRobes, 1049729, typeof( RewardCloak ), IceBlue, 1049761 ),
						new RewardEntry( cloaksAndRobes, 1049730, typeof( RewardRobe ), IceBlue, 1049760 ),
						new RewardEntry( cloaksAndRobes, 1080373, typeof( RewardDress ), Expansion.ML, IceBlue, 1080373 ),
						new RewardEntry( monsterStatues, 1049742, typeof( MonsterStatuette ), MonsterStatuetteType.Ophidian ),
						new RewardEntry( monsterStatues, 1049743, typeof( MonsterStatuette ), MonsterStatuetteType.Reaper ),
						new RewardEntry( monsterStatues, 1049744, typeof( MonsterStatuette ), MonsterStatuetteType.Mongbat ),
						new RewardEntry( etherealSteeds, 1049746, typeof( EtherealKirin ) ),
						new RewardEntry( etherealSteeds, 1049745, typeof( EtherealUnicorn ) ),
						new RewardEntry( etherealSteeds, 1049747, typeof( EtherealRidgeback ) ),
						new RewardEntry( houseAddOns,    1049737, typeof( DecorativeShieldDeed ) ),
						new RewardEntry( houseAddOns, 	 1049738, typeof( HangingSkeletonDeed ) )
					} ),
					new RewardList( RewardInterval, 5, new RewardEntry[]
					{
						new RewardEntry( specialDyeTubs, 1049741, typeof( StatuetteDyeTub ) ),
						new RewardEntry( cloaksAndRobes, 1049731, typeof( RewardCloak ), JetBlack, 1049763 ),
						new RewardEntry( cloaksAndRobes, 1049732, typeof( RewardRobe ), JetBlack, 1049762 ),
						new RewardEntry( cloaksAndRobes, 1080377, typeof( RewardDress ), Expansion.ML, JetBlack, 1080377 ),
						new RewardEntry( cloaksAndRobes, 1049733, typeof( RewardCloak ), IceWhite, 1049765 ),
						new RewardEntry( cloaksAndRobes, 1049734, typeof( RewardRobe ), IceWhite, 1049764 ),
						new RewardEntry( cloaksAndRobes, 1080376, typeof( RewardDress ), Expansion.ML, IceWhite, 1080376 ),
						new RewardEntry( cloaksAndRobes, 1049735, typeof( RewardCloak ), Fire, 1049767 ),
						new RewardEntry( cloaksAndRobes, 1049736, typeof( RewardRobe ), Fire, 1049766 ),
						new RewardEntry( cloaksAndRobes, 1080375, typeof( RewardDress ), Expansion.ML, Fire, 1080375 ),
						new RewardEntry( monsterStatues, 1049768, typeof( MonsterStatuette ), MonsterStatuetteType.Gazer ),
						new RewardEntry( monsterStatues, 1049769, typeof( MonsterStatuette ), MonsterStatuetteType.FireElemental ),
						new RewardEntry( monsterStatues, 1049770, typeof( MonsterStatuette ), MonsterStatuetteType.Wolf ),
						new RewardEntry( etherealSteeds, 1049749, typeof( EtherealSwampDragon ) ),
						new RewardEntry( etherealSteeds, 1049748, typeof( EtherealBeetle ) ),
						new RewardEntry( houseAddOns,    1049739, typeof( StoneAnkhDeed ) ),
						new RewardEntry( houseAddOns,    1080384, typeof( BloodyPentagramDeed ), Expansion.ML )
					} ),					
					new RewardList( RewardInterval, 6, new RewardEntry[]
					{
						new RewardEntry( houseAddOns,	1076188, typeof( CharacterStatueMaker ), Expansion.ML, StatueType.Jade ),
						new RewardEntry( houseAddOns,	1076189, typeof( CharacterStatueMaker ), Expansion.ML, StatueType.Marble ),
						new RewardEntry( houseAddOns,	1076190, typeof( CharacterStatueMaker ), Expansion.ML, StatueType.Bronze ),						
						new RewardEntry( houseAddOns,	1080527, typeof( RewardBrazierDeed ), Expansion.ML )
					} ),		
					new RewardList( RewardInterval, 7, new RewardEntry[]
					{
						new RewardEntry( houseAddOns,	1076157, typeof( CannonDeed ), Expansion.ML ),					
						new RewardEntry( houseAddOns,	1080550, typeof( TreeStumpDeed ), Expansion.ML )
					} ),
					new RewardList( RewardInterval, 8, new RewardEntry[]
					{
						new RewardEntry( miscellaneous,	1076158, typeof( WeaponEngravingTool ), Expansion.ML )
					} ),
					new RewardList( RewardInterval, 9, new RewardEntry[]
					{
						new RewardEntry( etherealSteeds,	1076159, typeof( RideablePolarBear ), Expansion.ML ),
						new RewardEntry( houseAddOns,		1080549, typeof( WallBannerDeed ), Expansion.ML )
					} ),
					new RewardList( RewardInterval, 10, new RewardEntry[]
					{												
						new RewardEntry( monsterStatues,	1080520, typeof( MonsterStatuette ), Expansion.ML, MonsterStatuetteType.Harrower ),
						new RewardEntry( monsterStatues,	1080521, typeof( MonsterStatuette ), Expansion.ML, MonsterStatuetteType.Efreet ),

						new RewardEntry( cloaksAndRobes,	1080382, typeof( RewardCloak ), Expansion.ML, Pink, 1080382 ),
						new RewardEntry( cloaksAndRobes,	1080380, typeof( RewardRobe ), Expansion.ML, Pink, 1080380 ),						
						new RewardEntry( cloaksAndRobes,	1080378, typeof( RewardDress ), Expansion.ML, Pink, 1080378 ),
						new RewardEntry( cloaksAndRobes,	1080383, typeof( RewardCloak ), Expansion.ML, Crimson, 1080383 ),
						new RewardEntry( cloaksAndRobes,	1080381, typeof( RewardRobe ), Expansion.ML, Crimson, 1080381 ),						
						new RewardEntry( cloaksAndRobes,	1080379, typeof( RewardDress ), Expansion.ML, Crimson, 1080379 ),
						
						new RewardEntry( etherealSteeds,	1080386, typeof( EtherealCuSidhe ), Expansion.ML ),

						new RewardEntry( houseAddOns,		1080548, typeof( MiningCartDeed ), Expansion.ML ),
						new RewardEntry( houseAddOns,		1080397, typeof( AnkhOfSacrificeDeed ), Expansion.ML )
					} ),

					new RewardList( RewardInterval, 11, new RewardEntry[]
					{
						new RewardEntry( etherealSteeds,	1113908, typeof( EtherealReptalon ), Expansion.ML ),
					} ),

					new RewardList( RewardInterval, 12, new RewardEntry[]
					{
						new RewardEntry( etherealSteeds,	1113813, typeof( EtherealHiryu ), Expansion.ML ),
					} ),
			};
        }
        public static void Initialize()
        {
            if (Enabled)
                EventSink.Login += new LoginEventHandler(EventSink_Login);
        }

        private static void EventSink_Login(LoginEventArgs e)
        {
            if (!e.Mobile.Alive)
                return;

            int cur, max, level;

            ComputeRewardInfo(e.Mobile, out cur, out max, out level);

            if (e.Mobile.SkillsCap == 7000 || e.Mobile.SkillsCap == 7050 || e.Mobile.SkillsCap == 7100 || e.Mobile.SkillsCap == 7150 || e.Mobile.SkillsCap == 7200)
            {
                if (level > 4)
                    level = 4;
                else if (level < 0)
                    level = 0;

                if (SkillCapRewards)
                    e.Mobile.SkillsCap = 7000 + (level * 50);
                else
                    e.Mobile.SkillsCap = 7000;
            }

            if (Core.ML && e.Mobile is PlayerMobile && !((PlayerMobile)e.Mobile).HasStatReward && HasHalfLevel(e.Mobile))
            {
                ((PlayerMobile)e.Mobile).HasStatReward = true;
                e.Mobile.StatCap += 5;
            }

            if (cur < max)
                e.Mobile.SendGump(new RewardNoticeGump(e.Mobile));
        }
    }

    public interface IRewardItem
    {
        bool IsRewardItem { get; set; }
    }

    public class RewardNoticeGump : Gump
    {
        private Mobile m_From;

        public RewardNoticeGump(Mobile from)
            : base(0, 0)
        {
            m_From = from;

            from.CloseGump(typeof(RewardNoticeGump));

            AddPage(0);

            AddBackground(10, 10, 500, 135, 2600);

            /* You have reward items available.
             * Click 'ok' below to get the selection menu or 'cancel' to be prompted upon your next login.
             */
            AddHtmlLocalized(52, 35, 420, 55, 1006046, true, true);

            AddButton(60, 95, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(95, 96, 150, 35, 1006044, false, false); // Ok

            AddButton(285, 95, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(320, 96, 150, 35, 1006045, false, false); // Cancel
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_From.SendGump(new RewardChoiceGump(m_From));
        }
    }

    public class RewardChoiceGump : Gump
    {
        private Mobile m_From;

        private void RenderBackground()
        {
            AddPage(0);

            AddBackground(10, 10, 600, 450, 2600);

            AddButton(530, 415, 4017, 4019, 0, GumpButtonType.Reply, 0);

            AddButton(60, 415, 4014, 4016, 0, GumpButtonType.Page, 1);
            AddHtmlLocalized(95, 415, 200, 20, 1049755, false, false); // Main Menu
        }

        private void RenderCategories()
        {
            TimeSpan rewardInterval = RewardSystem.RewardInterval;

            string intervalAsString;

            if (rewardInterval == TimeSpan.FromDays(30.0))
                intervalAsString = "month";
            else if (rewardInterval == TimeSpan.FromDays(60.0))
                intervalAsString = "two months";
            else if (rewardInterval == TimeSpan.FromDays(90.0))
                intervalAsString = "three months";
            else if (rewardInterval == TimeSpan.FromDays(365.0))
                intervalAsString = "year";
            else
                intervalAsString = String.Format("{0} day{1}", rewardInterval.TotalDays, rewardInterval.TotalDays == 1 ? "" : "s");

            AddPage(1);

            AddHtml(60, 35, 500, 70, "<B>Ultima Online Rewards Program</B><BR>" +
                                    "Thank you for being a part of the Ultima Online community for a full " + intervalAsString + ".  " +
                                    "As a token of our appreciation,  you may select from the following in-game reward items listed below.  " +
                                    "The gift items will be attributed to the character you have logged-in with on the shard you are on when you chose the item(s).  " +
                                    "The number of rewards you are entitled to are listed below and are for your entire account.  " +
                                    "To read more about these rewards before making a selection, feel free to visit the uo.com site at " +
                                    "<A HREF=\"http://www.uo.com/rewards\">http://www.uo.com/rewards</A>.", true, true);

            int cur, max;

            RewardSystem.ComputeRewardInfo(m_From, out cur, out max);

            AddHtmlLocalized(60, 105, 300, 35, 1006006, false, false); // Your current total of rewards to choose:
            AddLabel(370, 107, 50, (max - cur).ToString());

            AddHtmlLocalized(60, 140, 300, 35, 1006007, false, false); // You have already chosen:
            AddLabel(370, 142, 50, cur.ToString());

            RewardCategory[] categories = RewardSystem.Categories;

            int page = 2;

            for (int i = 0; i < categories.Length; ++i)
            {
                if (!RewardSystem.HasAccess(m_From, categories[i]))
                {
                    page += 1;
                    continue;
                }

                AddButton(100, 180 + (i * 40), 4005, 4005, 0, GumpButtonType.Page, page);

                page += PagesPerCategory(categories[i]);

                if (categories[i].NameString != null)
                    AddHtml(135, 180 + (i * 40), 300, 20, categories[i].NameString, false, false);
                else
                    AddHtmlLocalized(135, 180 + (i * 40), 300, 20, categories[i].Name, false, false);
            }

            page = 2;

            for (int i = 0; i < categories.Length; ++i)
                RenderCategory(categories[i], i, ref page);
        }

        private int PagesPerCategory(RewardCategory category)
        {
            List<RewardEntry> entries = category.Entries;
            int j = 0, i = 0;

            for (j = 0; j < entries.Count; j++)
            {
                if (RewardSystem.HasAccess(m_From, entries[j]))
                    i++;
            }

            return (int)Math.Ceiling(i / 24.0);
        }

        private int GetButtonID(int type, int index)
        {
            return 2 + (index * 20) + type;
        }

        private void RenderCategory(RewardCategory category, int index, ref int page)
        {
            AddPage(page);

            List<RewardEntry> entries = category.Entries;

            int i = 0;

            for (int j = 0; j < entries.Count; ++j)
            {
                RewardEntry entry = entries[j];

                if (!RewardSystem.HasAccess(m_From, entry))
                    continue;

                if (i == 24)
                {
                    AddButton(305, 415, 0xFA5, 0xFA7, 0, GumpButtonType.Page, ++page);
                    AddHtmlLocalized(340, 415, 200, 20, 1011066, false, false); // Next page

                    AddPage(page);

                    AddButton(270, 415, 0xFAE, 0xFB0, 0, GumpButtonType.Page, page - 1);
                    AddHtmlLocalized(185, 415, 200, 20, 1011067, false, false); // Previous page

                    i = 0;
                }

                AddButton(55 + ((i / 12) * 250), 80 + ((i % 12) * 25), 5540, 5541, GetButtonID(index, j), GumpButtonType.Reply, 0);

                if (entry.NameString != null)
                    AddHtml(80 + ((i / 12) * 250), 80 + ((i % 12) * 25), 250, 20, entry.NameString, false, false);
                else
                    AddHtmlLocalized(80 + ((i / 12) * 250), 80 + ((i % 12) * 25), 250, 20, entry.Name, false, false);
                ++i;
            }

            page += 1;
        }

        public RewardChoiceGump(Mobile from)
            : base(0, 0)
        {
            m_From = from;

            from.CloseGump(typeof(RewardChoiceGump));

            RenderBackground();
            RenderCategories();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int buttonID = info.ButtonID - 1;

            if (buttonID == 0)
            {
                int cur, max;

                RewardSystem.ComputeRewardInfo(m_From, out cur, out max);

                if (cur < max)
                    m_From.SendGump(new RewardNoticeGump(m_From));
            }
            else
            {
                --buttonID;

                int type = (buttonID % 20);
                int index = (buttonID / 20);

                RewardCategory[] categories = RewardSystem.Categories;

                if (type >= 0 && type < categories.Length)
                {
                    RewardCategory category = categories[type];

                    if (index >= 0 && index < category.Entries.Count)
                    {
                        RewardEntry entry = category.Entries[index];

                        if (!RewardSystem.HasAccess(m_From, entry))
                            return;

                        m_From.SendGump(new RewardConfirmGump(m_From, entry));
                    }
                }
            }
        }
    }

    public class RewardEntry
    {
        private RewardList m_List;
        private RewardCategory m_Category;
        private Type m_ItemType;
        private Expansion m_RequiredExpansion;
        private int m_Name;
        private string m_NameString;
        private object[] m_Args;

        public RewardList List { get { return m_List; } set { m_List = value; } }
        public RewardCategory Category { get { return m_Category; } }
        public Type ItemType { get { return m_ItemType; } }
        public Expansion RequiredExpansion { get { return m_RequiredExpansion; } }
        public int Name { get { return m_Name; } }
        public string NameString { get { return m_NameString; } }
        public object[] Args { get { return m_Args; } }

        public Item Construct()
        {
            try
            {
                Item item = Activator.CreateInstance(m_ItemType, m_Args) as Item;

                if (item is IRewardItem)
                    ((IRewardItem)item).IsRewardItem = true;

                return item;
            }
            catch
            {
            }

            return null;
        }

        public RewardEntry(RewardCategory category, int name, Type itemType, params object[] args)
        {
            m_Category = category;
            m_ItemType = itemType;
            m_RequiredExpansion = Expansion.None;
            m_Name = name;
            m_Args = args;
            category.Entries.Add(this);
        }

        public RewardEntry(RewardCategory category, string name, Type itemType, params object[] args)
        {
            m_Category = category;
            m_ItemType = itemType;
            m_RequiredExpansion = Expansion.None;
            m_NameString = name;
            m_Args = args;
            category.Entries.Add(this);
        }

        public RewardEntry(RewardCategory category, int name, Type itemType, Expansion requiredExpansion, params object[] args)
        {
            m_Category = category;
            m_ItemType = itemType;
            m_RequiredExpansion = requiredExpansion;
            m_Name = name;
            m_Args = args;
            category.Entries.Add(this);
        }

        public RewardEntry(RewardCategory category, string name, Type itemType, Expansion requiredExpansion, params object[] args)
        {
            m_Category = category;
            m_ItemType = itemType;
            m_RequiredExpansion = requiredExpansion;
            m_NameString = name;
            m_Args = args;
            category.Entries.Add(this);
        }
    }

    public class RewardList
    {
        private TimeSpan m_Age;
        private RewardEntry[] m_Entries;

        public TimeSpan Age { get { return m_Age; } }
        public RewardEntry[] Entries { get { return m_Entries; } }

        public RewardList(TimeSpan interval, int index, RewardEntry[] entries)
        {
            m_Age = TimeSpan.FromDays(interval.TotalDays * index);
            m_Entries = entries;

            for (int i = 0; i < entries.Length; ++i)
                entries[i].List = this;
        }
    }

    public class RewardCategory
    {
        private int m_Name;
        private string m_NameString;
        private List<RewardEntry> m_Entries;

        public int Name { get { return m_Name; } }
        public string NameString { get { return m_NameString; } }
        public List<RewardEntry> Entries { get { return m_Entries; } }

        public RewardCategory(int name)
        {
            m_Name = name;
            m_Entries = new List<RewardEntry>();
        }

        public RewardCategory(string name)
        {
            m_NameString = name;
            m_Entries = new List<RewardEntry>();
        }
    }

    public class RewardConfirmGump : Gump
    {
        private Mobile m_From;
        private RewardEntry m_Entry;

        public RewardConfirmGump(Mobile from, RewardEntry entry)
            : base(0, 0)
        {
            m_From = from;
            m_Entry = entry;

            from.CloseGump(typeof(RewardConfirmGump));

            AddPage(0);

            AddBackground(10, 10, 500, 300, 2600);

            AddHtmlLocalized(30, 55, 300, 35, 1006000, false, false); // You have selected:

            if (entry.NameString != null)
                AddHtml(335, 55, 150, 35, entry.NameString, false, false);
            else
                AddHtmlLocalized(335, 55, 150, 35, entry.Name, false, false);

            AddHtmlLocalized(30, 95, 300, 35, 1006001, false, false); // This will be assigned to this character:
            AddLabel(335, 95, 0, from.Name);

            AddHtmlLocalized(35, 160, 450, 90, 1006002, true, true); // Are you sure you wish to select this reward for this character?  You will not be able to transfer this reward to another character on another shard.  Click 'ok' below to confirm your selection or 'cancel' to go back to the selection screen.

            AddButton(60, 265, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(95, 266, 150, 35, 1006044, false, false); // Ok

            AddButton(295, 265, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(330, 266, 150, 35, 1006045, false, false); // Cancel
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                if (!RewardSystem.HasAccess(m_From, m_Entry))
                    return;

                Item item = m_Entry.Construct();

                if (item != null)
                {
                    if (item is Server.Items.RedSoulstone)
                        ((Server.Items.RedSoulstone)item).Account = m_From.Account.Username;

                    if (RewardSystem.ConsumeRewardPoint(m_From))
                        m_From.AddToBackpack(item);
                    else
                        item.Delete();
                }
            }

            int cur, max;

            RewardSystem.ComputeRewardInfo(m_From, out cur, out max);

            if (cur < max)
                m_From.SendGump(new RewardNoticeGump(m_From));
        }
    }
}

namespace Server.Gumps
{
    public class RewardDemolitionGump : Gump
    {
        private IAddon m_Addon;

        private enum Buttons
        {
            Cancel,
            Confirm,
        }

        public RewardDemolitionGump(IAddon addon, int question)
            : base(150, 50)
        {
            m_Addon = addon;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddBackground(0, 0, 220, 170, 0x13BE);
            AddBackground(10, 10, 200, 150, 0xBB8);

            AddHtmlLocalized(20, 30, 180, 60, question, false, false); // Do you wish to re-deed this decoration?

            AddHtmlLocalized(55, 100, 150, 25, 1011011, false, false); // CONTINUE
            AddButton(20, 100, 0xFA5, 0xFA7, (int)Buttons.Confirm, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 125, 150, 25, 1011012, false, false); // CANCEL
            AddButton(20, 125, 0xFA5, 0xFA7, (int)Buttons.Cancel, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Item item = m_Addon as Item;

            if (item == null || item.Deleted)
                return;

            if (info.ButtonID == (int)Buttons.Confirm)
            {
                Mobile m = sender.Mobile;
                BaseHouse house = BaseHouse.FindHouseAt(m);

                if (house != null && house.IsOwner(m))
                {
                    if (m.InRange(item.Location, 2))
                    {
                        Item deed = m_Addon.Deed;

                        if (deed != null)
                        {
                            m.AddToBackpack(deed);
                            house.Addons.Remove(item);
                            item.Delete();
                        }
                    }
                    else
                        m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                }
                else
                    m.SendLocalizedMessage(1049784); // You can only re-deed this decoration if you are the house owner or originally placed the decoration.
            }
        }
    }

    public interface IRewardOption
    {
        void GetOptions(RewardOptionList list);
        void OnOptionSelected(Mobile from, int choice);
    }

    public class RewardOptionGump : Gump
    {
        private RewardOptionList m_Options = new RewardOptionList();
        private IRewardOption m_Option;

        public RewardOptionGump(IRewardOption option)
            : this(option, 0)
        {
        }

        public RewardOptionGump(IRewardOption option, int title)
            : base(60, 36)
        {
            m_Option = option;

            if (m_Option != null)
                m_Option.GetOptions(m_Options);

            AddPage(0);

            AddBackground(0, 0, 273, 324, 0x13BE);
            AddImageTiled(10, 10, 253, 20, 0xA40);
            AddImageTiled(10, 40, 253, 244, 0xA40);
            AddImageTiled(10, 294, 253, 20, 0xA40);
            AddAlphaRegion(10, 10, 253, 304);

            AddButton(10, 294, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(45, 296, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

            if (title > 0)
                AddHtmlLocalized(14, 12, 273, 20, title, 0x7FFF, false, false);
            else
                AddHtmlLocalized(14, 12, 273, 20, 1080392, 0x7FFF, false, false); // Select your choice from the menu below.

            AddPage(1);

            for (int i = 0; i < m_Options.Count; i++)
            {
                AddButton(19, 49 + i * 24, 0x845, 0x846, m_Options[i].ID, GumpButtonType.Reply, 0);
                AddHtmlLocalized(44, 47 + i * 24, 213, 20, m_Options[i].Cliloc, 0x7FFF, false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Option != null && Contains(info.ButtonID))
                m_Option.OnOptionSelected(sender.Mobile, info.ButtonID);
        }

        private bool Contains(int chosen)
        {
            if (m_Options == null)
                return false;

            foreach (RewardOption option in m_Options)
            {
                if (option.ID == chosen)
                    return true;
            }

            return false;
        }
    }

    public class RewardOption
    {
        private int m_ID;
        private int m_Cliloc;

        public int ID { get { return m_ID; } }
        public int Cliloc { get { return m_Cliloc; } }

        public RewardOption(int id, int cliloc)
        {
            m_ID = id;
            m_Cliloc = cliloc;
        }
    }

    public class RewardOptionList : List<RewardOption>
    {
        public RewardOptionList()
            : base()
        {
        }

        public void Add(int id, int cliloc)
        {
            Add(new RewardOption(id, cliloc));
        }
    }
}