using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Targeting;

namespace Server
{
    public interface IChopable
    {
        void OnChop(Mobile from);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FurnitureAttribute : Attribute
    {
        public static bool Check(Item item)
        {
            return (item != null && item.GetType().IsDefined(typeof(FurnitureAttribute), false));
        }

        public FurnitureAttribute()
        {
        }
    }
}

namespace Server.Engines.Harvest
{
    public class HarvestDefinition
    {
        private int m_BankWidth, m_BankHeight;
        private int m_MinTotal, m_MaxTotal;
        private int[] m_Tiles;
        private bool m_RangedTiles;
        private TimeSpan m_MinRespawn, m_MaxRespawn;
        private int m_MaxRange;
        private int m_ConsumedPerHarvest, m_ConsumedPerFeluccaHarvest;
        private bool m_PlaceAtFeetIfFull;
        private SkillName m_Skill;
        private int[] m_EffectActions;
        private int[] m_EffectCounts;
        private int[] m_EffectSounds;
        private TimeSpan m_EffectSoundDelay;
        private TimeSpan m_EffectDelay;
        private object m_NoResourcesMessage, m_OutOfRangeMessage, m_TimedOutOfRangeMessage, m_DoubleHarvestMessage, m_FailMessage, m_PackFullMessage, m_ToolBrokeMessage;
        private HarvestResource[] m_Resources;
        private HarvestVein[] m_Veins;
        private BonusHarvestResource[] m_BonusResources;
        private bool m_RaceBonus;
        private bool m_RandomizeVeins;

        public int BankWidth { get { return m_BankWidth; } set { m_BankWidth = value; } }
        public int BankHeight { get { return m_BankHeight; } set { m_BankHeight = value; } }
        public int MinTotal { get { return m_MinTotal; } set { m_MinTotal = value; } }
        public int MaxTotal { get { return m_MaxTotal; } set { m_MaxTotal = value; } }
        public int[] Tiles { get { return m_Tiles; } set { m_Tiles = value; } }
        public bool RangedTiles { get { return m_RangedTiles; } set { m_RangedTiles = value; } }
        public TimeSpan MinRespawn { get { return m_MinRespawn; } set { m_MinRespawn = value; } }
        public TimeSpan MaxRespawn { get { return m_MaxRespawn; } set { m_MaxRespawn = value; } }
        public int MaxRange { get { return m_MaxRange; } set { m_MaxRange = value; } }
        public int ConsumedPerHarvest { get { return m_ConsumedPerHarvest; } set { m_ConsumedPerHarvest = value; } }
        public int ConsumedPerFeluccaHarvest { get { return m_ConsumedPerFeluccaHarvest; } set { m_ConsumedPerFeluccaHarvest = value; } }
        public bool PlaceAtFeetIfFull { get { return m_PlaceAtFeetIfFull; } set { m_PlaceAtFeetIfFull = value; } }
        public SkillName Skill { get { return m_Skill; } set { m_Skill = value; } }
        public int[] EffectActions { get { return m_EffectActions; } set { m_EffectActions = value; } }
        public int[] EffectCounts { get { return m_EffectCounts; } set { m_EffectCounts = value; } }
        public int[] EffectSounds { get { return m_EffectSounds; } set { m_EffectSounds = value; } }
        public TimeSpan EffectSoundDelay { get { return m_EffectSoundDelay; } set { m_EffectSoundDelay = value; } }
        public TimeSpan EffectDelay { get { return m_EffectDelay; } set { m_EffectDelay = value; } }
        public object NoResourcesMessage { get { return m_NoResourcesMessage; } set { m_NoResourcesMessage = value; } }
        public object OutOfRangeMessage { get { return m_OutOfRangeMessage; } set { m_OutOfRangeMessage = value; } }
        public object TimedOutOfRangeMessage { get { return m_TimedOutOfRangeMessage; } set { m_TimedOutOfRangeMessage = value; } }
        public object DoubleHarvestMessage { get { return m_DoubleHarvestMessage; } set { m_DoubleHarvestMessage = value; } }
        public object FailMessage { get { return m_FailMessage; } set { m_FailMessage = value; } }
        public object PackFullMessage { get { return m_PackFullMessage; } set { m_PackFullMessage = value; } }
        public object ToolBrokeMessage { get { return m_ToolBrokeMessage; } set { m_ToolBrokeMessage = value; } }
        public HarvestResource[] Resources { get { return m_Resources; } set { m_Resources = value; } }
        public HarvestVein[] Veins { get { return m_Veins; } set { m_Veins = value; } }
        public BonusHarvestResource[] BonusResources { get { return m_BonusResources; } set { m_BonusResources = value; } }
        public bool RaceBonus { get { return m_RaceBonus; } set { m_RaceBonus = value; } }
        public bool RandomizeVeins { get { return m_RandomizeVeins; } set { m_RandomizeVeins = value; } }

        private Dictionary<Map, Dictionary<Point2D, HarvestBank>> m_BanksByMap;

        public Dictionary<Map, Dictionary<Point2D, HarvestBank>> Banks { get { return m_BanksByMap; } set { m_BanksByMap = value; } }

        public void SendMessageTo(Mobile from, object message)
        {
            if (message is int)
                from.SendLocalizedMessage((int)message);
            else if (message is string)
                from.SendMessage((string)message);
        }

        public HarvestBank GetBank(Map map, int x, int y)
        {
            if (map == null || map == Map.Internal)
                return null;

            x /= m_BankWidth;
            y /= m_BankHeight;

            Dictionary<Point2D, HarvestBank> banks = null;
            m_BanksByMap.TryGetValue(map, out banks);

            if (banks == null)
                m_BanksByMap[map] = banks = new Dictionary<Point2D, HarvestBank>();

            Point2D key = new Point2D(x, y);
            HarvestBank bank = null;
            banks.TryGetValue(key, out bank);

            if (bank == null)
                banks[key] = bank = new HarvestBank(this, GetVeinAt(map, x, y));

            return bank;
        }

        public HarvestVein GetVeinAt(Map map, int x, int y)
        {
            if (m_Veins.Length == 1)
                return m_Veins[0];

            double randomValue;

            if (m_RandomizeVeins)
            {
                randomValue = Utility.RandomDouble();
            }
            else
            {
                Random random = new Random((x * 17) + (y * 11) + (map.MapID * 3));
                randomValue = random.NextDouble();
            }

            return GetVeinFrom(randomValue);
        }

        public HarvestVein GetVeinFrom(double randomValue)
        {
            if (m_Veins.Length == 1)
                return m_Veins[0];

            randomValue *= 100;

            for (int i = 0; i < m_Veins.Length; ++i)
            {
                if (randomValue <= m_Veins[i].VeinChance)
                    return m_Veins[i];

                randomValue -= m_Veins[i].VeinChance;
            }

            return null;
        }

        public BonusHarvestResource GetBonusResource()
        {
            if (m_BonusResources == null)
                return null;

            double randomValue = Utility.RandomDouble() * 100;

            for (int i = 0; i < m_BonusResources.Length; ++i)
            {
                if (randomValue <= m_BonusResources[i].Chance)
                    return m_BonusResources[i];

                randomValue -= m_BonusResources[i].Chance;
            }

            return null;
        }

        public HarvestDefinition()
        {
            m_BanksByMap = new Dictionary<Map, Dictionary<Point2D, HarvestBank>>();
        }

        public bool Validate(int tileID)
        {
            if (m_RangedTiles)
            {
                bool contains = false;

                for (int i = 0; !contains && i < m_Tiles.Length; i += 2)
                    contains = (tileID >= m_Tiles[i] && tileID <= m_Tiles[i + 1]);

                return contains;
            }
            else
            {
                int dist = -1;

                for (int i = 0; dist < 0 && i < m_Tiles.Length; ++i)
                    dist = (m_Tiles[i] - tileID);

                return (dist == 0);
            }
        }
    }

    public abstract class HarvestSystem
    {
        private List<HarvestDefinition> m_Definitions;

        public List<HarvestDefinition> Definitions { get { return m_Definitions; } }

        public HarvestSystem()
        {
            m_Definitions = new List<HarvestDefinition>();
        }

        public virtual bool CheckTool(Mobile from, Item tool)
        {
            bool wornOut = (tool == null || tool.Deleted || (tool is IUsesRemaining && ((IUsesRemaining)tool).UsesRemaining <= 0));

            if (wornOut)
                from.SendLocalizedMessage(1044038); // You have worn out your tool!

            return !wornOut;
        }

        public virtual bool CheckHarvest(Mobile from, Item tool)
        {
            return CheckTool(from, tool);
        }

        public virtual bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            return CheckTool(from, tool);
        }

        public virtual bool CheckRange(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            bool inRange = (from.Map == map && from.InRange(loc, def.MaxRange));

            if (!inRange)
                def.SendMessageTo(from, timed ? def.TimedOutOfRangeMessage : def.OutOfRangeMessage);

            return inRange;
        }

        public virtual bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            HarvestBank bank = def.GetBank(map, loc.X, loc.Y);
            bool available = (bank != null && bank.Current >= def.ConsumedPerHarvest);

            if (!available)
                def.SendMessageTo(from, timed ? def.DoubleHarvestMessage : def.NoResourcesMessage);

            return available;
        }

        public virtual void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest)
        {
        }

        public virtual object GetLock(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            /* Here we prevent multiple harvesting.
             * 
             * Some options:
             *  - 'return tool;' : This will allow the player to harvest more than once concurrently, but only if they use multiple tools. This seems to be as OSI.
             *  - 'return GetType();' : This will disallow multiple harvesting of the same type. That is, we couldn't mine more than once concurrently, but we could be both mining and lumberjacking.
             *  - 'return typeof( HarvestSystem );' : This will completely restrict concurrent harvesting.
             */

            return tool;
        }

        public virtual void OnConcurrentHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
        }

        public virtual void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
        }

        public virtual bool BeginHarvesting(Mobile from, Item tool)
        {
            if (!CheckHarvest(from, tool))
                return false;

            from.Target = new HarvestTarget(tool, this);
            return true;
        }

        public virtual void FinishHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked)
        {
            from.EndAction(locked);

            if (!CheckHarvest(from, tool))
                return;

            int tileID;
            Map map;
            Point3D loc;

            if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }
            else if (!def.Validate(tileID))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            if (!CheckRange(from, tool, def, map, loc, true))
                return;
            else if (!CheckResources(from, tool, def, map, loc, true))
                return;
            else if (!CheckHarvest(from, tool, def, toHarvest))
                return;

            if (SpecialHarvest(from, tool, def, map, loc))
                return;

            HarvestBank bank = def.GetBank(map, loc.X, loc.Y);

            if (bank == null)
                return;

            HarvestVein vein = bank.Vein;

            if (vein != null)
                vein = MutateVein(from, tool, def, bank, toHarvest, vein);

            if (vein == null)
                return;

            HarvestResource primary = vein.PrimaryResource;
            HarvestResource fallback = vein.FallbackResource;
            HarvestResource resource = MutateResource(from, tool, def, map, loc, vein, primary, fallback);

            double skillBase = from.Skills[def.Skill].Base;
            double skillValue = from.Skills[def.Skill].Value;

            Type type = null;

            if (skillBase >= resource.ReqSkill && from.CheckSkill(def.Skill, resource.MinSkill, resource.MaxSkill))
            {
                type = GetResourceType(from, tool, def, map, loc, resource);

                if (type != null)
                    type = MutateType(type, from, tool, def, map, loc, resource);

                if (type != null)
                {
                    Item item = Construct(type, from);

                    if (item == null)
                    {
                        type = null;
                    }
                    else
                    {
                        //The whole harvest system is kludgy and I'm sure this is just adding to it.
                        if (item.Stackable)
                        {
                            int amount = def.ConsumedPerHarvest;
                            int feluccaAmount = def.ConsumedPerFeluccaHarvest;

                            int racialAmount = (int)Math.Ceiling(amount * 1.1);
                            int feluccaRacialAmount = (int)Math.Ceiling(feluccaAmount * 1.1);

                            bool eligableForRacialBonus = (def.RaceBonus && from.Race == Race.Human);
                            bool inFelucca = (map == Map.Felucca);

                            if (eligableForRacialBonus && inFelucca && bank.Current >= feluccaRacialAmount && 0.1 > Utility.RandomDouble())
                                item.Amount = feluccaRacialAmount;
                            else if (inFelucca && bank.Current >= feluccaAmount)
                                item.Amount = feluccaAmount;
                            else if (eligableForRacialBonus && bank.Current >= racialAmount && 0.1 > Utility.RandomDouble())
                                item.Amount = racialAmount;
                            else
                                item.Amount = amount;
                        }

                        bank.Consume(item.Amount, from);

                        if (Give(from, item, def.PlaceAtFeetIfFull))
                        {
                            SendSuccessTo(from, item, resource);
                        }
                        else
                        {
                            SendPackFullTo(from, item, def, resource);
                            item.Delete();
                        }

                        BonusHarvestResource bonus = def.GetBonusResource();

                        if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                        {
                            Item bonusItem = Construct(bonus.Type, from);

                            if (Give(from, bonusItem, true))	//Bonuses always allow placing at feet, even if pack is full irregrdless of def
                            {
                                bonus.SendSuccessTo(from);
                            }
                            else
                            {
                                item.Delete();
                            }
                        }

                        if (tool is IUsesRemaining)
                        {
                            IUsesRemaining toolWithUses = (IUsesRemaining)tool;

                            toolWithUses.ShowUsesRemaining = true;

                            if (toolWithUses.UsesRemaining > 0)
                                --toolWithUses.UsesRemaining;

                            if (toolWithUses.UsesRemaining < 1)
                            {
                                tool.Delete();
                                def.SendMessageTo(from, def.ToolBrokeMessage);
                            }
                        }
                    }
                }
            }

            if (type == null)
                def.SendMessageTo(from, def.FailMessage);

            OnHarvestFinished(from, tool, def, vein, bank, resource, toHarvest);
        }

        public virtual void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested)
        {
        }

        public virtual bool SpecialHarvest(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
        {
            return false;
        }

        public virtual Item Construct(Type type, Mobile from)
        {
            try { return Activator.CreateInstance(type) as Item; }
            catch { return null; }
        }

        public virtual HarvestVein MutateVein(Mobile from, Item tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein)
        {
            return vein;
        }

        public virtual void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
        {
            resource.SendSuccessTo(from);
        }

        public virtual void SendPackFullTo(Mobile from, Item item, HarvestDefinition def, HarvestResource resource)
        {
            def.SendMessageTo(from, def.PackFullMessage);
        }

        public virtual bool Give(Mobile m, Item item, bool placeAtFeet)
        {
            if (m.PlaceInBackpack(item))
                return true;

            if (!placeAtFeet)
                return false;

            Map map = m.Map;

            if (map == null)
                return false;

            List<Item> atFeet = new List<Item>();

            foreach (Item obj in m.GetItemsInRange(0))
                atFeet.Add(obj);

            for (int i = 0; i < atFeet.Count; ++i)
            {
                Item check = atFeet[i];

                if (check.StackWith(m, item, false))
                    return true;
            }

            item.MoveToWorld(m.Location, map);
            return true;
        }

        public virtual Type MutateType(Type type, Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
        {
            return from.Region.GetResource(type);
        }

        public virtual Type GetResourceType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
        {
            if (resource.Types.Length > 0)
                return resource.Types[Utility.Random(resource.Types.Length)];

            return null;
        }

        public virtual HarvestResource MutateResource(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestVein vein, HarvestResource primary, HarvestResource fallback)
        {
            bool racialBonus = (def.RaceBonus && from.Race == Race.Elf);

            if (vein.ChanceToFallback > (Utility.RandomDouble() + (racialBonus ? .20 : 0)))
                return fallback;

            double skillValue = from.Skills[def.Skill].Value;

            if (fallback != null && (skillValue < primary.ReqSkill || skillValue < primary.MinSkill))
                return fallback;

            return primary;
        }

        public virtual bool OnHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked, bool last)
        {
            if (!CheckHarvest(from, tool))
            {
                from.EndAction(locked);
                return false;
            }

            int tileID;
            Map map;
            Point3D loc;

            if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
            {
                from.EndAction(locked);
                OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (!def.Validate(tileID))
            {
                from.EndAction(locked);
                OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (!CheckRange(from, tool, def, map, loc, true))
            {
                from.EndAction(locked);
                return false;
            }
            else if (!CheckResources(from, tool, def, map, loc, true))
            {
                from.EndAction(locked);
                return false;
            }
            else if (!CheckHarvest(from, tool, def, toHarvest))
            {
                from.EndAction(locked);
                return false;
            }

            DoHarvestingEffect(from, tool, def, map, loc);

            new HarvestSoundTimer(from, tool, this, def, toHarvest, locked, last).Start();

            return !last;
        }

        public virtual void DoHarvestingSound(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (def.EffectSounds.Length > 0)
                from.PlaySound(Utility.RandomList(def.EffectSounds));
        }

        public virtual void DoHarvestingEffect(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
        {
            from.Direction = from.GetDirectionTo(loc);

            if (!from.Mounted)
                from.Animate(Utility.RandomList(def.EffectActions), 5, 1, true, false, 0);
        }

        public virtual HarvestDefinition GetDefinition(int tileID)
        {
            HarvestDefinition def = null;

            for (int i = 0; def == null && i < m_Definitions.Count; ++i)
            {
                HarvestDefinition check = m_Definitions[i];

                if (check.Validate(tileID))
                    def = check;
            }

            return def;
        }

        public virtual void StartHarvesting(Mobile from, Item tool, object toHarvest)
        {
            if (!CheckHarvest(from, tool))
                return;

            int tileID;
            Map map;
            Point3D loc;

            if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            HarvestDefinition def = GetDefinition(tileID);

            if (def == null)
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            if (!CheckRange(from, tool, def, map, loc, false))
                return;
            else if (!CheckResources(from, tool, def, map, loc, false))
                return;
            else if (!CheckHarvest(from, tool, def, toHarvest))
                return;

            object toLock = GetLock(from, tool, def, toHarvest);

            if (!from.BeginAction(toLock))
            {
                OnConcurrentHarvest(from, tool, def, toHarvest);
                return;
            }

            new HarvestTimer(from, tool, this, def, toHarvest, toLock).Start();
            OnHarvestStarted(from, tool, def, toHarvest);
        }

        public virtual bool GetHarvestDetails(Mobile from, Item tool, object toHarvest, out int tileID, out Map map, out Point3D loc)
        {
            if (toHarvest is Static && !((Static)toHarvest).Movable)
            {
                Static obj = (Static)toHarvest;

                tileID = (obj.ItemID & 0x3FFF) | 0x4000;
                map = obj.Map;
                loc = obj.GetWorldLocation();
            }
            else if (toHarvest is StaticTarget)
            {
                StaticTarget obj = (StaticTarget)toHarvest;

                tileID = (obj.ItemID & 0x3FFF) | 0x4000;
                map = from.Map;
                loc = obj.Location;
            }
            else if (toHarvest is LandTarget)
            {
                LandTarget obj = (LandTarget)toHarvest;

                tileID = obj.TileID;
                map = from.Map;
                loc = obj.Location;
            }
            else
            {
                tileID = 0;
                map = null;
                loc = Point3D.Zero;
                return false;
            }

            return (map != null && map != Map.Internal);
        }
    }

    public class HarvestResource
    {
        private Type[] m_Types;
        private double m_ReqSkill, m_MinSkill, m_MaxSkill;
        private object m_SuccessMessage;

        public Type[] Types { get { return m_Types; } set { m_Types = value; } }
        public double ReqSkill { get { return m_ReqSkill; } set { m_ReqSkill = value; } }
        public double MinSkill { get { return m_MinSkill; } set { m_MinSkill = value; } }
        public double MaxSkill { get { return m_MaxSkill; } set { m_MaxSkill = value; } }
        public object SuccessMessage { get { return m_SuccessMessage; } }

        public void SendSuccessTo(Mobile m)
        {
            if (m_SuccessMessage is int)
                m.SendLocalizedMessage((int)m_SuccessMessage);
            else if (m_SuccessMessage is string)
                m.SendMessage((string)m_SuccessMessage);
        }

        public HarvestResource(double reqSkill, double minSkill, double maxSkill, object message, params Type[] types)
        {
            m_ReqSkill = reqSkill;
            m_MinSkill = minSkill;
            m_MaxSkill = maxSkill;
            m_Types = types;
            m_SuccessMessage = message;
        }
    }

    public class BonusHarvestResource
    {
        private Type m_Type;
        private double m_ReqSkill, m_Chance;
        private TextDefinition m_SuccessMessage;

        public Type Type { get { return m_Type; } set { m_Type = value; } }
        public double ReqSkill { get { return m_ReqSkill; } set { m_ReqSkill = value; } }
        public double Chance { get { return m_Chance; } set { m_Chance = value; } }

        public TextDefinition SuccessMessage { get { return m_SuccessMessage; } }

        public void SendSuccessTo(Mobile m)
        {
            TextDefinition.SendMessageTo(m, m_SuccessMessage);
        }

        public BonusHarvestResource(double reqSkill, double chance, TextDefinition message, Type type)
        {
            m_ReqSkill = reqSkill;

            m_Chance = chance;
            m_Type = type;
            m_SuccessMessage = message;
        }
    }

    public class HarvestBank
    {
        private int m_Current;
        private int m_Maximum;
        private DateTime m_NextRespawn;
        private HarvestVein m_Vein, m_DefaultVein;

        HarvestDefinition m_Definition;

        public HarvestDefinition Definition
        {
            get { return m_Definition; }
        }

        public int Current
        {
            get
            {
                CheckRespawn();
                return m_Current;
            }
        }

        public HarvestVein Vein
        {
            get
            {
                CheckRespawn();
                return m_Vein;
            }
            set
            {
                m_Vein = value;
            }
        }

        public HarvestVein DefaultVein
        {
            get
            {
                CheckRespawn();
                return m_DefaultVein;
            }
        }

        public void CheckRespawn()
        {
            if (m_Current == m_Maximum || m_NextRespawn > DateTime.UtcNow)
                return;

            m_Current = m_Maximum;

            if (m_Definition.RandomizeVeins)
            {
                m_DefaultVein = m_Definition.GetVeinFrom(Utility.RandomDouble());
            }

            m_Vein = m_DefaultVein;
        }

        public void Consume(int amount, Mobile from)
        {
            CheckRespawn();

            if (m_Current == m_Maximum)
            {
                double min = m_Definition.MinRespawn.TotalMinutes;
                double max = m_Definition.MaxRespawn.TotalMinutes;
                double rnd = Utility.RandomDouble();

                m_Current = m_Maximum - amount;

                double minutes = min + (rnd * (max - min));
                if (m_Definition.RaceBonus && from.Race == Race.Elf)	//def.RaceBonus = Core.ML
                    minutes *= .75;	//25% off the time.  

                m_NextRespawn = DateTime.UtcNow + TimeSpan.FromMinutes(minutes);
            }
            else
            {
                m_Current -= amount;
            }

            if (m_Current < 0)
                m_Current = 0;
        }

        public HarvestBank(HarvestDefinition def, HarvestVein defaultVein)
        {
            m_Maximum = Utility.RandomMinMax(def.MinTotal, def.MaxTotal);
            m_Current = m_Maximum;
            m_DefaultVein = defaultVein;
            m_Vein = m_DefaultVein;

            m_Definition = def;
        }
    }

    public class HarvestVein
    {
        private double m_VeinChance;
        private double m_ChanceToFallback;
        private HarvestResource m_PrimaryResource;
        private HarvestResource m_FallbackResource;

        public double VeinChance { get { return m_VeinChance; } set { m_VeinChance = value; } }
        public double ChanceToFallback { get { return m_ChanceToFallback; } set { m_ChanceToFallback = value; } }
        public HarvestResource PrimaryResource { get { return m_PrimaryResource; } set { m_PrimaryResource = value; } }
        public HarvestResource FallbackResource { get { return m_FallbackResource; } set { m_FallbackResource = value; } }

        public HarvestVein(double veinChance, double chanceToFallback, HarvestResource primaryResource, HarvestResource fallbackResource)
        {
            m_VeinChance = veinChance;
            m_ChanceToFallback = chanceToFallback;
            m_PrimaryResource = primaryResource;
            m_FallbackResource = fallbackResource;
        }
    }

    public class HarvestTarget : Target
    {
        private Item m_Tool;
        private HarvestSystem m_System;

        public HarvestTarget(Item tool, HarvestSystem system)
            : base(-1, true, TargetFlags.None)
        {
            m_Tool = tool;
            m_System = system;

            DisallowMultis = true;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_System is Mining && targeted is StaticTarget)
            {
                int itemID = ((StaticTarget)targeted).ItemID;

                // grave
                if (itemID == 0xED3 || itemID == 0xEDF || itemID == 0xEE0 || itemID == 0xEE1 || itemID == 0xEE2 || itemID == 0xEE8)
                {
                    PlayerMobile player = from as PlayerMobile;

                    if (player != null)
                    {
                        QuestSystem qs = player.Quest;

                        if (qs is WitchApprenticeQuest)
                        {
                            FindIngredientObjective obj = qs.FindObjective(typeof(FindIngredientObjective)) as FindIngredientObjective;

                            if (obj != null && !obj.Completed && obj.Ingredient == Ingredient.Bones)
                            {
                                player.SendLocalizedMessage(1055037); // You finish your grim work, finding some of the specific bones listed in the Hag's recipe.
                                obj.Complete();

                                return;
                            }
                        }
                    }
                }
            }

            if (m_System is Lumberjacking && targeted is IChopable)
                ((IChopable)targeted).OnChop(from);
            else if (m_System is Lumberjacking && targeted is IAxe && m_Tool is BaseAxe)
            {
                IAxe obj = (IAxe)targeted;
                Item item = (Item)targeted;

                if (!item.IsChildOf(from.Backpack))
                    from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
                else if (obj.Axe(from, (BaseAxe)m_Tool))
                    from.PlaySound(0x13E);
            }
            else if (m_System is Lumberjacking && targeted is ICarvable)
                ((ICarvable)targeted).Carve(from, (Item)m_Tool);
            else if (m_System is Lumberjacking && FurnitureAttribute.Check(targeted as Item))
                DestroyFurniture(from, (Item)targeted);
            else if (m_System is Mining && targeted is TreasureMap)
                ((TreasureMap)targeted).OnBeginDig(from);
            else
                m_System.StartHarvesting(from, m_Tool, targeted);
        }

        private void DestroyFurniture(Mobile from, Item item)
        {
            if (!from.InRange(item.GetWorldLocation(), 3))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }
            else if (!item.IsChildOf(from.Backpack) && !item.Movable)
            {
                from.SendLocalizedMessage(500462); // You can't destroy that while it is here.
                return;
            }

            from.SendLocalizedMessage(500461); // You destroy the item.
            Effects.PlaySound(item.GetWorldLocation(), item.Map, 0x3B3);

            if (item is Container)
            {
                if (item is TrapableContainer)
                    (item as TrapableContainer).ExecuteTrap(from);

                ((Container)item).Destroy();
            }
            else
            {
                item.Delete();
            }
        }
    }

    public class HarvestTimer : Timer
    {
        private Mobile m_From;
        private Item m_Tool;
        private HarvestSystem m_System;
        private HarvestDefinition m_Definition;
        private object m_ToHarvest, m_Locked;
        private int m_Index, m_Count;

        public HarvestTimer(Mobile from, Item tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked)
            : base(TimeSpan.Zero, def.EffectDelay)
        {
            m_From = from;
            m_Tool = tool;
            m_System = system;
            m_Definition = def;
            m_ToHarvest = toHarvest;
            m_Locked = locked;
            m_Count = Utility.RandomList(def.EffectCounts);
        }

        protected override void OnTick()
        {
            if (!m_System.OnHarvesting(m_From, m_Tool, m_Definition, m_ToHarvest, m_Locked, ++m_Index == m_Count))
                Stop();
        }
    }

    public class HarvestSoundTimer : Timer
    {
        private Mobile m_From;
        private Item m_Tool;
        private HarvestSystem m_System;
        private HarvestDefinition m_Definition;
        private object m_ToHarvest, m_Locked;
        private bool m_Last;

        public HarvestSoundTimer(Mobile from, Item tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked, bool last)
            : base(def.EffectSoundDelay)
        {
            m_From = from;
            m_Tool = tool;
            m_System = system;
            m_Definition = def;
            m_ToHarvest = toHarvest;
            m_Locked = locked;
            m_Last = last;
        }

        protected override void OnTick()
        {
            m_System.DoHarvestingSound(m_From, m_Tool, m_Definition, m_ToHarvest);

            if (m_Last)
                m_System.FinishHarvesting(m_From, m_Tool, m_Definition, m_ToHarvest, m_Locked);
        }
    }
}

namespace Server.Items
{
    public abstract class FarmableCrop : Item
    {
        private bool m_Picked;

        public abstract Item GetCropObject();
        public abstract int GetPickedID();

        public FarmableCrop(int itemID)
            : base(itemID)
        {
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            Map map = this.Map;
            Point3D loc = this.Location;

            if (Parent != null || Movable || IsLockedDown || IsSecure || map == null || map == Map.Internal)
                return;

            if (!from.InRange(loc, 2) || !from.InLOS(this))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (!m_Picked)
                OnPicked(from, loc, map);
        }

        public virtual void OnPicked(Mobile from, Point3D loc, Map map)
        {
            ItemID = GetPickedID();

            Item spawn = GetCropObject();

            if (spawn != null)
                spawn.MoveToWorld(loc, map);

            m_Picked = true;

            Unlink();

            Timer.DelayCall(TimeSpan.FromMinutes(5.0), new TimerCallback(Delete));
        }

        public void Unlink()
        {
            ISpawner se = this.Spawner;

            if (se != null)
            {
                this.Spawner.Remove(this);
                this.Spawner = null;
            }

        }

        public FarmableCrop(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write(m_Picked);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    m_Picked = reader.ReadBool();
                    break;
            }
            if (m_Picked)
            {
                Unlink();
                Delete();
            }
        }
    }
}