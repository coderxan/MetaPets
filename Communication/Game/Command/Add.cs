using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Xml;

using Server;
using Server.Accounting;
using Server.Commands;
using Server.Engines.Help;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;
using Server.Targeting;

namespace Server.Commands
{
    public class Categorization
    {
        private static CategoryEntry m_RootItems, m_RootMobiles;

        public static CategoryEntry Items
        {
            get
            {
                if (m_RootItems == null)
                    Load();

                return m_RootItems;
            }
        }

        public static CategoryEntry Mobiles
        {
            get
            {
                if (m_RootMobiles == null)
                    Load();

                return m_RootMobiles;
            }
        }

        public static void Initialize()
        {
            CommandSystem.Register("RebuildCategorization", AccessLevel.Administrator, new CommandEventHandler(RebuildCategorization_OnCommand));
        }

        [Usage("RebuildCategorization")]
        [Description("Rebuilds the categorization data file used by the Add command.")]
        public static void RebuildCategorization_OnCommand(CommandEventArgs e)
        {
            CategoryEntry root = new CategoryEntry(null, "Add Menu", new CategoryEntry[] { Items, Mobiles });

            Export(root, "Data/objects.xml", "Objects");

            e.Mobile.SendMessage("Categorization menu rebuilt.");
        }

        public static void RecurseFindCategories(CategoryEntry ce, ArrayList list)
        {
            list.Add(ce);

            for (int i = 0; i < ce.SubCategories.Length; ++i)
                RecurseFindCategories(ce.SubCategories[i], list);
        }

        public static void Export(CategoryEntry ce, string fileName, string title)
        {
            XmlTextWriter xml = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);

            xml.Indentation = 1;
            xml.IndentChar = '\t';
            xml.Formatting = Formatting.Indented;

            xml.WriteStartDocument(true);

            RecurseExport(xml, ce);

            xml.Flush();
            xml.Close();
        }

        public static void RecurseExport(XmlTextWriter xml, CategoryEntry ce)
        {
            xml.WriteStartElement("category");

            xml.WriteAttributeString("title", ce.Title);

            ArrayList subCats = new ArrayList(ce.SubCategories);

            subCats.Sort(new CategorySorter());

            for (int i = 0; i < subCats.Count; ++i)
                RecurseExport(xml, (CategoryEntry)subCats[i]);

            ce.Matched.Sort(new CategorySorter());

            for (int i = 0; i < ce.Matched.Count; ++i)
            {
                CategoryTypeEntry cte = (CategoryTypeEntry)ce.Matched[i];

                xml.WriteStartElement("object");

                xml.WriteAttributeString("type", cte.Type.ToString());

                object obj = cte.Object;

                if (obj is Item)
                {
                    Item item = (Item)obj;

                    int itemID = item.ItemID;

                    if (item is BaseAddon && ((BaseAddon)item).Components.Count == 1)
                        itemID = ((AddonComponent)(((BaseAddon)item).Components[0])).ItemID;

                    if (itemID > TileData.MaxItemValue)
                        itemID = 1;

                    xml.WriteAttributeString("gfx", XmlConvert.ToString(itemID));

                    int hue = item.Hue & 0x7FFF;

                    if ((hue & 0x4000) != 0)
                        hue = 0;

                    if (hue != 0)
                        xml.WriteAttributeString("hue", XmlConvert.ToString(hue));

                    item.Delete();
                }
                else if (obj is Mobile)
                {
                    Mobile mob = (Mobile)obj;

                    int itemID = ShrinkTable.Lookup(mob, 1);

                    xml.WriteAttributeString("gfx", XmlConvert.ToString(itemID));

                    int hue = mob.Hue & 0x7FFF;

                    if ((hue & 0x4000) != 0)
                        hue = 0;

                    if (hue != 0)
                        xml.WriteAttributeString("hue", XmlConvert.ToString(hue));

                    mob.Delete();
                }

                xml.WriteEndElement();
            }

            xml.WriteEndElement();
        }

        public static void Load()
        {
            ArrayList types = new ArrayList();

            AddTypes(Core.Assembly, types);

            for (int i = 0; i < ScriptCompiler.Assemblies.Length; ++i)
                AddTypes(ScriptCompiler.Assemblies[i], types);

            m_RootItems = Load(types, "Data/items.cfg");
            m_RootMobiles = Load(types, "Data/mobiles.cfg");
        }

        private static CategoryEntry Load(ArrayList types, string config)
        {
            CategoryLine[] lines = CategoryLine.Load(config);

            if (lines.Length > 0)
            {
                int index = 0;
                CategoryEntry root = new CategoryEntry(null, lines, ref index);

                Fill(root, types);

                return root;
            }

            return new CategoryEntry();
        }

        private static Type typeofItem = typeof(Item);
        private static Type typeofMobile = typeof(Mobile);
        private static Type typeofConstructable = typeof(ConstructableAttribute);

        private static bool IsConstructable(Type type)
        {
            if (!type.IsSubclassOf(typeofItem) && !type.IsSubclassOf(typeofMobile))
                return false;

            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);

            return (ctor != null && ctor.IsDefined(typeofConstructable, false));
        }

        private static void AddTypes(Assembly asm, ArrayList types)
        {
            Type[] allTypes = asm.GetTypes();

            for (int i = 0; i < allTypes.Length; ++i)
            {
                Type type = allTypes[i];

                if (type.IsAbstract)
                    continue;

                if (IsConstructable(type))
                    types.Add(type);
            }
        }

        private static void Fill(CategoryEntry root, ArrayList list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                Type type = (Type)list[i];
                CategoryEntry match = GetDeepestMatch(root, type);

                if (match == null)
                    continue;

                try
                {
                    match.Matched.Add(new CategoryTypeEntry(type));
                }
                catch
                {
                }
            }
        }

        private static CategoryEntry GetDeepestMatch(CategoryEntry root, Type type)
        {
            if (!root.IsMatch(type))
                return null;

            for (int i = 0; i < root.SubCategories.Length; ++i)
            {
                CategoryEntry check = GetDeepestMatch(root.SubCategories[i], type);

                if (check != null)
                    return check;
            }

            return root;
        }
    }

    public class CategorySorter : IComparer
    {
        public int Compare(object x, object y)
        {
            string a = null, b = null;

            if (x is CategoryEntry)
                a = ((CategoryEntry)x).Title;
            else if (x is CategoryTypeEntry)
                a = ((CategoryTypeEntry)x).Type.Name;

            if (y is CategoryEntry)
                b = ((CategoryEntry)y).Title;
            else if (y is CategoryTypeEntry)
                b = ((CategoryTypeEntry)y).Type.Name;

            if (a == null && b == null)
                return 0;

            if (a == null)
                return 1;

            if (b == null)
                return -1;

            return a.CompareTo(b);
        }
    }

    public class CategoryTypeEntry
    {
        private Type m_Type;
        private object m_Object;

        public Type Type { get { return m_Type; } }
        public object Object { get { return m_Object; } }

        public CategoryTypeEntry(Type type)
        {
            m_Type = type;
            m_Object = Activator.CreateInstance(type);
        }
    }

    public class CategoryEntry
    {
        private string m_Title;
        private Type[] m_Matches;
        private CategoryEntry[] m_SubCategories;
        private CategoryEntry m_Parent;
        private ArrayList m_Matched;

        public string Title { get { return m_Title; } }
        public Type[] Matches { get { return m_Matches; } }
        public CategoryEntry Parent { get { return m_Parent; } }
        public CategoryEntry[] SubCategories { get { return m_SubCategories; } }
        public ArrayList Matched { get { return m_Matched; } }

        public CategoryEntry()
        {
            m_Title = "(empty)";
            m_Matches = new Type[0];
            m_SubCategories = new CategoryEntry[0];
            m_Matched = new ArrayList();
        }

        public CategoryEntry(CategoryEntry parent, string title, CategoryEntry[] subCats)
        {
            m_Parent = parent;
            m_Title = title;
            m_SubCategories = subCats;
            m_Matches = new Type[0];
            m_Matched = new ArrayList();
        }

        public bool IsMatch(Type type)
        {
            bool isMatch = false;

            for (int i = 0; !isMatch && i < m_Matches.Length; ++i)
                isMatch = (type == m_Matches[i] || type.IsSubclassOf(m_Matches[i]));

            return isMatch;
        }

        public CategoryEntry(CategoryEntry parent, CategoryLine[] lines, ref int index)
        {
            m_Parent = parent;

            string text = lines[index].Text;

            int start = text.IndexOf('(');

            if (start < 0)
                throw new FormatException(String.Format("Input string not correctly formatted ('{0}')", text));

            m_Title = text.Substring(0, start).Trim();

            int end = text.IndexOf(')', ++start);

            if (end < start)
                throw new FormatException(String.Format("Input string not correctly formatted ('{0}')", text));

            text = text.Substring(start, end - start);
            string[] split = text.Split(';');

            ArrayList list = new ArrayList();

            for (int i = 0; i < split.Length; ++i)
            {
                Type type = ScriptCompiler.FindTypeByName(split[i].Trim());

                if (type == null)
                    Console.WriteLine("Match type not found ('{0}')", split[i].Trim());
                else
                    list.Add(type);
            }

            m_Matches = (Type[])list.ToArray(typeof(Type));
            list.Clear();

            int ourIndentation = lines[index].Indentation;

            ++index;

            while (index < lines.Length && lines[index].Indentation > ourIndentation)
                list.Add(new CategoryEntry(this, lines, ref index));

            m_SubCategories = (CategoryEntry[])list.ToArray(typeof(CategoryEntry));
            list.Clear();

            m_Matched = list;
        }
    }

    public class CategoryLine
    {
        private int m_Indentation;
        private string m_Text;

        public int Indentation { get { return m_Indentation; } }
        public string Text { get { return m_Text; } }

        public CategoryLine(string input)
        {
            int index;

            for (index = 0; index < input.Length; ++index)
            {
                if (Char.IsLetter(input, index))
                    break;
            }

            if (index >= input.Length)
                throw new FormatException(String.Format("Input string not correctly formatted ('{0}')", input));

            m_Indentation = index;
            m_Text = input.Substring(index);
        }

        public static CategoryLine[] Load(string path)
        {
            ArrayList list = new ArrayList();

            if (File.Exists(path))
            {
                using (StreamReader ip = new StreamReader(path))
                {
                    string line;

                    while ((line = ip.ReadLine()) != null)
                        list.Add(new CategoryLine(line));
                }
            }

            return (CategoryLine[])list.ToArray(typeof(CategoryLine));
        }
    }
}

namespace Server.Commands.Generic
{
    public class AddCommand : BaseCommand
    {
        public AddCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.Simple | CommandSupport.Self;
            Commands = new string[] { "Add" };
            ObjectTypes = ObjectTypes.All;
            Usage = "Add [<name> [params] [set {<propertyName> <value> ...}]]";
            Description = "Adds an item or npc by name to a targeted location. Optional constructor parameters. Optional set property list. If no arguments are specified, this brings up a categorized add menu.";
        }

        public override bool ValidateArgs(BaseCommandImplementor impl, CommandEventArgs e)
        {
            if (e.Length >= 1)
            {
                Type t = ScriptCompiler.FindTypeByName(e.GetString(0));

                if (t == null)
                {
                    e.Mobile.SendMessage("No type with that name was found.");

                    string match = e.GetString(0).Trim();

                    if (match.Length < 3)
                    {
                        e.Mobile.SendMessage("Invalid search string.");
                        e.Mobile.SendGump(new AddGump(e.Mobile, match, 0, Type.EmptyTypes, false));
                    }
                    else
                    {
                        e.Mobile.SendGump(new AddGump(e.Mobile, match, 0, AddGump.Match(match).ToArray(), true));
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                e.Mobile.SendGump(new CategorizedAddGump(e.Mobile));
            }

            return false;
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            IPoint3D p = obj as IPoint3D;

            if (p == null)
                return;

            if (p is Item)
                p = ((Item)p).GetWorldTop();
            else if (p is Mobile)
                p = ((Mobile)p).Location;

            Add.Invoke(e.Mobile, new Point3D(p), new Point3D(p), e.Arguments);
        }
    }
}

namespace Server.Gumps
{
    public class AddGump : Gump
    {
        private string m_SearchString;
        private Type[] m_SearchResults;
        private int m_Page;

        public static void Initialize()
        {
            CommandSystem.Register("AddMenu", AccessLevel.GameMaster, new CommandEventHandler(AddMenu_OnCommand));
        }

        [Usage("AddMenu [searchString]")]
        [Description("Opens an add menu, with an optional initial search string. This menu allows you to search for Items or Mobiles and add them interactively.")]
        private static void AddMenu_OnCommand(CommandEventArgs e)
        {
            string val = e.ArgString.Trim();
            Type[] types;
            bool explicitSearch = false;

            if (val.Length == 0)
            {
                types = Type.EmptyTypes;
            }
            else if (val.Length < 3)
            {
                e.Mobile.SendMessage("Invalid search string.");
                types = Type.EmptyTypes;
            }
            else
            {
                types = Match(val).ToArray();
                explicitSearch = true;
            }

            e.Mobile.SendGump(new AddGump(e.Mobile, val, 0, types, explicitSearch));
        }

        public AddGump(Mobile from, string searchString, int page, Type[] searchResults, bool explicitSearch)
            : base(50, 50)
        {
            m_SearchString = searchString;
            m_SearchResults = searchResults;
            m_Page = page;

            from.CloseGump(typeof(AddGump));

            AddPage(0);

            AddBackground(0, 0, 420, 280, 5054);

            AddImageTiled(10, 10, 400, 20, 2624);
            AddAlphaRegion(10, 10, 400, 20);
            AddImageTiled(41, 11, 184, 18, 0xBBC);
            AddImageTiled(42, 12, 182, 16, 2624);
            AddAlphaRegion(42, 12, 182, 16);

            AddButton(10, 9, 4011, 4013, 1, GumpButtonType.Reply, 0);
            AddTextEntry(44, 10, 180, 20, 0x480, 0, searchString);

            AddHtmlLocalized(230, 10, 100, 20, 3010005, 0x7FFF, false, false);

            AddImageTiled(10, 40, 400, 200, 2624);
            AddAlphaRegion(10, 40, 400, 200);

            if (searchResults.Length > 0)
            {
                for (int i = (page * 10); i < ((page + 1) * 10) && i < searchResults.Length; ++i)
                {
                    int index = i % 10;

                    AddLabel(44, 39 + (index * 20), 0x480, searchResults[i].Name);
                    AddButton(10, 39 + (index * 20), 4023, 4025, 4 + i, GumpButtonType.Reply, 0);
                }
            }
            else
            {
                AddLabel(15, 44, 0x480, explicitSearch ? "Nothing matched your search terms." : "No results to display.");
            }

            AddImageTiled(10, 250, 400, 20, 2624);
            AddAlphaRegion(10, 250, 400, 20);

            if (m_Page > 0)
                AddButton(10, 249, 4014, 4016, 2, GumpButtonType.Reply, 0);
            else
                AddImage(10, 249, 4014);

            AddHtmlLocalized(44, 250, 170, 20, 1061028, m_Page > 0 ? 0x7FFF : 0x5EF7, false, false); // Previous page

            if (((m_Page + 1) * 10) < searchResults.Length)
                AddButton(210, 249, 4005, 4007, 3, GumpButtonType.Reply, 0);
            else
                AddImage(210, 249, 4005);

            AddHtmlLocalized(244, 250, 170, 20, 1061027, ((m_Page + 1) * 10) < searchResults.Length ? 0x7FFF : 0x5EF7, false, false); // Next page
        }

        private static Type typeofItem = typeof(Item), typeofMobile = typeof(Mobile);

        private static void Match(string match, Type[] types, List<Type> results)
        {
            if (match.Length == 0)
                return;

            match = match.ToLower();

            for (int i = 0; i < types.Length; ++i)
            {
                Type t = types[i];

                if ((typeofMobile.IsAssignableFrom(t) || typeofItem.IsAssignableFrom(t)) && t.Name.ToLower().IndexOf(match) >= 0 && !results.Contains(t))
                {
                    ConstructorInfo[] ctors = t.GetConstructors();

                    for (int j = 0; j < ctors.Length; ++j)
                    {
                        if (ctors[j].GetParameters().Length == 0 && ctors[j].IsDefined(typeof(ConstructableAttribute), false))
                        {
                            results.Add(t);
                            break;
                        }
                    }
                }
            }
        }

        public static List<Type> Match(string match)
        {
            List<Type> results = new List<Type>();
            Type[] types;

            Assembly[] asms = ScriptCompiler.Assemblies;

            for (int i = 0; i < asms.Length; ++i)
            {
                types = ScriptCompiler.GetTypeCache(asms[i]).Types;
                Match(match, types, results);
            }

            types = ScriptCompiler.GetTypeCache(Core.Assembly).Types;
            Match(match, types, results);

            results.Sort(new TypeNameComparer());

            return results;
        }

        private class TypeNameComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        public class InternalTarget : Target
        {
            private Type m_Type;
            private Type[] m_SearchResults;
            private string m_SearchString;
            private int m_Page;

            public InternalTarget(Type type, Type[] searchResults, string searchString, int page)
                : base(-1, true, TargetFlags.None)
            {
                m_Type = type;
                m_SearchResults = searchResults;
                m_SearchString = searchString;
                m_Page = page;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;

                if (p != null)
                {
                    if (p is Item)
                        p = ((Item)p).GetWorldTop();
                    else if (p is Mobile)
                        p = ((Mobile)p).Location;

                    Server.Commands.Add.Invoke(from, new Point3D(p), new Point3D(p), new string[] { m_Type.Name });

                    from.Target = new InternalTarget(m_Type, m_SearchResults, m_SearchString, m_Page);
                }
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                if (cancelType == TargetCancelType.Canceled)
                    from.SendGump(new AddGump(from, m_SearchString, m_Page, m_SearchResults, true));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 1: // Search
                    {
                        TextRelay te = info.GetTextEntry(0);
                        string match = (te == null ? "" : te.Text.Trim());

                        if (match.Length < 3)
                        {
                            from.SendMessage("Invalid search string.");
                            from.SendGump(new AddGump(from, match, m_Page, m_SearchResults, false));
                        }
                        else
                        {
                            from.SendGump(new AddGump(from, match, 0, Match(match).ToArray(), true));
                        }

                        break;
                    }
                case 2: // Previous page
                    {
                        if (m_Page > 0)
                            from.SendGump(new AddGump(from, m_SearchString, m_Page - 1, m_SearchResults, true));

                        break;
                    }
                case 3: // Next page
                    {
                        if ((m_Page + 1) * 10 < m_SearchResults.Length)
                            from.SendGump(new AddGump(from, m_SearchString, m_Page + 1, m_SearchResults, true));

                        break;
                    }
                default:
                    {
                        int index = info.ButtonID - 4;

                        if (index >= 0 && index < m_SearchResults.Length)
                        {
                            from.SendMessage("Where do you wish to place this object? <ESC> to cancel.");
                            from.Target = new InternalTarget(m_SearchResults[index], m_SearchResults, m_SearchString, m_Page);
                        }

                        break;
                    }
            }
        }
    }

    public abstract class CAGNode
    {
        public abstract string Caption { get; }
        public abstract void OnClick(Mobile from, int page);
    }

    public class CAGObject : CAGNode
    {
        private Type m_Type;
        private int m_ItemID;
        private int m_Hue;
        private CAGCategory m_Parent;

        public Type Type { get { return m_Type; } }
        public int ItemID { get { return m_ItemID; } }
        public int Hue { get { return m_Hue; } }
        public CAGCategory Parent { get { return m_Parent; } }

        public override string Caption { get { return (m_Type == null ? "bad type" : m_Type.Name); } }

        public override void OnClick(Mobile from, int page)
        {
            if (m_Type == null)
            {
                from.SendMessage("That is an invalid type name.");
            }
            else
            {
                CommandSystem.Handle(from, String.Format("{0}Add {1}", CommandSystem.Prefix, m_Type.Name));

                from.SendGump(new CategorizedAddGump(from, m_Parent, page));
            }
        }

        public CAGObject(CAGCategory parent, XmlTextReader xml)
        {
            m_Parent = parent;

            if (xml.MoveToAttribute("type"))
                m_Type = ScriptCompiler.FindTypeByFullName(xml.Value, false);

            if (xml.MoveToAttribute("gfx"))
                m_ItemID = XmlConvert.ToInt32(xml.Value);

            if (xml.MoveToAttribute("hue"))
                m_Hue = XmlConvert.ToInt32(xml.Value);
        }
    }

    public class CAGCategory : CAGNode
    {
        private string m_Title;
        private CAGNode[] m_Nodes;
        private CAGCategory m_Parent;

        public string Title { get { return m_Title; } }
        public CAGNode[] Nodes { get { return m_Nodes; } }
        public CAGCategory Parent { get { return m_Parent; } }

        public override string Caption { get { return m_Title; } }

        public override void OnClick(Mobile from, int page)
        {
            from.SendGump(new CategorizedAddGump(from, this, 0));
        }

        private CAGCategory()
        {
            m_Title = "no data";
            m_Nodes = new CAGNode[0];
        }

        public CAGCategory(CAGCategory parent, XmlTextReader xml)
        {
            m_Parent = parent;

            if (xml.MoveToAttribute("title"))
                m_Title = xml.Value;
            else
                m_Title = "empty";

            if (m_Title == "Docked")
                m_Title = "Docked 2";

            if (xml.IsEmptyElement)
            {
                m_Nodes = new CAGNode[0];
            }
            else
            {
                ArrayList nodes = new ArrayList();

                while (xml.Read() && xml.NodeType != XmlNodeType.EndElement)
                {
                    if (xml.NodeType == XmlNodeType.Element && xml.Name == "object")
                        nodes.Add(new CAGObject(this, xml));
                    else if (xml.NodeType == XmlNodeType.Element && xml.Name == "category")
                    {
                        if (!xml.IsEmptyElement)
                            nodes.Add(new CAGCategory(this, xml));
                    }
                    else
                        xml.Skip();
                }

                m_Nodes = (CAGNode[])nodes.ToArray(typeof(CAGNode));
            }
        }

        private static CAGCategory m_Root;

        public static CAGCategory Root
        {
            get
            {
                if (m_Root == null)
                    m_Root = Load("Data/objects.xml");

                return m_Root;
            }
        }

        public static CAGCategory Load(string path)
        {
            if (File.Exists(path))
            {
                XmlTextReader xml = new XmlTextReader(path);

                xml.WhitespaceHandling = WhitespaceHandling.None;

                while (xml.Read())
                {
                    if (xml.Name == "category" && xml.NodeType == XmlNodeType.Element)
                    {
                        CAGCategory cat = new CAGCategory(null, xml);

                        xml.Close();

                        return cat;
                    }
                }
            }

            return new CAGCategory();
        }
    }

    public class CategorizedAddGump : Gump
    {
        public static bool OldStyle = PropsConfig.OldStyle;

        public static readonly int EntryHeight = 24;//PropsConfig.EntryHeight;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY + (((EntryHeight - 20) / 2) / 2);
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY + (((EntryHeight - 20) / 2) / 2);
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY + (((EntryHeight - 20) / 2) / 2);
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        private static bool PrevLabel = false, NextLabel = false;

        private static readonly int PrevLabelOffsetX = PrevWidth + 1;
        private static readonly int PrevLabelOffsetY = 0;

        private static readonly int NextLabelOffsetX = -29;
        private static readonly int NextLabelOffsetY = 0;

        private static readonly int EntryWidth = 180;
        private static readonly int EntryCount = 15;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        private Mobile m_Owner;
        private CAGCategory m_Category;
        private int m_Page;

        public CategorizedAddGump(Mobile owner)
            : this(owner, CAGCategory.Root, 0)
        {
        }

        public CategorizedAddGump(Mobile owner, CAGCategory category, int page)
            : base(GumpOffsetX, GumpOffsetY)
        {
            owner.CloseGump(typeof(WhoGump));

            m_Owner = owner;
            m_Category = category;

            Initialize(page);
        }

        public void Initialize(int page)
        {
            m_Page = page;

            CAGNode[] nodes = m_Category.Nodes;

            int count = nodes.Length - (page * EntryCount);

            if (count < 0)
                count = 0;
            else if (count > EntryCount)
                count = EntryCount;

            int totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

            AddPage(0);

            AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            if (OldStyle)
                AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
            else
                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

            if (m_Category.Parent != null)
            {
                AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 1, GumpButtonType.Reply, 0);

                if (PrevLabel)
                    AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
            }

            x += PrevWidth + OffsetSize;

            int emptyWidth = TotalWidth - (PrevWidth * 2) - NextWidth - (OffsetSize * 5) - (OldStyle ? SetWidth + OffsetSize : 0);

            if (!OldStyle)
                AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, EntryGumpID);

            AddHtml(x + TextOffsetX, y + ((EntryHeight - 20) / 2), emptyWidth - TextOffsetX, EntryHeight, String.Format("<center>{0}</center>", m_Category.Caption), false, false);

            x += emptyWidth + OffsetSize;

            if (OldStyle)
                AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
            else
                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

            if (page > 0)
            {
                AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 2, GumpButtonType.Reply, 0);

                if (PrevLabel)
                    AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
            }

            x += PrevWidth + OffsetSize;

            if (!OldStyle)
                AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);

            if ((page + 1) * EntryCount < nodes.Length)
            {
                AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 3, GumpButtonType.Reply, 1);

                if (NextLabel)
                    AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
            }

            for (int i = 0, index = page * EntryCount; i < EntryCount && index < nodes.Length; ++i, ++index)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                CAGNode node = nodes[index];

                AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
                AddLabelCropped(x + TextOffsetX, y + ((EntryHeight - 20) / 2), EntryWidth - TextOffsetX, EntryHeight, TextHue, node.Caption);

                x += EntryWidth + OffsetSize;

                if (SetGumpID != 0)
                    AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, i + 4, GumpButtonType.Reply, 0);

                if (node is CAGObject)
                {
                    CAGObject obj = (CAGObject)node;
                    int itemID = obj.ItemID;

                    Rectangle2D bounds = ItemBounds.Table[itemID];

                    if (itemID != 1 && bounds.Height < (EntryHeight * 2))
                    {
                        if (bounds.Height < EntryHeight)
                            AddItem(x - OffsetSize - 22 - ((i % 2) * 44) - (bounds.Width / 2) - bounds.X, y + (EntryHeight / 2) - (bounds.Height / 2) - bounds.Y, itemID);
                        else
                            AddItem(x - OffsetSize - 22 - ((i % 2) * 44) - (bounds.Width / 2) - bounds.X, y + EntryHeight - 1 - bounds.Height - bounds.Y, itemID);
                    }
                }
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = m_Owner;

            switch (info.ButtonID)
            {
                case 0: // Closed
                    {
                        return;
                    }
                case 1: // Up
                    {
                        if (m_Category.Parent != null)
                        {
                            int index = Array.IndexOf(m_Category.Parent.Nodes, m_Category) / EntryCount;

                            if (index < 0)
                                index = 0;

                            from.SendGump(new CategorizedAddGump(from, m_Category.Parent, index));
                        }

                        break;
                    }
                case 2: // Previous
                    {
                        if (m_Page > 0)
                            from.SendGump(new CategorizedAddGump(from, m_Category, m_Page - 1));

                        break;
                    }
                case 3: // Next
                    {
                        if ((m_Page + 1) * EntryCount < m_Category.Nodes.Length)
                            from.SendGump(new CategorizedAddGump(from, m_Category, m_Page + 1));

                        break;
                    }
                default:
                    {
                        int index = (m_Page * EntryCount) + (info.ButtonID - 4);

                        if (index >= 0 && index < m_Category.Nodes.Length)
                            m_Category.Nodes[index].OnClick(from, m_Page);

                        break;
                    }
            }
        }
    }
}