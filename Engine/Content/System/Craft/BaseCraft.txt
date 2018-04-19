using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Commands;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Craft
{
    public enum CraftECA
    {
        ChanceMinusSixty,
        FiftyPercentChanceMinusTenPercent,
        ChanceMinusSixtyToFourtyFive
    }

    public abstract class CraftSystem
    {
        private int m_MinCraftEffect;
        private int m_MaxCraftEffect;
        private double m_Delay;
        private bool m_Resmelt;
        private bool m_Repair;
        private bool m_MarkOption;
        private bool m_CanEnhance;

        private CraftItemCol m_CraftItems;
        private CraftGroupCol m_CraftGroups;
        private CraftSubResCol m_CraftSubRes;
        private CraftSubResCol m_CraftSubRes2;

        private List<int> m_Recipes;
        private List<int> m_RareRecipes;

        public int MinCraftEffect { get { return m_MinCraftEffect; } }
        public int MaxCraftEffect { get { return m_MaxCraftEffect; } }
        public double Delay { get { return m_Delay; } }

        public CraftItemCol CraftItems { get { return m_CraftItems; } }
        public CraftGroupCol CraftGroups { get { return m_CraftGroups; } }
        public CraftSubResCol CraftSubRes { get { return m_CraftSubRes; } }
        public CraftSubResCol CraftSubRes2 { get { return m_CraftSubRes2; } }

        public abstract SkillName MainSkill { get; }

        public virtual int GumpTitleNumber { get { return 0; } }
        public virtual string GumpTitleString { get { return ""; } }

        public virtual CraftECA ECA { get { return CraftECA.ChanceMinusSixty; } }

        private Dictionary<Mobile, CraftContext> m_ContextTable = new Dictionary<Mobile, CraftContext>();

        public abstract double GetChanceAtMin(CraftItem item);

        public virtual bool RetainsColorFrom(CraftItem item, Type type)
        {
            return false;
        }

        public CraftContext GetContext(Mobile m)
        {
            if (m == null)
                return null;

            if (m.Deleted)
            {
                m_ContextTable.Remove(m);
                return null;
            }

            CraftContext c = null;
            m_ContextTable.TryGetValue(m, out c);

            if (c == null)
                m_ContextTable[m] = c = new CraftContext();

            return c;
        }

        public void OnMade(Mobile m, CraftItem item)
        {
            CraftContext c = GetContext(m);

            if (c != null)
                c.OnMade(item);
        }

        public bool Resmelt
        {
            get { return m_Resmelt; }
            set { m_Resmelt = value; }
        }

        public bool Repair
        {
            get { return m_Repair; }
            set { m_Repair = value; }
        }

        public bool MarkOption
        {
            get { return m_MarkOption; }
            set { m_MarkOption = value; }
        }

        public bool CanEnhance
        {
            get { return m_CanEnhance; }
            set { m_CanEnhance = value; }
        }

        public CraftSystem(int minCraftEffect, int maxCraftEffect, double delay)
        {
            m_MinCraftEffect = minCraftEffect;
            m_MaxCraftEffect = maxCraftEffect;
            m_Delay = delay;

            m_CraftItems = new CraftItemCol();
            m_CraftGroups = new CraftGroupCol();
            m_CraftSubRes = new CraftSubResCol();
            m_CraftSubRes2 = new CraftSubResCol();

            m_Recipes = new List<int>();
            m_RareRecipes = new List<int>();

            InitCraftList();
        }

        public virtual bool ConsumeOnFailure(Mobile from, Type resourceType, CraftItem craftItem)
        {
            return true;
        }

        public void CreateItem(Mobile from, Type type, Type typeRes, BaseTool tool, CraftItem realCraftItem)
        {
            // Verify if the type is in the list of the craftable item
            CraftItem craftItem = m_CraftItems.SearchFor(type);
            if (craftItem != null)
            {
                // The item is in the list, try to create it
                // Test code: items like sextant parts can be crafted either directly from ingots, or from different parts
                realCraftItem.Craft(from, this, typeRes, tool);
                //craftItem.Craft( from, this, typeRes, tool );
            }
        }

        public int RandomRecipe()
        {
            if (m_Recipes.Count == 0)
                return -1;

            return m_Recipes[Utility.Random(m_Recipes.Count)];
        }

        public int RandomRareRecipe()
        {
            if (m_RareRecipes.Count == 0)
                return -1;

            return m_RareRecipes[Utility.Random(m_RareRecipes.Count)];
        }


        public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
        {
            return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, "");
        }

        public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
        {
            return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, message);
        }

        public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
        {
            return AddCraft(typeItem, group, name, skillToMake, minSkill, maxSkill, typeRes, nameRes, amount, "");
        }

        public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
        {
            CraftItem craftItem = new CraftItem(typeItem, group, name);
            craftItem.AddRes(typeRes, nameRes, amount, message);
            craftItem.AddSkill(skillToMake, minSkill, maxSkill);

            DoGroup(group, craftItem);
            return m_CraftItems.Add(craftItem);
        }


        private void DoGroup(TextDefinition groupName, CraftItem craftItem)
        {
            int index = m_CraftGroups.SearchFor(groupName);

            if (index == -1)
            {
                CraftGroup craftGroup = new CraftGroup(groupName);
                craftGroup.AddCraftItem(craftItem);
                m_CraftGroups.Add(craftGroup);
            }
            else
            {
                m_CraftGroups.GetAt(index).AddCraftItem(craftItem);
            }
        }


        public void SetItemHue(int index, int hue)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.ItemHue = hue;
        }

        public void SetManaReq(int index, int mana)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.Mana = mana;
        }

        public void SetStamReq(int index, int stam)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.Stam = stam;
        }

        public void SetHitsReq(int index, int hits)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.Hits = hits;
        }

        public void SetUseAllRes(int index, bool useAll)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.UseAllRes = useAll;
        }

        public void SetNeedHeat(int index, bool needHeat)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.NeedHeat = needHeat;
        }

        public void SetNeedOven(int index, bool needOven)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.NeedOven = needOven;
        }

        public void SetBeverageType(int index, BeverageType requiredBeverage)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.RequiredBeverage = requiredBeverage;
        }

        public void SetNeedMill(int index, bool needMill)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.NeedMill = needMill;
        }

        public void SetNeededExpansion(int index, Expansion expansion)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.RequiredExpansion = expansion;
        }

        public void AddRes(int index, Type type, TextDefinition name, int amount)
        {
            AddRes(index, type, name, amount, "");
        }

        public void AddRes(int index, Type type, TextDefinition name, int amount, TextDefinition message)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.AddRes(type, name, amount, message);
        }

        public void AddSkill(int index, SkillName skillToMake, double minSkill, double maxSkill)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.AddSkill(skillToMake, minSkill, maxSkill);
        }

        public void SetUseSubRes2(int index, bool val)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.UseSubRes2 = val;
        }

        private void AddRecipeBase(int index, int id)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.AddRecipe(id, this);
        }

        public void AddRecipe(int index, int id)
        {
            AddRecipeBase(index, id);
            m_Recipes.Add(id);
        }

        public void AddRareRecipe(int index, int id)
        {
            AddRecipeBase(index, id);
            m_RareRecipes.Add(id);
        }

        public void AddQuestRecipe(int index, int id)
        {
            AddRecipeBase(index, id);
        }

        public void ForceNonExceptional(int index)
        {
            CraftItem craftItem = m_CraftItems.GetAt(index);
            craftItem.ForceNonExceptional = true;
        }


        public void SetSubRes(Type type, string name)
        {
            m_CraftSubRes.ResType = type;
            m_CraftSubRes.NameString = name;
            m_CraftSubRes.Init = true;
        }

        public void SetSubRes(Type type, int name)
        {
            m_CraftSubRes.ResType = type;
            m_CraftSubRes.NameNumber = name;
            m_CraftSubRes.Init = true;
        }

        public void AddSubRes(Type type, int name, double reqSkill, object message)
        {
            CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
            m_CraftSubRes.Add(craftSubRes);
        }

        public void AddSubRes(Type type, int name, double reqSkill, int genericName, object message)
        {
            CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, genericName, message);
            m_CraftSubRes.Add(craftSubRes);
        }

        public void AddSubRes(Type type, string name, double reqSkill, object message)
        {
            CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
            m_CraftSubRes.Add(craftSubRes);
        }


        public void SetSubRes2(Type type, string name)
        {
            m_CraftSubRes2.ResType = type;
            m_CraftSubRes2.NameString = name;
            m_CraftSubRes2.Init = true;
        }

        public void SetSubRes2(Type type, int name)
        {
            m_CraftSubRes2.ResType = type;
            m_CraftSubRes2.NameNumber = name;
            m_CraftSubRes2.Init = true;
        }

        public void AddSubRes2(Type type, int name, double reqSkill, object message)
        {
            CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
            m_CraftSubRes2.Add(craftSubRes);
        }

        public void AddSubRes2(Type type, int name, double reqSkill, int genericName, object message)
        {
            CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, genericName, message);
            m_CraftSubRes2.Add(craftSubRes);
        }

        public void AddSubRes2(Type type, string name, double reqSkill, object message)
        {
            CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
            m_CraftSubRes2.Add(craftSubRes);
        }

        public abstract void InitCraftList();

        public abstract void PlayCraftEffect(Mobile from);
        public abstract int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item);

        public abstract int CanCraft(Mobile from, BaseTool tool, Type itemType);
    }

    public abstract class CustomCraft
    {
        private Mobile m_From;
        private CraftItem m_CraftItem;
        private CraftSystem m_CraftSystem;
        private Type m_TypeRes;
        private BaseTool m_Tool;
        private int m_Quality;

        public Mobile From { get { return m_From; } }
        public CraftItem CraftItem { get { return m_CraftItem; } }
        public CraftSystem CraftSystem { get { return m_CraftSystem; } }
        public Type TypeRes { get { return m_TypeRes; } }
        public BaseTool Tool { get { return m_Tool; } }
        public int Quality { get { return m_Quality; } }

        public CustomCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, BaseTool tool, int quality)
        {
            m_From = from;
            m_CraftItem = craftItem;
            m_CraftSystem = craftSystem;
            m_TypeRes = typeRes;
            m_Tool = tool;
            m_Quality = quality;
        }

        public abstract void EndCraftAction();
        public abstract Item CompleteCraft(out int message);
    }

    public enum ConsumeType
    {
        All, Half, None
    }

    public interface ICraftable
    {
        int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue);
    }

    public class CraftSkill
    {
        private SkillName m_SkillToMake;
        private double m_MinSkill;
        private double m_MaxSkill;

        public CraftSkill(SkillName skillToMake, double minSkill, double maxSkill)
        {
            m_SkillToMake = skillToMake;
            m_MinSkill = minSkill;
            m_MaxSkill = maxSkill;
        }

        public SkillName SkillToMake
        {
            get { return m_SkillToMake; }
        }

        public double MinSkill
        {
            get { return m_MinSkill; }
        }

        public double MaxSkill
        {
            get { return m_MaxSkill; }
        }
    }

    public class CraftSkillCol : System.Collections.CollectionBase
    {
        public CraftSkillCol()
        {
        }

        public void Add(CraftSkill craftSkill)
        {
            List.Add(craftSkill);
        }

        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
            }
            else
            {
                List.RemoveAt(index);
            }
        }

        public CraftSkill GetAt(int index)
        {
            return (CraftSkill)List[index];
        }
    }

    public class CraftGroup
    {
        private CraftItemCol m_arCraftItem;

        private string m_NameString;
        private int m_NameNumber;

        public CraftGroup(TextDefinition groupName)
        {
            m_NameNumber = groupName;
            m_NameString = groupName;
            m_arCraftItem = new CraftItemCol();
        }

        public void AddCraftItem(CraftItem craftItem)
        {
            m_arCraftItem.Add(craftItem);
        }

        public CraftItemCol CraftItems
        {
            get { return m_arCraftItem; }
        }

        public string NameString
        {
            get { return m_NameString; }
        }

        public int NameNumber
        {
            get { return m_NameNumber; }
        }
    }

    public class CraftGroupCol : System.Collections.CollectionBase
    {
        public CraftGroupCol()
        {
        }

        public int Add(CraftGroup craftGroup)
        {
            return List.Add(craftGroup);
        }

        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
            }
            else
            {
                List.RemoveAt(index);
            }
        }

        public CraftGroup GetAt(int index)
        {
            return (CraftGroup)List[index];
        }

        public int SearchFor(TextDefinition groupName)
        {
            for (int i = 0; i < List.Count; i++)
            {
                CraftGroup craftGroup = (CraftGroup)List[i];

                int nameNumber = craftGroup.NameNumber;
                string nameString = craftGroup.NameString;

                if ((nameNumber != 0 && nameNumber == groupName.Number) || (nameString != null && nameString == groupName.String))
                    return i;
            }

            return -1;
        }
    }

    public class Recipe
    {
        public static void Initialize()
        {
            CommandSystem.Register("LearnAllRecipes", AccessLevel.GameMaster, new CommandEventHandler(LearnAllRecipes_OnCommand));
            CommandSystem.Register("ForgetAllRecipes", AccessLevel.GameMaster, new CommandEventHandler(ForgetAllRecipes_OnCommand));
        }

        [Usage("LearnAllRecipes")]
        [Description("Teaches a player all available recipes.")]
        private static void LearnAllRecipes_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            m.SendMessage("Target a player to teach them all of the recipies.");

            m.BeginTarget(-1, false, Server.Targeting.TargetFlags.None, new TargetCallback(
                delegate(Mobile from, object targeted)
                {
                    if (targeted is PlayerMobile)
                    {
                        foreach (KeyValuePair<int, Recipe> kvp in m_Recipes)
                            ((PlayerMobile)targeted).AcquireRecipe(kvp.Key);

                        m.SendMessage("You teach them all of the recipies.");
                    }
                    else
                    {
                        m.SendMessage("That is not a player!");
                    }
                }
            ));
        }

        [Usage("ForgetAllRecipes")]
        [Description("Makes a player forget all the recipies they've learned.")]
        private static void ForgetAllRecipes_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            m.SendMessage("Target a player to have them forget all of the recipies they've learned.");

            m.BeginTarget(-1, false, Server.Targeting.TargetFlags.None, new TargetCallback(
                delegate(Mobile from, object targeted)
                {
                    if (targeted is PlayerMobile)
                    {
                        ((PlayerMobile)targeted).ResetRecipes();

                        m.SendMessage("They forget all their recipies.");
                    }
                    else
                    {
                        m.SendMessage("That is not a player!");
                    }
                }
            ));
        }


        private static Dictionary<int, Recipe> m_Recipes = new Dictionary<int, Recipe>();

        public static Dictionary<int, Recipe> Recipes { get { return m_Recipes; } }

        private static int m_LargestRecipeID;
        public static int LargestRecipeID { get { return m_LargestRecipeID; } }

        private CraftSystem m_System;

        public CraftSystem CraftSystem
        {
            get { return m_System; }
            set { m_System = value; }
        }

        private CraftItem m_CraftItem;

        public CraftItem CraftItem
        {
            get { return m_CraftItem; }
            set { m_CraftItem = value; }
        }

        private int m_ID;

        public int ID
        {
            get { return m_ID; }
        }

        private TextDefinition m_TD;
        public TextDefinition TextDefinition
        {
            get
            {
                if (m_TD == null)
                    m_TD = new TextDefinition(m_CraftItem.NameNumber, m_CraftItem.NameString);

                return m_TD;
            }
        }

        public Recipe(int id, CraftSystem system, CraftItem item)
        {
            m_ID = id;
            m_System = system;
            m_CraftItem = item;

            if (m_Recipes.ContainsKey(id))
                throw new Exception("Attempting to create recipe with preexisting ID.");

            m_Recipes.Add(id, this);
            m_LargestRecipeID = Math.Max(id, m_LargestRecipeID);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CraftItemIDAttribute : Attribute
    {
        private int m_ItemID;

        public int ItemID { get { return m_ItemID; } }

        public CraftItemIDAttribute(int itemID)
        {
            m_ItemID = itemID;
        }
    }

    public class CraftItem
    {
        private CraftResCol m_arCraftRes;
        private CraftSkillCol m_arCraftSkill;
        private Type m_Type;

        private string m_GroupNameString;
        private int m_GroupNameNumber;

        private string m_NameString;
        private int m_NameNumber;

        private int m_ItemHue;

        private int m_Mana;
        private int m_Hits;
        private int m_Stam;

        private BeverageType m_RequiredBeverage;

        private bool m_UseAllRes;

        private bool m_NeedHeat;
        private bool m_NeedOven;
        private bool m_NeedMill;

        private bool m_UseSubRes2;

        private bool m_ForceNonExceptional;

        public bool ForceNonExceptional
        {
            get { return m_ForceNonExceptional; }
            set { m_ForceNonExceptional = value; }
        }

        private Expansion m_RequiredExpansion;

        public Expansion RequiredExpansion
        {
            get { return m_RequiredExpansion; }
            set { m_RequiredExpansion = value; }
        }

        private Recipe m_Recipe;

        public Recipe Recipe
        {
            get { return m_Recipe; }
        }

        public void AddRecipe(int id, CraftSystem system)
        {
            if (m_Recipe != null)
            {
                Console.WriteLine("Warning: Attempted add of recipe #{0} to the crafting of {1} in CraftSystem {2}.", id, this.m_Type.Name, system);
                return;
            }

            m_Recipe = new Recipe(id, system, this);
        }

        public static int LabelNumber(Type type)
        {
            int number = ItemIDOf(type);

            if (number >= 0x4000)
                number += 1078872;
            else
                number += 1020000;

            return number;
        }

        private static Dictionary<Type, int> _itemIds = new Dictionary<Type, int>();

        public static int ItemIDOf(Type type)
        {
            int itemId;

            if (!_itemIds.TryGetValue(type, out itemId))
            {
                if (type == typeof(FactionExplosionTrap))
                {
                    itemId = 14034;
                }
                else if (type == typeof(FactionGasTrap))
                {
                    itemId = 4523;
                }
                else if (type == typeof(FactionSawTrap))
                {
                    itemId = 4359;
                }
                else if (type == typeof(FactionSpikeTrap))
                {
                    itemId = 4517;
                }

                if (itemId == 0)
                {
                    object[] attrs = type.GetCustomAttributes(typeof(CraftItemIDAttribute), false);

                    if (attrs.Length > 0)
                    {
                        CraftItemIDAttribute craftItemID = (CraftItemIDAttribute)attrs[0];
                        itemId = craftItemID.ItemID;
                    }
                }

                if (itemId == 0)
                {
                    Item item = null;

                    try { item = Activator.CreateInstance(type) as Item; }
                    catch { }

                    if (item != null)
                    {
                        itemId = item.ItemID;
                        item.Delete();
                    }
                }

                _itemIds[type] = itemId;
            }

            return itemId;
        }

        public CraftItem(Type type, TextDefinition groupName, TextDefinition name)
        {
            m_arCraftRes = new CraftResCol();
            m_arCraftSkill = new CraftSkillCol();

            m_Type = type;

            m_GroupNameString = groupName;
            m_NameString = name;

            m_GroupNameNumber = groupName;
            m_NameNumber = name;

            m_RequiredBeverage = BeverageType.Water;
        }

        public BeverageType RequiredBeverage
        {
            get { return m_RequiredBeverage; }
            set { m_RequiredBeverage = value; }
        }

        public void AddRes(Type type, TextDefinition name, int amount)
        {
            AddRes(type, name, amount, "");
        }

        public void AddRes(Type type, TextDefinition name, int amount, TextDefinition message)
        {
            CraftRes craftRes = new CraftRes(type, name, amount, message);
            m_arCraftRes.Add(craftRes);
        }


        public void AddSkill(SkillName skillToMake, double minSkill, double maxSkill)
        {
            CraftSkill craftSkill = new CraftSkill(skillToMake, minSkill, maxSkill);
            m_arCraftSkill.Add(craftSkill);
        }

        public int Mana
        {
            get { return m_Mana; }
            set { m_Mana = value; }
        }

        public int Hits
        {
            get { return m_Hits; }
            set { m_Hits = value; }
        }

        public int Stam
        {
            get { return m_Stam; }
            set { m_Stam = value; }
        }

        public bool UseSubRes2
        {
            get { return m_UseSubRes2; }
            set { m_UseSubRes2 = value; }
        }

        public bool UseAllRes
        {
            get { return m_UseAllRes; }
            set { m_UseAllRes = value; }
        }

        public bool NeedHeat
        {
            get { return m_NeedHeat; }
            set { m_NeedHeat = value; }
        }

        public bool NeedOven
        {
            get { return m_NeedOven; }
            set { m_NeedOven = value; }
        }

        public bool NeedMill
        {
            get { return m_NeedMill; }
            set { m_NeedMill = value; }
        }

        public Type ItemType
        {
            get { return m_Type; }
        }

        public int ItemHue
        {
            get { return m_ItemHue; }
            set { m_ItemHue = value; }
        }

        public string GroupNameString
        {
            get { return m_GroupNameString; }
        }

        public int GroupNameNumber
        {
            get { return m_GroupNameNumber; }
        }

        public string NameString
        {
            get { return m_NameString; }
        }

        public int NameNumber
        {
            get { return m_NameNumber; }
        }

        public CraftResCol Resources
        {
            get { return m_arCraftRes; }
        }

        public CraftSkillCol Skills
        {
            get { return m_arCraftSkill; }
        }

        public bool ConsumeAttributes(Mobile from, ref object message, bool consume)
        {
            bool consumMana = false;
            bool consumHits = false;
            bool consumStam = false;

            if (Hits > 0 && from.Hits < Hits)
            {
                message = "You lack the required hit points to make that.";
                return false;
            }
            else
            {
                consumHits = consume;
            }

            if (Mana > 0 && from.Mana < Mana)
            {
                message = "You lack the required mana to make that.";
                return false;
            }
            else
            {
                consumMana = consume;
            }

            if (Stam > 0 && from.Stam < Stam)
            {
                message = "You lack the required stamina to make that.";
                return false;
            }
            else
            {
                consumStam = consume;
            }

            if (consumMana)
                from.Mana -= Mana;

            if (consumHits)
                from.Hits -= Hits;

            if (consumStam)
                from.Stam -= Stam;

            return true;
        }

        #region Tables
        private static int[] m_HeatSources = new int[]
            {
                0x461, 0x48E, // Sandstone oven/fireplace
				0x92B, 0x96C, // Stone oven/fireplace
				0xDE3, 0xDE9, // Campfire
				0xFAC, 0xFAC, // Firepit
				0x184A, 0x184C, // Heating stand (left)
				0x184E, 0x1850, // Heating stand (right)
				0x398C, 0x399F,  // Fire field
				0x2DDB, 0x2DDC,	//Elven stove
				0x19AA, 0x19BB,	// Veteran Reward Brazier
				0x197A, 0x19A9, // Large Forge 
				0x0FB1, 0x0FB1, // Small Forge
				0x2DD8, 0x2DD8 // Elven Forge
			};

        private static int[] m_Ovens = new int[]
            {
                0x461, 0x46F, // Sandstone oven
				0x92B, 0x93F,  // Stone oven
				0x2DDB, 0x2DDC	//Elven stove
			};

        private static int[] m_Mills = new int[]
            {
                0x1920, 0x1921, 0x1922, 0x1923, 0x1924, 0x1295, 0x1926, 0x1928,
                0x192C, 0x192D, 0x192E, 0x129F, 0x1930, 0x1931, 0x1932, 0x1934
            };

        private static Type[][] m_TypesTable = new Type[][]
            {
                new Type[]{ typeof( Log ), typeof( Board ) },
                new Type[]{ typeof( HeartwoodLog ), typeof( HeartwoodBoard ) },
                new Type[]{ typeof( BloodwoodLog ), typeof( BloodwoodBoard ) },
                new Type[]{ typeof( FrostwoodLog ), typeof( FrostwoodBoard ) },
                new Type[]{ typeof( OakLog ), typeof( OakBoard ) },
                new Type[]{ typeof( AshLog ), typeof( AshBoard ) },
                new Type[]{ typeof( YewLog ), typeof( YewBoard ) },
                new Type[]{ typeof( Leather ), typeof( Hides ) },
                new Type[]{ typeof( SpinedLeather ), typeof( SpinedHides ) },
                new Type[]{ typeof( HornedLeather ), typeof( HornedHides ) },
                new Type[]{ typeof( BarbedLeather ), typeof( BarbedHides ) },
                new Type[]{ typeof( BlankMap ), typeof( BlankScroll ) },
                new Type[]{ typeof( Cloth ), typeof( UncutCloth ) },
                new Type[]{ typeof( CheeseWheel ), typeof( CheeseWedge ) },
                new Type[]{ typeof( Pumpkin ), typeof( SmallPumpkin ) },
                new Type[]{ typeof( WoodenBowlOfPeas ), typeof( PewterBowlOfPeas ) }
            };

        private static Type[] m_ColoredItemTable = new Type[]
            {
                typeof( BaseWeapon ), typeof( BaseArmor ), typeof( BaseClothing ),
                typeof( BaseJewel ), typeof( DragonBardingDeed )
            };

        private static Type[] m_ColoredResourceTable = new Type[]
            {
                typeof( BaseIngot ), typeof( BaseOre ),
                typeof( BaseLeather ), typeof( BaseHides ),
                typeof( UncutCloth ), typeof( Cloth ),
                typeof( BaseGranite ), typeof( BaseScales )
            };

        private static Type[] m_MarkableTable = new Type[]
                {
                    typeof( BaseArmor ),
                    typeof( BaseWeapon ),
                    typeof( BaseClothing ),
                    typeof( BaseInstrument ),
                    typeof( DragonBardingDeed ),
                    typeof( BaseTool ),
                    typeof( BaseHarvestTool ),
                    typeof( FukiyaDarts ), typeof( Shuriken ),
                    typeof( Spellbook ), typeof( Runebook ),
                    typeof( BaseQuiver )
                };

        private static Type[] m_NeverColorTable = new Type[]
                {
                    typeof( OrcHelm )
                };
        #endregion

        public bool IsMarkable(Type type)
        {
            if (m_ForceNonExceptional)  //Don't even display the stuff for marking if it can't ever be exceptional.
                return false;

            for (int i = 0; i < m_MarkableTable.Length; ++i)
            {
                if (type == m_MarkableTable[i] || type.IsSubclassOf(m_MarkableTable[i]))
                    return true;
            }

            return false;
        }

        public static bool RetainsColor(Type type)
        {
            bool neverColor = false;

            for (int i = 0; !neverColor && i < m_NeverColorTable.Length; ++i)
                neverColor = (type == m_NeverColorTable[i] || type.IsSubclassOf(m_NeverColorTable[i]));

            if (neverColor)
                return false;

            bool inItemTable = false;

            for (int i = 0; !inItemTable && i < m_ColoredItemTable.Length; ++i)
                inItemTable = (type == m_ColoredItemTable[i] || type.IsSubclassOf(m_ColoredItemTable[i]));

            return inItemTable;
        }

        public bool RetainsColorFrom(CraftSystem system, Type type)
        {
            if (system.RetainsColorFrom(this, type))
                return true;

            bool inItemTable = RetainsColor(m_Type);

            if (!inItemTable)
                return false;

            bool inResourceTable = false;

            for (int i = 0; !inResourceTable && i < m_ColoredResourceTable.Length; ++i)
                inResourceTable = (type == m_ColoredResourceTable[i] || type.IsSubclassOf(m_ColoredResourceTable[i]));

            return inResourceTable;
        }

        public bool Find(Mobile from, int[] itemIDs)
        {
            Map map = from.Map;

            if (map == null)
                return false;

            IPooledEnumerable eable = map.GetItemsInRange(from.Location, 2);

            foreach (Item item in eable)
            {
                if ((item.Z + 16) > from.Z && (from.Z + 16) > item.Z && Find(item.ItemID, itemIDs))
                {
                    eable.Free();
                    return true;
                }
            }

            eable.Free();

            for (int x = -2; x <= 2; ++x)
            {
                for (int y = -2; y <= 2; ++y)
                {
                    int vx = from.X + x;
                    int vy = from.Y + y;

                    StaticTile[] tiles = map.Tiles.GetStaticTiles(vx, vy, true);

                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        int z = tiles[i].Z;
                        int id = tiles[i].ID;

                        if ((z + 16) > from.Z && (from.Z + 16) > z && Find(id, itemIDs))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool Find(int itemID, int[] itemIDs)
        {
            bool contains = false;

            for (int i = 0; !contains && i < itemIDs.Length; i += 2)
                contains = (itemID >= itemIDs[i] && itemID <= itemIDs[i + 1]);

            return contains;
        }

        public bool IsQuantityType(Type[][] types)
        {
            for (int i = 0; i < types.Length; ++i)
            {
                Type[] check = types[i];

                for (int j = 0; j < check.Length; ++j)
                {
                    if (typeof(IHasQuantity).IsAssignableFrom(check[j]))
                        return true;
                }
            }

            return false;
        }

        public int ConsumeQuantity(Container cont, Type[][] types, int[] amounts)
        {
            if (types.Length != amounts.Length)
                throw new ArgumentException();

            Item[][] items = new Item[types.Length][];
            int[] totals = new int[types.Length];

            for (int i = 0; i < types.Length; ++i)
            {
                items[i] = cont.FindItemsByType(types[i], true);

                for (int j = 0; j < items[i].Length; ++j)
                {
                    IHasQuantity hq = items[i][j] as IHasQuantity;

                    if (hq == null)
                    {
                        totals[i] += items[i][j].Amount;
                    }
                    else
                    {
                        if (hq is BaseBeverage && ((BaseBeverage)hq).Content != m_RequiredBeverage)
                            continue;

                        totals[i] += hq.Quantity;
                    }
                }

                if (totals[i] < amounts[i])
                    return i;
            }

            for (int i = 0; i < types.Length; ++i)
            {
                int need = amounts[i];

                for (int j = 0; j < items[i].Length; ++j)
                {
                    Item item = items[i][j];
                    IHasQuantity hq = item as IHasQuantity;

                    if (hq == null)
                    {
                        int theirAmount = item.Amount;

                        if (theirAmount < need)
                        {
                            item.Delete();
                            need -= theirAmount;
                        }
                        else
                        {
                            item.Consume(need);
                            break;
                        }
                    }
                    else
                    {
                        if (hq is BaseBeverage && ((BaseBeverage)hq).Content != m_RequiredBeverage)
                            continue;

                        int theirAmount = hq.Quantity;

                        if (theirAmount < need)
                        {
                            hq.Quantity -= theirAmount;
                            need -= theirAmount;
                        }
                        else
                        {
                            hq.Quantity -= need;
                            break;
                        }
                    }
                }
            }

            return -1;
        }

        public int GetQuantity(Container cont, Type[] types)
        {
            Item[] items = cont.FindItemsByType(types, true);

            int amount = 0;

            for (int i = 0; i < items.Length; ++i)
            {
                IHasQuantity hq = items[i] as IHasQuantity;

                if (hq == null)
                {
                    amount += items[i].Amount;
                }
                else
                {
                    if (hq is BaseBeverage && ((BaseBeverage)hq).Content != m_RequiredBeverage)
                        continue;

                    amount += hq.Quantity;
                }
            }

            return amount;
        }

        public bool ConsumeRes(Mobile from, Type typeRes, CraftSystem craftSystem, ref int resHue, ref int maxAmount, ConsumeType consumeType, ref object message)
        {
            return ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, false);
        }

        public bool ConsumeRes(Mobile from, Type typeRes, CraftSystem craftSystem, ref int resHue, ref int maxAmount, ConsumeType consumeType, ref object message, bool isFailure)
        {
            Container ourPack = from.Backpack;

            if (ourPack == null)
                return false;

            if (m_NeedHeat && !Find(from, m_HeatSources))
            {
                message = 1044487; // You must be near a fire source to cook.
                return false;
            }

            if (m_NeedOven && !Find(from, m_Ovens))
            {
                message = 1044493; // You must be near an oven to bake that.
                return false;
            }

            if (m_NeedMill && !Find(from, m_Mills))
            {
                message = 1044491; // You must be near a flour mill to do that.
                return false;
            }

            Type[][] types = new Type[m_arCraftRes.Count][];
            int[] amounts = new int[m_arCraftRes.Count];

            maxAmount = int.MaxValue;

            CraftSubResCol resCol = (m_UseSubRes2 ? craftSystem.CraftSubRes2 : craftSystem.CraftSubRes);

            for (int i = 0; i < types.Length; ++i)
            {
                CraftRes craftRes = m_arCraftRes.GetAt(i);
                Type baseType = craftRes.ItemType;

                // Resource Mutation
                if ((baseType == resCol.ResType) && (typeRes != null))
                {
                    baseType = typeRes;

                    CraftSubRes subResource = resCol.SearchFor(baseType);

                    if (subResource != null && from.Skills[craftSystem.MainSkill].Base < subResource.RequiredSkill)
                    {
                        message = subResource.Message;
                        return false;
                    }
                }
                // ******************

                for (int j = 0; types[i] == null && j < m_TypesTable.Length; ++j)
                {
                    if (m_TypesTable[j][0] == baseType)
                        types[i] = m_TypesTable[j];
                }

                if (types[i] == null)
                    types[i] = new Type[] { baseType };

                amounts[i] = craftRes.Amount;

                // For stackable items that can ben crafted more than one at a time
                if (UseAllRes)
                {
                    int tempAmount = ourPack.GetAmount(types[i]);
                    tempAmount /= amounts[i];
                    if (tempAmount < maxAmount)
                    {
                        maxAmount = tempAmount;

                        if (maxAmount == 0)
                        {
                            CraftRes res = m_arCraftRes.GetAt(i);

                            if (res.MessageNumber > 0)
                                message = res.MessageNumber;
                            else if (!String.IsNullOrEmpty(res.MessageString))
                                message = res.MessageString;
                            else
                                message = 502925; // You don't have the resources required to make that item.

                            return false;
                        }
                    }
                }
                // ****************************

                if (isFailure && !craftSystem.ConsumeOnFailure(from, types[i][0], this))
                    amounts[i] = 0;
            }

            // We adjust the amount of each resource to consume the max posible
            if (UseAllRes)
            {
                for (int i = 0; i < amounts.Length; ++i)
                    amounts[i] *= maxAmount;
            }
            else
                maxAmount = -1;

            Item consumeExtra = null;

            if (m_NameNumber == 1041267)
            {
                // Runebooks are a special case, they need a blank recall rune

                List<RecallRune> runes = ourPack.FindItemsByType<RecallRune>();

                for (int i = 0; i < runes.Count; ++i)
                {
                    RecallRune rune = runes[i];

                    if (rune != null && !rune.Marked)
                    {
                        consumeExtra = rune;
                        break;
                    }
                }

                if (consumeExtra == null)
                {
                    message = 1044253; // You don't have the components needed to make that.
                    return false;
                }
            }

            int index = 0;

            // Consume ALL
            if (consumeType == ConsumeType.All)
            {
                m_ResHue = 0; m_ResAmount = 0; m_System = craftSystem;

                if (IsQuantityType(types))
                    index = ConsumeQuantity(ourPack, types, amounts);
                else
                    index = ourPack.ConsumeTotalGrouped(types, amounts, true, new OnItemConsumed(OnResourceConsumed), new CheckItemGroup(CheckHueGrouping));

                resHue = m_ResHue;
            }

            // Consume Half ( for use all resource craft type )
            else if (consumeType == ConsumeType.Half)
            {
                for (int i = 0; i < amounts.Length; i++)
                {
                    amounts[i] /= 2;

                    if (amounts[i] < 1)
                        amounts[i] = 1;
                }

                m_ResHue = 0; m_ResAmount = 0; m_System = craftSystem;

                if (IsQuantityType(types))
                    index = ConsumeQuantity(ourPack, types, amounts);
                else
                    index = ourPack.ConsumeTotalGrouped(types, amounts, true, new OnItemConsumed(OnResourceConsumed), new CheckItemGroup(CheckHueGrouping));

                resHue = m_ResHue;
            }

            else // ConstumeType.None ( it's basicaly used to know if the crafter has enough resource before starting the process )
            {
                index = -1;

                if (IsQuantityType(types))
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (GetQuantity(ourPack, types[i]) < amounts[i])
                        {
                            index = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (ourPack.GetBestGroupAmount(types[i], true, new CheckItemGroup(CheckHueGrouping)) < amounts[i])
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }

            if (index == -1)
            {
                if (consumeType != ConsumeType.None)
                    if (consumeExtra != null)
                        consumeExtra.Delete();

                return true;
            }
            else
            {
                CraftRes res = m_arCraftRes.GetAt(index);

                if (res.MessageNumber > 0)
                    message = res.MessageNumber;
                else if (res.MessageString != null && res.MessageString != String.Empty)
                    message = res.MessageString;
                else
                    message = 502925; // You don't have the resources required to make that item.

                return false;
            }
        }

        private int m_ResHue;
        private int m_ResAmount;
        private CraftSystem m_System;

        private void OnResourceConsumed(Item item, int amount)
        {
            if (!RetainsColorFrom(m_System, item.GetType()))
                return;

            if (amount >= m_ResAmount)
            {
                m_ResHue = item.Hue;
                m_ResAmount = amount;
            }
        }

        private int CheckHueGrouping(Item a, Item b)
        {
            return b.Hue.CompareTo(a.Hue);
        }

        public double GetExceptionalChance(CraftSystem system, double chance, Mobile from)
        {
            if (m_ForceNonExceptional)
                return 0.0;

            double bonus = 0.0;

            if (from.Talisman is BaseTalisman)
            {
                BaseTalisman talisman = (BaseTalisman)from.Talisman;

                if (talisman.Skill == system.MainSkill)
                {
                    chance -= talisman.SuccessBonus / 100.0;
                    bonus = talisman.ExceptionalBonus / 100.0;
                }
            }

            switch (system.ECA)
            {
                default:
                case CraftECA.ChanceMinusSixty: chance -= 0.6; break;
                case CraftECA.FiftyPercentChanceMinusTenPercent: chance = chance * 0.5 - 0.1; break;
                case CraftECA.ChanceMinusSixtyToFourtyFive:
                    {
                        double offset = 0.60 - ((from.Skills[system.MainSkill].Value - 95.0) * 0.03);

                        if (offset < 0.45)
                            offset = 0.45;
                        else if (offset > 0.60)
                            offset = 0.60;

                        chance -= offset;
                        break;
                    }
            }

            if (chance > 0)
                return chance + bonus;

            return chance;
        }

        public bool CheckSkills(Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills)
        {
            return CheckSkills(from, typeRes, craftSystem, ref quality, ref allRequiredSkills, true);
        }

        public bool CheckSkills(Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills, bool gainSkills)
        {
            double chance = GetSuccessChance(from, typeRes, craftSystem, gainSkills, ref allRequiredSkills);

            if (GetExceptionalChance(craftSystem, chance, from) > Utility.RandomDouble())
                quality = 2;

            return (chance > Utility.RandomDouble());
        }

        public double GetSuccessChance(Mobile from, Type typeRes, CraftSystem craftSystem, bool gainSkills, ref bool allRequiredSkills)
        {
            double minMainSkill = 0.0;
            double maxMainSkill = 0.0;
            double valMainSkill = 0.0;

            allRequiredSkills = true;

            for (int i = 0; i < m_arCraftSkill.Count; i++)
            {
                CraftSkill craftSkill = m_arCraftSkill.GetAt(i);

                double minSkill = craftSkill.MinSkill;
                double maxSkill = craftSkill.MaxSkill;
                double valSkill = from.Skills[craftSkill.SkillToMake].Value;

                if (valSkill < minSkill)
                    allRequiredSkills = false;

                if (craftSkill.SkillToMake == craftSystem.MainSkill)
                {
                    minMainSkill = minSkill;
                    maxMainSkill = maxSkill;
                    valMainSkill = valSkill;
                }

                if (gainSkills) // This is a passive check. Success chance is entirely dependant on the main skill
                    from.CheckSkill(craftSkill.SkillToMake, minSkill, maxSkill);
            }

            double chance;

            if (allRequiredSkills)
                chance = craftSystem.GetChanceAtMin(this) + ((valMainSkill - minMainSkill) / (maxMainSkill - minMainSkill) * (1.0 - craftSystem.GetChanceAtMin(this)));
            else
                chance = 0.0;

            if (allRequiredSkills && from.Talisman is BaseTalisman)
            {
                BaseTalisman talisman = (BaseTalisman)from.Talisman;

                if (talisman.Skill == craftSystem.MainSkill)
                    chance += talisman.SuccessBonus / 100.0;
            }

            if (allRequiredSkills && valMainSkill == maxMainSkill)
                chance = 1.0;

            return chance;
        }

        public void Craft(Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool)
        {
            if (from.BeginAction(typeof(CraftSystem)))
            {
                if (RequiredExpansion == Expansion.None || (from.NetState != null && from.NetState.SupportsExpansion(RequiredExpansion)))
                {
                    bool allRequiredSkills = true;
                    double chance = GetSuccessChance(from, typeRes, craftSystem, false, ref allRequiredSkills);

                    if (allRequiredSkills && chance >= 0.0)
                    {
                        if (this.Recipe == null || !(from is PlayerMobile) || ((PlayerMobile)from).HasRecipe(this.Recipe))
                        {
                            int badCraft = craftSystem.CanCraft(from, tool, m_Type);

                            if (badCraft <= 0)
                            {
                                int resHue = 0;
                                int maxAmount = 0;
                                object message = null;

                                if (ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref message))
                                {
                                    message = null;

                                    if (ConsumeAttributes(from, ref message, false))
                                    {
                                        CraftContext context = craftSystem.GetContext(from);

                                        if (context != null)
                                            context.OnMade(this);

                                        int iMin = craftSystem.MinCraftEffect;
                                        int iMax = (craftSystem.MaxCraftEffect - iMin) + 1;
                                        int iRandom = Utility.Random(iMax);
                                        iRandom += iMin + 1;
                                        new InternalTimer(from, craftSystem, this, typeRes, tool, iRandom).Start();
                                    }
                                    else
                                    {
                                        from.EndAction(typeof(CraftSystem));
                                        from.SendGump(new CraftGump(from, craftSystem, tool, message));
                                    }
                                }
                                else
                                {
                                    from.EndAction(typeof(CraftSystem));
                                    from.SendGump(new CraftGump(from, craftSystem, tool, message));
                                }
                            }
                            else
                            {
                                from.EndAction(typeof(CraftSystem));
                                from.SendGump(new CraftGump(from, craftSystem, tool, badCraft));
                            }
                        }
                        else
                        {
                            from.EndAction(typeof(CraftSystem));
                            from.SendGump(new CraftGump(from, craftSystem, tool, 1072847)); // You must learn that recipe from a scroll.
                        }
                    }
                    else
                    {
                        from.EndAction(typeof(CraftSystem));
                        from.SendGump(new CraftGump(from, craftSystem, tool, 1044153)); // You don't have the required skills to attempt this item.
                    }
                }
                else
                {
                    from.EndAction(typeof(CraftSystem));
                    from.SendGump(new CraftGump(from, craftSystem, tool, RequiredExpansionMessage(RequiredExpansion))); //The {0} expansion is required to attempt this item.
                }
            }
            else
            {
                from.SendLocalizedMessage(500119); // You must wait to perform another action
            }
        }

        private object RequiredExpansionMessage(Expansion expansion)    //Eventually convert to TextDefinition, but that requires that we convert all the gumps to ues it too.  Not that it wouldn't be a bad idea.
        {
            switch (expansion)
            {
                case Expansion.SE:
                    return 1063307; // The "Samurai Empire" expansion is required to attempt this item.
                case Expansion.ML:
                    return 1072650; // The "Mondain's Legacy" expansion is required to attempt this item.
                default:
                    return String.Format("The \"{0}\" expansion is required to attempt this item.", ExpansionInfo.GetInfo(expansion).Name);
            }
        }

        public void CompleteCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CustomCraft customCraft)
        {
            int badCraft = craftSystem.CanCraft(from, tool, m_Type);

            if (badCraft > 0)
            {
                if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                    from.SendGump(new CraftGump(from, craftSystem, tool, badCraft));
                else
                    from.SendLocalizedMessage(badCraft);

                return;
            }

            int checkResHue = 0, checkMaxAmount = 0;
            object checkMessage = null;

            // Not enough resource to craft it
            if (!ConsumeRes(from, typeRes, craftSystem, ref checkResHue, ref checkMaxAmount, ConsumeType.None, ref checkMessage))
            {
                if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                    from.SendGump(new CraftGump(from, craftSystem, tool, checkMessage));
                else if (checkMessage is int && (int)checkMessage > 0)
                    from.SendLocalizedMessage((int)checkMessage);
                else if (checkMessage is string)
                    from.SendMessage((string)checkMessage);

                return;
            }
            else if (!ConsumeAttributes(from, ref checkMessage, false))
            {
                if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                    from.SendGump(new CraftGump(from, craftSystem, tool, checkMessage));
                else if (checkMessage is int && (int)checkMessage > 0)
                    from.SendLocalizedMessage((int)checkMessage);
                else if (checkMessage is string)
                    from.SendMessage((string)checkMessage);

                return;
            }

            bool toolBroken = false;

            int ignored = 1;
            int endquality = 1;

            bool allRequiredSkills = true;

            if (CheckSkills(from, typeRes, craftSystem, ref ignored, ref allRequiredSkills))
            {
                // Resource
                int resHue = 0;
                int maxAmount = 0;

                object message = null;

                // Not enough resource to craft it
                if (!ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref message))
                {
                    if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                        from.SendGump(new CraftGump(from, craftSystem, tool, message));
                    else if (message is int && (int)message > 0)
                        from.SendLocalizedMessage((int)message);
                    else if (message is string)
                        from.SendMessage((string)message);

                    return;
                }
                else if (!ConsumeAttributes(from, ref message, true))
                {
                    if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                        from.SendGump(new CraftGump(from, craftSystem, tool, message));
                    else if (message is int && (int)message > 0)
                        from.SendLocalizedMessage((int)message);
                    else if (message is string)
                        from.SendMessage((string)message);

                    return;
                }

                tool.UsesRemaining--;

                if (craftSystem is DefBlacksmithy)
                {
                    AncientSmithyHammer hammer = from.FindItemOnLayer(Layer.OneHanded) as AncientSmithyHammer;
                    if (hammer != null && hammer != tool)
                    {
                        hammer.UsesRemaining--;
                        if (hammer.UsesRemaining < 1)
                            hammer.Delete();
                    }
                }

                if (tool.UsesRemaining < 1 && tool.BreakOnDepletion)
                    toolBroken = true;

                if (toolBroken)
                    tool.Delete();

                int num = 0;

                Item item;
                if (customCraft != null)
                {
                    item = customCraft.CompleteCraft(out num);
                }
                else if (typeof(MapItem).IsAssignableFrom(ItemType) && from.Map != Map.Trammel && from.Map != Map.Felucca)
                {
                    item = new IndecipherableMap();
                    from.SendLocalizedMessage(1070800); // The map you create becomes mysteriously indecipherable.
                }
                else
                {
                    item = Activator.CreateInstance(ItemType) as Item;
                }

                if (item != null)
                {
                    if (item is ICraftable)
                        endquality = ((ICraftable)item).OnCraft(quality, makersMark, from, craftSystem, typeRes, tool, this, resHue);
                    else if (item.Hue == 0)
                        item.Hue = resHue;

                    if (maxAmount > 0)
                    {
                        if (!item.Stackable && item is IUsesRemaining)
                            ((IUsesRemaining)item).UsesRemaining *= maxAmount;
                        else
                            item.Amount = maxAmount;
                    }

                    from.AddToBackpack(item);

                    if (from.AccessLevel > AccessLevel.Player)
                        CommandLogging.WriteLine(from, "Crafting {0} with craft system {1}", CommandLogging.Format(item), craftSystem.GetType().Name);

                    //from.PlaySound( 0x57 );
                }

                if (num == 0)
                    num = craftSystem.PlayEndingEffect(from, false, true, toolBroken, endquality, makersMark, this);

                bool queryFactionImbue = false;
                int availableSilver = 0;
                FactionItemDefinition def = null;
                Faction faction = null;

                if (item is IFactionItem)
                {
                    def = FactionItemDefinition.Identify(item);

                    if (def != null)
                    {
                        faction = Faction.Find(from);

                        if (faction != null)
                        {
                            Town town = Town.FromRegion(from.Region);

                            if (town != null && town.Owner == faction)
                            {
                                Container pack = from.Backpack;

                                if (pack != null)
                                {
                                    availableSilver = pack.GetAmount(typeof(Silver));

                                    if (availableSilver >= def.SilverCost)
                                        queryFactionImbue = Faction.IsNearType(from, def.VendorType, 12);
                                }
                            }
                        }
                    }
                }

                // TODO: Scroll imbuing

                if (queryFactionImbue)
                    from.SendGump(new FactionImbueGump(quality, item, from, craftSystem, tool, num, availableSilver, faction, def));
                else if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                    from.SendGump(new CraftGump(from, craftSystem, tool, num));
                else if (num > 0)
                    from.SendLocalizedMessage(num);
            }
            else if (!allRequiredSkills)
            {
                if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                    from.SendGump(new CraftGump(from, craftSystem, tool, 1044153));
                else
                    from.SendLocalizedMessage(1044153); // You don't have the required skills to attempt this item.
            }
            else
            {
                ConsumeType consumeType = (UseAllRes ? ConsumeType.Half : ConsumeType.All);
                int resHue = 0;
                int maxAmount = 0;

                object message = null;

                // Not enough resource to craft it
                if (!ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, true))
                {
                    if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                        from.SendGump(new CraftGump(from, craftSystem, tool, message));
                    else if (message is int && (int)message > 0)
                        from.SendLocalizedMessage((int)message);
                    else if (message is string)
                        from.SendMessage((string)message);

                    return;
                }

                tool.UsesRemaining--;

                if (tool.UsesRemaining < 1 && tool.BreakOnDepletion)
                    toolBroken = true;

                if (toolBroken)
                    tool.Delete();

                // SkillCheck failed.
                int num = craftSystem.PlayEndingEffect(from, true, true, toolBroken, endquality, false, this);

                if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
                    from.SendGump(new CraftGump(from, craftSystem, tool, num));
                else if (num > 0)
                    from.SendLocalizedMessage(num);
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_From;
            private int m_iCount;
            private int m_iCountMax;
            private CraftItem m_CraftItem;
            private CraftSystem m_CraftSystem;
            private Type m_TypeRes;
            private BaseTool m_Tool;

            public InternalTimer(Mobile from, CraftSystem craftSystem, CraftItem craftItem, Type typeRes, BaseTool tool, int iCountMax)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(craftSystem.Delay), iCountMax)
            {
                m_From = from;
                m_CraftItem = craftItem;
                m_iCount = 0;
                m_iCountMax = iCountMax;
                m_CraftSystem = craftSystem;
                m_TypeRes = typeRes;
                m_Tool = tool;
            }

            protected override void OnTick()
            {
                m_iCount++;

                m_From.DisruptiveAction();

                if (m_iCount < m_iCountMax)
                {
                    m_CraftSystem.PlayCraftEffect(m_From);
                }
                else
                {
                    m_From.EndAction(typeof(CraftSystem));

                    int badCraft = m_CraftSystem.CanCraft(m_From, m_Tool, m_CraftItem.m_Type);

                    if (badCraft > 0)
                    {
                        if (m_Tool != null && !m_Tool.Deleted && m_Tool.UsesRemaining > 0)
                            m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, badCraft));
                        else
                            m_From.SendLocalizedMessage(badCraft);

                        return;
                    }

                    int quality = 1;
                    bool allRequiredSkills = true;

                    m_CraftItem.CheckSkills(m_From, m_TypeRes, m_CraftSystem, ref quality, ref allRequiredSkills, false);

                    CraftContext context = m_CraftSystem.GetContext(m_From);

                    if (context == null)
                        return;

                    if (typeof(CustomCraft).IsAssignableFrom(m_CraftItem.ItemType))
                    {
                        CustomCraft cc = null;

                        try { cc = Activator.CreateInstance(m_CraftItem.ItemType, new object[] { m_From, m_CraftItem, m_CraftSystem, m_TypeRes, m_Tool, quality }) as CustomCraft; }
                        catch { }

                        if (cc != null)
                            cc.EndCraftAction();

                        return;
                    }

                    bool makersMark = false;

                    if (quality == 2 && m_From.Skills[m_CraftSystem.MainSkill].Base >= 100.0)
                        makersMark = m_CraftItem.IsMarkable(m_CraftItem.ItemType);

                    if (makersMark && context.MarkOption == CraftMarkOption.PromptForMark)
                    {
                        m_From.SendGump(new QueryMakersMarkGump(quality, m_From, m_CraftItem, m_CraftSystem, m_TypeRes, m_Tool));
                    }
                    else
                    {
                        if (context.MarkOption == CraftMarkOption.DoNotMark)
                            makersMark = false;

                        m_CraftItem.CompleteCraft(quality, makersMark, m_From, m_CraftSystem, m_TypeRes, m_Tool, null);
                    }
                }
            }
        }
    }

    public class CraftItemCol : System.Collections.CollectionBase
    {
        public CraftItemCol()
        {
        }

        public int Add(CraftItem craftItem)
        {
            return List.Add(craftItem);
        }

        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
            }
            else
            {
                List.RemoveAt(index);
            }
        }

        public CraftItem GetAt(int index)
        {
            return (CraftItem)List[index];
        }

        public CraftItem SearchForSubclass(Type type)
        {
            for (int i = 0; i < List.Count; i++)
            {
                CraftItem craftItem = (CraftItem)List[i];

                if (craftItem.ItemType == type || type.IsSubclassOf(craftItem.ItemType))
                    return craftItem;
            }

            return null;
        }

        public CraftItem SearchFor(Type type)
        {
            for (int i = 0; i < List.Count; i++)
            {
                CraftItem craftItem = (CraftItem)List[i];
                if (craftItem.ItemType == type)
                {
                    return craftItem;
                }
            }
            return null;
        }
    }

    public class CraftRes
    {
        private Type m_Type;
        private int m_Amount;

        private string m_MessageString;
        private int m_MessageNumber;

        private string m_NameString;
        private int m_NameNumber;

        public CraftRes(Type type, int amount)
        {
            m_Type = type;
            m_Amount = amount;
        }

        public CraftRes(Type type, TextDefinition name, int amount, TextDefinition message)
            : this(type, amount)
        {
            m_NameNumber = name;
            m_MessageNumber = message;

            m_NameString = name;
            m_MessageString = message;
        }

        public void SendMessage(Mobile from)
        {
            if (m_MessageNumber > 0)
                from.SendLocalizedMessage(m_MessageNumber);
            else if (!String.IsNullOrEmpty(m_MessageString))
                from.SendMessage(m_MessageString);
            else
                from.SendLocalizedMessage(502925); // You don't have the resources required to make that item.
        }

        public Type ItemType
        {
            get { return m_Type; }
        }

        public string MessageString
        {
            get { return m_MessageString; }
        }

        public int MessageNumber
        {
            get { return m_MessageNumber; }
        }

        public string NameString
        {
            get { return m_NameString; }
        }

        public int NameNumber
        {
            get { return m_NameNumber; }
        }

        public int Amount
        {
            get { return m_Amount; }
        }
    }

    public class CraftResCol : System.Collections.CollectionBase
    {
        public CraftResCol()
        {
        }

        public void Add(CraftRes craftRes)
        {
            List.Add(craftRes);
        }

        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
            }
            else
            {
                List.RemoveAt(index);
            }
        }

        public CraftRes GetAt(int index)
        {
            return (CraftRes)List[index];
        }
    }

    public class CraftSubRes
    {
        private Type m_Type;
        private double m_ReqSkill;
        private string m_NameString;
        private int m_NameNumber;
        private int m_GenericNameNumber;
        private object m_Message;

        public CraftSubRes(Type type, TextDefinition name, double reqSkill, object message)
            : this(type, name, reqSkill, 0, message)
        {
        }

        public CraftSubRes(Type type, TextDefinition name, double reqSkill, int genericNameNumber, object message)
        {
            m_Type = type;
            m_NameNumber = name;
            m_NameString = name;
            m_ReqSkill = reqSkill;
            m_GenericNameNumber = genericNameNumber;
            m_Message = message;
        }

        public Type ItemType
        {
            get { return m_Type; }
        }

        public string NameString
        {
            get { return m_NameString; }
        }

        public int NameNumber
        {
            get { return m_NameNumber; }
        }

        public int GenericNameNumber
        {
            get { return m_GenericNameNumber; }
        }

        public object Message
        {
            get { return m_Message; }
        }

        public double RequiredSkill
        {
            get { return m_ReqSkill; }
        }
    }

    public class CraftSubResCol : System.Collections.CollectionBase
    {
        private Type m_Type;
        private string m_NameString;
        private int m_NameNumber;
        private bool m_Init;

        public bool Init
        {
            get { return m_Init; }
            set { m_Init = value; }
        }

        public Type ResType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public string NameString
        {
            get { return m_NameString; }
            set { m_NameString = value; }
        }

        public int NameNumber
        {
            get { return m_NameNumber; }
            set { m_NameNumber = value; }
        }

        public CraftSubResCol()
        {
            m_Init = false;
        }

        public void Add(CraftSubRes craftSubRes)
        {
            List.Add(craftSubRes);
        }

        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
            }
            else
            {
                List.RemoveAt(index);
            }
        }

        public CraftSubRes GetAt(int index)
        {
            return (CraftSubRes)List[index];
        }

        public CraftSubRes SearchFor(Type type)
        {
            for (int i = 0; i < List.Count; i++)
            {
                CraftSubRes craftSubRes = (CraftSubRes)List[i];
                if (craftSubRes.ItemType == type)
                {
                    return craftSubRes;
                }
            }
            return null;
        }
    }

    public class CraftGump : Gump
    {
        private Mobile m_From;
        private CraftSystem m_CraftSystem;
        private BaseTool m_Tool;

        private CraftPage m_Page;

        private const int LabelHue = 0x480;
        private const int LabelColor = 0x7FFF;
        private const int FontColor = 0xFFFFFF;

        private enum CraftPage
        {
            None,
            PickResource,
            PickResource2
        }

        /*public CraftGump( Mobile from, CraftSystem craftSystem, BaseTool tool ): this( from, craftSystem, -1, -1, tool, null )
        {
        }*/

        public CraftGump(Mobile from, CraftSystem craftSystem, BaseTool tool, object notice)
            : this(from, craftSystem, tool, notice, CraftPage.None)
        {
        }

        private CraftGump(Mobile from, CraftSystem craftSystem, BaseTool tool, object notice, CraftPage page)
            : base(40, 40)
        {
            m_From = from;
            m_CraftSystem = craftSystem;
            m_Tool = tool;
            m_Page = page;

            CraftContext context = craftSystem.GetContext(from);

            from.CloseGump(typeof(CraftGump));
            from.CloseGump(typeof(CraftGumpItem));

            AddPage(0);

            AddBackground(0, 0, 530, 437, 5054);
            AddImageTiled(10, 10, 510, 22, 2624);
            AddImageTiled(10, 292, 150, 45, 2624);
            AddImageTiled(165, 292, 355, 45, 2624);
            AddImageTiled(10, 342, 510, 85, 2624);
            AddImageTiled(10, 37, 200, 250, 2624);
            AddImageTiled(215, 37, 305, 250, 2624);
            AddAlphaRegion(10, 10, 510, 417);

            if (craftSystem.GumpTitleNumber > 0)
                AddHtmlLocalized(10, 12, 510, 20, craftSystem.GumpTitleNumber, LabelColor, false, false);
            else
                AddHtml(10, 12, 510, 20, craftSystem.GumpTitleString, false, false);

            AddHtmlLocalized(10, 37, 200, 22, 1044010, LabelColor, false, false); // <CENTER>CATEGORIES</CENTER>
            AddHtmlLocalized(215, 37, 305, 22, 1044011, LabelColor, false, false); // <CENTER>SELECTIONS</CENTER>
            AddHtmlLocalized(10, 302, 150, 25, 1044012, LabelColor, false, false); // <CENTER>NOTICES</CENTER>

            AddButton(15, 402, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 405, 150, 18, 1011441, LabelColor, false, false); // EXIT

            AddButton(270, 402, 4005, 4007, GetButtonID(6, 2), GumpButtonType.Reply, 0);
            AddHtmlLocalized(305, 405, 150, 18, 1044013, LabelColor, false, false); // MAKE LAST

            // Mark option
            if (craftSystem.MarkOption)
            {
                AddButton(270, 362, 4005, 4007, GetButtonID(6, 6), GumpButtonType.Reply, 0);
                AddHtmlLocalized(305, 365, 150, 18, 1044017 + (context == null ? 0 : (int)context.MarkOption), LabelColor, false, false); // MARK ITEM
            }
            // ****************************************

            // Resmelt option
            if (craftSystem.Resmelt)
            {
                AddButton(15, 342, 4005, 4007, GetButtonID(6, 1), GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, 345, 150, 18, 1044259, LabelColor, false, false); // SMELT ITEM
            }
            // ****************************************

            // Repair option
            if (craftSystem.Repair)
            {
                AddButton(270, 342, 4005, 4007, GetButtonID(6, 5), GumpButtonType.Reply, 0);
                AddHtmlLocalized(305, 345, 150, 18, 1044260, LabelColor, false, false); // REPAIR ITEM
            }
            // ****************************************

            // Enhance option
            if (craftSystem.CanEnhance)
            {
                AddButton(270, 382, 4005, 4007, GetButtonID(6, 8), GumpButtonType.Reply, 0);
                AddHtmlLocalized(305, 385, 150, 18, 1061001, LabelColor, false, false); // ENHANCE ITEM
            }
            // ****************************************

            if (notice is int && (int)notice > 0)
                AddHtmlLocalized(170, 295, 350, 40, (int)notice, LabelColor, false, false);
            else if (notice is string)
                AddHtml(170, 295, 350, 40, String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", FontColor, notice), false, false);

            // If the system has more than one resource
            if (craftSystem.CraftSubRes.Init)
            {
                string nameString = craftSystem.CraftSubRes.NameString;
                int nameNumber = craftSystem.CraftSubRes.NameNumber;

                int resIndex = (context == null ? -1 : context.LastResourceIndex);

                Type resourceType = craftSystem.CraftSubRes.ResType;

                if (resIndex > -1)
                {
                    CraftSubRes subResource = craftSystem.CraftSubRes.GetAt(resIndex);

                    nameString = subResource.NameString;
                    nameNumber = subResource.NameNumber;
                    resourceType = subResource.ItemType;
                }

                int resourceCount = 0;

                if (from.Backpack != null)
                {
                    Item[] items = from.Backpack.FindItemsByType(resourceType, true);

                    for (int i = 0; i < items.Length; ++i)
                        resourceCount += items[i].Amount;
                }

                AddButton(15, 362, 4005, 4007, GetButtonID(6, 0), GumpButtonType.Reply, 0);

                if (nameNumber > 0)
                    AddHtmlLocalized(50, 365, 250, 18, nameNumber, resourceCount.ToString(), LabelColor, false, false);
                else
                    AddLabel(50, 362, LabelHue, String.Format("{0} ({1} Available)", nameString, resourceCount));
            }
            // ****************************************

            // For dragon scales
            if (craftSystem.CraftSubRes2.Init)
            {
                string nameString = craftSystem.CraftSubRes2.NameString;
                int nameNumber = craftSystem.CraftSubRes2.NameNumber;

                int resIndex = (context == null ? -1 : context.LastResourceIndex2);

                Type resourceType = craftSystem.CraftSubRes2.ResType;

                if (resIndex > -1)
                {
                    CraftSubRes subResource = craftSystem.CraftSubRes2.GetAt(resIndex);

                    nameString = subResource.NameString;
                    nameNumber = subResource.NameNumber;
                    resourceType = subResource.ItemType;
                }

                int resourceCount = 0;

                if (from.Backpack != null)
                {
                    Item[] items = from.Backpack.FindItemsByType(resourceType, true);

                    for (int i = 0; i < items.Length; ++i)
                        resourceCount += items[i].Amount;
                }

                AddButton(15, 382, 4005, 4007, GetButtonID(6, 7), GumpButtonType.Reply, 0);

                if (nameNumber > 0)
                    AddHtmlLocalized(50, 385, 250, 18, nameNumber, resourceCount.ToString(), LabelColor, false, false);
                else
                    AddLabel(50, 385, LabelHue, String.Format("{0} ({1} Available)", nameString, resourceCount));
            }
            // ****************************************

            CreateGroupList();

            if (page == CraftPage.PickResource)
                CreateResList(false, from);
            else if (page == CraftPage.PickResource2)
                CreateResList(true, from);
            else if (context != null && context.LastGroupIndex > -1)
                CreateItemList(context.LastGroupIndex);
        }

        public void CreateResList(bool opt, Mobile from)
        {
            CraftSubResCol res = (opt ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);

            for (int i = 0; i < res.Count; ++i)
            {
                int index = i % 10;

                CraftSubRes subResource = res.GetAt(i);

                if (index == 0)
                {
                    if (i > 0)
                        AddButton(485, 260, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1);

                    AddPage((i / 10) + 1);

                    if (i > 0)
                        AddButton(455, 260, 4014, 4015, 0, GumpButtonType.Page, i / 10);

                    CraftContext context = m_CraftSystem.GetContext(m_From);

                    AddButton(220, 260, 4005, 4007, GetButtonID(6, 4), GumpButtonType.Reply, 0);
                    AddHtmlLocalized(255, 263, 200, 18, (context == null || !context.DoNotColor) ? 1061591 : 1061590, LabelColor, false, false);
                }

                int resourceCount = 0;

                if (from.Backpack != null)
                {
                    Item[] items = from.Backpack.FindItemsByType(subResource.ItemType, true);

                    for (int j = 0; j < items.Length; ++j)
                        resourceCount += items[j].Amount;
                }

                AddButton(220, 60 + (index * 20), 4005, 4007, GetButtonID(5, i), GumpButtonType.Reply, 0);

                if (subResource.NameNumber > 0)
                    AddHtmlLocalized(255, 63 + (index * 20), 250, 18, subResource.NameNumber, resourceCount.ToString(), LabelColor, false, false);
                else
                    AddLabel(255, 60 + (index * 20), LabelHue, String.Format("{0} ({1})", subResource.NameString, resourceCount));
            }
        }

        public void CreateMakeLastList()
        {
            CraftContext context = m_CraftSystem.GetContext(m_From);

            if (context == null)
                return;

            List<CraftItem> items = context.Items;

            if (items.Count > 0)
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    int index = i % 10;

                    CraftItem craftItem = items[i];

                    if (index == 0)
                    {
                        if (i > 0)
                        {
                            AddButton(370, 260, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1);
                            AddHtmlLocalized(405, 263, 100, 18, 1044045, LabelColor, false, false); // NEXT PAGE
                        }

                        AddPage((i / 10) + 1);

                        if (i > 0)
                        {
                            AddButton(220, 260, 4014, 4015, 0, GumpButtonType.Page, i / 10);
                            AddHtmlLocalized(255, 263, 100, 18, 1044044, LabelColor, false, false); // PREV PAGE
                        }
                    }

                    AddButton(220, 60 + (index * 20), 4005, 4007, GetButtonID(3, i), GumpButtonType.Reply, 0);

                    if (craftItem.NameNumber > 0)
                        AddHtmlLocalized(255, 63 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor, false, false);
                    else
                        AddLabel(255, 60 + (index * 20), LabelHue, craftItem.NameString);

                    AddButton(480, 60 + (index * 20), 4011, 4012, GetButtonID(4, i), GumpButtonType.Reply, 0);
                }
            }
            else
            {
                // NOTE: This is not as OSI; it is an intentional difference

                AddHtmlLocalized(230, 62, 200, 22, 1044165, LabelColor, false, false); // You haven't made anything yet.
            }
        }

        public void CreateItemList(int selectedGroup)
        {
            if (selectedGroup == 501) // 501 : Last 10
            {
                CreateMakeLastList();
                return;
            }

            CraftGroupCol craftGroupCol = m_CraftSystem.CraftGroups;
            CraftGroup craftGroup = craftGroupCol.GetAt(selectedGroup);
            CraftItemCol craftItemCol = craftGroup.CraftItems;

            for (int i = 0; i < craftItemCol.Count; ++i)
            {
                int index = i % 10;

                CraftItem craftItem = craftItemCol.GetAt(i);

                if (index == 0)
                {
                    if (i > 0)
                    {
                        AddButton(370, 260, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1);
                        AddHtmlLocalized(405, 263, 100, 18, 1044045, LabelColor, false, false); // NEXT PAGE
                    }

                    AddPage((i / 10) + 1);

                    if (i > 0)
                    {
                        AddButton(220, 260, 4014, 4015, 0, GumpButtonType.Page, i / 10);
                        AddHtmlLocalized(255, 263, 100, 18, 1044044, LabelColor, false, false); // PREV PAGE
                    }
                }

                AddButton(220, 60 + (index * 20), 4005, 4007, GetButtonID(1, i), GumpButtonType.Reply, 0);

                if (craftItem.NameNumber > 0)
                    AddHtmlLocalized(255, 63 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor, false, false);
                else
                    AddLabel(255, 60 + (index * 20), LabelHue, craftItem.NameString);

                AddButton(480, 60 + (index * 20), 4011, 4012, GetButtonID(2, i), GumpButtonType.Reply, 0);
            }
        }

        public int CreateGroupList()
        {
            CraftGroupCol craftGroupCol = m_CraftSystem.CraftGroups;

            AddButton(15, 60, 4005, 4007, GetButtonID(6, 3), GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 63, 150, 18, 1044014, LabelColor, false, false); // LAST TEN

            for (int i = 0; i < craftGroupCol.Count; i++)
            {
                CraftGroup craftGroup = craftGroupCol.GetAt(i);

                AddButton(15, 80 + (i * 20), 4005, 4007, GetButtonID(0, i), GumpButtonType.Reply, 0);

                if (craftGroup.NameNumber > 0)
                    AddHtmlLocalized(50, 83 + (i * 20), 150, 18, craftGroup.NameNumber, LabelColor, false, false);
                else
                    AddLabel(50, 80 + (i * 20), LabelHue, craftGroup.NameString);
            }

            return craftGroupCol.Count;
        }

        public static int GetButtonID(int type, int index)
        {
            return 1 + type + (index * 7);
        }

        public void CraftItem(CraftItem item)
        {
            int num = m_CraftSystem.CanCraft(m_From, m_Tool, item.ItemType);

            if (num > 0)
            {
                m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, num));
            }
            else
            {
                Type type = null;

                CraftContext context = m_CraftSystem.GetContext(m_From);

                if (context != null)
                {
                    CraftSubResCol res = (item.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);
                    int resIndex = (item.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                    if (resIndex >= 0 && resIndex < res.Count)
                        type = res.GetAt(resIndex).ItemType;
                }

                m_CraftSystem.CreateItem(m_From, item.ItemType, type, m_Tool, item);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID <= 0)
                return; // Canceled

            int buttonID = info.ButtonID - 1;
            int type = buttonID % 7;
            int index = buttonID / 7;

            CraftSystem system = m_CraftSystem;
            CraftGroupCol groups = system.CraftGroups;
            CraftContext context = system.GetContext(m_From);

            switch (type)
            {
                case 0: // Show group
                    {
                        if (context == null)
                            break;

                        if (index >= 0 && index < groups.Count)
                        {
                            context.LastGroupIndex = index;
                            m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));
                        }

                        break;
                    }
                case 1: // Create item
                    {
                        if (context == null)
                            break;

                        int groupIndex = context.LastGroupIndex;

                        if (groupIndex >= 0 && groupIndex < groups.Count)
                        {
                            CraftGroup group = groups.GetAt(groupIndex);

                            if (index >= 0 && index < group.CraftItems.Count)
                                CraftItem(group.CraftItems.GetAt(index));
                        }

                        break;
                    }
                case 2: // Item details
                    {
                        if (context == null)
                            break;

                        int groupIndex = context.LastGroupIndex;

                        if (groupIndex >= 0 && groupIndex < groups.Count)
                        {
                            CraftGroup group = groups.GetAt(groupIndex);

                            if (index >= 0 && index < group.CraftItems.Count)
                                m_From.SendGump(new CraftGumpItem(m_From, system, group.CraftItems.GetAt(index), m_Tool));
                        }

                        break;
                    }
                case 3: // Create item (last 10)
                    {
                        if (context == null)
                            break;

                        List<CraftItem> lastTen = context.Items;

                        if (index >= 0 && index < lastTen.Count)
                            CraftItem(lastTen[index]);

                        break;
                    }
                case 4: // Item details (last 10)
                    {
                        if (context == null)
                            break;

                        List<CraftItem> lastTen = context.Items;

                        if (index >= 0 && index < lastTen.Count)
                            m_From.SendGump(new CraftGumpItem(m_From, system, lastTen[index], m_Tool));

                        break;
                    }
                case 5: // Resource selected
                    {
                        if (m_Page == CraftPage.PickResource && index >= 0 && index < system.CraftSubRes.Count)
                        {
                            int groupIndex = (context == null ? -1 : context.LastGroupIndex);

                            CraftSubRes res = system.CraftSubRes.GetAt(index);

                            if (m_From.Skills[system.MainSkill].Base < res.RequiredSkill)
                            {
                                m_From.SendGump(new CraftGump(m_From, system, m_Tool, res.Message));
                            }
                            else
                            {
                                if (context != null)
                                    context.LastResourceIndex = index;

                                m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));
                            }
                        }
                        else if (m_Page == CraftPage.PickResource2 && index >= 0 && index < system.CraftSubRes2.Count)
                        {
                            int groupIndex = (context == null ? -1 : context.LastGroupIndex);

                            CraftSubRes res = system.CraftSubRes2.GetAt(index);

                            if (m_From.Skills[system.MainSkill].Base < res.RequiredSkill)
                            {
                                m_From.SendGump(new CraftGump(m_From, system, m_Tool, res.Message));
                            }
                            else
                            {
                                if (context != null)
                                    context.LastResourceIndex2 = index;

                                m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));
                            }
                        }

                        break;
                    }
                case 6: // Misc. buttons
                    {
                        switch (index)
                        {
                            case 0: // Resource selection
                                {
                                    if (system.CraftSubRes.Init)
                                        m_From.SendGump(new CraftGump(m_From, system, m_Tool, null, CraftPage.PickResource));

                                    break;
                                }
                            case 1: // Smelt item
                                {
                                    if (system.Resmelt)
                                        Resmelt.Do(m_From, system, m_Tool);

                                    break;
                                }
                            case 2: // Make last
                                {
                                    if (context == null)
                                        break;

                                    CraftItem item = context.LastMade;

                                    if (item != null)
                                        CraftItem(item);
                                    else
                                        m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, 1044165, m_Page)); // You haven't made anything yet.

                                    break;
                                }
                            case 3: // Last 10
                                {
                                    if (context == null)
                                        break;

                                    context.LastGroupIndex = 501;
                                    m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));

                                    break;
                                }
                            case 4: // Toggle use resource hue
                                {
                                    if (context == null)
                                        break;

                                    context.DoNotColor = !context.DoNotColor;

                                    m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null, m_Page));

                                    break;
                                }
                            case 5: // Repair item
                                {
                                    if (system.Repair)
                                        Repair.Do(m_From, system, m_Tool);

                                    break;
                                }
                            case 6: // Toggle mark option
                                {
                                    if (context == null || !system.MarkOption)
                                        break;

                                    switch (context.MarkOption)
                                    {
                                        case CraftMarkOption.MarkItem: context.MarkOption = CraftMarkOption.DoNotMark; break;
                                        case CraftMarkOption.DoNotMark: context.MarkOption = CraftMarkOption.PromptForMark; break;
                                        case CraftMarkOption.PromptForMark: context.MarkOption = CraftMarkOption.MarkItem; break;
                                    }

                                    m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null, m_Page));

                                    break;
                                }
                            case 7: // Resource selection 2
                                {
                                    if (system.CraftSubRes2.Init)
                                        m_From.SendGump(new CraftGump(m_From, system, m_Tool, null, CraftPage.PickResource2));

                                    break;
                                }
                            case 8: // Enhance item
                                {
                                    if (system.CanEnhance)
                                        Enhance.BeginTarget(m_From, system, m_Tool);

                                    break;
                                }
                        }

                        break;
                    }
            }
        }
    }

    public class CraftGumpItem : Gump
    {
        private Mobile m_From;
        private CraftSystem m_CraftSystem;
        private CraftItem m_CraftItem;
        private BaseTool m_Tool;

        private const int LabelHue = 0x480; // 0x384
        private const int RedLabelHue = 0x20;

        private const int LabelColor = 0x7FFF;
        private const int RedLabelColor = 0x6400;

        private const int GreyLabelColor = 0x3DEF;

        private int m_OtherCount;

        public CraftGumpItem(Mobile from, CraftSystem craftSystem, CraftItem craftItem, BaseTool tool)
            : base(40, 40)
        {
            m_From = from;
            m_CraftSystem = craftSystem;
            m_CraftItem = craftItem;
            m_Tool = tool;

            from.CloseGump(typeof(CraftGump));
            from.CloseGump(typeof(CraftGumpItem));

            AddPage(0);
            AddBackground(0, 0, 530, 417, 5054);
            AddImageTiled(10, 10, 510, 22, 2624);
            AddImageTiled(10, 37, 150, 148, 2624);
            AddImageTiled(165, 37, 355, 90, 2624);
            AddImageTiled(10, 190, 155, 22, 2624);
            AddImageTiled(10, 217, 150, 53, 2624);
            AddImageTiled(165, 132, 355, 80, 2624);
            AddImageTiled(10, 275, 155, 22, 2624);
            AddImageTiled(10, 302, 150, 53, 2624);
            AddImageTiled(165, 217, 355, 80, 2624);
            AddImageTiled(10, 360, 155, 22, 2624);
            AddImageTiled(165, 302, 355, 80, 2624);
            AddImageTiled(10, 387, 510, 22, 2624);
            AddAlphaRegion(10, 10, 510, 399);

            AddHtmlLocalized(170, 40, 150, 20, 1044053, LabelColor, false, false); // ITEM
            AddHtmlLocalized(10, 192, 150, 22, 1044054, LabelColor, false, false); // <CENTER>SKILLS</CENTER>
            AddHtmlLocalized(10, 277, 150, 22, 1044055, LabelColor, false, false); // <CENTER>MATERIALS</CENTER>
            AddHtmlLocalized(10, 362, 150, 22, 1044056, LabelColor, false, false); // <CENTER>OTHER</CENTER>

            if (craftSystem.GumpTitleNumber > 0)
                AddHtmlLocalized(10, 12, 510, 20, craftSystem.GumpTitleNumber, LabelColor, false, false);
            else
                AddHtml(10, 12, 510, 20, craftSystem.GumpTitleString, false, false);

            AddButton(15, 387, 4014, 4016, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 390, 150, 18, 1044150, LabelColor, false, false); // BACK

            bool needsRecipe = (craftItem.Recipe != null && from is PlayerMobile && !((PlayerMobile)from).HasRecipe(craftItem.Recipe));

            if (needsRecipe)
            {
                AddButton(270, 387, 4005, 4007, 0, GumpButtonType.Page, 0);
                AddHtmlLocalized(305, 390, 150, 18, 1044151, GreyLabelColor, false, false); // MAKE NOW
            }
            else
            {
                AddButton(270, 387, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(305, 390, 150, 18, 1044151, LabelColor, false, false); // MAKE NOW
            }

            if (craftItem.NameNumber > 0)
                AddHtmlLocalized(330, 40, 180, 18, craftItem.NameNumber, LabelColor, false, false);
            else
                AddLabel(330, 40, LabelHue, craftItem.NameString);

            if (craftItem.UseAllRes)
                AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1048176, LabelColor, false, false); // Makes as many as possible at once

            DrawItem();
            DrawSkill();
            DrawResource();

            /*
            if( craftItem.RequiresSE )
                AddHtmlLocalized( 170, 302 + (m_OtherCount++ * 20), 310, 18, 1063363, LabelColor, false, false ); //* Requires the "Samurai Empire" expansion
             * */

            if (craftItem.RequiredExpansion != Expansion.None)
            {
                bool supportsEx = (from.NetState != null && from.NetState.SupportsExpansion(craftItem.RequiredExpansion));
                TextDefinition.AddHtmlText(this, 170, 302 + (m_OtherCount++ * 20), 310, 18, RequiredExpansionMessage(craftItem.RequiredExpansion), false, false, supportsEx ? LabelColor : RedLabelColor, supportsEx ? LabelHue : RedLabelHue);
            }

            if (needsRecipe)
                AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1073620, RedLabelColor, false, false); // You have not learned this recipe.

        }

        private TextDefinition RequiredExpansionMessage(Expansion expansion)
        {
            switch (expansion)
            {
                case Expansion.SE:
                    return 1063363; // * Requires the "Samurai Empire" expansion
                case Expansion.ML:
                    return 1072651; // * Requires the "Mondain's Legacy" expansion
                default:
                    return String.Format("* Requires the \"{0}\" expansion", ExpansionInfo.GetInfo(expansion).Name);
            }
        }

        private bool m_ShowExceptionalChance;

        public void DrawItem()
        {
            Type type = m_CraftItem.ItemType;

            AddItem(20, 50, CraftItem.ItemIDOf(type), m_CraftItem.ItemHue);

            if (m_CraftItem.IsMarkable(type))
            {
                AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1044059, LabelColor, false, false); // This item may hold its maker's mark
                m_ShowExceptionalChance = true;
            }
        }

        public void DrawSkill()
        {
            for (int i = 0; i < m_CraftItem.Skills.Count; i++)
            {
                CraftSkill skill = m_CraftItem.Skills.GetAt(i);
                double minSkill = skill.MinSkill, maxSkill = skill.MaxSkill;

                if (minSkill < 0)
                    minSkill = 0;

                AddHtmlLocalized(170, 132 + (i * 20), 200, 18, AosSkillBonuses.GetLabel(skill.SkillToMake), LabelColor, false, false);
                AddLabel(430, 132 + (i * 20), LabelHue, String.Format("{0:F1}", minSkill));
            }

            CraftSubResCol res = (m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);
            int resIndex = -1;

            CraftContext context = m_CraftSystem.GetContext(m_From);

            if (context != null)
                resIndex = (m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

            bool allRequiredSkills = true;
            double chance = m_CraftItem.GetSuccessChance(m_From, resIndex > -1 ? res.GetAt(resIndex).ItemType : null, m_CraftSystem, false, ref allRequiredSkills);
            double excepChance = m_CraftItem.GetExceptionalChance(m_CraftSystem, chance, m_From);

            if (chance < 0.0)
                chance = 0.0;
            else if (chance > 1.0)
                chance = 1.0;

            AddHtmlLocalized(170, 80, 250, 18, 1044057, LabelColor, false, false); // Success Chance:
            AddLabel(430, 80, LabelHue, String.Format("{0:F1}%", chance * 100));

            if (m_ShowExceptionalChance)
            {
                if (excepChance < 0.0)
                    excepChance = 0.0;
                else if (excepChance > 1.0)
                    excepChance = 1.0;

                AddHtmlLocalized(170, 100, 250, 18, 1044058, 32767, false, false); // Exceptional Chance:
                AddLabel(430, 100, LabelHue, String.Format("{0:F1}%", excepChance * 100));
            }
        }

        private static Type typeofBlankScroll = typeof(BlankScroll);
        private static Type typeofSpellScroll = typeof(SpellScroll);

        public void DrawResource()
        {
            bool retainedColor = false;

            CraftContext context = m_CraftSystem.GetContext(m_From);

            CraftSubResCol res = (m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);
            int resIndex = -1;

            if (context != null)
                resIndex = (m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

            bool cropScroll = (m_CraftItem.Resources.Count > 1)
                && m_CraftItem.Resources.GetAt(m_CraftItem.Resources.Count - 1).ItemType == typeofBlankScroll
                && typeofSpellScroll.IsAssignableFrom(m_CraftItem.ItemType);

            for (int i = 0; i < m_CraftItem.Resources.Count - (cropScroll ? 1 : 0) && i < 4; i++)
            {
                Type type;
                string nameString;
                int nameNumber;

                CraftRes craftResource = m_CraftItem.Resources.GetAt(i);

                type = craftResource.ItemType;
                nameString = craftResource.NameString;
                nameNumber = craftResource.NameNumber;

                // Resource Mutation
                if (type == res.ResType && resIndex > -1)
                {
                    CraftSubRes subResource = res.GetAt(resIndex);

                    type = subResource.ItemType;

                    nameString = subResource.NameString;
                    nameNumber = subResource.GenericNameNumber;

                    if (nameNumber <= 0)
                        nameNumber = subResource.NameNumber;
                }
                // ******************

                if (!retainedColor && m_CraftItem.RetainsColorFrom(m_CraftSystem, type))
                {
                    retainedColor = true;
                    AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1044152, LabelColor, false, false); // * The item retains the color of this material
                    AddLabel(500, 219 + (i * 20), LabelHue, "*");
                }

                if (nameNumber > 0)
                    AddHtmlLocalized(170, 219 + (i * 20), 310, 18, nameNumber, LabelColor, false, false);
                else
                    AddLabel(170, 219 + (i * 20), LabelHue, nameString);

                AddLabel(430, 219 + (i * 20), LabelHue, craftResource.Amount.ToString());
            }

            if (m_CraftItem.NameNumber == 1041267) // runebook
            {
                AddHtmlLocalized(170, 219 + (m_CraftItem.Resources.Count * 20), 310, 18, 1044447, LabelColor, false, false);
                AddLabel(430, 219 + (m_CraftItem.Resources.Count * 20), LabelHue, "1");
            }

            if (cropScroll)
                AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 360, 18, 1044379, LabelColor, false, false); // Inscribing scrolls also requires a blank scroll and mana.
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            // Back Button
            if (info.ButtonID == 0)
            {
                CraftGump craftGump = new CraftGump(m_From, m_CraftSystem, m_Tool, null);
                m_From.SendGump(craftGump);
            }
            else // Make Button
            {
                int num = m_CraftSystem.CanCraft(m_From, m_Tool, m_CraftItem.ItemType);

                if (num > 0)
                {
                    m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, num));
                }
                else
                {
                    Type type = null;

                    CraftContext context = m_CraftSystem.GetContext(m_From);

                    if (context != null)
                    {
                        CraftSubResCol res = (m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);
                        int resIndex = (m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                        if (resIndex > -1)
                            type = res.GetAt(resIndex).ItemType;
                    }

                    m_CraftSystem.CreateItem(m_From, m_CraftItem.ItemType, type, m_Tool, m_CraftItem);
                }
            }
        }
    }

    public enum EnhanceResult
    {
        None,
        NotInBackpack,
        BadItem,
        BadResource,
        AlreadyEnhanced,
        Success,
        Failure,
        Broken,
        NoResources,
        NoSkill
    }

    public class Enhance
    {
        public static EnhanceResult Invoke(Mobile from, CraftSystem craftSystem, BaseTool tool, Item item, CraftResource resource, Type resType, ref object resMessage)
        {
            if (item == null)
                return EnhanceResult.BadItem;

            if (!item.IsChildOf(from.Backpack))
                return EnhanceResult.NotInBackpack;

            if (!(item is BaseArmor) && !(item is BaseWeapon))
                return EnhanceResult.BadItem;

            if (item is IArcaneEquip)
            {
                IArcaneEquip eq = (IArcaneEquip)item;
                if (eq.IsArcane)
                    return EnhanceResult.BadItem;
            }

            if (CraftResources.IsStandard(resource))
                return EnhanceResult.BadResource;

            int num = craftSystem.CanCraft(from, tool, item.GetType());

            if (num > 0)
            {
                resMessage = num;
                return EnhanceResult.None;
            }

            CraftItem craftItem = craftSystem.CraftItems.SearchFor(item.GetType());

            if (craftItem == null || craftItem.Resources.Count == 0)
                return EnhanceResult.BadItem;

            bool allRequiredSkills = false;
            if (craftItem.GetSuccessChance(from, resType, craftSystem, false, ref allRequiredSkills) <= 0.0)
                return EnhanceResult.NoSkill;

            CraftResourceInfo info = CraftResources.GetInfo(resource);

            if (info == null || info.ResourceTypes.Length == 0)
                return EnhanceResult.BadResource;

            CraftAttributeInfo attributes = info.AttributeInfo;

            if (attributes == null)
                return EnhanceResult.BadResource;

            int resHue = 0, maxAmount = 0;

            if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref resMessage))
                return EnhanceResult.NoResources;

            if (craftSystem is DefBlacksmithy)
            {
                AncientSmithyHammer hammer = from.FindItemOnLayer(Layer.OneHanded) as AncientSmithyHammer;
                if (hammer != null)
                {
                    hammer.UsesRemaining--;
                    if (hammer.UsesRemaining < 1)
                        hammer.Delete();
                }
            }

            int phys = 0, fire = 0, cold = 0, pois = 0, nrgy = 0;
            int dura = 0, luck = 0, lreq = 0, dinc = 0;
            int baseChance = 0;

            bool physBonus = false;
            bool fireBonus = false;
            bool coldBonus = false;
            bool nrgyBonus = false;
            bool poisBonus = false;
            bool duraBonus = false;
            bool luckBonus = false;
            bool lreqBonus = false;
            bool dincBonus = false;

            if (item is BaseWeapon)
            {
                BaseWeapon weapon = (BaseWeapon)item;

                if (!CraftResources.IsStandard(weapon.Resource))
                    return EnhanceResult.AlreadyEnhanced;

                baseChance = 20;

                dura = weapon.MaxHitPoints;
                luck = weapon.Attributes.Luck;
                lreq = weapon.WeaponAttributes.LowerStatReq;
                dinc = weapon.Attributes.WeaponDamage;

                fireBonus = (attributes.WeaponFireDamage > 0);
                coldBonus = (attributes.WeaponColdDamage > 0);
                nrgyBonus = (attributes.WeaponEnergyDamage > 0);
                poisBonus = (attributes.WeaponPoisonDamage > 0);

                duraBonus = (attributes.WeaponDurability > 0);
                luckBonus = (attributes.WeaponLuck > 0);
                lreqBonus = (attributes.WeaponLowerRequirements > 0);
                dincBonus = (dinc > 0);
            }
            else
            {
                BaseArmor armor = (BaseArmor)item;

                if (!CraftResources.IsStandard(armor.Resource))
                    return EnhanceResult.AlreadyEnhanced;

                baseChance = 20;

                phys = armor.PhysicalResistance;
                fire = armor.FireResistance;
                cold = armor.ColdResistance;
                pois = armor.PoisonResistance;
                nrgy = armor.EnergyResistance;

                dura = armor.MaxHitPoints;
                luck = armor.Attributes.Luck;
                lreq = armor.ArmorAttributes.LowerStatReq;

                physBonus = (attributes.ArmorPhysicalResist > 0);
                fireBonus = (attributes.ArmorFireResist > 0);
                coldBonus = (attributes.ArmorColdResist > 0);
                nrgyBonus = (attributes.ArmorEnergyResist > 0);
                poisBonus = (attributes.ArmorPoisonResist > 0);

                duraBonus = (attributes.ArmorDurability > 0);
                luckBonus = (attributes.ArmorLuck > 0);
                lreqBonus = (attributes.ArmorLowerRequirements > 0);
                dincBonus = false;
            }

            int skill = from.Skills[craftSystem.MainSkill].Fixed / 10;

            if (skill >= 100)
                baseChance -= (skill - 90) / 10;

            EnhanceResult res = EnhanceResult.Success;

            if (physBonus)
                CheckResult(ref res, baseChance + phys);

            if (fireBonus)
                CheckResult(ref res, baseChance + fire);

            if (coldBonus)
                CheckResult(ref res, baseChance + cold);

            if (nrgyBonus)
                CheckResult(ref res, baseChance + nrgy);

            if (poisBonus)
                CheckResult(ref res, baseChance + pois);

            if (duraBonus)
                CheckResult(ref res, baseChance + (dura / 40));

            if (luckBonus)
                CheckResult(ref res, baseChance + 10 + (luck / 2));

            if (lreqBonus)
                CheckResult(ref res, baseChance + (lreq / 4));

            if (dincBonus)
                CheckResult(ref res, baseChance + (dinc / 4));

            switch (res)
            {
                case EnhanceResult.Broken:
                    {
                        if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.Half, ref resMessage))
                            return EnhanceResult.NoResources;

                        item.Delete();
                        break;
                    }
                case EnhanceResult.Success:
                    {
                        if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref resMessage))
                            return EnhanceResult.NoResources;

                        if (item is BaseWeapon)
                        {
                            BaseWeapon w = (BaseWeapon)item;

                            w.Resource = resource;

                            int hue = w.GetElementalDamageHue();
                            if (hue > 0)
                                w.Hue = hue;
                        }
                        else if (item is BaseArmor)	//Sanity
                        {
                            ((BaseArmor)item).Resource = resource;
                        }

                        break;
                    }
                case EnhanceResult.Failure:
                    {
                        if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.Half, ref resMessage))
                            return EnhanceResult.NoResources;

                        break;
                    }
            }

            return res;
        }

        public static void CheckResult(ref EnhanceResult res, int chance)
        {
            if (res != EnhanceResult.Success)
                return; // we've already failed..

            int random = Utility.Random(100);

            if (10 > random)
                res = EnhanceResult.Failure;
            else if (chance > random)
                res = EnhanceResult.Broken;
        }

        public static void BeginTarget(Mobile from, CraftSystem craftSystem, BaseTool tool)
        {
            CraftContext context = craftSystem.GetContext(from);

            if (context == null)
                return;

            int lastRes = context.LastResourceIndex;
            CraftSubResCol subRes = craftSystem.CraftSubRes;

            if (lastRes >= 0 && lastRes < subRes.Count)
            {
                CraftSubRes res = subRes.GetAt(lastRes);

                if (from.Skills[craftSystem.MainSkill].Value < res.RequiredSkill)
                {
                    from.SendGump(new CraftGump(from, craftSystem, tool, res.Message));
                }
                else
                {
                    CraftResource resource = CraftResources.GetFromType(res.ItemType);

                    if (resource != CraftResource.None)
                    {
                        from.Target = new InternalTarget(craftSystem, tool, res.ItemType, resource);
                        from.SendLocalizedMessage(1061004); // Target an item to enhance with the properties of your selected material.
                    }
                    else
                    {
                        from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
                    }
                }
            }
            else
            {
                from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
            }

        }

        private class InternalTarget : Target
        {
            private CraftSystem m_CraftSystem;
            private BaseTool m_Tool;
            private Type m_ResourceType;
            private CraftResource m_Resource;

            public InternalTarget(CraftSystem craftSystem, BaseTool tool, Type resourceType, CraftResource resource)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Tool = tool;
                m_ResourceType = resourceType;
                m_Resource = resource;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item)
                {
                    object message = null;
                    EnhanceResult res = Enhance.Invoke(from, m_CraftSystem, m_Tool, (Item)targeted, m_Resource, m_ResourceType, ref message);

                    switch (res)
                    {
                        case EnhanceResult.NotInBackpack: message = 1061005; break; // The item must be in your backpack to enhance it.
                        case EnhanceResult.AlreadyEnhanced: message = 1061012; break; // This item is already enhanced with the properties of a special material.
                        case EnhanceResult.BadItem: message = 1061011; break; // You cannot enhance this type of item with the properties of the selected special material.
                        case EnhanceResult.BadResource: message = 1061010; break; // You must select a special material in order to enhance an item with its properties.
                        case EnhanceResult.Broken: message = 1061080; break; // You attempt to enhance the item, but fail catastrophically. The item is lost.
                        case EnhanceResult.Failure: message = 1061082; break; // You attempt to enhance the item, but fail. Some material is lost in the process.
                        case EnhanceResult.Success: message = 1061008; break; // You enhance the item with the properties of the special material.
                        case EnhanceResult.NoSkill: message = 1044153; break; // You don't have the required skills to attempt this item.
                    }

                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, message));
                }
            }
        }
    }

    public class Repair
    {
        public Repair()
        {
        }

        public static void Do(Mobile from, CraftSystem craftSystem, BaseTool tool)
        {
            from.Target = new InternalTarget(craftSystem, tool);
            from.SendLocalizedMessage(1044276); // Target an item to repair.
        }

        public static void Do(Mobile from, CraftSystem craftSystem, RepairDeed deed)
        {
            from.Target = new InternalTarget(craftSystem, deed);
            from.SendLocalizedMessage(1044276); // Target an item to repair.
        }

        private class InternalTarget : Target
        {
            private CraftSystem m_CraftSystem;
            private BaseTool m_Tool;
            private RepairDeed m_Deed;

            public InternalTarget(CraftSystem craftSystem, BaseTool tool)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Tool = tool;
            }

            public InternalTarget(CraftSystem craftSystem, RepairDeed deed)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Deed = deed;
            }

            private static void EndGolemRepair(object state)
            {
                ((Mobile)state).EndAction(typeof(Golem));
            }

            private int GetWeakenChance(Mobile mob, SkillName skill, int curHits, int maxHits)
            {
                // 40% - (1% per hp lost) - (1% per 10 craft skill)
                return (40 + (maxHits - curHits)) - (int)(((m_Deed != null) ? m_Deed.SkillLevel : mob.Skills[skill].Value) / 10);
            }

            private bool CheckWeaken(Mobile mob, SkillName skill, int curHits, int maxHits)
            {
                return (GetWeakenChance(mob, skill, curHits, maxHits) > Utility.Random(100));
            }

            private int GetRepairDifficulty(int curHits, int maxHits)
            {
                return (((maxHits - curHits) * 1250) / Math.Max(maxHits, 1)) - 250;
            }

            private bool CheckRepairDifficulty(Mobile mob, SkillName skill, int curHits, int maxHits)
            {
                double difficulty = GetRepairDifficulty(curHits, maxHits) * 0.1;


                if (m_Deed != null)
                {
                    double value = m_Deed.SkillLevel;
                    double minSkill = difficulty - 25.0;
                    double maxSkill = difficulty + 25;

                    if (value < minSkill)
                        return false; // Too difficult
                    else if (value >= maxSkill)
                        return true; // No challenge

                    double chance = (value - minSkill) / (maxSkill - minSkill);

                    return (chance >= Utility.RandomDouble());
                }
                else
                {
                    return mob.CheckSkill(skill, difficulty - 25.0, difficulty + 25.0);
                }
            }

            private bool CheckDeed(Mobile from)
            {
                if (m_Deed != null)
                {
                    return m_Deed.Check(from);
                }

                return true;
            }

            private bool IsSpecialClothing(BaseClothing clothing)
            {
                // Clothing repairable but not craftable

                if (m_CraftSystem is DefTailoring)
                {
                    return (clothing is BearMask)
                        || (clothing is DeerMask)
                        || (clothing is TheMostKnowledgePerson)
                        || (clothing is TheRobeOfBritanniaAri)
                        || (clothing is EmbroideredOakLeafCloak);
                }

                return false;
            }

            private bool IsSpecialWeapon(BaseWeapon weapon)
            {
                // Weapons repairable but not craftable

                if (m_CraftSystem is DefTinkering)
                {
                    return (weapon is Cleaver)
                        || (weapon is Hatchet)
                        || (weapon is Pickaxe)
                        || (weapon is ButcherKnife)
                        || (weapon is SkinningKnife);
                }
                else if (m_CraftSystem is DefCarpentry)
                {
                    return (weapon is Club)
                        || (weapon is BlackStaff)
                        || (weapon is MagicWand)
                    #region Temporary
                        // TODO: Make these items craftable
                        || (weapon is WildStaff);
                    #endregion
                }
                else if (m_CraftSystem is DefBlacksmithy)
                {
                    return (weapon is Pitchfork)
                    #region Temporary
                        // TODO: Make these items craftable
                        || (weapon is RadiantScimitar)
                        || (weapon is WarCleaver)
                        || (weapon is ElvenSpellblade)
                        || (weapon is AssassinSpike)
                        || (weapon is Leafblade)
                        || (weapon is RuneBlade)
                        || (weapon is ElvenMachete)
                        || (weapon is OrnateAxe)
                        || (weapon is DiamondMace);
                    #endregion
                }
                #region Temporary
                // TODO: Make these items craftable
                else if (m_CraftSystem is DefBowFletching)
                {
                    return (weapon is ElvenCompositeLongbow)
                        || (weapon is MagicalShortbow);
                }
                #endregion

                return false;
            }

            private bool IsSpecialArmor(BaseArmor armor)
            {
                // Armor repairable but not craftable

                #region Temporary
                // TODO: Make these items craftable
                if (m_CraftSystem is DefTailoring)
                {
                    return (armor is LeafTonlet)
                        || (armor is LeafArms)
                        || (armor is LeafChest)
                        || (armor is LeafGloves)
                        || (armor is LeafGorget)
                        || (armor is LeafLegs)
                        || (armor is HideChest)
                        || (armor is HideGloves)
                        || (armor is HideGorget)
                        || (armor is HidePants)
                        || (armor is HidePauldrons);
                }
                else if (m_CraftSystem is DefCarpentry)
                {
                    return (armor is WingedHelm)
                        || (armor is RavenHelm)
                        || (armor is VultureHelm)
                        || (armor is WoodlandArms)
                        || (armor is WoodlandChest)
                        || (armor is WoodlandGloves)
                        || (armor is WoodlandGorget)
                        || (armor is WoodlandLegs);
                }
                else if (m_CraftSystem is DefBlacksmithy)
                {
                    return (armor is Circlet)
                        || (armor is RoyalCirclet)
                        || (armor is GemmedCirclet);
                }
                #endregion

                return false;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                int number;

                if (!CheckDeed(from))
                    return;

                bool usingDeed = (m_Deed != null);
                bool toDelete = false;

                // TODO: Make an IRepairable

                if (m_CraftSystem.CanCraft(from, m_Tool, targeted.GetType()) == 1044267)
                {
                    number = 1044282; // You must be near a forge and and anvil to repair items. * Yes, there are two and's *
                }
                else if (m_CraftSystem is DefTinkering && targeted is Golem)
                {
                    Golem g = (Golem)targeted;
                    int damage = g.HitsMax - g.Hits;

                    if (g.IsDeadBondedPet)
                    {
                        number = 500426; // You can't repair that.
                    }
                    else if (damage <= 0)
                    {
                        number = 500423; // That is already in full repair.
                    }
                    else
                    {
                        double skillValue = (usingDeed) ? m_Deed.SkillLevel : from.Skills[SkillName.Tinkering].Value;

                        if (skillValue < 60.0)
                        {
                            number = 1044153; // You don't have the required skills to attempt this item.	//TODO: How does OSI handle this with deeds with golems?
                        }
                        else if (!from.CanBeginAction(typeof(Golem)))
                        {
                            number = 501789; // You must wait before trying again.
                        }
                        else
                        {
                            if (damage > (int)(skillValue * 0.3))
                                damage = (int)(skillValue * 0.3);

                            damage += 30;

                            if (!from.CheckSkill(SkillName.Tinkering, 0.0, 100.0))
                                damage /= 2;

                            Container pack = from.Backpack;

                            if (pack != null)
                            {
                                int v = pack.ConsumeUpTo(typeof(IronIngot), (damage + 4) / 5);

                                if (v > 0)
                                {
                                    g.Hits += v * 5;

                                    number = 1044279; // You repair the item.
                                    toDelete = true;

                                    from.BeginAction(typeof(Golem));
                                    Timer.DelayCall(TimeSpan.FromSeconds(12.0), new TimerStateCallback(EndGolemRepair), from);
                                }
                                else
                                {
                                    number = 1044037; // You do not have sufficient metal to make that.
                                }
                            }
                            else
                            {
                                number = 1044037; // You do not have sufficient metal to make that.
                            }
                        }
                    }
                }
                else if (targeted is BaseWeapon)
                {
                    BaseWeapon weapon = (BaseWeapon)targeted;
                    SkillName skill = m_CraftSystem.MainSkill;
                    int toWeaken = 0;

                    if (Core.AOS)
                    {
                        toWeaken = 1;
                    }
                    else if (skill != SkillName.Tailoring)
                    {
                        double skillLevel = (usingDeed) ? m_Deed.SkillLevel : from.Skills[skill].Base;

                        if (skillLevel >= 90.0)
                            toWeaken = 1;
                        else if (skillLevel >= 70.0)
                            toWeaken = 2;
                        else
                            toWeaken = 3;
                    }

                    if (m_CraftSystem.CraftItems.SearchForSubclass(weapon.GetType()) == null && !IsSpecialWeapon(weapon))
                    {
                        number = (usingDeed) ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                    }
                    else if (!weapon.IsChildOf(from.Backpack) && (!Core.ML || weapon.Parent != from))
                    {
                        number = 1044275; // The item must be in your backpack to repair it.
                    }
                    else if (!Core.AOS && weapon.PoisonCharges != 0)
                    {
                        number = 1005012; // You cannot repair an item while a caustic substance is on it.
                    }
                    else if (weapon.MaxHitPoints <= 0 || weapon.HitPoints == weapon.MaxHitPoints)
                    {
                        number = 1044281; // That item is in full repair
                    }
                    else if (weapon.MaxHitPoints <= toWeaken)
                    {
                        number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
                    }
                    else
                    {
                        if (CheckWeaken(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
                        {
                            weapon.MaxHitPoints -= toWeaken;
                            weapon.HitPoints = Math.Max(0, weapon.HitPoints - toWeaken);
                        }

                        if (CheckRepairDifficulty(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
                        {
                            number = 1044279; // You repair the item.
                            m_CraftSystem.PlayCraftEffect(from);
                            weapon.HitPoints = weapon.MaxHitPoints;
                        }
                        else
                        {
                            number = (usingDeed) ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
                            m_CraftSystem.PlayCraftEffect(from);
                        }

                        toDelete = true;
                    }
                }
                else if (targeted is BaseArmor)
                {
                    BaseArmor armor = (BaseArmor)targeted;
                    SkillName skill = m_CraftSystem.MainSkill;
                    int toWeaken = 0;

                    if (Core.AOS)
                    {
                        toWeaken = 1;
                    }
                    else if (skill != SkillName.Tailoring)
                    {
                        double skillLevel = (usingDeed) ? m_Deed.SkillLevel : from.Skills[skill].Base;

                        if (skillLevel >= 90.0)
                            toWeaken = 1;
                        else if (skillLevel >= 70.0)
                            toWeaken = 2;
                        else
                            toWeaken = 3;
                    }

                    if (m_CraftSystem.CraftItems.SearchForSubclass(armor.GetType()) == null && !IsSpecialArmor(armor))
                    {
                        number = (usingDeed) ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                    }
                    else if (!armor.IsChildOf(from.Backpack) && (!Core.ML || armor.Parent != from))
                    {
                        number = 1044275; // The item must be in your backpack to repair it.
                    }
                    else if (armor.MaxHitPoints <= 0 || armor.HitPoints == armor.MaxHitPoints)
                    {
                        number = 1044281; // That item is in full repair
                    }
                    else if (armor.MaxHitPoints <= toWeaken)
                    {
                        number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
                    }
                    else
                    {
                        if (CheckWeaken(from, skill, armor.HitPoints, armor.MaxHitPoints))
                        {
                            armor.MaxHitPoints -= toWeaken;
                            armor.HitPoints = Math.Max(0, armor.HitPoints - toWeaken);
                        }

                        if (CheckRepairDifficulty(from, skill, armor.HitPoints, armor.MaxHitPoints))
                        {
                            number = 1044279; // You repair the item.
                            m_CraftSystem.PlayCraftEffect(from);
                            armor.HitPoints = armor.MaxHitPoints;
                        }
                        else
                        {
                            number = (usingDeed) ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
                            m_CraftSystem.PlayCraftEffect(from);
                        }

                        toDelete = true;
                    }
                }
                else if (targeted is BaseClothing)
                {
                    BaseClothing clothing = (BaseClothing)targeted;
                    SkillName skill = m_CraftSystem.MainSkill;
                    int toWeaken = 0;

                    if (Core.AOS)
                    {
                        toWeaken = 1;
                    }
                    else if (skill != SkillName.Tailoring)
                    {
                        double skillLevel = (usingDeed) ? m_Deed.SkillLevel : from.Skills[skill].Base;

                        if (skillLevel >= 90.0)
                            toWeaken = 1;
                        else if (skillLevel >= 70.0)
                            toWeaken = 2;
                        else
                            toWeaken = 3;
                    }

                    if (m_CraftSystem.CraftItems.SearchForSubclass(clothing.GetType()) == null && !IsSpecialClothing(clothing) && !((targeted is TribalMask) || (targeted is HornedTribalMask)))
                    {
                        number = (usingDeed) ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                    }
                    else if (!clothing.IsChildOf(from.Backpack) && (!Core.ML || clothing.Parent != from))
                    {
                        number = 1044275; // The item must be in your backpack to repair it.
                    }
                    else if (clothing.MaxHitPoints <= 0 || clothing.HitPoints == clothing.MaxHitPoints)
                    {
                        number = 1044281; // That item is in full repair
                    }
                    else if (clothing.MaxHitPoints <= toWeaken)
                    {
                        number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
                    }
                    else
                    {
                        if (CheckWeaken(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
                        {
                            clothing.MaxHitPoints -= toWeaken;
                            clothing.HitPoints = Math.Max(0, clothing.HitPoints - toWeaken);
                        }

                        if (CheckRepairDifficulty(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
                        {
                            number = 1044279; // You repair the item.
                            m_CraftSystem.PlayCraftEffect(from);
                            clothing.HitPoints = clothing.MaxHitPoints;
                        }
                        else
                        {
                            number = (usingDeed) ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
                            m_CraftSystem.PlayCraftEffect(from);
                        }

                        toDelete = true;
                    }
                }
                else if (!usingDeed && targeted is BlankScroll)
                {
                    SkillName skill = m_CraftSystem.MainSkill;

                    if (from.Skills[skill].Value >= 50.0)
                    {
                        ((BlankScroll)targeted).Consume(1);
                        RepairDeed deed = new RepairDeed(RepairDeed.GetTypeFor(m_CraftSystem), from.Skills[skill].Value, from);
                        from.AddToBackpack(deed);

                        number = 500442; // You create the item and put it in your backpack.
                    }
                    else
                        number = 1047005; // You must be at least apprentice level to create a repair service contract.
                }
                else if (targeted is Item)
                {
                    number = (usingDeed) ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                }
                else
                {
                    number = 500426; // You can't repair that.
                }

                if (!usingDeed)
                {
                    CraftContext context = m_CraftSystem.GetContext(from);
                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, number));
                }
                else
                {
                    from.SendLocalizedMessage(number);

                    if (toDelete)
                        m_Deed.Delete();
                }
            }
        }
    }

    public enum SmeltResult
    {
        Success,
        Invalid,
        NoSkill
    }

    public class Resmelt
    {
        public Resmelt()
        {
        }

        public static void Do(Mobile from, CraftSystem craftSystem, BaseTool tool)
        {
            int num = craftSystem.CanCraft(from, tool, null);

            if (num > 0 && num != 1044267)
            {
                from.SendGump(new CraftGump(from, craftSystem, tool, num));
            }
            else
            {
                from.Target = new InternalTarget(craftSystem, tool);
                from.SendLocalizedMessage(1044273); // Target an item to recycle.
            }
        }

        private class InternalTarget : Target
        {
            private CraftSystem m_CraftSystem;
            private BaseTool m_Tool;

            public InternalTarget(CraftSystem craftSystem, BaseTool tool)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Tool = tool;
            }

            private SmeltResult Resmelt(Mobile from, Item item, CraftResource resource)
            {
                try
                {

                    if (Ethics.Ethic.IsImbued(item))
                        return SmeltResult.Invalid;

                    if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                        return SmeltResult.Invalid;

                    CraftResourceInfo info = CraftResources.GetInfo(resource);

                    if (info == null || info.ResourceTypes.Length == 0)
                        return SmeltResult.Invalid;

                    CraftItem craftItem = m_CraftSystem.CraftItems.SearchFor(item.GetType());

                    if (craftItem == null || craftItem.Resources.Count == 0)
                        return SmeltResult.Invalid;

                    CraftRes craftResource = craftItem.Resources.GetAt(0);

                    if (craftResource.Amount < 2)
                        return SmeltResult.Invalid; // Not enough metal to resmelt

                    double difficulty = 0.0;

                    switch (resource)
                    {
                        case CraftResource.DullCopper: difficulty = 65.0; break;
                        case CraftResource.ShadowIron: difficulty = 70.0; break;
                        case CraftResource.Copper: difficulty = 75.0; break;
                        case CraftResource.Bronze: difficulty = 80.0; break;
                        case CraftResource.Gold: difficulty = 85.0; break;
                        case CraftResource.Agapite: difficulty = 90.0; break;
                        case CraftResource.Verite: difficulty = 95.0; break;
                        case CraftResource.Valorite: difficulty = 99.0; break;
                    }

                    if (difficulty > from.Skills[SkillName.Mining].Value)
                        return SmeltResult.NoSkill;

                    Type resourceType = info.ResourceTypes[0];
                    Item ingot = (Item)Activator.CreateInstance(resourceType);

                    if (item is DragonBardingDeed || (item is BaseArmor && ((BaseArmor)item).PlayerConstructed) || (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed) || (item is BaseClothing && ((BaseClothing)item).PlayerConstructed))
                        ingot.Amount = craftResource.Amount / 2;
                    else
                        ingot.Amount = 1;

                    item.Delete();
                    from.AddToBackpack(ingot);

                    from.PlaySound(0x2A);
                    from.PlaySound(0x240);
                    return SmeltResult.Success;
                }
                catch
                {
                }

                return SmeltResult.Invalid;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                int num = m_CraftSystem.CanCraft(from, m_Tool, null);

                if (num > 0)
                {
                    if (num == 1044267)
                    {
                        bool anvil, forge;

                        DefBlacksmithy.CheckAnvilAndForge(from, 2, out anvil, out forge);

                        if (!anvil)
                            num = 1044266; // You must be near an anvil
                        else if (!forge)
                            num = 1044265; // You must be near a forge.
                    }

                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, num));
                }
                else
                {
                    SmeltResult result = SmeltResult.Invalid;
                    bool isStoreBought = false;
                    int message;

                    if (targeted is BaseArmor)
                    {
                        result = Resmelt(from, (BaseArmor)targeted, ((BaseArmor)targeted).Resource);
                        isStoreBought = !((BaseArmor)targeted).PlayerConstructed;
                    }
                    else if (targeted is BaseWeapon)
                    {
                        result = Resmelt(from, (BaseWeapon)targeted, ((BaseWeapon)targeted).Resource);
                        isStoreBought = !((BaseWeapon)targeted).PlayerConstructed;
                    }
                    else if (targeted is DragonBardingDeed)
                    {
                        result = Resmelt(from, (DragonBardingDeed)targeted, ((DragonBardingDeed)targeted).Resource);
                        isStoreBought = false;
                    }

                    switch (result)
                    {
                        default:
                        case SmeltResult.Invalid: message = 1044272; break; // You can't melt that down into ingots.
                        case SmeltResult.NoSkill: message = 1044269; break; // You have no idea how to work this metal.
                        case SmeltResult.Success: message = isStoreBought ? 500418 : 1044270; break; // You melt the item down into ingots.
                    }

                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, message));
                }
            }
        }
    }

    public class QueryMakersMarkGump : Gump
    {
        private int m_Quality;
        private Mobile m_From;
        private CraftItem m_CraftItem;
        private CraftSystem m_CraftSystem;
        private Type m_TypeRes;
        private BaseTool m_Tool;

        public QueryMakersMarkGump(int quality, Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, BaseTool tool)
            : base(100, 200)
        {
            from.CloseGump(typeof(QueryMakersMarkGump));

            m_Quality = quality;
            m_From = from;
            m_CraftItem = craftItem;
            m_CraftSystem = craftSystem;
            m_TypeRes = typeRes;
            m_Tool = tool;

            AddPage(0);

            AddBackground(0, 0, 220, 170, 5054);
            AddBackground(10, 10, 200, 150, 3000);

            AddHtmlLocalized(20, 20, 180, 80, 1018317, false, false); // Do you wish to place your maker's mark on this item?

            AddHtmlLocalized(55, 100, 140, 25, 1011011, false, false); // CONTINUE
            AddButton(20, 100, 4005, 4007, 1, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 125, 140, 25, 1011012, false, false); // CANCEL
            AddButton(20, 125, 4005, 4007, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            bool makersMark = (info.ButtonID == 1);

            if (makersMark)
                m_From.SendLocalizedMessage(501808); // You mark the item.
            else
                m_From.SendLocalizedMessage(501809); // Cancelled mark.

            m_CraftItem.CompleteCraft(m_Quality, makersMark, m_From, m_CraftSystem, m_TypeRes, m_Tool, null);
        }
    }

    public enum CraftMarkOption
    {
        MarkItem,
        DoNotMark,
        PromptForMark
    }

    public class CraftContext
    {
        private List<CraftItem> m_Items;
        private int m_LastResourceIndex;
        private int m_LastResourceIndex2;
        private int m_LastGroupIndex;
        private bool m_DoNotColor;
        private CraftMarkOption m_MarkOption;

        public List<CraftItem> Items { get { return m_Items; } }
        public int LastResourceIndex { get { return m_LastResourceIndex; } set { m_LastResourceIndex = value; } }
        public int LastResourceIndex2 { get { return m_LastResourceIndex2; } set { m_LastResourceIndex2 = value; } }
        public int LastGroupIndex { get { return m_LastGroupIndex; } set { m_LastGroupIndex = value; } }
        public bool DoNotColor { get { return m_DoNotColor; } set { m_DoNotColor = value; } }
        public CraftMarkOption MarkOption { get { return m_MarkOption; } set { m_MarkOption = value; } }

        public CraftContext()
        {
            m_Items = new List<CraftItem>();
            m_LastResourceIndex = -1;
            m_LastResourceIndex2 = -1;
            m_LastGroupIndex = -1;
        }

        public CraftItem LastMade
        {
            get
            {
                if (m_Items.Count > 0)
                    return m_Items[0];

                return null;
            }
        }

        public void OnMade(CraftItem item)
        {
            m_Items.Remove(item);

            if (m_Items.Count == 10)
                m_Items.RemoveAt(9);

            m_Items.Insert(0, item);
        }
    }
}

namespace Server.Items
{
    public enum CraftResource
    {
        None = 0,
        Iron = 1,
        DullCopper,
        ShadowIron,
        Copper,
        Bronze,
        Gold,
        Agapite,
        Verite,
        Valorite,

        RegularLeather = 101,
        SpinedLeather,
        HornedLeather,
        BarbedLeather,

        RedScales = 201,
        YellowScales,
        BlackScales,
        GreenScales,
        WhiteScales,
        BlueScales,

        RegularWood = 301,
        OakWood,
        AshWood,
        YewWood,
        Heartwood,
        Bloodwood,
        Frostwood
    }

    public enum CraftResourceType
    {
        None,
        Metal,
        Leather,
        Scales,
        Wood
    }

    public class CraftAttributeInfo
    {
        private int m_WeaponFireDamage;
        private int m_WeaponColdDamage;
        private int m_WeaponPoisonDamage;
        private int m_WeaponEnergyDamage;
        private int m_WeaponChaosDamage;
        private int m_WeaponDirectDamage;
        private int m_WeaponDurability;
        private int m_WeaponLuck;
        private int m_WeaponGoldIncrease;
        private int m_WeaponLowerRequirements;

        private int m_ArmorPhysicalResist;
        private int m_ArmorFireResist;
        private int m_ArmorColdResist;
        private int m_ArmorPoisonResist;
        private int m_ArmorEnergyResist;
        private int m_ArmorDurability;
        private int m_ArmorLuck;
        private int m_ArmorGoldIncrease;
        private int m_ArmorLowerRequirements;

        private int m_RunicMinAttributes;
        private int m_RunicMaxAttributes;
        private int m_RunicMinIntensity;
        private int m_RunicMaxIntensity;

        public int WeaponFireDamage { get { return m_WeaponFireDamage; } set { m_WeaponFireDamage = value; } }
        public int WeaponColdDamage { get { return m_WeaponColdDamage; } set { m_WeaponColdDamage = value; } }
        public int WeaponPoisonDamage { get { return m_WeaponPoisonDamage; } set { m_WeaponPoisonDamage = value; } }
        public int WeaponEnergyDamage { get { return m_WeaponEnergyDamage; } set { m_WeaponEnergyDamage = value; } }
        public int WeaponChaosDamage { get { return m_WeaponChaosDamage; } set { m_WeaponChaosDamage = value; } }
        public int WeaponDirectDamage { get { return m_WeaponDirectDamage; } set { m_WeaponDirectDamage = value; } }
        public int WeaponDurability { get { return m_WeaponDurability; } set { m_WeaponDurability = value; } }
        public int WeaponLuck { get { return m_WeaponLuck; } set { m_WeaponLuck = value; } }
        public int WeaponGoldIncrease { get { return m_WeaponGoldIncrease; } set { m_WeaponGoldIncrease = value; } }
        public int WeaponLowerRequirements { get { return m_WeaponLowerRequirements; } set { m_WeaponLowerRequirements = value; } }

        public int ArmorPhysicalResist { get { return m_ArmorPhysicalResist; } set { m_ArmorPhysicalResist = value; } }
        public int ArmorFireResist { get { return m_ArmorFireResist; } set { m_ArmorFireResist = value; } }
        public int ArmorColdResist { get { return m_ArmorColdResist; } set { m_ArmorColdResist = value; } }
        public int ArmorPoisonResist { get { return m_ArmorPoisonResist; } set { m_ArmorPoisonResist = value; } }
        public int ArmorEnergyResist { get { return m_ArmorEnergyResist; } set { m_ArmorEnergyResist = value; } }
        public int ArmorDurability { get { return m_ArmorDurability; } set { m_ArmorDurability = value; } }
        public int ArmorLuck { get { return m_ArmorLuck; } set { m_ArmorLuck = value; } }
        public int ArmorGoldIncrease { get { return m_ArmorGoldIncrease; } set { m_ArmorGoldIncrease = value; } }
        public int ArmorLowerRequirements { get { return m_ArmorLowerRequirements; } set { m_ArmorLowerRequirements = value; } }

        public int RunicMinAttributes { get { return m_RunicMinAttributes; } set { m_RunicMinAttributes = value; } }
        public int RunicMaxAttributes { get { return m_RunicMaxAttributes; } set { m_RunicMaxAttributes = value; } }
        public int RunicMinIntensity { get { return m_RunicMinIntensity; } set { m_RunicMinIntensity = value; } }
        public int RunicMaxIntensity { get { return m_RunicMaxIntensity; } set { m_RunicMaxIntensity = value; } }

        public CraftAttributeInfo()
        {
        }

        public static readonly CraftAttributeInfo Blank;
        public static readonly CraftAttributeInfo DullCopper, ShadowIron, Copper, Bronze, Golden, Agapite, Verite, Valorite;
        public static readonly CraftAttributeInfo Spined, Horned, Barbed;
        public static readonly CraftAttributeInfo RedScales, YellowScales, BlackScales, GreenScales, WhiteScales, BlueScales;
        public static readonly CraftAttributeInfo OakWood, AshWood, YewWood, Heartwood, Bloodwood, Frostwood;

        static CraftAttributeInfo()
        {
            Blank = new CraftAttributeInfo();

            CraftAttributeInfo dullCopper = DullCopper = new CraftAttributeInfo();

            dullCopper.ArmorPhysicalResist = 6;
            dullCopper.ArmorDurability = 50;
            dullCopper.ArmorLowerRequirements = 20;
            dullCopper.WeaponDurability = 100;
            dullCopper.WeaponLowerRequirements = 50;
            dullCopper.RunicMinAttributes = 1;
            dullCopper.RunicMaxAttributes = 2;
            if (Core.ML)
            {
                dullCopper.RunicMinIntensity = 40;
                dullCopper.RunicMaxIntensity = 100;
            }
            else
            {
                dullCopper.RunicMinIntensity = 10;
                dullCopper.RunicMaxIntensity = 35;
            }

            CraftAttributeInfo shadowIron = ShadowIron = new CraftAttributeInfo();

            shadowIron.ArmorPhysicalResist = 2;
            shadowIron.ArmorFireResist = 1;
            shadowIron.ArmorEnergyResist = 5;
            shadowIron.ArmorDurability = 100;
            shadowIron.WeaponColdDamage = 20;
            shadowIron.WeaponDurability = 50;
            shadowIron.RunicMinAttributes = 2;
            shadowIron.RunicMaxAttributes = 2;
            if (Core.ML)
            {
                shadowIron.RunicMinIntensity = 45;
                shadowIron.RunicMaxIntensity = 100;
            }
            else
            {
                shadowIron.RunicMinIntensity = 20;
                shadowIron.RunicMaxIntensity = 45;
            }

            CraftAttributeInfo copper = Copper = new CraftAttributeInfo();

            copper.ArmorPhysicalResist = 1;
            copper.ArmorFireResist = 1;
            copper.ArmorPoisonResist = 5;
            copper.ArmorEnergyResist = 2;
            copper.WeaponPoisonDamage = 10;
            copper.WeaponEnergyDamage = 20;
            copper.RunicMinAttributes = 2;
            copper.RunicMaxAttributes = 3;
            if (Core.ML)
            {
                copper.RunicMinIntensity = 50;
                copper.RunicMaxIntensity = 100;
            }
            else
            {
                copper.RunicMinIntensity = 25;
                copper.RunicMaxIntensity = 50;
            }

            CraftAttributeInfo bronze = Bronze = new CraftAttributeInfo();

            bronze.ArmorPhysicalResist = 3;
            bronze.ArmorColdResist = 5;
            bronze.ArmorPoisonResist = 1;
            bronze.ArmorEnergyResist = 1;
            bronze.WeaponFireDamage = 40;
            bronze.RunicMinAttributes = 3;
            bronze.RunicMaxAttributes = 3;
            if (Core.ML)
            {
                bronze.RunicMinIntensity = 55;
                bronze.RunicMaxIntensity = 100;
            }
            else
            {
                bronze.RunicMinIntensity = 30;
                bronze.RunicMaxIntensity = 65;
            }

            CraftAttributeInfo golden = Golden = new CraftAttributeInfo();

            golden.ArmorPhysicalResist = 1;
            golden.ArmorFireResist = 1;
            golden.ArmorColdResist = 2;
            golden.ArmorEnergyResist = 2;
            golden.ArmorLuck = 40;
            golden.ArmorLowerRequirements = 30;
            golden.WeaponLuck = 40;
            golden.WeaponLowerRequirements = 50;
            golden.RunicMinAttributes = 3;
            golden.RunicMaxAttributes = 4;
            if (Core.ML)
            {
                golden.RunicMinIntensity = 60;
                golden.RunicMaxIntensity = 100;
            }
            else
            {
                golden.RunicMinIntensity = 35;
                golden.RunicMaxIntensity = 75;
            }

            CraftAttributeInfo agapite = Agapite = new CraftAttributeInfo();

            agapite.ArmorPhysicalResist = 2;
            agapite.ArmorFireResist = 3;
            agapite.ArmorColdResist = 2;
            agapite.ArmorPoisonResist = 2;
            agapite.ArmorEnergyResist = 2;
            agapite.WeaponColdDamage = 30;
            agapite.WeaponEnergyDamage = 20;
            agapite.RunicMinAttributes = 4;
            agapite.RunicMaxAttributes = 4;
            if (Core.ML)
            {
                agapite.RunicMinIntensity = 65;
                agapite.RunicMaxIntensity = 100;
            }
            else
            {
                agapite.RunicMinIntensity = 40;
                agapite.RunicMaxIntensity = 80;
            }

            CraftAttributeInfo verite = Verite = new CraftAttributeInfo();

            verite.ArmorPhysicalResist = 3;
            verite.ArmorFireResist = 3;
            verite.ArmorColdResist = 2;
            verite.ArmorPoisonResist = 3;
            verite.ArmorEnergyResist = 1;
            verite.WeaponPoisonDamage = 40;
            verite.WeaponEnergyDamage = 20;
            verite.RunicMinAttributes = 4;
            verite.RunicMaxAttributes = 5;
            if (Core.ML)
            {
                verite.RunicMinIntensity = 70;
                verite.RunicMaxIntensity = 100;
            }
            else
            {
                verite.RunicMinIntensity = 45;
                verite.RunicMaxIntensity = 90;
            }

            CraftAttributeInfo valorite = Valorite = new CraftAttributeInfo();

            valorite.ArmorPhysicalResist = 4;
            valorite.ArmorColdResist = 3;
            valorite.ArmorPoisonResist = 3;
            valorite.ArmorEnergyResist = 3;
            valorite.ArmorDurability = 50;
            valorite.WeaponFireDamage = 10;
            valorite.WeaponColdDamage = 20;
            valorite.WeaponPoisonDamage = 10;
            valorite.WeaponEnergyDamage = 20;
            valorite.RunicMinAttributes = 5;
            valorite.RunicMaxAttributes = 5;
            if (Core.ML)
            {
                valorite.RunicMinIntensity = 85;
                valorite.RunicMaxIntensity = 100;
            }
            else
            {
                valorite.RunicMinIntensity = 50;
                valorite.RunicMaxIntensity = 100;
            }

            CraftAttributeInfo spined = Spined = new CraftAttributeInfo();

            spined.ArmorPhysicalResist = 5;
            spined.ArmorLuck = 40;
            spined.RunicMinAttributes = 1;
            spined.RunicMaxAttributes = 3;
            if (Core.ML)
            {
                spined.RunicMinIntensity = 40;
                spined.RunicMaxIntensity = 100;
            }
            else
            {
                spined.RunicMinIntensity = 20;
                spined.RunicMaxIntensity = 40;
            }

            CraftAttributeInfo horned = Horned = new CraftAttributeInfo();

            horned.ArmorPhysicalResist = 2;
            horned.ArmorFireResist = 3;
            horned.ArmorColdResist = 2;
            horned.ArmorPoisonResist = 2;
            horned.ArmorEnergyResist = 2;
            horned.RunicMinAttributes = 3;
            horned.RunicMaxAttributes = 4;
            if (Core.ML)
            {
                horned.RunicMinIntensity = 45;
                horned.RunicMaxIntensity = 100;
            }
            else
            {
                horned.RunicMinIntensity = 30;
                horned.RunicMaxIntensity = 70;
            }

            CraftAttributeInfo barbed = Barbed = new CraftAttributeInfo();

            barbed.ArmorPhysicalResist = 2;
            barbed.ArmorFireResist = 1;
            barbed.ArmorColdResist = 2;
            barbed.ArmorPoisonResist = 3;
            barbed.ArmorEnergyResist = 4;
            barbed.RunicMinAttributes = 4;
            barbed.RunicMaxAttributes = 5;
            if (Core.ML)
            {
                barbed.RunicMinIntensity = 50;
                barbed.RunicMaxIntensity = 100;
            }
            else
            {
                barbed.RunicMinIntensity = 40;
                barbed.RunicMaxIntensity = 100;
            }

            CraftAttributeInfo red = RedScales = new CraftAttributeInfo();

            red.ArmorFireResist = 10;
            red.ArmorColdResist = -3;

            CraftAttributeInfo yellow = YellowScales = new CraftAttributeInfo();

            yellow.ArmorPhysicalResist = -3;
            yellow.ArmorLuck = 20;

            CraftAttributeInfo black = BlackScales = new CraftAttributeInfo();

            black.ArmorPhysicalResist = 10;
            black.ArmorEnergyResist = -3;

            CraftAttributeInfo green = GreenScales = new CraftAttributeInfo();

            green.ArmorFireResist = -3;
            green.ArmorPoisonResist = 10;

            CraftAttributeInfo white = WhiteScales = new CraftAttributeInfo();

            white.ArmorPhysicalResist = -3;
            white.ArmorColdResist = 10;

            CraftAttributeInfo blue = BlueScales = new CraftAttributeInfo();

            blue.ArmorPoisonResist = -3;
            blue.ArmorEnergyResist = 10;

            //public static readonly CraftAttributeInfo OakWood, AshWood, YewWood, Heartwood, Bloodwood, Frostwood;

            CraftAttributeInfo oak = OakWood = new CraftAttributeInfo();

            CraftAttributeInfo ash = AshWood = new CraftAttributeInfo();

            CraftAttributeInfo yew = YewWood = new CraftAttributeInfo();

            CraftAttributeInfo heart = Heartwood = new CraftAttributeInfo();

            CraftAttributeInfo blood = Bloodwood = new CraftAttributeInfo();

            CraftAttributeInfo frost = Frostwood = new CraftAttributeInfo();
        }
    }

    public class CraftResourceInfo
    {
        private int m_Hue;
        private int m_Number;
        private string m_Name;
        private CraftAttributeInfo m_AttributeInfo;
        private CraftResource m_Resource;
        private Type[] m_ResourceTypes;

        public int Hue { get { return m_Hue; } }
        public int Number { get { return m_Number; } }
        public string Name { get { return m_Name; } }
        public CraftAttributeInfo AttributeInfo { get { return m_AttributeInfo; } }
        public CraftResource Resource { get { return m_Resource; } }
        public Type[] ResourceTypes { get { return m_ResourceTypes; } }

        public CraftResourceInfo(int hue, int number, string name, CraftAttributeInfo attributeInfo, CraftResource resource, params Type[] resourceTypes)
        {
            m_Hue = hue;
            m_Number = number;
            m_Name = name;
            m_AttributeInfo = attributeInfo;
            m_Resource = resource;
            m_ResourceTypes = resourceTypes;

            for (int i = 0; i < resourceTypes.Length; ++i)
                CraftResources.RegisterType(resourceTypes[i], resource);
        }
    }

    public class CraftResources
    {
        private static CraftResourceInfo[] m_MetalInfo = new CraftResourceInfo[]
            {
                new CraftResourceInfo( 0x000, 1053109, "Iron",          CraftAttributeInfo.Blank,       CraftResource.Iron,             typeof( IronIngot ),        typeof( IronOre ),          typeof( Granite ) ),
                new CraftResourceInfo( 0x973, 1053108, "Dull Copper",   CraftAttributeInfo.DullCopper,  CraftResource.DullCopper,       typeof( DullCopperIngot ),  typeof( DullCopperOre ),    typeof( DullCopperGranite ) ),
                new CraftResourceInfo( 0x966, 1053107, "Shadow Iron",   CraftAttributeInfo.ShadowIron,  CraftResource.ShadowIron,       typeof( ShadowIronIngot ),  typeof( ShadowIronOre ),    typeof( ShadowIronGranite ) ),
                new CraftResourceInfo( 0x96D, 1053106, "Copper",        CraftAttributeInfo.Copper,      CraftResource.Copper,           typeof( CopperIngot ),      typeof( CopperOre ),        typeof( CopperGranite ) ),
                new CraftResourceInfo( 0x972, 1053105, "Bronze",        CraftAttributeInfo.Bronze,      CraftResource.Bronze,           typeof( BronzeIngot ),      typeof( BronzeOre ),        typeof( BronzeGranite ) ),
                new CraftResourceInfo( 0x8A5, 1053104, "Gold",          CraftAttributeInfo.Golden,      CraftResource.Gold,             typeof( GoldIngot ),        typeof( GoldOre ),          typeof( GoldGranite ) ),
                new CraftResourceInfo( 0x979, 1053103, "Agapite",       CraftAttributeInfo.Agapite,     CraftResource.Agapite,          typeof( AgapiteIngot ),     typeof( AgapiteOre ),       typeof( AgapiteGranite ) ),
                new CraftResourceInfo( 0x89F, 1053102, "Verite",        CraftAttributeInfo.Verite,      CraftResource.Verite,           typeof( VeriteIngot ),      typeof( VeriteOre ),        typeof( VeriteGranite ) ),
                new CraftResourceInfo( 0x8AB, 1053101, "Valorite",      CraftAttributeInfo.Valorite,    CraftResource.Valorite,         typeof( ValoriteIngot ),    typeof( ValoriteOre ),      typeof( ValoriteGranite ) ),
            };

        private static CraftResourceInfo[] m_ScaleInfo = new CraftResourceInfo[]
            {
                new CraftResourceInfo( 0x66D, 1053129, "Red Scales",    CraftAttributeInfo.RedScales,       CraftResource.RedScales,        typeof( RedScales ) ),
                new CraftResourceInfo( 0x8A8, 1053130, "Yellow Scales", CraftAttributeInfo.YellowScales,    CraftResource.YellowScales,     typeof( YellowScales ) ),
                new CraftResourceInfo( 0x455, 1053131, "Black Scales",  CraftAttributeInfo.BlackScales,     CraftResource.BlackScales,      typeof( BlackScales ) ),
                new CraftResourceInfo( 0x851, 1053132, "Green Scales",  CraftAttributeInfo.GreenScales,     CraftResource.GreenScales,      typeof( GreenScales ) ),
                new CraftResourceInfo( 0x8FD, 1053133, "White Scales",  CraftAttributeInfo.WhiteScales,     CraftResource.WhiteScales,      typeof( WhiteScales ) ),
                new CraftResourceInfo( 0x8B0, 1053134, "Blue Scales",   CraftAttributeInfo.BlueScales,      CraftResource.BlueScales,       typeof( BlueScales ) )
            };

        private static CraftResourceInfo[] m_LeatherInfo = new CraftResourceInfo[]
            {
                new CraftResourceInfo( 0x000, 1049353, "Normal",        CraftAttributeInfo.Blank,       CraftResource.RegularLeather,   typeof( Leather ),          typeof( Hides ) ),
                new CraftResourceInfo( 0x283, 1049354, "Spined",        CraftAttributeInfo.Spined,      CraftResource.SpinedLeather,    typeof( SpinedLeather ),    typeof( SpinedHides ) ),
                new CraftResourceInfo( 0x227, 1049355, "Horned",        CraftAttributeInfo.Horned,      CraftResource.HornedLeather,    typeof( HornedLeather ),    typeof( HornedHides ) ),
                new CraftResourceInfo( 0x1C1, 1049356, "Barbed",        CraftAttributeInfo.Barbed,      CraftResource.BarbedLeather,    typeof( BarbedLeather ),    typeof( BarbedHides ) )
            };

        private static CraftResourceInfo[] m_AOSLeatherInfo = new CraftResourceInfo[]
            {
                new CraftResourceInfo( 0x000, 1049353, "Normal",        CraftAttributeInfo.Blank,       CraftResource.RegularLeather,   typeof( Leather ),          typeof( Hides ) ),
                new CraftResourceInfo( 0x8AC, 1049354, "Spined",        CraftAttributeInfo.Spined,      CraftResource.SpinedLeather,    typeof( SpinedLeather ),    typeof( SpinedHides ) ),
                new CraftResourceInfo( 0x845, 1049355, "Horned",        CraftAttributeInfo.Horned,      CraftResource.HornedLeather,    typeof( HornedLeather ),    typeof( HornedHides ) ),
                new CraftResourceInfo( 0x851, 1049356, "Barbed",        CraftAttributeInfo.Barbed,      CraftResource.BarbedLeather,    typeof( BarbedLeather ),    typeof( BarbedHides ) ),
            };

        private static CraftResourceInfo[] m_WoodInfo = new CraftResourceInfo[]
            {
                new CraftResourceInfo( 0x000, 1011542, "Normal",        CraftAttributeInfo.Blank,       CraftResource.RegularWood,  typeof( Log ),          typeof( Board ) ),
                new CraftResourceInfo( 0x7DA, 1072533, "Oak",           CraftAttributeInfo.OakWood,     CraftResource.OakWood,      typeof( OakLog ),       typeof( OakBoard ) ),
                new CraftResourceInfo( 0x4A7, 1072534, "Ash",           CraftAttributeInfo.AshWood,     CraftResource.AshWood,      typeof( AshLog ),       typeof( AshBoard ) ),
                new CraftResourceInfo( 0x4A8, 1072535, "Yew",           CraftAttributeInfo.YewWood,     CraftResource.YewWood,      typeof( YewLog ),       typeof( YewBoard ) ),
                new CraftResourceInfo( 0x4A9, 1072536, "Heartwood",     CraftAttributeInfo.Heartwood,   CraftResource.Heartwood,    typeof( HeartwoodLog ), typeof( HeartwoodBoard ) ),
                new CraftResourceInfo( 0x4AA, 1072538, "Bloodwood",     CraftAttributeInfo.Bloodwood,   CraftResource.Bloodwood,    typeof( BloodwoodLog ), typeof( BloodwoodBoard ) ),
                new CraftResourceInfo( 0x47F, 1072539, "Frostwood",     CraftAttributeInfo.Frostwood,   CraftResource.Frostwood,    typeof( FrostwoodLog ), typeof( FrostwoodBoard ) )
            };

        /// <summary>
        /// Returns true if '<paramref name="resource"/>' is None, Iron, RegularLeather or RegularWood. False if otherwise.
        /// </summary>
        public static bool IsStandard(CraftResource resource)
        {
            return (resource == CraftResource.None || resource == CraftResource.Iron || resource == CraftResource.RegularLeather || resource == CraftResource.RegularWood);
        }

        private static Dictionary<Type, CraftResource> m_TypeTable;

        /// <summary>
        /// Registers that '<paramref name="resourceType"/>' uses '<paramref name="resource"/>' so that it can later be queried by <see cref="CraftResources.GetFromType"/>
        /// </summary>
        public static void RegisterType(Type resourceType, CraftResource resource)
        {
            if (m_TypeTable == null)
                m_TypeTable = new Dictionary<Type, CraftResource>();

            m_TypeTable[resourceType] = resource;
        }

        /// <summary>
        /// Returns the <see cref="CraftResource"/> value for which '<paramref name="resourceType"/>' uses -or- CraftResource.None if an unregistered type was specified.
        /// </summary>
        public static CraftResource GetFromType(Type resourceType)
        {
            if (m_TypeTable == null)
                return CraftResource.None;

            CraftResource res;

            if (!m_TypeTable.TryGetValue(resourceType, out res))
                return CraftResource.None;

            return res;
        }

        /// <summary>
        /// Returns a <see cref="CraftResourceInfo"/> instance describing '<paramref name="resource"/>' -or- null if an invalid resource was specified.
        /// </summary>
        public static CraftResourceInfo GetInfo(CraftResource resource)
        {
            CraftResourceInfo[] list = null;

            switch (GetType(resource))
            {
                case CraftResourceType.Metal: list = m_MetalInfo; break;
                case CraftResourceType.Leather: list = Core.AOS ? m_AOSLeatherInfo : m_LeatherInfo; break;
                case CraftResourceType.Scales: list = m_ScaleInfo; break;
                case CraftResourceType.Wood: list = m_WoodInfo; break;
            }

            if (list != null)
            {
                int index = GetIndex(resource);

                if (index >= 0 && index < list.Length)
                    return list[index];
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="CraftResourceType"/> value indiciating the type of '<paramref name="resource"/>'.
        /// </summary>
        public static CraftResourceType GetType(CraftResource resource)
        {
            if (resource >= CraftResource.Iron && resource <= CraftResource.Valorite)
                return CraftResourceType.Metal;

            if (resource >= CraftResource.RegularLeather && resource <= CraftResource.BarbedLeather)
                return CraftResourceType.Leather;

            if (resource >= CraftResource.RedScales && resource <= CraftResource.BlueScales)
                return CraftResourceType.Scales;

            if (resource >= CraftResource.RegularWood && resource <= CraftResource.Frostwood)
                return CraftResourceType.Wood;

            return CraftResourceType.None;
        }

        /// <summary>
        /// Returns the first <see cref="CraftResource"/> in the series of resources for which '<paramref name="resource"/>' belongs.
        /// </summary>
        public static CraftResource GetStart(CraftResource resource)
        {
            switch (GetType(resource))
            {
                case CraftResourceType.Metal: return CraftResource.Iron;
                case CraftResourceType.Leather: return CraftResource.RegularLeather;
                case CraftResourceType.Scales: return CraftResource.RedScales;
                case CraftResourceType.Wood: return CraftResource.RegularWood;
            }

            return CraftResource.None;
        }

        /// <summary>
        /// Returns the index of '<paramref name="resource"/>' in the seriest of resources for which it belongs.
        /// </summary>
        public static int GetIndex(CraftResource resource)
        {
            CraftResource start = GetStart(resource);

            if (start == CraftResource.None)
                return 0;

            return (int)(resource - start);
        }

        /// <summary>
        /// Returns the <see cref="CraftResourceInfo.Number"/> property of '<paramref name="resource"/>' -or- 0 if an invalid resource was specified.
        /// </summary>
        public static int GetLocalizationNumber(CraftResource resource)
        {
            CraftResourceInfo info = GetInfo(resource);

            return (info == null ? 0 : info.Number);
        }

        /// <summary>
        /// Returns the <see cref="CraftResourceInfo.Hue"/> property of '<paramref name="resource"/>' -or- 0 if an invalid resource was specified.
        /// </summary>
        public static int GetHue(CraftResource resource)
        {
            CraftResourceInfo info = GetInfo(resource);

            return (info == null ? 0 : info.Hue);
        }

        /// <summary>
        /// Returns the <see cref="CraftResourceInfo.Name"/> property of '<paramref name="resource"/>' -or- an empty string if the resource specified was invalid.
        /// </summary>
        public static string GetName(CraftResource resource)
        {
            CraftResourceInfo info = GetInfo(resource);

            return (info == null ? String.Empty : info.Name);
        }

        /// <summary>
        /// Returns the <see cref="CraftResource"/> value which represents '<paramref name="info"/>' -or- CraftResource.None if unable to convert.
        /// </summary>
        public static CraftResource GetFromOreInfo(OreInfo info)
        {
            if (info.Name.IndexOf("Spined") >= 0)
                return CraftResource.SpinedLeather;
            else if (info.Name.IndexOf("Horned") >= 0)
                return CraftResource.HornedLeather;
            else if (info.Name.IndexOf("Barbed") >= 0)
                return CraftResource.BarbedLeather;
            else if (info.Name.IndexOf("Leather") >= 0)
                return CraftResource.RegularLeather;

            if (info.Level == 0)
                return CraftResource.Iron;
            else if (info.Level == 1)
                return CraftResource.DullCopper;
            else if (info.Level == 2)
                return CraftResource.ShadowIron;
            else if (info.Level == 3)
                return CraftResource.Copper;
            else if (info.Level == 4)
                return CraftResource.Bronze;
            else if (info.Level == 5)
                return CraftResource.Gold;
            else if (info.Level == 6)
                return CraftResource.Agapite;
            else if (info.Level == 7)
                return CraftResource.Verite;
            else if (info.Level == 8)
                return CraftResource.Valorite;

            return CraftResource.None;
        }

        /// <summary>
        /// Returns the <see cref="CraftResource"/> value which represents '<paramref name="info"/>', using '<paramref name="material"/>' to help resolve leather OreInfo instances.
        /// </summary>
        public static CraftResource GetFromOreInfo(OreInfo info, ArmorMaterialType material)
        {
            if (material == ArmorMaterialType.Studded || material == ArmorMaterialType.Leather || material == ArmorMaterialType.Spined ||
                material == ArmorMaterialType.Horned || material == ArmorMaterialType.Barbed)
            {
                if (info.Level == 0)
                    return CraftResource.RegularLeather;
                else if (info.Level == 1)
                    return CraftResource.SpinedLeather;
                else if (info.Level == 2)
                    return CraftResource.HornedLeather;
                else if (info.Level == 3)
                    return CraftResource.BarbedLeather;

                return CraftResource.None;
            }

            return GetFromOreInfo(info);
        }
    }

    // NOTE: This class is only for compatability with very old RunUO versions.
    // No changes to it should be required for custom resources.
    public class OreInfo
    {
        public static readonly OreInfo Iron = new OreInfo(0, 0x000, "Iron");
        public static readonly OreInfo DullCopper = new OreInfo(1, 0x973, "Dull Copper");
        public static readonly OreInfo ShadowIron = new OreInfo(2, 0x966, "Shadow Iron");
        public static readonly OreInfo Copper = new OreInfo(3, 0x96D, "Copper");
        public static readonly OreInfo Bronze = new OreInfo(4, 0x972, "Bronze");
        public static readonly OreInfo Gold = new OreInfo(5, 0x8A5, "Gold");
        public static readonly OreInfo Agapite = new OreInfo(6, 0x979, "Agapite");
        public static readonly OreInfo Verite = new OreInfo(7, 0x89F, "Verite");
        public static readonly OreInfo Valorite = new OreInfo(8, 0x8AB, "Valorite");

        private int m_Level;
        private int m_Hue;
        private string m_Name;

        public OreInfo(int level, int hue, string name)
        {
            m_Level = level;
            m_Hue = hue;
            m_Name = name;
        }

        public int Level
        {
            get
            {
                return m_Level;
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }
    }
}