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
    public class HideCommand : BaseCommand
    {
        private bool m_Value;

        public HideCommand(bool value)
        {
            m_Value = value;

            AccessLevel = AccessLevel.Counselor;
            Supports = CommandSupport.AllMobiles;
            Commands = new string[] { value ? "Hide" : "Unhide" };
            ObjectTypes = ObjectTypes.Mobiles;

            if (value)
            {
                Usage = "Hide";
                Description = "Makes a targeted mobile disappear in a puff of smoke.";
            }
            else
            {
                Usage = "Unhide";
                Description = "Makes a targeted mobile appear in a puff of smoke.";
            }
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Mobile m = (Mobile)obj;

            CommandLogging.WriteLine(e.Mobile, "{0} {1} {2} {3}", e.Mobile.AccessLevel, CommandLogging.Format(e.Mobile), m_Value ? "hiding" : "unhiding", CommandLogging.Format(m));

            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y, m.Z + 4), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y, m.Z), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y, m.Z - 4), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y + 1, m.Z + 4), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y + 1, m.Z), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y + 1, m.Z - 4), m.Map, 0x3728, 13);

            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z + 11), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z + 7), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z + 3), m.Map, 0x3728, 13);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z - 1), m.Map, 0x3728, 13);

            m.PlaySound(0x228);
            m.Hidden = m_Value;

            if (m_Value)
                AddResponse("They have been hidden.");
            else
                AddResponse("They have been revealed.");
        }
    }
}