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
    public class AddToPackCommand : BaseCommand
    {
        public AddToPackCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.All;
            Commands = new string[] { "AddToPack", "AddToCont" };
            ObjectTypes = ObjectTypes.Both;
            ListOptimized = true;
            Usage = "AddToPack <name> [params] [set {<propertyName> <value> ...}]";
            Description = "Adds an item by name to the backpack of a targeted player or npc, or a targeted container. Optional constructor parameters. Optional set property list.";
        }

        public override void ExecuteList(CommandEventArgs e, ArrayList list)
        {
            if (e.Arguments.Length == 0)
                return;

            List<Container> packs = new List<Container>(list.Count);

            for (int i = 0; i < list.Count; ++i)
            {
                object obj = list[i];
                Container cont = null;

                if (obj is Mobile)
                    cont = ((Mobile)obj).Backpack;
                else if (obj is Container)
                    cont = (Container)obj;

                if (cont != null)
                    packs.Add(cont);
                else
                    LogFailure("That is not a container.");
            }

            Add.Invoke(e.Mobile, e.Mobile.Location, e.Mobile.Location, e.Arguments, packs);
        }
    }
}