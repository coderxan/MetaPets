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
    public class IncreaseCommand : BaseCommand
    {
        public IncreaseCommand()
        {
            AccessLevel = AccessLevel.Counselor;
            Supports = CommandSupport.All;
            Commands = new string[] { "Increase", "Inc" };
            ObjectTypes = ObjectTypes.Both;
            Usage = "Increase {<propertyName> <offset> ...}";
            Description = "Increases the value of a specified property by the specified offset.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            if (obj is BaseMulti)
            {
                LogFailure("This command does not work on multis.");
            }
            else if (e.Length >= 2)
            {
                string result = Properties.IncreaseValue(e.Mobile, obj, e.Arguments);

                if (result == "The property has been increased." || result == "The properties have been increased." || result == "The property has been decreased." || result == "The properties have been decreased." || result == "The properties have been changed.")
                    AddResponse(result);
                else
                    LogFailure(result);
            }
            else
            {
                LogFailure("Format: Increase {<propertyName> <offset> ...}");
            }
        }
    }
}