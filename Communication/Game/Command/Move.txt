using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Server;
using Server.Accounting;
using Server.Commands;
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
        [Usage("Move")]
        [Description("Repositions a targeted item or mobile.")]
        private static void Move_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new PickMoveTarget();
        }
    }
}

namespace Server.Targets
{
    public class PickMoveTarget : Target
    {
        public PickMoveTarget()
            : base(-1, false, TargetFlags.None)
        {
        }

        protected override void OnTarget(Mobile from, object o)
        {
            if (!BaseCommand.IsAccessible(from, o))
            {
                from.SendMessage("That is not accessible.");
                return;
            }

            if (o is Item || o is Mobile)
                from.Target = new MoveTarget(o);
        }
    }

    public class MoveTarget : Target
    {
        private object m_Object;

        public MoveTarget(object o)
            : base(-1, true, TargetFlags.None)
        {
            m_Object = o;
        }

        protected override void OnTarget(Mobile from, object o)
        {
            IPoint3D p = o as IPoint3D;

            if (p != null)
            {
                if (!BaseCommand.IsAccessible(from, m_Object))
                {
                    from.SendMessage("That is not accessible.");
                    return;
                }

                if (p is Item)
                    p = ((Item)p).GetWorldTop();

                CommandLogging.WriteLine(from, "{0} {1} moving {2} to {3}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(m_Object), new Point3D(p));

                if (m_Object is Item)
                {
                    Item item = (Item)m_Object;

                    if (!item.Deleted)
                        item.MoveToWorld(new Point3D(p), from.Map);
                }
                else if (m_Object is Mobile)
                {
                    Mobile m = (Mobile)m_Object;

                    if (!m.Deleted)
                        m.MoveToWorld(new Point3D(p), from.Map);
                }
            }
        }
    }
}