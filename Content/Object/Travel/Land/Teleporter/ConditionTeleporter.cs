using System;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public class ConditionTeleporter : Teleporter
    {
        [Flags]
        protected enum ConditionFlag
        {
            None = 0x000,
            DenyMounted = 0x001,
            DenyFollowers = 0x002,
            DenyPackContents = 0x004,
            DenyHolding = 0x008,
            DenyEquipment = 0x010,
            DenyTransformed = 0x020,
            StaffOnly = 0x040,
            DenyPackEthereals = 0x080,
            DeadOnly = 0x100
        }

        private ConditionFlag m_Flags;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyMounted
        {
            get { return GetFlag(ConditionFlag.DenyMounted); }
            set { SetFlag(ConditionFlag.DenyMounted, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyFollowers
        {
            get { return GetFlag(ConditionFlag.DenyFollowers); }
            set { SetFlag(ConditionFlag.DenyFollowers, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyPackContents
        {
            get { return GetFlag(ConditionFlag.DenyPackContents); }
            set { SetFlag(ConditionFlag.DenyPackContents, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyHolding
        {
            get { return GetFlag(ConditionFlag.DenyHolding); }
            set { SetFlag(ConditionFlag.DenyHolding, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyEquipment
        {
            get { return GetFlag(ConditionFlag.DenyEquipment); }
            set { SetFlag(ConditionFlag.DenyEquipment, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyTransformed
        {
            get { return GetFlag(ConditionFlag.DenyTransformed); }
            set { SetFlag(ConditionFlag.DenyTransformed, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool StaffOnly
        {
            get { return GetFlag(ConditionFlag.StaffOnly); }
            set { SetFlag(ConditionFlag.StaffOnly, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DenyPackEthereals
        {
            get { return GetFlag(ConditionFlag.DenyPackEthereals); }
            set { SetFlag(ConditionFlag.DenyPackEthereals, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DeadOnly
        {
            get { return GetFlag(ConditionFlag.DeadOnly); }
            set { SetFlag(ConditionFlag.DeadOnly, value); InvalidateProperties(); }
        }

        public override bool CanTeleport(Mobile m)
        {
            if (!base.CanTeleport(m))
                return false;

            if (GetFlag(ConditionFlag.StaffOnly) && m.AccessLevel < AccessLevel.Counselor)
                return false;

            if (GetFlag(ConditionFlag.DenyMounted) && m.Mounted)
            {
                m.SendLocalizedMessage(1077252); // You must dismount before proceeding.
                return false;
            }

            if (GetFlag(ConditionFlag.DenyFollowers) && (m.Followers != 0 || (m is PlayerMobile && ((PlayerMobile)m).AutoStabled.Count != 0)))
            {
                m.SendLocalizedMessage(1077250); // No pets permitted beyond this point.
                return false;
            }

            Container pack = m.Backpack;

            if (pack != null)
            {
                if (GetFlag(ConditionFlag.DenyPackContents) && pack.TotalItems != 0)
                {
                    m.SendMessage("You must empty your backpack before proceeding.");
                    return false;
                }

                if (GetFlag(ConditionFlag.DenyPackEthereals) && (pack.FindItemByType(typeof(EtherealMount)) != null || pack.FindItemByType(typeof(BaseImprisonedMobile)) != null))
                {
                    m.SendMessage("You must empty your backpack of ethereal mounts before proceeding.");
                    return false;
                }
            }

            if (GetFlag(ConditionFlag.DenyHolding) && m.Holding != null)
            {
                m.SendMessage("You must let go of what you are holding before proceeding.");
                return false;
            }

            if (GetFlag(ConditionFlag.DenyEquipment))
            {
                foreach (Item item in m.Items)
                {
                    switch (item.Layer)
                    {
                        case Layer.Hair:
                        case Layer.FacialHair:
                        case Layer.Backpack:
                        case Layer.Mount:
                        case Layer.Bank:
                            {
                                continue; // ignore
                            }
                        default:
                            {
                                m.SendMessage("You must remove all of your equipment before proceeding.");
                                return false;
                            }
                    }
                }
            }

            if (GetFlag(ConditionFlag.DenyTransformed) && m.IsBodyMod)
            {
                m.SendMessage("You cannot go there in this form.");
                return false;
            }

            if (GetFlag(ConditionFlag.DeadOnly) && m.Alive)
            {
                m.SendLocalizedMessage(1060014); // Only the dead may pass.
                return false;
            }

            return true;
        }

        [Constructable]
        public ConditionTeleporter()
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            StringBuilder props = new StringBuilder();

            if (GetFlag(ConditionFlag.DenyMounted))
                props.Append("<BR>Deny Mounted");

            if (GetFlag(ConditionFlag.DenyFollowers))
                props.Append("<BR>Deny Followers");

            if (GetFlag(ConditionFlag.DenyPackContents))
                props.Append("<BR>Deny Pack Contents");

            if (GetFlag(ConditionFlag.DenyPackEthereals))
                props.Append("<BR>Deny Pack Ethereals");

            if (GetFlag(ConditionFlag.DenyHolding))
                props.Append("<BR>Deny Holding");

            if (GetFlag(ConditionFlag.DenyEquipment))
                props.Append("<BR>Deny Equipment");

            if (GetFlag(ConditionFlag.DenyTransformed))
                props.Append("<BR>Deny Transformed");

            if (GetFlag(ConditionFlag.StaffOnly))
                props.Append("<BR>Staff Only");

            if (GetFlag(ConditionFlag.DeadOnly))
                props.Append("<BR>Dead Only");

            if (props.Length != 0)
            {
                props.Remove(0, 4);
                list.Add(props.ToString());
            }
        }

        public ConditionTeleporter(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_Flags);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Flags = (ConditionFlag)reader.ReadInt();
        }

        protected bool GetFlag(ConditionFlag flag)
        {
            return ((m_Flags & flag) != 0);
        }

        protected void SetFlag(ConditionFlag flag, bool value)
        {
            if (value)
                m_Flags |= flag;
            else
                m_Flags &= ~flag;
        }
    }
}