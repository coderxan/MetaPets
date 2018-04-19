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
    public class FirewallCommand : BaseCommand
    {
        public FirewallCommand()
        {
            AccessLevel = AccessLevel.Administrator;
            Supports = CommandSupport.AllMobiles;
            Commands = new string[] { "Firewall" };
            ObjectTypes = ObjectTypes.Mobiles;
            Usage = "Firewall";
            Description = "Adds a targeted player to the firewall (list of blocked IP addresses). This command does not ban or kick.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Mobile from = e.Mobile;
            Mobile targ = (Mobile)obj;
            NetState state = targ.NetState;

            if (state != null)
            {
                CommandLogging.WriteLine(from, "{0} {1} firewalling {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targ));

                try
                {
                    Firewall.Add(state.Address);
                    AddResponse("They have been firewalled.");
                }
                catch (Exception ex)
                {
                    LogFailure(ex.Message);
                }
            }
            else
            {
                LogFailure("They are not online.");
            }
        }
    }
}