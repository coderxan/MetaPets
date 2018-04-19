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
        [Usage("Cast <name>")]
        [Description("Casts a spell by name.")]
        public static void Cast_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
            {
                if (!Multis.DesignContext.Check(e.Mobile))
                    return; // They are customizing

                Spell spell = SpellRegistry.NewSpell(e.GetString(0), e.Mobile, null);

                if (spell != null)
                    spell.Cast();
                else
                    e.Mobile.SendMessage("That spell was not found.");
            }
            else
            {
                e.Mobile.SendMessage("Format: Cast <name>");
            }
        }
    }
}