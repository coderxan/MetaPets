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
        [Usage("Echo <text>")]
        [Description("Relays (text) as a system message.")]
        public static void Echo_OnCommand(CommandEventArgs e)
        {
            string toEcho = e.ArgString.Trim();

            if (toEcho.Length > 0)
                e.Mobile.SendMessage(toEcho);
            else
                e.Mobile.SendMessage("Format: Echo \"<text>\"");
        }
    }
}