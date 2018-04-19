using System;
using System.Collections;

using Server;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Plants
{
    /// <summary>
    /// Planting Resources
    /// </summary>
    public class PlantResourceInfo
    {
        private static PlantResourceInfo[] m_ResourceList = new PlantResourceInfo[]
			{
				new PlantResourceInfo( PlantType.ElephantEarPlant, PlantHue.BrightRed, typeof( RedLeaves ) ),
				new PlantResourceInfo( PlantType.PonytailPalm, PlantHue.BrightRed, typeof( RedLeaves ) ),
				new PlantResourceInfo( PlantType.CenturyPlant, PlantHue.BrightRed, typeof( RedLeaves ) ),
				new PlantResourceInfo( PlantType.Poppies, PlantHue.BrightOrange, typeof( OrangePetals ) ),
				new PlantResourceInfo( PlantType.Bulrushes, PlantHue.BrightOrange, typeof( OrangePetals ) ),
				new PlantResourceInfo( PlantType.PampasGrass, PlantHue.BrightOrange, typeof( OrangePetals ) ),
				new PlantResourceInfo( PlantType.SnakePlant, PlantHue.BrightGreen, typeof( GreenThorns ) ),
				new PlantResourceInfo( PlantType.BarrelCactus, PlantHue.BrightGreen, typeof( GreenThorns ) ),
				new PlantResourceInfo( PlantType.CocoaTree, PlantHue.Plain, typeof( CocoaPulp ) )
			};

        public static PlantResourceInfo GetInfo(PlantType plantType, PlantHue plantHue)
        {
            foreach (PlantResourceInfo info in m_ResourceList)
            {
                if (info.PlantType == plantType && info.PlantHue == plantHue)
                    return info;
            }

            return null;
        }

        private PlantType m_PlantType;
        private PlantHue m_PlantHue;
        private Type m_ResourceType;

        public PlantType PlantType { get { return m_PlantType; } }
        public PlantHue PlantHue { get { return m_PlantHue; } }
        public Type ResourceType { get { return m_ResourceType; } }

        private PlantResourceInfo(PlantType plantType, PlantHue plantHue, Type resourceType)
        {
            m_PlantType = plantType;
            m_PlantHue = plantHue;
            m_ResourceType = resourceType;
        }

        public Item CreateResource()
        {
            return (Item)Activator.CreateInstance(m_ResourceType);
        }
    }
}

namespace Server.Items
{
    #region CustomHuePicker

    public class CustomHueGroup
    {
        private int m_Name;
        private string m_NameString;
        private int[] m_Hues;

        public int Name { get { return m_Name; } }
        public string NameString { get { return m_NameString; } }

        public int[] Hues { get { return m_Hues; } }

        public CustomHueGroup(int name, int[] hues)
        {
            m_Name = name;
            m_Hues = hues;
        }

        public CustomHueGroup(string name, int[] hues)
        {
            m_NameString = name;
            m_Hues = hues;
        }
    }

    public class CustomHuePicker
    {
        private CustomHueGroup[] m_Groups;
        private bool m_DefaultSupported;
        private int m_Title;
        private string m_TitleString;

        public bool DefaultSupported { get { return m_DefaultSupported; } }
        public CustomHueGroup[] Groups { get { return m_Groups; } }
        public int Title { get { return m_Title; } }
        public string TitleString { get { return m_TitleString; } }

        public CustomHuePicker(CustomHueGroup[] groups, bool defaultSupported)
        {
            m_Groups = groups;
            m_DefaultSupported = defaultSupported;
        }

        public CustomHuePicker(CustomHueGroup[] groups, bool defaultSupported, int title)
        {
            m_Groups = groups;
            m_DefaultSupported = defaultSupported;
            m_Title = title;
        }

        public CustomHuePicker(CustomHueGroup[] groups, bool defaultSupported, string title)
        {
            m_Groups = groups;
            m_DefaultSupported = defaultSupported;
            m_TitleString = title;
        }

        public static readonly CustomHuePicker SpecialDyeTub = new CustomHuePicker(new CustomHueGroup[]
			{
				/* Violet */
				new CustomHueGroup( 1018345, new int[]{ 1230, 1231, 1232, 1233, 1234, 1235 } ),
				/* Tan */
				new CustomHueGroup( 1018346, new int[]{ 1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508 } ),
				/* Brown */
				new CustomHueGroup( 1018347, new int[]{ 2012, 2013, 2014, 2015, 2016, 2017 } ),
				/* Dark Blue */
				new CustomHueGroup( 1018348, new int[]{ 1303, 1304, 1305, 1306, 1307, 1308 } ),
				/* Forest Green */
				new CustomHueGroup( 1018349, new int[]{ 1420, 1421, 1422, 1423, 1424, 1425, 1426 } ),
				/* Pink */
				new CustomHueGroup( 1018350, new int[]{ 1619, 1620, 1621, 1622, 1623, 1624, 1625, 1626 } ),
				/* Red */
				new CustomHueGroup( 1018351, new int[]{ 1640, 1641, 1642, 1643, 1644 } ),
				/* Olive */
				new CustomHueGroup( 1018352, new int[]{ 2001, 2002, 2003, 2004, 2005 } )
			}, false, 1018344);

        public static readonly CustomHuePicker LeatherDyeTub = new CustomHuePicker(new CustomHueGroup[]
			{
				/* Dull Copper */
				new CustomHueGroup( 1018332, new int[]{ 2419, 2420, 2421, 2422, 2423, 2424 } ),
				/* Shadow Iron */
				new CustomHueGroup( 1018333, new int[]{ 2406, 2407, 2408, 2409, 2410, 2411, 2412 } ),
				/* Copper */
				new CustomHueGroup( 1018334, new int[]{ 2413, 2414, 2415, 2416, 2417, 2418 } ),
				/* Bronze */
				new CustomHueGroup( 1018335, new int[]{ 2414, 2415, 2416, 2417, 2418 } ),
				/* Glden */
				new CustomHueGroup( 1018336, new int[]{ 2213, 2214, 2215, 2216, 2217, 2218 } ),
				/* Agapite */
				new CustomHueGroup( 1018337, new int[]{ 2425, 2426, 2427, 2428, 2429, 2430 } ),
				/* Verite */
				new CustomHueGroup( 1018338, new int[]{ 2207, 2208, 2209, 2210, 2211, 2212 } ),
				/* Valorite */
				new CustomHueGroup( 1018339, new int[]{ 2219, 2220, 2221, 2222, 2223, 2224 } ),
				/* Reds */
				new CustomHueGroup( 1018340, new int[]{ 2113, 2114, 2115, 2116, 2117, 2118 } ),
				/* Blues */
				new CustomHueGroup( 1018341, new int[]{ 2119, 2120, 2121, 2122, 2123, 2124 } ),
				/* Greens */
				new CustomHueGroup( 1018342, new int[]{ 2126, 2127, 2128, 2129, 2130 } ),
				/* Yellows */
				new CustomHueGroup( 1018343, new int[]{ 2213, 2214, 2215, 2216, 2217, 2218 } )
			}, true);
    }

    public delegate void CustomHuePickerCallback(Mobile from, object state, int hue);

    public class CustomHuePickerGump : Gump
    {
        private Mobile m_From;
        private CustomHuePicker m_Definition;
        private CustomHuePickerCallback m_Callback;
        private object m_State;

        private int GetRadioID(int group, int index)
        {
            return (index * m_Definition.Groups.Length) + group;
        }

        private void RenderBackground()
        {
            AddPage(0);

            AddBackground(0, 0, 450, 450, 5054);
            AddBackground(10, 10, 430, 430, 3000);

            if (m_Definition.TitleString != null)
                AddHtml(20, 30, 400, 25, m_Definition.TitleString, false, false);
            else if (m_Definition.Title > 0)
                AddHtmlLocalized(20, 30, 400, 25, m_Definition.Title, false, false);

            AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(55, 400, 200, 25, 1011036, false, false); // OKAY

            if (m_Definition.DefaultSupported)
            {
                AddButton(200, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddLabel(235, 400, 0, "DEFAULT");
            }
        }

        private void RenderCategories()
        {
            CustomHueGroup[] groups = m_Definition.Groups;

            for (int i = 0; i < groups.Length; ++i)
            {
                AddButton(30, 85 + (i * 25), 5224, 5224, 0, GumpButtonType.Page, 1 + i);

                if (groups[i].NameString != null)
                    AddHtml(55, 85 + (i * 25), 200, 25, groups[i].NameString, false, false);
                else
                    AddHtmlLocalized(55, 85 + (i * 25), 200, 25, groups[i].Name, false, false);
            }

            for (int i = 0; i < groups.Length; ++i)
            {
                AddPage(1 + i);

                int[] hues = groups[i].Hues;

                for (int j = 0; j < hues.Length; ++j)
                {
                    AddRadio(260, 90 + (j * 25), 210, 211, false, GetRadioID(i, j));
                    AddLabel(278, 90 + (j * 25), hues[j] - 1, "*****");
                }
            }
        }

        public CustomHuePickerGump(Mobile from, CustomHuePicker definition, CustomHuePickerCallback callback, object state)
            : base(50, 50)
        {
            m_From = from;
            m_Definition = definition;
            m_Callback = callback;
            m_State = state;

            RenderBackground();
            RenderCategories();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1: // Okay
                    {
                        int[] switches = info.Switches;

                        if (switches.Length > 0)
                        {
                            int index = switches[0];

                            int group = index % m_Definition.Groups.Length;
                            index /= m_Definition.Groups.Length;

                            if (group >= 0 && group < m_Definition.Groups.Length)
                            {
                                int[] hues = m_Definition.Groups[group].Hues;

                                if (index >= 0 && index < hues.Length)
                                    m_Callback(m_From, m_State, hues[index]);
                            }
                        }

                        break;
                    }
                case 2: // Default
                    {
                        if (m_Definition.DefaultSupported)
                            m_Callback(m_From, m_State, 0);

                        break;
                    }
            }
        }
    }

    #endregion

    public class MetallicHuePicker : Gump
    {
        public delegate void MetallicHuePickerCallback(Mobile from, object state, int hue);

        private Mobile m_From;
        private MetallicHuePickerCallback m_Callback;
        private object m_State;

        public void Render()
        {
            AddPage(0);

            AddBackground(0, 0, 450, 450, 0x13BE);
            AddBackground(10, 10, 430, 430, 0xBB8);

            AddHtmlLocalized(55, 400, 200, 25, 1011036, false, false); // OKAY

            AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddButton(200, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(235, 400, 0, "DEFAULT");

            AddHtmlLocalized(55, 25, 200, 25, 1150063, false, false); // Base/Shadow Color
            AddHtmlLocalized(260, 25, 200, 25, 1150064, false, false); // Highlight Color

            for (int row = 0; row < 13; row++)
            {
                AddButton(30, (65 + (row * 25)), 0x1467, 0x1468, row + 1, GumpButtonType.Page, row + 1);
                AddItem(50, (65 + (row * 25)), 0x1412, 2501 + (row * 12) + ((row == 12) ? 6 : 0));
            }

            for (int page = 1; page < 14; page++)
            {
                AddPage(page);

                for (int row = 0; row < 12; row++)
                {
                    int hue = (2501 + ((page == 13) ? 6 : 0) + (row + (12 * (page - 1)))); /* OSI just had to skip 6 unused hues, didnt they */
                    AddRadio(260, (65 + (row * 25)), 0xd2, 0xd3, false, hue);
                    AddItem(280, (65 + (row * 25)), 0x1412, hue);
                }
            }
        }

        public MetallicHuePicker(Mobile from, MetallicHuePickerCallback callback, object state)
            : base(450, 450)
        {
            m_From = from;
            m_Callback = callback;
            m_State = state;

            Render();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1: // Okay
                    {
                        if (info.Switches.Length > 0)
                        {
                            m_Callback(m_From, m_State, info.Switches[0]);
                        }
                        break;
                    }
                case 2: // Default
                    {
                        m_Callback(m_From, m_State, 0);

                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Blacksmith Resources
    /// </summary>
    public abstract class BaseOre : Item
    {
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; InvalidateProperties(); }
        }

        public abstract BaseIngot GetIngot();

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info;

                        switch (reader.ReadInt())
                        {
                            case 0: info = OreInfo.Iron; break;
                            case 1: info = OreInfo.DullCopper; break;
                            case 2: info = OreInfo.ShadowIron; break;
                            case 3: info = OreInfo.Copper; break;
                            case 4: info = OreInfo.Bronze; break;
                            case 5: info = OreInfo.Gold; break;
                            case 6: info = OreInfo.Agapite; break;
                            case 7: info = OreInfo.Verite; break;
                            case 8: info = OreInfo.Valorite; break;
                            default: info = null; break;
                        }

                        m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }

        private static int RandomSize()
        {
            double rand = Utility.RandomDouble();

            if (rand < 0.12)
                return 0x19B7;
            else if (rand < 0.18)
                return 0x19B8;
            else if (rand < 0.25)
                return 0x19BA;
            else
                return 0x19B9;
        }

        public BaseOre(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseOre(CraftResource resource, int amount)
            : base(RandomSize())
        {
            Stackable = true;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);

            m_Resource = resource;
        }

        public BaseOre(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Amount > 1)
                list.Add(1050039, "{0}\t#{1}", Amount, 1026583); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1026583); // ore
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.DullCopper && m_Resource <= CraftResource.Valorite)
                    return 1042845 + (int)(m_Resource - CraftResource.DullCopper);

                return 1042853; // iron ore;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Movable)
                return;

            if (RootParent is BaseCreature)
            {
                from.SendLocalizedMessage(500447); // That is not accessible
            }
            else if (from.InRange(this.GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(501971); // Select the forge on which to smelt the ore, or another pile of ore with which to combine it.
                from.Target = new InternalTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(501976); // The ore is too far away.
            }
        }

        private class InternalTarget : Target
        {
            private BaseOre m_Ore;

            public InternalTarget(BaseOre ore)
                : base(2, false, TargetFlags.None)
            {
                m_Ore = ore;
            }

            private bool IsForge(object obj)
            {
                if (Core.ML && obj is Mobile && ((Mobile)obj).IsDeadBondedPet)
                    return false;

                if (obj.GetType().IsDefined(typeof(ForgeAttribute), false))
                    return true;

                int itemID = 0;

                if (obj is Item)
                    itemID = ((Item)obj).ItemID;
                else if (obj is StaticTarget)
                    itemID = ((StaticTarget)obj).ItemID;

                return (itemID == 4017 || (itemID >= 6522 && itemID <= 6569));
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Ore.Deleted)
                    return;

                if (!from.InRange(m_Ore.GetWorldLocation(), 2))
                {
                    from.SendLocalizedMessage(501976); // The ore is too far away.
                    return;
                }

                #region Combine Ore
                if (targeted is BaseOre)
                {
                    BaseOre ore = (BaseOre)targeted;

                    if (!ore.Movable)
                    {
                        return;
                    }
                    else if (m_Ore == ore)
                    {
                        from.SendLocalizedMessage(501972); // Select another pile or ore with which to combine this.
                        from.Target = new InternalTarget(ore);
                        return;
                    }
                    else if (ore.Resource != m_Ore.Resource)
                    {
                        from.SendLocalizedMessage(501979); // You cannot combine ores of different metals.
                        return;
                    }

                    int worth = ore.Amount;

                    if (ore.ItemID == 0x19B9)
                        worth *= 8;
                    else if (ore.ItemID == 0x19B7)
                        worth *= 2;
                    else
                        worth *= 4;

                    int sourceWorth = m_Ore.Amount;

                    if (m_Ore.ItemID == 0x19B9)
                        sourceWorth *= 8;
                    else if (m_Ore.ItemID == 0x19B7)
                        sourceWorth *= 2;
                    else
                        sourceWorth *= 4;

                    worth += sourceWorth;

                    int plusWeight = 0;
                    int newID = ore.ItemID;

                    if (ore.DefaultWeight != m_Ore.DefaultWeight)
                    {
                        if (ore.ItemID == 0x19B7 || m_Ore.ItemID == 0x19B7)
                        {
                            newID = 0x19B7;
                        }
                        else if (ore.ItemID == 0x19B9)
                        {
                            newID = m_Ore.ItemID;
                            plusWeight = ore.Amount * 2;
                        }
                        else
                        {
                            plusWeight = m_Ore.Amount * 2;
                        }
                    }

                    if ((ore.ItemID == 0x19B9 && worth > 120000) || ((ore.ItemID == 0x19B8 || ore.ItemID == 0x19BA) && worth > 60000) || (ore.ItemID == 0x19B7 && worth > 30000))
                    {
                        from.SendLocalizedMessage(1062844); // There is too much ore to combine.
                        return;
                    }
                    else if (ore.RootParent is Mobile && (plusWeight + ((Mobile)ore.RootParent).Backpack.TotalWeight) > ((Mobile)ore.RootParent).Backpack.MaxWeight)
                    {
                        from.SendLocalizedMessage(501978); // The weight is too great to combine in a container.
                        return;
                    }

                    ore.ItemID = newID;

                    if (ore.ItemID == 0x19B9)
                        ore.Amount = worth / 8;
                    else if (ore.ItemID == 0x19B7)
                        ore.Amount = worth / 2;
                    else
                        ore.Amount = worth / 4;

                    m_Ore.Delete();
                    return;
                }
                #endregion

                if (IsForge(targeted))
                {
                    double difficulty;

                    switch (m_Ore.Resource)
                    {
                        default: difficulty = 50.0; break;
                        case CraftResource.DullCopper: difficulty = 65.0; break;
                        case CraftResource.ShadowIron: difficulty = 70.0; break;
                        case CraftResource.Copper: difficulty = 75.0; break;
                        case CraftResource.Bronze: difficulty = 80.0; break;
                        case CraftResource.Gold: difficulty = 85.0; break;
                        case CraftResource.Agapite: difficulty = 90.0; break;
                        case CraftResource.Verite: difficulty = 95.0; break;
                        case CraftResource.Valorite: difficulty = 99.0; break;
                    }

                    double minSkill = difficulty - 25.0;
                    double maxSkill = difficulty + 25.0;

                    if (difficulty > 50.0 && difficulty > from.Skills[SkillName.Mining].Value)
                    {
                        from.SendLocalizedMessage(501986); // You have no idea how to smelt this strange ore!
                        return;
                    }

                    if (m_Ore.ItemID == 0x19B7 && m_Ore.Amount < 2)
                    {
                        from.SendLocalizedMessage(501987); // There is not enough metal-bearing ore in this pile to make an ingot.
                        return;
                    }

                    if (from.CheckTargetSkill(SkillName.Mining, targeted, minSkill, maxSkill))
                    {
                        int toConsume = m_Ore.Amount;

                        if (toConsume <= 0)
                        {
                            from.SendLocalizedMessage(501987); // There is not enough metal-bearing ore in this pile to make an ingot.
                        }
                        else
                        {
                            if (toConsume > 30000)
                                toConsume = 30000;

                            int ingotAmount;

                            if (m_Ore.ItemID == 0x19B7)
                            {
                                ingotAmount = toConsume / 2;

                                if (toConsume % 2 != 0)
                                    --toConsume;
                            }
                            else if (m_Ore.ItemID == 0x19B9)
                            {
                                ingotAmount = toConsume * 2;
                            }
                            else
                            {
                                ingotAmount = toConsume;
                            }

                            BaseIngot ingot = m_Ore.GetIngot();
                            ingot.Amount = ingotAmount;

                            m_Ore.Consume(toConsume);
                            from.AddToBackpack(ingot);
                            //from.PlaySound( 0x57 );

                            from.SendLocalizedMessage(501988); // You smelt the ore removing the impurities and put the metal in your backpack.
                        }
                    }
                    else
                    {
                        if (m_Ore.Amount < 2)
                        {
                            if (m_Ore.ItemID == 0x19B9)
                                m_Ore.ItemID = 0x19B8;
                            else
                                m_Ore.ItemID = 0x19B7;
                        }
                        else
                        {
                            m_Ore.Amount /= 2;
                        }

                        from.SendLocalizedMessage(501990); // You burn away the impurities but are left with less useable metal.
                    }
                }
            }
        }
    }

    public abstract class BaseIngot : Item, ICommodity
    {
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; InvalidateProperties(); }
        }

        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        int ICommodity.DescriptionNumber { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info;

                        switch (reader.ReadInt())
                        {
                            case 0: info = OreInfo.Iron; break;
                            case 1: info = OreInfo.DullCopper; break;
                            case 2: info = OreInfo.ShadowIron; break;
                            case 3: info = OreInfo.Copper; break;
                            case 4: info = OreInfo.Bronze; break;
                            case 5: info = OreInfo.Gold; break;
                            case 6: info = OreInfo.Agapite; break;
                            case 7: info = OreInfo.Verite; break;
                            case 8: info = OreInfo.Valorite; break;
                            default: info = null; break;
                        }

                        m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }

        public BaseIngot(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseIngot(CraftResource resource, int amount)
            : base(0x1BF2)
        {
            Stackable = true;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);

            m_Resource = resource;
        }

        public BaseIngot(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Amount > 1)
                list.Add(1050039, "{0}\t#{1}", Amount, 1027154); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1027154); // ingots
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.DullCopper && m_Resource <= CraftResource.Valorite)
                    return 1042684 + (int)(m_Resource - CraftResource.DullCopper);

                return 1042692;
            }
        }
    }

    public abstract class BaseScales : Item, ICommodity
    {
        public override int LabelNumber { get { return 1053139; } } // dragon scales

        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; InvalidateProperties(); }
        }

        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        int ICommodity.DescriptionNumber { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
            }
        }

        public BaseScales(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseScales(CraftResource resource, int amount)
            : base(0x26B4)
        {
            Stackable = true;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);

            m_Resource = resource;
        }

        public BaseScales(Serial serial)
            : base(serial)
        {
        }
    }

    /// <summary>
    /// Cooking Resources
    /// </summary>
    public abstract class BaseMagicFish : Item
    {
        public virtual int Bonus { get { return 0; } }
        public virtual StatType Type { get { return StatType.Str; } }

        public override double DefaultWeight
        {
            get { return 1.0; }
        }

        public BaseMagicFish(int hue)
            : base(0xDD6)
        {
            Hue = hue;
        }

        public BaseMagicFish(Serial serial)
            : base(serial)
        {
        }

        public virtual bool Apply(Mobile from)
        {
            bool applied = Spells.SpellHelper.AddStatOffset(from, Type, Bonus, TimeSpan.FromMinutes(1.0));

            if (!applied)
                from.SendLocalizedMessage(502173); // You are already under a similar effect.

            return applied;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else if (Apply(from))
            {
                from.FixedEffect(0x375A, 10, 15);
                from.PlaySound(0x1E7);
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501774); // You swallow the fish whole!
                Delete();
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

    /// <summary>
    /// Gardening Resources
    /// </summary>
    public abstract class BaseReagent : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        public BaseReagent(int itemID)
            : this(itemID, 1)
        {
        }

        public BaseReagent(int itemID, int amount)
            : base(itemID)
        {
            Stackable = true;
            Amount = amount;
        }

        public BaseReagent(Serial serial)
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
    /// Masonry Resources
    /// </summary>
    public abstract class BaseGranite : Item
    {
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; InvalidateProperties(); }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                case 0:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
            }

            if (version < 1)
                Stackable = Core.ML;
        }

        public override double DefaultWeight
        {
            get { return Core.ML ? 1.0 : 10.0; } // Pub 57
        }

        public BaseGranite(CraftResource resource)
            : base(0x1779)
        {
            Hue = CraftResources.GetHue(resource);
            Stackable = Core.ML;

            m_Resource = resource;
        }

        public BaseGranite(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber { get { return 1044607; } } // high quality granite

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }
    }

    /// <summary>
    /// Tailoring Resources
    /// </summary>
    public abstract class BaseClothMaterial : Item, IDyable
    {
        public BaseClothMaterial(int itemID)
            : this(itemID, 1)
        {
        }

        public BaseClothMaterial(int itemID, int amount)
            : base(itemID)
        {
            Stackable = true;
            Weight = 1.0;
            Amount = amount;
        }

        public BaseClothMaterial(Serial serial)
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

        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;

            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(500366); // Select a loom to use that on.
                from.Target = new PickLoomTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        private class PickLoomTarget : Target
        {
            private BaseClothMaterial m_Material;

            public PickLoomTarget(BaseClothMaterial material)
                : base(3, false, TargetFlags.None)
            {
                m_Material = material;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Material.Deleted)
                    return;

                ILoom loom = targeted as ILoom;

                if (loom == null && targeted is AddonComponent)
                    loom = ((AddonComponent)targeted).Addon as ILoom;

                if (loom != null)
                {
                    if (!m_Material.IsChildOf(from.Backpack))
                    {
                        from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                    }
                    else if (loom.Phase < 4)
                    {
                        m_Material.Consume();

                        if (targeted is Item)
                            ((Item)targeted).SendLocalizedMessageTo(from, 1010001 + loom.Phase++);
                    }
                    else
                    {
                        Item create = new BoltOfCloth();
                        create.Hue = m_Material.Hue;

                        m_Material.Consume();
                        loom.Phase = 0;
                        from.SendLocalizedMessage(500368); // You create some cloth and put it in your backpack.
                        from.AddToBackpack(create);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(500367); // Try using that on a loom.
                }
            }
        }
    }

    public abstract class BaseHides : Item, ICommodity
    {
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; InvalidateProperties(); }
        }

        int ICommodity.DescriptionNumber { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info = new OreInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadString());

                        m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }

        public BaseHides(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseHides(CraftResource resource, int amount)
            : base(0x1079)
        {
            Stackable = true;
            Weight = 5.0;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);

            m_Resource = resource;
        }

        public BaseHides(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Amount > 1)
                list.Add(1050039, "{0}\t#{1}", Amount, 1024216); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1024216); // pile of hides
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.SpinedLeather && m_Resource <= CraftResource.BarbedLeather)
                    return 1049687 + (int)(m_Resource - CraftResource.SpinedLeather);

                return 1047023;
            }
        }
    }

    public abstract class BaseLeather : Item, ICommodity
    {
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; InvalidateProperties(); }
        }

        int ICommodity.DescriptionNumber { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info = new OreInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadString());

                        m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }

        public BaseLeather(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseLeather(CraftResource resource, int amount)
            : base(0x1081)
        {
            Stackable = true;
            Weight = 1.0;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);

            m_Resource = resource;
        }

        public BaseLeather(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Amount > 1)
                list.Add(1050039, "{0}\t#{1}", Amount, 1024199); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1024199); // cut leather
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.SpinedLeather && m_Resource <= CraftResource.BarbedLeather)
                    return 1049684 + (int)(m_Resource - CraftResource.SpinedLeather);

                return 1047022;
            }
        }
    }
}