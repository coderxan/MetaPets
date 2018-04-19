using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Server.Engines.Reports
{
    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.PersistableObject.
    /// </summary>
    public class ObjectCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ObjectCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.PersistableObject at a specific position in the ObjectCollection.
        /// </summary>
        public Server.Engines.Reports.PersistableObject this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.PersistableObject)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Append a Server.Engines.Reports.PersistableObject entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.PersistableObject instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.PersistableObject value)
        {
            return this.List.Add(value);
        }

        public void AddRange(PersistableObject[] col)
        {
            this.InnerList.AddRange(col);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.PersistableObject instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.PersistableObject instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.PersistableObject instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.PersistableObject value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.PersistableObject instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.PersistableObject instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.PersistableObject instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.PersistableObject value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.PersistableObject instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.PersistableObject instance to remove.</param>
        public void Remove(Server.Engines.Reports.PersistableObject value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.PersistableObject instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.PersistableObject's enumerator.</returns>
        public new ObjectCollectionEnumerator GetEnumerator()
        {
            return new ObjectCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.PersistableObject instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.PersistableObject instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.PersistableObject value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.PersistableObject.
        /// </summary>
        public class ObjectCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.PersistableObject _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private ObjectCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal ObjectCollectionEnumerator(ObjectCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.PersistableObject object in the enumerated ObjectCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.PersistableObject Current
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

    public abstract class PersistableObject
    {
        public abstract PersistableType TypeID { get; }

        public virtual void SerializeAttributes(PersistanceWriter op)
        {
        }

        public virtual void SerializeChildren(PersistanceWriter op)
        {
        }

        public void Serialize(PersistanceWriter op)
        {
            op.BeginObject(this.TypeID);
            SerializeAttributes(op);
            op.BeginChildren();
            SerializeChildren(op);
            op.FinishChildren();
            op.FinishObject();
        }

        public virtual void DeserializeAttributes(PersistanceReader ip)
        {
        }

        public virtual void DeserializeChildren(PersistanceReader ip)
        {
        }

        public void Deserialize(PersistanceReader ip)
        {
            DeserializeAttributes(ip);

            if (ip.BeginChildren())
            {
                DeserializeChildren(ip);
                ip.FinishChildren();
            }
        }

        public PersistableObject()
        {
        }
    }

    public delegate PersistableObject ConstructCallback();

    public sealed class PersistableTypeRegistry
    {
        private static Hashtable m_Table;

        public static PersistableType Find(string name)
        {
            return m_Table[name] as PersistableType;
        }

        public static void Register(PersistableType type)
        {
            if (type != null)
                m_Table[type.Name] = type;
        }

        static PersistableTypeRegistry()
        {
            m_Table = new Hashtable(StringComparer.OrdinalIgnoreCase);

            Register(Report.ThisTypeID);
            Register(BarGraph.ThisTypeID);
            Register(PieChart.ThisTypeID);
            Register(Snapshot.ThisTypeID);
            Register(ItemValue.ThisTypeID);
            Register(ChartItem.ThisTypeID);
            Register(ReportItem.ThisTypeID);
            Register(ReportColumn.ThisTypeID);
            Register(SnapshotHistory.ThisTypeID);

            Register(PageInfo.ThisTypeID);
            Register(QueueStatus.ThisTypeID);
            Register(StaffHistory.ThisTypeID);
            Register(ResponseInfo.ThisTypeID);
        }
    }

    public sealed class PersistableType
    {
        private string m_Name;
        private ConstructCallback m_Constructor;

        public string Name { get { return m_Name; } }
        public ConstructCallback Constructor { get { return m_Constructor; } }

        public PersistableType(string name, ConstructCallback constructor)
        {
            m_Name = name;
            m_Constructor = constructor;
        }
    }

    public abstract class PersistanceReader
    {
        public abstract int GetInt32(string key);
        public abstract bool GetBoolean(string key);
        public abstract string GetString(string key);
        public abstract DateTime GetDateTime(string key);

        public abstract bool BeginChildren();
        public abstract void FinishChildren();
        public abstract bool HasChild { get; }
        public abstract PersistableObject GetChild();

        public abstract void ReadDocument(PersistableObject root);
        public abstract void Close();

        public PersistanceReader()
        {
        }
    }

    public class XmlPersistanceReader : PersistanceReader
    {
        private StreamReader m_Reader;
        private XmlTextReader m_Xml;
        private string m_Title;

        public XmlPersistanceReader(string filePath, string title)
        {
            m_Reader = new StreamReader(filePath);
            m_Xml = new XmlTextReader(m_Reader);
            m_Xml.WhitespaceHandling = WhitespaceHandling.None;
            m_Title = title;
        }

        public override int GetInt32(string key)
        {
            return XmlConvert.ToInt32(m_Xml.GetAttribute(key));
        }

        public override bool GetBoolean(string key)
        {
            return XmlConvert.ToBoolean(m_Xml.GetAttribute(key));
        }

        public override string GetString(string key)
        {
            return m_Xml.GetAttribute(key);
        }

        public override DateTime GetDateTime(string key)
        {
            string val = m_Xml.GetAttribute(key);

            if (val == null)
                return DateTime.MinValue;

            return XmlConvert.ToDateTime(val, XmlDateTimeSerializationMode.Utc);
        }

        private bool m_HasChild;

        public override bool HasChild
        {
            get
            {
                return m_HasChild;
            }
        }

        private bool m_WasEmptyElement;

        public override bool BeginChildren()
        {
            m_HasChild = !m_WasEmptyElement;

            m_Xml.Read();

            return m_HasChild;
        }

        public override void FinishChildren()
        {
            m_Xml.Read();
        }

        public override PersistableObject GetChild()
        {
            PersistableType type = PersistableTypeRegistry.Find(m_Xml.Name);
            PersistableObject obj = type.Constructor();

            m_WasEmptyElement = m_Xml.IsEmptyElement;

            obj.Deserialize(this);

            m_HasChild = (m_Xml.NodeType == XmlNodeType.Element);

            return obj;
        }

        public override void ReadDocument(PersistableObject root)
        {
            Console.Write("Reports: {0}: Loading...", m_Title);
            m_Xml.Read();
            m_Xml.Read();
            m_HasChild = !m_Xml.IsEmptyElement;
            root.Deserialize(this);
            Console.WriteLine("done");
        }

        public override void Close()
        {
            m_Xml.Close();
            m_Reader.Close();
        }
    }

    public abstract class PersistanceWriter
    {
        public abstract void SetInt32(string key, int value);
        public abstract void SetBoolean(string key, bool value);
        public abstract void SetString(string key, string value);
        public abstract void SetDateTime(string key, DateTime value);

        public abstract void BeginObject(PersistableType typeID);
        public abstract void BeginChildren();
        public abstract void FinishChildren();
        public abstract void FinishObject();

        public abstract void WriteDocument(PersistableObject root);
        public abstract void Close();

        public PersistanceWriter()
        {
        }
    }

    public sealed class XmlPersistanceWriter : PersistanceWriter
    {
        private string m_RealFilePath;
        private string m_TempFilePath;

        private StreamWriter m_Writer;
        private XmlTextWriter m_Xml;
        private string m_Title;

        public XmlPersistanceWriter(string filePath, string title)
        {
            m_RealFilePath = filePath;
            m_TempFilePath = Path.ChangeExtension(filePath, ".tmp");

            m_Writer = new StreamWriter(m_TempFilePath);
            m_Xml = new XmlTextWriter(m_Writer);

            m_Title = title;
        }

        public override void SetInt32(string key, int value)
        {
            m_Xml.WriteAttributeString(key, XmlConvert.ToString(value));
        }

        public override void SetBoolean(string key, bool value)
        {
            m_Xml.WriteAttributeString(key, XmlConvert.ToString(value));
        }

        public override void SetString(string key, string value)
        {
            if (value != null)
                m_Xml.WriteAttributeString(key, value);
        }

        public override void SetDateTime(string key, DateTime value)
        {
            if (value != DateTime.MinValue)
                m_Xml.WriteAttributeString(key, XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc));
        }

        public override void BeginObject(PersistableType typeID)
        {
            m_Xml.WriteStartElement(typeID.Name);
        }

        public override void BeginChildren()
        {
        }

        public override void FinishChildren()
        {
        }

        public override void FinishObject()
        {
            m_Xml.WriteEndElement();
        }

        public override void WriteDocument(PersistableObject root)
        {
            Console.WriteLine("Reports: {0}: Save started", m_Title);

            m_Xml.Formatting = Formatting.Indented;
            m_Xml.IndentChar = '\t';
            m_Xml.Indentation = 1;

            m_Xml.WriteStartDocument(true);

            root.Serialize(this);

            Console.WriteLine("Reports: {0}: Save complete", m_Title);
        }

        public override void Close()
        {
            m_Xml.Close();
            m_Writer.Close();

            try
            {
                string renamed = null;

                if (File.Exists(m_RealFilePath))
                {
                    renamed = Path.ChangeExtension(m_RealFilePath, ".rem");
                    File.Move(m_RealFilePath, renamed);
                    File.Move(m_TempFilePath, m_RealFilePath);
                    File.Delete(renamed);
                }
                else
                {
                    File.Move(m_TempFilePath, m_RealFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}