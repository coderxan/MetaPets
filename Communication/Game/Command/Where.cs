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
        [Usage("Where")]
        [Description("Tells the commanding player his coordinates, region, and facet.")]
        public static void Where_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            Map map = from.Map;

            from.SendMessage("You are at {0} {1} {2} in {3}.", from.X, from.Y, from.Z, map);

            if (map != null)
            {
                Region reg = from.Region;

                if (!reg.IsDefault)
                {
                    StringBuilder builder = new StringBuilder();

                    builder.Append(reg.ToString());
                    reg = reg.Parent;

                    while (reg != null)
                    {
                        builder.Append(" <- " + reg.ToString());
                        reg = reg.Parent;
                    }

                    from.SendMessage("Your region is {0}.", builder.ToString());
                }
            }
        }
    }
}