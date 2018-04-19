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
        [Usage("Stuck")]
        [Description("Opens a menu of towns, used for teleporting stuck mobiles.")]
        public static void Stuck_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new StuckMenuTarget();
        }

        private class StuckMenuTarget : Target
        {
            public StuckMenuTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    if (((Mobile)targeted).AccessLevel >= from.AccessLevel && targeted != from)
                        from.SendMessage("You can't do that to someone with higher Accesslevel than you!");
                    else
                        from.SendGump(new StuckMenu(from, (Mobile)targeted, false));
                }
            }
        }
    }
}