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
        private static bool FixMap(ref Map map, ref Point3D loc, Item item)
        {
            if (map == null || map == Map.Internal)
            {
                Mobile m = item.RootParent as Mobile;

                return (m != null && FixMap(ref map, ref loc, m));
            }

            return true;
        }

        private static bool FixMap(ref Map map, ref Point3D loc, Mobile m)
        {
            if (map == null || map == Map.Internal)
            {
                map = m.LogoutMap;
                loc = m.LogoutLocation;
            }

            return (map != null && map != Map.Internal);
        }
    }
}