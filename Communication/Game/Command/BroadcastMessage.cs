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
        [Usage("BCast <text>")]
        [Aliases("B", "BC")]
        [Description("Broadcasts a message to everyone online.")]
        public static void BroadcastMessage_OnCommand(CommandEventArgs e)
        {
            BroadcastMessage(AccessLevel.Player, 0x482, String.Format("Staff message from {0}:", e.Mobile.Name));
            BroadcastMessage(AccessLevel.Player, 0x482, e.ArgString);
        }

        public static void BroadcastMessage(AccessLevel ac, int hue, string message)
        {
            foreach (NetState state in NetState.Instances)
            {
                Mobile m = state.Mobile;

                if (m != null && m.AccessLevel >= ac)
                    m.SendMessage(hue, message);
            }
        }
    }
}