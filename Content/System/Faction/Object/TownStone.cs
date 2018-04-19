using System;

using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Factions
{
    public class TownStone : BaseSystemController
    {
        private Town m_Town;

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public Town Town
        {
            get { return m_Town; }
            set
            {
                m_Town = value;

                AssignName(m_Town == null ? null : m_Town.Definition.TownStoneName);
            }
        }

        public override string DefaultName { get { return "faction town stone"; } }

        [Constructable]
        public TownStone()
            : this(null)
        {
        }

        [Constructable]
        public TownStone(Town town)
            : base(0xEDE)
        {
            Movable = false;
            Town = town;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Town == null)
                return;

            Faction faction = Faction.Find(from);

            if (faction == null && from.AccessLevel < AccessLevel.GameMaster)
                return; // TODO: Message?

            if (m_Town.Owner == null || (from.AccessLevel < AccessLevel.GameMaster && faction != m_Town.Owner))
                from.SendLocalizedMessage(1010332); // Your faction does not control this town
            else if (!m_Town.Owner.IsCommander(from))
                from.SendLocalizedMessage(1005242); // Only faction Leaders can use townstones
            else if (FactionGump.Exists(from))
                from.SendLocalizedMessage(1042160); // You already have a faction menu open.
            else if (from is PlayerMobile)
                from.SendGump(new TownStoneGump((PlayerMobile)from, m_Town.Owner, m_Town));
        }

        public TownStone(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Town.WriteReference(writer, m_Town);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        Town = Town.ReadReference(reader);
                        break;
                    }
            }
        }
    }

    public class TownStoneGump : FactionGump
    {
        private PlayerMobile m_From;
        private Faction m_Faction;
        private Town m_Town;

        public TownStoneGump(PlayerMobile from, Faction faction, Town town)
            : base(50, 50)
        {
            m_From = from;
            m_Faction = faction;
            m_Town = town;

            AddPage(0);

            AddBackground(0, 0, 320, 250, 5054);
            AddBackground(10, 10, 300, 230, 3000);

            AddHtmlText(25, 30, 250, 25, town.Definition.TownStoneHeader, false, false);

            AddHtmlLocalized(55, 60, 150, 25, 1011557, false, false); // Hire Sheriff
            AddButton(20, 60, 4005, 4007, 1, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 90, 150, 25, 1011559, false, false); // Hire Finance Minister
            AddButton(20, 90, 4005, 4007, 2, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 120, 150, 25, 1011558, false, false); // Fire Sheriff
            AddButton(20, 120, 4005, 4007, 3, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 150, 150, 25, 1011560, false, false); // Fire Finance Minister
            AddButton(20, 150, 4005, 4007, 4, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 210, 150, 25, 1011441, false, false); // EXIT
            AddButton(20, 210, 4005, 4007, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Town.Owner != m_Faction || !m_Faction.IsCommander(m_From))
            {
                m_From.SendLocalizedMessage(1010339); // You no longer control this city
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // hire sheriff
                    {
                        if (m_Town.Sheriff != null)
                        {
                            m_From.SendLocalizedMessage(1010342); // You must fire your Sheriff before you can elect a new one
                        }
                        else
                        {
                            m_From.SendLocalizedMessage(1010347); // Who shall be your new sheriff
                            m_From.BeginTarget(12, false, TargetFlags.None, new TargetCallback(HireSheriff_OnTarget));
                        }

                        break;
                    }
                case 2: // hire finance minister
                    {
                        if (m_Town.Finance != null)
                        {
                            m_From.SendLocalizedMessage(1010345); // You must fire your finance minister before you can elect a new one
                        }
                        else
                        {
                            m_From.SendLocalizedMessage(1010348); // Who shall be your new Minister of Finances?
                            m_From.BeginTarget(12, false, TargetFlags.None, new TargetCallback(HireFinanceMinister_OnTarget));
                        }

                        break;
                    }
                case 3: // fire sheriff
                    {
                        if (m_Town.Sheriff == null)
                        {
                            m_From.SendLocalizedMessage(1010350); // You need to elect a sheriff before you can fire one
                        }
                        else
                        {
                            m_From.SendLocalizedMessage(1010349); // You have fired your sheriff
                            m_Town.Sheriff.SendLocalizedMessage(1010270); // You have been fired as Sheriff
                            m_Town.Sheriff = null;
                        }

                        break;
                    }
                case 4: // fire finance minister
                    {
                        if (m_Town.Finance == null)
                        {
                            m_From.SendLocalizedMessage(1010352); // You need to elect a financial minister before you can fire one
                        }
                        else
                        {
                            m_From.SendLocalizedMessage(1010351); // You have fired your financial Minister
                            m_Town.Finance.SendLocalizedMessage(1010151); // You have been fired as Finance Minister
                            m_Town.Finance = null;
                        }

                        break;
                    }
            }
        }

        private void HireSheriff_OnTarget(Mobile from, object obj)
        {
            if (m_Town.Owner != m_Faction || !m_Faction.IsCommander(from))
            {
                from.SendLocalizedMessage(1010339); // You no longer control this city
                return;
            }
            else if (m_Town.Sheriff != null)
            {
                from.SendLocalizedMessage(1010342); // You must fire your Sheriff before you can elect a new one
            }
            else if (obj is Mobile)
            {
                Mobile targ = (Mobile)obj;
                PlayerState pl = PlayerState.Find(targ);

                if (pl == null)
                {
                    from.SendLocalizedMessage(1010337); // You must pick someone in a faction
                }
                else if (pl.Faction != m_Faction)
                {
                    from.SendLocalizedMessage(1010338); // You must pick someone in the correct faction
                }
                else if (m_Faction.Commander == targ)
                {
                    from.SendLocalizedMessage(1010335); // You cannot elect a commander to a town position
                }
                else if (pl.Sheriff != null || pl.Finance != null)
                {
                    from.SendLocalizedMessage(1005245); // You must pick someone who does not already hold a city post
                }
                else
                {
                    m_Town.Sheriff = targ;
                    targ.SendLocalizedMessage(1010340); // You are now the Sheriff
                    from.SendLocalizedMessage(1010341); // You have elected a Sheriff
                }
            }
            else
            {
                from.SendLocalizedMessage(1010334); // You must select a player to hold a city position!
            }
        }

        private void HireFinanceMinister_OnTarget(Mobile from, object obj)
        {
            if (m_Town.Owner != m_Faction || !m_Faction.IsCommander(from))
            {
                from.SendLocalizedMessage(1010339); // You no longer control this city
                return;
            }
            else if (m_Town.Finance != null)
            {
                from.SendLocalizedMessage(1010342); // You must fire your Sheriff before you can elect a new one
            }
            else if (obj is Mobile)
            {
                Mobile targ = (Mobile)obj;
                PlayerState pl = PlayerState.Find(targ);

                if (pl == null)
                {
                    from.SendLocalizedMessage(1010337); // You must pick someone in a faction
                }
                else if (pl.Faction != m_Faction)
                {
                    from.SendLocalizedMessage(1010338); // You must pick someone in the correct faction
                }
                else if (m_Faction.Commander == targ)
                {
                    from.SendLocalizedMessage(1010335); // You cannot elect a commander to a town position
                }
                else if (pl.Sheriff != null || pl.Finance != null)
                {
                    from.SendLocalizedMessage(1005245); // You must pick someone who does not already hold a city post
                }
                else
                {
                    m_Town.Finance = targ;
                    targ.SendLocalizedMessage(1010343); // You are now the Financial Minister
                    from.SendLocalizedMessage(1010344); // You have elected a Financial Minister
                }
            }
            else
            {
                from.SendLocalizedMessage(1010334); // You must select a player to hold a city position!
            }
        }
    }
}