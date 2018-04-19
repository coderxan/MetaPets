using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Server;
using Server.Accounting;
using Server.Commands.Generic;
using Server.Engines.Help;
using Server.Gumps;
using Server.Items;
using Server.Menus;
using Server.Menus.ItemLists;
using Server.Menus.Questions;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;
using Server.Targeting;
using Server.Targets;

/// <summary>
/// This file is a Base Script for several commands in the 'Scripts\Communication\Game\Command' directory.
/// Editing any of the commands contained in this file could result in loss of function.
/// 
/// CommandHandler Format: Register (yourCommand, AccessLevel.AccessLevel, new CommandEventHandler(yourCommand_OnCommand));
/// Instructions: This should be set up under: public partial class CommandHandlers
///               Under the namespace: Server.Scripts.Commands
/// 
/// TargetCommand Format: Register (new yourCommand()); 
/// Instructions: This should be set up as: public class yourCommand : BaseCommand
///               Under the namespace: Server.Scripts.Commands
///               
/// If you follow the directions above you should be able to add new commands in this file seamlessly.
/// </summary>

#region Register CH

namespace Server.Commands
{
    public partial class CommandHandlers
    {
        public static void Initialize()
        {
            CommandSystem.Prefix = "[";

            Register("Go", AccessLevel.Counselor, new CommandEventHandler(Go_OnCommand));
            Register("DropHolding", AccessLevel.Counselor, new CommandEventHandler(DropHolding_OnCommand));
            Register("GetFollowers", AccessLevel.GameMaster, new CommandEventHandler(GetFollowers_OnCommand));
            Register("ClearFacet", AccessLevel.Administrator, new CommandEventHandler(ClearFacet_OnCommand));
            Register("Where", AccessLevel.Counselor, new CommandEventHandler(Where_OnCommand));
            Register("AutoPageNotify", AccessLevel.Counselor, new CommandEventHandler(APN_OnCommand));
            Register("APN", AccessLevel.Counselor, new CommandEventHandler(APN_OnCommand));
            Register("Animate", AccessLevel.GameMaster, new CommandEventHandler(Animate_OnCommand));
            Register("Cast", AccessLevel.Counselor, new CommandEventHandler(Cast_OnCommand));
            Register("Stuck", AccessLevel.Counselor, new CommandEventHandler(Stuck_OnCommand));
            Register("Help", AccessLevel.Player, new CommandEventHandler(Help_OnCommand));
            Register("Save", AccessLevel.Administrator, new CommandEventHandler(Save_OnCommand));
            Register("BackgroundSave", AccessLevel.Administrator, new CommandEventHandler(BackgroundSave_OnCommand));
            Register("BGSave", AccessLevel.Administrator, new CommandEventHandler(BackgroundSave_OnCommand));
            Register("SaveBG", AccessLevel.Administrator, new CommandEventHandler(BackgroundSave_OnCommand));
            Register("Move", AccessLevel.GameMaster, new CommandEventHandler(Move_OnCommand));
            Register("Client", AccessLevel.Counselor, new CommandEventHandler(Client_OnCommand));
            Register("SMsg", AccessLevel.Counselor, new CommandEventHandler(StaffMessage_OnCommand));
            Register("SM", AccessLevel.Counselor, new CommandEventHandler(StaffMessage_OnCommand));
            Register("S", AccessLevel.Counselor, new CommandEventHandler(StaffMessage_OnCommand));
            Register("BCast", AccessLevel.GameMaster, new CommandEventHandler(BroadcastMessage_OnCommand));
            Register("BC", AccessLevel.GameMaster, new CommandEventHandler(BroadcastMessage_OnCommand));
            Register("B", AccessLevel.GameMaster, new CommandEventHandler(BroadcastMessage_OnCommand));
            Register("Bank", AccessLevel.GameMaster, new CommandEventHandler(Bank_OnCommand));
            Register("Echo", AccessLevel.Counselor, new CommandEventHandler(Echo_OnCommand));
            Register("Sound", AccessLevel.GameMaster, new CommandEventHandler(Sound_OnCommand));
            Register("ViewEquip", AccessLevel.GameMaster, new CommandEventHandler(ViewEquip_OnCommand));
            Register("Light", AccessLevel.Counselor, new CommandEventHandler(Light_OnCommand));
            Register("Stats", AccessLevel.Counselor, new CommandEventHandler(Stats_OnCommand));
            Register("ReplaceBankers", AccessLevel.Administrator, new CommandEventHandler(ReplaceBankers_OnCommand));
            Register("SpeedBoost", AccessLevel.Counselor, new CommandEventHandler(SpeedBoost_OnCommand));
        }

        public static void Register(string command, AccessLevel access, CommandEventHandler handler)
        {
            CommandSystem.Register(command, access, handler);
        }
    }
}

#endregion

#region Register TC

namespace Server.Commands.Generic
{
    public class TargetCommands
    {
        public static void Initialize()
        {
            Register(new KillCommand(true));
            Register(new KillCommand(false));
            Register(new HideCommand(true));
            Register(new HideCommand(false));
            Register(new KickCommand(true));
            Register(new KickCommand(false));
            Register(new FirewallCommand());
            Register(new TeleCommand());
            Register(new SetCommand());
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "Immortal", "blessed", "true", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "Invul", "blessed", "true", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "Mortal", "blessed", "false", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "NoInvul", "blessed", "false", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "Squelch", "squelched", "true", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "Unsquelch", "squelched", "false", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "ShaveHair", "HairItemID", "0", ObjectTypes.Mobiles));
            Register(new AliasedSetCommand(AccessLevel.GameMaster, "ShaveBeard", "FacialHairItemID", "0", ObjectTypes.Mobiles));
            Register(new GetCommand());
            Register(new GetTypeCommand());
            Register(new DeleteCommand());
            Register(new RestockCommand());
            Register(new DismountCommand());
            Register(new AddCommand());
            Register(new AddToPackCommand());
            Register(new TellCommand(true));
            Register(new TellCommand(false));
            Register(new PrivSoundCommand());
            Register(new IncreaseCommand());
            Register(new OpenBrowserCommand());
            Register(new CountCommand());
            Register(new InterfaceCommand());
            Register(new RefreshHouseCommand());
            Register(new ConditionCommand());
            Register(new Factions.FactionKickCommand(Factions.FactionKickType.Kick));
            Register(new Factions.FactionKickCommand(Factions.FactionKickType.Ban));
            Register(new Factions.FactionKickCommand(Factions.FactionKickType.Unban));
            Register(new BringToPackCommand());
            Register(new TraceLockdownCommand());
        }

        private static List<BaseCommand> m_AllCommands = new List<BaseCommand>();

        public static List<BaseCommand> AllCommands { get { return m_AllCommands; } }

        public static void Register(BaseCommand command)
        {
            m_AllCommands.Add(command);

            List<BaseCommandImplementor> impls = BaseCommandImplementor.Implementors;

            for (int i = 0; i < impls.Count; ++i)
            {
                BaseCommandImplementor impl = impls[i];

                if ((command.Supports & impl.SupportRequirement) != 0)
                    impl.Register(command);
            }
        }
    }
}

#endregion