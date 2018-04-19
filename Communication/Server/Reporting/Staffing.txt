using System;
using System.Collections;
using System.IO;
using System.Xml;

using Server;
using Server.Accounting;
using Server.Engines;
using Server.Engines.Help;

namespace Server.Engines.Reports
{
    public abstract class BaseInfo : IComparable
    {
        private static TimeSpan m_SortRange;

        public static TimeSpan SortRange { get { return m_SortRange; } set { m_SortRange = value; } }

        private string m_Account;
        private string m_Display;
        private PageInfoCollection m_Pages;

        public string Account { get { return m_Account; } set { m_Account = value; } }
        public PageInfoCollection Pages { get { return m_Pages; } set { m_Pages = value; } }

        public string Display
        {
            get
            {
                if (m_Display != null)
                    return m_Display;

                if (m_Account != null)
                {
                    IAccount acct = Accounts.GetAccount(m_Account);

                    if (acct != null)
                    {
                        Mobile mob = null;

                        for (int i = 0; i < acct.Length; ++i)
                        {
                            Mobile check = acct[i];

                            if (check != null && (mob == null || check.AccessLevel > mob.AccessLevel))
                                mob = check;
                        }

                        if (mob != null && mob.Name != null && mob.Name.Length > 0)
                            return (m_Display = mob.Name);
                    }
                }

                return (m_Display = m_Account);
            }
        }

        public int GetPageCount(PageResolution res, DateTime min, DateTime max)
        {
            return StaffHistory.GetPageCount(m_Pages, res, min, max);
        }

        public BaseInfo(string account)
        {
            m_Account = account;
            m_Pages = new PageInfoCollection();
        }

        public void Register(PageInfo page)
        {
            m_Pages.Add(page);
        }

        public void Unregister(PageInfo page)
        {
            m_Pages.Remove(page);
        }

        public int CompareTo(object obj)
        {
            BaseInfo cmp = obj as BaseInfo;

            int v = cmp.GetPageCount(cmp is StaffInfo ? PageResolution.Handled : PageResolution.None, DateTime.UtcNow - m_SortRange, DateTime.UtcNow)
                - this.GetPageCount(this is StaffInfo ? PageResolution.Handled : PageResolution.None, DateTime.UtcNow - m_SortRange, DateTime.UtcNow);

            if (v == 0)
                v = String.Compare(this.Display, cmp.Display);

            return v;
        }
    }

    public class StaffInfo : BaseInfo
    {
        public StaffInfo(string account)
            : base(account)
        {
        }
    }

    public class UserInfo : BaseInfo
    {
        public UserInfo(string account)
            : base(account)
        {
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.PageInfo.
    /// </summary>
    public class PageInfoCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PageInfoCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.PageInfo at a specific position in the PageInfoCollection.
        /// </summary>
        public Server.Engines.Reports.PageInfo this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.PageInfo)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Append a Server.Engines.Reports.PageInfo entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.PageInfo instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.PageInfo value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.PageInfo instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.PageInfo instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.PageInfo instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.PageInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.PageInfo instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.PageInfo instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.PageInfo instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.PageInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.PageInfo instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.PageInfo instance to remove.</param>
        public void Remove(Server.Engines.Reports.PageInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.PageInfo instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.PageInfo's enumerator.</returns>
        public new PageInfoCollectionEnumerator GetEnumerator()
        {
            return new PageInfoCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.PageInfo instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.PageInfo instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.PageInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.PageInfo.
        /// </summary>
        public class PageInfoCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.PageInfo _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private PageInfoCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal PageInfoCollectionEnumerator(PageInfoCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.PageInfo object in the enumerated PageInfoCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.PageInfo Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }

    public enum PageResolution
    {
        None,
        Handled,
        Deleted,
        Logged,
        Canceled
    }

    public class PageInfo : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("pi", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new PageInfo();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private StaffHistory m_History;
        private StaffInfo m_Resolver;
        private UserInfo m_Sender;

        public StaffInfo Resolver
        {
            get { return m_Resolver; }
            set
            {
                if (m_Resolver == value)
                    return;

                lock (StaffHistory.RenderLock)
                {
                    if (m_Resolver != null)
                        m_Resolver.Unregister(this);

                    m_Resolver = value;

                    if (m_Resolver != null)
                        m_Resolver.Register(this);
                }
            }
        }

        public UserInfo Sender
        {
            get { return m_Sender; }
            set
            {
                if (m_Sender == value)
                    return;

                lock (StaffHistory.RenderLock)
                {
                    if (m_Sender != null)
                        m_Sender.Unregister(this);

                    m_Sender = value;

                    if (m_Sender != null)
                        m_Sender.Register(this);
                }
            }
        }

        private PageType m_PageType;
        private PageResolution m_Resolution;

        private DateTime m_TimeSent;
        private DateTime m_TimeResolved;

        private string m_SentBy;
        private string m_ResolvedBy;

        private string m_Message;
        private ResponseInfoCollection m_Responses;

        public StaffHistory History
        {
            get { return m_History; }
            set
            {
                if (m_History == value)
                    return;

                if (m_History != null)
                {
                    Sender = null;
                    Resolver = null;
                }

                m_History = value;

                if (m_History != null)
                {
                    Sender = m_History.GetUserInfo(m_SentBy);
                    UpdateResolver();
                }
            }
        }

        public PageType PageType { get { return m_PageType; } set { m_PageType = value; } }
        public PageResolution Resolution { get { return m_Resolution; } }

        public DateTime TimeSent { get { return m_TimeSent; } set { m_TimeSent = value; } }
        public DateTime TimeResolved { get { return m_TimeResolved; } }

        public string SentBy
        {
            get { return m_SentBy; }
            set
            {
                m_SentBy = value;

                if (m_History != null)
                    Sender = m_History.GetUserInfo(m_SentBy);
            }
        }

        public string ResolvedBy
        {
            get { return m_ResolvedBy; }
        }

        public string Message { get { return m_Message; } set { m_Message = value; } }
        public ResponseInfoCollection Responses { get { return m_Responses; } set { m_Responses = value; } }

        public void UpdateResolver()
        {
            string resolvedBy;
            DateTime timeResolved;
            PageResolution res = GetResolution(out resolvedBy, out timeResolved);

            if (m_History != null && IsStaffResolution(res))
                Resolver = m_History.GetStaffInfo(resolvedBy);
            else
                Resolver = null;

            m_ResolvedBy = resolvedBy;
            m_TimeResolved = timeResolved;
            m_Resolution = res;
        }

        public bool IsStaffResolution(PageResolution res)
        {
            return (res == PageResolution.Handled);
        }

        public static PageResolution ResFromResp(string resp)
        {
            switch (resp)
            {
                case "[Handled]": return PageResolution.Handled;
                case "[Deleting]": return PageResolution.Deleted;
                case "[Logout]": return PageResolution.Logged;
                case "[Canceled]": return PageResolution.Canceled;
            }

            return PageResolution.None;
        }

        public PageResolution GetResolution(out string resolvedBy, out DateTime timeResolved)
        {
            for (int i = m_Responses.Count - 1; i >= 0; --i)
            {
                ResponseInfo resp = m_Responses[i];
                PageResolution res = ResFromResp(resp.Message);

                if (res != PageResolution.None)
                {
                    resolvedBy = resp.SentBy;
                    timeResolved = resp.TimeStamp;
                    return res;
                }
            }

            resolvedBy = m_SentBy;
            timeResolved = m_TimeSent;
            return PageResolution.None;
        }

        public static string GetAccount(Mobile mob)
        {
            if (mob == null)
                return null;

            Accounting.Account acct = mob.Account as Accounting.Account;

            if (acct == null)
                return null;

            return acct.Username;
        }

        public PageInfo()
        {
            m_Responses = new ResponseInfoCollection();
        }

        public PageInfo(PageEntry entry)
        {
            m_PageType = entry.Type;

            m_TimeSent = entry.Sent;
            m_SentBy = GetAccount(entry.Sender);

            m_Message = entry.Message;
            m_Responses = new ResponseInfoCollection();
        }

        public override void SerializeAttributes(PersistanceWriter op)
        {
            op.SetInt32("p", (int)m_PageType);

            op.SetDateTime("ts", m_TimeSent);
            op.SetString("s", m_SentBy);

            op.SetString("m", m_Message);
        }

        public override void DeserializeAttributes(PersistanceReader ip)
        {
            m_PageType = (PageType)ip.GetInt32("p");

            m_TimeSent = ip.GetDateTime("ts");
            m_SentBy = ip.GetString("s");

            m_Message = ip.GetString("m");
        }

        public override void SerializeChildren(PersistanceWriter op)
        {
            lock (this)
            {
                for (int i = 0; i < m_Responses.Count; ++i)
                    m_Responses[i].Serialize(op);
            }
        }

        public override void DeserializeChildren(PersistanceReader ip)
        {
            while (ip.HasChild)
                m_Responses.Add(ip.GetChild() as ResponseInfo);
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.QueueStatus.
    /// </summary>
    public class QueueStatusCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public QueueStatusCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.QueueStatus at a specific position in the QueueStatusCollection.
        /// </summary>
        public Server.Engines.Reports.QueueStatus this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.QueueStatus)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Append a Server.Engines.Reports.QueueStatus entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.QueueStatus instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.QueueStatus value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.QueueStatus instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.QueueStatus instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.QueueStatus instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.QueueStatus value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.QueueStatus instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.QueueStatus instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.QueueStatus instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.QueueStatus value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.QueueStatus instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.QueueStatus instance to remove.</param>
        public void Remove(Server.Engines.Reports.QueueStatus value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.QueueStatus instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.QueueStatus's enumerator.</returns>
        public new QueueStatusCollectionEnumerator GetEnumerator()
        {
            return new QueueStatusCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.QueueStatus instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.QueueStatus instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.QueueStatus value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.QueueStatus.
        /// </summary>
        public class QueueStatusCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.QueueStatus _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private QueueStatusCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal QueueStatusCollectionEnumerator(QueueStatusCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.QueueStatus object in the enumerated QueueStatusCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.QueueStatus Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }

    public class QueueStatus : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("qs", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new QueueStatus();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private DateTime m_TimeStamp;
        private int m_Count;

        public DateTime TimeStamp { get { return m_TimeStamp; } set { m_TimeStamp = value; } }
        public int Count { get { return m_Count; } set { m_Count = value; } }

        public QueueStatus()
        {
        }

        public QueueStatus(int count)
        {
            m_TimeStamp = DateTime.UtcNow;
            m_Count = count;
        }

        public override void SerializeAttributes(PersistanceWriter op)
        {
            op.SetDateTime("t", m_TimeStamp);
            op.SetInt32("c", m_Count);
        }

        public override void DeserializeAttributes(PersistanceReader ip)
        {
            m_TimeStamp = ip.GetDateTime("t");
            m_Count = ip.GetInt32("c");
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.ResponseInfo.
    /// </summary>
    public class ResponseInfoCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ResponseInfoCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.ResponseInfo at a specific position in the ResponseInfoCollection.
        /// </summary>
        public Server.Engines.Reports.ResponseInfo this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.ResponseInfo)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        public int Add(string sentBy, string message)
        {
            return Add(new ResponseInfo(sentBy, message));
        }

        /// <summary>
        /// Append a Server.Engines.Reports.ResponseInfo entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ResponseInfo instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.ResponseInfo value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.ResponseInfo instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ResponseInfo instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.ResponseInfo instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.ResponseInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.ResponseInfo instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ResponseInfo instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.ResponseInfo instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.ResponseInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.ResponseInfo instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.ResponseInfo instance to remove.</param>
        public void Remove(Server.Engines.Reports.ResponseInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.ResponseInfo instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.ResponseInfo's enumerator.</returns>
        public new ResponseInfoCollectionEnumerator GetEnumerator()
        {
            return new ResponseInfoCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.ResponseInfo instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.ResponseInfo instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.ResponseInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.ResponseInfo.
        /// </summary>
        public class ResponseInfoCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.ResponseInfo _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private ResponseInfoCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal ResponseInfoCollectionEnumerator(ResponseInfoCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.ResponseInfo object in the enumerated ResponseInfoCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.ResponseInfo Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }

    public class ResponseInfo : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("rs", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new ResponseInfo();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private DateTime m_TimeStamp;

        private string m_SentBy;
        private string m_Message;

        public DateTime TimeStamp { get { return m_TimeStamp; } set { m_TimeStamp = value; } }

        public string SentBy { get { return m_SentBy; } set { m_SentBy = value; } }
        public string Message { get { return m_Message; } set { m_Message = value; } }

        public ResponseInfo()
        {
        }

        public ResponseInfo(string sentBy, string message)
        {
            m_TimeStamp = DateTime.UtcNow;
            m_SentBy = sentBy;
            m_Message = message;
        }

        public override void SerializeAttributes(PersistanceWriter op)
        {
            op.SetDateTime("t", m_TimeStamp);

            op.SetString("s", m_SentBy);
            op.SetString("m", m_Message);
        }

        public override void DeserializeAttributes(PersistanceReader ip)
        {
            m_TimeStamp = ip.GetDateTime("t");

            m_SentBy = ip.GetString("s");
            m_Message = ip.GetString("m");
        }
    }

    public class StaffHistory : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("stfhst", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new StaffHistory();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private PageInfoCollection m_Pages;
        private QueueStatusCollection m_QueueStats;

        private Hashtable m_UserInfo;
        private Hashtable m_StaffInfo;

        public PageInfoCollection Pages { get { return m_Pages; } set { m_Pages = value; } }
        public QueueStatusCollection QueueStats { get { return m_QueueStats; } set { m_QueueStats = value; } }

        public Hashtable UserInfo { get { return m_UserInfo; } set { m_UserInfo = value; } }
        public Hashtable StaffInfo { get { return m_StaffInfo; } set { m_StaffInfo = value; } }

        public void AddPage(PageInfo info)
        {
            lock (SaveLock)
                m_Pages.Add(info);

            info.History = this;
        }

        public StaffHistory()
        {
            m_Pages = new PageInfoCollection();
            m_QueueStats = new QueueStatusCollection();

            m_UserInfo = new Hashtable(StringComparer.OrdinalIgnoreCase);
            m_StaffInfo = new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        public StaffInfo GetStaffInfo(string account)
        {
            lock (RenderLock)
            {
                if (account == null || account.Length == 0)
                    return null;

                StaffInfo info = m_StaffInfo[account] as StaffInfo;

                if (info == null)
                    m_StaffInfo[account] = info = new StaffInfo(account);

                return info;
            }
        }

        public UserInfo GetUserInfo(string account)
        {
            if (account == null || account.Length == 0)
                return null;

            UserInfo info = m_UserInfo[account] as UserInfo;

            if (info == null)
                m_UserInfo[account] = info = new UserInfo(account);

            return info;
        }

        public static readonly object RenderLock = new object();
        public static readonly object SaveLock = new object();

        public void Save()
        {
            lock (SaveLock)
            {
                string path = Path.Combine(Core.BaseDirectory, "staffHistory.xml");
                PersistanceWriter pw = new XmlPersistanceWriter(path, "Staff");

                pw.WriteDocument(this);

                pw.Close();
            }
        }

        public void Load()
        {
            string path = Path.Combine(Core.BaseDirectory, "staffHistory.xml");

            if (!File.Exists(path))
                return;

            PersistanceReader pr = new XmlPersistanceReader(path, "Staff");

            pr.ReadDocument(this);

            pr.Close();
        }

        public override void SerializeChildren(PersistanceWriter op)
        {
            for (int i = 0; i < m_Pages.Count; ++i)
                m_Pages[i].Serialize(op);

            for (int i = 0; i < m_QueueStats.Count; ++i)
                m_QueueStats[i].Serialize(op);
        }

        public override void DeserializeChildren(PersistanceReader ip)
        {
            DateTime min = DateTime.UtcNow - TimeSpan.FromDays(8.0);

            while (ip.HasChild)
            {
                PersistableObject obj = ip.GetChild();

                if (obj is PageInfo)
                {
                    PageInfo pageInfo = obj as PageInfo;

                    pageInfo.UpdateResolver();

                    if (pageInfo.TimeSent >= min || pageInfo.TimeResolved >= min)
                    {
                        m_Pages.Add(pageInfo);
                        pageInfo.History = this;
                    }
                    else
                    {
                        pageInfo.Sender = null;
                        pageInfo.Resolver = null;
                    }
                }
                else if (obj is QueueStatus)
                {
                    QueueStatus queueStatus = obj as QueueStatus;

                    if (queueStatus.TimeStamp >= min)
                        m_QueueStats.Add(queueStatus);
                }
            }
        }

        public StaffInfo[] GetStaff()
        {
            StaffInfo[] staff = new StaffInfo[m_StaffInfo.Count];
            int index = 0;

            foreach (StaffInfo staffInfo in m_StaffInfo.Values)
                staff[index++] = staffInfo;

            return staff;
        }

        public void Render(ObjectCollection objects)
        {
            lock (RenderLock)
            {
                objects.Add(GraphQueueStatus());

                StaffInfo[] staff = GetStaff();

                BaseInfo.SortRange = TimeSpan.FromDays(7.0);
                Array.Sort(staff);

                objects.Add(GraphHourlyPages(m_Pages, PageResolution.None, "New pages by hour", "graph_new_pages_hr"));
                objects.Add(GraphHourlyPages(m_Pages, PageResolution.Handled, "Handled pages by hour", "graph_handled_pages_hr"));
                objects.Add(GraphHourlyPages(m_Pages, PageResolution.Deleted, "Deleted pages by hour", "graph_deleted_pages_hr"));
                objects.Add(GraphHourlyPages(m_Pages, PageResolution.Canceled, "Canceled pages by hour", "graph_canceled_pages_hr"));
                objects.Add(GraphHourlyPages(m_Pages, PageResolution.Logged, "Logged-out pages by hour", "graph_logged_pages_hr"));

                BaseInfo.SortRange = TimeSpan.FromDays(1.0);
                Array.Sort(staff);

                objects.Add(ReportTotalPages(staff, TimeSpan.FromDays(1.0), "1 Day"));
                objects.AddRange((PersistableObject[])ChartTotalPages(staff, TimeSpan.FromDays(1.0), "1 Day", "graph_daily_pages"));

                BaseInfo.SortRange = TimeSpan.FromDays(7.0);
                Array.Sort(staff);

                objects.Add(ReportTotalPages(staff, TimeSpan.FromDays(7.0), "1 Week"));
                objects.AddRange((PersistableObject[])ChartTotalPages(staff, TimeSpan.FromDays(7.0), "1 Week", "graph_weekly_pages"));

                BaseInfo.SortRange = TimeSpan.FromDays(30.0);
                Array.Sort(staff);

                objects.Add(ReportTotalPages(staff, TimeSpan.FromDays(30.0), "1 Month"));
                objects.AddRange((PersistableObject[])ChartTotalPages(staff, TimeSpan.FromDays(30.0), "1 Month", "graph_monthly_pages"));

                for (int i = 0; i < staff.Length; ++i)
                    objects.Add(GraphHourlyPages(staff[i]));
            }
        }

        public static int GetPageCount(StaffInfo staff, DateTime min, DateTime max)
        {
            return GetPageCount(staff.Pages, PageResolution.Handled, min, max);
        }

        public static int GetPageCount(PageInfoCollection pages, PageResolution res, DateTime min, DateTime max)
        {
            int count = 0;

            for (int i = 0; i < pages.Count; ++i)
            {
                if (res != PageResolution.None && pages[i].Resolution != res)
                    continue;

                DateTime ts = pages[i].TimeResolved;

                if (ts >= min && ts < max)
                    ++count;
            }

            return count;
        }

        private BarGraph GraphQueueStatus()
        {
            int[] totals = new int[24];
            int[] counts = new int[24];

            DateTime max = DateTime.UtcNow;
            DateTime min = max - TimeSpan.FromDays(7.0);

            for (int i = 0; i < m_QueueStats.Count; ++i)
            {
                DateTime ts = m_QueueStats[i].TimeStamp;

                if (ts >= min && ts < max)
                {
                    DateTime date = ts.Date;
                    TimeSpan time = ts.TimeOfDay;

                    int hour = time.Hours;

                    totals[hour] += m_QueueStats[i].Count;
                    counts[hour]++;
                }
            }

            BarGraph barGraph = new BarGraph("Average pages in queue", "graph_pagequeue_avg", 10, "Time", "Pages", BarGraphRenderMode.Lines);

            barGraph.FontSize = 6;

            for (int i = 7; i <= totals.Length + 7; ++i)
            {
                int val;

                if (counts[i % totals.Length] == 0)
                    val = 0;
                else
                    val = (totals[i % totals.Length] + (counts[i % totals.Length] / 2)) / counts[i % totals.Length];

                int realHours = i % totals.Length;
                int hours;

                if (realHours == 0)
                    hours = 12;
                else if (realHours > 12)
                    hours = realHours - 12;
                else
                    hours = realHours;

                barGraph.Items.Add(hours + (realHours >= 12 ? " PM" : " AM"), val);
            }

            return barGraph;
        }

        private BarGraph GraphHourlyPages(StaffInfo staff)
        {
            return GraphHourlyPages(staff.Pages, PageResolution.Handled, "Average pages handled by " + staff.Display, "graphs_" + staff.Account.ToLower() + "_avg");
        }

        private BarGraph GraphHourlyPages(PageInfoCollection pages, PageResolution res, string title, string fname)
        {
            int[] totals = new int[24];
            int[] counts = new int[24];

            DateTime[] dates = new DateTime[24];

            DateTime max = DateTime.UtcNow;
            DateTime min = max - TimeSpan.FromDays(7.0);

            bool sentStamp = (res == PageResolution.None);

            for (int i = 0; i < pages.Count; ++i)
            {
                if (res != PageResolution.None && pages[i].Resolution != res)
                    continue;

                DateTime ts = (sentStamp ? pages[i].TimeSent : pages[i].TimeResolved);

                if (ts >= min && ts < max)
                {
                    DateTime date = ts.Date;
                    TimeSpan time = ts.TimeOfDay;

                    int hour = time.Hours;

                    totals[hour]++;

                    if (dates[hour] != date)
                    {
                        counts[hour]++;
                        dates[hour] = date;
                    }
                }
            }

            BarGraph barGraph = new BarGraph(title, fname, 10, "Time", "Pages", BarGraphRenderMode.Lines);

            barGraph.FontSize = 6;

            for (int i = 7; i <= totals.Length + 7; ++i)
            {
                int val;

                if (counts[i % totals.Length] == 0)
                    val = 0;
                else
                    val = (totals[i % totals.Length] + (counts[i % totals.Length] / 2)) / counts[i % totals.Length];

                int realHours = i % totals.Length;
                int hours;

                if (realHours == 0)
                    hours = 12;
                else if (realHours > 12)
                    hours = realHours - 12;
                else
                    hours = realHours;

                barGraph.Items.Add(hours + (realHours >= 12 ? " PM" : " AM"), val);
            }

            return barGraph;
        }

        private Report ReportTotalPages(StaffInfo[] staff, TimeSpan ts, string title)
        {
            DateTime max = DateTime.UtcNow;
            DateTime min = max - ts;

            Report report = new Report(title + " Staff Report", "400");

            report.Columns.Add("65%", "left", "Staff Name");
            report.Columns.Add("35%", "center", "Page Count");

            for (int i = 0; i < staff.Length; ++i)
                report.Items.Add(staff[i].Display, GetPageCount(staff[i], min, max));

            return report;
        }

        private PieChart[] ChartTotalPages(StaffInfo[] staff, TimeSpan ts, string title, string fname)
        {
            DateTime max = DateTime.UtcNow;
            DateTime min = max - ts;

            PieChart staffChart = new PieChart(title + " Staff Chart", fname + "_staff", true);

            int other = 0;

            for (int i = 0; i < staff.Length; ++i)
            {
                int count = GetPageCount(staff[i], min, max);

                if (i < 12 && count > 0)
                    staffChart.Items.Add(staff[i].Display, count);
                else
                    other += count;
            }

            if (other > 0)
                staffChart.Items.Add("Other", other);

            PieChart resChart = new PieChart(title + " Resolutions", fname + "_resol", true);

            int countTotal = GetPageCount(m_Pages, PageResolution.None, min, max);
            int countHandled = GetPageCount(m_Pages, PageResolution.Handled, min, max);
            int countDeleted = GetPageCount(m_Pages, PageResolution.Deleted, min, max);
            int countCanceled = GetPageCount(m_Pages, PageResolution.Canceled, min, max);
            int countLogged = GetPageCount(m_Pages, PageResolution.Logged, min, max);
            int countUnres = countTotal - (countHandled + countDeleted + countCanceled + countLogged);

            resChart.Items.Add("Handled", countHandled);
            resChart.Items.Add("Deleted", countDeleted);
            resChart.Items.Add("Canceled", countCanceled);
            resChart.Items.Add("Logged Out", countLogged);
            resChart.Items.Add("Unresolved", countUnres);

            return new PieChart[] { staffChart, resChart };
        }
    }
}