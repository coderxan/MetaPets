using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Server;
using Server.Accounting;
using Server.Commands.Generic;
using Server.Engines.Help;
using Server.Gumps;
using Server.Items;
using Server.Menus;
using Server.Menus.ItemLists;
using Server.Menus.Questions;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;
using Server.Targeting;
using Server.Targets;

namespace Server.Commands
{
    public partial class CommandHandlers
    {
        private class DismountTarget : Target
        {
            public DismountTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    CommandLogging.WriteLine(from, "{0} {1} dismounting {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targeted));

                    Mobile targ = (Mobile)targeted;

                    for (int i = 0; i < targ.Items.Count; ++i)
                    {
                        Item item = targ.Items[i];

                        if (item is IMountItem)
                        {
                            IMount mount = ((IMountItem)item).Mount;

                            if (mount != null)
                                mount.Rider = null;

                            if (targ.Items.IndexOf(item) == -1)
                                --i;
                        }
                    }

                    for (int i = 0; i < targ.Items.Count; ++i)
                    {
                        Item item = targ.Items[i];

                        if (item.Layer == Layer.Mount)
                        {
                            item.Delete();
                            --i;
                        }
                    }
                }
            }
        }
    }
}

namespace Server.Commands.Generic
{
    public class DismountCommand : BaseCommand
    {
        public DismountCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.AllMobiles;
            Commands = new string[] { "Dismount" };
            ObjectTypes = ObjectTypes.Mobiles;
            Usage = "Dismount";
            Description = "Forcefully dismounts a given target.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Mobile from = e.Mobile;
            Mobile mob = (Mobile)obj;

            CommandLogging.WriteLine(from, "{0} {1} dismounting {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(mob));

            bool takenAction = false;

            for (int i = 0; i < mob.Items.Count; ++i)
            {
                Item item = mob.Items[i];

                if (item is IMountItem)
                {
                    IMount mount = ((IMountItem)item).Mount;

                    if (mount != null)
                    {
                        mount.Rider = null;
                        takenAction = true;
                    }

                    if (mob.Items.IndexOf(item) == -1)
                        --i;
                }
            }

            for (int i = 0; i < mob.Items.Count; ++i)
            {
                Item item = mob.Items[i];

                if (item.Layer == Layer.Mount)
                {
                    takenAction = true;
                    item.Delete();
                    --i;
                }
            }

            if (takenAction)
                AddResponse("They have been dismounted.");
            else
                LogFailure("They were not mounted.");
        }
    }
}