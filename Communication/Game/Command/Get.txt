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
    public class GetCommand : BaseCommand
    {
        public GetCommand()
        {
            AccessLevel = AccessLevel.Counselor;
            Supports = CommandSupport.All;
            Commands = new string[] { "Get" };
            ObjectTypes = ObjectTypes.All;
            Usage = "Get <propertyName>";
            Description = "Gets one or more property values by name of a targeted object.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            if (e.Length >= 1)
            {
                for (int i = 0; i < e.Length; ++i)
                {
                    string result = Properties.GetValue(e.Mobile, obj, e.GetString(i));

                    if (result == "Property not found." || result == "Property is write only." || result.StartsWith("Getting this property"))
                        LogFailure(result);
                    else
                        AddResponse(result);
                }
            }
            else
            {
                LogFailure("Format: Get <propertyName>");
            }
        }
    }
}