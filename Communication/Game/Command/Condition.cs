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
    public class ConditionCommand : BaseCommand
    {
        public ConditionCommand()
        {
            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.Simple | CommandSupport.Complex | CommandSupport.Self;
            Commands = new string[] { "Condition" };
            ObjectTypes = ObjectTypes.All;
            Usage = "Condition <condition>";
            Description = "Checks that the given condition matches a targeted object.";
            ListOptimized = true;
        }

        public override void ExecuteList(CommandEventArgs e, ArrayList list)
        {
            try
            {
                string[] args = e.Arguments;
                ObjectConditional condition = ObjectConditional.Parse(e.Mobile, ref args);

                for (int i = 0; i < list.Count; ++i)
                {
                    if (condition.CheckCondition(list[i]))
                        AddResponse("True - that object matches the condition.");
                    else
                        AddResponse("False - that object does not match the condition.");
                }
            }
            catch (Exception ex)
            {
                e.Mobile.SendMessage(ex.Message);
            }
        }
    }
}