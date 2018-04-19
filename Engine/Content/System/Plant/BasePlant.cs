using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Plants
{
    public enum PlantHealth
    {
        Dying,
        Wilted,
        Healthy,
        Vibrant
    }

    public enum PlantGrowthIndicator
    {
        None,
        InvalidLocation,
        NotHealthy,
        Delay,
        Grown,
        DoubleGrown
    }

    public class PlantSystem
    {
        public static readonly TimeSpan CheckDelay = TimeSpan.FromHours(23.0);

        private PlantItem m_Plant;
        private bool m_FertileDirt;

        private DateTime m_NextGrowth;
        private PlantGrowthIndicator m_GrowthIndicator;

        private int m_Water;

        private int m_Hits;
        private int m_Infestation;
        private int m_Fungus;
        private int m_Poison;
        private int m_Disease;
        private int m_PoisonPotion;
        private int m_CurePotion;
        private int m_HealPotion;
        private int m_StrengthPotion;

        private bool m_Pollinated;
        private PlantType m_SeedType;
        private PlantHue m_SeedHue;
        private int m_AvailableSeeds;
        private int m_LeftSeeds;

        private int m_AvailableResources;
        private int m_LeftResources;

        public PlantItem Plant { get { return m_Plant; } }

        public bool FertileDirt
        {
            get { return m_FertileDirt; }
            set { m_FertileDirt = value; }
        }

        public DateTime NextGrowth
        {
            get { return m_NextGrowth; }
        }

        public PlantGrowthIndicator GrowthIndicator
        {
            get { return m_GrowthIndicator; }
        }

        public bool IsFullWater { get { return m_Water >= 4; } }
        public int Water
        {
            get { return m_Water; }
            set
            {
                if (value < 0)
                    m_Water = 0;
                else if (value > 4)
                    m_Water = 4;
                else
                    m_Water = value;

                m_Plant.InvalidateProperties();
            }
        }

        public int Hits
        {
            get { return m_Hits; }
            set
            {
                if (m_Hits == value)
                    return;

                if (value < 0)
                    m_Hits = 0;
                else if (value > MaxHits)
                    m_Hits = MaxHits;
                else
                    m_Hits = value;

                if (m_Hits == 0)
                    m_Plant.Die();

                m_Plant.InvalidateProperties();
            }
        }

        public int MaxHits
        {
            get { return 10 + (int)m_Plant.PlantStatus * 2; }
        }

        public PlantHealth Health
        {
            get
            {
                int perc = m_Hits * 100 / MaxHits;

                if (perc < 33)
                    return PlantHealth.Dying;
                else if (perc < 66)
                    return PlantHealth.Wilted;
                else if (perc < 100)
                    return PlantHealth.Healthy;
                else
                    return PlantHealth.Vibrant;
            }
        }

        public int Infestation
        {
            get { return m_Infestation; }
            set
            {
                if (value < 0)
                    m_Infestation = 0;
                else if (value > 2)
                    m_Infestation = 2;
                else
                    m_Infestation = value;
            }
        }

        public int Fungus
        {
            get { return m_Fungus; }
            set
            {
                if (value < 0)
                    m_Fungus = 0;
                else if (value > 2)
                    m_Fungus = 2;
                else
                    m_Fungus = value;
            }
        }

        public int Poison
        {
            get { return m_Poison; }
            set
            {
                if (value < 0)
                    m_Poison = 0;
                else if (value > 2)
                    m_Poison = 2;
                else
                    m_Poison = value;
            }
        }

        public int Disease
        {
            get { return m_Disease; }
            set
            {
                if (value < 0)
                    m_Disease = 0;
                else if (value > 2)
                    m_Disease = 2;
                else
                    m_Disease = value;
            }
        }

        public bool IsFullPoisonPotion { get { return m_PoisonPotion >= 2; } }
        public int PoisonPotion
        {
            get { return m_PoisonPotion; }
            set
            {
                if (value < 0)
                    m_PoisonPotion = 0;
                else if (value > 2)
                    m_PoisonPotion = 2;
                else
                    m_PoisonPotion = value;
            }
        }

        public bool IsFullCurePotion { get { return m_CurePotion >= 2; } }
        public int CurePotion
        {
            get { return m_CurePotion; }
            set
            {
                if (value < 0)
                    m_CurePotion = 0;
                else if (value > 2)
                    m_CurePotion = 2;
                else
                    m_CurePotion = value;
            }
        }

        public bool IsFullHealPotion { get { return m_HealPotion >= 2; } }
        public int HealPotion
        {
            get { return m_HealPotion; }
            set
            {
                if (value < 0)
                    m_HealPotion = 0;
                else if (value > 2)
                    m_HealPotion = 2;
                else
                    m_HealPotion = value;
            }
        }

        public bool IsFullStrengthPotion { get { return m_StrengthPotion >= 2; } }
        public int StrengthPotion
        {
            get { return m_StrengthPotion; }
            set
            {
                if (value < 0)
                    m_StrengthPotion = 0;
                else if (value > 2)
                    m_StrengthPotion = 2;
                else
                    m_StrengthPotion = value;
            }
        }

        public bool HasMaladies
        {
            get { return Infestation > 0 || Fungus > 0 || Poison > 0 || Disease > 0 || Water != 2; }
        }

        public bool PollenProducing
        {
            get { return m_Plant.IsCrossable && m_Plant.PlantStatus >= PlantStatus.FullGrownPlant; }
        }

        public bool Pollinated
        {
            get { return m_Pollinated; }
            set { m_Pollinated = value; }
        }

        public PlantType SeedType
        {
            get
            {
                if (m_Pollinated)
                    return m_SeedType;
                else
                    return m_Plant.PlantType;
            }
            set { m_SeedType = value; }
        }

        public PlantHue SeedHue
        {
            get
            {
                if (m_Pollinated)
                    return m_SeedHue;
                else
                    return m_Plant.PlantHue;
            }
            set { m_SeedHue = value; }
        }

        public int AvailableSeeds
        {
            get { return m_AvailableSeeds; }
            set { if (value >= 0) m_AvailableSeeds = value; }
        }

        public int LeftSeeds
        {
            get { return m_LeftSeeds; }
            set { if (value >= 0) m_LeftSeeds = value; }
        }

        public int AvailableResources
        {
            get { return m_AvailableResources; }
            set { if (value >= 0) m_AvailableResources = value; }
        }

        public int LeftResources
        {
            get { return m_LeftResources; }
            set { if (value >= 0) m_LeftResources = value; }
        }

        public PlantSystem(PlantItem plant, bool fertileDirt)
        {
            m_Plant = plant;
            m_FertileDirt = fertileDirt;

            m_NextGrowth = DateTime.UtcNow + CheckDelay;
            m_GrowthIndicator = PlantGrowthIndicator.None;
            m_Hits = MaxHits;
            m_LeftSeeds = 8;
            m_LeftResources = 8;
        }

        public void Reset(bool potions)
        {
            m_NextGrowth = DateTime.UtcNow + CheckDelay;
            m_GrowthIndicator = PlantGrowthIndicator.None;

            Hits = MaxHits;
            m_Infestation = 0;
            m_Fungus = 0;
            m_Poison = 0;
            m_Disease = 0;

            if (potions)
            {
                m_PoisonPotion = 0;
                m_CurePotion = 0;
                m_HealPotion = 0;
                m_StrengthPotion = 0;
            }

            m_Pollinated = false;
            m_AvailableSeeds = 0;
            m_LeftSeeds = 8;

            m_AvailableResources = 0;
            m_LeftResources = 8;
        }

        public int GetLocalizedDirtStatus()
        {
            if (Water <= 1)
                return 1060826; // hard
            else if (Water <= 2)
                return 1060827; // soft
            else if (Water <= 3)
                return 1060828; // squishy
            else
                return 1060829; // sopping wet
        }

        public int GetLocalizedHealth()
        {
            switch (Health)
            {
                case PlantHealth.Dying: return 1060825; // dying
                case PlantHealth.Wilted: return 1060824; // wilted
                case PlantHealth.Healthy: return 1060823; // healthy
                default: return 1060822; // vibrant
            }
        }

        public static void Configure()
        {
            EventSink.WorldLoad += new WorldLoadEventHandler(EventSink_WorldLoad);

            if (!Misc.AutoRestart.Enabled)
                EventSink.WorldSave += new WorldSaveEventHandler(EventSink_WorldSave);

            EventSink.Login += new LoginEventHandler(EventSink_Login);
        }

        private static void EventSink_Login(LoginEventArgs args)
        {
            Mobile from = args.Mobile;

            if (from.Backpack != null)
            {
                List<PlantItem> plants = from.Backpack.FindItemsByType<PlantItem>();

                foreach (PlantItem plant in plants)
                {
                    if (plant.IsGrowable)
                        plant.PlantSystem.DoGrowthCheck();
                }
            }

            BankBox bank = from.FindBankNoCreate();

            if (bank != null)
            {
                List<PlantItem> plants = bank.FindItemsByType<PlantItem>();

                foreach (PlantItem plant in plants)
                {
                    if (plant.IsGrowable)
                        plant.PlantSystem.DoGrowthCheck();
                }
            }
        }

        public static void GrowAll()
        {
            ArrayList plants = PlantItem.Plants;
            DateTime now = DateTime.UtcNow;

            for (int i = plants.Count - 1; i >= 0; --i)
            {
                PlantItem plant = (PlantItem)plants[i];

                if (plant.IsGrowable && (plant.RootParent as Mobile) == null && now >= plant.PlantSystem.NextGrowth)
                    plant.PlantSystem.DoGrowthCheck();
            }
        }

        private static void EventSink_WorldLoad()
        {
            GrowAll();
        }

        private static void EventSink_WorldSave(WorldSaveEventArgs args)
        {
            GrowAll();
        }

        public void DoGrowthCheck()
        {
            if (!m_Plant.IsGrowable)
                return;

            if (DateTime.UtcNow < m_NextGrowth)
            {
                m_GrowthIndicator = PlantGrowthIndicator.Delay;
                return;
            }

            m_NextGrowth = DateTime.UtcNow + CheckDelay;

            if (!m_Plant.ValidGrowthLocation)
            {
                m_GrowthIndicator = PlantGrowthIndicator.InvalidLocation;
                return;
            }

            if (m_Plant.PlantStatus == PlantStatus.BowlOfDirt)
            {
                if (Water > 2 || Utility.RandomDouble() < 0.9)
                    Water--;
                return;
            }

            ApplyBeneficEffects();

            if (!ApplyMaladiesEffects()) // Dead
                return;

            Grow();

            UpdateMaladies();
        }

        private void ApplyBeneficEffects()
        {
            if (PoisonPotion >= Infestation)
            {
                PoisonPotion -= Infestation;
                Infestation = 0;
            }
            else
            {
                Infestation -= PoisonPotion;
                PoisonPotion = 0;
            }

            if (CurePotion >= Fungus)
            {
                CurePotion -= Fungus;
                Fungus = 0;
            }
            else
            {
                Fungus -= CurePotion;
                CurePotion = 0;
            }

            if (HealPotion >= Poison)
            {
                HealPotion -= Poison;
                Poison = 0;
            }
            else
            {
                Poison -= HealPotion;
                HealPotion = 0;
            }

            if (HealPotion >= Disease)
            {
                HealPotion -= Disease;
                Disease = 0;
            }
            else
            {
                Disease -= HealPotion;
                HealPotion = 0;
            }

            if (!HasMaladies)
            {
                if (HealPotion > 0)
                    Hits += HealPotion * 7;
                else
                    Hits += 2;
            }

            HealPotion = 0;
        }

        private bool ApplyMaladiesEffects()
        {
            int damage = 0;

            if (Infestation > 0)
                damage += Infestation * Utility.RandomMinMax(3, 6);

            if (Fungus > 0)
                damage += Fungus * Utility.RandomMinMax(3, 6);

            if (Poison > 0)
                damage += Poison * Utility.RandomMinMax(3, 6);

            if (Disease > 0)
                damage += Disease * Utility.RandomMinMax(3, 6);

            if (Water > 2)
                damage += (Water - 2) * Utility.RandomMinMax(3, 6);
            else if (Water < 2)
                damage += (2 - Water) * Utility.RandomMinMax(3, 6);

            Hits -= damage;

            return m_Plant.IsGrowable && m_Plant.PlantStatus != PlantStatus.BowlOfDirt;
        }

        private void Grow()
        {
            if (Health < PlantHealth.Healthy)
            {
                m_GrowthIndicator = PlantGrowthIndicator.NotHealthy;
            }
            else if (m_FertileDirt && m_Plant.PlantStatus <= PlantStatus.Stage5 && Utility.RandomDouble() < 0.1)
            {
                int curStage = (int)m_Plant.PlantStatus;
                m_Plant.PlantStatus = (PlantStatus)(curStage + 2);

                m_GrowthIndicator = PlantGrowthIndicator.DoubleGrown;
            }
            else if (m_Plant.PlantStatus < PlantStatus.Stage9)
            {
                int curStage = (int)m_Plant.PlantStatus;
                m_Plant.PlantStatus = (PlantStatus)(curStage + 1);

                m_GrowthIndicator = PlantGrowthIndicator.Grown;
            }
            else
            {
                if (Pollinated && LeftSeeds > 0 && m_Plant.Reproduces)
                {
                    LeftSeeds--;
                    AvailableSeeds++;
                }

                if (LeftResources > 0 && PlantResourceInfo.GetInfo(m_Plant.PlantType, m_Plant.PlantHue) != null)
                {
                    LeftResources--;
                    AvailableResources++;
                }

                m_GrowthIndicator = PlantGrowthIndicator.Grown;
            }

            if (m_Plant.PlantStatus >= PlantStatus.Stage9 && !Pollinated)
            {
                Pollinated = true;
                SeedType = m_Plant.PlantType;
                SeedHue = m_Plant.PlantHue;
            }
        }

        private void UpdateMaladies()
        {
            double infestationChance = 0.30 - StrengthPotion * 0.075 + (Water - 2) * 0.10;

            PlantTypeInfo typeInfo = PlantTypeInfo.GetInfo(m_Plant.PlantType);
            if (typeInfo.Flowery)
                infestationChance += 0.10;

            if (PlantHueInfo.IsBright(m_Plant.PlantHue))
                infestationChance += 0.10;

            if (Utility.RandomDouble() < infestationChance)
                Infestation++;


            double fungusChance = 0.15 - StrengthPotion * 0.075 + (Water - 2) * 0.10;

            if (Utility.RandomDouble() < fungusChance)
                Fungus++;

            if (Water > 2 || Utility.RandomDouble() < 0.9)
                Water--;

            if (PoisonPotion > 0)
            {
                Poison += PoisonPotion;
                PoisonPotion = 0;
            }

            if (CurePotion > 0)
            {
                Disease += CurePotion;
                CurePotion = 0;
            }

            StrengthPotion = 0;
        }

        public void Save(GenericWriter writer)
        {
            writer.Write((int)2); // version

            writer.Write((bool)m_FertileDirt);

            writer.Write((DateTime)m_NextGrowth);
            writer.Write((int)m_GrowthIndicator);

            writer.Write((int)m_Water);

            writer.Write((int)m_Hits);
            writer.Write((int)m_Infestation);
            writer.Write((int)m_Fungus);
            writer.Write((int)m_Poison);
            writer.Write((int)m_Disease);
            writer.Write((int)m_PoisonPotion);
            writer.Write((int)m_CurePotion);
            writer.Write((int)m_HealPotion);
            writer.Write((int)m_StrengthPotion);

            writer.Write((bool)m_Pollinated);
            writer.Write((int)m_SeedType);
            writer.Write((int)m_SeedHue);
            writer.Write((int)m_AvailableSeeds);
            writer.Write((int)m_LeftSeeds);

            writer.Write((int)m_AvailableResources);
            writer.Write((int)m_LeftResources);
        }

        public PlantSystem(PlantItem plant, GenericReader reader)
        {
            m_Plant = plant;

            int version = reader.ReadInt();

            m_FertileDirt = reader.ReadBool();

            if (version >= 1)
                m_NextGrowth = reader.ReadDateTime();
            else
                m_NextGrowth = reader.ReadDeltaTime();

            m_GrowthIndicator = (PlantGrowthIndicator)reader.ReadInt();

            m_Water = reader.ReadInt();

            m_Hits = reader.ReadInt();
            m_Infestation = reader.ReadInt();
            m_Fungus = reader.ReadInt();
            m_Poison = reader.ReadInt();
            m_Disease = reader.ReadInt();
            m_PoisonPotion = reader.ReadInt();
            m_CurePotion = reader.ReadInt();
            m_HealPotion = reader.ReadInt();
            m_StrengthPotion = reader.ReadInt();

            m_Pollinated = reader.ReadBool();
            m_SeedType = (PlantType)reader.ReadInt();
            m_SeedHue = (PlantHue)reader.ReadInt();
            m_AvailableSeeds = reader.ReadInt();
            m_LeftSeeds = reader.ReadInt();

            m_AvailableResources = reader.ReadInt();
            m_LeftResources = reader.ReadInt();

            if (version < 2 && PlantHueInfo.IsCrossable(m_SeedHue))
                m_SeedHue |= PlantHue.Reproduces;
        }
    }

    public class MainPlantGump : Gump
    {
        private PlantItem m_Plant;

        public MainPlantGump(PlantItem plant)
            : base(20, 20)
        {
            m_Plant = plant;

            DrawBackground();

            DrawPlant();

            AddButton(71, 67, 0xD4, 0xD4, 1, GumpButtonType.Reply, 0); // Reproduction menu
            AddItem(59, 68, 0xD08);

            PlantSystem system = plant.PlantSystem;

            AddButton(71, 91, 0xD4, 0xD4, 2, GumpButtonType.Reply, 0); // Infestation
            AddItem(8, 96, 0x372);
            AddPlus(95, 92, system.Infestation);

            AddButton(71, 115, 0xD4, 0xD4, 3, GumpButtonType.Reply, 0); // Fungus
            AddItem(58, 115, 0xD16);
            AddPlus(95, 116, system.Fungus);

            AddButton(71, 139, 0xD4, 0xD4, 4, GumpButtonType.Reply, 0); // Poison
            AddItem(59, 143, 0x1AE4);
            AddPlus(95, 140, system.Poison);

            AddButton(71, 163, 0xD4, 0xD4, 5, GumpButtonType.Reply, 0); // Disease
            AddItem(55, 167, 0x1727);
            AddPlus(95, 164, system.Disease);

            AddButton(209, 67, 0xD2, 0xD2, 6, GumpButtonType.Reply, 0); // Water
            AddItem(193, 67, 0x1F9D);
            AddPlusMinus(196, 67, system.Water);

            AddButton(209, 91, 0xD4, 0xD4, 7, GumpButtonType.Reply, 0); // Poison potion
            AddItem(201, 91, 0xF0A);
            AddLevel(196, 91, system.PoisonPotion);

            AddButton(209, 115, 0xD4, 0xD4, 8, GumpButtonType.Reply, 0); // Cure potion
            AddItem(201, 115, 0xF07);
            AddLevel(196, 115, system.CurePotion);

            AddButton(209, 139, 0xD4, 0xD4, 9, GumpButtonType.Reply, 0); // Heal potion
            AddItem(201, 139, 0xF0C);
            AddLevel(196, 139, system.HealPotion);

            AddButton(209, 163, 0xD4, 0xD4, 10, GumpButtonType.Reply, 0); // Strength potion
            AddItem(201, 163, 0xF09);
            AddLevel(196, 163, system.StrengthPotion);

            AddImage(48, 47, 0xD2);
            AddLevel(54, 47, (int)m_Plant.PlantStatus);

            AddImage(232, 47, 0xD2);
            AddGrowthIndicator(239, 47);

            AddButton(48, 183, 0xD2, 0xD2, 11, GumpButtonType.Reply, 0); // Help
            AddLabel(54, 183, 0x835, "?");

            AddButton(232, 183, 0xD4, 0xD4, 12, GumpButtonType.Reply, 0); // Empty the bowl
            AddItem(219, 180, 0x15FD);
        }

        private void DrawBackground()
        {
            AddBackground(50, 50, 200, 150, 0xE10);

            AddItem(45, 45, 0xCEF);
            AddItem(45, 118, 0xCF0);

            AddItem(211, 45, 0xCEB);
            AddItem(211, 118, 0xCEC);
        }

        private void DrawPlant()
        {
            PlantStatus status = m_Plant.PlantStatus;

            if (status < PlantStatus.FullGrownPlant)
            {
                AddImage(110, 85, 0x589);

                AddItem(122, 94, 0x914);
                AddItem(135, 94, 0x914);
                AddItem(120, 112, 0x914);
                AddItem(135, 112, 0x914);

                if (status >= PlantStatus.Stage2)
                {
                    AddItem(127, 112, 0xC62);
                }
                if (status == PlantStatus.Stage3 || status == PlantStatus.Stage4)
                {
                    AddItem(129, 85, 0xC7E);
                }
                if (status >= PlantStatus.Stage4)
                {
                    AddItem(121, 117, 0xC62);
                    AddItem(133, 117, 0xC62);
                }
                if (status >= PlantStatus.Stage5)
                {
                    AddItem(110, 100, 0xC62);
                    AddItem(140, 100, 0xC62);
                    AddItem(110, 130, 0xC62);
                    AddItem(140, 130, 0xC62);
                }
                if (status >= PlantStatus.Stage6)
                {
                    AddItem(105, 115, 0xC62);
                    AddItem(145, 115, 0xC62);
                    AddItem(125, 90, 0xC62);
                    AddItem(125, 135, 0xC62);
                }
            }
            else
            {
                PlantTypeInfo typeInfo = PlantTypeInfo.GetInfo(m_Plant.PlantType);
                PlantHueInfo hueInfo = PlantHueInfo.GetInfo(m_Plant.PlantHue);

                // The large images for these trees trigger a client crash, so use a smaller, generic tree.
                if (m_Plant.PlantType == PlantType.CypressTwisted || m_Plant.PlantType == PlantType.CypressStraight)
                    AddItem(130 + typeInfo.OffsetX, 96 + typeInfo.OffsetY, 0x0CCA, hueInfo.Hue);
                else
                    AddItem(130 + typeInfo.OffsetX, 96 + typeInfo.OffsetY, typeInfo.ItemID, hueInfo.Hue);
            }

            if (status != PlantStatus.BowlOfDirt)
            {
                int message = m_Plant.PlantSystem.GetLocalizedHealth();

                switch (m_Plant.PlantSystem.Health)
                {
                    case PlantHealth.Dying:
                        {
                            AddItem(92, 167, 0x1B9D);
                            AddItem(161, 167, 0x1B9D);

                            AddHtmlLocalized(136, 167, 42, 20, message, 0x00FC00, false, false);

                            break;
                        }
                    case PlantHealth.Wilted:
                        {
                            AddItem(91, 164, 0x18E6);
                            AddItem(161, 164, 0x18E6);

                            AddHtmlLocalized(132, 167, 42, 20, message, 0x00C207, false, false);

                            break;
                        }
                    case PlantHealth.Healthy:
                        {
                            AddItem(96, 168, 0xC61);
                            AddItem(162, 168, 0xC61);

                            AddHtmlLocalized(129, 167, 42, 20, message, 0x008200, false, false);

                            break;
                        }
                    case PlantHealth.Vibrant:
                        {
                            AddItem(93, 162, 0x1A99);
                            AddItem(162, 162, 0x1A99);

                            AddHtmlLocalized(129, 167, 42, 20, message, 0x0083E0, false, false);

                            break;
                        }
                }
            }
        }

        private void AddPlus(int x, int y, int value)
        {
            switch (value)
            {
                case 1: AddLabel(x, y, 0x35, "+"); break;
                case 2: AddLabel(x, y, 0x21, "+"); break;
            }
        }

        private void AddPlusMinus(int x, int y, int value)
        {
            switch (value)
            {
                case 0: AddLabel(x, y, 0x21, "-"); break;
                case 1: AddLabel(x, y, 0x35, "-"); break;
                case 3: AddLabel(x, y, 0x35, "+"); break;
                case 4: AddLabel(x, y, 0x21, "+"); break;
            }
        }

        private void AddLevel(int x, int y, int value)
        {
            AddLabel(x, y, 0x835, value.ToString());
        }

        private void AddGrowthIndicator(int x, int y)
        {
            if (!m_Plant.IsGrowable)
                return;

            switch (m_Plant.PlantSystem.GrowthIndicator)
            {
                case PlantGrowthIndicator.InvalidLocation: AddLabel(x, y, 0x21, "!"); break;
                case PlantGrowthIndicator.NotHealthy: AddLabel(x, y, 0x21, "-"); break;
                case PlantGrowthIndicator.Delay: AddLabel(x, y, 0x35, "-"); break;
                case PlantGrowthIndicator.Grown: AddLabel(x, y, 0x3, "+"); break;
                case PlantGrowthIndicator.DoubleGrown: AddLabel(x, y, 0x3F, "+"); break;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus >= PlantStatus.DecorativePlant)
                return;

            if (((info.ButtonID >= 6 && info.ButtonID <= 10) || info.ButtonID == 12) && !from.InRange(m_Plant.GetWorldLocation(), 3))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
                return;
            }

            if (!m_Plant.IsUsableBy(from))
            {
                m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // Reproduction menu
                    {
                        if (m_Plant.PlantStatus > PlantStatus.BowlOfDirt)
                        {
                            from.SendGump(new ReproductionGump(m_Plant));
                        }
                        else
                        {
                            from.SendLocalizedMessage(1061885); // You need to plant a seed in the bowl first.

                            from.SendGump(new MainPlantGump(m_Plant));
                        }

                        break;
                    }
                case 2: // Infestation
                    {
                        from.Send(new DisplayHelpTopic(54, true)); // INFESTATION LEVEL

                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 3: // Fungus
                    {
                        from.Send(new DisplayHelpTopic(56, true)); // FUNGUS LEVEL

                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 4: // Poison
                    {
                        from.Send(new DisplayHelpTopic(58, true)); // POISON LEVEL

                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 5: // Disease
                    {
                        from.Send(new DisplayHelpTopic(60, true)); // DISEASE LEVEL

                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 6: // Water
                    {
                        Item[] item = from.Backpack.FindItemsByType(typeof(BaseBeverage));

                        bool foundUsableWater = false;

                        if (item != null && item.Length > 0)
                        {
                            for (int i = 0; i < item.Length; ++i)
                            {
                                BaseBeverage beverage = (BaseBeverage)item[i];

                                if (!beverage.IsEmpty && beverage.Pourable && beverage.Content == BeverageType.Water)
                                {
                                    foundUsableWater = true;
                                    m_Plant.Pour(from, beverage);
                                    break;
                                }
                            }
                        }

                        if (!foundUsableWater)
                        {
                            from.Target = new PlantPourTarget(m_Plant);
                            from.SendLocalizedMessage(1060808, "#" + m_Plant.GetLocalizedPlantStatus().ToString()); // Target the container you wish to use to water the ~1_val~.
                        }

                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 7: // Poison potion
                    {
                        AddPotion(from, PotionEffect.PoisonGreater, PotionEffect.PoisonDeadly);

                        break;
                    }
                case 8: // Cure potion
                    {
                        AddPotion(from, PotionEffect.CureGreater);

                        break;
                    }
                case 9: // Heal potion
                    {
                        AddPotion(from, PotionEffect.HealGreater);

                        break;
                    }
                case 10: // Strength potion
                    {
                        AddPotion(from, PotionEffect.StrengthGreater);

                        break;
                    }
                case 11: // Help
                    {
                        from.Send(new DisplayHelpTopic(48, true)); // PLANT GROWING

                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 12: // Empty the bowl
                    {
                        from.SendGump(new EmptyTheBowlGump(m_Plant));

                        break;
                    }
            }
        }

        private void AddPotion(Mobile from, params PotionEffect[] effects)
        {
            Item item = GetPotion(from, effects);

            if (item != null)
            {
                m_Plant.Pour(from, item);
            }
            else
            {
                int message;
                if (m_Plant.ApplyPotion(effects[0], true, out message))
                {
                    from.SendLocalizedMessage(1061884); // You don't have any strong potions of that type in your pack.

                    from.Target = new PlantPourTarget(m_Plant);
                    from.SendLocalizedMessage(1060808, "#" + m_Plant.GetLocalizedPlantStatus().ToString()); // Target the container you wish to use to water the ~1_val~.

                    return;
                }
                else
                {
                    m_Plant.LabelTo(from, message);
                }
            }

            from.SendGump(new MainPlantGump(m_Plant));
        }

        public static Item GetPotion(Mobile from, PotionEffect[] effects)
        {
            if (from.Backpack == null)
                return null;

            Item[] items = from.Backpack.FindItemsByType(new Type[] { typeof(BasePotion), typeof(PotionKeg) });

            foreach (Item item in items)
            {
                if (item is BasePotion)
                {
                    BasePotion potion = (BasePotion)item;

                    if (Array.IndexOf(effects, potion.PotionEffect) >= 0)
                        return potion;
                }
                else
                {
                    PotionKeg keg = (PotionKeg)item;

                    if (keg.Held > 0 && Array.IndexOf(effects, keg.Type) >= 0)
                        return keg;
                }
            }

            return null;
        }
    }

    public class ReproductionGump : Gump
    {
        private PlantItem m_Plant;

        public ReproductionGump(PlantItem plant)
            : base(20, 20)
        {
            m_Plant = plant;

            DrawBackground();

            AddButton(70, 67, 0xD4, 0xD4, 1, GumpButtonType.Reply, 0); // Main menu
            AddItem(57, 65, 0x1600);

            AddLabel(108, 67, 0x835, "Reproduction");

            if (m_Plant.PlantStatus == PlantStatus.Stage9)
            {
                AddButton(212, 67, 0xD4, 0xD4, 2, GumpButtonType.Reply, 0); // Set to decorative
                AddItem(202, 68, 0xC61);
                AddLabel(216, 66, 0x21, "/");
            }

            AddButton(80, 116, 0xD4, 0xD4, 3, GumpButtonType.Reply, 0); // Pollination
            AddItem(66, 117, 0x1AA2);
            AddPollinationState(106, 116);

            AddButton(128, 116, 0xD4, 0xD4, 4, GumpButtonType.Reply, 0); // Resources
            AddItem(113, 120, 0x1021);
            AddResourcesState(149, 116);

            AddButton(177, 116, 0xD4, 0xD4, 5, GumpButtonType.Reply, 0); // Seeds
            AddItem(160, 121, 0xDCF);
            AddSeedsState(199, 116);

            AddButton(70, 163, 0xD2, 0xD2, 6, GumpButtonType.Reply, 0); // Gather pollen
            AddItem(56, 164, 0x1AA2);

            AddButton(138, 163, 0xD2, 0xD2, 7, GumpButtonType.Reply, 0); // Gather resources
            AddItem(123, 167, 0x1021);

            AddButton(212, 163, 0xD2, 0xD2, 8, GumpButtonType.Reply, 0); // Gather seeds
            AddItem(195, 168, 0xDCF);
        }

        private void DrawBackground()
        {
            AddBackground(50, 50, 200, 150, 0xE10);

            AddImage(60, 90, 0xE17);
            AddImage(120, 90, 0xE17);

            AddImage(60, 145, 0xE17);
            AddImage(120, 145, 0xE17);

            AddItem(45, 45, 0xCEF);
            AddItem(45, 118, 0xCF0);

            AddItem(211, 45, 0xCEB);
            AddItem(211, 118, 0xCEC);
        }

        private void AddPollinationState(int x, int y)
        {
            PlantSystem system = m_Plant.PlantSystem;

            if (!system.PollenProducing)
                AddLabel(x, y, 0x35, "-");
            else if (!system.Pollinated)
                AddLabel(x, y, 0x21, "!");
            else
                AddLabel(x, y, 0x3F, "+");
        }

        private void AddResourcesState(int x, int y)
        {
            PlantResourceInfo resInfo = PlantResourceInfo.GetInfo(m_Plant.PlantType, m_Plant.PlantHue);

            PlantSystem system = m_Plant.PlantSystem;
            int totalResources = system.AvailableResources + system.LeftResources;

            if (resInfo == null || totalResources == 0)
            {
                AddLabel(x + 5, y, 0x21, "X");
            }
            else
            {
                AddLabel(x, y, PlantHueInfo.GetInfo(m_Plant.PlantHue).GumpHue,
                    string.Format("{0}/{1}", system.AvailableResources, totalResources));
            }
        }

        private void AddSeedsState(int x, int y)
        {
            PlantSystem system = m_Plant.PlantSystem;
            int totalSeeds = system.AvailableSeeds + system.LeftSeeds;

            if (!m_Plant.Reproduces || totalSeeds == 0)
            {
                AddLabel(x + 5, y, 0x21, "X");
            }
            else
            {
                AddLabel(x, y, PlantHueInfo.GetInfo(system.SeedHue).GumpHue,
                    string.Format("{0}/{1}", system.AvailableSeeds, totalSeeds));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus >= PlantStatus.DecorativePlant || m_Plant.PlantStatus == PlantStatus.BowlOfDirt)
                return;

            if ((info.ButtonID >= 6 && info.ButtonID <= 8) && !from.InRange(m_Plant.GetWorldLocation(), 3))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
                return;
            }

            if (!m_Plant.IsUsableBy(from))
            {
                m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // Main menu
                    {
                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 2: // Set to decorative
                    {
                        if (m_Plant.PlantStatus == PlantStatus.Stage9)
                        {
                            from.SendGump(new SetToDecorativeGump(m_Plant));
                        }

                        break;
                    }
                case 3: // Pollination
                    {
                        from.Send(new DisplayHelpTopic(67, true)); // POLLINATION STATE

                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
                case 4: // Resources
                    {
                        from.Send(new DisplayHelpTopic(69, true)); // RESOURCE PRODUCTION

                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
                case 5: // Seeds
                    {
                        from.Send(new DisplayHelpTopic(68, true)); // SEED PRODUCTION

                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
                case 6: // Gather pollen
                    {
                        if (!m_Plant.IsCrossable)
                        {
                            m_Plant.LabelTo(from, 1053050); // You cannot gather pollen from a mutated plant!
                        }
                        else if (!m_Plant.PlantSystem.PollenProducing)
                        {
                            m_Plant.LabelTo(from, 1053051); // You cannot gather pollen from a plant in this stage of development!
                        }
                        else if (m_Plant.PlantSystem.Health < PlantHealth.Healthy)
                        {
                            m_Plant.LabelTo(from, 1053052); // You cannot gather pollen from an unhealthy plant!
                        }
                        else
                        {
                            from.Target = new PollinateTarget(m_Plant);
                            from.SendLocalizedMessage(1053054); // Target the plant you wish to cross-pollinate to.

                            break;
                        }

                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
                case 7: // Gather resources
                    {
                        PlantResourceInfo resInfo = PlantResourceInfo.GetInfo(m_Plant.PlantType, m_Plant.PlantHue);
                        PlantSystem system = m_Plant.PlantSystem;

                        if (resInfo == null)
                        {
                            if (m_Plant.IsCrossable)
                                m_Plant.LabelTo(from, 1053056); // This plant has no resources to gather!
                            else
                                m_Plant.LabelTo(from, 1053055); // Mutated plants do not produce resources!
                        }
                        else if (system.AvailableResources == 0)
                        {
                            m_Plant.LabelTo(from, 1053056); // This plant has no resources to gather!
                        }
                        else
                        {
                            Item resource = resInfo.CreateResource();

                            if (from.PlaceInBackpack(resource))
                            {
                                system.AvailableResources--;
                                m_Plant.LabelTo(from, 1053059); // You gather resources from the plant.
                            }
                            else
                            {
                                resource.Delete();
                                m_Plant.LabelTo(from, 1053058); // You attempt to gather as many resources as you can hold, but your backpack is full.
                            }
                        }

                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
                case 8: // Gather seeds
                    {
                        PlantSystem system = m_Plant.PlantSystem;

                        if (!m_Plant.Reproduces)
                        {
                            m_Plant.LabelTo(from, 1053060); // Mutated plants do not produce seeds!
                        }
                        else if (system.AvailableSeeds == 0)
                        {
                            m_Plant.LabelTo(from, 1053061); // This plant has no seeds to gather!
                        }
                        else
                        {
                            Seed seed = new Seed(system.SeedType, system.SeedHue, true);

                            if (from.PlaceInBackpack(seed))
                            {
                                system.AvailableSeeds--;
                                m_Plant.LabelTo(from, 1053063); // You gather seeds from the plant.
                            }
                            else
                            {
                                seed.Delete();
                                m_Plant.LabelTo(from, 1053062); // You attempt to gather as many seeds as you can hold, but your backpack is full.
                            }
                        }

                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
            }
        }
    }

    public enum PlantType
    {
        CampionFlowers,
        Poppies,
        Snowdrops,
        Bulrushes,
        Lilies,
        PampasGrass,
        Rushes,
        ElephantEarPlant,
        Fern,
        PonytailPalm,
        SmallPalm,
        CenturyPlant,
        WaterPlant,
        SnakePlant,
        PricklyPearCactus,
        BarrelCactus,
        TribarrelCactus,
        CommonGreenBonsai,
        CommonPinkBonsai,
        UncommonGreenBonsai,
        UncommonPinkBonsai,
        RareGreenBonsai,
        RarePinkBonsai,
        ExceptionalBonsai,
        ExoticBonsai,
        Cactus,
        FlaxFlowers,
        FoxgloveFlowers,
        HopsEast,
        OrfluerFlowers,
        CypressTwisted,
        HedgeShort,
        JuniperBush,
        SnowdropPatch,
        Cattails,
        PoppyPatch,
        SpiderTree,
        WaterLily,
        CypressStraight,
        HedgeTall,
        HopsSouth,
        SugarCanes,
        CocoaTree
    }

    public enum PlantCategory
    {
        Default,
        Common = 1063335, //
        Uncommon = 1063336, //
        Rare = 1063337, // Bonsai
        Exceptional = 1063341, //
        Exotic = 1063342, //
        Peculiar = 1080528,
        Fragrant = 1080529
    }

    public class PlantTypeInfo
    {
        private static PlantTypeInfo[] m_Table = new PlantTypeInfo[]
		{
			new PlantTypeInfo( 0xC83, 0, 0,			PlantType.CampionFlowers,		false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC86, 0, 0,			PlantType.Poppies,				false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC88, 0, 10,		PlantType.Snowdrops,			false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC94, -15, 0,		PlantType.Bulrushes,			false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC8B, 0, 0,			PlantType.Lilies,				false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xCA5, -8, 0,		PlantType.PampasGrass,			false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xCA7, -10, 0,		PlantType.Rushes,				false, true, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC97, -20, 0,		PlantType.ElephantEarPlant,		true, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC9F, -20, 0,		PlantType.Fern,					false, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xCA6, -16, -5,		PlantType.PonytailPalm,			false, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xC9C, -5, -10,		PlantType.SmallPalm,			false, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xD31, 0, -27,		PlantType.CenturyPlant,			true, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xD04, 0, 10,		PlantType.WaterPlant,			true, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xCA9, 0, 0,			PlantType.SnakePlant,			true, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xD2C, 0, 10,		PlantType.PricklyPearCactus,	false, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xD26, 0, 10,		PlantType.BarrelCactus,			false, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0xD27, 0, 10,		PlantType.TribarrelCactus,		false, false, true, true,		PlantCategory.Default ),
			new PlantTypeInfo( 0x28DC, -5, 5,		PlantType.CommonGreenBonsai,	true, false, false, false,		PlantCategory.Common ),
			new PlantTypeInfo( 0x28DF, -5, 5,		PlantType.CommonPinkBonsai,		true, false, false, false,		PlantCategory.Common ),
			new PlantTypeInfo( 0x28DD, -5, 5,		PlantType.UncommonGreenBonsai,	true, false, false, false,		PlantCategory.Uncommon ),
			new PlantTypeInfo( 0x28E0, -5, 5,		PlantType.UncommonPinkBonsai,	true, false, false, false,		PlantCategory.Uncommon ),
			new PlantTypeInfo( 0x28DE, -5, 5,		PlantType.RareGreenBonsai,		true, false, false, false,		PlantCategory.Rare ),
			new PlantTypeInfo( 0x28E1, -5, 5,		PlantType.RarePinkBonsai,		true, false, false, false,		PlantCategory.Rare ),
			new PlantTypeInfo( 0x28E2, -5, 5,		PlantType.ExceptionalBonsai,	true, false, false, false,		PlantCategory.Exceptional ),
			new PlantTypeInfo( 0x28E3, -5, 5,		PlantType.ExoticBonsai,			true, false, false, false,		PlantCategory.Exotic ),
			new PlantTypeInfo( 0x0D25, 0, 0,		PlantType.Cactus,				false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x1A9A, 5, 10,		PlantType.FlaxFlowers,			false, true, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0C84, 0, 0,		PlantType.FoxgloveFlowers,		false, true, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x1A9F, 5, -25,		PlantType.HopsEast,				false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CC1, 0, 0,		PlantType.OrfluerFlowers,		false, true, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CFE, -45, -30,	PlantType.CypressTwisted,		false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0C8F, 0, 0,		PlantType.HedgeShort,			false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CC8, 0, 0,		PlantType.JuniperBush,			true, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0C8E, -20, 0,		PlantType.SnowdropPatch,		false, true, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CB7, 0, 0,		PlantType.Cattails,				false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CBE, -20, 0,		PlantType.PoppyPatch,			false, true, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CC9, 0, 0,		PlantType.SpiderTree,			false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0DC1, -5, 15,		PlantType.WaterLily,			false, true, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0CFB, -45, -30,	PlantType.CypressStraight,		false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x0DB8, 0, -20,		PlantType.HedgeTall,			false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x1AA1, 10, -25,		PlantType.HopsSouth,			false, false, false, false,		PlantCategory.Peculiar ),
			new PlantTypeInfo( 0x246C, -25, -20,	PlantType.SugarCanes,			false, false, false, false,		PlantCategory.Peculiar,		1114898, 1114898, 1094702, 1094703, 1095221, 1113715 ),
			new PlantTypeInfo( 0xC9E, -40, -30,		PlantType.CocoaTree,			false, false, false, true,		PlantCategory.Fragrant,		1080536, 1080536, 1080534, 1080531, 1080533, 1113716 )
		};

        public static PlantTypeInfo GetInfo(PlantType plantType)
        {
            int index = (int)plantType;

            if (index >= 0 && index < m_Table.Length)
                return m_Table[index];
            else
                return m_Table[0];
        }

        public static PlantType RandomFirstGeneration()
        {
            switch (Utility.Random(3))
            {
                case 0: return PlantType.CampionFlowers;
                case 1: return PlantType.Fern;
                default: return PlantType.TribarrelCactus;
            }
        }

        public static PlantType RandomPeculiarGroupOne()
        {
            switch (Utility.Random(6))
            {
                case 0: return PlantType.Cactus;
                case 1: return PlantType.FlaxFlowers;
                case 2: return PlantType.FoxgloveFlowers;
                case 3: return PlantType.HopsEast;
                case 4: return PlantType.CocoaTree;
                default: return PlantType.OrfluerFlowers;
            }
        }

        public static PlantType RandomPeculiarGroupTwo()
        {
            switch (Utility.Random(5))
            {
                case 0: return PlantType.CypressTwisted;
                case 1: return PlantType.HedgeShort;
                case 2: return PlantType.JuniperBush;
                case 3: return PlantType.CocoaTree;
                default: return PlantType.SnowdropPatch;
            }
        }

        public static PlantType RandomPeculiarGroupThree()
        {
            switch (Utility.Random(5))
            {
                case 0: return PlantType.Cattails;
                case 1: return PlantType.PoppyPatch;
                case 2: return PlantType.SpiderTree;
                case 3: return PlantType.CocoaTree;
                default: return PlantType.WaterLily;
            }
        }

        public static PlantType RandomPeculiarGroupFour()
        {
            switch (Utility.Random(5))
            {
                case 0: return PlantType.CypressStraight;
                case 1: return PlantType.HedgeTall;
                case 2: return PlantType.HopsSouth;
                case 3: return PlantType.CocoaTree;
                default: return PlantType.SugarCanes;
            }
        }

        public static PlantType RandomBonsai(double increaseRatio)
        {
            /* Chances of each plant type are equal to the chances of the previous plant type * increaseRatio:
             * E.g.:
             *  chances_of_uncommon = chances_of_common * increaseRatio
             *  chances_of_rare = chances_of_uncommon * increaseRatio
             *  ...
             *
             * If increaseRatio < 1 -> rare plants are actually rarer than the others
             * If increaseRatio > 1 -> rare plants are actually more common than the others (it might be the case with certain monsters)
             *
             * If a plant type (common, uncommon, ...) has 2 different colors, they have the same chances:
             *  chances_of_green_common = chances_of_pink_common = chances_of_common / 2
             *  ...
             */

            double k1 = increaseRatio >= 0.0 ? increaseRatio : 0.0;
            double k2 = k1 * k1;
            double k3 = k2 * k1;
            double k4 = k3 * k1;

            double exp1 = k1 + 1.0;
            double exp2 = k2 + exp1;
            double exp3 = k3 + exp2;
            double exp4 = k4 + exp3;

            double rand = Utility.RandomDouble();

            if (rand < 0.5 / exp4)
                return PlantType.CommonGreenBonsai;
            else if (rand < 1.0 / exp4)
                return PlantType.CommonPinkBonsai;
            else if (rand < (k1 * 0.5 + 1.0) / exp4)
                return PlantType.UncommonGreenBonsai;
            else if (rand < exp1 / exp4)
                return PlantType.UncommonPinkBonsai;
            else if (rand < (k2 * 0.5 + exp1) / exp4)
                return PlantType.RareGreenBonsai;
            else if (rand < exp2 / exp4)
                return PlantType.RarePinkBonsai;
            else if (rand < exp3 / exp4)
                return PlantType.ExceptionalBonsai;
            else
                return PlantType.ExoticBonsai;
        }

        public static bool IsCrossable(PlantType plantType)
        {
            return GetInfo(plantType).Crossable;
        }

        public static PlantType Cross(PlantType first, PlantType second)
        {
            if (!IsCrossable(first) || !IsCrossable(second))
                return PlantType.CampionFlowers;

            int firstIndex = (int)first;
            int secondIndex = (int)second;

            if (firstIndex + 1 == secondIndex || firstIndex == secondIndex + 1)
                return Utility.RandomBool() ? first : second;
            else
                return (PlantType)((firstIndex + secondIndex) / 2);
        }

        public static bool CanReproduce(PlantType plantType)
        {
            return GetInfo(plantType).Reproduces;
        }

        public int GetPlantLabelSeed(PlantHueInfo hueInfo)
        {
            if (m_PlantLabelSeed != -1)
                return m_PlantLabelSeed;

            return hueInfo.IsBright() ? 1061887 : 1061888; // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~ ~6_val~
        }

        public int GetPlantLabelPlant(PlantHueInfo hueInfo)
        {
            if (m_PlantLabelPlant != -1)
                return m_PlantLabelPlant;

            if (m_ContainsPlant)
                return hueInfo.IsBright() ? 1060832 : 1060831; // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~
            else
                return hueInfo.IsBright() ? 1061887 : 1061888; // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~ ~6_val~
        }

        public int GetPlantLabelFullGrown(PlantHueInfo hueInfo)
        {
            if (m_PlantLabelFullGrown != -1)
                return m_PlantLabelFullGrown;

            if (m_ContainsPlant)
                return hueInfo.IsBright() ? 1061891 : 1061889; // a ~1_HEALTH~ [bright] ~2_COLOR~ ~3_NAME~
            else
                return hueInfo.IsBright() ? 1061892 : 1061890; // a ~1_HEALTH~ [bright] ~2_COLOR~ ~3_NAME~ plant
        }

        public int GetPlantLabelDecorative(PlantHueInfo hueInfo)
        {
            if (m_PlantLabelDecorative != -1)
                return m_PlantLabelDecorative;

            return hueInfo.IsBright() ? 1074267 : 1070973; // a decorative [bright] ~1_COLOR~ ~2_TYPE~
        }

        public int GetSeedLabel(PlantHueInfo hueInfo)
        {
            if (m_SeedLabel != -1)
                return m_SeedLabel;

            return hueInfo.IsBright() ? 1061918 : 1061917; // [bright] ~1_COLOR~ ~2_TYPE~ seed
        }

        public int GetSeedLabelPlural(PlantHueInfo hueInfo)
        {
            if (m_SeedLabelPlural != -1)
                return m_SeedLabelPlural;

            return hueInfo.IsBright() ? 1113493 : 1113492; // ~1_amount~ [bright] ~2_color~ ~3_type~ seeds
        }

        private int m_ItemID;
        private int m_OffsetX;
        private int m_OffsetY;
        private PlantType m_PlantType;
        private bool m_ContainsPlant;
        private bool m_Flowery;
        private bool m_Crossable;
        private bool m_Reproduces;
        private PlantCategory m_PlantCategory;

        // Cliloc overrides
        private int m_PlantLabelSeed;
        private int m_PlantLabelPlant;
        private int m_PlantLabelFullGrown;
        private int m_PlantLabelDecorative;
        private int m_SeedLabel;
        private int m_SeedLabelPlural;

        public int ItemID { get { return m_ItemID; } }
        public int OffsetX { get { return m_OffsetX; } }
        public int OffsetY { get { return m_OffsetY; } }
        public PlantType PlantType { get { return m_PlantType; } }
        public PlantCategory PlantCategory { get { return m_PlantCategory; } }
        public int Name { get { return (m_ItemID < 0x4000) ? 1020000 + m_ItemID : 1078872 + m_ItemID; } }

        public bool ContainsPlant { get { return m_ContainsPlant; } }
        public bool Flowery { get { return m_Flowery; } }
        public bool Crossable { get { return m_Crossable; } }
        public bool Reproduces { get { return m_Reproduces; } }

        private PlantTypeInfo(int itemID, int offsetX, int offsetY, PlantType plantType, bool containsPlant, bool flowery, bool crossable, bool reproduces, PlantCategory plantCategory)
            : this(itemID, offsetX, offsetY, plantType, containsPlant, flowery, crossable, reproduces, plantCategory, -1, -1, -1, -1, -1, -1)
        {
        }

        private PlantTypeInfo(int itemID, int offsetX, int offsetY, PlantType plantType, bool containsPlant, bool flowery, bool crossable, bool reproduces, PlantCategory plantCategory, int plantLabelSeed, int plantLabelPlant, int plantLabelFullGrown, int plantLabelDecorative, int seedLabel, int seedLabelPlural)
        {
            m_ItemID = itemID;
            m_OffsetX = offsetX;
            m_OffsetY = offsetY;
            m_PlantType = plantType;
            m_ContainsPlant = containsPlant;
            m_Flowery = flowery;
            m_Crossable = crossable;
            m_Reproduces = reproduces;
            m_PlantCategory = plantCategory;
            m_PlantLabelSeed = plantLabelSeed;
            m_PlantLabelPlant = plantLabelPlant;
            m_PlantLabelFullGrown = plantLabelFullGrown;
            m_PlantLabelDecorative = plantLabelDecorative;
            m_SeedLabel = seedLabel;
            m_SeedLabelPlural = seedLabelPlural;
        }
    }

    [Flags]
    public enum PlantHue
    {
        Plain = 0x1 | Crossable | Reproduces,

        Red = 0x2 | Crossable | Reproduces,
        Blue = 0x4 | Crossable | Reproduces,
        Yellow = 0x8 | Crossable | Reproduces,

        BrightRed = Red | Bright,
        BrightBlue = Blue | Bright,
        BrightYellow = Yellow | Bright,

        Purple = Red | Blue,
        Green = Blue | Yellow,
        Orange = Red | Yellow,

        BrightPurple = Purple | Bright,
        BrightGreen = Green | Bright,
        BrightOrange = Orange | Bright,

        Black = 0x10,
        White = 0x20,
        Pink = 0x40,
        Magenta = 0x80,
        Aqua = 0x100,
        FireRed = 0x200,

        None = 0,
        Reproduces = 0x2000000,
        Crossable = 0x4000000,
        Bright = 0x8000000
    }

    public class PlantHueInfo
    {
        private static Dictionary<PlantHue, PlantHueInfo> m_Table;

        static PlantHueInfo()
        {
            m_Table = new Dictionary<PlantHue, PlantHueInfo>();

            m_Table[PlantHue.Plain] = new PlantHueInfo(0, 1060813, PlantHue.Plain, 0x835);
            m_Table[PlantHue.Red] = new PlantHueInfo(0x66D, 1060814, PlantHue.Red, 0x24);
            m_Table[PlantHue.Blue] = new PlantHueInfo(0x53D, 1060815, PlantHue.Blue, 0x6);
            m_Table[PlantHue.Yellow] = new PlantHueInfo(0x8A5, 1060818, PlantHue.Yellow, 0x38);
            m_Table[PlantHue.BrightRed] = new PlantHueInfo(0x21, 1060814, PlantHue.BrightRed, 0x21);
            m_Table[PlantHue.BrightBlue] = new PlantHueInfo(0x5, 1060815, PlantHue.BrightBlue, 0x6);
            m_Table[PlantHue.BrightYellow] = new PlantHueInfo(0x38, 1060818, PlantHue.BrightYellow, 0x35);
            m_Table[PlantHue.Purple] = new PlantHueInfo(0xD, 1060816, PlantHue.Purple, 0x10);
            m_Table[PlantHue.Green] = new PlantHueInfo(0x59B, 1060819, PlantHue.Green, 0x42);
            m_Table[PlantHue.Orange] = new PlantHueInfo(0x46F, 1060817, PlantHue.Orange, 0x2E);
            m_Table[PlantHue.BrightPurple] = new PlantHueInfo(0x10, 1060816, PlantHue.BrightPurple, 0xD);
            m_Table[PlantHue.BrightGreen] = new PlantHueInfo(0x42, 1060819, PlantHue.BrightGreen, 0x3F);
            m_Table[PlantHue.BrightOrange] = new PlantHueInfo(0x2B, 1060817, PlantHue.BrightOrange, 0x2B);
            m_Table[PlantHue.Black] = new PlantHueInfo(0x455, 1060820, PlantHue.Black, 0);
            m_Table[PlantHue.White] = new PlantHueInfo(0x481, 1060821, PlantHue.White, 0x481);
            m_Table[PlantHue.Pink] = new PlantHueInfo(0x48E, 1061854, PlantHue.Pink);
            m_Table[PlantHue.Magenta] = new PlantHueInfo(0x486, 1061852, PlantHue.Magenta);
            m_Table[PlantHue.Aqua] = new PlantHueInfo(0x495, 1061853, PlantHue.Aqua);
            m_Table[PlantHue.FireRed] = new PlantHueInfo(0x489, 1061855, PlantHue.FireRed);
        }

        public static PlantHueInfo GetInfo(PlantHue plantHue)
        {
            PlantHueInfo info = null;

            if (m_Table.TryGetValue(plantHue, out info))
                return info;
            else
                return m_Table[PlantHue.Plain];
        }

        public static PlantHue RandomFirstGeneration()
        {
            switch (Utility.Random(4))
            {
                case 0: return PlantHue.Plain;
                case 1: return PlantHue.Red;
                case 2: return PlantHue.Blue;
                default: return PlantHue.Yellow;
            }
        }

        public static bool CanReproduce(PlantHue plantHue)
        {
            return (plantHue & PlantHue.Reproduces) != PlantHue.None;
        }

        public static bool IsCrossable(PlantHue plantHue)
        {
            return (plantHue & PlantHue.Crossable) != PlantHue.None;
        }

        public static bool IsBright(PlantHue plantHue)
        {
            return (plantHue & PlantHue.Bright) != PlantHue.None;
        }

        public static PlantHue GetNotBright(PlantHue plantHue)
        {
            return plantHue & ~PlantHue.Bright;
        }

        public static bool IsPrimary(PlantHue plantHue)
        {
            return plantHue == PlantHue.Red || plantHue == PlantHue.Blue || plantHue == PlantHue.Yellow;
        }

        public static PlantHue Cross(PlantHue first, PlantHue second)
        {
            if (!IsCrossable(first) || !IsCrossable(second))
                return PlantHue.None;

            if (Utility.RandomDouble() < 0.01)
                return Utility.RandomBool() ? PlantHue.Black : PlantHue.White;

            if (first == PlantHue.Plain || second == PlantHue.Plain)
                return PlantHue.Plain;

            PlantHue notBrightFirst = GetNotBright(first);
            PlantHue notBrightSecond = GetNotBright(second);

            if (notBrightFirst == notBrightSecond)
                return first | PlantHue.Bright;

            bool firstPrimary = IsPrimary(notBrightFirst);
            bool secondPrimary = IsPrimary(notBrightSecond);

            if (firstPrimary && secondPrimary)
                return notBrightFirst | notBrightSecond;

            if (firstPrimary && !secondPrimary)
                return notBrightFirst;

            if (!firstPrimary && secondPrimary)
                return notBrightSecond;

            return notBrightFirst & notBrightSecond;
        }

        private int m_Hue;
        private int m_Name;
        private PlantHue m_PlantHue;
        private int m_GumpHue;

        public int Hue { get { return m_Hue; } }
        public int Name { get { return m_Name; } }
        public PlantHue PlantHue { get { return m_PlantHue; } }
        public int GumpHue { get { return m_GumpHue; } }

        private PlantHueInfo(int hue, int name, PlantHue plantHue)
            : this(hue, name, plantHue, hue)
        {
        }

        private PlantHueInfo(int hue, int name, PlantHue plantHue, int gumpHue)
        {
            m_Hue = hue;
            m_Name = name;
            m_PlantHue = plantHue;
            m_GumpHue = gumpHue;
        }

        public bool IsCrossable()
        {
            return IsCrossable(m_PlantHue);
        }

        public bool IsBright()
        {
            return IsBright(m_PlantHue);
        }

        public PlantHue GetNotBright()
        {
            return GetNotBright(m_PlantHue);
        }

        public bool IsPrimary()
        {
            return IsPrimary(m_PlantHue);
        }
    }

    public enum PlantStatus
    {
        BowlOfDirt = 0,
        Seed = 1,
        Sapling = 2,
        Plant = 4,
        FullGrownPlant = 7,
        DecorativePlant = 10,
        DeadTwigs = 11,

        Stage1 = 1,
        Stage2 = 2,
        Stage3 = 3,
        Stage4 = 4,
        Stage5 = 5,
        Stage6 = 6,
        Stage7 = 7,
        Stage8 = 8,
        Stage9 = 9
    }

    public class PlantItem : Item, ISecurable
    {
        /*
         * Clients 7.0.12.0+ expect a container type in the plant label.
         * To support older (and only older) clients, change this to false.
         */
        private static readonly bool ShowContainerType = true;

        private PlantSystem m_PlantSystem;

        private PlantStatus m_PlantStatus;
        private PlantType m_PlantType;
        private PlantHue m_PlantHue;
        private bool m_ShowType;

        private SecureLevel m_Level;

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        public PlantSystem PlantSystem { get { return m_PlantSystem; } }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

        public override void OnSingleClick(Mobile from)
        {
            if (m_PlantStatus >= PlantStatus.DeadTwigs)
                LabelTo(from, LabelNumber);
            else if (m_PlantStatus >= PlantStatus.DecorativePlant)
                LabelTo(from, 1061924); // a decorative plant
            else if (m_PlantStatus >= PlantStatus.FullGrownPlant)
                LabelTo(from, PlantTypeInfo.GetInfo(m_PlantType).Name);
            else
                LabelTo(from, 1029913); // plant bowl
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlantStatus PlantStatus
        {
            get { return m_PlantStatus; }
            set
            {
                if (m_PlantStatus == value || value < PlantStatus.BowlOfDirt || value > PlantStatus.DeadTwigs)
                    return;

                double ratio;
                if (m_PlantSystem != null)
                    ratio = (double)m_PlantSystem.Hits / m_PlantSystem.MaxHits;
                else
                    ratio = 1.0;

                m_PlantStatus = value;

                if (m_PlantStatus >= PlantStatus.DecorativePlant)
                {
                    m_PlantSystem = null;
                }
                else
                {
                    if (m_PlantSystem == null)
                        m_PlantSystem = new PlantSystem(this, false);

                    int hits = (int)(m_PlantSystem.MaxHits * ratio);

                    if (hits == 0 && m_PlantStatus > PlantStatus.BowlOfDirt)
                        m_PlantSystem.Hits = hits + 1;
                    else
                        m_PlantSystem.Hits = hits;
                }

                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlantType PlantType
        {
            get { return m_PlantType; }
            set
            {
                m_PlantType = value;
                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlantHue PlantHue
        {
            get { return m_PlantHue; }
            set
            {
                m_PlantHue = value;
                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ShowType
        {
            get { return m_ShowType; }
            set
            {
                m_ShowType = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ValidGrowthLocation
        {
            get
            {
                if (IsLockedDown && RootParent == null)
                    return true;


                Mobile owner = RootParent as Mobile;
                if (owner == null)
                    return false;

                if (owner.Backpack != null && IsChildOf(owner.Backpack))
                    return true;

                BankBox bank = owner.FindBankNoCreate();
                if (bank != null && IsChildOf(bank))
                    return true;

                return false;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsGrowable
        {
            get { return m_PlantStatus >= PlantStatus.BowlOfDirt && m_PlantStatus <= PlantStatus.Stage9; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsCrossable
        {
            get { return PlantHueInfo.IsCrossable(this.PlantHue) && PlantTypeInfo.IsCrossable(this.PlantType); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Reproduces
        {
            get { return PlantHueInfo.CanReproduce(this.PlantHue) && PlantTypeInfo.CanReproduce(this.PlantType); }
        }

        private static ArrayList m_Instances = new ArrayList();

        public static ArrayList Plants { get { return m_Instances; } }

        [Constructable]
        public PlantItem()
            : this(false)
        {
        }

        [Constructable]
        public PlantItem(bool fertileDirt)
            : base(0x1602)
        {
            Weight = 1.0;

            m_PlantStatus = PlantStatus.BowlOfDirt;
            m_PlantSystem = new PlantSystem(this, fertileDirt);
            m_Level = SecureLevel.Owner;

            m_Instances.Add(this);
        }

        public PlantItem(Serial serial)
            : base(serial)
        {
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public int GetLocalizedPlantStatus()
        {
            if (m_PlantStatus >= PlantStatus.Plant)
                return 1060812; // plant
            else if (m_PlantStatus >= PlantStatus.Sapling)
                return 1023305; // sapling
            else if (m_PlantStatus >= PlantStatus.Seed)
                return 1060810; // seed
            else
                return 1026951; // dirt
        }

        public int GetLocalizedContainerType()
        {
            return 1150435; // bowl
        }

        private void Update()
        {
            if (m_PlantStatus >= PlantStatus.DeadTwigs)
            {
                ItemID = 0x1B9D;
                Hue = PlantHueInfo.GetInfo(m_PlantHue).Hue;
            }
            else if (m_PlantStatus >= PlantStatus.FullGrownPlant)
            {
                ItemID = PlantTypeInfo.GetInfo(m_PlantType).ItemID;
                Hue = PlantHueInfo.GetInfo(m_PlantHue).Hue;
            }
            else if (m_PlantStatus >= PlantStatus.Plant)
            {
                ItemID = 0x1600;
                Hue = 0;
            }
            else
            {
                ItemID = 0x1602;
                Hue = 0;
            }

            InvalidateProperties();
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (m_PlantStatus >= PlantStatus.DeadTwigs)
            {
                base.AddNameProperty(list);
            }
            else if (m_PlantStatus < PlantStatus.Seed)
            {
                string args;

                if (ShowContainerType)
                    args = String.Format("#{0}\t#{1}", GetLocalizedContainerType(), m_PlantSystem.GetLocalizedDirtStatus());
                else
                    args = String.Format("#{0}", m_PlantSystem.GetLocalizedDirtStatus());

                list.Add(1060830, args); // a ~1_val~ of ~2_val~ dirt
            }
            else
            {
                PlantTypeInfo typeInfo = PlantTypeInfo.GetInfo(m_PlantType);
                PlantHueInfo hueInfo = PlantHueInfo.GetInfo(m_PlantHue);

                if (m_PlantStatus >= PlantStatus.DecorativePlant)
                {
                    list.Add(typeInfo.GetPlantLabelDecorative(hueInfo), String.Format("#{0}\t#{1}", hueInfo.Name, typeInfo.Name));
                }
                else if (m_PlantStatus >= PlantStatus.FullGrownPlant)
                {
                    list.Add(typeInfo.GetPlantLabelFullGrown(hueInfo), String.Format("#{0}\t#{1}\t#{2}", m_PlantSystem.GetLocalizedHealth(), hueInfo.Name, typeInfo.Name));
                }
                else
                {
                    string args;

                    if (ShowContainerType)
                        args = String.Format("#{0}\t#{1}\t#{2}", GetLocalizedContainerType(), m_PlantSystem.GetLocalizedDirtStatus(), m_PlantSystem.GetLocalizedHealth());
                    else
                        args = String.Format("#{0}\t#{1}", m_PlantSystem.GetLocalizedDirtStatus(), m_PlantSystem.GetLocalizedHealth());

                    if (m_ShowType)
                    {
                        args += String.Format("\t#{0}\t#{1}\t#{2}", hueInfo.Name, typeInfo.Name, GetLocalizedPlantStatus());

                        if (m_PlantStatus == PlantStatus.Plant)
                            list.Add(typeInfo.GetPlantLabelPlant(hueInfo), args);
                        else
                            list.Add(typeInfo.GetPlantLabelSeed(hueInfo), args);
                    }
                    else
                    {
                        args += String.Format("\t#{0}\t#{1}", (typeInfo.PlantCategory == PlantCategory.Default) ? hueInfo.Name : (int)typeInfo.PlantCategory, GetLocalizedPlantStatus());

                        list.Add(hueInfo.IsBright() ? 1060832 : 1060831, args); // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~
                    }
                }
            }
        }

        public bool IsUsableBy(Mobile from)
        {
            Item root = RootParent as Item;
            return IsChildOf(from.Backpack) || IsChildOf(from.FindBankNoCreate()) || IsLockedDown && IsAccessibleTo(from) || root != null && root.IsSecure && root.IsAccessibleTo(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_PlantStatus >= PlantStatus.DecorativePlant)
                return;

            Point3D loc = this.GetWorldLocation();

            if (!from.InLOS(loc) || !from.InRange(loc, 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1019045); // I can't reach that.
                return;
            }

            if (!IsUsableBy(from))
            {
                LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                return;
            }

            from.SendGump(new MainPlantGump(this));
        }

        public void PlantSeed(Mobile from, Seed seed)
        {
            if (m_PlantStatus >= PlantStatus.FullGrownPlant)
            {
                LabelTo(from, 1061919); // You must use a seed on some prepared soil!
            }
            else if (!IsUsableBy(from))
            {
                LabelTo(from, 1061921); // The bowl of dirt must be in your pack, or you must lock it down.
            }
            else if (m_PlantStatus != PlantStatus.BowlOfDirt)
            {
                from.SendLocalizedMessage(1080389, "#" + GetLocalizedPlantStatus().ToString()); // This bowl of dirt already has a ~1_val~ in it!
            }
            else if (m_PlantSystem.Water < 2)
            {
                LabelTo(from, 1061920); // The dirt needs to be softened first.
            }
            else
            {
                m_PlantType = seed.PlantType;
                m_PlantHue = seed.PlantHue;
                m_ShowType = seed.ShowType;

                seed.Consume();

                PlantStatus = PlantStatus.Seed;

                m_PlantSystem.Reset(false);

                LabelTo(from, 1061922); // You plant the seed in the bowl of dirt.
            }
        }

        public void Die()
        {
            if (m_PlantStatus >= PlantStatus.FullGrownPlant)
            {
                PlantStatus = PlantStatus.DeadTwigs;
            }
            else
            {
                PlantStatus = PlantStatus.BowlOfDirt;
                m_PlantSystem.Reset(true);
            }
        }

        public void Pour(Mobile from, Item item)
        {
            if (m_PlantStatus >= PlantStatus.DeadTwigs)
                return;

            if (m_PlantStatus == PlantStatus.DecorativePlant)
            {
                LabelTo(from, 1053049); // This is a decorative plant, it does not need watering!
                return;
            }

            if (!IsUsableBy(from))
            {
                LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                return;
            }

            if (item is BaseBeverage)
            {
                BaseBeverage beverage = (BaseBeverage)item;

                if (beverage.IsEmpty || !beverage.Pourable || beverage.Content != BeverageType.Water)
                {
                    LabelTo(from, 1053069); // You can't use that on a plant!
                    return;
                }

                if (!beverage.ValidateUse(from, true))
                    return;

                beverage.Quantity--;
                m_PlantSystem.Water++;

                from.PlaySound(0x4E);
                LabelTo(from, 1061858); // You soften the dirt with water.
            }
            else if (item is BasePotion)
            {
                BasePotion potion = (BasePotion)item;

                int message;
                if (ApplyPotion(potion.PotionEffect, false, out message))
                {
                    potion.Consume();
                    from.PlaySound(0x240);
                    from.AddToBackpack(new Bottle());
                }
                LabelTo(from, message);
            }
            else if (item is PotionKeg)
            {
                PotionKeg keg = (PotionKeg)item;

                if (keg.Held <= 0)
                {
                    LabelTo(from, 1053069); // You can't use that on a plant!
                    return;
                }

                int message;
                if (ApplyPotion(keg.Type, false, out message))
                {
                    keg.Held--;
                    from.PlaySound(0x240);
                }
                LabelTo(from, message);
            }
            else
            {
                LabelTo(from, 1053069); // You can't use that on a plant!
            }
        }

        public bool ApplyPotion(PotionEffect effect, bool testOnly, out int message)
        {
            if (m_PlantStatus >= PlantStatus.DecorativePlant)
            {
                message = 1053049; // This is a decorative plant, it does not need watering!
                return false;
            }

            if (m_PlantStatus == PlantStatus.BowlOfDirt)
            {
                message = 1053066; // You should only pour potions on a plant or seed!
                return false;
            }

            bool full = false;

            if (effect == PotionEffect.PoisonGreater || effect == PotionEffect.PoisonDeadly)
            {
                if (m_PlantSystem.IsFullPoisonPotion)
                    full = true;
                else if (!testOnly)
                    m_PlantSystem.PoisonPotion++;
            }
            else if (effect == PotionEffect.CureGreater)
            {
                if (m_PlantSystem.IsFullCurePotion)
                    full = true;
                else if (!testOnly)
                    m_PlantSystem.CurePotion++;
            }
            else if (effect == PotionEffect.HealGreater)
            {
                if (m_PlantSystem.IsFullHealPotion)
                    full = true;
                else if (!testOnly)
                    m_PlantSystem.HealPotion++;
            }
            else if (effect == PotionEffect.StrengthGreater)
            {
                if (m_PlantSystem.IsFullStrengthPotion)
                    full = true;
                else if (!testOnly)
                    m_PlantSystem.StrengthPotion++;
            }
            else if (effect == PotionEffect.PoisonLesser || effect == PotionEffect.Poison || effect == PotionEffect.CureLesser || effect == PotionEffect.Cure ||
                effect == PotionEffect.HealLesser || effect == PotionEffect.Heal || effect == PotionEffect.Strength)
            {
                message = 1053068; // This potion is not powerful enough to use on a plant!
                return false;
            }
            else
            {
                message = 1053069; // You can't use that on a plant!
                return false;
            }

            if (full)
            {
                message = 1053065; // The plant is already soaked with this type of potion!
                return false;
            }
            else
            {
                message = 1053067; // You pour the potion over the plant.
                return true;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write((int)m_Level);

            writer.Write((int)m_PlantStatus);
            writer.Write((int)m_PlantType);
            writer.Write((int)m_PlantHue);
            writer.Write((bool)m_ShowType);

            if (m_PlantStatus < PlantStatus.DecorativePlant)
                m_PlantSystem.Save(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                case 1:
                    {
                        m_Level = (SecureLevel)reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 1)
                            m_Level = SecureLevel.CoOwners;

                        m_PlantStatus = (PlantStatus)reader.ReadInt();
                        m_PlantType = (PlantType)reader.ReadInt();
                        m_PlantHue = (PlantHue)reader.ReadInt();
                        m_ShowType = reader.ReadBool();

                        if (m_PlantStatus < PlantStatus.DecorativePlant)
                            m_PlantSystem = new PlantSystem(this, reader);

                        if (version < 2 && PlantHueInfo.IsCrossable(m_PlantHue))
                            m_PlantHue |= PlantHue.Reproduces;

                        break;
                    }
            }

            m_Instances.Add(this);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            m_Instances.Remove(this);
        }
    }

    #region Plant Bowl Player Interaction

    public class PollinateTarget : Target
    {
        private PlantItem m_Plant;

        public PollinateTarget(PlantItem plant)
            : base(3, true, TargetFlags.None)
        {
            m_Plant = plant;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (!m_Plant.Deleted && m_Plant.PlantStatus < PlantStatus.DecorativePlant && from.InRange(m_Plant.GetWorldLocation(), 3))
            {
                if (!m_Plant.IsUsableBy(from))
                {
                    m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                }
                else if (!m_Plant.IsCrossable)
                {
                    m_Plant.LabelTo(from, 1053050); // You cannot gather pollen from a mutated plant!
                }
                else if (!m_Plant.PlantSystem.PollenProducing)
                {
                    m_Plant.LabelTo(from, 1053051); // You cannot gather pollen from a plant in this stage of development!
                }
                else if (m_Plant.PlantSystem.Health < PlantHealth.Healthy)
                {
                    m_Plant.LabelTo(from, 1053052); // You cannot gather pollen from an unhealthy plant!
                }
                else
                {
                    PlantItem targ = targeted as PlantItem;

                    if (targ == null || targ.PlantStatus >= PlantStatus.DecorativePlant || targ.PlantStatus <= PlantStatus.BowlOfDirt)
                    {
                        m_Plant.LabelTo(from, 1053070); // You can only pollinate other specially grown plants!
                    }
                    else if (!targ.IsUsableBy(from))
                    {
                        targ.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                    }
                    else if (!targ.IsCrossable)
                    {
                        targ.LabelTo(from, 1053073); // You cannot cross-pollinate with a mutated plant!
                    }
                    else if (!targ.PlantSystem.PollenProducing)
                    {
                        targ.LabelTo(from, 1053074); // This plant is not in the flowering stage. You cannot pollinate it!
                    }
                    else if (targ.PlantSystem.Health < PlantHealth.Healthy)
                    {
                        targ.LabelTo(from, 1053075); // You cannot pollinate an unhealthy plant!
                    }
                    else if (targ.PlantSystem.Pollinated)
                    {
                        targ.LabelTo(from, 1053072); // This plant has already been pollinated!
                    }
                    else if (targ == m_Plant)
                    {
                        targ.PlantSystem.Pollinated = true;
                        targ.PlantSystem.SeedType = m_Plant.PlantType;
                        targ.PlantSystem.SeedHue = m_Plant.PlantHue;

                        targ.LabelTo(from, 1053071); // You pollinate the plant with its own pollen.
                    }
                    else
                    {
                        targ.PlantSystem.Pollinated = true;
                        targ.PlantSystem.SeedType = PlantTypeInfo.Cross(m_Plant.PlantType, targ.PlantType);
                        targ.PlantSystem.SeedHue = PlantHueInfo.Cross(m_Plant.PlantHue, targ.PlantHue);

                        targ.LabelTo(from, 1053076); // You successfully cross-pollinate the plant.
                    }
                }
            }
        }

        protected override void OnTargetFinish(Mobile from)
        {
            if (!m_Plant.Deleted && m_Plant.PlantStatus < PlantStatus.DecorativePlant && m_Plant.PlantStatus != PlantStatus.BowlOfDirt && from.InRange(m_Plant.GetWorldLocation(), 3) && m_Plant.IsUsableBy(from))
            {
                from.SendGump(new ReproductionGump(m_Plant));
            }
        }
    }

    public class PlantPourTarget : Target
    {
        private PlantItem m_Plant;

        public PlantPourTarget(PlantItem plant)
            : base(3, true, TargetFlags.None)
        {
            m_Plant = plant;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (!m_Plant.Deleted && from.InRange(m_Plant.GetWorldLocation(), 3) && targeted is Item)
            {
                m_Plant.Pour(from, (Item)targeted);
            }
        }

        protected override void OnTargetFinish(Mobile from)
        {
            if (!m_Plant.Deleted && m_Plant.PlantStatus < PlantStatus.DecorativePlant && from.InRange(m_Plant.GetWorldLocation(), 3) && m_Plant.IsUsableBy(from))
            {
                if (from.HasGump(typeof(MainPlantGump)))
                    from.CloseGump(typeof(MainPlantGump));

                from.SendGump(new MainPlantGump(m_Plant));
            }
        }
    }

    public class SetToDecorativeGump : Gump
    {
        private PlantItem m_Plant;

        public SetToDecorativeGump(PlantItem plant)
            : base(20, 20)
        {
            m_Plant = plant;

            DrawBackground();

            AddLabel(115, 85, 0x44, "Set plant");
            AddLabel(82, 105, 0x44, "to decorative mode?");

            AddButton(98, 140, 0x47E, 0x480, 1, GumpButtonType.Reply, 0); // Cancel

            AddButton(138, 141, 0xD2, 0xD2, 2, GumpButtonType.Reply, 0); // Help
            AddLabel(143, 141, 0x835, "?");

            AddButton(168, 140, 0x481, 0x483, 3, GumpButtonType.Reply, 0); // Ok
        }

        private void DrawBackground()
        {
            AddBackground(50, 50, 200, 150, 0xE10);

            AddItem(25, 45, 0xCEB);
            AddItem(25, 118, 0xCEC);

            AddItem(227, 45, 0xCEF);
            AddItem(227, 118, 0xCF0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus != PlantStatus.Stage9)
                return;

            if (info.ButtonID == 3 && !from.InRange(m_Plant.GetWorldLocation(), 3))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
                return;
            }

            if (!m_Plant.IsUsableBy(from))
            {
                m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // Cancel
                    {
                        from.SendGump(new ReproductionGump(m_Plant));

                        break;
                    }
                case 2: // Help
                    {
                        from.Send(new DisplayHelpTopic(70, true)); // DECORATIVE MODE

                        from.SendGump(new SetToDecorativeGump(m_Plant));

                        break;
                    }
                case 3: // Ok
                    {
                        m_Plant.PlantStatus = PlantStatus.DecorativePlant;
                        m_Plant.LabelTo(from, 1053077); // You prune the plant. This plant will no longer produce resources or seeds, but will require no upkeep.

                        break;
                    }
            }
        }
    }

    public class EmptyTheBowlGump : Gump
    {
        private PlantItem m_Plant;

        public EmptyTheBowlGump(PlantItem plant)
            : base(20, 20)
        {
            m_Plant = plant;

            DrawBackground();

            AddLabel(90, 70, 0x44, "Empty the bowl?");

            DrawPicture();

            AddButton(98, 150, 0x47E, 0x480, 1, GumpButtonType.Reply, 0); // Cancel

            AddButton(138, 151, 0xD2, 0xD2, 2, GumpButtonType.Reply, 0); // Help
            AddLabel(143, 151, 0x835, "?");

            AddButton(168, 150, 0x481, 0x483, 3, GumpButtonType.Reply, 0); // Ok
        }

        private void DrawBackground()
        {
            AddBackground(50, 50, 200, 150, 0xE10);

            AddItem(45, 45, 0xCEF);
            AddItem(45, 118, 0xCF0);

            AddItem(211, 45, 0xCEB);
            AddItem(211, 118, 0xCEC);
        }

        private void DrawPicture()
        {
            AddItem(90, 100, 0x1602);
            AddImage(140, 102, 0x15E1);
            AddItem(160, 100, 0x15FD);

            if (m_Plant.PlantStatus != PlantStatus.BowlOfDirt && m_Plant.PlantStatus < PlantStatus.Plant)
                AddItem(156, 130, 0xDCF); // Seed
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus >= PlantStatus.DecorativePlant)
                return;

            if (info.ButtonID == 3 && !from.InRange(m_Plant.GetWorldLocation(), 3))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
                return;
            }

            if (!m_Plant.IsUsableBy(from))
            {
                m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // Cancel
                    {
                        from.SendGump(new MainPlantGump(m_Plant));

                        break;
                    }
                case 2: // Help
                    {
                        from.Send(new DisplayHelpTopic(71, true)); // EMPTYING THE BOWL

                        from.SendGump(new EmptyTheBowlGump(m_Plant));

                        break;
                    }
                case 3: // Ok
                    {
                        PlantBowl bowl = new PlantBowl();

                        if (!from.PlaceInBackpack(bowl))
                        {
                            bowl.Delete();

                            m_Plant.LabelTo(from, 1053047); // You cannot empty a bowl with a full pack!
                            from.SendGump(new MainPlantGump(m_Plant));

                            break;
                        }

                        if (m_Plant.PlantStatus != PlantStatus.BowlOfDirt && m_Plant.PlantStatus < PlantStatus.Plant)
                        {
                            Seed seed = new Seed(m_Plant.PlantType, m_Plant.PlantHue, m_Plant.ShowType);

                            if (!from.PlaceInBackpack(seed))
                            {
                                bowl.Delete();
                                seed.Delete();

                                m_Plant.LabelTo(from, 1053047); // You cannot empty a bowl with a full pack!
                                from.SendGump(new MainPlantGump(m_Plant));

                                break;
                            }
                        }

                        m_Plant.Delete();

                        break;
                    }
            }
        }
    }

    #endregion
}

namespace Server.Network
{
    public class DisplayHelpTopic : Packet
    {
        public DisplayHelpTopic(int topicID, bool display)
            : base(0xBF)
        {
            EnsureCapacity(11);

            m_Stream.Write((short)0x17);
            m_Stream.Write((byte)1);
            m_Stream.Write((int)topicID);
            m_Stream.Write((bool)display);
        }
    }
}