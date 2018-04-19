using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Server;
using Server.Items;
using Server.Mobiles;

namespace Server
{
    public class Loot
    {
        private static Type[] m_MLWeaponTypes = new Type[]
			{
				typeof( AssassinSpike ),		typeof( DiamondMace ),			typeof( ElvenMachete ),
				typeof( ElvenSpellblade ),		typeof( Leafblade ),			typeof( OrnateAxe ),
				typeof( RadiantScimitar ),		typeof( RuneBlade ),			typeof( WarCleaver ),
				typeof( WildStaff )
			};

        public static Type[] MLWeaponTypes { get { return m_MLWeaponTypes; } }

        private static Type[] m_MLRangedWeaponTypes = new Type[]
			{
				typeof( ElvenCompositeLongbow ),	typeof( MagicalShortbow )
			};

        public static Type[] MLRangedWeaponTypes { get { return m_MLRangedWeaponTypes; } }

        private static Type[] m_MLArmorTypes = new Type[]
			{
				typeof( Circlet ),				typeof( GemmedCirclet ),		typeof( LeafTonlet ),
				typeof( RavenHelm ),			typeof( RoyalCirclet ),			typeof( VultureHelm ),
				typeof( WingedHelm ),			typeof( LeafArms ),				typeof( LeafChest ),
				typeof( LeafGloves ),			typeof( LeafGorget ),			typeof( LeafLegs ),
				typeof( WoodlandArms ),			typeof( WoodlandChest ),		typeof( WoodlandGloves ),
				typeof( WoodlandGorget ),		typeof( WoodlandLegs ),			typeof( HideChest ),
				typeof( HideGloves ),			typeof( HideGorget ),			typeof( HidePants ),
				typeof( HidePauldrons )
			};

        public static Type[] MLArmorTypes { get { return m_MLArmorTypes; } }

        private static Type[] m_MLClothingTypes = new Type[]
			{
				typeof( MaleElvenRobe ),		typeof( FemaleElvenRobe ),		typeof( ElvenPants ),
				typeof( ElvenShirt ),			typeof( ElvenDarkShirt ),		typeof( ElvenBoots ),
				typeof( VultureHelm ),			typeof( WoodlandBelt )
			};

        public static Type[] MLClothingTypes { get { return m_MLClothingTypes; } }

        private static Type[] m_SEWeaponTypes = new Type[]
			{
				typeof( Bokuto ),				typeof( Daisho ),				typeof( Kama ),
				typeof( Lajatang ),				typeof( NoDachi ),				typeof( Nunchaku ),
				typeof( Sai ),					typeof( Tekagi ),				typeof( Tessen ),
				typeof( Tetsubo ),				typeof( Wakizashi )
			};

        public static Type[] SEWeaponTypes { get { return m_SEWeaponTypes; } }

        private static Type[] m_AosWeaponTypes = new Type[]
			{
				typeof( Scythe ),				typeof( BoneHarvester ),		typeof( Scepter ),
				typeof( BladedStaff ),			typeof( Pike ),					typeof( DoubleBladedStaff ),
				typeof( Lance ),				typeof( CrescentBlade )
			};

        public static Type[] AosWeaponTypes { get { return m_AosWeaponTypes; } }

        private static Type[] m_WeaponTypes = new Type[]
			{
				typeof( Axe ),					typeof( BattleAxe ),			typeof( DoubleAxe ),
				typeof( ExecutionersAxe ),		typeof( Hatchet ),				typeof( LargeBattleAxe ),
				typeof( TwoHandedAxe ),			typeof( WarAxe ),				typeof( Club ),
				typeof( Mace ),					typeof( Maul ),					typeof( WarHammer ),
				typeof( WarMace ),				typeof( Bardiche ),				typeof( Halberd ),
				typeof( Spear ),				typeof( ShortSpear ),			typeof( Pitchfork ),
				typeof( WarFork ),				typeof( BlackStaff ),			typeof( GnarledStaff ),
				typeof( QuarterStaff ),			typeof( Broadsword ),			typeof( Cutlass ),
				typeof( Katana ),				typeof( Kryss ),				typeof( Longsword ),
				typeof( Scimitar ),				typeof( VikingSword ),			typeof( Pickaxe ),
				typeof( HammerPick ),			typeof( ButcherKnife ),			typeof( Cleaver ),
				typeof( Dagger ),				typeof( SkinningKnife ),		typeof( ShepherdsCrook )
			};

        public static Type[] WeaponTypes { get { return m_WeaponTypes; } }

        private static Type[] m_SERangedWeaponTypes = new Type[]
			{
				typeof( Yumi )
			};

        public static Type[] SERangedWeaponTypes { get { return m_SERangedWeaponTypes; } }

        private static Type[] m_AosRangedWeaponTypes = new Type[]
			{
				typeof( CompositeBow ),			typeof( RepeatingCrossbow )
			};

        public static Type[] AosRangedWeaponTypes { get { return m_AosRangedWeaponTypes; } }

        private static Type[] m_RangedWeaponTypes = new Type[]
			{
				typeof( Bow ),					typeof( Crossbow ),				typeof( HeavyCrossbow )
			};

        public static Type[] RangedWeaponTypes { get { return m_RangedWeaponTypes; } }

        private static Type[] m_SEArmorTypes = new Type[]
			{
				typeof( ChainHatsuburi ),		typeof( LeatherDo ),			typeof( LeatherHaidate ),
				typeof( LeatherHiroSode ),		typeof( LeatherJingasa ),		typeof( LeatherMempo ),
				typeof( LeatherNinjaHood ),		typeof( LeatherNinjaJacket ),	typeof( LeatherNinjaMitts ),
				typeof( LeatherNinjaPants ),	typeof( LeatherSuneate ),		typeof( DecorativePlateKabuto ),
				typeof( HeavyPlateJingasa ),	typeof( LightPlateJingasa ),	typeof( PlateBattleKabuto ),
				typeof( PlateDo ),				typeof( PlateHaidate ),			typeof( PlateHatsuburi ),
				typeof( PlateHiroSode ),		typeof( PlateMempo ),			typeof( PlateSuneate ),
				typeof( SmallPlateJingasa ),	typeof( StandardPlateKabuto ),	typeof( StuddedDo ),
				typeof( StuddedHaidate ),		typeof( StuddedHiroSode ),		typeof( StuddedMempo ),
				typeof( StuddedSuneate )
			};

        public static Type[] SEArmorTypes { get { return m_SEArmorTypes; } }

        private static Type[] m_ArmorTypes = new Type[]
			{
				typeof( BoneArms ),				typeof( BoneChest ),			typeof( BoneGloves ),
				typeof( BoneLegs ),				typeof( BoneHelm ),				typeof( ChainChest ),
				typeof( ChainLegs ),			typeof( ChainCoif ),			typeof( Bascinet ),
				typeof( CloseHelm ),			typeof( Helmet ),				typeof( NorseHelm ),
				typeof( OrcHelm ),				typeof( FemaleLeatherChest ),	typeof( LeatherArms ),
				typeof( LeatherBustierArms ),	typeof( LeatherChest ),			typeof( LeatherGloves ),
				typeof( LeatherGorget ),		typeof( LeatherLegs ),			typeof( LeatherShorts ),
				typeof( LeatherSkirt ),			typeof( LeatherCap ),			typeof( FemalePlateChest ),
				typeof( PlateArms ),			typeof( PlateChest ),			typeof( PlateGloves ),
				typeof( PlateGorget ),			typeof( PlateHelm ),			typeof( PlateLegs ),
				typeof( RingmailArms ),			typeof( RingmailChest ),		typeof( RingmailGloves ),
				typeof( RingmailLegs ),			typeof( FemaleStuddedChest ),	typeof( StuddedArms ),
				typeof( StuddedBustierArms ),	typeof( StuddedChest ),			typeof( StuddedGloves ),
				typeof( StuddedGorget ),		typeof( StuddedLegs )
			};

        public static Type[] ArmorTypes { get { return m_ArmorTypes; } }

        private static Type[] m_AosShieldTypes = new Type[]
			{
				typeof( ChaosShield ),			typeof( OrderShield )
			};

        public static Type[] AosShieldTypes { get { return m_AosShieldTypes; } }

        private static Type[] m_ShieldTypes = new Type[]
			{
				typeof( BronzeShield ),			typeof( Buckler ),				typeof( HeaterShield ),
				typeof( MetalShield ),			typeof( MetalKiteShield ),		typeof( WoodenKiteShield ),
				typeof( WoodenShield )
			};

        public static Type[] ShieldTypes { get { return m_ShieldTypes; } }

        private static Type[] m_GemTypes = new Type[]
			{
				typeof( Amber ),				typeof( Amethyst ),				typeof( Citrine ),
				typeof( Diamond ),				typeof( Emerald ),				typeof( Ruby ),
				typeof( Sapphire ),				typeof( StarSapphire ),			typeof( Tourmaline )
			};

        public static Type[] GemTypes { get { return m_GemTypes; } }

        private static Type[] m_JewelryTypes = new Type[]
			{
				typeof( GoldRing ),				typeof( GoldBracelet ),
				typeof( SilverRing ),			typeof( SilverBracelet )
			};

        public static Type[] JewelryTypes { get { return m_JewelryTypes; } }

        private static Type[] m_RegTypes = new Type[]
			{
				typeof( BlackPearl ),			typeof( Bloodmoss ),			typeof( Garlic ),
				typeof( Ginseng ),				typeof( MandrakeRoot ),			typeof( Nightshade ),
				typeof( SulfurousAsh ),			typeof( SpidersSilk )
			};

        public static Type[] RegTypes { get { return m_RegTypes; } }

        private static Type[] m_NecroRegTypes = new Type[]
			{
				typeof( BatWing ),				typeof( GraveDust ),			typeof( DaemonBlood ),
				typeof( NoxCrystal ),			typeof( PigIron )
			};

        public static Type[] NecroRegTypes { get { return m_NecroRegTypes; } }

        private static Type[] m_PotionTypes = new Type[]
			{
				typeof( AgilityPotion ),		typeof( StrengthPotion ),		typeof( RefreshPotion ),
				typeof( LesserCurePotion ),		typeof( LesserHealPotion ),		typeof( LesserPoisonPotion )
			};

        public static Type[] PotionTypes { get { return m_PotionTypes; } }

        private static Type[] m_SEInstrumentTypes = new Type[]
			{
				typeof( BambooFlute )
			};

        public static Type[] SEInstrumentTypes { get { return m_SEInstrumentTypes; } }

        private static Type[] m_InstrumentTypes = new Type[]
			{
				typeof( Drums ),				typeof( Harp ),					typeof( LapHarp ),
				typeof( Lute ),					typeof( Tambourine ),			typeof( TambourineTassel )
			};

        public static Type[] InstrumentTypes { get { return m_InstrumentTypes; } }

        private static Type[] m_StatueTypes = new Type[]
		{
			typeof( StatueSouth ),			typeof( StatueSouth2 ),			typeof( StatueNorth ),
			typeof( StatueWest ),			typeof( StatueEast ),			typeof( StatueEast2 ),
			typeof( StatueSouthEast ),		typeof( BustSouth ),			typeof( BustEast )
		};

        public static Type[] StatueTypes { get { return m_StatueTypes; } }

        private static Type[] m_RegularScrollTypes = new Type[]
			{
				typeof( ReactiveArmorScroll ),	typeof( ClumsyScroll ),			typeof( CreateFoodScroll ),		typeof( FeeblemindScroll ),
				typeof( HealScroll ),			typeof( MagicArrowScroll ),		typeof( NightSightScroll ),		typeof( WeakenScroll ),
				typeof( AgilityScroll ),		typeof( CunningScroll ),		typeof( CureScroll ),			typeof( HarmScroll ),
				typeof( MagicTrapScroll ),		typeof( MagicUnTrapScroll ),	typeof( ProtectionScroll ),		typeof( StrengthScroll ),
				typeof( BlessScroll ),			typeof( FireballScroll ),		typeof( MagicLockScroll ),		typeof( PoisonScroll ),
				typeof( TelekinisisScroll ),	typeof( TeleportScroll ),		typeof( UnlockScroll ),			typeof( WallOfStoneScroll ),
				typeof( ArchCureScroll ),		typeof( ArchProtectionScroll ),	typeof( CurseScroll ),			typeof( FireFieldScroll ),
				typeof( GreaterHealScroll ),	typeof( LightningScroll ),		typeof( ManaDrainScroll ),		typeof( RecallScroll ),
				typeof( BladeSpiritsScroll ),	typeof( DispelFieldScroll ),	typeof( IncognitoScroll ),		typeof( MagicReflectScroll ),
				typeof( MindBlastScroll ),		typeof( ParalyzeScroll ),		typeof( PoisonFieldScroll ),	typeof( SummonCreatureScroll ),
				typeof( DispelScroll ),			typeof( EnergyBoltScroll ),		typeof( ExplosionScroll ),		typeof( InvisibilityScroll ),
				typeof( MarkScroll ),			typeof( MassCurseScroll ),		typeof( ParalyzeFieldScroll ),	typeof( RevealScroll ),
				typeof( ChainLightningScroll ), typeof( EnergyFieldScroll ),	typeof( FlamestrikeScroll ),	typeof( GateTravelScroll ),
				typeof( ManaVampireScroll ),	typeof( MassDispelScroll ),		typeof( MeteorSwarmScroll ),	typeof( PolymorphScroll ),
				typeof( EarthquakeScroll ),		typeof( EnergyVortexScroll ),	typeof( ResurrectionScroll ),	typeof( SummonAirElementalScroll ),
				typeof( SummonDaemonScroll ),	typeof( SummonEarthElementalScroll ),	typeof( SummonFireElementalScroll ),	typeof( SummonWaterElementalScroll )
			};

        private static Type[] m_NecromancyScrollTypes = new Type[]
			{
				typeof( AnimateDeadScroll ),		typeof( BloodOathScroll ),		typeof( CorpseSkinScroll ),	typeof( CurseWeaponScroll ),
				typeof( EvilOmenScroll ),			typeof( HorrificBeastScroll ),	typeof( LichFormScroll ),	typeof( MindRotScroll ),
				typeof( PainSpikeScroll ),			typeof( PoisonStrikeScroll ),	typeof( StrangleScroll ),	typeof( SummonFamiliarScroll ),
				typeof( VampiricEmbraceScroll ),	typeof( VengefulSpiritScroll ),	typeof( WitherScroll ),		typeof( WraithFormScroll )
			};

        private static Type[] m_SENecromancyScrollTypes = new Type[]
		{
			typeof( AnimateDeadScroll ),		typeof( BloodOathScroll ),		typeof( CorpseSkinScroll ),	typeof( CurseWeaponScroll ),
			typeof( EvilOmenScroll ),			typeof( HorrificBeastScroll ),	typeof( LichFormScroll ),	typeof( MindRotScroll ),
			typeof( PainSpikeScroll ),			typeof( PoisonStrikeScroll ),	typeof( StrangleScroll ),	typeof( SummonFamiliarScroll ),
			typeof( VampiricEmbraceScroll ),	typeof( VengefulSpiritScroll ),	typeof( WitherScroll ),		typeof( WraithFormScroll ),
			typeof( ExorcismScroll )
		};

        private static Type[] m_PaladinScrollTypes = new Type[0];

        #region Mondain's Legacy
        private static Type[] m_ArcanistScrollTypes = new Type[]
		{
			typeof( ArcaneCircleScroll ),	typeof( GiftOfRenewalScroll ),	typeof( ImmolatingWeaponScroll ),	typeof( AttuneWeaponScroll ),
			typeof( ThunderstormScroll ),	typeof( NatureFuryScroll ),		/*typeof( SummonFeyScroll ),			typeof( SummonFiendScroll ),*/
			typeof( ReaperFormScroll ),		typeof( WildfireScroll ),		typeof( EssenceOfWindScroll ),		typeof( DryadAllureScroll ),
			typeof( EtherealVoyageScroll ),	typeof( WordOfDeathScroll ),	typeof( GiftOfLifeScroll ),			typeof( ArcaneEmpowermentScroll )
		};
        #endregion

        public static Type[] RegularScrollTypes { get { return m_RegularScrollTypes; } }
        public static Type[] NecromancyScrollTypes { get { return m_NecromancyScrollTypes; } }
        public static Type[] SENecromancyScrollTypes { get { return m_SENecromancyScrollTypes; } }
        public static Type[] PaladinScrollTypes { get { return m_PaladinScrollTypes; } }
        #region Mondain's Legacy
        public static Type[] ArcanistScrollTypes { get { return m_ArcanistScrollTypes; } }
        #endregion

        private static Type[] m_GrimmochJournalTypes = new Type[]
		{
			typeof( GrimmochJournal1 ),		typeof( GrimmochJournal2 ),		typeof( GrimmochJournal3 ),
			typeof( GrimmochJournal6 ),		typeof( GrimmochJournal7 ),		typeof( GrimmochJournal11 ),
			typeof( GrimmochJournal14 ),	typeof( GrimmochJournal17 ),	typeof( GrimmochJournal23 )
		};

        public static Type[] GrimmochJournalTypes { get { return m_GrimmochJournalTypes; } }

        private static Type[] m_LysanderNotebookTypes = new Type[]
		{
			typeof( LysanderNotebook1 ),		typeof( LysanderNotebook2 ),		typeof( LysanderNotebook3 ),
			typeof( LysanderNotebook7 ),		typeof( LysanderNotebook8 ),		typeof( LysanderNotebook11 )
		};

        public static Type[] LysanderNotebookTypes { get { return m_LysanderNotebookTypes; } }

        private static Type[] m_TavarasJournalTypes = new Type[]
		{
			typeof( TavarasJournal1 ),		typeof( TavarasJournal2 ),		typeof( TavarasJournal3 ),
			typeof( TavarasJournal6 ),		typeof( TavarasJournal7 ),		typeof( TavarasJournal8 ),
			typeof( TavarasJournal9 ),		typeof( TavarasJournal11 ),		typeof( TavarasJournal14 ),
			typeof( TavarasJournal16 ),		typeof( TavarasJournal16b ),	typeof( TavarasJournal17 ),
			typeof( TavarasJournal19 )
		};

        public static Type[] TavarasJournalTypes { get { return m_TavarasJournalTypes; } }


        private static Type[] m_NewWandTypes = new Type[]
			{
				typeof( FireballWand ),		typeof( LightningWand ),		typeof( MagicArrowWand ),
				typeof( GreaterHealWand ),	typeof( HarmWand ),				typeof( HealWand )
			};
        public static Type[] NewWandTypes { get { return m_NewWandTypes; } }

        private static Type[] m_WandTypes = new Type[]
			{
				typeof( ClumsyWand ),		typeof( FeebleWand ),
				typeof( ManaDrainWand ),	typeof( WeaknessWand )
			};
        public static Type[] WandTypes { get { return m_WandTypes; } }

        private static Type[] m_OldWandTypes = new Type[]
			{
				typeof( IDWand )
			};
        public static Type[] OldWandTypes { get { return m_OldWandTypes; } }

        private static Type[] m_SEClothingTypes = new Type[]
			{
				typeof( ClothNinjaJacket ),		typeof( FemaleKimono ),			typeof( Hakama ),
				typeof( HakamaShita ),			typeof( JinBaori ),				typeof( Kamishimo ),
				typeof( MaleKimono ),			typeof( NinjaTabi ),			typeof( Obi ),
				typeof( SamuraiTabi ),			typeof( TattsukeHakama ),		typeof( Waraji )
			};

        public static Type[] SEClothingTypes { get { return m_SEClothingTypes; } }

        private static Type[] m_AosClothingTypes = new Type[]
			{
				typeof( FurSarong ),			typeof( FurCape ),				typeof( FlowerGarland ),
				typeof( GildedDress ),			typeof( FurBoots ),				typeof( FormalShirt ),
		};

        public static Type[] AosClothingTypes { get { return m_AosClothingTypes; } }

        private static Type[] m_ClothingTypes = new Type[]
			{
				typeof( Cloak ),				
				typeof( Bonnet ),               typeof( Cap ),		            typeof( FeatheredHat ),
				typeof( FloppyHat ),            typeof( JesterHat ),			typeof( Surcoat ),
				typeof( SkullCap ),             typeof( StrawHat ),	            typeof( TallStrawHat ),
				typeof( TricorneHat ),			typeof( WideBrimHat ),          typeof( WizardsHat ),
				typeof( BodySash ),             typeof( Doublet ),              typeof( Boots ),
				typeof( FullApron ),            typeof( JesterSuit ),           typeof( Sandals ),
				typeof( Tunic ),				typeof( Shoes ),				typeof( Shirt ),
				typeof( Kilt ),                 typeof( Skirt ),				typeof( FancyShirt ),
				typeof( FancyDress ),			typeof( ThighBoots ),			typeof( LongPants ),
				typeof( PlainDress ),           typeof( Robe ),					typeof( ShortPants ),
				typeof( HalfApron )
			};
        public static Type[] ClothingTypes { get { return m_ClothingTypes; } }

        private static Type[] m_SEHatTypes = new Type[]
			{
				typeof( ClothNinjaHood ),		typeof( Kasa )
			};

        public static Type[] SEHatTypes { get { return m_SEHatTypes; } }

        private static Type[] m_AosHatTypes = new Type[]
			{
				typeof( FlowerGarland ),	typeof( BearMask ),		typeof( DeerMask )	//Are Bear& Deer mask inside the Pre-AoS loottables too?
			};

        public static Type[] AosHatTypes { get { return m_AosHatTypes; } }

        private static Type[] m_HatTypes = new Type[]
			{
				typeof( SkullCap ),			typeof( Bandana ),		typeof( FloppyHat ),
				typeof( Cap ),				typeof( WideBrimHat ),	typeof( StrawHat ),
				typeof( TallStrawHat ),		typeof( WizardsHat ),	typeof( Bonnet ),
				typeof( FeatheredHat ),		typeof( TricorneHat ),	typeof( JesterHat )
			};

        public static Type[] HatTypes { get { return m_HatTypes; } }

        private static Type[] m_LibraryBookTypes = new Type[]
			{
				typeof( GrammarOfOrcish ),		typeof( CallToAnarchy ),				typeof( ArmsAndWeaponsPrimer ),
				typeof( SongOfSamlethe ),		typeof( TaleOfThreeTribes ),			typeof( GuideToGuilds ),
				typeof( BirdsOfBritannia ),		typeof( BritannianFlora ),				typeof( ChildrenTalesVol2 ),
				typeof( TalesOfVesperVol1 ),	typeof( DeceitDungeonOfHorror ),		typeof( DimensionalTravel ),
				typeof( EthicalHedonism ),		typeof( MyStory ),						typeof( DiversityOfOurLand ),
				typeof( QuestOfVirtues ),		typeof( RegardingLlamas ),				typeof( TalkingToWisps ),
				typeof( TamingDragons ),		typeof( BoldStranger ),					typeof( BurningOfTrinsic ),
				typeof( TheFight ),				typeof( LifeOfATravellingMinstrel ),	typeof( MajorTradeAssociation ),
				typeof( RankingsOfTrades ),		typeof( WildGirlOfTheForest ),			typeof( TreatiseOnAlchemy ),
				typeof( VirtueBook )
			};

        public static Type[] LibraryBookTypes { get { return m_LibraryBookTypes; } }

        public static BaseWand RandomWand()
        {
            if (Core.ML)
                return Construct(m_NewWandTypes) as BaseWand;
            else if (Core.AOS)
                return Construct(m_WandTypes, m_NewWandTypes) as BaseWand;
            else
                return Construct(m_OldWandTypes, m_WandTypes, m_NewWandTypes) as BaseWand;
        }

        public static BaseClothing RandomClothing()
        {
            return RandomClothing(false, false);
        }

        public static BaseClothing RandomClothing(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLClothingTypes, m_AosClothingTypes, m_ClothingTypes) as BaseClothing;
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEClothingTypes, m_AosClothingTypes, m_ClothingTypes) as BaseClothing;

            if (Core.AOS)
                return Construct(m_AosClothingTypes, m_ClothingTypes) as BaseClothing;

            return Construct(m_ClothingTypes) as BaseClothing;
        }

        public static BaseWeapon RandomRangedWeapon()
        {
            return RandomRangedWeapon(false, false);
        }

        public static BaseWeapon RandomRangedWeapon(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLRangedWeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes) as BaseWeapon;
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SERangedWeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes) as BaseWeapon;

            if (Core.AOS)
                return Construct(m_AosRangedWeaponTypes, m_RangedWeaponTypes) as BaseWeapon;

            return Construct(m_RangedWeaponTypes) as BaseWeapon;
        }

        public static BaseWeapon RandomWeapon()
        {
            return RandomWeapon(false, false);
        }

        public static BaseWeapon RandomWeapon(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLWeaponTypes, m_AosWeaponTypes, m_WeaponTypes) as BaseWeapon;
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEWeaponTypes, m_AosWeaponTypes, m_WeaponTypes) as BaseWeapon;

            if (Core.AOS)
                return Construct(m_AosWeaponTypes, m_WeaponTypes) as BaseWeapon;

            return Construct(m_WeaponTypes) as BaseWeapon;
        }

        public static Item RandomWeaponOrJewelry()
        {
            return RandomWeaponOrJewelry(false, false);
        }

        public static Item RandomWeaponOrJewelry(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLWeaponTypes, m_AosWeaponTypes, m_WeaponTypes, m_JewelryTypes);
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEWeaponTypes, m_AosWeaponTypes, m_WeaponTypes, m_JewelryTypes);

            if (Core.AOS)
                return Construct(m_AosWeaponTypes, m_WeaponTypes, m_JewelryTypes);

            return Construct(m_WeaponTypes, m_JewelryTypes);
        }

        public static BaseJewel RandomJewelry()
        {
            return Construct(m_JewelryTypes) as BaseJewel;
        }

        public static BaseArmor RandomArmor()
        {
            return RandomArmor(false, false);
        }

        public static BaseArmor RandomArmor(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLArmorTypes, m_ArmorTypes) as BaseArmor;
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEArmorTypes, m_ArmorTypes) as BaseArmor;

            return Construct(m_ArmorTypes) as BaseArmor;
        }

        public static BaseHat RandomHat()
        {
            return RandomHat(false);
        }

        public static BaseHat RandomHat(bool inTokuno)
        {
            if (Core.SE && inTokuno)
                return Construct(m_SEHatTypes, m_AosHatTypes, m_HatTypes) as BaseHat;

            if (Core.AOS)
                return Construct(m_AosHatTypes, m_HatTypes) as BaseHat;

            return Construct(m_HatTypes) as BaseHat;
        }

        public static Item RandomArmorOrHat()
        {
            return RandomArmorOrHat(false, false);
        }

        public static Item RandomArmorOrHat(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLArmorTypes, m_ArmorTypes, m_AosHatTypes, m_HatTypes);
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEArmorTypes, m_ArmorTypes, m_SEHatTypes, m_AosHatTypes, m_HatTypes);

            if (Core.AOS)
                return Construct(m_ArmorTypes, m_AosHatTypes, m_HatTypes);

            return Construct(m_ArmorTypes, m_HatTypes);
        }

        public static BaseShield RandomShield()
        {
            if (Core.AOS)
                return Construct(m_AosShieldTypes, m_ShieldTypes) as BaseShield;

            return Construct(m_ShieldTypes) as BaseShield;
        }

        public static BaseArmor RandomArmorOrShield()
        {
            return RandomArmorOrShield(false, false);
        }

        public static BaseArmor RandomArmorOrShield(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLArmorTypes, m_ArmorTypes, m_AosShieldTypes, m_ShieldTypes) as BaseArmor;
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEArmorTypes, m_ArmorTypes, m_AosShieldTypes, m_ShieldTypes) as BaseArmor;

            if (Core.AOS)
                return Construct(m_ArmorTypes, m_AosShieldTypes, m_ShieldTypes) as BaseArmor;

            return Construct(m_ArmorTypes, m_ShieldTypes) as BaseArmor;
        }

        public static Item RandomArmorOrShieldOrJewelry()
        {
            return RandomArmorOrShieldOrJewelry(false, false);
        }

        public static Item RandomArmorOrShieldOrJewelry(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLArmorTypes, m_ArmorTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes, m_JewelryTypes);
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEArmorTypes, m_ArmorTypes, m_SEHatTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes, m_JewelryTypes);

            if (Core.AOS)
                return Construct(m_ArmorTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes, m_JewelryTypes);

            return Construct(m_ArmorTypes, m_HatTypes, m_ShieldTypes, m_JewelryTypes);
        }

        public static Item RandomArmorOrShieldOrWeapon()
        {
            return RandomArmorOrShieldOrWeapon(false, false);
        }

        public static Item RandomArmorOrShieldOrWeapon(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLWeaponTypes, m_AosWeaponTypes, m_WeaponTypes, m_MLRangedWeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes, m_MLArmorTypes, m_ArmorTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes);
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEWeaponTypes, m_AosWeaponTypes, m_WeaponTypes, m_SERangedWeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes, m_SEArmorTypes, m_ArmorTypes, m_SEHatTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes);

            if (Core.AOS)
                return Construct(m_AosWeaponTypes, m_WeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes, m_ArmorTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes);

            return Construct(m_WeaponTypes, m_RangedWeaponTypes, m_ArmorTypes, m_HatTypes, m_ShieldTypes);
        }

        public static Item RandomArmorOrShieldOrWeaponOrJewelry()
        {
            return RandomArmorOrShieldOrWeaponOrJewelry(false, false);
        }

        public static Item RandomArmorOrShieldOrWeaponOrJewelry(bool inTokuno, bool isMondain)
        {
            #region Mondain's Legacy
            if (Core.ML && isMondain)
                return Construct(m_MLWeaponTypes, m_AosWeaponTypes, m_WeaponTypes, m_MLRangedWeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes, m_MLArmorTypes, m_ArmorTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes, m_JewelryTypes);
            #endregion

            if (Core.SE && inTokuno)
                return Construct(m_SEWeaponTypes, m_AosWeaponTypes, m_WeaponTypes, m_SERangedWeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes, m_SEArmorTypes, m_ArmorTypes, m_SEHatTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes, m_JewelryTypes);

            if (Core.AOS)
                return Construct(m_AosWeaponTypes, m_WeaponTypes, m_AosRangedWeaponTypes, m_RangedWeaponTypes, m_ArmorTypes, m_AosHatTypes, m_HatTypes, m_AosShieldTypes, m_ShieldTypes, m_JewelryTypes);

            return Construct(m_WeaponTypes, m_RangedWeaponTypes, m_ArmorTypes, m_HatTypes, m_ShieldTypes, m_JewelryTypes);
        }

        #region Chest of Heirlooms
        public static Item ChestOfHeirloomsContains()
        {
            return Construct(m_SEArmorTypes, m_SEHatTypes, m_SEWeaponTypes, m_SERangedWeaponTypes, m_JewelryTypes);
        }
        #endregion

        public static Item RandomGem()
        {
            return Construct(m_GemTypes);
        }

        public static Item RandomReagent()
        {
            return Construct(m_RegTypes);
        }

        public static Item RandomNecromancyReagent()
        {
            return Construct(m_NecroRegTypes);
        }

        public static Item RandomPossibleReagent()
        {
            if (Core.AOS)
                return Construct(m_RegTypes, m_NecroRegTypes);

            return Construct(m_RegTypes);
        }

        public static Item RandomPotion()
        {
            return Construct(m_PotionTypes);
        }

        public static BaseInstrument RandomInstrument()
        {
            if (Core.SE)
                return Construct(m_InstrumentTypes, m_SEInstrumentTypes) as BaseInstrument;

            return Construct(m_InstrumentTypes) as BaseInstrument;
        }

        public static Item RandomStatue()
        {
            return Construct(m_StatueTypes);
        }

        public static SpellScroll RandomScroll(int minIndex, int maxIndex, SpellbookType type)
        {
            Type[] types;

            switch (type)
            {
                default:
                case SpellbookType.Regular: types = m_RegularScrollTypes; break;
                case SpellbookType.Necromancer: types = (Core.SE ? m_SENecromancyScrollTypes : m_NecromancyScrollTypes); break;
                case SpellbookType.Paladin: types = m_PaladinScrollTypes; break;
                case SpellbookType.Arcanist: types = m_ArcanistScrollTypes; break;
            }

            return Construct(types, Utility.RandomMinMax(minIndex, maxIndex)) as SpellScroll;
        }

        public static BaseBook RandomGrimmochJournal()
        {
            return Construct(m_GrimmochJournalTypes) as BaseBook;
        }

        public static BaseBook RandomLysanderNotebook()
        {
            return Construct(m_LysanderNotebookTypes) as BaseBook;
        }

        public static BaseBook RandomTavarasJournal()
        {
            return Construct(m_TavarasJournalTypes) as BaseBook;
        }

        public static BaseBook RandomLibraryBook()
        {
            return Construct(m_LibraryBookTypes) as BaseBook;
        }

        public static BaseTalisman RandomTalisman()
        {
            BaseTalisman talisman = new BaseTalisman(BaseTalisman.GetRandomItemID());

            talisman.Summoner = BaseTalisman.GetRandomSummoner();

            if (talisman.Summoner.IsEmpty)
            {
                talisman.Removal = BaseTalisman.GetRandomRemoval();

                if (talisman.Removal != TalismanRemoval.None)
                {
                    talisman.MaxCharges = BaseTalisman.GetRandomCharges();
                    talisman.MaxChargeTime = 1200;
                }
            }
            else
            {
                talisman.MaxCharges = Utility.RandomMinMax(10, 50);

                if (talisman.Summoner.IsItem)
                    talisman.MaxChargeTime = 60;
                else
                    talisman.MaxChargeTime = 1800;
            }

            talisman.Blessed = BaseTalisman.GetRandomBlessed();
            talisman.Slayer = BaseTalisman.GetRandomSlayer();
            talisman.Protection = BaseTalisman.GetRandomProtection();
            talisman.Killer = BaseTalisman.GetRandomKiller();
            talisman.Skill = BaseTalisman.GetRandomSkill();
            talisman.ExceptionalBonus = BaseTalisman.GetRandomExceptional();
            talisman.SuccessBonus = BaseTalisman.GetRandomSuccessful();
            talisman.Charges = talisman.MaxCharges;

            return talisman;
        }

        public static Item Construct(Type type)
        {
            try
            {
                return Activator.CreateInstance(type) as Item;
            }
            catch
            {
                return null;
            }
        }

        public static Item Construct(Type[] types)
        {
            if (types.Length > 0)
                return Construct(types, Utility.Random(types.Length));

            return null;
        }

        public static Item Construct(Type[] types, int index)
        {
            if (index >= 0 && index < types.Length)
                return Construct(types[index]);

            return null;
        }

        public static Item Construct(params Type[][] types)
        {
            int totalLength = 0;

            for (int i = 0; i < types.Length; ++i)
                totalLength += types[i].Length;

            if (totalLength > 0)
            {
                int index = Utility.Random(totalLength);

                for (int i = 0; i < types.Length; ++i)
                {
                    if (index >= 0 && index < types[i].Length)
                        return Construct(types[i][index]);

                    index -= types[i].Length;
                }
            }

            return null;
        }
    }

    public class LootPack
    {
        public static int GetLuckChance(Mobile killer, Mobile victim)
        {
            if (!Core.AOS)
                return 0;

            int luck = killer.Luck;

            PlayerMobile pmKiller = killer as PlayerMobile;
            if (pmKiller != null && pmKiller.SentHonorContext != null && pmKiller.SentHonorContext.Target == victim)
                luck += pmKiller.SentHonorContext.PerfectionLuckBonus;

            if (luck < 0)
                return 0;

            if (!Core.SE && luck > 1200)
                luck = 1200;

            return (int)(Math.Pow(luck, 1 / 1.8) * 100);
        }

        public static int GetLuckChanceForKiller(Mobile dead)
        {
            List<DamageStore> list = BaseCreature.GetLootingRights(dead.DamageEntries, dead.HitsMax);

            DamageStore highest = null;

            for (int i = 0; i < list.Count; ++i)
            {
                DamageStore ds = list[i];

                if (ds.m_HasRight && (highest == null || ds.m_Damage > highest.m_Damage))
                    highest = ds;
            }

            if (highest == null)
                return 0;

            return GetLuckChance(highest.m_Mobile, dead);
        }

        public static bool CheckLuck(int chance)
        {
            return (chance > Utility.Random(10000));
        }

        private LootPackEntry[] m_Entries;

        public LootPack(LootPackEntry[] entries)
        {
            m_Entries = entries;
        }

        public void Generate(Mobile from, Container cont, bool spawning, int luckChance)
        {
            if (cont == null)
                return;

            bool checkLuck = Core.AOS;

            for (int i = 0; i < m_Entries.Length; ++i)
            {
                LootPackEntry entry = m_Entries[i];

                bool shouldAdd = (entry.Chance > Utility.Random(10000));

                if (!shouldAdd && checkLuck)
                {
                    checkLuck = false;

                    if (LootPack.CheckLuck(luckChance))
                        shouldAdd = (entry.Chance > Utility.Random(10000));
                }

                if (!shouldAdd)
                    continue;

                Item item = entry.Construct(from, luckChance, spawning);

                if (item != null)
                {
                    if (!item.Stackable || !cont.TryDropItem(from, item, false))
                        cont.DropItem(item);
                }
            }
        }

        public static readonly LootPackItem[] Gold = new LootPackItem[]
			{
				new LootPackItem( typeof( Gold ), 1 )
			};

        public static readonly LootPackItem[] Instruments = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseInstrument ), 1 )
			};


        public static readonly LootPackItem[] LowScrollItems = new LootPackItem[]
			{
				new LootPackItem( typeof( ClumsyScroll ), 1 )
			};

        public static readonly LootPackItem[] MedScrollItems = new LootPackItem[]
			{
				new LootPackItem( typeof( ArchCureScroll ), 1 )
			};

        public static readonly LootPackItem[] HighScrollItems = new LootPackItem[]
			{
				new LootPackItem( typeof( SummonAirElementalScroll ), 1 )
			};

        public static readonly LootPackItem[] GemItems = new LootPackItem[]
			{
				new LootPackItem( typeof( Amber ), 1 )
			};

        public static readonly LootPackItem[] PotionItems = new LootPackItem[]
			{
				new LootPackItem( typeof( AgilityPotion ), 1 ),
				new LootPackItem( typeof( StrengthPotion ), 1 ),
				new LootPackItem( typeof( RefreshPotion ), 1 ),
				new LootPackItem( typeof( LesserCurePotion ), 1 ),
				new LootPackItem( typeof( LesserHealPotion ), 1 ),
				new LootPackItem( typeof( LesserPoisonPotion ), 1 )
			};

        #region Old Magic Items
        public static readonly LootPackItem[] OldMagicItems = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseJewel ), 1 ),
				new LootPackItem( typeof( BaseArmor ), 4 ),
				new LootPackItem( typeof( BaseWeapon ), 3 ),
				new LootPackItem( typeof( BaseRanged ), 1 ),
				new LootPackItem( typeof( BaseShield ), 1 )
			};
        #endregion

        #region AOS Magic Items
        public static readonly LootPackItem[] AosMagicItemsPoor = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 3 ),
				new LootPackItem( typeof( BaseRanged ), 1 ),
				new LootPackItem( typeof( BaseArmor ), 4 ),
				new LootPackItem( typeof( BaseShield ), 1 ),
				new LootPackItem( typeof( BaseJewel ), 2 )
			};

        public static readonly LootPackItem[] AosMagicItemsMeagerType1 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 56 ),
				new LootPackItem( typeof( BaseRanged ), 14 ),
				new LootPackItem( typeof( BaseArmor ), 81 ),
				new LootPackItem( typeof( BaseShield ), 11 ),
				new LootPackItem( typeof( BaseJewel ), 42 )
			};

        public static readonly LootPackItem[] AosMagicItemsMeagerType2 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 28 ),
				new LootPackItem( typeof( BaseRanged ), 7 ),
				new LootPackItem( typeof( BaseArmor ), 40 ),
				new LootPackItem( typeof( BaseShield ), 5 ),
				new LootPackItem( typeof( BaseJewel ), 21 )
			};

        public static readonly LootPackItem[] AosMagicItemsAverageType1 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 90 ),
				new LootPackItem( typeof( BaseRanged ), 23 ),
				new LootPackItem( typeof( BaseArmor ), 130 ),
				new LootPackItem( typeof( BaseShield ), 17 ),
				new LootPackItem( typeof( BaseJewel ), 68 )
			};

        public static readonly LootPackItem[] AosMagicItemsAverageType2 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 54 ),
				new LootPackItem( typeof( BaseRanged ), 13 ),
				new LootPackItem( typeof( BaseArmor ), 77 ),
				new LootPackItem( typeof( BaseShield ), 10 ),
				new LootPackItem( typeof( BaseJewel ), 40 )
			};

        public static readonly LootPackItem[] AosMagicItemsRichType1 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 211 ),
				new LootPackItem( typeof( BaseRanged ), 53 ),
				new LootPackItem( typeof( BaseArmor ), 303 ),
				new LootPackItem( typeof( BaseShield ), 39 ),
				new LootPackItem( typeof( BaseJewel ), 158 )
			};

        public static readonly LootPackItem[] AosMagicItemsRichType2 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 170 ),
				new LootPackItem( typeof( BaseRanged ), 43 ),
				new LootPackItem( typeof( BaseArmor ), 245 ),
				new LootPackItem( typeof( BaseShield ), 32 ),
				new LootPackItem( typeof( BaseJewel ), 128 )
			};

        public static readonly LootPackItem[] AosMagicItemsFilthyRichType1 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 219 ),
				new LootPackItem( typeof( BaseRanged ), 55 ),
				new LootPackItem( typeof( BaseArmor ), 315 ),
				new LootPackItem( typeof( BaseShield ), 41 ),
				new LootPackItem( typeof( BaseJewel ), 164 )
			};

        public static readonly LootPackItem[] AosMagicItemsFilthyRichType2 = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 239 ),
				new LootPackItem( typeof( BaseRanged ), 60 ),
				new LootPackItem( typeof( BaseArmor ), 343 ),
				new LootPackItem( typeof( BaseShield ), 90 ),
				new LootPackItem( typeof( BaseJewel ), 45 )
			};

        public static readonly LootPackItem[] AosMagicItemsUltraRich = new LootPackItem[]
			{
				new LootPackItem( typeof( BaseWeapon ), 276 ),
				new LootPackItem( typeof( BaseRanged ), 69 ),
				new LootPackItem( typeof( BaseArmor ), 397 ),
				new LootPackItem( typeof( BaseShield ), 52 ),
				new LootPackItem( typeof( BaseJewel ), 207 )
			};
        #endregion

        #region ML definitions
        public static readonly LootPack MlRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "4d50+450" ),
				new LootPackEntry( false, AosMagicItemsRichType1,	100.00, 1, 3, 0, 75 ),
				new LootPackEntry( false, AosMagicItemsRichType1,	 80.00, 1, 3, 0, 75 ),
				new LootPackEntry( false, AosMagicItemsRichType1,	 60.00, 1, 5, 0, 100 ),
				new LootPackEntry( false, Instruments,				  1.00, 1 )
			});
        #endregion

        #region SE definitions
        public static readonly LootPack SePoor = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "2d10+20" ),
				new LootPackEntry( false, AosMagicItemsPoor,		  1.00, 1, 5, 0, 100 ),
				new LootPackEntry( false, Instruments,				  0.02, 1 )
			});

        public static readonly LootPack SeMeager = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "4d10+40" ),
				new LootPackEntry( false, AosMagicItemsMeagerType1,	 20.40, 1, 2, 0, 50 ),
				new LootPackEntry( false, AosMagicItemsMeagerType2,	 10.20, 1, 5, 0, 100 ),
				new LootPackEntry( false, Instruments,				  0.10, 1 )
			});

        public static readonly LootPack SeAverage = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "8d10+100" ),
				new LootPackEntry( false, AosMagicItemsAverageType1, 32.80, 1, 3, 0, 50 ),
				new LootPackEntry( false, AosMagicItemsAverageType1, 32.80, 1, 4, 0, 75 ),
				new LootPackEntry( false, AosMagicItemsAverageType2, 19.50, 1, 5, 0, 100 ),
				new LootPackEntry( false, Instruments,				  0.40, 1 )
			});

        public static readonly LootPack SeRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "15d10+225" ),
				new LootPackEntry( false, AosMagicItemsRichType1,	 76.30, 1, 4, 0, 75 ),
				new LootPackEntry( false, AosMagicItemsRichType1,	 76.30, 1, 4, 0, 75 ),
				new LootPackEntry( false, AosMagicItemsRichType2,	 61.70, 1, 5, 0, 100 ),
				new LootPackEntry( false, Instruments,				  1.00, 1 )
			});

        public static readonly LootPack SeFilthyRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						   100.00, "3d100+400" ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType1,	79.50, 1, 5, 0, 100 ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType1,	79.50, 1, 5, 0, 100 ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType2,	77.60, 1, 5, 25, 100 ),
				new LootPackEntry( false, Instruments,					 2.00, 1 )
			});

        public static readonly LootPack SeUltraRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "6d100+600" ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, Instruments,				  2.00, 1 )
			});

        public static readonly LootPack SeSuperBoss = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,						100.00, "10d100+800" ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 50, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 50, 100 ),
				new LootPackEntry( false, Instruments,				  2.00, 1 )
			});
        #endregion

        #region AOS definitions
        public static readonly LootPack AosPoor = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "1d10+10" ),
				new LootPackEntry( false, AosMagicItemsPoor,	  0.02, 1, 5, 0, 90 ),
				new LootPackEntry( false, Instruments,	  0.02, 1 )
			});

        public static readonly LootPack AosMeager = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "3d10+20" ),
				new LootPackEntry( false, AosMagicItemsMeagerType1,	  1.00, 1, 2, 0, 10 ),
				new LootPackEntry( false, AosMagicItemsMeagerType2,	  0.20, 1, 5, 0, 90 ),
				new LootPackEntry( false, Instruments,	  0.10, 1 )
			});

        public static readonly LootPack AosAverage = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "5d10+50" ),
				new LootPackEntry( false, AosMagicItemsAverageType1,  5.00, 1, 4, 0, 20 ),
				new LootPackEntry( false, AosMagicItemsAverageType1,  2.00, 1, 3, 0, 50 ),
				new LootPackEntry( false, AosMagicItemsAverageType2,  0.50, 1, 5, 0, 90 ),
				new LootPackEntry( false, Instruments,	  0.40, 1 )
			});

        public static readonly LootPack AosRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "10d10+150" ),
				new LootPackEntry( false, AosMagicItemsRichType1,	 20.00, 1, 4, 0, 40 ),
				new LootPackEntry( false, AosMagicItemsRichType1,	 10.00, 1, 5, 0, 60 ),
				new LootPackEntry( false, AosMagicItemsRichType2,	  1.00, 1, 5, 0, 90 ),
				new LootPackEntry( false, Instruments,	  1.00, 1 )
			});

        public static readonly LootPack AosFilthyRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "2d100+200" ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType1,	 33.00, 1, 4, 0, 50 ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType1,	 33.00, 1, 4, 0, 60 ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType2,	 20.00, 1, 5, 0, 75 ),
				new LootPackEntry( false, AosMagicItemsFilthyRichType2,	  5.00, 1, 5, 0, 100 ),
				new LootPackEntry( false, Instruments,	  2.00, 1 )
			});

        public static readonly LootPack AosUltraRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "5d100+500" ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 35, 100 ),
				new LootPackEntry( false, Instruments,	  2.00, 1 )
			});

        public static readonly LootPack AosSuperBoss = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "5d100+500" ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 25, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 33, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 50, 100 ),
				new LootPackEntry( false, AosMagicItemsUltraRich,	100.00, 1, 5, 50, 100 ),
				new LootPackEntry( false, Instruments,	  2.00, 1 )
			});
        #endregion

        #region Pre-AOS definitions
        public static readonly LootPack OldPoor = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "1d25" ),
				new LootPackEntry( false, Instruments,	  0.02, 1 )
			});

        public static readonly LootPack OldMeager = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "5d10+25" ),
				new LootPackEntry( false, Instruments,	  0.10, 1 ),
				new LootPackEntry( false, OldMagicItems,  1.00, 1, 1, 0, 60 ),
				new LootPackEntry( false, OldMagicItems,  0.20, 1, 1, 10, 70 )
			});

        public static readonly LootPack OldAverage = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "10d10+50" ),
				new LootPackEntry( false, Instruments,	  0.40, 1 ),
				new LootPackEntry( false, OldMagicItems,  5.00, 1, 1, 20, 80 ),
				new LootPackEntry( false, OldMagicItems,  2.00, 1, 1, 30, 90 ),
				new LootPackEntry( false, OldMagicItems,  0.50, 1, 1, 40, 100 )
			});

        public static readonly LootPack OldRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "10d10+250" ),
				new LootPackEntry( false, Instruments,	  1.00, 1 ),
				new LootPackEntry( false, OldMagicItems, 20.00, 1, 1, 60, 100 ),
				new LootPackEntry( false, OldMagicItems, 10.00, 1, 1, 65, 100 ),
				new LootPackEntry( false, OldMagicItems,  1.00, 1, 1, 70, 100 )
			});

        public static readonly LootPack OldFilthyRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "2d125+400" ),
				new LootPackEntry( false, Instruments,	  2.00, 1 ),
				new LootPackEntry( false, OldMagicItems, 33.00, 1, 1, 50, 100 ),
				new LootPackEntry( false, OldMagicItems, 33.00, 1, 1, 60, 100 ),
				new LootPackEntry( false, OldMagicItems, 20.00, 1, 1, 70, 100 ),
				new LootPackEntry( false, OldMagicItems,  5.00, 1, 1, 80, 100 )
			});

        public static readonly LootPack OldUltraRich = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "5d100+500" ),
				new LootPackEntry( false, Instruments,	  2.00, 1 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 40, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 40, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 50, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 50, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 60, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 60, 100 )
			});

        public static readonly LootPack OldSuperBoss = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry(  true, Gold,			100.00, "5d100+500" ),
				new LootPackEntry( false, Instruments,	  2.00, 1 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 40, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 40, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 40, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 50, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 50, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 50, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 60, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 60, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 60, 100 ),
				new LootPackEntry( false, OldMagicItems,	100.00, 1, 1, 70, 100 )
			});
        #endregion

        #region Generic accessors
        public static LootPack Poor { get { return Core.SE ? SePoor : Core.AOS ? AosPoor : OldPoor; } }
        public static LootPack Meager { get { return Core.SE ? SeMeager : Core.AOS ? AosMeager : OldMeager; } }
        public static LootPack Average { get { return Core.SE ? SeAverage : Core.AOS ? AosAverage : OldAverage; } }
        public static LootPack Rich { get { return Core.SE ? SeRich : Core.AOS ? AosRich : OldRich; } }
        public static LootPack FilthyRich { get { return Core.SE ? SeFilthyRich : Core.AOS ? AosFilthyRich : OldFilthyRich; } }
        public static LootPack UltraRich { get { return Core.SE ? SeUltraRich : Core.AOS ? AosUltraRich : OldUltraRich; } }
        public static LootPack SuperBoss { get { return Core.SE ? SeSuperBoss : Core.AOS ? AosSuperBoss : OldSuperBoss; } }
        #endregion

        public static readonly LootPack LowScrolls = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry( false, LowScrollItems,	100.00, 1 )
			});

        public static readonly LootPack MedScrolls = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry( false, MedScrollItems,	100.00, 1 )
			});

        public static readonly LootPack HighScrolls = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry( false, HighScrollItems,	100.00, 1 )
			});

        public static readonly LootPack Gems = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry( false, GemItems,			100.00, 1 )
			});

        public static readonly LootPack Potions = new LootPack(new LootPackEntry[]
			{
				new LootPackEntry( false, PotionItems,		100.00, 1 )
			});

        /*
        // TODO: Uncomment once added
        #region Mondain's Legacy
        public static readonly LootPackItem[] ParrotItem = new LootPackItem[]
            {
                new LootPackItem( typeof( ParrotItem ), 1 )
            };

        public static readonly LootPack Parrot = new LootPack( new LootPackEntry[]
            {
                new LootPackEntry( false, ParrotItem, 10.00, 1 )
            } );
        #endregion
        */
    }

    public class LootPackEntry
    {
        private int m_Chance;
        private LootPackDice m_Quantity;

        private int m_MaxProps, m_MinIntensity, m_MaxIntensity;

        private bool m_AtSpawnTime;

        private LootPackItem[] m_Items;

        public int Chance
        {
            get { return m_Chance; }
            set { m_Chance = value; }
        }

        public LootPackDice Quantity
        {
            get { return m_Quantity; }
            set { m_Quantity = value; }
        }

        public int MaxProps
        {
            get { return m_MaxProps; }
            set { m_MaxProps = value; }
        }

        public int MinIntensity
        {
            get { return m_MinIntensity; }
            set { m_MinIntensity = value; }
        }

        public int MaxIntensity
        {
            get { return m_MaxIntensity; }
            set { m_MaxIntensity = value; }
        }

        public LootPackItem[] Items
        {
            get { return m_Items; }
            set { m_Items = value; }
        }

        private static bool IsInTokuno(Mobile m)
        {
            if (m.Region.IsPartOf("Fan Dancer's Dojo"))
                return true;

            if (m.Region.IsPartOf("Yomotsu Mines"))
                return true;

            return (m.Map == Map.Tokuno);
        }

        #region Mondain's Legacy
        private static bool IsMondain(Mobile m)
        {
            return MondainsLegacy.IsMLRegion(m.Region);
        }
        #endregion

        public Item Construct(Mobile from, int luckChance, bool spawning)
        {
            if (m_AtSpawnTime != spawning)
                return null;

            int totalChance = 0;

            for (int i = 0; i < m_Items.Length; ++i)
                totalChance += m_Items[i].Chance;

            int rnd = Utility.Random(totalChance);

            for (int i = 0; i < m_Items.Length; ++i)
            {
                LootPackItem item = m_Items[i];

                if (rnd < item.Chance)
                    return Mutate(from, luckChance, item.Construct(IsInTokuno(from), IsMondain(from)));

                rnd -= item.Chance;
            }

            return null;
        }

        private int GetRandomOldBonus()
        {
            int rnd = Utility.RandomMinMax(m_MinIntensity, m_MaxIntensity);

            if (50 > rnd)
                return 1;
            else
                rnd -= 50;

            if (25 > rnd)
                return 2;
            else
                rnd -= 25;

            if (14 > rnd)
                return 3;
            else
                rnd -= 14;

            if (8 > rnd)
                return 4;

            return 5;
        }

        public Item Mutate(Mobile from, int luckChance, Item item)
        {
            if (item != null)
            {
                if (item is BaseWeapon && 1 > Utility.Random(100))
                {
                    item.Delete();
                    item = new FireHorn();
                    return item;
                }

                if (item is BaseWeapon || item is BaseArmor || item is BaseJewel || item is BaseHat)
                {
                    if (Core.AOS)
                    {
                        int bonusProps = GetBonusProperties();
                        int min = m_MinIntensity;
                        int max = m_MaxIntensity;

                        if (bonusProps < m_MaxProps && LootPack.CheckLuck(luckChance))
                            ++bonusProps;

                        int props = 1 + bonusProps;

                        // Make sure we're not spawning items with 6 properties.
                        if (props > m_MaxProps)
                            props = m_MaxProps;

                        if (item is BaseWeapon)
                            BaseRunicTool.ApplyAttributesTo((BaseWeapon)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
                        else if (item is BaseArmor)
                            BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
                        else if (item is BaseJewel)
                            BaseRunicTool.ApplyAttributesTo((BaseJewel)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
                        else if (item is BaseHat)
                            BaseRunicTool.ApplyAttributesTo((BaseHat)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
                    }
                    else // not aos
                    {
                        if (item is BaseWeapon)
                        {
                            BaseWeapon weapon = (BaseWeapon)item;

                            if (80 > Utility.Random(100))
                                weapon.AccuracyLevel = (WeaponAccuracyLevel)GetRandomOldBonus();

                            if (60 > Utility.Random(100))
                                weapon.DamageLevel = (WeaponDamageLevel)GetRandomOldBonus();

                            if (40 > Utility.Random(100))
                                weapon.DurabilityLevel = (WeaponDurabilityLevel)GetRandomOldBonus();

                            if (5 > Utility.Random(100))
                                weapon.Slayer = SlayerName.Silver;

                            if (from != null && weapon.AccuracyLevel == 0 && weapon.DamageLevel == 0 && weapon.DurabilityLevel == 0 && weapon.Slayer == SlayerName.None && 5 > Utility.Random(100))
                                weapon.Slayer = SlayerGroup.GetLootSlayerType(from.GetType());
                        }
                        else if (item is BaseArmor)
                        {
                            BaseArmor armor = (BaseArmor)item;

                            if (80 > Utility.Random(100))
                                armor.ProtectionLevel = (ArmorProtectionLevel)GetRandomOldBonus();

                            if (40 > Utility.Random(100))
                                armor.Durability = (ArmorDurabilityLevel)GetRandomOldBonus();
                        }
                    }
                }
                else if (item is BaseInstrument)
                {
                    SlayerName slayer = SlayerName.None;

                    if (Core.AOS)
                        slayer = BaseRunicTool.GetRandomSlayer();
                    else
                        slayer = SlayerGroup.GetLootSlayerType(from.GetType());

                    if (slayer == SlayerName.None)
                    {
                        item.Delete();
                        return null;
                    }

                    BaseInstrument instr = (BaseInstrument)item;

                    instr.Quality = InstrumentQuality.Regular;
                    instr.Slayer = slayer;
                }

                if (item.Stackable)
                    item.Amount = m_Quantity.Roll();
            }

            return item;
        }

        public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, string quantity)
            : this(atSpawnTime, items, chance, new LootPackDice(quantity), 0, 0, 0)
        {
        }

        public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, int quantity)
            : this(atSpawnTime, items, chance, new LootPackDice(0, 0, quantity), 0, 0, 0)
        {
        }

        public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, string quantity, int maxProps, int minIntensity, int maxIntensity)
            : this(atSpawnTime, items, chance, new LootPackDice(quantity), maxProps, minIntensity, maxIntensity)
        {
        }

        public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, int quantity, int maxProps, int minIntensity, int maxIntensity)
            : this(atSpawnTime, items, chance, new LootPackDice(0, 0, quantity), maxProps, minIntensity, maxIntensity)
        {
        }

        public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, LootPackDice quantity, int maxProps, int minIntensity, int maxIntensity)
        {
            m_AtSpawnTime = atSpawnTime;
            m_Items = items;
            m_Chance = (int)(100 * chance);
            m_Quantity = quantity;
            m_MaxProps = maxProps;
            m_MinIntensity = minIntensity;
            m_MaxIntensity = maxIntensity;
        }

        public int GetBonusProperties()
        {
            int p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0;

            switch (m_MaxProps)
            {
                case 1: p0 = 3; p1 = 1; break;
                case 2: p0 = 6; p1 = 3; p2 = 1; break;
                case 3: p0 = 10; p1 = 6; p2 = 3; p3 = 1; break;
                case 4: p0 = 16; p1 = 12; p2 = 6; p3 = 5; p4 = 1; break;
                case 5: p0 = 30; p1 = 25; p2 = 20; p3 = 15; p4 = 9; p5 = 1; break;
            }

            int pc = p0 + p1 + p2 + p3 + p4 + p5;

            int rnd = Utility.Random(pc);

            if (rnd < p5)
                return 5;
            else
                rnd -= p5;

            if (rnd < p4)
                return 4;
            else
                rnd -= p4;

            if (rnd < p3)
                return 3;
            else
                rnd -= p3;

            if (rnd < p2)
                return 2;
            else
                rnd -= p2;

            if (rnd < p1)
                return 1;

            return 0;
        }
    }

    public class LootPackItem
    {
        private Type m_Type;
        private int m_Chance;

        public Type Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public int Chance
        {
            get { return m_Chance; }
            set { m_Chance = value; }
        }

        private static Type[] m_BlankTypes = new Type[] { typeof(BlankScroll) };
        private static Type[][] m_NecroTypes = new Type[][]
			{
				new Type[] // low
				{
					typeof( AnimateDeadScroll ),		typeof( BloodOathScroll ),		typeof( CorpseSkinScroll ),	typeof( CurseWeaponScroll ),
					typeof( EvilOmenScroll ),			typeof( HorrificBeastScroll ),	typeof( MindRotScroll ),	typeof( PainSpikeScroll ),
					typeof( SummonFamiliarScroll ),		typeof( WraithFormScroll )
				},
				new Type[] // med
				{
					typeof( LichFormScroll ),			typeof( PoisonStrikeScroll ),	typeof( StrangleScroll ),	typeof( WitherScroll )
				},

				((Core.SE) ?
				new Type[] // high
				{
					typeof( VengefulSpiritScroll ),		typeof( VampiricEmbraceScroll ), typeof( ExorcismScroll )
				} :
				new Type[] // high
				{
					typeof( VengefulSpiritScroll ),		typeof( VampiricEmbraceScroll )
				})
			};

        public static Item RandomScroll(int index, int minCircle, int maxCircle)
        {
            --minCircle;
            --maxCircle;

            int scrollCount = ((maxCircle - minCircle) + 1) * 8;

            if (index == 0)
                scrollCount += m_BlankTypes.Length;

            if (Core.AOS)
                scrollCount += m_NecroTypes[index].Length;

            int rnd = Utility.Random(scrollCount);

            if (index == 0 && rnd < m_BlankTypes.Length)
                return Loot.Construct(m_BlankTypes);
            else if (index == 0)
                rnd -= m_BlankTypes.Length;

            if (Core.AOS && rnd < m_NecroTypes.Length)
                return Loot.Construct(m_NecroTypes[index]);
            else if (Core.AOS)
                rnd -= m_NecroTypes[index].Length;

            return Loot.RandomScroll(minCircle * 8, (maxCircle * 8) + 7, SpellbookType.Regular);
        }

        public Item Construct(bool inTokuno, bool isMondain)
        {
            try
            {
                Item item;

                if (m_Type == typeof(BaseRanged))
                    item = Loot.RandomRangedWeapon(inTokuno, isMondain);
                else if (m_Type == typeof(BaseWeapon))
                    item = Loot.RandomWeapon(inTokuno, isMondain);
                else if (m_Type == typeof(BaseArmor))
                    item = Loot.RandomArmorOrHat(inTokuno, isMondain);
                else if (m_Type == typeof(BaseShield))
                    item = Loot.RandomShield();
                else if (m_Type == typeof(BaseJewel))
                    item = Core.AOS ? Loot.RandomJewelry() : Loot.RandomArmorOrShieldOrWeapon();
                else if (m_Type == typeof(BaseInstrument))
                    item = Loot.RandomInstrument();
                else if (m_Type == typeof(Amber)) // gem
                    item = Loot.RandomGem();
                else if (m_Type == typeof(ClumsyScroll)) // low scroll
                    item = RandomScroll(0, 1, 3);
                else if (m_Type == typeof(ArchCureScroll)) // med scroll
                    item = RandomScroll(1, 4, 7);
                else if (m_Type == typeof(SummonAirElementalScroll)) // high scroll
                    item = RandomScroll(2, 8, 8);
                else
                    item = Activator.CreateInstance(m_Type) as Item;

                return item;
            }
            catch
            {
            }

            return null;
        }

        public LootPackItem(Type type, int chance)
        {
            m_Type = type;
            m_Chance = chance;
        }
    }

    public class LootPackDice
    {
        private int m_Count, m_Sides, m_Bonus;

        public int Count
        {
            get { return m_Count; }
            set { m_Count = value; }
        }

        public int Sides
        {
            get { return m_Sides; }
            set { m_Sides = value; }
        }

        public int Bonus
        {
            get { return m_Bonus; }
            set { m_Bonus = value; }
        }

        public int Roll()
        {
            int v = m_Bonus;

            for (int i = 0; i < m_Count; ++i)
                v += Utility.Random(1, m_Sides);

            return v;
        }

        public LootPackDice(string str)
        {
            int start = 0;
            int index = str.IndexOf('d', start);

            if (index < start)
                return;

            m_Count = Utility.ToInt32(str.Substring(start, index - start));

            bool negative;

            start = index + 1;
            index = str.IndexOf('+', start);

            if (negative = (index < start))
                index = str.IndexOf('-', start);

            if (index < start)
                index = str.Length;

            m_Sides = Utility.ToInt32(str.Substring(start, index - start));

            if (index == str.Length)
                return;

            start = index + 1;
            index = str.Length;

            m_Bonus = Utility.ToInt32(str.Substring(start, index - start));

            if (negative)
                m_Bonus *= -1;
        }

        public LootPackDice(int count, int sides, int bonus)
        {
            m_Count = count;
            m_Sides = sides;
            m_Bonus = bonus;
        }
    }
}