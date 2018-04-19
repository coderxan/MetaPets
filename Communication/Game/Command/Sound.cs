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
        [Usage("Sound <index> [toAll=true]")]
        [Description("Plays a sound to players within 12 tiles of you. The (toAll) argument specifies to everyone, or just those who can see you.")]
        public static void Sound_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
                PlaySound(e.Mobile, e.GetInt32(0), true);
            else if (e.Length == 2)
                PlaySound(e.Mobile, e.GetInt32(0), e.GetBoolean(1));
            else
                e.Mobile.SendMessage("Format: Sound <index> [toAll]");
        }

        private static void PlaySound(Mobile m, int index, bool toAll)
        {
            Map map = m.Map;

            if (map == null)
                return;

            CommandLogging.WriteLine(m, "{0} {1} playing sound {2} (toAll={3})", m.AccessLevel, CommandLogging.Format(m), index, toAll);

            Packet p = new PlaySound(index, m.Location);

            p.Acquire();

            foreach (NetState state in m.GetClientsInRange(12))
            {
                if (toAll || state.Mobile.CanSee(m))
                    state.Send(p);
            }

            p.Release();
        }
    }
}