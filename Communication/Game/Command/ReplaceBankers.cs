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
        [Usage("ReplaceBankers")]
        [Description("Tells the commanding player replaces all bankers in his coordinates, region, and facet.")]
        public static void ReplaceBankers_OnCommand(CommandEventArgs e)
        {
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile m in World.Mobiles.Values)
                if ((m is Banker) && !(m is BaseCreature))
                    list.Add(m);

            foreach (Mobile m in list)
            {
                Map map = m.Map;

                if (map != null)
                {
                    bool hasBankerSpawner = false;

                    foreach (Item item in m.GetItemsInRange(0))
                    {
                        if (item is Spawner)
                        {
                            Spawner spawner = (Spawner)item;

                            for (int i = 0; !hasBankerSpawner && i < spawner.SpawnNames.Count; ++i)
                                hasBankerSpawner = Insensitive.Equals((string)spawner.SpawnNames[i], "banker");

                            if (hasBankerSpawner)
                                break;
                        }
                    }

                    if (!hasBankerSpawner)
                    {
                        Spawner spawner = new Spawner(1, 1, 5, 0, 4, "banker");

                        spawner.MoveToWorld(m.Location, map);
                    }
                }
            }
        }
    }
}