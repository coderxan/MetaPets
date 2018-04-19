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
    public class PrivSoundCommand : BaseCommand
    {
        public PrivSoundCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.AllMobiles;
            Commands = new string[] { "PrivSound" };
            ObjectTypes = ObjectTypes.Mobiles;
            Usage = "PrivSound <index>";
            Description = "Plays a sound to a given target.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Mobile from = e.Mobile;

            if (e.Length == 1)
            {
                int index = e.GetInt32(0);
                Mobile mob = (Mobile)obj;

                CommandLogging.WriteLine(from, "{0} {1} playing sound {2} for {3}", from.AccessLevel, CommandLogging.Format(from), index, CommandLogging.Format(mob));
                mob.Send(new PlaySound(index, mob.Location));
            }
            else
            {
                from.SendMessage("Format: PrivSound <index>");
            }
        }
    }
}