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
    public class TellCommand : BaseCommand
    {
        private bool m_InGump;

        public TellCommand(bool inGump)
        {
            m_InGump = inGump;

            AccessLevel = AccessLevel.Counselor;
            Supports = CommandSupport.AllMobiles;
            ObjectTypes = ObjectTypes.Mobiles;

            if (inGump)
            {
                Commands = new string[] { "Message", "Msg" };
                Usage = "Message \"text\"";
                Description = "Sends a message to a targeted player.";
            }
            else
            {
                Commands = new string[] { "Tell" };
                Usage = "Tell \"text\"";
                Description = "Sends a system message to a targeted player.";
            }
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Mobile mob = (Mobile)obj;
            Mobile from = e.Mobile;

            CommandLogging.WriteLine(from, "{0} {1} {2} {3} \"{4}\"", from.AccessLevel, CommandLogging.Format(from), m_InGump ? "messaging" : "telling", CommandLogging.Format(mob), e.ArgString);

            if (m_InGump)
                mob.SendGump(new MessageSentGump(mob, from.Name, e.ArgString));
            else
                mob.SendMessage(e.ArgString);
        }
    }
}