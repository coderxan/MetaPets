using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using Server;
using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.BulkOrders;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis.Deeds;
using Server.Network;
using Server.Regions;
using Server.Targeting;

namespace Server.Multis
{
    public class TempNoHousingRegion : BaseRegion
    {
        private Mobile m_RegionOwner;

        public TempNoHousingRegion(BaseHouse house, Mobile regionowner)
            : base(null, house.Map, Region.DefaultPriority, house.Region.Area)
        {
            Register();

            m_RegionOwner = regionowner;

            Timer.DelayCall(house.RestrictedPlacingTime, Unregister);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return (from == m_RegionOwner || AccountHandler.CheckAccount(from, m_RegionOwner));
        }
    }
}

namespace Server.Regions
{
    public class NoHousingRegion : BaseRegion
    {
        /// <summary>
        /// False: this uses 'stupid OSI' house placement checking: part of the house may be placed here provided that the center is not in the region
        /// True: this uses 'smart RunUO' house placement checking: no part of the house may be in the region
        /// </summary>

        private bool m_SmartChecking;

        public bool SmartChecking { get { return m_SmartChecking; } }

        public NoHousingRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            ReadBoolean(xml["smartNoHousing"], "active", ref m_SmartChecking, false);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return m_SmartChecking;
        }
    }
}