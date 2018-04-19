using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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
        [Usage("ViewEquip")]
        [Description("Lists equipment of a targeted mobile. From the list you can move, delete, or open props.")]
        public static void ViewEquip_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new ViewEqTarget();
        }

        private class ViewEqTarget : Target
        {
            public ViewEqTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (!BaseCommand.IsAccessible(from, targeted))
                {
                    from.SendMessage("That is not accessible.");
                    return;
                }

                if (targeted is Mobile)
                    from.SendMenu(new EquipMenu(from, (Mobile)targeted, GetEquip((Mobile)targeted)));
            }

            private static ItemListEntry[] GetEquip(Mobile m)
            {
                ItemListEntry[] entries = new ItemListEntry[m.Items.Count];

                for (int i = 0; i < m.Items.Count; ++i)
                {
                    Item item = m.Items[i];

                    entries[i] = new ItemListEntry(String.Format("{0}: {1}", item.Layer, item.GetType().Name), item.ItemID, item.Hue);
                }

                return entries;
            }

            private class EquipMenu : ItemListMenu
            {
                private Mobile m_Mobile;

                public EquipMenu(Mobile from, Mobile m, ItemListEntry[] entries)
                    : base("Equipment", entries)
                {
                    m_Mobile = m;

                    CommandLogging.WriteLine(from, "{0} {1} viewing equipment of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(m));
                }

                public override void OnResponse(NetState state, int index)
                {
                    if (index >= 0 && index < m_Mobile.Items.Count)
                    {
                        Item item = m_Mobile.Items[index];

                        state.Mobile.SendMenu(new EquipDetailsMenu(m_Mobile, item));
                    }
                }

                private class EquipDetailsMenu : QuestionMenu
                {
                    private Mobile m_Mobile;
                    private Item m_Item;

                    public EquipDetailsMenu(Mobile m, Item item)
                        : base(String.Format("{0}: {1}", item.Layer, item.GetType().Name), new string[] { "Move", "Delete", "Props" })
                    {
                        m_Mobile = m;
                        m_Item = item;
                    }

                    public override void OnCancel(NetState state)
                    {
                        state.Mobile.SendMenu(new EquipMenu(state.Mobile, m_Mobile, ViewEqTarget.GetEquip(m_Mobile)));
                    }

                    public override void OnResponse(NetState state, int index)
                    {
                        if (index == 0)
                        {
                            CommandLogging.WriteLine(state.Mobile, "{0} {1} moving equipment item {2} of {3}", state.Mobile.AccessLevel, CommandLogging.Format(state.Mobile), CommandLogging.Format(m_Item), CommandLogging.Format(m_Mobile));
                            state.Mobile.Target = new MoveTarget(m_Item);
                        }
                        else if (index == 1)
                        {
                            CommandLogging.WriteLine(state.Mobile, "{0} {1} deleting equipment item {2} of {3}", state.Mobile.AccessLevel, CommandLogging.Format(state.Mobile), CommandLogging.Format(m_Item), CommandLogging.Format(m_Mobile));
                            m_Item.Delete();
                        }
                        else if (index == 2)
                        {
                            CommandLogging.WriteLine(state.Mobile, "{0} {1} opening properties for equipment item {2} of {3}", state.Mobile.AccessLevel, CommandLogging.Format(state.Mobile), CommandLogging.Format(m_Item), CommandLogging.Format(m_Mobile));
                            state.Mobile.SendGump(new PropertiesGump(state.Mobile, m_Item));
                        }
                    }
                }
            }
        }
    }
}