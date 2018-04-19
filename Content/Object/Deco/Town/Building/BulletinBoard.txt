using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Prompts;

namespace Server.Items
{
    /// <summary>
    /// Community Bulletin Board
    /// these are the boards a player would see and post to at a bank
    /// </summary>
    [Flipable(0x1E5E, 0x1E5F)]
    public class BulletinBoard : BaseBulletinBoard
    {
        [Constructable]
        public BulletinBoard()
            : base(0x1E5E)
        {
        }

        public BulletinBoard(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public abstract class BaseBulletinBoard : Item
    {
        private string m_BoardName;

        [CommandProperty(AccessLevel.GameMaster)]
        public string BoardName
        {
            get { return m_BoardName; }
            set { m_BoardName = value; }
        }

        public BaseBulletinBoard(int itemID)
            : base(itemID)
        {
            m_BoardName = "bulletin board";
            Movable = false;
        }

        // Threads will be removed six hours after the last post was made
        private static TimeSpan ThreadDeletionTime = TimeSpan.FromHours(6.0);

        // A player may only create a thread once every two minutes
        private static TimeSpan ThreadCreateTime = TimeSpan.FromMinutes(2.0);

        // A player may only reply once every thirty seconds
        private static TimeSpan ThreadReplyTime = TimeSpan.FromSeconds(30.0);

        public static bool CheckTime(DateTime time, TimeSpan range)
        {
            return (time + range) < DateTime.UtcNow;
        }

        public static string FormatTS(TimeSpan ts)
        {
            int totalSeconds = (int)ts.TotalSeconds;
            int seconds = totalSeconds % 60;
            int minutes = totalSeconds / 60;

            if (minutes != 0 && seconds != 0)
                return String.Format("{0} minute{1} and {2} second{3}", minutes, minutes == 1 ? "" : "s", seconds, seconds == 1 ? "" : "s");
            else if (minutes != 0)
                return String.Format("{0} minute{1}", minutes, minutes == 1 ? "" : "s");
            else
                return String.Format("{0} second{1}", seconds, seconds == 1 ? "" : "s");
        }

        public virtual void Cleanup()
        {
            List<Item> items = this.Items;

            for (int i = items.Count - 1; i >= 0; --i)
            {
                if (i >= items.Count)
                    continue;

                BulletinMessage msg = items[i] as BulletinMessage;

                if (msg == null)
                    continue;

                if (msg.Thread == null && CheckTime(msg.LastPostTime, ThreadDeletionTime))
                {
                    msg.Delete();
                    RecurseDelete(msg); // A root-level thread has expired
                }
            }
        }

        private void RecurseDelete(BulletinMessage msg)
        {
            List<Item> found = new List<Item>();
            List<Item> items = this.Items;

            for (int i = items.Count - 1; i >= 0; --i)
            {
                if (i >= items.Count)
                    continue;

                BulletinMessage check = items[i] as BulletinMessage;

                if (check == null)
                    continue;

                if (check.Thread == msg)
                {
                    check.Delete();
                    found.Add(check);
                }
            }

            for (int i = 0; i < found.Count; ++i)
                RecurseDelete((BulletinMessage)found[i]);
        }

        public virtual bool GetLastPostTime(Mobile poster, bool onlyCheckRoot, ref DateTime lastPostTime)
        {
            List<Item> items = this.Items;
            bool wasSet = false;

            for (int i = 0; i < items.Count; ++i)
            {
                BulletinMessage msg = items[i] as BulletinMessage;

                if (msg == null || msg.Poster != poster)
                    continue;

                if (onlyCheckRoot && msg.Thread != null)
                    continue;

                if (msg.Time > lastPostTime)
                {
                    wasSet = true;
                    lastPostTime = msg.Time;
                }
            }

            return wasSet;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (CheckRange(from))
            {
                Cleanup();

                NetState state = from.NetState;

                state.Send(new BBDisplayBoard(this));
                if (state.ContainerGridLines)
                    state.Send(new ContainerContent6017(from, this));
                else
                    state.Send(new ContainerContent(from, this));
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public virtual bool CheckRange(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            return (from.Map == this.Map && from.InRange(GetWorldLocation(), 2));
        }

        public void PostMessage(Mobile from, BulletinMessage thread, string subject, string[] lines)
        {
            if (thread != null)
                thread.LastPostTime = DateTime.UtcNow;

            AddItem(new BulletinMessage(from, thread, subject, lines));
        }

        public BaseBulletinBoard(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((string)m_BoardName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_BoardName = reader.ReadString();
                        break;
                    }
            }
        }

        public static void Initialize()
        {
            PacketHandlers.Register(0x71, 0, true, new OnPacketReceive(BBClientRequest));
        }

        public static void BBClientRequest(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            int packetID = pvSrc.ReadByte();
            BaseBulletinBoard board = World.FindItem(pvSrc.ReadInt32()) as BaseBulletinBoard;

            if (board == null || !board.CheckRange(from))
                return;

            switch (packetID)
            {
                case 3: BBRequestContent(from, board, pvSrc); break;
                case 4: BBRequestHeader(from, board, pvSrc); break;
                case 5: BBPostMessage(from, board, pvSrc); break;
                case 6: BBRemoveMessage(from, board, pvSrc); break;
            }
        }

        public static void BBRequestContent(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
        {
            BulletinMessage msg = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

            if (msg == null || msg.Parent != board)
                return;

            from.Send(new BBMessageContent(board, msg));
        }

        public static void BBRequestHeader(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
        {
            BulletinMessage msg = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

            if (msg == null || msg.Parent != board)
                return;

            from.Send(new BBMessageHeader(board, msg));
        }

        public static void BBPostMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
        {
            BulletinMessage thread = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

            if (thread != null && thread.Parent != board)
                thread = null;

            int breakout = 0;

            while (thread != null && thread.Thread != null && breakout++ < 10)
                thread = thread.Thread;

            DateTime lastPostTime = DateTime.MinValue;

            if (board.GetLastPostTime(from, (thread == null), ref lastPostTime))
            {
                if (!CheckTime(lastPostTime, (thread == null ? ThreadCreateTime : ThreadReplyTime)))
                {
                    if (thread == null)
                        from.SendMessage("You must wait {0} before creating a new thread.", FormatTS(ThreadCreateTime));
                    else
                        from.SendMessage("You must wait {0} before replying to another thread.", FormatTS(ThreadReplyTime));

                    return;
                }
            }

            string subject = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

            if (subject.Length == 0)
                return;

            string[] lines = new string[pvSrc.ReadByte()];

            if (lines.Length == 0)
                return;

            for (int i = 0; i < lines.Length; ++i)
                lines[i] = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

            board.PostMessage(from, thread, subject, lines);
        }

        public static void BBRemoveMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
        {
            BulletinMessage msg = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

            if (msg == null || msg.Parent != board)
                return;

            if (from.AccessLevel < AccessLevel.GameMaster && msg.Poster != from)
                return;

            msg.Delete();
        }
    }

    public struct BulletinEquip
    {
        public int itemID;
        public int hue;

        public BulletinEquip(int itemID, int hue)
        {
            this.itemID = itemID;
            this.hue = hue;
        }
    }

    public class BulletinMessage : Item
    {
        private Mobile m_Poster;
        private string m_Subject;
        private DateTime m_Time, m_LastPostTime;
        private BulletinMessage m_Thread;
        private string m_PostedName;
        private int m_PostedBody;
        private int m_PostedHue;
        private BulletinEquip[] m_PostedEquip;
        private string[] m_Lines;

        public string GetTimeAsString()
        {
            return m_Time.ToString("MMM dd, yyyy");
        }

        public override bool CheckTarget(Mobile from, Server.Targeting.Target targ, object targeted)
        {
            return false;
        }

        public override bool IsAccessibleTo(Mobile check)
        {
            return false;
        }

        public BulletinMessage(Mobile poster, BulletinMessage thread, string subject, string[] lines)
            : base(0xEB0)
        {
            Movable = false;

            m_Poster = poster;
            m_Subject = subject;
            m_Time = DateTime.UtcNow;
            m_LastPostTime = m_Time;
            m_Thread = thread;
            m_PostedName = m_Poster.Name;
            m_PostedBody = m_Poster.Body;
            m_PostedHue = m_Poster.Hue;
            m_Lines = lines;

            List<BulletinEquip> list = new List<BulletinEquip>();

            for (int i = 0; i < poster.Items.Count; ++i)
            {
                Item item = poster.Items[i];

                if (item.Layer >= Layer.OneHanded && item.Layer <= Layer.Mount)
                    list.Add(new BulletinEquip(item.ItemID, item.Hue));
            }

            m_PostedEquip = list.ToArray();
        }

        public Mobile Poster { get { return m_Poster; } }
        public BulletinMessage Thread { get { return m_Thread; } }
        public string Subject { get { return m_Subject; } }
        public DateTime Time { get { return m_Time; } }
        public DateTime LastPostTime { get { return m_LastPostTime; } set { m_LastPostTime = value; } }
        public string PostedName { get { return m_PostedName; } }
        public int PostedBody { get { return m_PostedBody; } }
        public int PostedHue { get { return m_PostedHue; } }
        public BulletinEquip[] PostedEquip { get { return m_PostedEquip; } }
        public string[] Lines { get { return m_Lines; } }

        public BulletinMessage(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((Mobile)m_Poster);
            writer.Write((string)m_Subject);
            writer.Write((DateTime)m_Time);
            writer.Write((DateTime)m_LastPostTime);
            writer.Write((bool)(m_Thread != null));
            writer.Write((Item)m_Thread);
            writer.Write((string)m_PostedName);
            writer.Write((int)m_PostedBody);
            writer.Write((int)m_PostedHue);

            writer.Write((int)m_PostedEquip.Length);

            for (int i = 0; i < m_PostedEquip.Length; ++i)
            {
                writer.Write((int)m_PostedEquip[i].itemID);
                writer.Write((int)m_PostedEquip[i].hue);
            }

            writer.Write((int)m_Lines.Length);

            for (int i = 0; i < m_Lines.Length; ++i)
                writer.Write((string)m_Lines[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                case 0:
                    {
                        m_Poster = reader.ReadMobile();
                        m_Subject = reader.ReadString();
                        m_Time = reader.ReadDateTime();
                        m_LastPostTime = reader.ReadDateTime();
                        bool hasThread = reader.ReadBool();
                        m_Thread = reader.ReadItem() as BulletinMessage;
                        m_PostedName = reader.ReadString();
                        m_PostedBody = reader.ReadInt();
                        m_PostedHue = reader.ReadInt();

                        m_PostedEquip = new BulletinEquip[reader.ReadInt()];

                        for (int i = 0; i < m_PostedEquip.Length; ++i)
                        {
                            m_PostedEquip[i].itemID = reader.ReadInt();
                            m_PostedEquip[i].hue = reader.ReadInt();
                        }

                        m_Lines = new string[reader.ReadInt()];

                        for (int i = 0; i < m_Lines.Length; ++i)
                            m_Lines[i] = reader.ReadString();

                        if (hasThread && m_Thread == null)
                            Delete();

                        if (version == 0)
                            ValidationQueue<BulletinMessage>.Add(this);

                        break;
                    }
            }
        }

        public void Validate()
        {
            if (!(Parent is BulletinBoard && ((BulletinBoard)Parent).Items.Contains(this)))
                Delete();
        }
    }

    public class BBDisplayBoard : Packet
    {
        public BBDisplayBoard(BaseBulletinBoard board)
            : base(0x71)
        {
            string name = board.BoardName;

            if (name == null)
                name = "";

            EnsureCapacity(38);

            byte[] buffer = Utility.UTF8.GetBytes(name);

            m_Stream.Write((byte)0x00); // PacketID
            m_Stream.Write((int)board.Serial); // Bulletin board serial

            // Bulletin board name
            if (buffer.Length >= 29)
            {
                m_Stream.Write(buffer, 0, 29);
                m_Stream.Write((byte)0);
            }
            else
            {
                m_Stream.Write(buffer, 0, buffer.Length);
                m_Stream.Fill(30 - buffer.Length);
            }
        }
    }

    public class BBMessageHeader : Packet
    {
        public BBMessageHeader(BaseBulletinBoard board, BulletinMessage msg)
            : base(0x71)
        {
            string poster = SafeString(msg.PostedName);
            string subject = SafeString(msg.Subject);
            string time = SafeString(msg.GetTimeAsString());

            EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

            m_Stream.Write((byte)0x01); // PacketID
            m_Stream.Write((int)board.Serial); // Bulletin board serial
            m_Stream.Write((int)msg.Serial); // Message serial

            BulletinMessage thread = msg.Thread;

            if (thread == null)
                m_Stream.Write((int)0); // Thread serial--root
            else
                m_Stream.Write((int)thread.Serial); // Thread serial--parent

            WriteString(poster);
            WriteString(subject);
            WriteString(time);
        }

        public void WriteString(string v)
        {
            byte[] buffer = Utility.UTF8.GetBytes(v);
            int len = buffer.Length + 1;

            if (len > 255)
                len = 255;

            m_Stream.Write((byte)len);
            m_Stream.Write(buffer, 0, len - 1);
            m_Stream.Write((byte)0);
        }

        public string SafeString(string v)
        {
            if (v == null)
                return String.Empty;

            return v;
        }
    }

    public class BBMessageContent : Packet
    {
        public BBMessageContent(BaseBulletinBoard board, BulletinMessage msg)
            : base(0x71)
        {
            string poster = SafeString(msg.PostedName);
            string subject = SafeString(msg.Subject);
            string time = SafeString(msg.GetTimeAsString());

            EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

            m_Stream.Write((byte)0x02); // PacketID
            m_Stream.Write((int)board.Serial); // Bulletin board serial
            m_Stream.Write((int)msg.Serial); // Message serial

            WriteString(poster);
            WriteString(subject);
            WriteString(time);

            m_Stream.Write((short)msg.PostedBody);
            m_Stream.Write((short)msg.PostedHue);

            int len = msg.PostedEquip.Length;

            if (len > 255)
                len = 255;

            m_Stream.Write((byte)len);

            for (int i = 0; i < len; ++i)
            {
                BulletinEquip eq = msg.PostedEquip[i];

                m_Stream.Write((short)eq.itemID);
                m_Stream.Write((short)eq.hue);
            }

            len = msg.Lines.Length;

            if (len > 255)
                len = 255;

            m_Stream.Write((byte)len);

            for (int i = 0; i < len; ++i)
                WriteString(msg.Lines[i], true);
        }

        public void WriteString(string v)
        {
            WriteString(v, false);
        }

        public void WriteString(string v, bool padding)
        {
            byte[] buffer = Utility.UTF8.GetBytes(v);
            int tail = padding ? 2 : 1;
            int len = buffer.Length + tail;

            if (len > 255)
                len = 255;

            m_Stream.Write((byte)len);
            m_Stream.Write(buffer, 0, len - tail);

            if (padding)
                m_Stream.Write((short)0); // padding compensates for a client bug
            else
                m_Stream.Write((byte)0);
        }

        public string SafeString(string v)
        {
            if (v == null)
                return String.Empty;

            return v;
        }
    }

    /// <summary>
    /// Personal Bulletin Board
    /// these are the boards a player would use and post to at a house
    /// </summary>
    public class PlayerBBSouth : BasePlayerBB
    {
        public override int LabelNumber { get { return 1062421; } } // bulletin board (south)

        [Constructable]
        public PlayerBBSouth()
            : base(0x2311)
        {
            Weight = 15.0;
        }

        public PlayerBBSouth(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PlayerBBEast : BasePlayerBB
    {
        public override int LabelNumber { get { return 1062420; } } // bulletin board (east)

        [Constructable]
        public PlayerBBEast()
            : base(0x2312)
        {
            Weight = 15.0;
        }

        public PlayerBBEast(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public abstract class BasePlayerBB : Item, ISecurable
    {
        private PlayerBBMessage m_Greeting;
        private List<PlayerBBMessage> m_Messages;
        private string m_Title;
        private SecureLevel m_Level;

        public List<PlayerBBMessage> Messages
        {
            get { return m_Messages; }
        }

        public PlayerBBMessage Greeting
        {
            get { return m_Greeting; }
            set { m_Greeting = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        public BasePlayerBB(int itemID)
            : base(itemID)
        {
            m_Messages = new List<PlayerBBMessage>();
            m_Level = SecureLevel.Anyone;
        }

        public BasePlayerBB(Serial serial)
            : base(serial)
        {
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.Write((int)m_Level);

            writer.Write(m_Title);

            if (m_Greeting != null)
            {
                writer.Write(true);
                m_Greeting.Serialize(writer);
            }
            else
            {
                writer.Write(false);
            }

            writer.WriteEncodedInt(m_Messages.Count);

            for (int i = 0; i < m_Messages.Count; ++i)
                m_Messages[i].Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Level = (SecureLevel)reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 1)
                            m_Level = SecureLevel.Anyone;

                        m_Title = reader.ReadString();

                        if (reader.ReadBool())
                            m_Greeting = new PlayerBBMessage(reader);

                        int count = reader.ReadEncodedInt();

                        m_Messages = new List<PlayerBBMessage>(count);

                        for (int i = 0; i < count; ++i)
                            m_Messages.Add(new PlayerBBMessage(reader));

                        break;
                    }
            }
        }

        public static bool CheckAccess(BaseHouse house, Mobile from)
        {
            if (house.Public || !house.IsAosRules)
                return !house.IsBanned(from);

            return house.HasAccess(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house == null || !house.IsLockedDown(this))
                from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
            else if (!from.InRange(this.GetWorldLocation(), 2) || !from.InLOS(this))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (CheckAccess(house, from))
                from.SendGump(new PlayerBBGump(from, house, this, 0));
        }

        public class PostPrompt : Prompt
        {
            private int m_Page;
            private BaseHouse m_House;
            private BasePlayerBB m_Board;
            private bool m_Greeting;

            public PostPrompt(int page, BaseHouse house, BasePlayerBB board, bool greeting)
            {
                m_Page = page;
                m_House = house;
                m_Board = board;
                m_Greeting = greeting;
            }

            public override void OnCancel(Mobile from)
            {
                OnResponse(from, "");
            }

            public override void OnResponse(Mobile from, string text)
            {
                int page = m_Page;
                BaseHouse house = m_House;
                BasePlayerBB board = m_Board;

                if (house == null || !house.IsLockedDown(board))
                {
                    from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
                    return;
                }
                else if (!from.InRange(board.GetWorldLocation(), 2) || !from.InLOS(board))
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                    return;
                }
                else if (!CheckAccess(house, from))
                {
                    from.SendLocalizedMessage(1062398); // You are not allowed to post to this bulletin board.
                    return;
                }
                else if (m_Greeting && !house.IsOwner(from))
                {
                    return;
                }

                text = text.Trim();

                if (text.Length > 255)
                    text = text.Substring(0, 255);

                if (text.Length > 0)
                {
                    PlayerBBMessage message = new PlayerBBMessage(DateTime.UtcNow, from, text);

                    if (m_Greeting)
                    {
                        board.Greeting = message;
                    }
                    else
                    {
                        board.Messages.Add(message);

                        if (board.Messages.Count > 50)
                        {
                            board.Messages.RemoveAt(0);

                            if (page > 0)
                                --page;
                        }
                    }
                }

                from.SendGump(new PlayerBBGump(from, house, board, page));
            }
        }

        public class SetTitlePrompt : Prompt
        {
            private int m_Page;
            private BaseHouse m_House;
            private BasePlayerBB m_Board;

            public SetTitlePrompt(int page, BaseHouse house, BasePlayerBB board)
            {
                m_Page = page;
                m_House = house;
                m_Board = board;
            }

            public override void OnCancel(Mobile from)
            {
                OnResponse(from, "");
            }

            public override void OnResponse(Mobile from, string text)
            {
                int page = m_Page;
                BaseHouse house = m_House;
                BasePlayerBB board = m_Board;

                if (house == null || !house.IsLockedDown(board))
                {
                    from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
                    return;
                }
                else if (!from.InRange(board.GetWorldLocation(), 2) || !from.InLOS(board))
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                    return;
                }
                else if (!CheckAccess(house, from))
                {
                    from.SendLocalizedMessage(1062398); // You are not allowed to post to this bulletin board.
                    return;
                }

                text = text.Trim();

                if (text.Length > 255)
                    text = text.Substring(0, 255);

                if (text.Length > 0)
                    board.Title = text;

                from.SendGump(new PlayerBBGump(from, house, board, page));
            }
        }
    }

    public class PlayerBBMessage
    {
        private DateTime m_Time;
        private Mobile m_Poster;
        private string m_Message;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime Time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Poster
        {
            get { return m_Poster; }
            set { m_Poster = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        public PlayerBBMessage(DateTime time, Mobile poster, string message)
        {
            m_Time = time;
            m_Poster = poster;
            m_Message = message;
        }

        public PlayerBBMessage(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_Time = reader.ReadDateTime();
                        m_Poster = reader.ReadMobile();
                        m_Message = reader.ReadString();
                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.Write(m_Time);
            writer.Write(m_Poster);
            writer.Write(m_Message);
        }
    }

    public class PlayerBBGump : Gump
    {
        private int m_Page;
        private Mobile m_From;
        private BaseHouse m_House;
        private BasePlayerBB m_Board;

        private const int LabelColor = 0x7FFF;
        private const int LabelHue = 1153;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int page = m_Page;
            Mobile from = m_From;
            BaseHouse house = m_House;
            BasePlayerBB board = m_Board;

            if (house == null || !house.IsLockedDown(board))
            {
                from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
                return;
            }
            else if (!from.InRange(board.GetWorldLocation(), 2) || !from.InLOS(board))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }
            else if (!BasePlayerBB.CheckAccess(house, from))
            {
                from.SendLocalizedMessage(1062398); // You are not allowed to post to this bulletin board.
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // Post message
                    {
                        from.Prompt = new BasePlayerBB.PostPrompt(page, house, board, false);
                        from.SendLocalizedMessage(1062397); // Please enter your message:

                        break;
                    }
                case 2: // Set title
                    {
                        if (house.IsOwner(from))
                        {
                            from.Prompt = new BasePlayerBB.SetTitlePrompt(page, house, board);
                            from.SendLocalizedMessage(1062402); // Enter new title:
                        }

                        break;
                    }
                case 3: // Post greeting
                    {
                        if (house.IsOwner(from))
                        {
                            from.Prompt = new BasePlayerBB.PostPrompt(page, house, board, true);
                            from.SendLocalizedMessage(1062404); // Enter new greeting (this will always be the first post):
                        }

                        break;
                    }
                case 4: // Scroll up
                    {
                        if (page == 0)
                            page = board.Messages.Count;
                        else
                            page -= 1;

                        from.SendGump(new PlayerBBGump(from, house, board, page));

                        break;
                    }
                case 5: // Scroll down
                    {
                        page += 1;
                        page %= board.Messages.Count + 1;

                        from.SendGump(new PlayerBBGump(from, house, board, page));

                        break;
                    }
                case 6: // Banish poster
                    {
                        if (house.IsOwner(from))
                        {
                            if (page >= 1 && page <= board.Messages.Count)
                            {
                                PlayerBBMessage message = (PlayerBBMessage)board.Messages[page - 1];
                                Mobile poster = message.Poster;

                                if (poster == null)
                                {
                                    from.SendGump(new PlayerBBGump(from, house, board, page));
                                    return;
                                }

                                if (poster.AccessLevel > AccessLevel.Player && from.AccessLevel <= poster.AccessLevel)
                                {
                                    from.SendLocalizedMessage(501354); // Uh oh...a bigger boot may be required.
                                }
                                else if (house.IsFriend(poster))
                                {
                                    from.SendLocalizedMessage(1060750); // That person is a friend, co-owner, or owner of this house, and therefore cannot be banished!
                                }
                                else if (poster is PlayerVendor)
                                {
                                    from.SendLocalizedMessage(501351); // You cannot eject a vendor.
                                }
                                else if (house.Bans.Count >= BaseHouse.MaxBans)
                                {
                                    from.SendLocalizedMessage(501355); // The ban limit for this house has been reached!
                                }
                                else if (house.IsBanned(poster))
                                {
                                    from.SendLocalizedMessage(501356); // This person is already banned!
                                }
                                else if (poster is BaseCreature && ((BaseCreature)poster).NoHouseRestrictions)
                                {
                                    from.SendLocalizedMessage(1062040); // You cannot ban that.
                                }
                                else
                                {
                                    if (!house.Bans.Contains(poster))
                                        house.Bans.Add(poster);

                                    from.SendLocalizedMessage(1062417); // That person has been banned from this house.

                                    if (house.IsInside(poster) && !BasePlayerBB.CheckAccess(house, poster))
                                        poster.MoveToWorld(house.BanLocation, house.Map);
                                }
                            }

                            from.SendGump(new PlayerBBGump(from, house, board, page));
                        }

                        break;
                    }
                case 7: // Delete message
                    {
                        if (house.IsOwner(from))
                        {
                            if (page >= 1 && page <= board.Messages.Count)
                                board.Messages.RemoveAt(page - 1);

                            from.SendGump(new PlayerBBGump(from, house, board, 0));
                        }

                        break;
                    }
                case 8: // Post props
                    {
                        if (from.AccessLevel >= AccessLevel.GameMaster)
                        {
                            PlayerBBMessage message = board.Greeting;

                            if (page >= 1 && page <= board.Messages.Count)
                                message = (PlayerBBMessage)board.Messages[page - 1];

                            from.SendGump(new PlayerBBGump(from, house, board, page));
                            from.SendGump(new PropertiesGump(from, message));
                        }

                        break;
                    }
            }
        }

        public PlayerBBGump(Mobile from, BaseHouse house, BasePlayerBB board, int page)
            : base(50, 10)
        {
            from.CloseGump(typeof(PlayerBBGump));

            m_Page = page;
            m_From = from;
            m_House = house;
            m_Board = board;

            AddPage(0);

            AddImage(30, 30, 5400);

            AddButton(393, 145, 2084, 2084, 4, GumpButtonType.Reply, 0); // Scroll up
            AddButton(390, 371, 2085, 2085, 5, GumpButtonType.Reply, 0); // Scroll down

            AddButton(32, 183, 5412, 5413, 1, GumpButtonType.Reply, 0); // Post message

            if (house.IsOwner(from))
            {
                AddButton(63, 90, 5601, 5605, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(81, 89, 230, 20, 1062400, LabelColor, false, false); // Set title

                AddButton(63, 109, 5601, 5605, 3, GumpButtonType.Reply, 0);
                AddHtmlLocalized(81, 108, 230, 20, 1062401, LabelColor, false, false); // Post greeting
            }

            string title = board.Title;

            if (title != null)
                AddHtml(183, 68, 180, 23, title, false, false);

            AddHtmlLocalized(385, 89, 60, 20, 1062409, LabelColor, false, false); // Post

            AddLabel(440, 89, LabelHue, page.ToString());
            AddLabel(455, 89, LabelHue, "/");
            AddLabel(470, 89, LabelHue, board.Messages.Count.ToString());

            PlayerBBMessage message = board.Greeting;

            if (page >= 1 && page <= board.Messages.Count)
                message = (PlayerBBMessage)board.Messages[page - 1];

            AddImageTiled(150, 220, 240, 1, 2700); // Separator

            AddHtmlLocalized(150, 180, 100, 20, 1062405, 16715, false, false); // Posted On:
            AddHtmlLocalized(150, 200, 100, 20, 1062406, 16715, false, false); // Posted By:

            if (message != null)
            {
                AddHtml(255, 180, 150, 20, message.Time.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                Mobile poster = message.Poster;
                string name = (poster == null ? null : poster.Name);

                if (name == null || (name = name.Trim()).Length == 0)
                    name = "Someone";

                AddHtml(255, 200, 150, 20, name, false, false);

                string body = message.Message;

                if (body == null)
                    body = "";

                AddHtml(150, 240, 250, 100, body, false, false);

                if (message != board.Greeting && house.IsOwner(from))
                {
                    AddButton(130, 395, 1209, 1210, 6, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(150, 393, 150, 20, 1062410, LabelColor, false, false); // Banish Poster

                    AddButton(310, 395, 1209, 1210, 7, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(330, 393, 150, 20, 1062411, LabelColor, false, false); // Delete Message
                }

                if (from.AccessLevel >= AccessLevel.GameMaster)
                    AddButton(135, 242, 1209, 1210, 8, GumpButtonType.Reply, 0); // Post props
            }
        }
    }
}