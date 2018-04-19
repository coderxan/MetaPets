using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.HuePickers;
using Server.Items;
using Server.Menus;
using Server.Menus.Questions;
using Server.Network;
using Server.Prompts;
using Server.Targeting;

using CPA = Server.CommandPropertyAttribute;

namespace Server
{
    public abstract class PropertyException : ApplicationException
    {
        protected Property m_Property;

        public Property Property
        {
            get { return m_Property; }
        }

        public PropertyException(Property property, string message)
            : base(message)
        {
            m_Property = property;
        }
    }

    public abstract class BindingException : PropertyException
    {
        public BindingException(Property property, string message)
            : base(property, message)
        {
        }
    }

    public sealed class NotYetBoundException : BindingException
    {
        public NotYetBoundException(Property property)
            : base(property, String.Format("Property has not yet been bound."))
        {
        }
    }

    public sealed class AlreadyBoundException : BindingException
    {
        public AlreadyBoundException(Property property)
            : base(property, String.Format("Property has already been bound."))
        {
        }
    }

    public sealed class UnknownPropertyException : BindingException
    {
        public UnknownPropertyException(Property property, string current)
            : base(property, String.Format("Property '{0}' not found.", current))
        {
        }
    }

    public sealed class ReadOnlyException : BindingException
    {
        public ReadOnlyException(Property property)
            : base(property, "Property is read-only.")
        {
        }
    }

    public sealed class WriteOnlyException : BindingException
    {
        public WriteOnlyException(Property property)
            : base(property, "Property is write-only.")
        {
        }
    }

    public abstract class AccessException : PropertyException
    {
        public AccessException(Property property, string message)
            : base(property, message)
        {
        }
    }

    public sealed class InternalAccessException : AccessException
    {
        public InternalAccessException(Property property)
            : base(property, "Property is internal.")
        {
        }
    }

    public abstract class ClearanceException : AccessException
    {
        protected AccessLevel m_PlayerAccess;
        protected AccessLevel m_NeededAccess;

        public AccessLevel PlayerAccess
        {
            get { return m_PlayerAccess; }
        }

        public AccessLevel NeededAccess
        {
            get { return m_NeededAccess; }
        }

        public ClearanceException(Property property, AccessLevel playerAccess, AccessLevel neededAccess, string accessType)
            : base(property, string.Format(
                "You must be at least {0} to {1} this property.",
                Mobile.GetAccessLevelName(neededAccess),
                accessType
            ))
        {
        }
    }

    public sealed class ReadAccessException : ClearanceException
    {
        public ReadAccessException(Property property, AccessLevel playerAccess, AccessLevel neededAccess)
            : base(property, playerAccess, neededAccess, "read")
        {
        }
    }

    public sealed class WriteAccessException : ClearanceException
    {
        public WriteAccessException(Property property, AccessLevel playerAccess, AccessLevel neededAccess)
            : base(property, playerAccess, neededAccess, "write")
        {
        }
    }

    public sealed class Property
    {
        private string m_Binding;

        private PropertyInfo[] m_Chain;
        private PropertyAccess m_Access;

        public string Binding
        {
            get { return m_Binding; }
        }

        public bool IsBound
        {
            get { return (m_Chain != null); }
        }

        public PropertyAccess Access
        {
            get { return m_Access; }
        }

        public PropertyInfo[] Chain
        {
            get
            {
                if (!IsBound)
                    throw new NotYetBoundException(this);

                return m_Chain;
            }
        }

        public Type Type
        {
            get
            {
                if (!IsBound)
                    throw new NotYetBoundException(this);

                return m_Chain[m_Chain.Length - 1].PropertyType;
            }
        }

        public bool CheckAccess(Mobile from)
        {
            if (!IsBound)
                throw new NotYetBoundException(this);

            for (int i = 0; i < m_Chain.Length; ++i)
            {
                PropertyInfo prop = m_Chain[i];

                bool isFinal = (i == (m_Chain.Length - 1));

                PropertyAccess access = m_Access;

                if (!isFinal)
                    access |= PropertyAccess.Read;

                CPA security = Properties.GetCPA(prop);

                if (security == null)
                    throw new InternalAccessException(this);

                if ((access & PropertyAccess.Read) != 0 && from.AccessLevel < security.ReadLevel)
                    throw new ReadAccessException(this, from.AccessLevel, security.ReadLevel);

                if ((access & PropertyAccess.Write) != 0 && (from.AccessLevel < security.WriteLevel || security.ReadOnly))
                    throw new WriteAccessException(this, from.AccessLevel, security.ReadLevel);
            }

            return true;
        }

        public void BindTo(Type objectType, PropertyAccess desiredAccess)
        {
            if (IsBound)
                throw new AlreadyBoundException(this);

            string[] split = m_Binding.Split('.');

            PropertyInfo[] chain = new PropertyInfo[split.Length];

            for (int i = 0; i < split.Length; ++i)
            {
                bool isFinal = (i == (chain.Length - 1));

                chain[i] = objectType.GetProperty(split[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                if (chain[i] == null)
                    throw new UnknownPropertyException(this, split[i]);

                objectType = chain[i].PropertyType;

                PropertyAccess access = desiredAccess;

                if (!isFinal)
                    access |= PropertyAccess.Read;

                if ((access & PropertyAccess.Read) != 0 && !chain[i].CanRead)
                    throw new WriteOnlyException(this);

                if ((access & PropertyAccess.Write) != 0 && !chain[i].CanWrite)
                    throw new ReadOnlyException(this);
            }

            m_Access = desiredAccess;
            m_Chain = chain;
        }

        public Property(string binding)
        {
            m_Binding = binding;
        }

        public Property(PropertyInfo[] chain)
        {
            m_Chain = chain;
        }

        public override string ToString()
        {
            if (!IsBound)
                return m_Binding;

            string[] toJoin = new string[m_Chain.Length];

            for (int i = 0; i < toJoin.Length; ++i)
                toJoin[i] = m_Chain[i].Name;

            return string.Join(".", toJoin);
        }

        public static Property Parse(Type type, string binding, PropertyAccess access)
        {
            Property prop = new Property(binding);

            prop.BindTo(type, access);

            return prop;
        }
    }
}

namespace Server.Commands
{
    public enum PropertyAccess
    {
        Read = 0x01,
        Write = 0x02,
        ReadWrite = Read | Write
    }

    public class Properties
    {
        public static void Initialize()
        {
            CommandSystem.Register("Props", AccessLevel.Counselor, new CommandEventHandler(Props_OnCommand));
        }

        private class PropsTarget : Target
        {
            public PropsTarget()
                : base(-1, true, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (!BaseCommand.IsAccessible(from, o))
                    from.SendMessage("That is not accessible.");
                else
                    from.SendGump(new PropertiesGump(from, o));
            }
        }

        [Usage("Props [serial]")]
        [Description("Opens a menu where you can view and edit all properties of a targeted (or specified) object.")]
        private static void Props_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
            {
                IEntity ent = World.FindEntity(e.GetInt32(0));

                if (ent == null)
                    e.Mobile.SendMessage("No object with that serial was found.");
                else if (!BaseCommand.IsAccessible(e.Mobile, ent))
                    e.Mobile.SendMessage("That is not accessible.");
                else
                    e.Mobile.SendGump(new PropertiesGump(e.Mobile, ent));
            }
            else
            {
                e.Mobile.Target = new PropsTarget();
            }
        }

        private static bool CIEqual(string l, string r)
        {
            return Insensitive.Equals(l, r);
        }

        private static Type typeofCPA = typeof(CPA);

        public static CPA GetCPA(PropertyInfo p)
        {
            object[] attrs = p.GetCustomAttributes(typeofCPA, false);

            if (attrs.Length == 0)
                return null;

            return attrs[0] as CPA;
        }

        public static PropertyInfo[] GetPropertyInfoChain(Mobile from, Type type, string propertyString, PropertyAccess endAccess, ref string failReason)
        {
            string[] split = propertyString.Split('.');

            if (split.Length == 0)
                return null;

            PropertyInfo[] info = new PropertyInfo[split.Length];

            for (int i = 0; i < info.Length; ++i)
            {
                string propertyName = split[i];

                if (CIEqual(propertyName, "current"))
                    continue;

                PropertyInfo[] props = type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

                bool isFinal = (i == (info.Length - 1));

                PropertyAccess access = endAccess;

                if (!isFinal)
                    access |= PropertyAccess.Read;

                for (int j = 0; j < props.Length; ++j)
                {
                    PropertyInfo p = props[j];

                    if (CIEqual(p.Name, propertyName))
                    {
                        CPA attr = GetCPA(p);

                        if (attr == null)
                        {
                            failReason = String.Format("Property '{0}' not found.", propertyName);
                            return null;
                        }
                        else if ((access & PropertyAccess.Read) != 0 && from.AccessLevel < attr.ReadLevel)
                        {
                            failReason = String.Format("You must be at least {0} to get the property '{1}'.",
                                Mobile.GetAccessLevelName(attr.ReadLevel), propertyName);

                            return null;
                        }
                        else if ((access & PropertyAccess.Write) != 0 && from.AccessLevel < attr.WriteLevel)
                        {
                            failReason = String.Format("You must be at least {0} to set the property '{1}'.",
                                Mobile.GetAccessLevelName(attr.WriteLevel), propertyName);

                            return null;
                        }
                        else if ((access & PropertyAccess.Read) != 0 && !p.CanRead)
                        {
                            failReason = String.Format("Property '{0}' is write only.", propertyName);
                            return null;
                        }
                        else if ((access & PropertyAccess.Write) != 0 && (!p.CanWrite || attr.ReadOnly) && isFinal)
                        {
                            failReason = String.Format("Property '{0}' is read only.", propertyName);
                            return null;
                        }

                        info[i] = p;
                        type = p.PropertyType;
                        break;
                    }
                }

                if (info[i] == null)
                {
                    failReason = String.Format("Property '{0}' not found.", propertyName);
                    return null;
                }
            }

            return info;
        }

        public static PropertyInfo GetPropertyInfo(Mobile from, ref object obj, string propertyName, PropertyAccess access, ref string failReason)
        {
            PropertyInfo[] chain = GetPropertyInfoChain(from, obj.GetType(), propertyName, access, ref failReason);

            if (chain == null)
                return null;

            return GetPropertyInfo(ref obj, chain, ref failReason);
        }

        public static PropertyInfo GetPropertyInfo(ref object obj, PropertyInfo[] chain, ref string failReason)
        {
            if (chain == null || chain.Length == 0)
            {
                failReason = "Property chain is empty.";
                return null;
            }

            for (int i = 0; i < chain.Length - 1; ++i)
            {
                if (chain[i] == null)
                    continue;

                obj = chain[i].GetValue(obj, null);

                if (obj == null)
                {
                    failReason = String.Format("Property '{0}' is null.", chain[i]);
                    return null;
                }
            }

            return chain[chain.Length - 1];
        }

        public static string GetValue(Mobile from, object o, string name)
        {
            string failReason = "";

            PropertyInfo[] chain = GetPropertyInfoChain(from, o.GetType(), name, PropertyAccess.Read, ref failReason);

            if (chain == null || chain.Length == 0)
                return failReason;

            PropertyInfo p = GetPropertyInfo(ref o, chain, ref failReason);

            if (p == null)
                return failReason;

            return InternalGetValue(o, p, chain);
        }

        public static string IncreaseValue(Mobile from, object o, string[] args)
        {
            Type type = o.GetType();

            object[] realObjs = new object[args.Length / 2];
            PropertyInfo[] realProps = new PropertyInfo[args.Length / 2];
            int[] realValues = new int[args.Length / 2];

            bool positive = false, negative = false;

            for (int i = 0; i < realProps.Length; ++i)
            {
                string name = args[i * 2];

                try
                {
                    string valueString = args[1 + (i * 2)];

                    if (valueString.StartsWith("0x"))
                    {
                        realValues[i] = Convert.ToInt32(valueString.Substring(2), 16);
                    }
                    else
                    {
                        realValues[i] = Convert.ToInt32(valueString);
                    }
                }
                catch
                {
                    return "Offset value could not be parsed.";
                }

                if (realValues[i] > 0)
                    positive = true;
                else if (realValues[i] < 0)
                    negative = true;
                else
                    return "Zero is not a valid value to offset.";

                string failReason = null;
                realObjs[i] = o;
                realProps[i] = GetPropertyInfo(from, ref realObjs[i], name, PropertyAccess.ReadWrite, ref failReason);

                if (failReason != null)
                    return failReason;

                if (realProps[i] == null)
                    return "Property not found.";
            }

            for (int i = 0; i < realProps.Length; ++i)
            {
                object obj = realProps[i].GetValue(realObjs[i], null);

                if (!(obj is IConvertible))
                    return "Property is not IConvertable.";

                try
                {

                    long v = (long)Convert.ChangeType(obj, TypeCode.Int64);
                    v += realValues[i];

                    realProps[i].SetValue(realObjs[i], Convert.ChangeType(v, realProps[i].PropertyType), null);
                }
                catch
                {
                    return "Value could not be converted";
                }
            }

            if (realProps.Length == 1)
            {
                if (positive)
                    return "The property has been increased.";

                return "The property has been decreased.";
            }

            if (positive && negative)
                return "The properties have been changed.";

            if (positive)
                return "The properties have been increased.";

            return "The properties have been decreased.";
        }

        private static string InternalGetValue(object o, PropertyInfo p)
        {
            return InternalGetValue(o, p, null);
        }

        private static string InternalGetValue(object o, PropertyInfo p, PropertyInfo[] chain)
        {
            Type type = p.PropertyType;

            object value = p.GetValue(o, null);
            string toString;

            if (value == null)
                toString = "null";
            else if (IsNumeric(type))
                toString = String.Format("{0} (0x{0:X})", value);
            else if (IsChar(type))
                toString = String.Format("'{0}' ({1} [0x{1:X}])", value, (int)value);
            else if (IsString(type))
                toString = ((string)value == "null" ? @"@""null""" : String.Format("\"{0}\"", value));
            else if (IsText(type))
                toString = ((TextDefinition)value).Format(false);
            else
                toString = value.ToString();

            if (chain == null)
                return String.Format("{0} = {1}", p.Name, toString);

            string[] concat = new string[chain.Length * 2 + 1];

            for (int i = 0; i < chain.Length; ++i)
            {
                concat[(i * 2) + 0] = chain[i].Name;
                concat[(i * 2) + 1] = (i < (chain.Length - 1)) ? "." : " = ";
            }

            concat[concat.Length - 1] = toString;

            return String.Concat(concat);
        }

        public static string SetValue(Mobile from, object o, string name, string value)
        {
            object logObject = o;

            string failReason = "";
            PropertyInfo p = GetPropertyInfo(from, ref o, name, PropertyAccess.Write, ref failReason);

            if (p == null)
                return failReason;

            return InternalSetValue(from, logObject, o, p, name, value, true);
        }

        private static Type typeofSerial = typeof(Serial);

        private static bool IsSerial(Type t)
        {
            return (t == typeofSerial);
        }

        private static Type typeofType = typeof(Type);

        private static bool IsType(Type t)
        {
            return (t == typeofType);
        }

        private static Type typeofChar = typeof(Char);

        private static bool IsChar(Type t)
        {
            return (t == typeofChar);
        }

        private static Type typeofString = typeof(String);

        private static bool IsString(Type t)
        {
            return (t == typeofString);
        }

        private static Type typeofText = typeof(TextDefinition);

        private static bool IsText(Type t)
        {
            return (t == typeofText);
        }

        private static bool IsEnum(Type t)
        {
            return t.IsEnum;
        }

        private static Type typeofTimeSpan = typeof(TimeSpan);
        private static Type typeofParsable = typeof(ParsableAttribute);

        private static bool IsParsable(Type t)
        {
            return (t == typeofTimeSpan || t.IsDefined(typeofParsable, false));
        }

        private static Type[] m_ParseTypes = new Type[] { typeof(string) };
        private static object[] m_ParseParams = new object[1];

        private static object Parse(object o, Type t, string value)
        {
            MethodInfo method = t.GetMethod("Parse", m_ParseTypes);

            m_ParseParams[0] = value;

            return method.Invoke(o, m_ParseParams);
        }

        private static Type[] m_NumericTypes = new Type[]
			{
				typeof( Byte ), typeof( SByte ),
				typeof( Int16 ), typeof( UInt16 ),
				typeof( Int32 ), typeof( UInt32 ),
				typeof( Int64 ), typeof( UInt64 )
			};

        private static bool IsNumeric(Type t)
        {
            return (Array.IndexOf(m_NumericTypes, t) >= 0);
        }

        public static string ConstructFromString(Type type, object obj, string value, ref object constructed)
        {
            object toSet;
            bool isSerial = IsSerial(type);

            if (isSerial) // mutate into int32
                type = m_NumericTypes[4];

            if (value == "(-null-)" && !type.IsValueType)
                value = null;

            if (IsEnum(type))
            {
                try
                {
                    toSet = Enum.Parse(type, value, true);
                }
                catch
                {
                    return "That is not a valid enumeration member.";
                }
            }
            else if (IsType(type))
            {
                try
                {
                    toSet = ScriptCompiler.FindTypeByName(value);

                    if (toSet == null)
                        return "No type with that name was found.";
                }
                catch
                {
                    return "No type with that name was found.";
                }
            }
            else if (IsParsable(type))
            {
                try
                {
                    toSet = Parse(obj, type, value);
                }
                catch
                {
                    return "That is not properly formatted.";
                }
            }
            else if (value == null)
            {
                toSet = null;
            }
            else if (value.StartsWith("0x") && IsNumeric(type))
            {
                try
                {
                    toSet = Convert.ChangeType(Convert.ToUInt64(value.Substring(2), 16), type);
                }
                catch
                {
                    return "That is not properly formatted.";
                }
            }
            else
            {
                try
                {
                    toSet = Convert.ChangeType(value, type);
                }
                catch
                {
                    return "That is not properly formatted.";
                }
            }

            if (isSerial) // mutate back
                toSet = (Serial)((Int32)toSet);

            constructed = toSet;
            return null;
        }

        public static string SetDirect(Mobile from, object logObject, object obj, PropertyInfo prop, string givenName, object toSet, bool shouldLog)
        {
            try
            {
                if (toSet is AccessLevel)
                {
                    AccessLevel newLevel = (AccessLevel)toSet;
                    AccessLevel reqLevel = AccessLevel.Administrator;

                    if (newLevel == AccessLevel.Administrator)
                        reqLevel = AccessLevel.Developer;
                    else if (newLevel >= AccessLevel.Developer)
                        reqLevel = AccessLevel.Owner;

                    if (from.AccessLevel < reqLevel)
                        return "You do not have access to that level.";
                }

                if (shouldLog)
                    CommandLogging.LogChangeProperty(from, logObject, givenName, toSet == null ? "(-null-)" : toSet.ToString());

                prop.SetValue(obj, toSet, null);
                return "Property has been set.";
            }
            catch
            {
                return "An exception was caught, the property may not be set.";
            }
        }

        public static string SetDirect(object obj, PropertyInfo prop, object toSet)
        {
            try
            {
                if (toSet is AccessLevel)
                {
                    return "You do not have access to that level.";
                }

                prop.SetValue(obj, toSet, null);
                return "Property has been set.";
            }
            catch
            {
                return "An exception was caught, the property may not be set.";
            }
        }

        public static string InternalSetValue(Mobile from, object logobj, object o, PropertyInfo p, string pname, string value, bool shouldLog)
        {
            object toSet = null;
            string result = ConstructFromString(p.PropertyType, o, value, ref toSet);

            if (result != null)
                return result;

            return SetDirect(from, logobj, o, p, pname, toSet, shouldLog);
        }

        public static string InternalSetValue(object o, PropertyInfo p, string value)
        {
            object toSet = null;
            string result = ConstructFromString(p.PropertyType, o, value, ref toSet);

            if (result != null)
                return result;

            return SetDirect(o, p, toSet);
        }
    }
}

namespace Server.Gumps
{
    public class PropsConfig
    {
        public static readonly bool OldStyle = false;

        public static readonly int GumpOffsetX = 30;
        public static readonly int GumpOffsetY = 30;

        public static readonly int TextHue = 0;
        public static readonly int TextOffsetX = 2;

        public static readonly int OffsetGumpID = 0x0A40; // Pure black
        public static readonly int HeaderGumpID = OldStyle ? 0x0BBC : 0x0E14; // Light offwhite, textured : Dark navy blue, textured
        public static readonly int EntryGumpID = 0x0BBC; // Light offwhite, textured
        public static readonly int BackGumpID = 0x13BE; // Gray slate/stoney
        public static readonly int SetGumpID = OldStyle ? 0x0000 : 0x0E14; // Empty : Dark navy blue, textured

        public static readonly int SetWidth = 20;
        public static readonly int SetOffsetX = OldStyle ? 4 : 2, SetOffsetY = 2;
        public static readonly int SetButtonID1 = 0x15E1; // Arrow pointing right
        public static readonly int SetButtonID2 = 0x15E5; // " pressed

        public static readonly int PrevWidth = 20;
        public static readonly int PrevOffsetX = 2, PrevOffsetY = 2;
        public static readonly int PrevButtonID1 = 0x15E3; // Arrow pointing left
        public static readonly int PrevButtonID2 = 0x15E7; // " pressed

        public static readonly int NextWidth = 20;
        public static readonly int NextOffsetX = 2, NextOffsetY = 2;
        public static readonly int NextButtonID1 = 0x15E1; // Arrow pointing right
        public static readonly int NextButtonID2 = 0x15E5; // " pressed

        public static readonly int OffsetSize = 1;

        public static readonly int EntryHeight = 20;
        public static readonly int BorderSize = 10;
    }

    public class StackEntry
    {
        public object m_Object;
        public PropertyInfo m_Property;

        public StackEntry(object obj, PropertyInfo prop)
        {
            m_Object = obj;
            m_Property = prop;
        }
    }

    public class PropertiesGump : Gump
    {
        private ArrayList m_List;
        private int m_Page;
        private Mobile m_Mobile;
        private object m_Object;
        private Type m_Type;
        private Stack<StackEntry> m_Stack;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static bool PrevLabel = OldStyle, NextLabel = OldStyle;
        private static bool TypeLabel = !OldStyle;

        private static readonly int PrevLabelOffsetX = PrevWidth + 1;
        private static readonly int PrevLabelOffsetY = 0;

        private static readonly int NextLabelOffsetX = -29;
        private static readonly int NextLabelOffsetY = 0;

        private static readonly int NameWidth = 107;
        private static readonly int ValueWidth = 128;

        private static readonly int EntryCount = 15;

        private static readonly int TypeWidth = NameWidth + OffsetSize + ValueWidth;

        private static readonly int TotalWidth = OffsetSize + NameWidth + OffsetSize + ValueWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        public PropertiesGump(Mobile mobile, object o)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Mobile = mobile;
            m_Object = o;
            m_Type = o.GetType();
            m_List = BuildList();

            Initialize(0);
        }

        public PropertiesGump(Mobile mobile, object o, Stack<StackEntry> stack, StackEntry parent)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Mobile = mobile;
            m_Object = o;
            m_Type = o.GetType();
            m_Stack = stack;
            m_List = BuildList();

            if (parent != null)
            {
                if (m_Stack == null)
                    m_Stack = new Stack<StackEntry>();

                m_Stack.Push(parent);
            }

            Initialize(0);
        }

        public PropertiesGump(Mobile mobile, object o, Stack<StackEntry> stack, ArrayList list, int page)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Mobile = mobile;
            m_Object = o;

            if (o != null)
                m_Type = o.GetType();

            m_List = list;
            m_Stack = stack;

            Initialize(page);
        }

        private void Initialize(int page)
        {
            m_Page = page;

            int count = m_List.Count - (page * EntryCount);

            if (count < 0)
                count = 0;
            else if (count > EntryCount)
                count = EntryCount;

            int lastIndex = (page * EntryCount) + count - 1;

            if (lastIndex >= 0 && lastIndex < m_List.Count && m_List[lastIndex] == null)
                --count;

            int totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

            AddPage(0);

            AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            int emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

            if (OldStyle)
                AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
            else
                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

            if (page > 0)
            {
                AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 1, GumpButtonType.Reply, 0);

                if (PrevLabel)
                    AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
            }

            x += PrevWidth + OffsetSize;

            if (!OldStyle)
                AddImageTiled(x, y, emptyWidth, EntryHeight, HeaderGumpID);

            if (TypeLabel && m_Type != null)
                AddHtml(x, y, emptyWidth, EntryHeight, String.Format("<BASEFONT COLOR=#FAFAFA><CENTER>{0}</CENTER></BASEFONT>", m_Type.Name), false, false);

            x += emptyWidth + OffsetSize;

            if (!OldStyle)
                AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);

            if ((page + 1) * EntryCount < m_List.Count)
            {
                AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 2, GumpButtonType.Reply, 1);

                if (NextLabel)
                    AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
            }

            for (int i = 0, index = page * EntryCount; i < count && index < m_List.Count; ++i, ++index)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                object o = m_List[index];

                if (o == null)
                {
                    AddImageTiled(x - OffsetSize, y, TotalWidth, EntryHeight, BackGumpID + 4);
                }
                else if (o is Type)
                {
                    Type type = (Type)o;

                    AddImageTiled(x, y, TypeWidth, EntryHeight, EntryGumpID);
                    AddLabelCropped(x + TextOffsetX, y, TypeWidth - TextOffsetX, EntryHeight, TextHue, type.Name);
                    x += TypeWidth + OffsetSize;

                    if (SetGumpID != 0)
                        AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
                }
                else if (o is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)o;

                    AddImageTiled(x, y, NameWidth, EntryHeight, EntryGumpID);
                    AddLabelCropped(x + TextOffsetX, y, NameWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
                    x += NameWidth + OffsetSize;
                    AddImageTiled(x, y, ValueWidth, EntryHeight, EntryGumpID);
                    AddLabelCropped(x + TextOffsetX, y, ValueWidth - TextOffsetX, EntryHeight, TextHue, ValueToString(prop));
                    x += ValueWidth + OffsetSize;

                    if (SetGumpID != 0)
                        AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                    CPA cpa = GetCPA(prop);

                    if (prop.CanWrite && cpa != null && m_Mobile.AccessLevel >= cpa.WriteLevel && !cpa.ReadOnly)
                        AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, i + 3, GumpButtonType.Reply, 0);
                }
            }
        }

        public static string[] m_BoolNames = new string[] { "True", "False" };
        public static object[] m_BoolValues = new object[] { true, false };

        public static string[] m_PoisonNames = new string[] { "None", "Lesser", "Regular", "Greater", "Deadly", "Lethal" };
        public static object[] m_PoisonValues = new object[] { null, Poison.Lesser, Poison.Regular, Poison.Greater, Poison.Deadly, Poison.Lethal };

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            if (!BaseCommand.IsAccessible(from, m_Object))
            {
                from.SendMessage("You may no longer access their properties.");
                return;
            }

            switch (info.ButtonID)
            {
                case 0: // Closed
                    {
                        if (m_Stack != null && m_Stack.Count > 0)
                        {
                            StackEntry entry = m_Stack.Pop();

                            from.SendGump(new PropertiesGump(from, entry.m_Object, m_Stack, null));
                        }

                        break;
                    }
                case 1: // Previous
                    {
                        if (m_Page > 0)
                            from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page - 1));

                        break;
                    }
                case 2: // Next
                    {
                        if ((m_Page + 1) * EntryCount < m_List.Count)
                            from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page + 1));

                        break;
                    }
                default:
                    {
                        int index = (m_Page * EntryCount) + (info.ButtonID - 3);

                        if (index >= 0 && index < m_List.Count)
                        {
                            PropertyInfo prop = m_List[index] as PropertyInfo;

                            if (prop == null)
                                return;

                            CPA attr = GetCPA(prop);

                            if (!prop.CanWrite || attr == null || from.AccessLevel < attr.WriteLevel || attr.ReadOnly)
                                return;

                            Type type = prop.PropertyType;

                            if (IsType(type, typeofMobile) || IsType(type, typeofItem))
                                from.SendGump(new SetObjectGump(prop, from, m_Object, m_Stack, type, m_Page, m_List));
                            else if (IsType(type, typeofType))
                                from.Target = new SetObjectTarget(prop, from, m_Object, m_Stack, type, m_Page, m_List);
                            else if (IsType(type, typeofPoint3D))
                                from.SendGump(new SetPoint3DGump(prop, from, m_Object, m_Stack, m_Page, m_List));
                            else if (IsType(type, typeofPoint2D))
                                from.SendGump(new SetPoint2DGump(prop, from, m_Object, m_Stack, m_Page, m_List));
                            else if (IsType(type, typeofTimeSpan))
                                from.SendGump(new SetTimeSpanGump(prop, from, m_Object, m_Stack, m_Page, m_List));
                            else if (IsCustomEnum(type))
                                from.SendGump(new SetCustomEnumGump(prop, from, m_Object, m_Stack, m_Page, m_List, GetCustomEnumNames(type)));
                            else if (IsType(type, typeofEnum))
                                from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, Enum.GetNames(type), GetObjects(Enum.GetValues(type))));
                            else if (IsType(type, typeofBool))
                                from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, m_BoolNames, m_BoolValues));
                            else if (IsType(type, typeofString) || IsType(type, typeofReal) || IsType(type, typeofNumeric) || IsType(type, typeofText))
                                from.SendGump(new SetGump(prop, from, m_Object, m_Stack, m_Page, m_List));
                            else if (IsType(type, typeofPoison))
                                from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, m_PoisonNames, m_PoisonValues));
                            else if (IsType(type, typeofMap))
                                from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, Map.GetMapNames(), Map.GetMapValues()));
                            else if (IsType(type, typeofSkills) && m_Object is Mobile)
                            {
                                from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
                                from.SendGump(new SkillsGump(from, (Mobile)m_Object));
                            }
                            else if (HasAttribute(type, typeofPropertyObject, true))
                            {
                                object obj = prop.GetValue(m_Object, null);

                                if (obj != null)
                                    from.SendGump(new PropertiesGump(from, obj, m_Stack, new StackEntry(m_Object, prop)));
                                else
                                    from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
                            }
                        }

                        break;
                    }
            }
        }

        private static object[] GetObjects(Array a)
        {
            object[] list = new object[a.Length];

            for (int i = 0; i < list.Length; ++i)
                list[i] = a.GetValue(i);

            return list;
        }

        private static bool IsCustomEnum(Type type)
        {
            return type.IsDefined(typeofCustomEnum, false);
        }

        public static void OnValueChanged(object obj, PropertyInfo prop, Stack<StackEntry> stack)
        {
            if (stack == null || stack.Count == 0)
                return;

            if (!prop.PropertyType.IsValueType)
                return;

            StackEntry peek = stack.Peek();

            if (peek.m_Property.CanWrite)
                peek.m_Property.SetValue(peek.m_Object, obj, null);
        }

        private static string[] GetCustomEnumNames(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeofCustomEnum, false);

            if (attrs.Length == 0)
                return new string[0];

            CustomEnumAttribute ce = attrs[0] as CustomEnumAttribute;

            if (ce == null)
                return new string[0];

            return ce.Names;
        }

        private static bool HasAttribute(Type type, Type check, bool inherit)
        {
            object[] objs = type.GetCustomAttributes(check, inherit);

            return (objs != null && objs.Length > 0);
        }

        private static bool IsType(Type type, Type check)
        {
            return type == check || type.IsSubclassOf(check);
        }

        private static bool IsType(Type type, Type[] check)
        {
            for (int i = 0; i < check.Length; ++i)
                if (IsType(type, check[i]))
                    return true;

            return false;
        }

        private static Type typeofMobile = typeof(Mobile);
        private static Type typeofItem = typeof(Item);
        private static Type typeofType = typeof(Type);
        private static Type typeofPoint3D = typeof(Point3D);
        private static Type typeofPoint2D = typeof(Point2D);
        private static Type typeofTimeSpan = typeof(TimeSpan);
        private static Type typeofCustomEnum = typeof(CustomEnumAttribute);
        private static Type typeofEnum = typeof(Enum);
        private static Type typeofBool = typeof(Boolean);
        private static Type typeofString = typeof(String);
        private static Type typeofText = typeof(TextDefinition);
        private static Type typeofPoison = typeof(Poison);
        private static Type typeofMap = typeof(Map);
        private static Type typeofSkills = typeof(Skills);
        private static Type typeofPropertyObject = typeof(PropertyObjectAttribute);
        private static Type typeofNoSort = typeof(NoSortAttribute);

        private static Type[] typeofReal = new Type[]
			{
				typeof( Single ),
				typeof( Double )
			};

        private static Type[] typeofNumeric = new Type[]
			{
				typeof( Byte ),
				typeof( Int16 ),
				typeof( Int32 ),
				typeof( Int64 ),
				typeof( SByte ),
				typeof( UInt16 ),
				typeof( UInt32 ),
				typeof( UInt64 )
			};

        private string ValueToString(PropertyInfo prop)
        {
            return ValueToString(m_Object, prop);
        }

        public static string ValueToString(object obj, PropertyInfo prop)
        {
            try
            {
                return ValueToString(prop.GetValue(obj, null));
            }
            catch (Exception e)
            {
                return String.Format("!{0}!", e.GetType());
            }
        }

        public static string ValueToString(object o)
        {
            if (o == null)
            {
                return "-null-";
            }
            else if (o is string)
            {
                return String.Format("\"{0}\"", (string)o);
            }
            else if (o is bool)
            {
                return o.ToString();
            }
            else if (o is char)
            {
                return String.Format("0x{0:X} '{1}'", (int)(char)o, (char)o);
            }
            else if (o is Serial)
            {
                Serial s = (Serial)o;

                if (s.IsValid)
                {
                    if (s.IsItem)
                    {
                        return String.Format("(I) 0x{0:X}", s.Value);
                    }
                    else if (s.IsMobile)
                    {
                        return String.Format("(M) 0x{0:X}", s.Value);
                    }
                }

                return String.Format("(?) 0x{0:X}", s.Value);
            }
            else if (o is byte || o is sbyte || o is short || o is ushort || o is int || o is uint || o is long || o is ulong)
            {
                return String.Format("{0} (0x{0:X})", o);
            }
            else if (o is Mobile)
            {
                return String.Format("(M) 0x{0:X} \"{1}\"", ((Mobile)o).Serial.Value, ((Mobile)o).Name);
            }
            else if (o is Item)
            {
                return String.Format("(I) 0x{0:X}", ((Item)o).Serial.Value);
            }
            else if (o is Type)
            {
                return ((Type)o).Name;
            }
            else if (o is TextDefinition)
            {
                return ((TextDefinition)o).Format(true);
            }
            else
            {
                return o.ToString();
            }
        }

        private ArrayList BuildList()
        {
            ArrayList list = new ArrayList();

            if (m_Type == null)
                return list;

            PropertyInfo[] props = m_Type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

            ArrayList groups = GetGroups(m_Type, props);

            for (int i = 0; i < groups.Count; ++i)
            {
                DictionaryEntry de = (DictionaryEntry)groups[i];
                ArrayList groupList = (ArrayList)de.Value;

                if (!HasAttribute((Type)de.Key, typeofNoSort, false))
                    groupList.Sort(PropertySorter.Instance);

                if (i != 0)
                    list.Add(null);

                list.Add(de.Key);
                list.AddRange(groupList);
            }

            return list;
        }

        private static Type typeofCPA = typeof(CPA);
        private static Type typeofObject = typeof(object);

        private static CPA GetCPA(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(typeofCPA, false);

            if (attrs.Length > 0)
                return attrs[0] as CPA;
            else
                return null;
        }

        private ArrayList GetGroups(Type objectType, PropertyInfo[] props)
        {
            Hashtable groups = new Hashtable();

            for (int i = 0; i < props.Length; ++i)
            {
                PropertyInfo prop = props[i];

                if (prop.CanRead)
                {
                    CPA attr = GetCPA(prop);

                    if (attr != null && m_Mobile.AccessLevel >= attr.ReadLevel)
                    {
                        Type type = prop.DeclaringType;

                        while (true)
                        {
                            Type baseType = type.BaseType;

                            if (baseType == null || baseType == typeofObject)
                                break;

                            if (baseType.GetProperty(prop.Name, prop.PropertyType) != null)
                                type = baseType;
                            else
                                break;
                        }

                        ArrayList list = (ArrayList)groups[type];

                        if (list == null)
                            groups[type] = list = new ArrayList();

                        list.Add(prop);
                    }
                }
            }

            ArrayList sorted = new ArrayList(groups);

            sorted.Sort(new GroupComparer(objectType));

            return sorted;
        }

        public static object GetObjectFromString(Type t, string s)
        {
            if (t == typeof(string))
            {
                return s;
            }
            else if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong))
            {
                if (s.StartsWith("0x"))
                {
                    if (t == typeof(ulong) || t == typeof(uint) || t == typeof(ushort) || t == typeof(byte))
                    {
                        return Convert.ChangeType(Convert.ToUInt64(s.Substring(2), 16), t);
                    }
                    else
                    {
                        return Convert.ChangeType(Convert.ToInt64(s.Substring(2), 16), t);
                    }
                }
                else
                {
                    return Convert.ChangeType(s, t);
                }
            }
            else if (t == typeof(double) || t == typeof(float))
            {
                return Convert.ChangeType(s, t);
            }
            else if (t.IsDefined(typeof(ParsableAttribute), false))
            {
                MethodInfo parseMethod = t.GetMethod("Parse", new Type[] { typeof(string) });

                return parseMethod.Invoke(null, new object[] { s });
            }

            throw new Exception("bad");
        }

        private static string GetStringFromObject(object o)
        {
            if (o == null)
            {
                return "-null-";
            }
            else if (o is string)
            {
                return String.Format("\"{0}\"", (string)o);
            }
            else if (o is bool)
            {
                return o.ToString();
            }
            else if (o is char)
            {
                return String.Format("0x{0:X} '{1}'", (int)(char)o, (char)o);
            }
            else if (o is Serial)
            {
                Serial s = (Serial)o;

                if (s.IsValid)
                {
                    if (s.IsItem)
                    {
                        return String.Format("(I) 0x{0:X}", s.Value);
                    }
                    else if (s.IsMobile)
                    {
                        return String.Format("(M) 0x{0:X}", s.Value);
                    }
                }

                return String.Format("(?) 0x{0:X}", s.Value);
            }
            else if (o is byte || o is sbyte || o is short || o is ushort || o is int || o is uint || o is long || o is ulong)
            {
                return String.Format("{0} (0x{0:X})", o);
            }
            else if (o is Mobile)
            {
                return String.Format("(M) 0x{0:X} \"{1}\"", ((Mobile)o).Serial.Value, ((Mobile)o).Name);
            }
            else if (o is Item)
            {
                return String.Format("(I) 0x{0:X}", ((Item)o).Serial.Value);
            }
            else if (o is Type)
            {
                return ((Type)o).Name;
            }
            else
            {
                return o.ToString();
            }
        }

        private class PropertySorter : IComparer
        {
            public static readonly PropertySorter Instance = new PropertySorter();

            private PropertySorter()
            {
            }

            public int Compare(object x, object y)
            {
                if (x == null && y == null)
                    return 0;
                else if (x == null)
                    return -1;
                else if (y == null)
                    return 1;

                PropertyInfo a = x as PropertyInfo;
                PropertyInfo b = y as PropertyInfo;

                if (a == null || b == null)
                    throw new ArgumentException();

                return a.Name.CompareTo(b.Name);
            }
        }

        private class GroupComparer : IComparer
        {
            private Type m_Start;

            public GroupComparer(Type start)
            {
                m_Start = start;
            }

            private static Type typeofObject = typeof(Object);

            private int GetDistance(Type type)
            {
                Type current = m_Start;

                int dist;

                for (dist = 0; current != null && current != typeofObject && current != type; ++dist)
                    current = current.BaseType;

                return dist;
            }

            public int Compare(object x, object y)
            {
                if (x == null && y == null)
                    return 0;
                else if (x == null)
                    return -1;
                else if (y == null)
                    return 1;

                if (!(x is DictionaryEntry) || !(y is DictionaryEntry))
                    throw new ArgumentException();

                DictionaryEntry de1 = (DictionaryEntry)x;
                DictionaryEntry de2 = (DictionaryEntry)y;

                Type a = (Type)de1.Key;
                Type b = (Type)de2.Key;

                return GetDistance(a).CompareTo(GetDistance(b));
            }
        }
    }

    public class SetGump : Gump
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private int m_Page;
        private ArrayList m_List;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static readonly int EntryWidth = 212;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + (2 * (EntryHeight + OffsetSize));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        public SetGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = page;
            m_List = list;

            bool canNull = !prop.PropertyType.IsValueType;
            bool canDye = prop.IsDefined(typeof(HueAttribute), false);
            bool isBody = prop.IsDefined(typeof(BodyAttribute), false);

            object val = prop.GetValue(m_Object, null);
            string initialText;

            if (val == null)
                initialText = "";
            else if (val is TextDefinition)
                initialText = ((TextDefinition)val).GetValue();
            else
                initialText = val.ToString();

            AddPage(0);

            AddBackground(0, 0, BackWidth, BackHeight + (canNull ? (EntryHeight + OffsetSize) : 0) + (canDye ? (EntryHeight + OffsetSize) : 0) + (isBody ? (EntryHeight + OffsetSize) : 0), BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight + (canNull ? (EntryHeight + OffsetSize) : 0) + (canDye ? (EntryHeight + OffsetSize) : 0) + (isBody ? (EntryHeight + OffsetSize) : 0), OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddTextEntry(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, 0, initialText);
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

            if (canNull)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
                AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Null");
                x += EntryWidth + OffsetSize;

                if (SetGumpID != 0)
                    AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);
            }

            if (canDye)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
                AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Hue Picker");
                x += EntryWidth + OffsetSize;

                if (SetGumpID != 0)
                    AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);
            }

            if (isBody)
            {
                x = BorderSize + OffsetSize;
                y += EntryHeight + OffsetSize;

                AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
                AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Body Picker");
                x += EntryWidth + OffsetSize;

                if (SetGumpID != 0)
                    AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 4, GumpButtonType.Reply, 0);
            }
        }

        private class InternalPicker : HuePicker
        {
            private PropertyInfo m_Property;
            private Mobile m_Mobile;
            private object m_Object;
            private Stack<StackEntry> m_Stack;
            private int m_Page;
            private ArrayList m_List;

            public InternalPicker(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
                : base(((IHued)o).HuedItemID)
            {
                m_Property = prop;
                m_Mobile = mobile;
                m_Object = o;
                m_Stack = stack;
                m_Page = page;
                m_List = list;
            }

            public override void OnResponse(int hue)
            {
                try
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, hue.ToString());
                    m_Property.SetValue(m_Object, hue, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }

                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            object toSet;
            bool shouldSet, shouldSend = true;

            switch (info.ButtonID)
            {
                case 1:
                    {
                        TextRelay text = info.GetTextEntry(0);

                        if (text != null)
                        {
                            try
                            {
                                toSet = PropertiesGump.GetObjectFromString(m_Property.PropertyType, text.Text);
                                shouldSet = true;
                            }
                            catch
                            {
                                toSet = null;
                                shouldSet = false;
                                m_Mobile.SendMessage("Bad format");
                            }
                        }
                        else
                        {
                            toSet = null;
                            shouldSet = false;
                        }

                        break;
                    }
                case 2: // Null
                    {
                        toSet = null;
                        shouldSet = true;

                        break;
                    }
                case 3: // Hue Picker
                    {
                        toSet = null;
                        shouldSet = false;
                        shouldSend = false;

                        m_Mobile.SendHuePicker(new InternalPicker(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List));

                        break;
                    }
                case 4: // Body Picker
                    {
                        toSet = null;
                        shouldSet = false;
                        shouldSend = false;

                        m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List));

                        break;
                    }
                default:
                    {
                        toSet = null;
                        shouldSet = false;

                        break;
                    }
            }

            if (shouldSet)
            {
                try
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet == null ? "(null)" : toSet.ToString());
                    m_Property.SetValue(m_Object, toSet, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            if (shouldSend)
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }

    public class SetListOptionGump : Gump
    {
        protected PropertyInfo m_Property;
        protected Mobile m_Mobile;
        protected object m_Object;
        protected Stack<StackEntry> m_Stack;
        protected int m_Page;
        protected ArrayList m_List;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static readonly int EntryWidth = 212;
        private static readonly int EntryCount = 13;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;

        private static bool PrevLabel = OldStyle, NextLabel = OldStyle;

        private static readonly int PrevLabelOffsetX = PrevWidth + 1;
        private static readonly int PrevLabelOffsetY = 0;

        private static readonly int NextLabelOffsetX = -29;
        private static readonly int NextLabelOffsetY = 0;

        protected object[] m_Values;

        public SetListOptionGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int propspage, ArrayList list, string[] names, object[] values)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = propspage;
            m_List = list;

            m_Values = values;

            int pages = (names.Length + EntryCount - 1) / EntryCount;
            int index = 0;

            for (int page = 1; page <= pages; ++page)
            {
                AddPage(page);

                int start = (page - 1) * EntryCount;
                int count = names.Length - start;

                if (count > EntryCount)
                    count = EntryCount;

                int totalHeight = OffsetSize + ((count + 2) * (EntryHeight + OffsetSize));
                int backHeight = BorderSize + totalHeight + BorderSize;

                AddBackground(0, 0, BackWidth, backHeight, BackGumpID);
                AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);



                int x = BorderSize + OffsetSize;
                int y = BorderSize + OffsetSize;

                int emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

                if (page > 1)
                {
                    AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 0, GumpButtonType.Page, page - 1);

                    if (PrevLabel)
                        AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
                }

                x += PrevWidth + OffsetSize;

                if (!OldStyle)
                    AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, HeaderGumpID);

                x += emptyWidth + OffsetSize;

                if (!OldStyle)
                    AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);

                if (page < pages)
                {
                    AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 0, GumpButtonType.Page, page + 1);

                    if (NextLabel)
                        AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
                }



                AddRect(0, prop.Name, 0);

                for (int i = 0; i < count; ++i)
                    AddRect(i + 1, names[index], ++index);
            }
        }

        private void AddRect(int index, string str, int button)
        {
            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize + ((index + 1) * (EntryHeight + OffsetSize));

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            if (button != 0)
                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int index = info.ButtonID - 1;

            if (index >= 0 && index < m_Values.Length)
            {
                try
                {
                    object toSet = m_Values[index];

                    string result = Properties.SetDirect(m_Mobile, m_Object, m_Object, m_Property, m_Property.Name, toSet, true);

                    m_Mobile.SendMessage(result);

                    if (result == "Property has been set.")
                        PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }

    public class SetCustomEnumGump : SetListOptionGump
    {
        private string[] m_Names;

        public SetCustomEnumGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int propspage, ArrayList list, string[] names)
            : base(prop, mobile, o, stack, propspage, list, names, null)
        {
            m_Names = names;
        }

        public override void OnResponse(NetState sender, RelayInfo relayInfo)
        {
            int index = relayInfo.ButtonID - 1;

            if (index >= 0 && index < m_Names.Length)
            {
                try
                {
                    MethodInfo info = m_Property.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });

                    string result = "";

                    if (info != null)
                        result = Properties.SetDirect(m_Mobile, m_Object, m_Object, m_Property, m_Property.Name, info.Invoke(null, new object[] { m_Names[index] }), true);
                    else if (m_Property.PropertyType == typeof(Enum) || m_Property.PropertyType.IsSubclassOf(typeof(Enum)))
                        result = Properties.SetDirect(m_Mobile, m_Object, m_Object, m_Property, m_Property.Name, Enum.Parse(m_Property.PropertyType, m_Names[index], false), true);

                    m_Mobile.SendMessage(result);

                    if (result == "Property has been set.")
                        PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }

    public class SetBodyGump : Gump
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private int m_Page;
        private ArrayList m_List;
        private int m_OurPage;
        private ArrayList m_OurList;
        private ModelBodyType m_OurType;

        private const int LabelColor32 = 0xFFFFFF;
        private const int SelectedColor32 = 0x8080FF;
        private const int TextColor32 = 0xFFFFFF;

        public SetBodyGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
            : this(prop, mobile, o, stack, page, list, 0, null, ModelBodyType.Invalid)
        {
        }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        public void AddTypeButton(int x, int y, int buttonID, string text, ModelBodyType type)
        {
            bool isSelection = (m_OurType == type);

            AddButton(x, y - 1, isSelection ? 4006 : 4005, 4007, buttonID, GumpButtonType.Reply, 0);
            AddHtml(x + 35, y, 200, 20, Color(text, isSelection ? SelectedColor32 : LabelColor32), false, false);
        }

        public SetBodyGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list, int ourPage, ArrayList ourList, ModelBodyType ourType)
            : base(20, 30)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = page;
            m_List = list;
            m_OurPage = ourPage;
            m_OurList = ourList;
            m_OurType = ourType;

            AddPage(0);

            AddBackground(0, 0, 525, 328, 5054);

            AddImageTiled(10, 10, 505, 20, 0xA40);
            AddAlphaRegion(10, 10, 505, 20);

            AddImageTiled(10, 35, 505, 283, 0xA40);
            AddAlphaRegion(10, 35, 505, 283);

            AddTypeButton(10, 10, 1, "Monster", ModelBodyType.Monsters);
            AddTypeButton(130, 10, 2, "Animal", ModelBodyType.Animals);
            AddTypeButton(250, 10, 3, "Marine", ModelBodyType.Sea);
            AddTypeButton(370, 10, 4, "Human", ModelBodyType.Human);

            AddImage(480, 12, 0x25EA);
            AddImage(497, 12, 0x25E6);

            if (ourList == null)
            {
                AddLabel(15, 40, 0x480, "Choose a body type above.");
            }
            else if (ourList.Count == 0)
            {
                AddLabel(15, 40, 0x480, "The server must have UO:3D installed to use this feature.");
            }
            else
            {
                for (int i = 0, index = (ourPage * 12); i < 12 && index >= 0 && index < ourList.Count; ++i, ++index)
                {
                    InternalEntry entry = (InternalEntry)ourList[index];
                    int itemID = entry.ItemID;

                    Rectangle2D bounds = ItemBounds.Table[itemID & 0x3FFF];

                    int x = 15 + ((i % 4) * 125);
                    int y = 40 + ((i / 4) * 93);

                    AddItem(x + ((120 - bounds.Width) / 2) - bounds.X, y + ((69 - bounds.Height) / 2) - bounds.Y, itemID);
                    AddButton(x + 6, y + 66, 0x98D, 0x98D, 7 + index, GumpButtonType.Reply, 0);

                    x += 6;
                    y += 67;

                    AddHtml(x + 0, y - 1, 108, 21, Center(entry.DisplayName), false, false);
                    AddHtml(x + 0, y + 1, 108, 21, Center(entry.DisplayName), false, false);
                    AddHtml(x - 1, y + 0, 108, 21, Center(entry.DisplayName), false, false);
                    AddHtml(x + 1, y + 0, 108, 21, Center(entry.DisplayName), false, false);
                    AddHtml(x + 0, y + 0, 108, 21, Color(Center(entry.DisplayName), TextColor32), false, false);
                }

                if (ourPage > 0)
                    AddButton(480, 12, 0x15E3, 0x15E7, 5, GumpButtonType.Reply, 0);

                if ((ourPage + 1) * 12 < ourList.Count)
                    AddButton(497, 12, 0x15E1, 0x15E5, 6, GumpButtonType.Reply, 0);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int index = info.ButtonID - 1;

            if (index == -1)
            {
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
            }
            else if (index >= 0 && index < 4)
            {
                if (m_Monster == null)
                    LoadLists();

                ModelBodyType type;
                ArrayList list;

                switch (index)
                {
                    default:
                    case 0: type = ModelBodyType.Monsters; list = m_Monster; break;
                    case 1: type = ModelBodyType.Animals; list = m_Animal; break;
                    case 2: type = ModelBodyType.Sea; list = m_Sea; break;
                    case 3: type = ModelBodyType.Human; list = m_Human; break;
                }

                m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, 0, list, type));
            }
            else if (m_OurList != null)
            {
                index -= 4;

                if (index == 0 && m_OurPage > 0)
                {
                    m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, m_OurPage - 1, m_OurList, m_OurType));
                }
                else if (index == 1 && ((m_OurPage + 1) * 12) < m_OurList.Count)
                {
                    m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, m_OurPage + 1, m_OurList, m_OurType));
                }
                else
                {
                    index -= 2;

                    if (index >= 0 && index < m_OurList.Count)
                    {
                        try
                        {
                            InternalEntry entry = (InternalEntry)m_OurList[index];

                            CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, entry.Body.ToString());
                            m_Property.SetValue(m_Object, entry.Body, null);
                            PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                        }
                        catch
                        {
                            m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                        }

                        m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, m_OurPage, m_OurList, m_OurType));
                    }
                }
            }
        }

        private static ArrayList m_Monster, m_Animal, m_Sea, m_Human;

        private static void LoadLists()
        {
            m_Monster = new ArrayList();
            m_Animal = new ArrayList();
            m_Sea = new ArrayList();
            m_Human = new ArrayList();

            List<BodyEntry> entries = Docs.LoadBodies();

            for (int i = 0; i < entries.Count; ++i)
            {
                BodyEntry oldEntry = (BodyEntry)entries[i];
                int bodyID = oldEntry.Body.BodyID;

                if (((Body)bodyID).IsEmpty)
                    continue;

                ArrayList list = null;

                switch (oldEntry.BodyType)
                {
                    case ModelBodyType.Monsters: list = m_Monster; break;
                    case ModelBodyType.Animals: list = m_Animal; break;
                    case ModelBodyType.Sea: list = m_Sea; break;
                    case ModelBodyType.Human: list = m_Human; break;
                }

                if (list == null)
                    continue;

                int itemID = ShrinkTable.Lookup(bodyID, -1);

                if (itemID != -1)
                    list.Add(new InternalEntry(bodyID, itemID, oldEntry.Name));
            }

            m_Monster.Sort();
            m_Animal.Sort();
            m_Sea.Sort();
            m_Human.Sort();
        }

        private class InternalEntry : IComparable
        {
            private int m_Body;
            private int m_ItemID;
            private string m_Name;
            private string m_DisplayName;

            public int Body { get { return m_Body; } }
            public int ItemID { get { return m_ItemID; } }
            public string Name { get { return m_Name; } }
            public string DisplayName { get { return m_DisplayName; } }

            private static string[] m_GroupNames = new string[]
				{
					"ogres_", "ettins_", "walking_dead_", "gargoyles_",
					"orcs_", "flails_", "daemons_", "arachnids_",
					"dragons_", "elementals_", "serpents_", "gazers_",
					"liche_", "spirits_", "harpies_", "headless_",
					"lizard_race_", "mongbat_", "rat_race_", "scorpions_",
					"trolls_", "slimes_", "skeletons_", "ethereals_",
					"terathan_", "imps_", "cyclops_", "krakens_",
					"frogs_", "ophidians_", "centaurs_", "mages_",
					"fey_race_", "genies_", "paladins_", "shadowlords_",
					"succubi_", "lizards_", "rodents_", "birds_",
					"bovines_", "bruins_", "canines_", "deer_",
					"equines_", "felines_", "fowl_", "gorillas_",
					"kirin_", "llamas_", "ostards_", "porcines_",
					"ruminants_", "walrus_", "dolphins_", "sea_horse_",
					"sea_serpents_", "character_", "h_", "titans_"
				};

            public InternalEntry(int body, int itemID, string name)
            {
                m_Body = body;
                m_ItemID = itemID;
                m_Name = name;

                m_DisplayName = name.ToLower();

                for (int i = 0; i < m_GroupNames.Length; ++i)
                {
                    if (m_DisplayName.StartsWith(m_GroupNames[i]))
                    {
                        m_DisplayName = m_DisplayName.Substring(m_GroupNames[i].Length);
                        break;
                    }
                }

                m_DisplayName = m_DisplayName.Replace('_', ' ');
            }

            public int CompareTo(object obj)
            {
                InternalEntry comp = (InternalEntry)obj;

                int v = m_Name.CompareTo(comp.m_Name);

                if (v == 0)
                    m_Body.CompareTo(comp.m_Body);

                return v;
            }
        }
    }

    public class SetObjectTarget : Target
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private Type m_Type;
        private int m_Page;
        private ArrayList m_List;

        public SetObjectTarget(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, Type type, int page, ArrayList list)
            : base(-1, false, TargetFlags.None)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Type = type;
            m_Page = page;
            m_List = list;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            try
            {
                if (m_Type == typeof(Type))
                    targeted = targeted.GetType();
                else if ((m_Type == typeof(BaseAddon) || m_Type.IsAssignableFrom(typeof(BaseAddon))) && targeted is AddonComponent)
                    targeted = ((AddonComponent)targeted).Addon;

                if (m_Type.IsAssignableFrom(targeted.GetType()))
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, targeted.ToString());
                    m_Property.SetValue(m_Object, targeted, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                else
                {
                    m_Mobile.SendMessage("That cannot be assigned to a property of type : {0}", m_Type.Name);
                }
            }
            catch
            {
                m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
            }
        }

        protected override void OnTargetFinish(Mobile from)
        {
            if (m_Type == typeof(Type))
                from.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
            else
                from.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
        }
    }

    public class SetObjectGump : Gump
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private Type m_Type;
        private int m_Page;
        private ArrayList m_List;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static readonly int EntryWidth = 212;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + (5 * (EntryHeight + OffsetSize));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        public SetObjectGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, Type type, int page, ArrayList list)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Type = type;
            m_Page = page;
            m_List = list;

            string initialText = PropertiesGump.ValueToString(o, prop);

            AddPage(0);

            AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, initialText);
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Change by Serial");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Nullify");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "View Properties");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 4, GumpButtonType.Reply, 0);
        }

        private class InternalPrompt : Prompt
        {
            private PropertyInfo m_Property;
            private Mobile m_Mobile;
            private object m_Object;
            private Stack<StackEntry> m_Stack;
            private Type m_Type;
            private int m_Page;
            private ArrayList m_List;

            public InternalPrompt(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, Type type, int page, ArrayList list)
            {
                m_Property = prop;
                m_Mobile = mobile;
                m_Object = o;
                m_Stack = stack;
                m_Type = type;
                m_Page = page;
                m_List = list;
            }

            public override void OnCancel(Mobile from)
            {
                m_Mobile.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
            }

            public override void OnResponse(Mobile from, string text)
            {
                object toSet;
                bool shouldSet;

                try
                {
                    int serial = Utility.ToInt32(text);

                    toSet = World.FindEntity(serial);

                    if (toSet == null)
                    {
                        shouldSet = false;
                        m_Mobile.SendMessage("No object with that serial was found.");
                    }
                    else if (!m_Type.IsAssignableFrom(toSet.GetType()))
                    {
                        toSet = null;
                        shouldSet = false;
                        m_Mobile.SendMessage("The object with that serial could not be assigned to a property of type : {0}", m_Type.Name);
                    }
                    else
                    {
                        shouldSet = true;
                    }
                }
                catch
                {
                    toSet = null;
                    shouldSet = false;
                    m_Mobile.SendMessage("Bad format");
                }

                if (shouldSet)
                {
                    try
                    {
                        CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet == null ? "(null)" : toSet.ToString());
                        m_Property.SetValue(m_Object, toSet, null);
                        PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                    }
                    catch
                    {
                        m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                    }
                }

                m_Mobile.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            object toSet;
            bool shouldSet, shouldSend = true;
            object viewProps = null;

            switch (info.ButtonID)
            {
                case 0: // closed
                    {
                        m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));

                        toSet = null;
                        shouldSet = false;
                        shouldSend = false;

                        break;
                    }
                case 1: // Change by Target
                    {
                        m_Mobile.Target = new SetObjectTarget(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List);
                        toSet = null;
                        shouldSet = false;
                        shouldSend = false;
                        break;
                    }
                case 2: // Change by Serial
                    {
                        toSet = null;
                        shouldSet = false;
                        shouldSend = false;

                        m_Mobile.SendMessage("Enter the serial you wish to find:");
                        m_Mobile.Prompt = new InternalPrompt(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List);

                        break;
                    }
                case 3: // Nullify
                    {
                        toSet = null;
                        shouldSet = true;

                        break;
                    }
                case 4: // View Properties
                    {
                        toSet = null;
                        shouldSet = false;

                        object obj = m_Property.GetValue(m_Object, null);

                        if (obj == null)
                            m_Mobile.SendMessage("The property is null and so you cannot view its properties.");
                        else if (!BaseCommand.IsAccessible(m_Mobile, obj))
                            m_Mobile.SendMessage("You may not view their properties.");
                        else
                            viewProps = obj;

                        break;
                    }
                default:
                    {
                        toSet = null;
                        shouldSet = false;

                        break;
                    }
            }

            if (shouldSet)
            {
                try
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet == null ? "(null)" : toSet.ToString());
                    m_Property.SetValue(m_Object, toSet, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            if (shouldSend)
                m_Mobile.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));

            if (viewProps != null)
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, viewProps));
        }
    }

    public class SetPoint2DGump : Gump
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private int m_Page;
        private ArrayList m_List;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static readonly int CoordWidth = 105;
        private static readonly int EntryWidth = CoordWidth + OffsetSize + CoordWidth;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + (4 * (EntryHeight + OffsetSize));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        public SetPoint2DGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = page;
            m_List = list;

            Point2D p = (Point2D)prop.GetValue(o, null);

            AddPage(0);

            AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Use your location");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Target a location");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "X:");
            AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 0, p.X.ToString());
            x += CoordWidth + OffsetSize;

            AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "Y:");
            AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 1, p.Y.ToString());
            x += CoordWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);
        }

        private class InternalTarget : Target
        {
            private PropertyInfo m_Property;
            private Mobile m_Mobile;
            private object m_Object;
            private Stack<StackEntry> m_Stack;
            private int m_Page;
            private ArrayList m_List;

            public InternalTarget(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
                : base(-1, true, TargetFlags.None)
            {
                m_Property = prop;
                m_Mobile = mobile;
                m_Object = o;
                m_Stack = stack;
                m_Page = page;
                m_List = list;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;

                if (p != null)
                {
                    try
                    {
                        CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, new Point2D(p).ToString());
                        m_Property.SetValue(m_Object, new Point2D(p), null);
                        PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                    }
                    catch
                    {
                        m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                    }
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Point2D toSet;
            bool shouldSet, shouldSend;

            switch (info.ButtonID)
            {
                case 1: // Current location
                    {
                        toSet = new Point2D(m_Mobile.Location);
                        shouldSet = true;
                        shouldSend = true;

                        break;
                    }
                case 2: // Pick location
                    {
                        m_Mobile.Target = new InternalTarget(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List);

                        toSet = Point2D.Zero;
                        shouldSet = false;
                        shouldSend = false;

                        break;
                    }
                case 3: // Use values
                    {
                        TextRelay x = info.GetTextEntry(0);
                        TextRelay y = info.GetTextEntry(1);

                        toSet = new Point2D(x == null ? 0 : Utility.ToInt32(x.Text), y == null ? 0 : Utility.ToInt32(y.Text));
                        shouldSet = true;
                        shouldSend = true;

                        break;
                    }
                default:
                    {
                        toSet = Point2D.Zero;
                        shouldSet = false;
                        shouldSend = true;

                        break;
                    }
            }

            if (shouldSet)
            {
                try
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());
                    m_Property.SetValue(m_Object, toSet, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            if (shouldSend)
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }

    public class SetPoint3DGump : Gump
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private int m_Page;
        private ArrayList m_List;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static readonly int CoordWidth = 70;
        private static readonly int EntryWidth = CoordWidth + OffsetSize + CoordWidth + OffsetSize + CoordWidth;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + (4 * (EntryHeight + OffsetSize));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        public SetPoint3DGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = page;
            m_List = list;

            Point3D p = (Point3D)prop.GetValue(o, null);

            AddPage(0);

            AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Use your location");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Target a location");
            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);

            x = BorderSize + OffsetSize;
            y += EntryHeight + OffsetSize;

            AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "X:");
            AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 0, p.X.ToString());
            x += CoordWidth + OffsetSize;

            AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "Y:");
            AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 1, p.Y.ToString());
            x += CoordWidth + OffsetSize;

            AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "Z:");
            AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 2, p.Z.ToString());
            x += CoordWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
            AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);
        }

        private class InternalTarget : Target
        {
            private PropertyInfo m_Property;
            private Mobile m_Mobile;
            private object m_Object;
            private Stack<StackEntry> m_Stack;
            private int m_Page;
            private ArrayList m_List;

            public InternalTarget(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
                : base(-1, true, TargetFlags.None)
            {
                m_Property = prop;
                m_Mobile = mobile;
                m_Object = o;
                m_Stack = stack;
                m_Page = page;
                m_List = list;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;

                if (p != null)
                {
                    try
                    {
                        CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, new Point3D(p).ToString());
                        m_Property.SetValue(m_Object, new Point3D(p), null);
                        PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                    }
                    catch
                    {
                        m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                    }
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Point3D toSet;
            bool shouldSet, shouldSend;

            switch (info.ButtonID)
            {
                case 1: // Current location
                    {
                        toSet = m_Mobile.Location;
                        shouldSet = true;
                        shouldSend = true;

                        break;
                    }
                case 2: // Pick location
                    {
                        m_Mobile.Target = new InternalTarget(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List);

                        toSet = Point3D.Zero;
                        shouldSet = false;
                        shouldSend = false;

                        break;
                    }
                case 3: // Use values
                    {
                        TextRelay x = info.GetTextEntry(0);
                        TextRelay y = info.GetTextEntry(1);
                        TextRelay z = info.GetTextEntry(2);

                        toSet = new Point3D(x == null ? 0 : Utility.ToInt32(x.Text), y == null ? 0 : Utility.ToInt32(y.Text), z == null ? 0 : Utility.ToInt32(z.Text));
                        shouldSet = true;
                        shouldSend = true;

                        break;
                    }
                default:
                    {
                        toSet = Point3D.Zero;
                        shouldSet = false;
                        shouldSend = true;

                        break;
                    }
            }

            if (shouldSet)
            {
                try
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());
                    m_Property.SetValue(m_Object, toSet, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            if (shouldSend)
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }

    public class SetTimeSpanGump : Gump
    {
        private PropertyInfo m_Property;
        private Mobile m_Mobile;
        private object m_Object;
        private Stack<StackEntry> m_Stack;
        private int m_Page;
        private ArrayList m_List;

        public static readonly bool OldStyle = PropsConfig.OldStyle;

        public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
        public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

        public static readonly int TextHue = PropsConfig.TextHue;
        public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

        public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
        public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
        public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
        public static readonly int BackGumpID = PropsConfig.BackGumpID;
        public static readonly int SetGumpID = PropsConfig.SetGumpID;

        public static readonly int SetWidth = PropsConfig.SetWidth;
        public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
        public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

        public static readonly int PrevWidth = PropsConfig.PrevWidth;
        public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public static readonly int NextWidth = PropsConfig.NextWidth;
        public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
        public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

        public static readonly int OffsetSize = PropsConfig.OffsetSize;

        public static readonly int EntryHeight = PropsConfig.EntryHeight;
        public static readonly int BorderSize = PropsConfig.BorderSize;

        private static readonly int EntryWidth = 212;

        private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
        private static readonly int TotalHeight = OffsetSize + (7 * (EntryHeight + OffsetSize));

        private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
        private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

        public SetTimeSpanGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = page;
            m_List = list;

            TimeSpan ts = (TimeSpan)prop.GetValue(o, null);

            AddPage(0);

            AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
            AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

            AddRect(0, prop.Name, 0, -1);
            AddRect(1, ts.ToString(), 0, -1);
            AddRect(2, "Zero", 1, -1);
            AddRect(3, "From H:M:S", 2, -1);
            AddRect(4, "H:", 3, 0);
            AddRect(5, "M:", 4, 1);
            AddRect(6, "S:", 5, 2);
        }

        private void AddRect(int index, string str, int button, int text)
        {
            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize + (index * (EntryHeight + OffsetSize));

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

            if (text != -1)
                AddTextEntry(x + 16 + TextOffsetX, y, EntryWidth - TextOffsetX - 16, EntryHeight, TextHue, text, "");

            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            if (button != 0)
                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            TimeSpan toSet;
            bool shouldSet, shouldSend;

            TextRelay h = info.GetTextEntry(0);
            TextRelay m = info.GetTextEntry(1);
            TextRelay s = info.GetTextEntry(2);

            switch (info.ButtonID)
            {
                case 1: // Zero
                    {
                        toSet = TimeSpan.Zero;
                        shouldSet = true;
                        shouldSend = true;

                        break;
                    }
                case 2: // From H:M:S
                    {
                        bool successfulParse = false;
                        if (h != null && m != null && s != null)
                        {
                            successfulParse = TimeSpan.TryParse(h.Text + ":" + m.Text + ":" + s.Text, out toSet);
                        }
                        else
                        {
                            toSet = TimeSpan.Zero;
                        }

                        shouldSet = shouldSend = successfulParse;

                        break;
                    }
                case 3: // From H
                    {
                        if (h != null)
                        {
                            try
                            {
                                toSet = TimeSpan.FromHours(Utility.ToDouble(h.Text));
                                shouldSet = true;
                                shouldSend = true;

                                break;
                            }
                            catch
                            {
                            }
                        }

                        toSet = TimeSpan.Zero;
                        shouldSet = false;
                        shouldSend = false;

                        break;
                    }
                case 4: // From M
                    {
                        if (m != null)
                        {
                            try
                            {
                                toSet = TimeSpan.FromMinutes(Utility.ToDouble(m.Text));
                                shouldSet = true;
                                shouldSend = true;

                                break;
                            }
                            catch
                            {
                            }
                        }

                        toSet = TimeSpan.Zero;
                        shouldSet = false;
                        shouldSend = false;

                        break;
                    }
                case 5: // From S
                    {
                        if (s != null)
                        {
                            try
                            {
                                toSet = TimeSpan.FromSeconds(Utility.ToDouble(s.Text));
                                shouldSet = true;
                                shouldSend = true;

                                break;
                            }
                            catch
                            {
                            }
                        }

                        toSet = TimeSpan.Zero;
                        shouldSet = false;
                        shouldSend = false;

                        break;
                    }
                default:
                    {
                        toSet = TimeSpan.Zero;
                        shouldSet = false;
                        shouldSend = true;

                        break;
                    }
            }

            if (shouldSet)
            {
                try
                {
                    CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());
                    m_Property.SetValue(m_Object, toSet, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            if (shouldSend)
                m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }
}