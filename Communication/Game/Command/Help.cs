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
        [Usage("Help")]
        [Description("Lists all available commands.")]
        public static void Help_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            List<CommandEntry> list = new List<CommandEntry>();

            foreach (CommandEntry entry in CommandSystem.Entries.Values)
                if (m.AccessLevel >= entry.AccessLevel)
                    list.Add(entry);

            list.Sort();

            StringBuilder sb = new StringBuilder();

            if (list.Count > 0)
                sb.Append(list[0].Command);

            for (int i = 1; i < list.Count; ++i)
            {
                string v = list[i].Command;

                if ((sb.Length + 1 + v.Length) >= 256)
                {
                    m.SendAsciiMessage(0x482, sb.ToString());
                    sb = new StringBuilder();
                    sb.Append(v);
                }
                else
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
            }

            if (sb.Length > 0)
                m.SendAsciiMessage(0x482, sb.ToString());
        }
    }
}