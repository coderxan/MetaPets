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
        [Usage("Stats")]
        [Description("View some stats about the server.")]
        public static void Stats_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Open Connections: {0}", Network.NetState.Instances.Count);
            e.Mobile.SendMessage("Mobiles: {0}", World.Mobiles.Count);
            e.Mobile.SendMessage("Items: {0}", World.Items.Count);
        }
    }
}