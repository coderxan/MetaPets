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
    public class SetCommand : BaseCommand
    {
        public SetCommand()
        {
            AccessLevel = AccessLevel.Counselor;
            Supports = CommandSupport.All;
            Commands = new string[] { "Set" };
            ObjectTypes = ObjectTypes.Both;
            Usage = "Set <propertyName> <value> [...]";
            Description = "Sets one or more property values by name of a targeted object.";
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            if (e.Length >= 2)
            {
                for (int i = 0; (i + 1) < e.Length; i += 2)
                {
                    string result = Properties.SetValue(e.Mobile, obj, e.GetString(i), e.GetString(i + 1));

                    if (result == "Property has been set.")
                        AddResponse(result);
                    else
                        LogFailure(result);
                }
            }
            else
            {
                LogFailure("Format: Set <propertyName> <value>");
            }
        }
    }

    public class AliasedSetCommand : BaseCommand
    {
        private string m_Name;
        private string m_Value;

        public AliasedSetCommand(AccessLevel level, string command, string name, string value, ObjectTypes objects)
        {
            m_Name = name;
            m_Value = value;

            AccessLevel = level;

            if (objects == ObjectTypes.Items)
                Supports = CommandSupport.AllItems;
            else if (objects == ObjectTypes.Mobiles)
                Supports = CommandSupport.AllMobiles;
            else
                Supports = CommandSupport.All;

            Commands = new string[] { command };
            ObjectTypes = objects;
            Usage = command;
            Description = String.Format("Sets the {0} property to {1}.", name, value);
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            string result = Properties.SetValue(e.Mobile, obj, m_Name, m_Value);

            if (result == "Property has been set.")
                AddResponse(result);
            else
                LogFailure(result);
        }
    }
}