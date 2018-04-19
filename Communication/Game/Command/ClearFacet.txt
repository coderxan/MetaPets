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
        [Usage("ClearFacet")]
        [Description("Deletes all items and mobiles in your facet. Players and their inventory will not be deleted.")]
        public static void ClearFacet_OnCommand(CommandEventArgs e)
        {
            Map map = e.Mobile.Map;

            if (map == null || map == Map.Internal)
            {
                e.Mobile.SendMessage("You may not run that command here.");
                return;
            }

            List<IEntity> list = new List<IEntity>();

            foreach (Item item in World.Items.Values)
                if (item.Map == map && item.Parent == null)
                    list.Add(item);

            foreach (Mobile m in World.Mobiles.Values)
                if (m.Map == map && !m.Player)
                    list.Add(m);

            if (list.Count > 0)
            {
                CommandLogging.WriteLine(e.Mobile, "{0} {1} starting facet clear of {2} ({3} object{4})", e.Mobile.AccessLevel, CommandLogging.Format(e.Mobile), map, list.Count, list.Count == 1 ? "" : "s");

                e.Mobile.SendGump(
                    new WarningGump(1060635, 30720,
                    String.Format("You are about to delete {0} object{1} from this facet.  Do you really wish to continue?",
                    list.Count, list.Count == 1 ? "" : "s"),
                    0xFFC000, 360, 260, new WarningGumpCallback(DeleteList_Callback), list));
            }
            else
            {
                e.Mobile.SendMessage("There were no objects found to delete.");
            }
        }

        public static void DeleteList_Callback(Mobile from, bool okay, object state)
        {
            if (okay)
            {
                List<IEntity> list = (List<IEntity>)state;

                CommandLogging.WriteLine(from, "{0} {1} deleting {2} object{3}", from.AccessLevel, CommandLogging.Format(from), list.Count, list.Count == 1 ? "" : "s");

                NetState.Pause();

                for (int i = 0; i < list.Count; ++i)
                    list[i].Delete();

                NetState.Resume();

                from.SendMessage("You have deleted {0} object{1}.", list.Count, list.Count == 1 ? "" : "s");
            }
            else
            {
                from.SendMessage("You have chosen not to delete those objects.");
            }
        }
    }
}