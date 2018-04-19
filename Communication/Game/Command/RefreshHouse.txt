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
    public class RefreshHouseCommand : BaseCommand
    {
        public RefreshHouseCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.Simple;
            Commands = new string[] { "RefreshHouse" };
            ObjectTypes = ObjectTypes.Items;
            Usage = "RefreshHouse";
            Description = "Refreshes a targeted house sign.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            if (obj is HouseSign)
            {
                BaseHouse house = ((HouseSign)obj).Owner;

                if (house == null)
                {
                    LogFailure("That sign has no house attached.");
                }
                else
                {
                    house.RefreshDecay();
                    AddResponse("The house has been refreshed.");
                }
            }
            else
            {
                LogFailure("That is not a house sign.");
            }
        }
    }
}