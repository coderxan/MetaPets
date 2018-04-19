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
    public class BringToPackCommand : BaseCommand
    {
        public BringToPackCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.AllItems;
            Commands = new string[] { "BringToPack" };
            ObjectTypes = ObjectTypes.Items;
            Usage = "BringToPack";
            Description = "Brings a targeted item to your backpack.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Item item = obj as Item;

            if (item != null)
            {
                if (e.Mobile.PlaceInBackpack(item))
                    AddResponse("The item has been placed in your backpack.");
                else
                    AddResponse("Your backpack could not hold the item.");
            }
        }
    }
}