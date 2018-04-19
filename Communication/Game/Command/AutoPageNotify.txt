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
        [Usage("AutoPageNotify")]
        [Aliases("APN")]
        [Description("Toggles your auto-page-notify status.")]
        public static void APN_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            m.AutoPageNotify = !m.AutoPageNotify;

            m.SendMessage("Your auto-page-notify has been turned {0}.", m.AutoPageNotify ? "on" : "off");
        }
    }
}