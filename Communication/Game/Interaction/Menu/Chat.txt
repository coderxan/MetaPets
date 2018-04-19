using System;
using System.Collections.Generic;

using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Misc;
using Server.Network;

/// <summary>
/// The following OLD namespace Server.Chat simply disabled the Menu Chat option
/// Uncomment either the namespace Server.Chat <OR> namespace Server.Engines.Chat
/// By default the old system has been commented out and disabled.
/// </summary>

#region Old Client Menu Chat Option
/*
namespace Server.Chat
{
    public class ChatSystem
    {
        public static void Initialize()
        {
            EventSink.ChatRequest += new ChatRequestEventHandler(EventSink_ChatRequest);
        }

        private static void EventSink_ChatRequest(ChatRequestEventArgs e)
        {
            e.Mobile.SendMessage("Chat is not currently supported.");
        }
    }
}
*/
#endregion

/// <summary>
/// The following NEW namespace Server.Engines.Chat enables the Menu Chat option
/// Uncomment either the namespace Server.Chat <OR> namespace Server.Engines.Chat
/// By default the new system has been uncommented and enabled.
/// </summary>

#region New Client Menu Chat Option

namespace Server.Engines.Chat
{
    public class ChatSystem
    {
        private static bool m_Enabled = true;

        public static bool Enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        public static void Initialize()
        {
            PacketHandlers.Register(0xB5, 0x40, true, new OnPacketReceive(OpenChatWindowRequest));
            PacketHandlers.Register(0xB3, 0, true, new OnPacketReceive(ChatAction));
        }

        public static void SendCommandTo(Mobile to, ChatCommand type)
        {
            SendCommandTo(to, type, null, null);
        }

        public static void SendCommandTo(Mobile to, ChatCommand type, string param1)
        {
            SendCommandTo(to, type, param1, null);
        }

        public static void SendCommandTo(Mobile to, ChatCommand type, string param1, string param2)
        {
            if (to != null)
                to.Send(new ChatMessagePacket(null, (int)type + 20, param1, param2));
        }

        public static void OpenChatWindowRequest(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            if (!m_Enabled)
            {
                from.SendMessage("The chat system has been disabled.");
                return;
            }

            pvSrc.Seek(2, System.IO.SeekOrigin.Begin);
            string chatName = pvSrc.ReadUnicodeStringSafe((0x40 - 2) >> 1).Trim();

            Account acct = state.Account as Account;

            string accountChatName = null;

            if (acct != null)
                accountChatName = acct.GetTag("ChatName");

            if (accountChatName != null)
                accountChatName = accountChatName.Trim();

            if (accountChatName != null && accountChatName.Length > 0)
            {
                if (chatName.Length > 0 && chatName != accountChatName)
                    from.SendMessage("You cannot change chat nickname once it has been set.");
            }
            else
            {
                if (chatName == null || chatName.Length == 0)
                {
                    SendCommandTo(from, ChatCommand.AskNewNickname);
                    return;
                }

                if (NameVerification.Validate(chatName, 2, 31, true, true, true, 0, NameVerification.SpaceDashPeriodQuote) && chatName.ToLower().IndexOf("system") == -1)
                {
                    // TODO: Optimize this search

                    foreach (Account checkAccount in Accounts.GetAccounts())
                    {
                        string existingName = checkAccount.GetTag("ChatName");

                        if (existingName != null)
                        {
                            existingName = existingName.Trim();

                            if (Insensitive.Equals(existingName, chatName))
                            {
                                from.SendMessage("Nickname already in use.");
                                SendCommandTo(from, ChatCommand.AskNewNickname);
                                return;
                            }
                        }
                    }

                    accountChatName = chatName;

                    if (acct != null)
                        acct.AddTag("ChatName", chatName);
                }
                else
                {
                    from.SendLocalizedMessage(501173); // That name is disallowed.
                    SendCommandTo(from, ChatCommand.AskNewNickname);
                    return;
                }
            }

            SendCommandTo(from, ChatCommand.OpenChatWindow, accountChatName);
            ChatUser.AddChatUser(from);
        }

        public static ChatUser SearchForUser(ChatUser from, string name)
        {
            ChatUser user = ChatUser.GetChatUser(name);

            if (user == null)
                from.SendMessage(32, name); // There is no player named '%1'.

            return user;
        }

        public static void ChatAction(NetState state, PacketReader pvSrc)
        {
            if (!m_Enabled)
                return;

            try
            {
                Mobile from = state.Mobile;
                ChatUser user = ChatUser.GetChatUser(from);

                if (user == null)
                    return;

                string lang = pvSrc.ReadStringSafe(4);
                int actionID = pvSrc.ReadInt16();
                string param = pvSrc.ReadUnicodeString();

                ChatActionHandler handler = ChatActionHandlers.GetHandler(actionID);

                if (handler != null)
                {
                    Channel channel = user.CurrentChannel;

                    if (handler.RequireConference && channel == null)
                    {
                        user.SendMessage(31); /* You must be in a conference to do this.
												 * To join a conference, select one from the Conference menu.
												 */
                    }
                    else if (handler.RequireModerator && !user.IsModerator)
                    {
                        user.SendMessage(29); // You must have operator status to do this.
                    }
                    else
                    {
                        handler.Callback(user, channel, param);
                    }
                }
                else
                {
                    Console.WriteLine("Client: {0}: Unknown chat action 0x{1:X}: {2}", state, actionID, param);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    class ChatInitializer
    {
        public static void Initialize()
        {
            CommandSystem.Register("Chat", AccessLevel.Player, new CommandEventHandler(OpenChat_OnCommand));
            EventSink.Login += new LoginEventHandler(EventSink_Login);
            EventSink.Logout += new LogoutEventHandler(EventSink_Logout);
            EventSink.Disconnected += new DisconnectedEventHandler(EventSink_Disconnected);
            EventSink.ChatRequest += new ChatRequestEventHandler(EventSink_ChatRequest);
        }

        [Usage("Chat")]
        [Description("Enables or Disables the Chat.")]
        private static void OpenChat_OnCommand(CommandEventArgs args)
        {
            Mobile player = args.Mobile;

            if (player.Account == null)
            {
                player.SendMessage(38, "You have no account! Report immediately to the staff!");
                return;
            }

            Account account = player.Account as Account;

            if (account.GetTag("ChatInitializer") != "OK")
            {
                account.SetTag("ChatInitializer", "OK");
                player.SendMessage(68, "Chat: ENABLED");
                DisplayChatTo(player);
            }
            else
            {
                account.SetTag("ChatInitializer", "NO");
                player.SendMessage(38, "Chat: DISABLED");
                HideChatFrom(player);
            }
        }

        private static void EventSink_Login(LoginEventArgs args)
        {
            Mobile player = args.Mobile;

            if (player.Account == null)
                return;

            Account account = player.Account as Account;

            DefineChatName(player);

            if (account.GetTag("ChatInitializer") == "OK")
            {
                DisplayChatTo(player);
            }
        }

        private static void EventSink_Logout(LogoutEventArgs args)
        {
            Mobile player = args.Mobile;
            HideChatFrom(player);
        }

        private static void EventSink_Disconnected(DisconnectedEventArgs args)
        {
            Mobile player = args.Mobile;
            HideChatFrom(player);
        }

        private static void EventSink_ChatRequest(ChatRequestEventArgs e)
        {
            Mobile player = e.Mobile;

            if (player.Account == null)
                return;

            Account account = player.Account as Account;

            if (account.GetTag("ChatInitializer") != "OK")
            {
                account.SetTag("ChatInitializer", "OK");
                player.SendMessage(68, "Chat: ENABLED");
                DisplayChatTo(player);
            }
        }

        private static void DisplayChatTo(Mobile player)
        {
            if (player.Account == null)
                return;

            Account account = player.Account as Account;
            string accountChatName = account.GetTag("ChatName");
            ChatSystem.SendCommandTo(player, ChatCommand.OpenChatWindow, accountChatName);
            ChatUser.AddChatUser(player);
        }

        private static void HideChatFrom(Mobile player)
        {
            if (player.Account == null)
                return;

            Account account = player.Account as Account;
            ChatSystem.SendCommandTo(player, ChatCommand.CloseChatWindow);
            ChatUser user = ChatUser.GetChatUser(player);
            ChatUser.RemoveChatUser(user);
        }

        private static void DefineChatName(Mobile player)
        {
            if (player.Account == null)
                return;

            Account account = player.Account as Account;
            string accountChatName = (player.RawName).Replace(" ", "");
            account.SetTag("ChatName", accountChatName);
        }
    }

    public enum ChatCommand
    {
        /// <summary>
        /// Add a channel to top list.
        /// </summary>
        AddChannel = 0x3E8,
        /// <summary>
        /// Remove channel from top list.
        /// </summary>
        RemoveChannel = 0x3E9,
        /// <summary>
        /// Queries for a new chat nickname.
        /// </summary>
        AskNewNickname = 0x3EB,
        /// <summary>
        /// Closes the chat window.
        /// </summary>
        CloseChatWindow = 0x3EC,
        /// <summary>
        /// Opens the chat window.
        /// </summary>
        OpenChatWindow = 0x3ED,
        /// <summary>
        /// Add a user to current channel.
        /// </summary>
        AddUserToChannel = 0x3EE,
        /// <summary>
        /// Remove a user from current channel.
        /// </summary>
        RemoveUserFromChannel = 0x3EF,
        /// <summary>
        /// Send a message putting generic conference name at top when player leaves a channel.
        /// </summary>
        LeaveChannel = 0x3F0,
        /// <summary>
        /// Send a message putting Channel name at top and telling player he joined the channel.
        /// </summary>
        JoinedChannel = 0x3F1
    }

    public delegate void OnChatAction(ChatUser from, Channel channel, string param);

    public class ChatActionHandler
    {
        private bool m_RequireModerator;
        private bool m_RequireConference;
        private OnChatAction m_Callback;

        public bool RequireModerator { get { return m_RequireModerator; } }
        public bool RequireConference { get { return m_RequireConference; } }
        public OnChatAction Callback { get { return m_Callback; } }

        public ChatActionHandler(bool requireModerator, bool requireConference, OnChatAction callback)
        {
            m_RequireModerator = requireModerator;
            m_RequireConference = requireConference;
            m_Callback = callback;
        }
    }

    public class ChatActionHandlers
    {
        private static ChatActionHandler[] m_Handlers;

        static ChatActionHandlers()
        {
            m_Handlers = new ChatActionHandler[0x100];

            Register(0x41, true, true, new OnChatAction(ChangeChannelPassword));

            Register(0x58, false, false, new OnChatAction(LeaveChat));

            Register(0x61, false, true, new OnChatAction(ChannelMessage));
            Register(0x62, false, false, new OnChatAction(JoinChannel));
            Register(0x63, false, false, new OnChatAction(JoinNewChannel));
            Register(0x64, true, true, new OnChatAction(RenameChannel));
            Register(0x65, false, false, new OnChatAction(PrivateMessage));
            Register(0x66, false, false, new OnChatAction(AddIgnore));
            Register(0x67, false, false, new OnChatAction(RemoveIgnore));
            Register(0x68, false, false, new OnChatAction(ToggleIgnore));
            Register(0x69, true, true, new OnChatAction(AddVoice));
            Register(0x6A, true, true, new OnChatAction(RemoveVoice));
            Register(0x6B, true, true, new OnChatAction(ToggleVoice));
            Register(0x6C, true, true, new OnChatAction(AddModerator));
            Register(0x6D, true, true, new OnChatAction(RemoveModerator));
            Register(0x6E, true, true, new OnChatAction(ToggleModerator));
            Register(0x6F, false, false, new OnChatAction(AllowPrivateMessages));
            Register(0x70, false, false, new OnChatAction(DisallowPrivateMessages));
            Register(0x71, false, false, new OnChatAction(TogglePrivateMessages));
            Register(0x72, false, false, new OnChatAction(ShowCharacterName));
            Register(0x73, false, false, new OnChatAction(HideCharacterName));
            Register(0x74, false, false, new OnChatAction(ToggleCharacterName));
            Register(0x75, false, false, new OnChatAction(QueryWhoIs));
            Register(0x76, true, true, new OnChatAction(Kick));
            Register(0x77, true, true, new OnChatAction(EnableDefaultVoice));
            Register(0x78, true, true, new OnChatAction(DisableDefaultVoice));
            Register(0x79, true, true, new OnChatAction(ToggleDefaultVoice));
            Register(0x7A, false, true, new OnChatAction(EmoteMessage));
        }

        public static void Register(int actionID, bool requireModerator, bool requireConference, OnChatAction callback)
        {
            if (actionID >= 0 && actionID < m_Handlers.Length)
                m_Handlers[actionID] = new ChatActionHandler(requireModerator, requireConference, callback);
        }

        public static ChatActionHandler GetHandler(int actionID)
        {
            if (actionID >= 0 && actionID < m_Handlers.Length)
                return m_Handlers[actionID];

            return null;
        }

        public static void ChannelMessage(ChatUser from, Channel channel, string param)
        {
            if (channel.CanTalk(from))
                channel.SendIgnorableMessage(57, from, from.GetColorCharacter() + from.Username, param); // %1: %2
            else
                from.SendMessage(36); // The moderator of this conference has not given you speaking priviledges.
        }

        public static void EmoteMessage(ChatUser from, Channel channel, string param)
        {
            if (channel.CanTalk(from))
                channel.SendIgnorableMessage(58, from, from.GetColorCharacter() + from.Username, param); // %1 %2
            else
                from.SendMessage(36); // The moderator of this conference has not given you speaking priviledges.
        }

        public static void PrivateMessage(ChatUser from, Channel channel, string param)
        {
            int indexOf = param.IndexOf(' ');

            string name = param.Substring(0, indexOf);
            string text = param.Substring(indexOf + 1);

            ChatUser target = ChatSystem.SearchForUser(from, name);

            if (target == null)
                return;

            if (target.IsIgnored(from))
                from.SendMessage(35, target.Username); // %1 has chosen to ignore you. None of your messages to them will get through.
            else if (target.IgnorePrivateMessage)
                from.SendMessage(42, target.Username); // %1 has chosen to not receive private messages at the moment.
            else
                target.SendMessage(59, from.Mobile, from.GetColorCharacter() + from.Username, text); // [%1]: %2
        }

        public static void LeaveChat(ChatUser from, Channel channel, string param)
        {
            ChatUser.RemoveChatUser(from);
        }

        public static void ChangeChannelPassword(ChatUser from, Channel channel, string param)
        {
            channel.Password = param;
            from.SendMessage(60); // The password to the conference has been changed.
        }

        public static void AllowPrivateMessages(ChatUser from, Channel channel, string param)
        {
            from.IgnorePrivateMessage = false;
            from.SendMessage(37); // You can now receive private messages.
        }

        public static void DisallowPrivateMessages(ChatUser from, Channel channel, string param)
        {
            from.IgnorePrivateMessage = true;
            from.SendMessage(38); /* You will no longer receive private messages.
									 * Those who send you a message will be notified that you are blocking incoming messages.
									 */
        }

        public static void TogglePrivateMessages(ChatUser from, Channel channel, string param)
        {
            from.IgnorePrivateMessage = !from.IgnorePrivateMessage;
            from.SendMessage(from.IgnorePrivateMessage ? 38 : 37); // See above for messages
        }

        public static void ShowCharacterName(ChatUser from, Channel channel, string param)
        {
            from.Anonymous = false;
            from.SendMessage(39); // You are now showing your character name to any players who inquire with the whois command.
        }

        public static void HideCharacterName(ChatUser from, Channel channel, string param)
        {
            from.Anonymous = true;
            from.SendMessage(40); // You are no longer showing your character name to any players who inquire with the whois command.
        }

        public static void ToggleCharacterName(ChatUser from, Channel channel, string param)
        {
            from.Anonymous = !from.Anonymous;
            from.SendMessage(from.Anonymous ? 40 : 39); // See above for messages
        }

        public static void JoinChannel(ChatUser from, Channel channel, string param)
        {
            string name;
            string password = null;

            int start = param.IndexOf('\"');

            if (start >= 0)
            {
                int end = param.IndexOf('\"', ++start);

                if (end >= 0)
                {
                    name = param.Substring(start, end - start);
                    password = param.Substring(++end);
                }
                else
                {
                    name = param.Substring(start);
                }
            }
            else
            {
                int indexOf = param.IndexOf(' ');

                if (indexOf >= 0)
                {
                    name = param.Substring(0, indexOf++);
                    password = param.Substring(indexOf);
                }
                else
                {
                    name = param;
                }
            }

            if (password != null)
                password = password.Trim();

            if (password != null && password.Length == 0)
                password = null;

            Channel joined = Channel.FindChannelByName(name);

            if (joined == null)
                from.SendMessage(33, name); // There is no conference named '%1'.
            else
                joined.AddUser(from, password);
        }

        public static void JoinNewChannel(ChatUser from, Channel channel, string param)
        {
            if ((param = param.Trim()).Length == 0)
                return;

            string name;
            string password = null;

            int start = param.IndexOf('{');

            if (start >= 0)
            {
                name = param.Substring(0, start++);

                int end = param.IndexOf('}', start);

                if (end >= start)
                    password = param.Substring(start, end - start);
            }
            else
            {
                name = param;
            }

            if (password != null)
                password = password.Trim();

            if (password != null && password.Length == 0)
                password = null;

            Channel.AddChannel(name, password).AddUser(from, password);
        }

        public static void AddIgnore(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target == null)
                return;

            from.AddIgnored(target);
        }

        public static void RemoveIgnore(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target == null)
                return;

            from.RemoveIgnored(target);
        }

        public static void ToggleIgnore(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target == null)
                return;

            if (from.IsIgnored(target))
                from.RemoveIgnored(target);
            else
                from.AddIgnored(target);
        }

        public static void AddVoice(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target != null)
                channel.AddVoiced(target, from);
        }

        public static void RemoveVoice(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target != null)
                channel.RemoveVoiced(target, from);
        }

        public static void ToggleVoice(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target == null)
                return;

            if (channel.IsVoiced(target))
                channel.RemoveVoiced(target, from);
            else
                channel.AddVoiced(target, from);
        }

        public static void AddModerator(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target != null)
                channel.AddModerator(target, from);
        }

        public static void RemoveModerator(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target != null)
                channel.RemoveModerator(target, from);
        }

        public static void ToggleModerator(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target == null)
                return;

            if (channel.IsModerator(target))
                channel.RemoveModerator(target, from);
            else
                channel.AddModerator(target, from);
        }

        public static void RenameChannel(ChatUser from, Channel channel, string param)
        {
            channel.Name = param;
        }

        public static void QueryWhoIs(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target == null)
                return;

            if (target.Anonymous)
                from.SendMessage(41, target.Username); // %1 is remaining anonymous.
            else
                from.SendMessage(43, target.Username, target.Mobile.Name); // %2 is known in the lands of Britannia as %2.
        }

        public static void Kick(ChatUser from, Channel channel, string param)
        {
            ChatUser target = ChatSystem.SearchForUser(from, param);

            if (target != null)
                channel.Kick(target, from);
        }

        public static void EnableDefaultVoice(ChatUser from, Channel channel, string param)
        {
            channel.VoiceRestricted = false;
        }

        public static void DisableDefaultVoice(ChatUser from, Channel channel, string param)
        {
            channel.VoiceRestricted = true;
        }

        public static void ToggleDefaultVoice(ChatUser from, Channel channel, string param)
        {
            channel.VoiceRestricted = !channel.VoiceRestricted;
        }
    }

    public sealed class ChatMessagePacket : Packet
    {
        public ChatMessagePacket(Mobile who, int number, string param1, string param2)
            : base(0xB2)
        {
            if (param1 == null)
                param1 = String.Empty;

            if (param2 == null)
                param2 = String.Empty;

            EnsureCapacity(13 + ((param1.Length + param2.Length) * 2));

            m_Stream.Write((ushort)(number - 20));

            if (who != null)
                m_Stream.WriteAsciiFixed(who.Language, 4);
            else
                m_Stream.Write((int)0);

            m_Stream.WriteBigUniNull(param1);
            m_Stream.WriteBigUniNull(param2);
        }
    }

    public class ChatUser
    {
        private Mobile m_Mobile;
        private Channel m_Channel;
        private bool m_Anonymous;
        private bool m_IgnorePrivateMessage;
        private List<ChatUser> m_Ignored, m_Ignoring;

        public ChatUser(Mobile m)
        {
            m_Mobile = m;
            m_Ignored = new List<ChatUser>();
            m_Ignoring = new List<ChatUser>();
        }

        public Mobile Mobile
        {
            get
            {
                return m_Mobile;
            }
        }

        public List<ChatUser> Ignored
        {
            get
            {
                return m_Ignored;
            }
        }

        public List<ChatUser> Ignoring
        {
            get
            {
                return m_Ignoring;
            }
        }

        public string Username
        {
            get
            {
                Account acct = m_Mobile.Account as Account;

                if (acct != null)
                    return acct.GetTag("ChatName");

                return null;
            }
            set
            {
                Account acct = m_Mobile.Account as Account;

                if (acct != null)
                    acct.SetTag("ChatName", value);
            }
        }

        public Channel CurrentChannel
        {
            get
            {
                return m_Channel;
            }
            set
            {
                m_Channel = value;
            }
        }

        public bool IsOnline
        {
            get
            {
                return (m_Mobile.NetState != null);
            }
        }

        public bool Anonymous
        {
            get
            {
                return m_Anonymous;
            }
            set
            {
                m_Anonymous = value;
            }
        }

        public bool IgnorePrivateMessage
        {
            get
            {
                return m_IgnorePrivateMessage;
            }
            set
            {
                m_IgnorePrivateMessage = value;
            }
        }

        public const char NormalColorCharacter = '0';
        public const char ModeratorColorCharacter = '1';
        public const char VoicedColorCharacter = '2';

        public char GetColorCharacter()
        {
            if (m_Channel != null && m_Channel.IsModerator(this))
                return ModeratorColorCharacter;

            if (m_Channel != null && m_Channel.IsVoiced(this))
                return VoicedColorCharacter;

            return NormalColorCharacter;
        }

        public bool CheckOnline()
        {
            if (IsOnline)
                return true;

            RemoveChatUser(this);
            return false;
        }

        public void SendMessage(int number)
        {
            SendMessage(number, null, null);
        }

        public void SendMessage(int number, string param1)
        {
            SendMessage(number, param1, null);
        }

        public void SendMessage(int number, string param1, string param2)
        {
            if (m_Mobile.NetState != null)
                m_Mobile.Send(new ChatMessagePacket(m_Mobile, number, param1, param2));
        }

        public void SendMessage(int number, Mobile from, string param1, string param2)
        {
            if (m_Mobile.NetState != null)
                m_Mobile.Send(new ChatMessagePacket(from, number, param1, param2));
        }

        public bool IsIgnored(ChatUser check)
        {
            return m_Ignored.Contains(check);
        }

        public bool IsModerator
        {
            get
            {
                return (m_Channel != null && m_Channel.IsModerator(this));
            }
        }

        public void AddIgnored(ChatUser user)
        {
            if (IsIgnored(user))
            {
                SendMessage(22, user.Username); // You are already ignoring %1.
            }
            else
            {
                m_Ignored.Add(user);
                user.m_Ignoring.Add(this);

                SendMessage(23, user.Username); // You are now ignoring %1.
            }
        }

        public void RemoveIgnored(ChatUser user)
        {
            if (IsIgnored(user))
            {
                m_Ignored.Remove(user);
                user.m_Ignoring.Remove(this);

                SendMessage(24, user.Username); // You are no longer ignoring %1.

                if (m_Ignored.Count == 0)
                    SendMessage(26); // You are no longer ignoring anyone.
            }
            else
            {
                SendMessage(25, user.Username); // You are not ignoring %1.
            }
        }

        private static List<ChatUser> m_Users = new List<ChatUser>();
        private static Dictionary<Mobile, ChatUser> m_Table = new Dictionary<Mobile, ChatUser>();

        public static ChatUser AddChatUser(Mobile from)
        {
            ChatUser user = GetChatUser(from);

            if (user == null)
            {
                user = new ChatUser(from);

                m_Users.Add(user);
                m_Table[from] = user;

                Channel.SendChannelsTo(user);

                List<Channel> list = Channel.Channels;

                for (int i = 0; i < list.Count; ++i)
                {
                    Channel c = list[i];

                    if (c.AddUser(user))
                        break;
                }

                //ChatSystem.SendCommandTo( user.m_Mobile, ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username );
            }

            return user;
        }

        public static void RemoveChatUser(ChatUser user)
        {
            if (user == null)
                return;

            for (int i = 0; i < user.m_Ignoring.Count; ++i)
                user.m_Ignoring[i].RemoveIgnored(user);

            if (m_Users.Contains(user))
            {
                ChatSystem.SendCommandTo(user.Mobile, ChatCommand.CloseChatWindow);

                if (user.m_Channel != null)
                    user.m_Channel.RemoveUser(user);

                m_Users.Remove(user);
                m_Table.Remove(user.m_Mobile);
            }
        }

        public static void RemoveChatUser(Mobile from)
        {
            ChatUser user = GetChatUser(from);

            RemoveChatUser(user);
        }

        public static ChatUser GetChatUser(Mobile from)
        {
            ChatUser c;
            m_Table.TryGetValue(from, out c);
            return c;
        }

        public static ChatUser GetChatUser(string username)
        {
            for (int i = 0; i < m_Users.Count; ++i)
            {
                ChatUser user = m_Users[i];

                if (user.Username == username)
                    return user;
            }

            return null;
        }

        public static void GlobalSendCommand(ChatCommand command)
        {
            GlobalSendCommand(command, null, null, null);
        }

        public static void GlobalSendCommand(ChatCommand command, string param1)
        {
            GlobalSendCommand(command, null, param1, null);
        }

        public static void GlobalSendCommand(ChatCommand command, string param1, string param2)
        {
            GlobalSendCommand(command, null, param1, param2);
        }

        public static void GlobalSendCommand(ChatCommand command, ChatUser initiator)
        {
            GlobalSendCommand(command, initiator, null, null);
        }

        public static void GlobalSendCommand(ChatCommand command, ChatUser initiator, string param1)
        {
            GlobalSendCommand(command, initiator, param1, null);
        }

        public static void GlobalSendCommand(ChatCommand command, ChatUser initiator, string param1, string param2)
        {
            for (int i = 0; i < m_Users.Count; ++i)
            {
                ChatUser user = m_Users[i];

                if (user == initiator)
                    continue;

                if (user.CheckOnline())
                    ChatSystem.SendCommandTo(user.m_Mobile, command, param1, param2);
            }
        }
    }

    public class Channel
    {
        private string m_Name;
        private string m_Password;
        private List<ChatUser> m_Users, m_Banned, m_Moderators, m_Voices;
        private bool m_VoiceRestricted;
        private bool m_AlwaysAvailable;

        public Channel(string name)
        {
            m_Name = name;

            m_Users = new List<ChatUser>();
            m_Banned = new List<ChatUser>();
            m_Moderators = new List<ChatUser>();
            m_Voices = new List<ChatUser>();
        }

        public Channel(string name, string password)
            : this(name)
        {
            m_Password = password;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                SendCommand(ChatCommand.RemoveChannel, m_Name);
                m_Name = value;
                SendCommand(ChatCommand.AddChannel, m_Name);
                SendCommand(ChatCommand.JoinedChannel, m_Name);
            }
        }

        public string Password
        {
            get
            {
                return m_Password;
            }
            set
            {
                string newValue = null;

                if (value != null)
                {
                    newValue = value.Trim();

                    if (String.IsNullOrEmpty(newValue))
                        newValue = null;
                }

                m_Password = newValue;
            }
        }

        public bool Contains(ChatUser user)
        {
            return m_Users.Contains(user);
        }

        public bool IsBanned(ChatUser user)
        {
            return m_Banned.Contains(user);
        }

        public bool CanTalk(ChatUser user)
        {
            return (!m_VoiceRestricted || m_Voices.Contains(user) || m_Moderators.Contains(user));
        }

        public bool IsModerator(ChatUser user)
        {
            return m_Moderators.Contains(user);
        }

        public bool IsVoiced(ChatUser user)
        {
            return m_Voices.Contains(user);
        }

        public bool ValidatePassword(string password)
        {
            return (m_Password == null || Insensitive.Equals(m_Password, password));
        }

        public bool ValidateModerator(ChatUser user)
        {
            if (user != null && !IsModerator(user))
            {
                user.SendMessage(29); // You must have operator status to do this.
                return false;
            }

            return true;
        }

        public bool ValidateAccess(ChatUser from, ChatUser target)
        {
            if (from != null && target != null && from.Mobile.AccessLevel < target.Mobile.AccessLevel)
            {
                from.Mobile.SendMessage("Your access level is too low to do this.");
                return false;
            }

            return true;
        }

        public bool AddUser(ChatUser user)
        {
            return AddUser(user, null);
        }

        public bool AddUser(ChatUser user, string password)
        {
            if (Contains(user))
            {
                user.SendMessage(46, m_Name); // You are already in the conference '%1'.
                return true;
            }
            else if (IsBanned(user))
            {
                user.SendMessage(64); // You have been banned from this conference.
                return false;
            }
            else if (!ValidatePassword(password))
            {
                user.SendMessage(34); // That is not the correct password.
                return false;
            }
            else
            {
                if (user.CurrentChannel != null)
                    user.CurrentChannel.RemoveUser(user); // Remove them from their current channel first

                ChatSystem.SendCommandTo(user.Mobile, ChatCommand.JoinedChannel, m_Name);

                SendCommand(ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);

                m_Users.Add(user);
                user.CurrentChannel = this;

                if (user.Mobile.AccessLevel >= AccessLevel.GameMaster || (!m_AlwaysAvailable && m_Users.Count == 1))
                    AddModerator(user);

                SendUsersTo(user);

                return true;
            }
        }

        public void RemoveUser(ChatUser user)
        {
            if (Contains(user))
            {
                m_Users.Remove(user);
                user.CurrentChannel = null;

                if (m_Moderators.Contains(user))
                    m_Moderators.Remove(user);

                if (m_Voices.Contains(user))
                    m_Voices.Remove(user);

                SendCommand(ChatCommand.RemoveUserFromChannel, user, user.Username);
                ChatSystem.SendCommandTo(user.Mobile, ChatCommand.LeaveChannel);

                if (m_Users.Count == 0 && !m_AlwaysAvailable)
                    RemoveChannel(this);
            }
        }

        public void AdBan(ChatUser user)
        {
            AddBan(user, null);
        }

        public void AddBan(ChatUser user, ChatUser moderator)
        {
            if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
                return;

            if (!m_Banned.Contains(user))
                m_Banned.Add(user);

            Kick(user, moderator, true);
        }

        public void RemoveBan(ChatUser user)
        {
            if (m_Banned.Contains(user))
                m_Banned.Remove(user);
        }

        public void Kick(ChatUser user)
        {
            Kick(user, null);
        }

        public void Kick(ChatUser user, ChatUser moderator)
        {
            Kick(user, moderator, false);
        }

        public void Kick(ChatUser user, ChatUser moderator, bool wasBanned)
        {
            if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
                return;

            if (Contains(user))
            {
                if (moderator != null)
                {
                    if (wasBanned)
                        user.SendMessage(63, moderator.Username); // %1, a conference moderator, has banned you from the conference.
                    else
                        user.SendMessage(45, moderator.Username); // %1, a conference moderator, has kicked you out of the conference.
                }

                RemoveUser(user);
                ChatSystem.SendCommandTo(user.Mobile, ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);

                SendMessage(44, user.Username); // %1 has been kicked out of the conference.
            }

            if (wasBanned && moderator != null)
                moderator.SendMessage(62, user.Username); // You are banning %1 from this conference.
        }

        public bool VoiceRestricted
        {
            get
            {
                return m_VoiceRestricted;
            }
            set
            {
                m_VoiceRestricted = value;

                if (value)
                    SendMessage(56); // From now on, only moderators will have speaking privileges in this conference by default.
                else
                    SendMessage(55); // From now on, everyone in the conference will have speaking privileges by default.
            }
        }

        public bool AlwaysAvailable
        {
            get
            {
                return m_AlwaysAvailable;
            }
            set
            {
                m_AlwaysAvailable = value;
            }
        }

        public void AddVoiced(ChatUser user)
        {
            AddVoiced(user, null);
        }

        public void AddVoiced(ChatUser user, ChatUser moderator)
        {
            if (!ValidateModerator(moderator))
                return;

            if (!IsBanned(user) && !IsModerator(user) && !IsVoiced(user))
            {
                m_Voices.Add(user);

                if (moderator != null)
                    user.SendMessage(54, moderator.Username); // %1, a conference moderator, has granted you speaking priviledges in this conference.

                SendMessage(52, user, user.Username); // %1 now has speaking privileges in this conference.
                SendCommand(ChatCommand.AddUserToChannel, user, user.GetColorCharacter() + user.Username);
            }
        }

        public void RemoveVoiced(ChatUser user, ChatUser moderator)
        {
            if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
                return;

            if (!IsModerator(user) && IsVoiced(user))
            {
                m_Voices.Remove(user);

                if (moderator != null)
                    user.SendMessage(53, moderator.Username); // %1, a conference moderator, has removed your speaking priviledges for this conference.

                SendMessage(51, user, user.Username); // %1 no longer has speaking privileges in this conference.
                SendCommand(ChatCommand.AddUserToChannel, user, user.GetColorCharacter() + user.Username);
            }
        }

        public void AddModerator(ChatUser user)
        {
            AddModerator(user, null);
        }

        public void AddModerator(ChatUser user, ChatUser moderator)
        {
            if (!ValidateModerator(moderator))
                return;

            if (IsBanned(user) || IsModerator(user))
                return;

            if (IsVoiced(user))
                m_Voices.Remove(user);

            m_Moderators.Add(user);

            if (moderator != null)
                user.SendMessage(50, moderator.Username); // %1 has made you a conference moderator.

            SendMessage(48, user, user.Username); // %1 is now a conference moderator.
            SendCommand(ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);
        }

        public void RemoveModerator(ChatUser user)
        {
            RemoveModerator(user, null);
        }

        public void RemoveModerator(ChatUser user, ChatUser moderator)
        {
            if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
                return;

            if (IsModerator(user))
            {
                m_Moderators.Remove(user);

                if (moderator != null)
                    user.SendMessage(49, moderator.Username); // %1 has removed you from the list of conference moderators.

                SendMessage(47, user, user.Username); // %1 is no longer a conference moderator.
                SendCommand(ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);
            }
        }

        public void SendMessage(int number)
        {
            SendMessage(number, null, null, null);
        }

        public void SendMessage(int number, string param1)
        {
            SendMessage(number, null, param1, null);
        }

        public void SendMessage(int number, string param1, string param2)
        {
            SendMessage(number, null, param1, param2);
        }

        public void SendMessage(int number, ChatUser initiator)
        {
            SendMessage(number, initiator, null, null);
        }

        public void SendMessage(int number, ChatUser initiator, string param1)
        {
            SendMessage(number, initiator, param1, null);
        }

        public void SendMessage(int number, ChatUser initiator, string param1, string param2)
        {
            for (int i = 0; i < m_Users.Count; ++i)
            {
                ChatUser user = m_Users[i];

                if (user == initiator)
                    continue;

                if (user.CheckOnline())
                    user.SendMessage(number, param1, param2);
                else if (!Contains(user))
                    --i;
            }
        }

        public void SendIgnorableMessage(int number, ChatUser from, string param1, string param2)
        {
            for (int i = 0; i < m_Users.Count; ++i)
            {
                ChatUser user = m_Users[i];

                if (user.IsIgnored(from))
                    continue;

                if (user.CheckOnline())
                    user.SendMessage(number, from.Mobile, param1, param2);
                else if (!Contains(user))
                    --i;
            }
        }

        public void SendCommand(ChatCommand command)
        {
            SendCommand(command, null, null, null);
        }

        public void SendCommand(ChatCommand command, string param1)
        {
            SendCommand(command, null, param1, null);
        }

        public void SendCommand(ChatCommand command, string param1, string param2)
        {
            SendCommand(command, null, param1, param2);
        }

        public void SendCommand(ChatCommand command, ChatUser initiator)
        {
            SendCommand(command, initiator, null, null);
        }

        public void SendCommand(ChatCommand command, ChatUser initiator, string param1)
        {
            SendCommand(command, initiator, param1, null);
        }

        public void SendCommand(ChatCommand command, ChatUser initiator, string param1, string param2)
        {
            for (int i = 0; i < m_Users.Count; ++i)
            {
                ChatUser user = m_Users[i];

                if (user == initiator)
                    continue;

                if (user.CheckOnline())
                    ChatSystem.SendCommandTo(user.Mobile, command, param1, param2);
                else if (!Contains(user))
                    --i;
            }
        }

        public void SendUsersTo(ChatUser to)
        {
            for (int i = 0; i < m_Users.Count; ++i)
            {
                ChatUser user = m_Users[i];

                ChatSystem.SendCommandTo(to.Mobile, ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);
            }
        }

        private static List<Channel> m_Channels = new List<Channel>();

        public static List<Channel> Channels
        {
            get
            {
                return m_Channels;
            }
        }

        public static void SendChannelsTo(ChatUser user)
        {
            for (int i = 0; i < m_Channels.Count; ++i)
            {
                Channel channel = m_Channels[i];

                if (!channel.IsBanned(user))
                    ChatSystem.SendCommandTo(user.Mobile, ChatCommand.AddChannel, channel.Name, "0");
            }
        }

        public static Channel AddChannel(string name)
        {
            return AddChannel(name, null);
        }

        public static Channel AddChannel(string name, string password)
        {
            Channel channel = FindChannelByName(name);

            if (channel == null)
            {
                channel = new Channel(name, password);
                m_Channels.Add(channel);
            }

            ChatUser.GlobalSendCommand(ChatCommand.AddChannel, name, "0");

            return channel;
        }

        public static void RemoveChannel(string name)
        {
            RemoveChannel(FindChannelByName(name));
        }

        public static void RemoveChannel(Channel channel)
        {
            if (channel == null)
                return;

            if (m_Channels.Contains(channel) && channel.m_Users.Count == 0)
            {
                ChatUser.GlobalSendCommand(ChatCommand.RemoveChannel, channel.Name);

                channel.m_Moderators.Clear();
                channel.m_Voices.Clear();

                m_Channels.Remove(channel);
            }
        }

        public static Channel FindChannelByName(string name)
        {
            for (int i = 0; i < m_Channels.Count; ++i)
            {
                Channel channel = m_Channels[i];

                if (channel.m_Name == name)
                    return channel;
            }

            return null;
        }

        public static void Initialize()
        {
            AddStaticChannel("Newbie Help");
        }

        public static void AddStaticChannel(string name)
        {
            AddChannel(name).AlwaysAvailable = true;
        }
    }
}

#endregion