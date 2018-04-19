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
    public class CountCommand : BaseCommand
    {
        public CountCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.Complex;
            Commands = new string[] { "Count" };
            ObjectTypes = ObjectTypes.All;
            Usage = "Count";
            Description = "Counts the number of objects that a command modifier would use. Generally used with condition arguments.";
            ListOptimized = true;
        }

        public override void ExecuteList(CommandEventArgs e, ArrayList list)
        {
            if (list.Count == 1)
                AddResponse("There is one matching object.");
            else
                AddResponse(String.Format("There are {0} matching objects.", list.Count));
        }
    }
}