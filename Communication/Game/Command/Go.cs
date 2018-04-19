using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using Server;
using Server.Accounting;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Menus;
using Server.Menus.ItemLists;
using Server.Menus.Questions;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Targeting;
using Server.Targets;

namespace Server.Commands
{
    public partial class CommandHandlers
    {
        [Usage("Go [name | serial | (x y [z]) | (deg min (N | S) deg min (E | W))]")]
        [Description("With no arguments, this command brings up the go menu. With one argument, (name), you are moved to that regions \"go location.\" Or, if a numerical value is specified for one argument, (serial), you are moved to that object. Two or three arguments, (x y [z]), will move your character to that location. When six arguments are specified, (deg min (N | S) deg min (E | W)), your character will go to an approximate of those sextant coordinates.")]
        private static void Go_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length == 0)
            {
                GoGump.DisplayTo(from);
                return;
            }

            if (e.Length == 1)
            {
                try
                {
                    int ser = e.GetInt32(0);

                    IEntity ent = World.FindEntity(ser);

                    if (ent is Item)
                    {
                        Item item = (Item)ent;

                        Map map = item.Map;
                        Point3D loc = item.GetWorldLocation();

                        Mobile owner = item.RootParent as Mobile;

                        if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(from, owner) /* !from.CanSee( owner )*/ )
                        {
                            from.SendMessage("You can not go to what you can not see.");
                            return;
                        }
                        else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= from.AccessLevel)
                        {
                            from.SendMessage("You can not go to what you can not see.");
                            return;
                        }
                        else if (!FixMap(ref map, ref loc, item))
                        {
                            from.SendMessage("That is an internal item and you cannot go to it.");
                            return;
                        }

                        from.MoveToWorld(loc, map);

                        return;
                    }
                    else if (ent is Mobile)
                    {
                        Mobile m = (Mobile)ent;

                        Map map = m.Map;
                        Point3D loc = m.Location;

                        Mobile owner = m;

                        if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(from, owner) /* !from.CanSee( owner )*/ )
                        {
                            from.SendMessage("You can not go to what you can not see.");
                            return;
                        }
                        else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= from.AccessLevel)
                        {
                            from.SendMessage("You can not go to what you can not see.");
                            return;
                        }
                        else if (!FixMap(ref map, ref loc, m))
                        {
                            from.SendMessage("That is an internal mobile and you cannot go to it.");
                            return;
                        }

                        from.MoveToWorld(loc, map);

                        return;
                    }
                    else
                    {
                        string name = e.GetString(0);
                        Map map;

                        for (int i = 0; i < Map.AllMaps.Count; ++i)
                        {
                            map = Map.AllMaps[i];

                            if (map.MapIndex == 0x7F || map.MapIndex == 0xFF)
                                continue;

                            if (Insensitive.Equals(name, map.Name))
                            {
                                from.Map = map;
                                return;
                            }
                        }

                        Dictionary<string, Region> list = from.Map.Regions;

                        foreach (KeyValuePair<string, Region> kvp in list)
                        {
                            Region r = kvp.Value;

                            if (Insensitive.Equals(r.Name, name))
                            {
                                from.Location = new Point3D(r.GoLocation);
                                return;
                            }
                        }

                        for (int i = 0; i < Map.AllMaps.Count; ++i)
                        {
                            Map m = Map.AllMaps[i];

                            if (m.MapIndex == 0x7F || m.MapIndex == 0xFF || from.Map == m)
                                continue;

                            foreach (Region r in m.Regions.Values)
                            {
                                if (Insensitive.Equals(r.Name, name))
                                {
                                    from.MoveToWorld(r.GoLocation, m);
                                    return;
                                }
                            }
                        }

                        if (ser != 0)
                            from.SendMessage("No object with that serial was found.");
                        else
                            from.SendMessage("No region with that name was found.");

                        return;
                    }
                }
                catch
                {
                }

                from.SendMessage("Region name not found");
            }
            else if (e.Length == 2 || e.Length == 3)
            {
                Map map = from.Map;

                if (map != null)
                {
                    try
                    {
                        /*
                         * This to avoid being teleported to (0,0) if trying to teleport
                         * to a region with spaces in its name.
                         */
                        int x = int.Parse(e.GetString(0));
                        int y = int.Parse(e.GetString(1));
                        int z = (e.Length == 3) ? int.Parse(e.GetString(2)) : map.GetAverageZ(x, y);

                        from.Location = new Point3D(x, y, z);
                    }
                    catch
                    {
                        from.SendMessage("Region name not found.");
                    }
                }
            }
            else if (e.Length == 6)
            {
                Map map = from.Map;

                if (map != null)
                {
                    Point3D p = Sextant.ReverseLookup(map, e.GetInt32(3), e.GetInt32(0), e.GetInt32(4), e.GetInt32(1), Insensitive.Equals(e.GetString(5), "E"), Insensitive.Equals(e.GetString(2), "S"));

                    if (p != Point3D.Zero)
                        from.Location = p;
                    else
                        from.SendMessage("Sextant reverse lookup failed.");
                }
            }
            else
            {
                from.SendMessage("Format: Go [name | serial | (x y [z]) | (deg min (N | S) deg min (E | W)]");
            }
        }
    }
}

namespace Server.Gumps
{
    public class GoGump : Gump
    {
        public static readonly LocationTree Felucca = new LocationTree("felucca.xml", Map.Felucca);
        public static readonly LocationTree Trammel = new LocationTree("trammel.xml", Map.Trammel);
        public static readonly LocationTree Ilshenar = new LocationTree("ilshenar.xml", Map.Ilshenar);
        public static readonly LocationTree Malas = new LocationTree("malas.xml", Map.Malas);
        public static readonly LocationTree Tokuno = new LocationTree("tokuno.xml", Map.Tokuno);

        public static bool OldStyle = PropsConfig.OldStyle;

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
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

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

        public static void DisplayTo(Mobile from)
        {
            LocationTree tree;

            if (from.Map == Map.Ilshenar)
                tree = Ilshenar;
            else if (from.Map == Map.Felucca)
                tree = Felucca;
            else if (from.Map == Map.Trammel)
                tree = Trammel;
            else if (from.Map == Map.Malas)
                tree = Malas;
            else
                tree = Tokuno;

            ParentNode branch = null;
            tree.LastBranch.TryGetValue(from, out branch);

            if (branch == null)
                branch = tree.Root;

            if (branch != null)
                from.SendGump(new GoGump(0, from, tree, branch));
        }

        private LocationTree m_Tree;
        private ParentNode m_Node;
        private int m_Page;

        private GoGump(int page, Mobile from, LocationTree tree, ParentNode node)
            : base(50, 50)
        {
            from.CloseGump(typeof(GoGump));

            tree.LastBranch[from] = node;

            m_Page = page;
            m_Tree = tree;
            m_Node = node;

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            int count = node.Children.Length - (page * EntryCount);

            if (count < 0)
                count = 0;
            else if (count > EntryCount)
                count = EntryCount;

            int totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

            AddPage(0);

            AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

            if (OldStyle)
                AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
            else
                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

            if (node.Parent != null)
            {
                AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 1, GumpButtonType.Reply, 0);

                if (PrevLabel)
                    AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
            }

            x += PrevWidth + OffsetSize;

            int emptyWidth = TotalWidth - (PrevWidth * 2) - NextWidth - (OffsetSize * 5) - (OldStyle ? SetWidth + OffsetSize : 0);

            if (!OldStyle)
                AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, EntryGumpID);

            AddHtml(x + TextOffsetX, y, emptyWidth - TextOffsetX, EntryHeight, String.Format("<center>{0}</center>", node.Name), false, false);

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

            if ((page + 1) * EntryCount < node.Children.Length)
            {
                AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 3, GumpButtonType.Reply, 1);

                if (NextLabel)
                    AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
            }

            for (int i = 0, index = page * EntryCount; i < EntryCount && index < node.Children.Length; ++i, ++index)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                object child = node.Children[index];
                string name = "";

                if (child is ParentNode)
                    name = ((ParentNode)child).Name;
                else if (child is ChildNode)
                    name = ((ChildNode)child).Name;

                AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
                AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, name);

                x += EntryWidth + OffsetSize;

                if (SetGumpID != 0)
                    AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, index + 4, GumpButtonType.Reply, 0);
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            switch (info.ButtonID)
            {
                case 1:
                    {
                        if (m_Node.Parent != null)
                            from.SendGump(new GoGump(0, from, m_Tree, m_Node.Parent));

                        break;
                    }
                case 2:
                    {
                        if (m_Page > 0)
                            from.SendGump(new GoGump(m_Page - 1, from, m_Tree, m_Node));

                        break;
                    }
                case 3:
                    {
                        if ((m_Page + 1) * EntryCount < m_Node.Children.Length)
                            from.SendGump(new GoGump(m_Page + 1, from, m_Tree, m_Node));

                        break;
                    }
                default:
                    {
                        int index = info.ButtonID - 4;

                        if (index >= 0 && index < m_Node.Children.Length)
                        {
                            object o = m_Node.Children[index];

                            if (o is ParentNode)
                            {
                                from.SendGump(new GoGump(0, from, m_Tree, (ParentNode)o));
                            }
                            else
                            {
                                ChildNode n = (ChildNode)o;

                                from.MoveToWorld(n.Location, m_Tree.Map);
                            }
                        }

                        break;
                    }
            }
        }
    }

    public class LocationTree
    {
        private Map m_Map;
        private ParentNode m_Root;
        private Dictionary<Mobile, ParentNode> m_LastBranch;

        public LocationTree(string fileName, Map map)
        {
            m_LastBranch = new Dictionary<Mobile, ParentNode>();
            m_Map = map;

            string path = Path.Combine("Data/Location/", fileName);

            if (File.Exists(path))
            {
                XmlTextReader xml = new XmlTextReader(new StreamReader(path));

                xml.WhitespaceHandling = WhitespaceHandling.None;

                m_Root = Parse(xml);

                xml.Close();
            }
        }

        public Dictionary<Mobile, ParentNode> LastBranch
        {
            get
            {
                return m_LastBranch;
            }
        }

        public Map Map
        {
            get
            {
                return m_Map;
            }
        }

        public ParentNode Root
        {
            get
            {
                return m_Root;
            }
        }

        private ParentNode Parse(XmlTextReader xml)
        {
            xml.Read();
            xml.Read();
            xml.Read();

            return new ParentNode(xml, null);
        }
    }

    public class ParentNode
    {
        private ParentNode m_Parent;
        private object[] m_Children;

        private string m_Name;

        public ParentNode(XmlTextReader xml, ParentNode parent)
        {
            m_Parent = parent;

            Parse(xml);
        }

        private void Parse(XmlTextReader xml)
        {
            if (xml.MoveToAttribute("name"))
                m_Name = xml.Value;
            else
                m_Name = "empty";

            if (xml.IsEmptyElement)
            {
                m_Children = new object[0];
            }
            else
            {
                ArrayList children = new ArrayList();

                while (xml.Read() && (xml.NodeType == XmlNodeType.Element || xml.NodeType == XmlNodeType.Comment))
                {
                    if (xml.NodeType == XmlNodeType.Comment)
                        continue;

                    if (xml.Name == "child")
                    {
                        ChildNode n = new ChildNode(xml, this);

                        children.Add(n);
                    }
                    else
                    {
                        children.Add(new ParentNode(xml, this));
                    }
                }

                m_Children = children.ToArray();
            }
        }

        public ParentNode Parent
        {
            get
            {
                return m_Parent;
            }
        }

        public object[] Children
        {
            get
            {
                return m_Children;
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

    public class ChildNode
    {
        private ParentNode m_Parent;

        private string m_Name;
        private Point3D m_Location;

        public ChildNode(XmlTextReader xml, ParentNode parent)
        {
            m_Parent = parent;

            Parse(xml);
        }

        private void Parse(XmlTextReader xml)
        {
            if (xml.MoveToAttribute("name"))
                m_Name = xml.Value;
            else
                m_Name = "empty";

            int x = 0, y = 0, z = 0;

            if (xml.MoveToAttribute("x"))
                x = Utility.ToInt32(xml.Value);

            if (xml.MoveToAttribute("y"))
                y = Utility.ToInt32(xml.Value);

            if (xml.MoveToAttribute("z"))
                z = Utility.ToInt32(xml.Value);

            m_Location = new Point3D(x, y, z);
        }

        public ParentNode Parent
        {
            get
            {
                return m_Parent;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public Point3D Location
        {
            get
            {
                return m_Location;
            }
        }
    }
}