using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using Server;
using Server.Misc;
using Server.Network;

namespace Server
{
    public class Firewall
    {
        #region Firewall Entries

        public interface IFirewallEntry
        {
            bool IsBlocked(IPAddress address);
        }

        public class IPFirewallEntry : IFirewallEntry
        {
            IPAddress m_Address;
            public IPFirewallEntry(IPAddress address)
            {
                m_Address = address;
            }

            public bool IsBlocked(IPAddress address)
            {
                return m_Address.Equals(address);
            }

            public override string ToString()
            {
                return m_Address.ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj is IPAddress)
                {
                    return obj.Equals(m_Address);
                }
                else if (obj is string)
                {
                    IPAddress otherAddress;

                    if (IPAddress.TryParse((string)obj, out otherAddress))
                        return otherAddress.Equals(m_Address);
                }
                else if (obj is IPFirewallEntry)
                {
                    return m_Address.Equals(((IPFirewallEntry)obj).m_Address);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return m_Address.GetHashCode();
            }
        }

        public class CIDRFirewallEntry : IFirewallEntry
        {
            IPAddress m_CIDRPrefix;
            int m_CIDRLength;

            public CIDRFirewallEntry(IPAddress cidrPrefix, int cidrLength)
            {
                m_CIDRPrefix = cidrPrefix;
                m_CIDRLength = cidrLength;
            }

            public bool IsBlocked(IPAddress address)
            {
                return Utility.IPMatchCIDR(m_CIDRPrefix, address, m_CIDRLength);
            }

            public override string ToString()
            {
                return String.Format("{0}/{1}", m_CIDRPrefix, m_CIDRLength);
            }

            public override bool Equals(object obj)
            {

                if (obj is string)
                {
                    string entry = (string)obj;

                    string[] str = entry.Split('/');

                    if (str.Length == 2)
                    {
                        IPAddress cidrPrefix;

                        if (IPAddress.TryParse(str[0], out cidrPrefix))
                        {
                            int cidrLength;

                            if (int.TryParse(str[1], out cidrLength))
                                return m_CIDRPrefix.Equals(cidrPrefix) && m_CIDRLength.Equals(cidrLength);
                        }
                    }
                }
                else if (obj is CIDRFirewallEntry)
                {
                    CIDRFirewallEntry entry = obj as CIDRFirewallEntry;

                    return m_CIDRPrefix.Equals(entry.m_CIDRPrefix) && m_CIDRLength.Equals(entry.m_CIDRLength);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return m_CIDRPrefix.GetHashCode() ^ m_CIDRLength.GetHashCode();
            }
        }

        public class WildcardIPFirewallEntry : IFirewallEntry
        {
            string m_Entry;

            bool m_Valid = true;

            public WildcardIPFirewallEntry(string entry)
            {
                m_Entry = entry;
            }

            public bool IsBlocked(IPAddress address)
            {
                if (!m_Valid)
                    return false;	//Why process if it's invalid?  it'll return false anyway after processing it.

                return Utility.IPMatch(m_Entry, address, ref m_Valid);
            }

            public override string ToString()
            {
                return m_Entry.ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj is string)
                    return obj.Equals(m_Entry);
                else if (obj is WildcardIPFirewallEntry)
                    return m_Entry.Equals(((WildcardIPFirewallEntry)obj).m_Entry);

                return false;
            }

            public override int GetHashCode()
            {
                return m_Entry.GetHashCode();
            }
        }

        #endregion

        private static List<IFirewallEntry> m_Blocked;

        static Firewall()
        {
            m_Blocked = new List<IFirewallEntry>();

            string path = "firewall.cfg";

            if (File.Exists(path))
            {
                using (StreamReader ip = new StreamReader(path))
                {
                    string line;

                    while ((line = ip.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line.Length == 0)
                            continue;

                        m_Blocked.Add(ToFirewallEntry(line));

                        /*
                        object toAdd;

                        IPAddress addr;
                        if( IPAddress.TryParse( line, out addr ) )
                            toAdd = addr;
                        else
                            toAdd = line;

                        m_Blocked.Add( toAdd.ToString() );
                         * */
                    }
                }
            }
        }

        public static List<IFirewallEntry> List
        {
            get
            {
                return m_Blocked;
            }
        }

        public static IFirewallEntry ToFirewallEntry(object entry)
        {
            if (entry is IFirewallEntry)
                return (IFirewallEntry)entry;
            else if (entry is IPAddress)
                return new IPFirewallEntry((IPAddress)entry);
            else if (entry is string)
                return ToFirewallEntry((string)entry);

            return null;
        }

        public static IFirewallEntry ToFirewallEntry(string entry)
        {
            IPAddress addr;

            if (IPAddress.TryParse(entry, out addr))
                return new IPFirewallEntry(addr);

            //Try CIDR parse
            string[] str = entry.Split('/');

            if (str.Length == 2)
            {
                IPAddress cidrPrefix;

                if (IPAddress.TryParse(str[0], out cidrPrefix))
                {
                    int cidrLength;

                    if (int.TryParse(str[1], out cidrLength))
                        return new CIDRFirewallEntry(cidrPrefix, cidrLength);
                }
            }

            return new WildcardIPFirewallEntry(entry);
        }

        public static void RemoveAt(int index)
        {
            m_Blocked.RemoveAt(index);
            Save();
        }

        public static void Remove(object obj)
        {
            IFirewallEntry entry = ToFirewallEntry(obj);

            if (entry != null)
            {
                m_Blocked.Remove(entry);
                Save();
            }
        }

        public static void Add(object obj)
        {
            if (obj is IPAddress)
                Add((IPAddress)obj);
            else if (obj is string)
                Add((string)obj);
            else if (obj is IFirewallEntry)
                Add((IFirewallEntry)obj);
        }

        public static void Add(IFirewallEntry entry)
        {
            if (!m_Blocked.Contains(entry))
                m_Blocked.Add(entry);

            Save();
        }

        public static void Add(string pattern)
        {
            IFirewallEntry entry = ToFirewallEntry(pattern);

            if (!m_Blocked.Contains(entry))
                m_Blocked.Add(entry);

            Save();
        }

        public static void Add(IPAddress ip)
        {
            IFirewallEntry entry = new IPFirewallEntry(ip);

            if (!m_Blocked.Contains(entry))
                m_Blocked.Add(entry);

            Save();
        }

        public static void Save()
        {
            string path = "firewall.cfg";

            using (StreamWriter op = new StreamWriter(path))
            {
                for (int i = 0; i < m_Blocked.Count; ++i)
                    op.WriteLine(m_Blocked[i]);
            }
        }

        public static bool IsBlocked(IPAddress ip)
        {
            for (int i = 0; i < m_Blocked.Count; i++)
            {
                if (m_Blocked[i].IsBlocked(ip))
                    return true;
            }

            return false;
            /*
            bool contains = false;

            for ( int i = 0; !contains && i < m_Blocked.Count; ++i )
            {
                if ( m_Blocked[i] is IPAddress )
                    contains = ip.Equals( m_Blocked[i] );
                else if ( m_Blocked[i] is String )
                {
                    string s = (string)m_Blocked[i];

                    contains = Utility.IPMatchCIDR( s, ip );

                    if( !contains )
                        contains = Utility.IPMatch( s, ip );
                }
            }

            return contains;
             * */
        }
    }

    public class AccessRestrictions
    {
        public static void Initialize()
        {
            EventSink.SocketConnect += new SocketConnectEventHandler(EventSink_SocketConnect);
        }

        private static void EventSink_SocketConnect(SocketConnectEventArgs e)
        {
            try
            {
                IPAddress ip = ((IPEndPoint)e.Socket.RemoteEndPoint).Address;

                if (Firewall.IsBlocked(ip))
                {
                    Console.WriteLine("Client: {0}: Firewall blocked connection attempt.", ip);
                    e.AllowConnection = false;
                    return;
                }
                else if (IPLimiter.SocketBlock && !IPLimiter.Verify(ip))
                {
                    Console.WriteLine("Client: {0}: Past IP limit threshold", ip);

                    using (StreamWriter op = new StreamWriter("ipLimits.log", true))
                        op.WriteLine("{0}\tPast IP limit threshold\t{1}", ip, DateTime.UtcNow);

                    e.AllowConnection = false;
                    return;
                }
            }
            catch
            {
                e.AllowConnection = false;
            }
        }
    }
}

namespace Server.Accounting
{
    public class AccountAttackLimiter
    {
        public static bool Enabled = true;

        public static void Initialize()
        {
            if (!Enabled)
                return;

            PacketHandlers.RegisterThrottler(0x80, new ThrottlePacketCallback(Throttle_Callback));
            PacketHandlers.RegisterThrottler(0x91, new ThrottlePacketCallback(Throttle_Callback));
            PacketHandlers.RegisterThrottler(0xCF, new ThrottlePacketCallback(Throttle_Callback));
        }

        public static bool Throttle_Callback(NetState ns)
        {
            InvalidAccountAccessLog accessLog = FindAccessLog(ns);

            if (accessLog == null)
                return true;

            return (DateTime.UtcNow >= (accessLog.LastAccessTime + ComputeThrottle(accessLog.Counts)));
        }

        private static List<InvalidAccountAccessLog> m_List = new List<InvalidAccountAccessLog>();

        public static InvalidAccountAccessLog FindAccessLog(NetState ns)
        {
            if (ns == null)
                return null;

            IPAddress ipAddress = ns.Address;

            for (int i = 0; i < m_List.Count; ++i)
            {
                InvalidAccountAccessLog accessLog = m_List[i];

                if (accessLog.HasExpired)
                    m_List.RemoveAt(i--);
                else if (accessLog.Address.Equals(ipAddress))
                    return accessLog;
            }

            return null;
        }

        public static void RegisterInvalidAccess(NetState ns)
        {
            if (ns == null || !Enabled)
                return;

            InvalidAccountAccessLog accessLog = FindAccessLog(ns);

            if (accessLog == null)
                m_List.Add(accessLog = new InvalidAccountAccessLog(ns.Address));

            accessLog.Counts += 1;
            accessLog.RefreshAccessTime();

            if (accessLog.Counts >= 3)
            {
                try
                {
                    using (StreamWriter op = new StreamWriter("throttle.log", true))
                    {
                        op.WriteLine(
                            "{0}\t{1}\t{2}",
                            DateTime.UtcNow,
                            ns,
                            accessLog.Counts
                        );
                    }
                }
                catch
                {
                }
            }
        }

        public static TimeSpan ComputeThrottle(int counts)
        {
            if (counts >= 15)
                return TimeSpan.FromMinutes(5.0);

            if (counts >= 10)
                return TimeSpan.FromMinutes(1.0);

            if (counts >= 5)
                return TimeSpan.FromSeconds(20.0);

            if (counts >= 3)
                return TimeSpan.FromSeconds(10.0);

            if (counts >= 1)
                return TimeSpan.FromSeconds(2.0);

            return TimeSpan.Zero;
        }
    }

    public class InvalidAccountAccessLog
    {
        private IPAddress m_Address;
        private DateTime m_LastAccessTime;
        private int m_Counts;

        public IPAddress Address
        {
            get { return m_Address; }
            set { m_Address = value; }
        }

        public DateTime LastAccessTime
        {
            get { return m_LastAccessTime; }
            set { m_LastAccessTime = value; }
        }

        public bool HasExpired
        {
            get { return (DateTime.UtcNow >= (m_LastAccessTime + TimeSpan.FromHours(1.0))); }
        }

        public int Counts
        {
            get { return m_Counts; }
            set { m_Counts = value; }
        }

        public void RefreshAccessTime()
        {
            m_LastAccessTime = DateTime.UtcNow;
        }

        public InvalidAccountAccessLog(IPAddress address)
        {
            m_Address = address;
            RefreshAccessTime();
        }
    }
}

namespace Server.Misc
{
    public class IPLimiter
    {
        public static bool Enabled = true;
        public static bool SocketBlock = true; // true to block at connection, false to block at login request

        public static int MaxAddresses = 10;

        public static IPAddress[] Exemptions = new IPAddress[]	//For hosting services where there are cases where IPs can be proxied
		{
			//IPAddress.Parse( "127.0.0.1" ),
		};

        public static bool IsExempt(IPAddress ip)
        {
            for (int i = 0; i < Exemptions.Length; i++)
            {
                if (ip.Equals(Exemptions[i]))
                    return true;
            }

            return false;
        }

        public static bool Verify(IPAddress ourAddress)
        {
            if (!Enabled || IsExempt(ourAddress))
                return true;

            List<NetState> netStates = NetState.Instances;

            int count = 0;

            for (int i = 0; i < netStates.Count; ++i)
            {
                NetState compState = netStates[i];

                if (ourAddress.Equals(compState.Address))
                {
                    ++count;

                    if (count >= MaxAddresses)
                        return false;
                }
            }

            return true;
        }
    }
}