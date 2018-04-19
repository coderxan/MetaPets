using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public enum WaterState
    {
        Dead,
        Dying,
        Unhealthy,
        Healthy,
        Strong
    }

    public enum FoodState
    {
        Dead,
        Starving,
        Hungry,
        Full,
        Overfed
    }

    [PropertyObject]
    public class AquariumState
    {
        private int m_State;
        private int m_Maintain;
        private int m_Improve;
        private int m_Added;

        [CommandProperty(AccessLevel.GameMaster)]
        public int State
        {
            get { return m_State; }
            set
            {
                m_State = value;

                if (m_State < 0)
                    m_State = 0;

                if (m_State > 4)
                    m_State = 4;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Maintain
        {
            get { return m_Maintain; }
            set { m_Maintain = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Improve
        {
            get { return m_Improve; }
            set { m_Improve = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Added
        {
            get { return m_Added; }
            set { m_Added = value; }
        }

        public AquariumState()
        {
        }

        public override string ToString()
        {
            return "...";
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(0); // version

            writer.Write(m_State);
            writer.Write(m_Maintain);
            writer.Write(m_Improve);
            writer.Write(m_Added);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            m_State = reader.ReadInt();
            m_Maintain = reader.ReadInt();
            m_Improve = reader.ReadInt();
            m_Added = reader.ReadInt();
        }
    }

    public class Aquarium : BaseAddonContainer
    {
        public static readonly TimeSpan EvaluationInterval = TimeSpan.FromDays(1);

        // items info
        private int m_LiveCreatures;

        [CommandProperty(AccessLevel.GameMaster)]
        public int LiveCreatures
        {
            get { return m_LiveCreatures; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DeadCreatures
        {
            get
            {
                int dead = 0;

                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] is BaseAquariumFish)
                    {
                        BaseAquariumFish fish = (BaseAquariumFish)Items[i];

                        if (fish.Dead)
                            dead += 1;
                    }
                }

                return dead;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxLiveCreatures
        {
            get
            {
                int state = (m_Food.State == (int)FoodState.Overfed) ? 1 : (int)FoodState.Full - m_Food.State;

                state += (int)WaterState.Strong - m_Water.State;

                state = (int)Math.Pow(state, 1.75);

                return MaxItems - state;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsFull
        {
            get { return (Items.Count >= MaxItems); }
        }

        // vacation info
        private int m_VacationLeft;

        [CommandProperty(AccessLevel.GameMaster)]
        public int VacationLeft
        {
            get { return m_VacationLeft; }
            set { m_VacationLeft = value; InvalidateProperties(); }
        }

        // aquarium state
        private AquariumState m_Food;
        private AquariumState m_Water;

        [CommandProperty(AccessLevel.GameMaster)]
        public AquariumState Food
        {
            get { return m_Food; }
            set { m_Food = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AquariumState Water
        {
            get { return m_Water; }
            set { m_Water = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool OptimalState
        {
            get { return (m_Food.State == (int)FoodState.Full && m_Water.State == (int)WaterState.Strong); }
        }

        // events
        private List<int> m_Events;
        private bool m_RewardAvailable;
        private bool m_EvaluateDay;

        public List<int> Events
        {
            get { return m_Events; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RewardAvailable
        {
            get { return m_RewardAvailable; }
            set { m_RewardAvailable = value; InvalidateProperties(); }
        }

        // evaluate timer
        private Timer m_Timer;

        public override BaseAddonContainerDeed Deed
        {
            get
            {
                if (ItemID == 0x3062)
                    return new AquariumEastDeed();
                else
                    return new AquariumNorthDeed();
            }
        }

        public override double DefaultWeight { get { return 10.0; } }

        public Aquarium(int itemID)
            : base(itemID)
        {
            Movable = false;

            if (itemID == 0x3060)
                AddComponent(new AddonContainerComponent(0x3061), -1, 0, 0);

            if (itemID == 0x3062)
                AddComponent(new AddonContainerComponent(0x3063), 0, -1, 0);

            MaxItems = 30;

            m_Food = new AquariumState();
            m_Water = new AquariumState();

            m_Food.State = (int)FoodState.Full;
            m_Water.State = (int)WaterState.Strong;

            m_Food.Maintain = Utility.RandomMinMax(1, 2);
            m_Food.Improve = m_Food.Maintain + Utility.RandomMinMax(1, 2);

            m_Water.Maintain = Utility.RandomMinMax(1, 3);

            m_Events = new List<int>();

            m_Timer = Timer.DelayCall(EvaluationInterval, EvaluationInterval, new TimerCallback(Evaluate));
        }

        public Aquarium(Serial serial)
            : base(serial)
        {
        }

        public override void OnDelete()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            ExamineAquarium(from);
        }

        public virtual bool HasAccess(Mobile from)
        {
            if (from == null || from.Deleted)
                return false;
            else if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            BaseHouse house = BaseHouse.FindHouseAt(this);

            return (house != null && house.IsCoOwner(from));
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!HasAccess(from))
            {
                from.SendLocalizedMessage(1073821); // You do not have access to that item for use with the aquarium.
                return false;
            }

            if (m_VacationLeft > 0)
            {
                from.SendLocalizedMessage(1074427); // The aquarium is in vacation mode.
                return false;
            }

            bool takeItem = true;

            if (dropped is FishBowl)
            {
                FishBowl bowl = (FishBowl)dropped;

                if (bowl.Empty || !AddFish(from, bowl.Fish))
                    return false;

                bowl.InvalidateProperties();

                takeItem = false;
            }
            else if (dropped is BaseAquariumFish)
            {
                BaseAquariumFish fish = (BaseAquariumFish)dropped;

                if (!AddFish(from, fish))
                    return false;
            }
            else if (dropped is VacationWafer)
            {
                m_VacationLeft = VacationWafer.VacationDays;
                dropped.Delete();

                from.SendLocalizedMessage(1074428, m_VacationLeft.ToString()); // The aquarium will be in vacation mode for ~1_DAYS~ days
            }
            else if (dropped is AquariumFood)
            {
                m_Food.Added += 1;
                dropped.Delete();

                from.SendLocalizedMessage(1074259, "1"); // ~1_NUM~ unit(s) of food have been added to the aquarium.
            }
            else if (dropped is BaseBeverage)
            {
                BaseBeverage beverage = (BaseBeverage)dropped;

                if (beverage.IsEmpty || !beverage.Pourable || beverage.Content != BeverageType.Water)
                {
                    from.SendLocalizedMessage(500840); // Can't pour that in there.
                    return false;
                }

                m_Water.Added += 1;
                beverage.Quantity -= 1;

                from.PlaySound(0x4E);
                from.SendLocalizedMessage(1074260, "1"); // ~1_NUM~ unit(s) of water have been added to the aquarium.

                takeItem = false;
            }
            else if (!AddDecoration(from, dropped))
            {
                takeItem = false;
            }

            from.CloseGump(typeof(AquariumGump));

            InvalidateProperties();

            if (takeItem)
                from.PlaySound(0x42);

            return takeItem;
        }

        public override void DropItemsToGround()
        {
            Point3D loc = GetWorldLocation();

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Item item = Items[i];

                item.MoveToWorld(loc, Map);

                if (item is BaseAquariumFish)
                {
                    BaseAquariumFish fish = (BaseAquariumFish)item;

                    if (!fish.Dead)
                        fish.StartTimer();
                }
            }
        }

        public override bool CheckItemUse(Mobile from, Item item)
        {
            if (item != this)
                return false;

            return base.CheckItemUse(from, item);
        }

        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
        {
            if (item != this)
            {
                reject = LRReason.CannotLift;
                return false;
            }

            return base.CheckLift(from, item, ref reject);
        }

        public override void OnSingleClick(Mobile from)
        {
            if (Deleted || !from.CanSee(this))
                return;

            base.OnSingleClick(from);

            if (m_VacationLeft > 0)
                this.LabelTo(from, 1074430, m_VacationLeft.ToString()); // Vacation days left: ~1_DAYS

            if (m_Events.Count > 0)
                this.LabelTo(from, 1074426, m_Events.Count.ToString()); // ~1_NUM~ event(s) to view!

            if (m_RewardAvailable)
                this.LabelTo(from, 1074362); // A reward is available!

            this.LabelTo(from, 1074247, String.Format("{0}\t{1}", m_LiveCreatures, MaxLiveCreatures)); // Live Creatures: ~1_NUM~ / ~2_MAX~

            if (DeadCreatures > 0)
                this.LabelTo(from, 1074248, DeadCreatures.ToString()); // Dead Creatures: ~1_NUM~

            int decorations = Items.Count - m_LiveCreatures - DeadCreatures;

            if (decorations > 0)
                this.LabelTo(from, 1074249, (Items.Count - m_LiveCreatures - DeadCreatures).ToString()); // Decorations: ~1_NUM~

            this.LabelTo(from, 1074250, "#" + FoodNumber()); // Food state: ~1_STATE~
            this.LabelTo(from, 1074251, "#" + WaterNumber()); // Water state: ~1_STATE~

            if (m_Food.State == (int)FoodState.Dead)
                this.LabelTo(from, 1074577, String.Format("{0}\t{1}", m_Food.Added, m_Food.Improve)); // Food Added: ~1_CUR~ Needed: ~2_NEED~ 				
            else if (m_Food.State == (int)FoodState.Overfed)
                this.LabelTo(from, 1074577, String.Format("{0}\t{1}", m_Food.Added, m_Food.Maintain)); // Food Added: ~1_CUR~ Needed: ~2_NEED~ 
            else
                this.LabelTo(from, 1074253, String.Format("{0}\t{1}\t{2}", m_Food.Added, m_Food.Maintain, m_Food.Improve)); // Food Added: ~1_CUR~ Feed: ~2_NEED~ Improve: ~3_GROW~

            if (m_Water.State == (int)WaterState.Dead)
                this.LabelTo(from, 1074578, String.Format("{0}\t{1}", m_Water.Added, m_Water.Improve)); // Water Added: ~1_CUR~ Needed: ~2_NEED~
            else if (m_Water.State == (int)WaterState.Strong)
                this.LabelTo(from, 1074578, String.Format("{0}\t{1}", m_Water.Added, m_Water.Maintain)); // Water Added: ~1_CUR~ Needed: ~2_NEED~
            else
                this.LabelTo(from, 1074254, String.Format("{0}\t{1}\t{2}", m_Water.Added, m_Water.Maintain, m_Water.Improve)); // Water Added: ~1_CUR~ Maintain: ~2_NEED~ Improve: ~3_GROW~
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (m_VacationLeft > 0)
                list.Add(1074430, m_VacationLeft.ToString()); // Vacation days left: ~1_DAYS

            if (m_Events.Count > 0)
                list.Add(1074426, m_Events.Count.ToString()); // ~1_NUM~ event(s) to view!

            if (m_RewardAvailable)
                list.Add(1074362); // A reward is available!

            list.Add(1074247, "{0}\t{1}", m_LiveCreatures, MaxLiveCreatures); // Live Creatures: ~1_NUM~ / ~2_MAX~

            int dead = DeadCreatures;

            if (dead > 0)
                list.Add(1074248, dead.ToString()); // Dead Creatures: ~1_NUM~

            int decorations = Items.Count - m_LiveCreatures - dead;

            if (decorations > 0)
                list.Add(1074249, decorations.ToString()); // Decorations: ~1_NUM~

            list.Add(1074250, "#{0}", FoodNumber()); // Food state: ~1_STATE~
            list.Add(1074251, "#{0}", WaterNumber()); // Water state: ~1_STATE~

            if (m_Food.State == (int)FoodState.Dead)
                list.Add(1074577, "{0}\t{1}", m_Food.Added, m_Food.Improve); // Food Added: ~1_CUR~ Needed: ~2_NEED~
            else if (m_Food.State == (int)FoodState.Overfed)
                list.Add(1074577, "{0}\t{1}", m_Food.Added, m_Food.Maintain); // Food Added: ~1_CUR~ Needed: ~2_NEED~
            else
                list.Add(1074253, "{0}\t{1}\t{2}", m_Food.Added, m_Food.Maintain, m_Food.Improve); // Food Added: ~1_CUR~ Feed: ~2_NEED~ Improve: ~3_GROW~

            if (m_Water.State == (int)WaterState.Dead)
                list.Add(1074578, "{0}\t{1}", m_Water.Added, m_Water.Improve); // Water Added: ~1_CUR~ Needed: ~2_NEED~
            else if (m_Water.State == (int)WaterState.Strong)
                list.Add(1074578, "{0}\t{1}", m_Water.Added, m_Water.Maintain); // Water Added: ~1_CUR~ Needed: ~2_NEED~
            else
                list.Add(1074254, "{0}\t{1}\t{2}", m_Water.Added, m_Water.Maintain, m_Water.Improve); // Water Added: ~1_CUR~ Maintain: ~2_NEED~ Improve: ~3_GROW~
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive)
            {
                list.Add(new ExamineEntry(this));

                if (HasAccess(from))
                {
                    if (m_RewardAvailable)
                        list.Add(new CollectRewardEntry(this));

                    if (m_Events.Count > 0)
                        list.Add(new ViewEventEntry(this));

                    if (m_VacationLeft > 0)
                        list.Add(new CancelVacationMode(this));
                }
            }

            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                list.Add(new GMAddFood(this));
                list.Add(new GMAddWater(this));
                list.Add(new GMForceEvaluate(this));
                list.Add(new GMOpen(this));
                list.Add(new GMFill(this));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(3); // Version

            // version 1
            if (m_Timer != null)
                writer.Write(m_Timer.Next);
            else
                writer.Write(DateTime.UtcNow + EvaluationInterval);

            // version 0
            writer.Write((int)m_LiveCreatures);
            writer.Write((int)m_VacationLeft);

            m_Food.Serialize(writer);
            m_Water.Serialize(writer);

            writer.Write((int)m_Events.Count);

            for (int i = 0; i < m_Events.Count; i++)
                writer.Write((int)m_Events[i]);

            writer.Write((bool)m_RewardAvailable);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 3:
                case 2:
                case 1:
                    {
                        DateTime next = reader.ReadDateTime();

                        if (next < DateTime.UtcNow)
                            next = DateTime.UtcNow;

                        m_Timer = Timer.DelayCall(next - DateTime.UtcNow, EvaluationInterval, new TimerCallback(Evaluate));

                        goto case 0;
                    }
                case 0:
                    {
                        m_LiveCreatures = reader.ReadInt();
                        m_VacationLeft = reader.ReadInt();

                        m_Food = new AquariumState();
                        m_Water = new AquariumState();

                        m_Food.Deserialize(reader);
                        m_Water.Deserialize(reader);

                        m_Events = new List<int>();

                        int count = reader.ReadInt();

                        for (int i = 0; i < count; i++)
                            m_Events.Add(reader.ReadInt());

                        m_RewardAvailable = reader.ReadBool();

                        break;
                    }
            }

            if (version < 2)
            {
                Weight = DefaultWeight;
                Movable = false;
            }

            if (version < 3)
                ValidationQueue<Aquarium>.Add(this);
        }

        private void RecountLiveCreatures()
        {
            m_LiveCreatures = 0;
            List<BaseAquariumFish> fish = FindItemsByType<BaseAquariumFish>();

            foreach (BaseAquariumFish f in fish)
            {
                if (!f.Dead)
                    ++m_LiveCreatures;
            }
        }

        public void Validate()
        {
            RecountLiveCreatures();
        }

        #region Members
        public int FoodNumber()
        {
            if (m_Food.State == (int)FoodState.Full)
                return 1074240;

            if (m_Food.State == (int)FoodState.Overfed)
                return 1074239;

            return 1074236 + m_Food.State;
        }

        public int WaterNumber()
        {
            return 1074242 + m_Water.State;
        }
        #endregion

        #region Virtual members
        public virtual void KillFish(int amount)
        {
            List<BaseAquariumFish> toKill = new List<BaseAquariumFish>();

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is BaseAquariumFish)
                {
                    BaseAquariumFish fish = (BaseAquariumFish)Items[i];

                    if (!fish.Dead)
                        toKill.Add(fish);
                }
            }

            while (amount > 0 && toKill.Count > 0)
            {
                int kill = Utility.Random(toKill.Count);

                toKill[kill].Kill();

                toKill.RemoveAt(kill);

                amount -= 1;
                m_LiveCreatures -= 1;

                if (m_LiveCreatures < 0)
                    m_LiveCreatures = 0;

                m_Events.Add(1074366); // An unfortunate accident has left a creature floating upside-down.  It is starting to smell.
            }
        }

        public virtual void Evaluate()
        {
            if (m_VacationLeft > 0)
            {
                m_VacationLeft -= 1;
            }
            else if (m_EvaluateDay)
            {
                // reset events
                m_Events = new List<int>();

                // food events
                if (
                    (m_Food.Added < m_Food.Maintain && m_Food.State != (int)FoodState.Overfed && m_Food.State != (int)FoodState.Dead) ||
                    (m_Food.Added >= m_Food.Improve && m_Food.State == (int)FoodState.Full)
                    )
                    m_Events.Add(1074368); // The tank looks worse than it did yesterday.

                if (
                    (m_Food.Added >= m_Food.Improve && m_Food.State != (int)FoodState.Full && m_Food.State != (int)FoodState.Overfed) ||
                    (m_Food.Added < m_Food.Maintain && m_Food.State == (int)FoodState.Overfed)
                    )
                    m_Events.Add(1074367); // The tank looks healthier today.

                // water events
                if (m_Water.Added < m_Water.Maintain && m_Water.State != (int)WaterState.Dead)
                    m_Events.Add(1074370); // This tank can use more water.

                if (m_Water.Added >= m_Water.Improve && m_Water.State != (int)WaterState.Strong)
                    m_Events.Add(1074369); // The water looks clearer today.

                UpdateFoodState();
                UpdateWaterState();

                // reward
                if (m_LiveCreatures > 0)
                    m_RewardAvailable = true;
            }
            else
            {
                // new fish
                if (OptimalState && m_LiveCreatures < MaxLiveCreatures)
                {
                    if (Utility.RandomDouble() < 0.005 * m_LiveCreatures)
                    {
                        BaseAquariumFish fish = null;
                        int message = 0;

                        switch (Utility.Random(6))
                        {
                            case 0:
                                {
                                    message = 1074371; // Brine shrimp have hatched overnight in the tank.
                                    fish = new BrineShrimp();
                                    break;
                                }
                            case 1:
                                {
                                    message = 1074365; // A new creature has hatched overnight in the tank.
                                    fish = new Coral();
                                    break;
                                }
                            case 2:
                                {
                                    message = 1074365; // A new creature has hatched overnight in the tank.
                                    fish = new FullMoonFish();
                                    break;
                                }
                            case 3:
                                {
                                    message = 1074373; // A sea horse has hatched overnight in the tank.
                                    fish = new SeaHorseFish();
                                    break;
                                }
                            case 4:
                                {
                                    message = 1074365; // A new creature has hatched overnight in the tank.
                                    fish = new StrippedFlakeFish();
                                    break;
                                }
                            case 5:
                                {
                                    message = 1074365; // A new creature has hatched overnight in the tank.
                                    fish = new StrippedSosarianSwill();
                                    break;
                                }
                        }

                        if (Utility.RandomDouble() < 0.05)
                            fish.Hue = m_FishHues[Utility.Random(m_FishHues.Length)];
                        else if (Utility.RandomDouble() < 0.5)
                            fish.Hue = Utility.RandomMinMax(0x100, 0x3E5);

                        if (AddFish(fish))
                            m_Events.Add(message);
                        else
                            fish.Delete();
                    }
                }

                // kill fish *grins*
                if (m_LiveCreatures < MaxLiveCreatures)
                {
                    if (Utility.RandomDouble() < 0.01)
                        KillFish(1);
                }
                else
                {
                    KillFish(m_LiveCreatures - MaxLiveCreatures);
                }
            }

            m_EvaluateDay = !m_EvaluateDay;
            InvalidateProperties();
        }

        public virtual void GiveReward(Mobile to)
        {
            if (!m_RewardAvailable)
                return;

            int max = (int)(((double)m_LiveCreatures / 30) * m_Decorations.Length);

            int random = (max <= 0) ? 0 : Utility.Random(max);

            if (random >= m_Decorations.Length)
                random = m_Decorations.Length - 1;

            Item item;

            try
            {
                item = Activator.CreateInstance(m_Decorations[random]) as Item;
            }
            catch
            {
                return;
            }

            if (item == null)
                return;

            if (!to.PlaceInBackpack(item))
            {
                item.Delete();
                to.SendLocalizedMessage(1074361); // The reward could not be given.  Make sure you have room in your pack.
                return;
            }

            to.SendLocalizedMessage(1074360, String.Format("#{0}", item.LabelNumber)); // You receive a reward: ~1_REWARD~
            to.PlaySound(0x5A3);

            m_RewardAvailable = false;

            InvalidateProperties();
        }

        public virtual void UpdateFoodState()
        {
            if (m_Food.Added < m_Food.Maintain)
                m_Food.State = (m_Food.State <= 0) ? 0 : m_Food.State - 1;
            else if (m_Food.Added >= m_Food.Improve)
                m_Food.State = (m_Food.State >= (int)FoodState.Overfed) ? (int)FoodState.Overfed : m_Food.State + 1;

            m_Food.Maintain = Utility.Random((int)FoodState.Overfed + 1 - m_Food.State, 2);

            if (m_Food.State == (int)FoodState.Overfed)
                m_Food.Improve = 0;
            else
                m_Food.Improve = m_Food.Maintain + 2;

            m_Food.Added = 0;
        }

        public virtual void UpdateWaterState()
        {
            if (m_Water.Added < m_Water.Maintain)
                m_Water.State = (m_Water.State <= 0) ? 0 : m_Water.State - 1;
            else if (m_Water.Added >= m_Water.Improve)
                m_Water.State = (m_Water.State >= (int)WaterState.Strong) ? (int)WaterState.Strong : m_Water.State + 1;

            m_Water.Maintain = Utility.Random((int)WaterState.Strong + 2 - m_Water.State, 2);

            if (m_Water.State == (int)WaterState.Strong)
                m_Water.Improve = 0;
            else
                m_Water.Improve = m_Water.Maintain + 2;

            m_Water.Added = 0;
        }

        public virtual bool RemoveItem(Mobile from, int at)
        {
            if (at < 0 && at >= Items.Count)
                return false;

            Item item = Items[at];

            if (item.IsLockedDown) // for legacy aquariums
            {
                from.SendLocalizedMessage(1010449); // You may not use this object while it is locked down.
                return false;
            }

            if (item is BaseAquariumFish)
            {
                BaseAquariumFish fish = (BaseAquariumFish)item;

                FishBowl bowl;

                if ((bowl = GetEmptyBowl(from)) != null)
                {
                    bowl.AddItem(fish);

                    from.SendLocalizedMessage(1074511); // You put the creature into a fish bowl.
                }
                else
                {
                    if (!from.PlaceInBackpack(fish))
                    {
                        from.SendLocalizedMessage(1074514); // You have no place to put it.
                        return false;
                    }
                    else
                    {
                        from.SendLocalizedMessage(1074512); // You put the gasping creature into your pack.
                    }
                }

                if (!fish.Dead)
                    m_LiveCreatures -= 1;
            }
            else
            {
                if (!from.PlaceInBackpack(item))
                {
                    from.SendLocalizedMessage(1074514); // You have no place to put it.
                    return false;
                }
                else
                {
                    from.SendLocalizedMessage(1074513); // You put the item into your pack.
                }
            }

            InvalidateProperties();
            return true;
        }

        public virtual void ExamineAquarium(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            from.CloseGump(typeof(AquariumGump));
            from.SendGump(new AquariumGump(this, HasAccess(from)));

            from.PlaySound(0x5A4);
        }

        public virtual bool AddFish(BaseAquariumFish fish)
        {
            return AddFish(null, fish);
        }

        public virtual bool AddFish(Mobile from, BaseAquariumFish fish)
        {
            if (fish == null)
                return false;

            if (IsFull || m_LiveCreatures >= MaxLiveCreatures || fish.Dead)
            {
                if (from != null)
                    from.SendLocalizedMessage(1073633); // The aquarium can not hold the creature.

                return false;
            }

            AddItem(fish);
            fish.StopTimer();

            m_LiveCreatures += 1;

            if (from != null)
                from.SendLocalizedMessage(1073632, String.Format("#{0}", fish.LabelNumber)); // You add the following creature to your aquarium: ~1_FISH~

            InvalidateProperties();
            return true;
        }

        public virtual bool AddDecoration(Item item)
        {
            return AddDecoration(null, item);
        }

        public virtual bool AddDecoration(Mobile from, Item item)
        {
            if (item == null)
                return false;

            if (IsFull)
            {
                if (from != null)
                    from.SendLocalizedMessage(1073636); // The decoration will not fit in the aquarium.

                return false;
            }

            if (!Accepts(item))
            {
                if (from != null)
                    from.SendLocalizedMessage(1073822); // The aquarium can not hold that item.

                return false;
            }

            AddItem(item);

            if (from != null)
                from.SendLocalizedMessage(1073635, (item.LabelNumber != 0) ? String.Format("#{0}", item.LabelNumber) : item.Name); // You add the following decoration to your aquarium: ~1_NAME~

            InvalidateProperties();
            return true;
        }
        #endregion

        #region Static members
        public static FishBowl GetEmptyBowl(Mobile from)
        {
            if (from == null || from.Backpack == null)
                return null;

            Item[] items = from.Backpack.FindItemsByType(typeof(FishBowl));

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] is FishBowl)
                {
                    FishBowl bowl = (FishBowl)items[i];

                    if (bowl.Empty)
                        return bowl;
                }
            }

            return null;
        }

        private static Type[] m_Decorations = new Type[]
		{
			typeof( FishBones ),
			typeof( WaterloggedBoots ),
			typeof( CaptainBlackheartsFishingPole ),
			typeof( CraftysFishingHat ),
			typeof( AquariumFishNet ),
			typeof( AquariumMessage ),
			typeof( IslandStatue ),
			typeof( Shell ),
			typeof( ToyBoat )
		};

        public static bool Accepts(Item item)
        {
            if (item == null)
                return false;

            Type type = item.GetType();

            for (int i = 0; i < m_Decorations.Length; i++)
            {
                if (type == m_Decorations[i])
                    return true;
            }

            return false;
        }

        private static int[] m_FishHues = new int[]
		{
			0x1C2, 0x1C3, 0x2A3, 0x47E, 0x51D
		};

        public static int[] FishHues
        {
            get { return m_FishHues; }
        }
        #endregion

        #region Context entries
        private class ExamineEntry : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public ExamineEntry(Aquarium aquarium)
                : base(6235, 2) // Examine Aquarium
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted)
                    return;

                m_Aquarium.ExamineAquarium(Owner.From);
            }
        }

        private class CollectRewardEntry : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public CollectRewardEntry(Aquarium aquarium)
                : base(6237, 2) // Collect Reward
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted || !m_Aquarium.HasAccess(Owner.From))
                    return;

                m_Aquarium.GiveReward(Owner.From);
            }
        }

        private class ViewEventEntry : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public ViewEventEntry(Aquarium aquarium)
                : base(6239, 2) // View events
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted || !m_Aquarium.HasAccess(Owner.From) || m_Aquarium.Events.Count == 0)
                    return;

                Owner.From.SendLocalizedMessage(m_Aquarium.Events[0]);

                if (m_Aquarium.Events[0] == 1074366)
                    Owner.From.PlaySound(0x5A2);

                m_Aquarium.Events.RemoveAt(0);
                m_Aquarium.InvalidateProperties();
            }
        }

        private class CancelVacationMode : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public CancelVacationMode(Aquarium aquarium)
                : base(6240, 2) // Cancel vacation mode
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted || !m_Aquarium.HasAccess(Owner.From))
                    return;

                Owner.From.SendLocalizedMessage(1074429); // Vacation mode has been cancelled.
                m_Aquarium.VacationLeft = 0;
                m_Aquarium.InvalidateProperties();
            }
        }

        // GM context entries
        private class GMAddFood : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public GMAddFood(Aquarium aquarium)
                : base(6231, -1) // GM Add Food
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted)
                    return;

                m_Aquarium.Food.Added += 1;
                m_Aquarium.InvalidateProperties();
            }
        }

        private class GMAddWater : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public GMAddWater(Aquarium aquarium)
                : base(6232, -1) // GM Add Water
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted)
                    return;

                m_Aquarium.Water.Added += 1;
                m_Aquarium.InvalidateProperties();
            }
        }

        private class GMForceEvaluate : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public GMForceEvaluate(Aquarium aquarium)
                : base(6233, -1) // GM Force Evaluate
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted)
                    return;

                m_Aquarium.Evaluate();
            }
        }

        private class GMOpen : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public GMOpen(Aquarium aquarium)
                : base(6234, -1) // GM Open Container
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted)
                    return;

                Owner.From.SendGump(new AquariumGump(m_Aquarium, true));
            }
        }

        private class GMFill : ContextMenuEntry
        {
            private Aquarium m_Aquarium;

            public GMFill(Aquarium aquarium)
                : base(6236, -1) // GM Fill Food and Water
            {
                m_Aquarium = aquarium;
            }

            public override void OnClick()
            {
                if (m_Aquarium.Deleted)
                    return;

                m_Aquarium.Food.Added = m_Aquarium.Food.Maintain;
                m_Aquarium.Water.Added = m_Aquarium.Water.Maintain;
                m_Aquarium.InvalidateProperties();
            }
        }
        #endregion
    }

    public class AquariumGump : Gump
    {
        private Aquarium m_Aquarium;

        public AquariumGump(Aquarium aquarium, bool edit)
            : base(100, 100)
        {
            m_Aquarium = aquarium;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 350, 323, 0xE10);
            AddImage(0, 0, 0x2C96);

            if (m_Aquarium.Items.Count == 0)
                return;

            for (int i = 1; i <= m_Aquarium.Items.Count; i++)
                DisplayPage(i, edit);
        }

        public virtual void DisplayPage(int page, bool edit)
        {
            AddPage(page);

            Item item = m_Aquarium.Items[page - 1];

            // item name
            if (item.LabelNumber != 0)
                AddHtmlLocalized(20, 217, 250, 20, item.LabelNumber, 0xFFFFFF, false, false); // Name

            // item details
            if (item is BaseAquariumFish)
                AddHtmlLocalized(20, 239, 315, 20, ((BaseAquariumFish)item).GetDescription(), 0xFFFFFF, false, false);
            else
                AddHtmlLocalized(20, 239, 315, 20, 1073634, 0xFFFFFF, false, false); // An aquarium decoration

            // item image
            AddItem(150, 80, item.ItemID, item.Hue);

            // item number / all items
            AddHtml(20, 195, 250, 20, String.Format("<BASEFONT COLOR=#FFFFFF>{0}/{1}</BASEFONT>", page, m_Aquarium.Items.Count), false, false);

            // remove item
            if (edit)
            {
                AddBackground(230, 195, 100, 26, 0x13BE);
                AddButton(235, 200, 0x845, 0x846, page, GumpButtonType.Reply, 0);
                AddHtmlLocalized(260, 198, 60, 26, 1073838, 0x0, false, false); // Remove
            }

            // next page
            if (page < m_Aquarium.Items.Count)
            {
                AddButton(195, 280, 0xFA5, 0xFA7, 0, GumpButtonType.Page, page + 1);
                AddHtmlLocalized(230, 283, 100, 18, 1044045, 0xFFFFFF, false, false); // NEXT PAGE
            }

            // previous page
            if (page > 1)
            {
                AddButton(45, 280, 0xFAE, 0xFAF, 0, GumpButtonType.Page, page - 1);
                AddHtmlLocalized(80, 283, 100, 18, 1044044, 0xFFFFFF, false, false); // PREV PAGE
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Aquarium == null || m_Aquarium.Deleted)
                return;

            bool edit = m_Aquarium.HasAccess(sender.Mobile);

            if (info.ButtonID > 0 && info.ButtonID <= m_Aquarium.Items.Count && edit)
                m_Aquarium.RemoveItem(sender.Mobile, info.ButtonID - 1);

            if (info.ButtonID > 0)
                sender.Mobile.SendGump(new AquariumGump(m_Aquarium, edit));
        }
    }

    public class AquariumEastDeed : BaseAddonContainerDeed
    {
        public override BaseAddonContainer Addon { get { return new Aquarium(0x3062); } }
        public override int LabelNumber { get { return 1074501; } } // Large Aquarium (east)

        [Constructable]
        public AquariumEastDeed()
            : base()
        {
        }

        public AquariumEastDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class AquariumNorthDeed : BaseAddonContainerDeed
    {
        public override BaseAddonContainer Addon { get { return new Aquarium(0x3060); } }
        public override int LabelNumber { get { return 1074497; } } // Large Aquarium (north)

        [Constructable]
        public AquariumNorthDeed()
            : base()
        {
        }

        public AquariumNorthDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    #region Aquarist Aquarium Trade Tools

    public class AquariumFishNet : SpecialFishingNet
    {
        public override int LabelNumber { get { return 1074463; } } // An aquarium fishing net

        [Constructable]
        public AquariumFishNet()
        {
            ItemID = 0xDC8;

            if (Hue == 0x8A0)
                Hue = 0x240;
        }

        protected override void AddNetProperties(ObjectPropertyList list)
        {
        }

        public override bool RequireDeepWater { get { return false; } }

        protected override void FinishEffect(Point3D p, Map map, Mobile from)
        {
            if (from.Skills.Fishing.Value < 10)
            {
                from.SendLocalizedMessage(1074487); // The creatures are too quick for you!
            }
            else
            {
                BaseAquariumFish fish = GiveFish(from);
                FishBowl bowl = Aquarium.GetEmptyBowl(from);

                if (bowl != null)
                {
                    fish.StopTimer();
                    bowl.AddItem(fish);
                    from.SendLocalizedMessage(1074489); // A live creature jumps into the fish bowl in your pack!
                    Delete();
                    return;
                }
                else
                {
                    if (from.PlaceInBackpack(fish))
                    {
                        from.PlaySound(0x5A2);
                        from.SendLocalizedMessage(1074490); // A live creature flops around in your pack before running out of air.

                        fish.Kill();
                        Delete();
                        return;
                    }
                    else
                    {
                        fish.Delete();

                        from.SendLocalizedMessage(1074488); // You could not hold the creature.
                    }
                }
            }

            InUse = false;
            Movable = true;

            if (!from.PlaceInBackpack(this))
            {
                if (from.Map == null || from.Map == Map.Internal)
                    Delete();
                else
                    MoveToWorld(from.Location, from.Map);
            }
        }

        private BaseAquariumFish GiveFish(Mobile from)
        {
            double skill = from.Skills.Fishing.Value;

            if ((skill / 100.0) >= Utility.RandomDouble())
            {
                int max = (int)skill / 5;

                if (max > 20)
                    max = 20;

                switch (Utility.Random(max))
                {
                    case 0: return new MinocBlueFish();
                    case 1: return new Shrimp();
                    case 2: return new FandancerFish();
                    case 3: return new GoldenBroadtail();
                    case 4: return new RedDartFish();
                    case 5: return new AlbinoCourtesanFish();
                    case 6: return new MakotoCourtesanFish();
                    case 7: return new NujelmHoneyFish();
                    case 8: return new Jellyfish();
                    case 9: return new SpeckledCrab();
                    case 10: return new LongClawCrab();
                    case 11: return new AlbinoFrog();
                    case 12: return new KillerFrog();
                    case 13: return new VesperReefTiger();
                    case 14: return new PurpleFrog();
                    case 15: return new BritainCrownFish();
                    case 16: return new YellowFinBluebelly();
                    case 17: return new SpottedBuccaneer();
                    case 18: return new SpinedScratcherFish();
                    default: return new SmallMouthSuckerFin();
                }
            }

            return new MinocBlueFish();
        }

        public AquariumFishNet(Serial serial)
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

    //Legacy Method For Older Servers
    public class AquariumFishingNet : Item
    {
        public override int LabelNumber { get { return 1074463; } } // An aquarium fishing net

        public AquariumFishingNet()
        {
        }

        public AquariumFishingNet(Serial serial)
            : base(serial)
        {
        }

        private Item CreateReplacement()
        {
            Item result = new AquariumFishNet();
            result.Hue = Hue;
            result.LootType = LootType;
            result.Movable = Movable;
            result.Name = Name;
            result.QuestItem = QuestItem;
            result.Visible = Visible;

            return result;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            Item replacement = CreateReplacement();

            if (!from.PlaceInBackpack(replacement))
            {
                replacement.Delete();
                from.SendLocalizedMessage(500720); // You don't have enough room in your backpack!
            }
            else
            {
                Delete();
                from.Use(replacement);
            }
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

    public class FishBowl : BaseContainer
    {
        public override int LabelNumber { get { return 1074499; } } // A fish bowl

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Empty
        {
            get { return (Items.Count == 0); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseAquariumFish Fish
        {
            get
            {
                if (Empty)
                    return null;

                if (Items[0] is BaseAquariumFish)
                    return (BaseAquariumFish)Items[0];

                return null;
            }
        }

        public override double DefaultWeight { get { return 2.0; } }

        [Constructable]
        public FishBowl()
            : base(0x241C)
        {
            Hue = 0x47E;
            MaxItems = 1;
        }

        public FishBowl(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
        }

        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (!CheckHold(from, dropped, sendFullMessage, true))
                return false;

            DropItem(dropped);
            return true;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!IsAccessibleTo(from))
            {
                from.SendLocalizedMessage(502436); // That is not accessible.
                return false;
            }

            if (!(dropped is BaseAquariumFish))
            {
                from.SendLocalizedMessage(1074836); // The container can not hold that type of object.
                return false;
            }

            if (base.OnDragDrop(from, dropped))
            {
                ((BaseAquariumFish)dropped).StopTimer();
                InvalidateProperties();

                return true;
            }

            return false;
        }

        public override bool CheckItemUse(Mobile from, Item item)
        {
            if (item != this)
                return false;

            return base.CheckItemUse(from, item);
        }

        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
        {
            if (item != this)
            {
                reject = LRReason.CannotLift;
                return false;
            }

            return base.CheckLift(from, item, ref reject);
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (!Empty)
            {
                BaseAquariumFish fish = Fish;

                if (fish != null)
                    list.Add(1074494, "#{0}", fish.LabelNumber); // Contains: ~1_CREATURE~
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (!Empty && IsAccessibleTo(from))
                list.Add(new RemoveCreature(this));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
                Weight = DefaultWeight;
        }

        private class RemoveCreature : ContextMenuEntry
        {
            private FishBowl m_Bowl;

            public RemoveCreature(FishBowl bowl)
                : base(6242, 3) // Remove creature
            {
                m_Bowl = bowl;
            }

            public override void OnClick()
            {
                if (m_Bowl == null || m_Bowl.Deleted || !m_Bowl.IsAccessibleTo(Owner.From))
                    return;

                BaseAquariumFish fish = m_Bowl.Fish;

                if (fish != null)
                {
                    if (fish.IsLockedDown) // for legacy fish bowls
                    {
                        Owner.From.SendLocalizedMessage(1010449); // You may not use this object while it is locked down.
                    }
                    else if (!Owner.From.PlaceInBackpack(fish))
                    {
                        Owner.From.SendLocalizedMessage(1074496); // There is no room in your pack for the creature.
                    }
                    else
                    {
                        Owner.From.SendLocalizedMessage(1074495); // The creature has been removed from the fish bowl.
                        fish.StartTimer();
                        m_Bowl.InvalidateProperties();
                    }
                }
            }
        }
    }

    #endregion

    #region Aquarist Aquarium Fish

    /// <summary>
    /// Common Aquarium Fish
    /// </summary>
    public class AlbinoCourtesanFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074592; } } // Albino Courtesan Fish

        [Constructable]
        public AlbinoCourtesanFish()
            : base(0x3B04)
        {
        }

        public AlbinoCourtesanFish(Serial serial)
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

    public class AlbinoFrog : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073824; } } // An Albino Frog

        [Constructable]
        public AlbinoFrog()
            : base(0x3B0D)
        {
            Hue = 0x47E;
        }

        public AlbinoFrog(Serial serial)
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

    public class BritainCrownFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074589; } } // Britain Crown Fish

        [Constructable]
        public BritainCrownFish()
            : base(0x3AFF)
        {
        }

        public BritainCrownFish(Serial serial)
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

    public class FandancerFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074591; } } // Fandancer Fish

        [Constructable]
        public FandancerFish()
            : base(0x3B02)
        {
        }

        public FandancerFish(Serial serial)
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

    public class GoldenBroadtail : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073828; } } // A Golden Broadtail

        [Constructable]
        public GoldenBroadtail()
            : base(0x3B03)
        {
        }

        public GoldenBroadtail(Serial serial)
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

    public class Jellyfish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074593; } } // Jellyfish

        [Constructable]
        public Jellyfish()
            : base(0x3B0E)
        {
        }

        public Jellyfish(Serial serial)
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

    public class KillerFrog : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073825; } } // A Killer Frog 

        [Constructable]
        public KillerFrog()
            : base(0x3B0D)
        {
        }

        public KillerFrog(Serial serial)
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

    public class LongClawCrab : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073827; } } // A Long Claw Crab 

        [Constructable]
        public LongClawCrab()
            : base(0x3AFC)
        {
            Hue = 0x527;
        }

        public LongClawCrab(Serial serial)
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

    public class MakotoCourtesanFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073835; } } // A Makoto Courtesan Fish

        [Constructable]
        public MakotoCourtesanFish()
            : base(0x3AFD)
        {
        }

        public MakotoCourtesanFish(Serial serial)
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

    public class MinocBlueFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073829; } } // A Minoc Blue Fish

        [Constructable]
        public MinocBlueFish()
            : base(0x3AFE)
        {
        }

        public MinocBlueFish(Serial serial)
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

    public class NujelmHoneyFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073830; } } // A Nujel'm Honey Fish

        [Constructable]
        public NujelmHoneyFish()
            : base(0x3B06)
        {
        }

        public NujelmHoneyFish(Serial serial)
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

    public class PurpleFrog : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073823; } } // A Purple Frog

        [Constructable]
        public PurpleFrog()
            : base(0x3B0D)
        {
            Hue = 0x4FA;
        }

        public PurpleFrog(Serial serial)
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

    public class RedDartFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073834; } } // A Red Dart Fish 

        [Constructable]
        public RedDartFish()
            : base(0x3B00)
        {
        }

        public RedDartFish(Serial serial)
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

    public class Shrimp : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074596; } } // Shrimp

        [Constructable]
        public Shrimp()
            : base(0x3B14)
        {
        }

        public Shrimp(Serial serial)
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

    public class SmallMouthSuckerFin : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074590; } } // Small Mouth Sucker Fin

        [Constructable]
        public SmallMouthSuckerFin()
            : base(0x3B01)
        {
        }

        public SmallMouthSuckerFin(Serial serial)
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

    public class SpeckledCrab : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073826; } } // A Speckled Crab 

        [Constructable]
        public SpeckledCrab()
            : base(0x3AFC)
        {
        }

        public SpeckledCrab(Serial serial)
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

    public class SpinedScratcherFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073832; } } // A Spined Scratcher Fish 

        [Constructable]
        public SpinedScratcherFish()
            : base(0x3B05)
        {
        }

        public SpinedScratcherFish(Serial serial)
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

    public class SpottedBuccaneer : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073833; } } // A Spotted Buccaneer

        [Constructable]
        public SpottedBuccaneer()
            : base(0x3B09)
        {
        }

        public SpottedBuccaneer(Serial serial)
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

    public class VesperReefTiger : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073836; } } // A Vesper Reef Tiger

        [Constructable]
        public VesperReefTiger()
            : base(0x3B08)
        {
        }

        public VesperReefTiger(Serial serial)
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

    public class YellowFinBluebelly : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1073831; } } // A Yellow Fin Bluebelly  

        [Constructable]
        public YellowFinBluebelly()
            : base(0x3B07)
        {
        }

        public YellowFinBluebelly(Serial serial)
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

    /// <summary>
    /// Reward Aquarium Fish
    /// </summary>
    public class BrineShrimp : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074415; } } // Brine shrimp

        [Constructable]
        public BrineShrimp()
            : base(0x3B11)
        {
        }

        public BrineShrimp(Serial serial)
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

    public class Coral : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074588; } } // Coral

        [Constructable]
        public Coral()
            : base(Utility.RandomList(0x3AF9, 0x3AFA, 0x3AFB))
        {
        }

        public Coral(Serial serial)
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

    public class FullMoonFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074597; } } // A Full Moon Fish

        [Constructable]
        public FullMoonFish()
            : base(0x3B15)
        {
        }

        public FullMoonFish(Serial serial)
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

    public class SeaHorseFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074414; } } // A sea horse

        [Constructable]
        public SeaHorseFish()
            : base(0x3B10)
        {
        }

        public SeaHorseFish(Serial serial)
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

    public class StrippedFlakeFish : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074595; } } // Stripped Flake Fish

        [Constructable]
        public StrippedFlakeFish()
            : base(0x3B0A)
        {
        }

        public StrippedFlakeFish(Serial serial)
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

    public class StrippedSosarianSwill : BaseAquariumFish
    {
        public override int LabelNumber { get { return 1074594; } } // Stripped Sosarian Swill

        [Constructable]
        public StrippedSosarianSwill()
            : base(0x3B0A)
        {
        }

        public StrippedSosarianSwill(Serial serial)
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

    #endregion

    #region Aquarist Aquarium Food

    public class AquariumFood : Item
    {
        public override int LabelNumber { get { return 1074819; } } // Aquarium food

        [Constructable]
        public AquariumFood()
            : base(0xEFC)
        {
        }

        public AquariumFood(Serial serial)
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

    public class VacationWafer : Item
    {
        public const int VacationDays = 7;

        public override int LabelNumber { get { return 1074431; } } // An aquarium flake sphere

        [Constructable]
        public VacationWafer()
            : base(0x973)
        {
        }

        public VacationWafer(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1074432, VacationDays.ToString()); // Vacation days: ~1_DAYS~
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1 && ItemID == 0x971)
                ItemID = 0x973;
        }
    }

    #endregion

    #region Aquarist Aquarium Tank Reward

    public class AquariumMessage : MessageInABottle
    {
        public override int LabelNumber { get { return 1073894; } } // Message in a Bottle

        [Constructable]
        public AquariumMessage()
            : base()
        {
        }

        public AquariumMessage(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    public class CaptainBlackheartsFishingPole : FishingPole
    {
        public override int LabelNumber { get { return 1074571; } } // Captain Blackheart's Fishing Pole

        [Constructable]
        public CaptainBlackheartsFishingPole()
            : base()
        {
        }

        public CaptainBlackheartsFishingPole(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    public class CraftysFishingHat : BaseHat
    {
        public override int LabelNumber { get { return 1074572; } } // Crafty's Fishing Hat

        public override int BasePhysicalResistance { get { return 0; } }
        public override int BaseFireResistance { get { return 5; } }
        public override int BaseColdResistance { get { return 9; } }
        public override int BasePoisonResistance { get { return 5; } }
        public override int BaseEnergyResistance { get { return 5; } }

        public override int InitMinHits { get { return 20; } }
        public override int InitMaxHits { get { return 30; } }

        [Constructable]
        public CraftysFishingHat()
            : base(0x1713)
        {
        }

        public CraftysFishingHat(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    public class FishBones : Item
    {
        public override int LabelNumber { get { return 1074601; } } // Fish bones
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public FishBones()
            : base(0x3B0C)
        {
        }

        public FishBones(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    public class IslandStatue : Item
    {
        public override int LabelNumber { get { return 1074600; } } // An island statue
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public IslandStatue()
            : base(0x3B0F)
        {
        }

        public IslandStatue(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    public class Shell : Item
    {
        public override int LabelNumber { get { return 1074598; } } // A shell
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public Shell()
            : base(Utility.RandomList(0x3B12, 0x3B13))
        {
        }

        public Shell(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    [FlipableAttribute(0x14F3, 0x14F4)]
    public class ToyBoat : Item
    {
        public override int LabelNumber { get { return 1074363; } } // A toy boat
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public ToyBoat()
            : base(0x14F4)
        {
        }

        public ToyBoat(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    public class WaterloggedBoots : BaseShoes
    {
        public override int LabelNumber { get { return 1074364; } } // Waterlogged boots

        [Constructable]
        public WaterloggedBoots()
            : base(0x1711)
        {
            if (Utility.RandomBool())
            {
                // thigh boots
                ItemID = 0x1711;
                Weight = 4.0;
            }
            else
            {
                // boots
                ItemID = 0x170B;
                Weight = 3.0;
            }
        }

        public WaterloggedBoots(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1073634); // An aquarium decoration
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

    #endregion
}