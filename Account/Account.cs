﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using Server;
using Server.Accounting;
using Server.Commands;
using Server.Engines.Help;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;

namespace Server.Accounting
{
    public class Account : IAccount, IComparable, IComparable<Account>
    {
        public static readonly TimeSpan YoungDuration = TimeSpan.FromHours(40.0);

        public static readonly TimeSpan InactiveDuration = TimeSpan.FromDays(180.0);

        public static readonly TimeSpan EmptyInactiveDuration = TimeSpan.FromDays(30.0);

        public static void Configure()
        {
            CommandSystem.Register("ConvertCurrency", AccessLevel.Owner, ConvertCurrency);
        }

        private static void ConvertCurrency(CommandEventArgs e)
        {
            e.Mobile.SendMessage(
                "Converting All Banked Gold from {0} to {1}.  Please wait...",
                AccountGold.Enabled ? "checks and coins" : "account treasury",
                AccountGold.Enabled ? "account treasury" : "checks and coins");

            NetState.Pause();

            double found = 0.0, converted = 0.0;

            try
            {
                BankBox box;
                List<Gold> gold;
                List<BankCheck> checks;
                long share = 0, shared;
                int diff;

                foreach (var a in Accounts.GetAccounts().OfType<Account>().Where(a => a.Count > 0))
                {
                    try
                    {
                        if (!AccountGold.Enabled)
                        {
                            share = (int)Math.Truncate((a.TotalCurrency / a.Count) * CurrencyThreshold);
                            found += a.TotalCurrency * CurrencyThreshold;
                        }

                        foreach (var m in a.m_Mobiles.Where(m => m != null))
                        {
                            box = m.FindBankNoCreate();

                            if (box == null)
                            {
                                continue;
                            }

                            if (AccountGold.Enabled)
                            {
                                foreach (var o in checks = box.FindItemsByType<BankCheck>())
                                {
                                    found += o.Worth;

                                    if (!a.DepositGold(o.Worth))
                                    {
                                        break;
                                    }

                                    converted += o.Worth;
                                    o.Delete();
                                }

                                checks.Clear();
                                checks.TrimExcess();

                                foreach (var o in gold = box.FindItemsByType<Gold>())
                                {
                                    found += o.Amount;

                                    if (!a.DepositGold(o.Amount))
                                    {
                                        break;
                                    }

                                    converted += o.Amount;
                                    o.Delete();
                                }

                                gold.Clear();
                                gold.TrimExcess();
                            }
                            else
                            {
                                shared = share;

                                while (shared > 0)
                                {
                                    if (shared > 60000)
                                    {
                                        diff = (int)Math.Min(10000000, shared);

                                        if (a.WithdrawGold(diff))
                                        {
                                            box.DropItem(new BankCheck(diff));
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        diff = (int)Math.Min(60000, shared);

                                        if (a.WithdrawGold(diff))
                                        {
                                            box.DropItem(new Gold(diff));
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    converted += diff;
                                    shared -= diff;
                                }
                            }

                            box.UpdateTotals();
                        }
                    }
                    catch
                    { }
                }
            }
            catch
            { }

            NetState.Resume();

            e.Mobile.SendMessage("Operation complete: {0:#,0} of {1:#,0} Gold has been converted in total.", converted, found);
        }

        private string m_Username, m_Email, m_PlainPassword, m_CryptPassword, m_NewCryptPassword;
        private AccessLevel m_AccessLevel;
        private int m_Flags;
        private DateTime m_Created, m_LastLogin;
        private TimeSpan m_TotalGameTime;
        private List<AccountComment> m_Comments;
        private List<AccountTag> m_Tags;
        private Mobile[] m_Mobiles;
        private string[] m_IPRestrictions;
        private IPAddress[] m_LoginIPs;
        private HardwareInfo m_HardwareInfo;

        /// <summary>
        /// Deletes the account, all characters of the account, and all houses of those characters
        /// </summary>
        public void Delete()
        {
            for (int i = 0; i < this.Length; ++i)
            {
                Mobile m = this[i];

                if (m == null)
                    continue;

                List<BaseHouse> list = BaseHouse.GetHouses(m);

                for (int j = 0; j < list.Count; ++j)
                    list[j].Delete();

                m.Delete();

                m.Account = null;
                m_Mobiles[i] = null;
            }

            if (m_LoginIPs.Length != 0 && AccountHandler.IPTable.ContainsKey(m_LoginIPs[0]))
                --AccountHandler.IPTable[m_LoginIPs[0]];

            Accounts.Remove(m_Username);
        }

        /// <summary>
        /// Object detailing information about the hardware of the last person to log into this account
        /// </summary>
        public HardwareInfo HardwareInfo
        {
            get { return m_HardwareInfo; }
            set { m_HardwareInfo = value; }
        }

        /// <summary>
        /// List of IP addresses for restricted access. '*' wildcard supported. If the array contains zero entries, all IP addresses are allowed.
        /// </summary>
        public string[] IPRestrictions
        {
            get { return m_IPRestrictions; }
            set { m_IPRestrictions = value; }
        }

        /// <summary>
        /// List of IP addresses which have successfully logged into this account.
        /// </summary>
        public IPAddress[] LoginIPs
        {
            get { return m_LoginIPs; }
            set { m_LoginIPs = value; }
        }

        /// <summary>
        /// List of account comments. Type of contained objects is AccountComment.
        /// </summary>
        public List<AccountComment> Comments
        {
            get { if (m_Comments == null) m_Comments = new List<AccountComment>(); return m_Comments; }
        }

        /// <summary>
        /// List of account tags. Type of contained objects is AccountTag.
        /// </summary>
        public List<AccountTag> Tags
        {
            get { if (m_Tags == null) m_Tags = new List<AccountTag>(); return m_Tags; }
        }

        /// <summary>
        /// Account username. Case insensitive validation.
        /// </summary>
        public string Username
        {
            get { return m_Username; }
            set { m_Username = value; }
        }

        /// <summary>
        /// Account email address.
        /// </summary>
        public string Email
        {
            get { return m_Email; }
            set { m_Email = value; }
        }

        /// <summary>
        /// Account password. Plain text. Case sensitive validation. May be null.
        /// </summary>
        public string PlainPassword
        {
            get { return m_PlainPassword; }
            set { m_PlainPassword = value; }
        }

        /// <summary>
        /// Account password. Hashed with MD5. May be null.
        /// </summary>
        public string CryptPassword
        {
            get { return m_CryptPassword; }
            set { m_CryptPassword = value; }
        }

        /// <summary>
        /// Account username and password hashed with SHA1. May be null.
        /// </summary>
        public string NewCryptPassword
        {
            get { return m_NewCryptPassword; }
            set { m_NewCryptPassword = value; }
        }

        /// <summary>
        /// Initial AccessLevel for new characters created on this account.
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return m_AccessLevel; }
            set { m_AccessLevel = value; }
        }

        /// <summary>
        /// Internal bitfield of account flags. Consider using direct access properties (Banned, Young), or GetFlag/SetFlag methods
        /// </summary>
        public int Flags
        {
            get { return m_Flags; }
            set { m_Flags = value; }
        }

        /// <summary>
        /// Gets or sets a flag indiciating if this account is banned.
        /// </summary>
        public bool Banned
        {
            get
            {
                bool isBanned = GetFlag(0);

                if (!isBanned)
                    return false;

                DateTime banTime;
                TimeSpan banDuration;

                if (GetBanTags(out banTime, out banDuration))
                {
                    if (banDuration != TimeSpan.MaxValue && DateTime.UtcNow >= (banTime + banDuration))
                    {
                        SetUnspecifiedBan(null); // clear
                        Banned = false;
                        return false;
                    }
                }

                return true;
            }
            set { SetFlag(0, value); }
        }

        /// <summary>
        /// Gets or sets a flag indicating if the characters created on this account will have the young status.
        /// </summary>
        public bool Young
        {
            get { return !GetFlag(1); }
            set
            {
                SetFlag(1, !value);

                if (m_YoungTimer != null)
                {
                    m_YoungTimer.Stop();
                    m_YoungTimer = null;
                }
            }
        }

        /// <summary>
        /// The date and time of when this account was created.
        /// </summary>
        public DateTime Created
        {
            get { return m_Created; }
        }

        /// <summary>
        /// Gets or sets the date and time when this account was last accessed.
        /// </summary>
        public DateTime LastLogin
        {
            get { return m_LastLogin; }
            set { m_LastLogin = value; }
        }

        /// <summary>
        /// An account is considered inactive based upon LastLogin and InactiveDuration.  If the account is empty, it is based upon EmptyInactiveDuration
        /// </summary>
        public bool Inactive
        {
            get
            {
                if (this.AccessLevel != AccessLevel.Player)
                    return false;

                TimeSpan inactiveLength = DateTime.UtcNow - m_LastLogin;

                return (inactiveLength > ((this.Count == 0) ? EmptyInactiveDuration : InactiveDuration));
            }
        }

        /// <summary>
        /// Gets the total game time of this account, also considering the game time of characters
        /// that have been deleted.
        /// </summary>
        public TimeSpan TotalGameTime
        {
            get
            {
                for (int i = 0; i < m_Mobiles.Length; i++)
                {
                    PlayerMobile m = m_Mobiles[i] as PlayerMobile;

                    if (m != null && m.NetState != null)
                        return m_TotalGameTime + (DateTime.UtcNow - m.SessionStart);
                }

                return m_TotalGameTime;
            }
        }

        /// <summary>
        /// Gets the value of a specific flag in the Flags bitfield.
        /// </summary>
        /// <param name="index">The zero-based flag index.</param>
        public bool GetFlag(int index)
        {
            return (m_Flags & (1 << index)) != 0;
        }

        /// <summary>
        /// Sets the value of a specific flag in the Flags bitfield.
        /// </summary>
        /// <param name="index">The zero-based flag index.</param>
        /// <param name="value">The value to set.</param>
        public void SetFlag(int index, bool value)
        {
            if (value)
                m_Flags |= (1 << index);
            else
                m_Flags &= ~(1 << index);
        }

        /// <summary>
        /// Adds a new tag to this account. This method does not check for duplicate names.
        /// </summary>
        /// <param name="name">New tag name.</param>
        /// <param name="value">New tag value.</param>
        public void AddTag(string name, string value)
        {
            Tags.Add(new AccountTag(name, value));
        }

        /// <summary>
        /// Removes all tags with the specified name from this account.
        /// </summary>
        /// <param name="name">Tag name to remove.</param>
        public void RemoveTag(string name)
        {
            for (int i = Tags.Count - 1; i >= 0; --i)
            {
                if (i >= Tags.Count)
                    continue;

                AccountTag tag = Tags[i];

                if (tag.Name == name)
                    Tags.RemoveAt(i);
            }
        }

        /// <summary>
        /// Modifies an existing tag or adds a new tag if no tag exists.
        /// </summary>
        /// <param name="name">Tag name.</param>
        /// <param name="value">Tag value.</param>
        public void SetTag(string name, string value)
        {
            for (int i = 0; i < Tags.Count; ++i)
            {
                AccountTag tag = Tags[i];

                if (tag.Name == name)
                {
                    tag.Value = value;
                    return;
                }
            }

            AddTag(name, value);
        }

        /// <summary>
        /// Gets the value of a tag -or- null if there are no tags with the specified name.
        /// </summary>
        /// <param name="name">Name of the desired tag value.</param>
        public string GetTag(string name)
        {
            for (int i = 0; i < Tags.Count; ++i)
            {
                AccountTag tag = Tags[i];

                if (tag.Name == name)
                    return tag.Value;
            }

            return null;
        }

        public void SetUnspecifiedBan(Mobile from)
        {
            SetBanTags(from, DateTime.MinValue, TimeSpan.Zero);
        }

        public void SetBanTags(Mobile from, DateTime banTime, TimeSpan banDuration)
        {
            if (from == null)
                RemoveTag("BanDealer");
            else
                SetTag("BanDealer", from.ToString());

            if (banTime == DateTime.MinValue)
                RemoveTag("BanTime");
            else
                SetTag("BanTime", XmlConvert.ToString(banTime, XmlDateTimeSerializationMode.Utc));

            if (banDuration == TimeSpan.Zero)
                RemoveTag("BanDuration");
            else
                SetTag("BanDuration", banDuration.ToString());
        }

        public bool GetBanTags(out DateTime banTime, out TimeSpan banDuration)
        {
            string tagTime = GetTag("BanTime");
            string tagDuration = GetTag("BanDuration");

            if (tagTime != null)
                banTime = Utility.GetXMLDateTime(tagTime, DateTime.MinValue);
            else
                banTime = DateTime.MinValue;

            if (tagDuration == "Infinite")
            {
                banDuration = TimeSpan.MaxValue;
            }
            else if (tagDuration != null)
            {
                banDuration = Utility.ToTimeSpan(tagDuration);
            }
            else
            {
                banDuration = TimeSpan.Zero;
            }

            return (banTime != DateTime.MinValue && banDuration != TimeSpan.Zero);
        }

        private static MD5CryptoServiceProvider m_MD5HashProvider;
        private static SHA1CryptoServiceProvider m_SHA1HashProvider;
        private static byte[] m_HashBuffer;

        public static string HashMD5(string phrase)
        {
            if (m_MD5HashProvider == null)
                m_MD5HashProvider = new MD5CryptoServiceProvider();

            if (m_HashBuffer == null)
                m_HashBuffer = new byte[256];

            int length = Encoding.ASCII.GetBytes(phrase, 0, phrase.Length > 256 ? 256 : phrase.Length, m_HashBuffer, 0);
            byte[] hashed = m_MD5HashProvider.ComputeHash(m_HashBuffer, 0, length);

            return BitConverter.ToString(hashed);
        }

        public static string HashSHA1(string phrase)
        {
            if (m_SHA1HashProvider == null)
                m_SHA1HashProvider = new SHA1CryptoServiceProvider();

            if (m_HashBuffer == null)
                m_HashBuffer = new byte[256];

            int length = Encoding.ASCII.GetBytes(phrase, 0, phrase.Length > 256 ? 256 : phrase.Length, m_HashBuffer, 0);
            byte[] hashed = m_SHA1HashProvider.ComputeHash(m_HashBuffer, 0, length);

            return BitConverter.ToString(hashed);
        }

        public void SetPassword(string plainPassword)
        {
            switch (AccountHandler.ProtectPasswords)
            {
                case PasswordProtection.None:
                    {
                        m_PlainPassword = plainPassword;
                        m_CryptPassword = null;
                        m_NewCryptPassword = null;

                        break;
                    }
                case PasswordProtection.Crypt:
                    {
                        m_PlainPassword = null;
                        m_CryptPassword = HashMD5(plainPassword);
                        m_NewCryptPassword = null;

                        break;
                    }
                default: // PasswordProtection.NewCrypt
                    {
                        m_PlainPassword = null;
                        m_CryptPassword = null;
                        m_NewCryptPassword = HashSHA1(m_Username + plainPassword);

                        break;
                    }
            }
        }

        public bool CheckPassword(string plainPassword)
        {
            bool ok;
            PasswordProtection curProt;

            if (m_PlainPassword != null)
            {
                ok = (m_PlainPassword == plainPassword);
                curProt = PasswordProtection.None;
            }
            else if (m_CryptPassword != null)
            {
                ok = (m_CryptPassword == HashMD5(plainPassword));
                curProt = PasswordProtection.Crypt;
            }
            else
            {
                ok = (m_NewCryptPassword == HashSHA1(m_Username + plainPassword));
                curProt = PasswordProtection.NewCrypt;
            }

            if (ok && curProt != AccountHandler.ProtectPasswords)
                SetPassword(plainPassword);

            return ok;
        }

        private Timer m_YoungTimer;

        public static void Initialize()
        {
            EventSink.Connected += new ConnectedEventHandler(EventSink_Connected);
            EventSink.Disconnected += new DisconnectedEventHandler(EventSink_Disconnected);
            EventSink.Login += new LoginEventHandler(EventSink_Login);
        }

        private static void EventSink_Connected(ConnectedEventArgs e)
        {
            Account acc = e.Mobile.Account as Account;

            if (acc == null)
                return;

            if (acc.Young && acc.m_YoungTimer == null)
            {
                acc.m_YoungTimer = new YoungTimer(acc);
                acc.m_YoungTimer.Start();
            }
        }

        private static void EventSink_Disconnected(DisconnectedEventArgs e)
        {
            Account acc = e.Mobile.Account as Account;

            if (acc == null)
                return;

            if (acc.m_YoungTimer != null)
            {
                acc.m_YoungTimer.Stop();
                acc.m_YoungTimer = null;
            }

            PlayerMobile m = e.Mobile as PlayerMobile;
            if (m == null)
                return;

            acc.m_TotalGameTime += DateTime.UtcNow - m.SessionStart;
        }

        private static void EventSink_Login(LoginEventArgs e)
        {
            PlayerMobile m = e.Mobile as PlayerMobile;

            if (m == null)
                return;

            Account acc = m.Account as Account;

            if (acc == null)
                return;

            if (m.Young && acc.Young)
            {
                TimeSpan ts = YoungDuration - acc.TotalGameTime;
                int hours = Math.Max((int)ts.TotalHours, 0);

                m.SendAsciiMessage("You will enjoy the benefits and relatively safe status of a young player for {0} more hour{1}.", hours, hours != 1 ? "s" : "");
            }
        }

        public void RemoveYoungStatus(int message)
        {
            this.Young = false;

            for (int i = 0; i < m_Mobiles.Length; i++)
            {
                PlayerMobile m = m_Mobiles[i] as PlayerMobile;

                if (m != null && m.Young)
                {
                    m.Young = false;

                    if (m.NetState != null)
                    {
                        if (message > 0)
                            m.SendLocalizedMessage(message);

                        m.SendLocalizedMessage(1019039); // You are no longer considered a young player of Ultima Online, and are no longer subject to the limitations and benefits of being in that caste.
                    }
                }
            }
        }

        public void CheckYoung()
        {
            if (TotalGameTime >= YoungDuration)
                RemoveYoungStatus(1019038); // You are old enough to be considered an adult, and have outgrown your status as a young player!
        }

        private class YoungTimer : Timer
        {
            private Account m_Account;

            public YoungTimer(Account account)
                : base(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0))
            {
                m_Account = account;

                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Account.CheckYoung();
            }
        }

        public Account(string username, string password)
        {
            m_Username = username;

            SetPassword(password);

            m_AccessLevel = AccessLevel.Player;

            m_Created = m_LastLogin = DateTime.UtcNow;
            m_TotalGameTime = TimeSpan.Zero;

            m_Mobiles = new Mobile[7];

            m_IPRestrictions = new string[0];
            m_LoginIPs = new IPAddress[0];

            Accounts.Add(this);
        }

        public Account(XmlElement node)
        {
            m_Username = Utility.GetText(node["username"], "empty");

            string plainPassword = Utility.GetText(node["password"], null);
            string cryptPassword = Utility.GetText(node["cryptPassword"], null);
            string newCryptPassword = Utility.GetText(node["newCryptPassword"], null);

            switch (AccountHandler.ProtectPasswords)
            {
                case PasswordProtection.None:
                    {
                        if (plainPassword != null)
                            SetPassword(plainPassword);
                        else if (newCryptPassword != null)
                            m_NewCryptPassword = newCryptPassword;
                        else if (cryptPassword != null)
                            m_CryptPassword = cryptPassword;
                        else
                            SetPassword("empty");

                        break;
                    }
                case PasswordProtection.Crypt:
                    {
                        if (cryptPassword != null)
                            m_CryptPassword = cryptPassword;
                        else if (plainPassword != null)
                            SetPassword(plainPassword);
                        else if (newCryptPassword != null)
                            m_NewCryptPassword = newCryptPassword;
                        else
                            SetPassword("empty");

                        break;
                    }
                default: // PasswordProtection.NewCrypt
                    {
                        if (newCryptPassword != null)
                            m_NewCryptPassword = newCryptPassword;
                        else if (plainPassword != null)
                            SetPassword(plainPassword);
                        else if (cryptPassword != null)
                            m_CryptPassword = cryptPassword;
                        else
                            SetPassword("empty");

                        break;
                    }
            }

            Enum.TryParse(Utility.GetText(node["accessLevel"], "Player"), true, out m_AccessLevel);
            m_Flags = Utility.GetXMLInt32(Utility.GetText(node["flags"], "0"), 0);
            m_Created = Utility.GetXMLDateTime(Utility.GetText(node["created"], null), DateTime.UtcNow);
            m_LastLogin = Utility.GetXMLDateTime(Utility.GetText(node["lastLogin"], null), DateTime.UtcNow);

            TotalCurrency = Utility.GetXMLDouble(Utility.GetText(node["totalCurrency"], "0"), 0);

            m_Mobiles = LoadMobiles(node);
            m_Comments = LoadComments(node);
            m_Tags = LoadTags(node);
            m_LoginIPs = LoadAddressList(node);
            m_IPRestrictions = LoadAccessCheck(node);

            for (int i = 0; i < m_Mobiles.Length; ++i)
            {
                if (m_Mobiles[i] != null)
                    m_Mobiles[i].Account = this;
            }

            TimeSpan totalGameTime = Utility.GetXMLTimeSpan(Utility.GetText(node["totalGameTime"], null), TimeSpan.Zero);
            if (totalGameTime == TimeSpan.Zero)
            {
                for (int i = 0; i < m_Mobiles.Length; i++)
                {
                    PlayerMobile m = m_Mobiles[i] as PlayerMobile;

                    if (m != null)
                        totalGameTime += m.GameTime;
                }
            }
            m_TotalGameTime = totalGameTime;

            if (this.Young)
                CheckYoung();

            Accounts.Add(this);
        }

        /// <summary>
        /// Deserializes a list of string values from an xml element. Null values are not added to the list.
        /// </summary>
        /// <param name="node">The XmlElement from which to deserialize.</param>
        /// <returns>String list. Value will never be null.</returns>
        public static string[] LoadAccessCheck(XmlElement node)
        {
            string[] stringList;
            XmlElement accessCheck = node["accessCheck"];

            if (accessCheck != null)
            {
                List<string> list = new List<string>();

                foreach (XmlElement ip in accessCheck.GetElementsByTagName("ip"))
                {
                    string text = Utility.GetText(ip, null);

                    if (text != null)
                        list.Add(text);
                }

                stringList = list.ToArray();
            }
            else
            {
                stringList = new string[0];
            }

            return stringList;
        }

        /// <summary>
        /// Deserializes a list of IPAddress values from an xml element.
        /// </summary>
        /// <param name="node">The XmlElement from which to deserialize.</param>
        /// <returns>Address list. Value will never be null.</returns>
        public static IPAddress[] LoadAddressList(XmlElement node)
        {
            IPAddress[] list;
            XmlElement addressList = node["addressList"];

            if (addressList != null)
            {
                int count = Utility.GetXMLInt32(Utility.GetAttribute(addressList, "count", "0"), 0);

                list = new IPAddress[count];

                count = 0;

                foreach (XmlElement ip in addressList.GetElementsByTagName("ip"))
                {
                    if (count < list.Length)
                    {
                        IPAddress address;

                        if (IPAddress.TryParse(Utility.GetText(ip, null), out address))
                        {
                            list[count] = Utility.Intern(address);
                            count++;
                        }
                    }
                }

                if (count != list.Length)
                {
                    IPAddress[] old = list;
                    list = new IPAddress[count];

                    for (int i = 0; i < count && i < old.Length; ++i)
                        list[i] = old[i];
                }
            }
            else
            {
                list = new IPAddress[0];
            }

            return list;
        }

        /// <summary>
        /// Deserializes a list of Mobile instances from an xml element.
        /// </summary>
        /// <param name="node">The XmlElement instance from which to deserialize.</param>
        /// <returns>Mobile list. Value will never be null.</returns>
        public static Mobile[] LoadMobiles(XmlElement node)
        {
            Mobile[] list = new Mobile[7];
            XmlElement chars = node["chars"];

            //int length = Accounts.GetInt32( Accounts.GetAttribute( chars, "length", "6" ), 6 );
            //list = new Mobile[length];
            //Above is legacy, no longer used

            if (chars != null)
            {
                foreach (XmlElement ele in chars.GetElementsByTagName("char"))
                {
                    try
                    {
                        int index = Utility.GetXMLInt32(Utility.GetAttribute(ele, "index", "0"), 0);
                        int serial = Utility.GetXMLInt32(Utility.GetText(ele, "0"), 0);

                        if (index >= 0 && index < list.Length)
                            list[index] = World.FindMobile(serial);
                    }
                    catch
                    {
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Deserializes a list of AccountComment instances from an xml element.
        /// </summary>
        /// <param name="node">The XmlElement from which to deserialize.</param>
        /// <returns>Comment list. Value will never be null.</returns>
        public static List<AccountComment> LoadComments(XmlElement node)
        {
            List<AccountComment> list = null;
            XmlElement comments = node["comments"];

            if (comments != null)
            {
                list = new List<AccountComment>();

                foreach (XmlElement comment in comments.GetElementsByTagName("comment"))
                {
                    try { list.Add(new AccountComment(comment)); }
                    catch { }
                }
            }

            return list;
        }

        /// <summary>
        /// Deserializes a list of AccountTag instances from an xml element.
        /// </summary>
        /// <param name="node">The XmlElement from which to deserialize.</param>
        /// <returns>Tag list. Value will never be null.</returns>
        public static List<AccountTag> LoadTags(XmlElement node)
        {
            List<AccountTag> list = null;
            XmlElement tags = node["tags"];

            if (tags != null)
            {
                list = new List<AccountTag>();

                foreach (XmlElement tag in tags.GetElementsByTagName("tag"))
                {
                    try { list.Add(new AccountTag(tag)); }
                    catch { }
                }
            }

            return list;
        }

        /// <summary>
        /// Checks if a specific NetState is allowed access to this account.
        /// </summary>
        /// <param name="ns">NetState instance to check.</param>
        /// <returns>True if allowed, false if not.</returns>
        public bool HasAccess(NetState ns)
        {
            return (ns != null && HasAccess(ns.Address));
        }

        public bool HasAccess(IPAddress ipAddress)
        {
            AccessLevel level = Misc.AccountHandler.LockdownLevel;

            if (level > AccessLevel.Player)
            {
                bool hasAccess = false;

                if (m_AccessLevel >= level)
                {
                    hasAccess = true;
                }
                else
                {
                    for (int i = 0; !hasAccess && i < this.Length; ++i)
                    {
                        Mobile m = this[i];

                        if (m != null && m.AccessLevel >= level)
                            hasAccess = true;
                    }
                }

                if (!hasAccess)
                    return false;
            }

            bool accessAllowed = (m_IPRestrictions.Length == 0 || IPLimiter.IsExempt(ipAddress));

            for (int i = 0; !accessAllowed && i < m_IPRestrictions.Length; ++i)
                accessAllowed = Utility.IPMatch(m_IPRestrictions[i], ipAddress);

            return accessAllowed;
        }

        /// <summary>
        /// Records the IP address of 'ns' in its 'LoginIPs' list.
        /// </summary>
        /// <param name="ns">NetState instance to record.</param>
        public void LogAccess(NetState ns)
        {
            if (ns != null)
            {
                LogAccess(ns.Address);
            }
        }

        public void LogAccess(IPAddress ipAddress)
        {
            if (IPLimiter.IsExempt(ipAddress))
                return;

            if (m_LoginIPs.Length == 0)
            {
                if (AccountHandler.IPTable.ContainsKey(ipAddress))
                    AccountHandler.IPTable[ipAddress]++;
                else
                    AccountHandler.IPTable[ipAddress] = 1;
            }

            bool contains = false;

            for (int i = 0; !contains && i < m_LoginIPs.Length; ++i)
                contains = m_LoginIPs[i].Equals(ipAddress);

            if (contains)
                return;

            IPAddress[] old = m_LoginIPs;
            m_LoginIPs = new IPAddress[old.Length + 1];

            for (int i = 0; i < old.Length; ++i)
                m_LoginIPs[i] = old[i];

            m_LoginIPs[old.Length] = ipAddress;
        }

        /// <summary>
        /// Checks if a specific NetState is allowed access to this account. If true, the NetState IPAddress is added to the address list.
        /// </summary>
        /// <param name="ns">NetState instance to check.</param>
        /// <returns>True if allowed, false if not.</returns>
        public bool CheckAccess(NetState ns)
        {
            return (ns != null && CheckAccess(ns.Address));
        }

        public bool CheckAccess(IPAddress ipAddress)
        {
            bool hasAccess = this.HasAccess(ipAddress);

            if (hasAccess)
            {
                LogAccess(ipAddress);
            }

            return hasAccess;
        }

        /// <summary>
        /// Serializes this Account instance to an XmlTextWriter.
        /// </summary>
        /// <param name="xml">The XmlTextWriter instance from which to serialize.</param>
        public void Save(XmlTextWriter xml)
        {
            xml.WriteStartElement("account");

            xml.WriteStartElement("username");
            xml.WriteString(m_Username);
            xml.WriteEndElement();

            if (m_PlainPassword != null)
            {
                xml.WriteStartElement("password");
                xml.WriteString(m_PlainPassword);
                xml.WriteEndElement();
            }

            if (m_CryptPassword != null)
            {
                xml.WriteStartElement("cryptPassword");
                xml.WriteString(m_CryptPassword);
                xml.WriteEndElement();
            }

            if (m_NewCryptPassword != null)
            {
                xml.WriteStartElement("newCryptPassword");
                xml.WriteString(m_NewCryptPassword);
                xml.WriteEndElement();
            }

            if (m_AccessLevel != AccessLevel.Player)
            {
                xml.WriteStartElement("accessLevel");
                xml.WriteString(m_AccessLevel.ToString());
                xml.WriteEndElement();
            }

            if (m_Flags != 0)
            {
                xml.WriteStartElement("flags");
                xml.WriteString(XmlConvert.ToString(m_Flags));
                xml.WriteEndElement();
            }

            xml.WriteStartElement("created");
            xml.WriteString(XmlConvert.ToString(m_Created, XmlDateTimeSerializationMode.Utc));
            xml.WriteEndElement();

            xml.WriteStartElement("lastLogin");
            xml.WriteString(XmlConvert.ToString(m_LastLogin, XmlDateTimeSerializationMode.Utc));
            xml.WriteEndElement();

            xml.WriteStartElement("totalGameTime");
            xml.WriteString(XmlConvert.ToString(TotalGameTime));
            xml.WriteEndElement();

            xml.WriteStartElement("chars");

            //xml.WriteAttributeString( "length", m_Mobiles.Length.ToString() );	//Legacy, Not used anymore

            for (int i = 0; i < m_Mobiles.Length; ++i)
            {
                Mobile m = m_Mobiles[i];

                if (m != null && !m.Deleted)
                {
                    xml.WriteStartElement("char");
                    xml.WriteAttributeString("index", i.ToString());
                    xml.WriteString(m.Serial.Value.ToString());
                    xml.WriteEndElement();
                }
            }

            xml.WriteEndElement();

            if (m_Comments != null && m_Comments.Count > 0)
            {
                xml.WriteStartElement("comments");

                for (int i = 0; i < m_Comments.Count; ++i)
                    m_Comments[i].Save(xml);

                xml.WriteEndElement();
            }

            if (m_Tags != null && m_Tags.Count > 0)
            {
                xml.WriteStartElement("tags");

                for (int i = 0; i < m_Tags.Count; ++i)
                    m_Tags[i].Save(xml);

                xml.WriteEndElement();
            }

            if (m_LoginIPs.Length > 0)
            {
                xml.WriteStartElement("addressList");

                xml.WriteAttributeString("count", m_LoginIPs.Length.ToString());

                for (int i = 0; i < m_LoginIPs.Length; ++i)
                {
                    xml.WriteStartElement("ip");
                    xml.WriteString(m_LoginIPs[i].ToString());
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }

            if (m_IPRestrictions.Length > 0)
            {
                xml.WriteStartElement("accessCheck");

                for (int i = 0; i < m_IPRestrictions.Length; ++i)
                {
                    xml.WriteStartElement("ip");
                    xml.WriteString(m_IPRestrictions[i]);
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }

            xml.WriteStartElement("totalCurrency");
            xml.WriteString(XmlConvert.ToString(TotalCurrency));
            xml.WriteEndElement();

            xml.WriteEndElement();
        }

        /// <summary>
        /// Gets the current number of characters on this account.
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;

                for (int i = 0; i < this.Length; ++i)
                {
                    if (this[i] != null)
                        ++count;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the maximum amount of characters allowed to be created on this account. Values other than 1, 5, 6, or 7 are not supported by the client.
        /// </summary>
        public int Limit
        {
            get { return (Core.SA ? 7 : Core.AOS ? 6 : 5); }
        }

        /// <summary>
        /// Gets the maxmimum amount of characters that this account can hold.
        /// </summary>
        public int Length
        {
            get { return m_Mobiles.Length; }
        }

        /// <summary>
        /// Gets or sets the character at a specified index for this account. Out of bound index values are handled; null returned for get, ignored for set.
        /// </summary>
        public Mobile this[int index]
        {
            get
            {
                if (index >= 0 && index < m_Mobiles.Length)
                {
                    Mobile m = m_Mobiles[index];

                    if (m != null && m.Deleted)
                    {
                        m.Account = null;
                        m_Mobiles[index] = m = null;
                    }

                    return m;
                }

                return null;
            }
            set
            {
                if (index >= 0 && index < m_Mobiles.Length)
                {
                    if (m_Mobiles[index] != null)
                        m_Mobiles[index].Account = null;

                    m_Mobiles[index] = value;

                    if (m_Mobiles[index] != null)
                        m_Mobiles[index].Account = this;
                }
            }
        }

        public override string ToString()
        {
            return m_Username;
        }

        public int CompareTo(Account other)
        {
            if (other == null)
                return 1;

            return m_Username.CompareTo(other.m_Username);
        }

        public int CompareTo(IAccount other)
        {
            if (other == null)
                return 1;

            return m_Username.CompareTo(other.Username);
        }

        public int CompareTo(object obj)
        {
            if (obj is Account)
                return this.CompareTo((Account)obj);

            throw new ArgumentException();
        }

        #region Gold Account
        /// <summary>
        ///     This amount specifies the value at which point Gold turns to Platinum.
        ///     By default, when 1,000,000,000 Gold is accumulated, it will transform
        ///     into 1 Platinum.
        /// </summary>
        public static int CurrencyThreshold
        {
            get { return AccountGold.CurrencyThreshold; }
            set { AccountGold.CurrencyThreshold = value; }
        }

        /// <summary>
        ///     This amount represents the total amount of currency owned by the player.
        ///     It is cumulative of both Gold and Platinum, the absolute total amount of
        ///     Gold owned by the player can be found by multiplying this value by the
        ///     CurrencyThreshold value.
        /// </summary>
        [CommandProperty(AccessLevel.Administrator, true)]
        public double TotalCurrency { get; private set; }

        /// <summary>
        ///     This amount represents the current amount of Gold owned by the player.
        ///     The value does not include the value of Platinum and ranges from
        ///     0 to 999,999,999 by default.
        /// </summary>
        [CommandProperty(AccessLevel.Administrator)]
        public int TotalGold
        {
            get { return (int)Math.Floor((TotalCurrency - Math.Truncate(TotalCurrency)) * Math.Max(1.0, CurrencyThreshold)); }
        }

        /// <summary>
        ///     This amount represents the current amount of Platinum owned by the player.
        ///     The value does not include the value of Gold and ranges from
        ///     0 to 2,147,483,647 by default.
        ///     One Platinum represents the value of CurrencyThreshold in Gold.
        /// </summary>
        [CommandProperty(AccessLevel.Administrator)]
        public int TotalPlat { get { return (int)Math.Truncate(TotalCurrency); } }

        /// <summary>
        ///     Attempts to deposit the given amount of Gold and Platinum into this account.
        /// </summary>
        /// <param name="amount">Amount to deposit.</param>
        /// <returns>True if successful, false if amount given is less than or equal to zero.</returns>
        public bool DepositCurrency(double amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            TotalCurrency += amount;
            return true;
        }

        /// <summary>
        ///     Attempts to deposit the given amount of Gold into this account.
        ///     If the given amount is greater than the CurrencyThreshold,
        ///     Platinum will be deposited to offset the difference.
        /// </summary>
        /// <param name="amount">Amount to deposit.</param>
        /// <returns>True if successful, false if amount given is less than or equal to zero.</returns>
        public bool DepositGold(int amount)
        {
            return DepositCurrency(amount / Math.Max(1.0, CurrencyThreshold));
        }

        /// <summary>
        ///     Attempts to deposit the given amount of Gold into this account.
        ///     If the given amount is greater than the CurrencyThreshold,
        ///     Platinum will be deposited to offset the difference.
        /// </summary>
        /// <param name="amount">Amount to deposit.</param>
        /// <returns>True if successful, false if amount given is less than or equal to zero.</returns>
        public bool DepositGold(long amount)
        {
            return DepositCurrency(amount / Math.Max(1.0, CurrencyThreshold));
        }

        /// <summary>
        ///     Attempts to deposit the given amount of Platinum into this account.
        /// </summary>
        /// <param name="amount">Amount to deposit.</param>
        /// <returns>True if successful, false if amount given is less than or equal to zero.</returns>
        public bool DepositPlat(int amount)
        {
            return DepositCurrency(amount);
        }

        /// <summary>
        ///     Attempts to deposit the given amount of Platinum into this account.
        /// </summary>
        /// <param name="amount">Amount to deposit.</param>
        /// <returns>True if successful, false if amount given is less than or equal to zero.</returns>
        public bool DepositPlat(long amount)
        {
            return DepositCurrency(amount);
        }

        /// <summary>
        ///     Attempts to withdraw the given amount of Platinum and Gold from this account.
        /// </summary>
        /// <param name="amount">Amount to withdraw.</param>
        /// <returns>True if successful, false if balance was too low.</returns>
        public bool WithdrawCurrency(double amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (amount > TotalCurrency)
            {
                return false;
            }

            TotalCurrency -= amount;
            return true;
        }

        /// <summary>
        ///     Attempts to withdraw the given amount of Gold from this account.
        ///     If the given amount is greater than the CurrencyThreshold,
        ///     Platinum will be withdrawn to offset the difference.
        /// </summary>
        /// <param name="amount">Amount to withdraw.</param>
        /// <returns>True if successful, false if balance was too low.</returns>
        public bool WithdrawGold(int amount)
        {
            return WithdrawCurrency(amount / Math.Max(1.0, CurrencyThreshold));
        }

        /// <summary>
        ///     Attempts to withdraw the given amount of Gold from this account.
        ///     If the given amount is greater than the CurrencyThreshold,
        ///     Platinum will be withdrawn to offset the difference.
        /// </summary>
        /// <param name="amount">Amount to withdraw.</param>
        /// <returns>True if successful, false if balance was too low.</returns>
        public bool WithdrawGold(long amount)
        {
            return WithdrawCurrency(amount / Math.Max(1.0, CurrencyThreshold));
        }

        /// <summary>
        ///     Attempts to withdraw the given amount of Platinum from this account.
        /// </summary>
        /// <param name="amount">Amount to withdraw.</param>
        /// <returns>True if successful, false if balance was too low.</returns>
        public bool WithdrawPlat(int amount)
        {
            return WithdrawCurrency(amount);
        }

        /// <summary>
        ///     Attempts to withdraw the given amount of Platinum from this account.
        /// </summary>
        /// <param name="amount">Amount to withdraw.</param>
        /// <returns>True if successful, false if balance was too low.</returns>
        public bool WithdrawPlat(long amount)
        {
            return WithdrawCurrency(amount);
        }

        /// <summary>
        ///     Gets the total balance of Gold for this account.
        /// </summary>
        /// <param name="gold">Gold value, Platinum exclusive</param>
        /// <param name="totalGold">Gold value, Platinum inclusive</param>
        public void GetGoldBalance(out int gold, out double totalGold)
        {
            gold = TotalGold;
            totalGold = TotalCurrency * Math.Max(1.0, CurrencyThreshold);
        }

        /// <summary>
        ///     Gets the total balance of Gold for this account.
        /// </summary>
        /// <param name="gold">Gold value, Platinum exclusive</param>
        /// <param name="totalGold">Gold value, Platinum inclusive</param>
        public void GetGoldBalance(out long gold, out double totalGold)
        {
            gold = TotalGold;
            totalGold = TotalCurrency * Math.Max(1.0, CurrencyThreshold);
        }

        /// <summary>
        ///     Gets the total balance of Platinum for this account.
        /// </summary>
        /// <param name="plat">Platinum value, Gold exclusive</param>
        /// <param name="totalPlat">Platinum value, Gold inclusive</param>
        public void GetPlatBalance(out int plat, out double totalPlat)
        {
            plat = TotalPlat;
            totalPlat = TotalCurrency;
        }

        /// <summary>
        ///     Gets the total balance of Platinum for this account.
        /// </summary>
        /// <param name="plat">Platinum value, Gold exclusive</param>
        /// <param name="totalPlat">Platinum value, Gold inclusive</param>
        public void GetPlatBalance(out long plat, out double totalPlat)
        {
            plat = TotalPlat;
            totalPlat = TotalCurrency;
        }

        /// <summary>
        ///     Gets the total balance of Gold and Platinum for this account.
        /// </summary>
        /// <param name="gold">Gold value, Platinum exclusive</param>
        /// <param name="totalGold">Gold value, Platinum inclusive</param>
        /// <param name="plat">Platinum value, Gold exclusive</param>
        /// <param name="totalPlat">Platinum value, Gold inclusive</param>
        public void GetBalance(out int gold, out double totalGold, out int plat, out double totalPlat)
        {
            GetGoldBalance(out gold, out totalGold);
            GetPlatBalance(out plat, out totalPlat);
        }

        /// <summary>
        ///     Gets the total balance of Gold and Platinum for this account.
        /// </summary>
        /// <param name="gold">Gold value, Platinum exclusive</param>
        /// <param name="totalGold">Gold value, Platinum inclusive</param>
        /// <param name="plat">Platinum value, Gold exclusive</param>
        /// <param name="totalPlat">Platinum value, Gold inclusive</param>
        public void GetBalance(out long gold, out double totalGold, out long plat, out double totalPlat)
        {
            GetGoldBalance(out gold, out totalGold);
            GetPlatBalance(out plat, out totalPlat);
        }
        #endregion
    }

    public class AccountComment
    {
        private string m_AddedBy;
        private string m_Content;
        private DateTime m_LastModified;

        /// <summary>
        /// A string representing who added this comment.
        /// </summary>
        public string AddedBy
        {
            get { return m_AddedBy; }
        }

        /// <summary>
        /// Gets or sets the body of this comment. Setting this value will reset LastModified.
        /// </summary>
        public string Content
        {
            get { return m_Content; }
            set { m_Content = value; m_LastModified = DateTime.UtcNow; }
        }

        /// <summary>
        /// The date and time when this account was last modified -or- the comment creation time, if never modified.
        /// </summary>
        public DateTime LastModified
        {
            get { return m_LastModified; }
        }

        /// <summary>
        /// Constructs a new AccountComment instance.
        /// </summary>
        /// <param name="addedBy">Initial AddedBy value.</param>
        /// <param name="content">Initial Content value.</param>
        public AccountComment(string addedBy, string content)
        {
            m_AddedBy = addedBy;
            m_Content = content;
            m_LastModified = DateTime.UtcNow;
        }

        /// <summary>
        /// Deserializes an AccountComment instance from an xml element.
        /// </summary>
        /// <param name="node">The XmlElement instance from which to deserialize.</param>
        public AccountComment(XmlElement node)
        {
            m_AddedBy = Utility.GetAttribute(node, "addedBy", "empty");
            m_LastModified = Utility.GetXMLDateTime(Utility.GetAttribute(node, "lastModified"), DateTime.UtcNow);
            m_Content = Utility.GetText(node, "");
        }

        /// <summary>
        /// Serializes this AccountComment instance to an XmlTextWriter.
        /// </summary>
        /// <param name="xml">The XmlTextWriter instance from which to serialize.</param>
        public void Save(XmlTextWriter xml)
        {
            xml.WriteStartElement("comment");

            xml.WriteAttributeString("addedBy", m_AddedBy);

            xml.WriteAttributeString("lastModified", XmlConvert.ToString(m_LastModified, XmlDateTimeSerializationMode.Utc));

            xml.WriteString(m_Content);

            xml.WriteEndElement();
        }
    }

    public class AccountTag
    {
        private string m_Name, m_Value;

        /// <summary>
        /// Gets or sets the name of this tag.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// Gets or sets the value of this tag.
        /// </summary>
        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        /// <summary>
        /// Constructs a new AccountTag instance with a specific name and value.
        /// </summary>
        /// <param name="name">Initial name.</param>
        /// <param name="value">Initial value.</param>
        public AccountTag(string name, string value)
        {
            m_Name = name;
            m_Value = value;
        }

        /// <summary>
        /// Deserializes an AccountTag instance from an xml element.
        /// </summary>
        /// <param name="node">The XmlElement instance from which to deserialize.</param>
        public AccountTag(XmlElement node)
        {
            m_Name = Utility.GetAttribute(node, "name", "empty");
            m_Value = Utility.GetText(node, "");
        }

        /// <summary>
        /// Serializes this AccountTag instance to an XmlTextWriter.
        /// </summary>
        /// <param name="xml">The XmlTextWriter instance from which to serialize.</param>
        public void Save(XmlTextWriter xml)
        {
            xml.WriteStartElement("tag");
            xml.WriteAttributeString("name", m_Name);
            xml.WriteString(m_Value);
            xml.WriteEndElement();
        }
    }
}

namespace Server.Misc
{
    public class AccountPrompt
    {
        public static void Initialize()
        {
            if (Accounts.Count == 0 && !Core.Service)
            {
                Console.WriteLine("This server has no accounts.");
                Console.Write("Do you want to create the owner account now? (y/n)");

                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    Console.WriteLine();

                    Console.Write("Username: ");
                    string username = Console.ReadLine();

                    Console.Write("Password: ");
                    string password = Console.ReadLine();

                    Account a = new Account(username, password);
                    a.AccessLevel = AccessLevel.Owner;

                    Console.WriteLine("Account created.");
                }
                else
                {
                    Console.WriteLine();

                    Console.WriteLine("Account not created.");
                }
            }
        }
    }

    public enum PasswordProtection
    {
        None,
        Crypt,
        NewCrypt
    }

    public class AccountHandler
    {
        private static int MaxAccountsPerIP = 1;
        private static bool AutoAccountCreation = true;
        private static bool RestrictDeletion = !TestCenter.Enabled;
        private static TimeSpan DeleteDelay = TimeSpan.FromDays(7.0);

        public static PasswordProtection ProtectPasswords = PasswordProtection.NewCrypt;

        private static AccessLevel m_LockdownLevel;

        public static AccessLevel LockdownLevel
        {
            get { return m_LockdownLevel; }
            set { m_LockdownLevel = value; }
        }

        private static CityInfo[] StartingCities = new CityInfo[]
			{
				new CityInfo( "New Haven",	"New Haven Bank",	1150168, 3667,	2625,	0  ),
				new CityInfo( "Yew",		"The Empath Abbey",	1075072, 633,	858,	0  ),
				new CityInfo( "Minoc",		"The Barnacle",		1075073, 2476,	413,	15 ),
				new CityInfo( "Britain",	"The Wayfarer's Inn",	1075074, 1602,	1591,	20 ),
				new CityInfo( "Moonglow",	"The Scholars Inn",	1075075, 4408,	1168,	0  ),
				new CityInfo( "Trinsic",	"The Traveler's Inn",	1075076, 1845,	2745,	0  ),
				new CityInfo( "Jhelom",		"The Mercenary Inn",	1075078, 1374,	3826,	0  ),
				new CityInfo( "Skara Brae",	"The Falconer's Inn",	1075079, 618,	2234,	0  ),
				new CityInfo( "Vesper",		"The Ironwood Inn",	1075080, 2771,	976,	0  )
			};

        /* Old Haven/Magincia Locations
            new CityInfo( "Britain", "Sweet Dreams Inn", 1496, 1628, 10 );
            // ..
            // Trinsic
            new CityInfo( "Magincia", "The Great Horns Tavern", 3734, 2222, 20 ),
            // Jhelom
            // ..
            new CityInfo( "Haven", "Buckler's Hideaway", 3667, 2625, 0 )

            if ( Core.AOS )
            {
                //CityInfo haven = new CityInfo( "Haven", "Uzeraan's Mansion", 3618, 2591, 0 );
                CityInfo haven = new CityInfo( "Haven", "Uzeraan's Mansion", 3503, 2574, 14 );
                StartingCities[StartingCities.Length - 1] = haven;
            }
        */

        private static bool PasswordCommandEnabled = false;

        public static void Initialize()
        {
            EventSink.DeleteRequest += new DeleteRequestEventHandler(EventSink_DeleteRequest);
            EventSink.AccountLogin += new AccountLoginEventHandler(EventSink_AccountLogin);
            EventSink.GameLogin += new GameLoginEventHandler(EventSink_GameLogin);

            if (PasswordCommandEnabled)
                CommandSystem.Register("Password", AccessLevel.Player, new CommandEventHandler(Password_OnCommand));
        }

        [Usage("Password <newPassword> <repeatPassword>")]
        [Description("Changes the password of the commanding players account. Requires the same C-class IP address as the account's creator.")]
        public static void Password_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            Account acct = from.Account as Account;

            if (acct == null)
                return;

            IPAddress[] accessList = acct.LoginIPs;

            if (accessList.Length == 0)
                return;

            NetState ns = from.NetState;

            if (ns == null)
                return;

            if (e.Length == 0)
            {
                from.SendMessage("You must specify the new password.");
                return;
            }
            else if (e.Length == 1)
            {
                from.SendMessage("To prevent potential typing mistakes, you must type the password twice. Use the format:");
                from.SendMessage("Password \"(newPassword)\" \"(repeated)\"");
                return;
            }

            string pass = e.GetString(0);
            string pass2 = e.GetString(1);

            if (pass != pass2)
            {
                from.SendMessage("The passwords do not match.");
                return;
            }

            bool isSafe = true;

            for (int i = 0; isSafe && i < pass.Length; ++i)
                isSafe = (pass[i] >= 0x20 && pass[i] < 0x7F);

            if (!isSafe)
            {
                from.SendMessage("That is not a valid password.");
                return;
            }

            try
            {
                IPAddress ipAddress = ns.Address;

                if (Utility.IPMatchClassC(accessList[0], ipAddress))
                {
                    acct.SetPassword(pass);
                    from.SendMessage("The password to your account has changed.");
                }
                else
                {
                    PageEntry entry = PageQueue.GetEntry(from);

                    if (entry != null)
                    {
                        if (entry.Message.StartsWith("[Automated: Change Password]"))
                            from.SendMessage("You already have a password change request in the help system queue.");
                        else
                            from.SendMessage("Your IP address does not match that which created this account.");
                    }
                    else if (PageQueue.CheckAllowedToPage(from))
                    {
                        from.SendMessage("Your IP address does not match that which created this account.  A page has been entered into the help system on your behalf.");

                        from.SendLocalizedMessage(501234, "", 0x35); /* The next available Counselor/Game Master will respond as soon as possible.
																	    * Please check your Journal for messages every few minutes.
																	    */

                        PageQueue.Enqueue(new PageEntry(from, String.Format("[Automated: Change Password]<br>Desired password: {0}<br>Current IP address: {1}<br>Account IP address: {2}", pass, ipAddress, accessList[0]), PageType.Account));
                    }

                }
            }
            catch
            {
            }
        }

        private static void EventSink_DeleteRequest(DeleteRequestEventArgs e)
        {
            NetState state = e.State;
            int index = e.Index;

            Account acct = state.Account as Account;

            if (acct == null)
            {
                state.Dispose();
            }
            else if (index < 0 || index >= acct.Length)
            {
                state.Send(new DeleteResult(DeleteResultType.BadRequest));
                state.Send(new CharacterListUpdate(acct));
            }
            else
            {
                Mobile m = acct[index];

                if (m == null)
                {
                    state.Send(new DeleteResult(DeleteResultType.CharNotExist));
                    state.Send(new CharacterListUpdate(acct));
                }
                else if (m.NetState != null)
                {
                    state.Send(new DeleteResult(DeleteResultType.CharBeingPlayed));
                    state.Send(new CharacterListUpdate(acct));
                }
                else if (RestrictDeletion && DateTime.UtcNow < (m.CreationTime + DeleteDelay))
                {
                    state.Send(new DeleteResult(DeleteResultType.CharTooYoung));
                    state.Send(new CharacterListUpdate(acct));
                }
                else if (m.AccessLevel == AccessLevel.Player && Region.Find(m.LogoutLocation, m.LogoutMap).GetRegion(typeof(Jail)) != null)	//Don't need to check current location, if netstate is null, they're logged out
                {
                    state.Send(new DeleteResult(DeleteResultType.BadRequest));
                    state.Send(new CharacterListUpdate(acct));
                }
                else
                {
                    Console.WriteLine("Client: {0}: Deleting character {1} (0x{2:X})", state, index, m.Serial.Value);

                    acct.Comments.Add(new AccountComment("System", String.Format("Character #{0} {1} deleted by {2}", index + 1, m, state)));

                    m.Delete();
                    state.Send(new CharacterListUpdate(acct));
                }
            }
        }

        public static bool CanCreate(IPAddress ip)
        {
            if (!IPTable.ContainsKey(ip))
                return true;

            return (IPTable[ip] < MaxAccountsPerIP);
        }

        private static Dictionary<IPAddress, Int32> m_IPTable;

        public static Dictionary<IPAddress, Int32> IPTable
        {
            get
            {
                if (m_IPTable == null)
                {
                    m_IPTable = new Dictionary<IPAddress, Int32>();

                    foreach (Account a in Accounts.GetAccounts())
                        if (a.LoginIPs.Length > 0)
                        {
                            IPAddress ip = a.LoginIPs[0];

                            if (m_IPTable.ContainsKey(ip))
                                m_IPTable[ip]++;
                            else
                                m_IPTable[ip] = 1;
                        }
                }

                return m_IPTable;
            }
        }

        private static readonly char[] m_ForbiddenChars = new char[]
		{
			'<', '>', ':', '"', '/', '\\', '|', '?', '*'
		};

        private static bool IsForbiddenChar(char c)
        {
            for (int i = 0; i < m_ForbiddenChars.Length; ++i)
                if (c == m_ForbiddenChars[i])
                    return true;

            return false;
        }

        private static Account CreateAccount(NetState state, string un, string pw)
        {
            if (un.Length == 0 || pw.Length == 0)
                return null;

            bool isSafe = !(un.StartsWith(" ") || un.EndsWith(" ") || un.EndsWith("."));

            for (int i = 0; isSafe && i < un.Length; ++i)
                isSafe = (un[i] >= 0x20 && un[i] < 0x7F && !IsForbiddenChar(un[i]));

            for (int i = 0; isSafe && i < pw.Length; ++i)
                isSafe = (pw[i] >= 0x20 && pw[i] < 0x7F);

            if (!isSafe)
                return null;

            if (!CanCreate(state.Address))
            {
                Console.WriteLine("Login: {0}: Account '{1}' not created, ip already has {2} account{3}.", state, un, MaxAccountsPerIP, MaxAccountsPerIP == 1 ? "" : "s");
                return null;
            }

            Console.WriteLine("Login: {0}: Creating new account '{1}'", state, un);

            Account a = new Account(un, pw);

            return a;
        }

        public static void EventSink_AccountLogin(AccountLoginEventArgs e)
        {
            if (!IPLimiter.SocketBlock && !IPLimiter.Verify(e.State.Address))
            {
                e.Accepted = false;
                e.RejectReason = ALRReason.InUse;

                Console.WriteLine("Login: {0}: Past IP limit threshold", e.State);

                using (StreamWriter op = new StreamWriter("ipLimits.log", true))
                    op.WriteLine("{0}\tPast IP limit threshold\t{1}", e.State, DateTime.UtcNow);

                return;
            }

            string un = e.Username;
            string pw = e.Password;

            e.Accepted = false;
            Account acct = Accounts.GetAccount(un) as Account;

            if (acct == null)
            {
                if (AutoAccountCreation && un.Trim().Length > 0) // To prevent someone from making an account of just '' or a bunch of meaningless spaces
                {
                    e.State.Account = acct = CreateAccount(e.State, un, pw);
                    e.Accepted = acct == null ? false : acct.CheckAccess(e.State);

                    if (!e.Accepted)
                        e.RejectReason = ALRReason.BadComm;
                }
                else
                {
                    Console.WriteLine("Login: {0}: Invalid username '{1}'", e.State, un);
                    e.RejectReason = ALRReason.Invalid;
                }
            }
            else if (!acct.HasAccess(e.State))
            {
                Console.WriteLine("Login: {0}: Access denied for '{1}'", e.State, un);
                e.RejectReason = (m_LockdownLevel > AccessLevel.Player ? ALRReason.BadComm : ALRReason.BadPass);
            }
            else if (!acct.CheckPassword(pw))
            {
                Console.WriteLine("Login: {0}: Invalid password for '{1}'", e.State, un);
                e.RejectReason = ALRReason.BadPass;
            }
            else if (acct.Banned)
            {
                Console.WriteLine("Login: {0}: Banned account '{1}'", e.State, un);
                e.RejectReason = ALRReason.Blocked;
            }
            else
            {
                Console.WriteLine("Login: {0}: Valid credentials for '{1}'", e.State, un);
                e.State.Account = acct;
                e.Accepted = true;

                acct.LogAccess(e.State);
            }

            if (!e.Accepted)
                AccountAttackLimiter.RegisterInvalidAccess(e.State);
        }

        public static void EventSink_GameLogin(GameLoginEventArgs e)
        {
            if (!IPLimiter.SocketBlock && !IPLimiter.Verify(e.State.Address))
            {
                e.Accepted = false;

                Console.WriteLine("Login: {0}: Past IP limit threshold", e.State);

                using (StreamWriter op = new StreamWriter("ipLimits.log", true))
                    op.WriteLine("{0}\tPast IP limit threshold\t{1}", e.State, DateTime.UtcNow);

                return;
            }

            string un = e.Username;
            string pw = e.Password;

            Account acct = Accounts.GetAccount(un) as Account;

            if (acct == null)
            {
                e.Accepted = false;
            }
            else if (!acct.HasAccess(e.State))
            {
                Console.WriteLine("Login: {0}: Access denied for '{1}'", e.State, un);
                e.Accepted = false;
            }
            else if (!acct.CheckPassword(pw))
            {
                Console.WriteLine("Login: {0}: Invalid password for '{1}'", e.State, un);
                e.Accepted = false;
            }
            else if (acct.Banned)
            {
                Console.WriteLine("Login: {0}: Banned account '{1}'", e.State, un);
                e.Accepted = false;
            }
            else
            {
                acct.LogAccess(e.State);

                Console.WriteLine("Login: {0}: Account '{1}' at character list", e.State, un);
                e.State.Account = acct;
                e.Accepted = true;
                e.CityInfo = StartingCities;
            }

            if (!e.Accepted)
                AccountAttackLimiter.RegisterInvalidAccess(e.State);
        }

        public static bool CheckAccount(Mobile mobCheck, Mobile accCheck)
        {
            if (accCheck != null)
            {
                Account a = accCheck.Account as Account;

                if (a != null)
                {
                    for (int i = 0; i < a.Length; ++i)
                    {
                        if (a[i] == mobCheck)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}