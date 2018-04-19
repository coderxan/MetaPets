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
        [Usage("Bank")]
        [Description("Opens the bank box of a given target.")]
        public static void Bank_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new BankTarget();
        }

        private class BankTarget : Target
        {
            public BankTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    Mobile m = (Mobile)targeted;

                    BankBox box = (m.Player ? m.BankBox : m.FindBankNoCreate());

                    if (box != null)
                    {
                        CommandLogging.WriteLine(from, "{0} {1} opening bank box of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targeted));

                        if (from == targeted)
                            box.Open();
                        else
                            box.DisplayTo(from);
                    }
                    else
                    {
                        from.SendMessage("They have no bank box.");
                    }
                }
            }
        }
    }
}