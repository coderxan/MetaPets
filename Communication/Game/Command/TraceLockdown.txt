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
    public class TraceLockdownCommand : BaseCommand
    {
        public TraceLockdownCommand()
        {
            AccessLevel = AccessLevel.Administrator;
            Supports = CommandSupport.Simple;
            Commands = new string[] { "TraceLockdown" };
            ObjectTypes = ObjectTypes.Items;
            Usage = "TraceLockdown";
            Description = "Finds the BaseHouse for which a targeted item is locked down or secured.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Item item = obj as Item;

            if (item == null)
                return;

            if (!item.IsLockedDown && !item.IsSecure)
            {
                LogFailure("That is not locked down.");
                return;
            }

            foreach (BaseHouse house in BaseHouse.AllHouses)
            {
                if (house.IsSecure(item) || house.IsLockedDown(item))
                {
                    e.Mobile.SendGump(new PropertiesGump(e.Mobile, house));
                    return;
                }
            }

            LogFailure("No house was found.");
        }
    }
}