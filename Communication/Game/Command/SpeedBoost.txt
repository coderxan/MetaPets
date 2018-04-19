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
        [Usage("SpeedBoost [true|false]")]
        [Description("Enables a speed boost for the invoker.  Disable with paramaters.")]
        private static void SpeedBoost_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length <= 1)
            {
                if (e.Length == 1 && !e.GetBoolean(0))
                {
                    from.Send(SpeedControl.Disable);
                    from.SendMessage("Speed boost has been disabled.");
                }
                else
                {
                    from.Send(SpeedControl.MountSpeed);
                    from.SendMessage("Speed boost has been enabled.");
                }
            }
            else
            {
                from.SendMessage("Format: SpeedBoost [true|false]");
            }
        }
    }
}