using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Server;
using Server.Accounting;
using Server.Engines.Help;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Commands.Generic
{
    public class DeleteCommand : BaseCommand
    {
        public DeleteCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.AllNPCs | CommandSupport.AllItems;
            Commands = new string[] { "Delete", "Remove" };
            ObjectTypes = ObjectTypes.Both;
            Usage = "Delete";
            Description = "Deletes a targeted item or mobile. Does not delete players.";
        }

        private void OnConfirmCallback(Mobile from, bool okay, object state)
        {
            object[] states = (object[])state;
            CommandEventArgs e = (CommandEventArgs)states[0];
            ArrayList list = (ArrayList)states[1];

            bool flushToLog = false;

            if (okay)
            {
                AddResponse("Delete command confirmed.");

                if (list.Count > 20)
                {
                    CommandLogging.Enabled = false;
                    NetState.Pause();
                }

                base.ExecuteList(e, list);

                if (list.Count > 20)
                {
                    NetState.Resume();
                    flushToLog = true;
                    CommandLogging.Enabled = true;
                }
            }
            else
            {
                AddResponse("Delete command aborted.");
            }

            Flush(from, flushToLog);
        }

        public override void ExecuteList(CommandEventArgs e, ArrayList list)
        {
            if (list.Count > 1)
            {
                e.Mobile.SendGump(new WarningGump(1060637, 30720, String.Format("You are about to delete {0} objects. This cannot be undone without a full server revert.<br><br>Continue?", list.Count), 0xFFC000, 420, 280, new WarningGumpCallback(OnConfirmCallback), new object[] { e, list }));
                AddResponse("Awaiting confirmation...");
            }
            else
            {
                base.ExecuteList(e, list);
            }
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            if (obj is Item)
            {
                CommandLogging.WriteLine(e.Mobile, "{0} {1} deleting {2}", e.Mobile.AccessLevel, CommandLogging.Format(e.Mobile), CommandLogging.Format(obj));
                ((Item)obj).Delete();
                AddResponse("The item has been deleted.");
            }
            else if (obj is Mobile && !((Mobile)obj).Player)
            {
                CommandLogging.WriteLine(e.Mobile, "{0} {1} deleting {2}", e.Mobile.AccessLevel, CommandLogging.Format(e.Mobile), CommandLogging.Format(obj));
                ((Mobile)obj).Delete();
                AddResponse("The mobile has been deleted.");
            }
            else
            {
                LogFailure("That cannot be deleted.");
            }
        }
    }
}