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
    public class OpenBrowserCommand : BaseCommand
    {
        public OpenBrowserCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.AllMobiles;
            Commands = new string[] { "OpenBrowser", "OB" };
            ObjectTypes = ObjectTypes.Mobiles;
            Usage = "OpenBrowser <url>";
            Description = "Opens the web browser of a targeted player to a specified url.";
        }

        public static void OpenBrowser_Callback(Mobile from, bool okay, object state)
        {
            object[] states = (object[])state;
            Mobile gm = (Mobile)states[0];
            string url = (string)states[1];
            bool echo = (bool)states[2];

            if (okay)
            {
                if (echo)
                    gm.SendMessage("{0} : has opened their web browser to : {1}", from.Name, url);

                from.LaunchBrowser(url);
            }
            else
            {
                if (echo)
                    gm.SendMessage("{0} : has chosen not to open their web browser to : {1}", from.Name, url);

                from.SendMessage("You have chosen not to open your web browser.");
            }
        }

        public void Execute(CommandEventArgs e, object obj, bool echo)
        {
            if (e.Length == 1)
            {
                Mobile mob = (Mobile)obj;
                Mobile from = e.Mobile;

                if (mob.Player)
                {
                    NetState ns = mob.NetState;

                    if (ns == null)
                    {
                        LogFailure("That player is not online.");
                    }
                    else
                    {
                        string url = e.GetString(0);

                        CommandLogging.WriteLine(from, "{0} {1} requesting to open web browser of {2} to {3}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(mob), url);

                        if (echo)
                            AddResponse("Awaiting user confirmation...");
                        else
                            AddResponse("Open web browser request sent.");

                        mob.SendGump(new WarningGump(1060637, 30720, String.Format("A game master is requesting to open your web browser to the following URL:<br>{0}", url), 0xFFC000, 320, 240, new WarningGumpCallback(OpenBrowser_Callback), new object[] { from, url, echo }));
                    }
                }
                else
                {
                    LogFailure("That is not a player.");
                }
            }
            else
            {
                LogFailure("Format: OpenBrowser <url>");
            }
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Execute(e, obj, true);
        }

        public override void ExecuteList(CommandEventArgs e, ArrayList list)
        {
            for (int i = 0; i < list.Count; ++i)
                Execute(e, list[i], false);
        }
    }
}