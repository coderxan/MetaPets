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
        [Usage("SMsg <text>")]
        [Aliases("S", "SM")]
        [Description("Broadcasts a message to all online staff.")]
        public static void StaffMessage_OnCommand(CommandEventArgs e)
        {
            BroadcastMessage(AccessLevel.Counselor, e.Mobile.SpeechHue, String.Format("[{0}] {1}", e.Mobile.Name, e.ArgString));
        }
    }
}