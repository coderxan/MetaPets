using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Server;

namespace Server.Commands.Generic
{
    /// <summary>
    /// Conditional
    /// </summary>
    public interface IConditional
    {
        bool Verify(object obj);
    }

    public interface ICondition
    {
        // Invoked during the constructor
        void Construct(TypeBuilder typeBuilder, ILGenerator il, int index);

        // Target object will be loaded on the stack
        void Compile(MethodEmitter emitter);
    }

    public sealed class TypeCondition : ICondition
    {
        public static TypeCondition Default = new TypeCondition();

        void ICondition.Construct(TypeBuilder typeBuilder, ILGenerator il, int index)
        {
        }

        void ICondition.Compile(MethodEmitter emitter)
        {
            // The object was safely cast to be the conditionals type
            // If it's null, then the type cast didn't work...

            emitter.LoadNull();
            emitter.Compare(OpCodes.Ceq);
            emitter.LogicalNot();
        }
    }

    public sealed class PropertyValue
    {
        private Type m_Type;
        private object m_Value;
        private FieldInfo m_Field;

        public Type Type
        {
            get { return m_Type; }
        }

        public object Value
        {
            get { return m_Value; }
        }

        public FieldInfo Field
        {
            get { return m_Field; }
        }

        public bool HasField
        {
            get { return (m_Field != null); }
        }

        public PropertyValue(Type type, object value)
        {
            m_Type = type;
            m_Value = value;
        }

        public void Load(MethodEmitter method)
        {
            if (m_Field != null)
            {
                method.LoadArgument(0);
                method.LoadField(m_Field);
            }
            else if (m_Value == null)
            {
                method.LoadNull(m_Type);
            }
            else
            {
                if (m_Value is int)
                    method.Load((int)m_Value);
                else if (m_Value is long)
                    method.Load((long)m_Value);
                else if (m_Value is float)
                    method.Load((float)m_Value);
                else if (m_Value is double)
                    method.Load((double)m_Value);
                else if (m_Value is char)
                    method.Load((char)m_Value);
                else if (m_Value is bool)
                    method.Load((bool)m_Value);
                else if (m_Value is string)
                    method.Load((string)m_Value);
                else if (m_Value is Enum)
                    method.Load((Enum)m_Value);
                else
                    throw new InvalidOperationException("Unrecognized comparison value.");
            }
        }

        public void Acquire(TypeBuilder typeBuilder, ILGenerator il, string fieldName)
        {
            if (m_Value is string)
            {
                string toParse = (string)m_Value;

                if (!m_Type.IsValueType && toParse == "null")
                {
                    m_Value = null;
                }
                else if (m_Type == typeof(string))
                {
                    if (toParse == @"@""null""")
                        toParse = "null";

                    m_Value = toParse;
                }
                else if (m_Type.IsEnum)
                {
                    m_Value = Enum.Parse(m_Type, toParse, true);
                }
                else
                {
                    MethodInfo parseMethod = null;
                    object[] parseArgs = null;

                    MethodInfo parseNumber = m_Type.GetMethod(
                        "Parse",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new Type[] { typeof(string), typeof(NumberStyles) },
                        null
                    );

                    if (parseNumber != null)
                    {
                        NumberStyles style = NumberStyles.Integer;

                        if (Insensitive.StartsWith(toParse, "0x"))
                        {
                            style = NumberStyles.HexNumber;
                            toParse = toParse.Substring(2);
                        }

                        parseMethod = parseNumber;
                        parseArgs = new object[] { toParse, style };
                    }
                    else
                    {
                        MethodInfo parseGeneral = m_Type.GetMethod(
                            "Parse",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            new Type[] { typeof(string) },
                            null
                        );

                        parseMethod = parseGeneral;
                        parseArgs = new object[] { toParse };
                    }

                    if (parseMethod != null)
                    {
                        m_Value = parseMethod.Invoke(null, parseArgs);

                        if (!m_Type.IsPrimitive)
                        {
                            m_Field = typeBuilder.DefineField(
                                fieldName,
                                m_Type,
                                FieldAttributes.Private | FieldAttributes.InitOnly
                            );

                            il.Emit(OpCodes.Ldarg_0);

                            il.Emit(OpCodes.Ldstr, toParse);

                            if (parseArgs.Length == 2) // dirty evil hack :-(
                                il.Emit(OpCodes.Ldc_I4, (int)parseArgs[1]);

                            il.Emit(OpCodes.Call, parseMethod);
                            il.Emit(OpCodes.Stfld, m_Field);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                "Unable to convert string \"{0}\" into type '{1}'.",
                                m_Value,
                                m_Type
                            )
                        );
                    }
                }
            }
        }
    }

    public abstract class PropertyCondition : ICondition
    {
        protected Property m_Property;
        protected bool m_Not;

        public PropertyCondition(Property property, bool not)
        {
            m_Property = property;
            m_Not = not;
        }

        public abstract void Construct(TypeBuilder typeBuilder, ILGenerator il, int index);

        public abstract void Compile(MethodEmitter emitter);
    }

    public enum StringOperator
    {
        Equal,
        NotEqual,

        Contains,

        StartsWith,
        EndsWith
    }

    public sealed class StringCondition : PropertyCondition
    {
        private StringOperator m_Operator;
        private PropertyValue m_Value;

        private bool m_IgnoreCase;

        public StringCondition(Property property, bool not, StringOperator op, object value, bool ignoreCase)
            : base(property, not)
        {
            m_Operator = op;
            m_Value = new PropertyValue(property.Type, value);

            m_IgnoreCase = ignoreCase;
        }

        public override void Construct(TypeBuilder typeBuilder, ILGenerator il, int index)
        {
            m_Value.Acquire(typeBuilder, il, "v" + index);
        }

        public override void Compile(MethodEmitter emitter)
        {
            bool inverse = false;

            string methodName;

            switch (m_Operator)
            {
                case StringOperator.Equal:
                    methodName = "Equals";
                    break;

                case StringOperator.NotEqual:
                    methodName = "Equals";
                    inverse = true;
                    break;

                case StringOperator.Contains:
                    methodName = "Contains";
                    break;

                case StringOperator.StartsWith:
                    methodName = "StartsWith";
                    break;

                case StringOperator.EndsWith:
                    methodName = "EndsWith";
                    break;

                default:
                    throw new InvalidOperationException("Invalid string comparison operator.");
            }

            if (m_IgnoreCase || methodName == "Equals")
            {
                Type type = (m_IgnoreCase ? typeof(Insensitive) : typeof(String));

                emitter.BeginCall(
                    type.GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new Type[]
						{
							typeof( string ),
							typeof( string )
						},
                        null
                    )
                );

                emitter.Chain(m_Property);
                m_Value.Load(emitter);

                emitter.FinishCall();
            }
            else
            {
                Label notNull = emitter.CreateLabel();
                Label moveOn = emitter.CreateLabel();

                LocalBuilder temp = emitter.AcquireTemp(m_Property.Type);

                emitter.Chain(m_Property);

                emitter.StoreLocal(temp);
                emitter.LoadLocal(temp);

                emitter.BranchIfTrue(notNull);

                emitter.Load(false);
                emitter.Pop();
                emitter.Branch(moveOn);

                emitter.MarkLabel(notNull);
                emitter.LoadLocal(temp);

                emitter.BeginCall(
                    typeof(string).GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new Type[]
						{
							typeof( string )
						},
                        null
                    )
                );

                m_Value.Load(emitter);

                emitter.FinishCall();

                emitter.MarkLabel(moveOn);
            }

            if (m_Not != inverse)
                emitter.LogicalNot();
        }
    }

    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Lesser,
        LesserEqual
    }

    public sealed class ComparisonCondition : PropertyCondition
    {
        private ComparisonOperator m_Operator;
        private PropertyValue m_Value;

        public ComparisonCondition(Property property, bool not, ComparisonOperator op, object value)
            : base(property, not)
        {
            m_Operator = op;
            m_Value = new PropertyValue(property.Type, value);
        }

        public override void Construct(TypeBuilder typeBuilder, ILGenerator il, int index)
        {
            m_Value.Acquire(typeBuilder, il, "v" + index);
        }

        public override void Compile(MethodEmitter emitter)
        {
            emitter.Chain(m_Property);

            bool inverse = false;

            bool couldCompare =
            emitter.CompareTo(1, delegate()
            {
                m_Value.Load(emitter);
            });

            if (couldCompare)
            {
                emitter.Load(0);

                switch (m_Operator)
                {
                    case ComparisonOperator.Equal:
                        emitter.Compare(OpCodes.Ceq);
                        break;

                    case ComparisonOperator.NotEqual:
                        emitter.Compare(OpCodes.Ceq);
                        inverse = true;
                        break;

                    case ComparisonOperator.Greater:
                        emitter.Compare(OpCodes.Cgt);
                        break;

                    case ComparisonOperator.GreaterEqual:
                        emitter.Compare(OpCodes.Clt);
                        inverse = true;
                        break;

                    case ComparisonOperator.Lesser:
                        emitter.Compare(OpCodes.Clt);
                        break;

                    case ComparisonOperator.LesserEqual:
                        emitter.Compare(OpCodes.Cgt);
                        inverse = true;
                        break;

                    default:
                        throw new InvalidOperationException("Invalid comparison operator.");
                }
            }
            else
            {
                // This type is -not- comparable
                // We can only support == and != operations

                m_Value.Load(emitter);

                switch (m_Operator)
                {
                    case ComparisonOperator.Equal:
                        emitter.Compare(OpCodes.Ceq);
                        break;

                    case ComparisonOperator.NotEqual:
                        emitter.Compare(OpCodes.Ceq);
                        inverse = true;
                        break;

                    case ComparisonOperator.Greater:
                    case ComparisonOperator.GreaterEqual:
                    case ComparisonOperator.Lesser:
                    case ComparisonOperator.LesserEqual:
                        throw new InvalidOperationException("Property does not support relational comparisons.");

                    default:
                        throw new InvalidOperationException("Invalid operator.");
                }
            }

            if (m_Not != inverse)
                emitter.LogicalNot();
        }
    }

    public static class ConditionalCompiler
    {
        public static IConditional Compile(AssemblyEmitter assembly, Type objectType, ICondition[] conditions, int index)
        {
            TypeBuilder typeBuilder = assembly.DefineType(
                    "__conditional" + index,
                    TypeAttributes.Public,
                    typeof(object)
                );

            #region Constructor
            {
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        Type.EmptyTypes
                    );

                ILGenerator il = ctor.GetILGenerator();

                // : base()
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

                for (int i = 0; i < conditions.Length; ++i)
                    conditions[i].Construct(typeBuilder, il, i);

                // return;
                il.Emit(OpCodes.Ret);
            }
            #endregion

            #region IComparer
            typeBuilder.AddInterfaceImplementation(typeof(IConditional));

            MethodBuilder compareMethod;

            #region Compare
            {
                MethodEmitter emitter = new MethodEmitter(typeBuilder);

                emitter.Define(
                    /*  name  */ "Verify",
                    /*  attr  */ MethodAttributes.Public | MethodAttributes.Virtual,
                    /* return */ typeof(bool),
                    /* params */ new Type[] { typeof(object) });

                LocalBuilder obj = emitter.CreateLocal(objectType);
                LocalBuilder eq = emitter.CreateLocal(typeof(bool));

                emitter.LoadArgument(1);
                emitter.CastAs(objectType);
                emitter.StoreLocal(obj);

                Label done = emitter.CreateLabel();

                for (int i = 0; i < conditions.Length; ++i)
                {
                    if (i > 0)
                    {
                        emitter.LoadLocal(eq);

                        emitter.BranchIfFalse(done);
                    }

                    emitter.LoadLocal(obj);

                    conditions[i].Compile(emitter);

                    emitter.StoreLocal(eq);
                }

                emitter.MarkLabel(done);

                emitter.LoadLocal(eq);

                emitter.Return();

                typeBuilder.DefineMethodOverride(
                        emitter.Method,
                        typeof(IConditional).GetMethod(
                            "Verify",
                            new Type[]
								{
									typeof( object )
								}
                        )
                    );

                compareMethod = emitter.Method;
            }
            #endregion
            #endregion

            Type conditionalType = typeBuilder.CreateType();

            return (IConditional)Activator.CreateInstance(conditionalType);
        }
    }

    /// <summary>
    /// Sort
    /// </summary>
    public sealed class OrderInfo
    {
        private Property m_Property;
        private int m_Order;

        public Property Property
        {
            get { return m_Property; }
            set { m_Property = value; }
        }

        public bool IsAscending
        {
            get { return (m_Order > 0); }
            set { m_Order = (value ? +1 : -1); }
        }

        public bool IsDescending
        {
            get { return (m_Order < 0); }
            set { m_Order = (value ? -1 : +1); }
        }

        public int Sign
        {
            get { return Math.Sign(m_Order); }
            set
            {
                m_Order = Math.Sign(value);

                if (m_Order == 0)
                    throw new InvalidOperationException("Sign cannot be zero.");
            }
        }

        public OrderInfo(Property property, bool isAscending)
        {
            m_Property = property;

            this.IsAscending = isAscending;
        }
    }

    public static class SortCompiler
    {
        public static IComparer Compile(AssemblyEmitter assembly, Type objectType, OrderInfo[] orders)
        {
            TypeBuilder typeBuilder = assembly.DefineType(
                    "__sort",
                    TypeAttributes.Public,
                    typeof(object)
                );

            #region Constructor
            {
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        Type.EmptyTypes
                    );

                ILGenerator il = ctor.GetILGenerator();

                // : base()
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

                // return;
                il.Emit(OpCodes.Ret);
            }
            #endregion

            #region IComparer
            typeBuilder.AddInterfaceImplementation(typeof(IComparer));

            MethodBuilder compareMethod;

            #region Compare
            {
                MethodEmitter emitter = new MethodEmitter(typeBuilder);

                emitter.Define(
                    /*  name  */ "Compare",
                    /*  attr  */ MethodAttributes.Public | MethodAttributes.Virtual,
                    /* return */ typeof(int),
                    /* params */ new Type[] { typeof(object), typeof(object) });

                LocalBuilder a = emitter.CreateLocal(objectType);
                LocalBuilder b = emitter.CreateLocal(objectType);

                LocalBuilder v = emitter.CreateLocal(typeof(int));

                emitter.LoadArgument(1);
                emitter.CastAs(objectType);
                emitter.StoreLocal(a);

                emitter.LoadArgument(2);
                emitter.CastAs(objectType);
                emitter.StoreLocal(b);

                emitter.Load(0);
                emitter.StoreLocal(v);

                Label end = emitter.CreateLabel();

                for (int i = 0; i < orders.Length; ++i)
                {
                    if (i > 0)
                    {
                        emitter.LoadLocal(v);
                        emitter.BranchIfTrue(end); // if ( v != 0 ) return v;
                    }

                    OrderInfo orderInfo = orders[i];

                    Property prop = orderInfo.Property;
                    int sign = orderInfo.Sign;

                    emitter.LoadLocal(a);
                    emitter.Chain(prop);

                    bool couldCompare =
                    emitter.CompareTo(sign, delegate()
                    {
                        emitter.LoadLocal(b);
                        emitter.Chain(prop);
                    });

                    if (!couldCompare)
                        throw new InvalidOperationException("Property is not comparable.");

                    emitter.StoreLocal(v);
                }

                emitter.MarkLabel(end);

                emitter.LoadLocal(v);
                emitter.Return();

                typeBuilder.DefineMethodOverride(
                        emitter.Method,
                        typeof(IComparer).GetMethod(
                            "Compare",
                            new Type[]
								{
									typeof( object ),
									typeof( object )
								}
                        )
                    );

                compareMethod = emitter.Method;
            }
            #endregion
            #endregion

            Type comparerType = typeBuilder.CreateType();

            return (IComparer)Activator.CreateInstance(comparerType);
        }
    }

    /// <summary>
    /// Distinict
    /// </summary>
    public static class DistinctCompiler
    {
        public static IComparer Compile(AssemblyEmitter assembly, Type objectType, Property[] props)
        {
            TypeBuilder typeBuilder = assembly.DefineType(
                    "__distinct",
                    TypeAttributes.Public,
                    typeof(object)
                );

            #region Constructor
            {
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        Type.EmptyTypes
                    );

                ILGenerator il = ctor.GetILGenerator();

                // : base()
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

                // return;
                il.Emit(OpCodes.Ret);
            }
            #endregion

            #region IComparer
            typeBuilder.AddInterfaceImplementation(typeof(IComparer));

            MethodBuilder compareMethod;

            #region Compare
            {
                MethodEmitter emitter = new MethodEmitter(typeBuilder);

                emitter.Define(
                    /*  name  */ "Compare",
                    /*  attr  */ MethodAttributes.Public | MethodAttributes.Virtual,
                    /* return */ typeof(int),
                    /* params */ new Type[] { typeof(object), typeof(object) });

                LocalBuilder a = emitter.CreateLocal(objectType);
                LocalBuilder b = emitter.CreateLocal(objectType);

                LocalBuilder v = emitter.CreateLocal(typeof(int));

                emitter.LoadArgument(1);
                emitter.CastAs(objectType);
                emitter.StoreLocal(a);

                emitter.LoadArgument(2);
                emitter.CastAs(objectType);
                emitter.StoreLocal(b);

                emitter.Load(0);
                emitter.StoreLocal(v);

                Label end = emitter.CreateLabel();

                for (int i = 0; i < props.Length; ++i)
                {
                    if (i > 0)
                    {
                        emitter.LoadLocal(v);
                        emitter.BranchIfTrue(end); // if ( v != 0 ) return v;
                    }

                    Property prop = props[i];

                    emitter.LoadLocal(a);
                    emitter.Chain(prop);

                    bool couldCompare =
                    emitter.CompareTo(1, delegate()
                    {
                        emitter.LoadLocal(b);
                        emitter.Chain(prop);
                    });

                    if (!couldCompare)
                        throw new InvalidOperationException("Property is not comparable.");

                    emitter.StoreLocal(v);
                }

                emitter.MarkLabel(end);

                emitter.LoadLocal(v);
                emitter.Return();

                typeBuilder.DefineMethodOverride(
                        emitter.Method,
                        typeof(IComparer).GetMethod(
                            "Compare",
                            new Type[]
								{
									typeof( object ),
									typeof( object )
								}
                        )
                    );

                compareMethod = emitter.Method;
            }
            #endregion
            #endregion

            #region IEqualityComparer
            typeBuilder.AddInterfaceImplementation(typeof(IEqualityComparer<object>));

            #region Equals
            {
                MethodEmitter emitter = new MethodEmitter(typeBuilder);

                emitter.Define(
                    /*  name  */ "Equals",
                    /*  attr  */ MethodAttributes.Public | MethodAttributes.Virtual,
                    /* return */ typeof(bool),
                    /* params */ new Type[] { typeof(object), typeof(object) });

                emitter.Generator.Emit(OpCodes.Ldarg_0);
                emitter.Generator.Emit(OpCodes.Ldarg_1);
                emitter.Generator.Emit(OpCodes.Ldarg_2);

                emitter.Generator.Emit(OpCodes.Call, compareMethod);

                emitter.Generator.Emit(OpCodes.Ldc_I4_0);

                emitter.Generator.Emit(OpCodes.Ceq);

                emitter.Generator.Emit(OpCodes.Ret);

                typeBuilder.DefineMethodOverride(
                        emitter.Method,
                        typeof(IEqualityComparer<object>).GetMethod(
                            "Equals",
                            new Type[]
							{
								typeof( object ),
								typeof( object )
							}
                        )
                    );
            }
            #endregion

            #region GetHashCode
            {
                MethodEmitter emitter = new MethodEmitter(typeBuilder);

                emitter.Define(
                    /*  name  */ "GetHashCode",
                    /*  attr  */ MethodAttributes.Public | MethodAttributes.Virtual,
                    /* return */ typeof(int),
                    /* params */ new Type[] { typeof(object) });

                LocalBuilder obj = emitter.CreateLocal(objectType);

                emitter.LoadArgument(1);
                emitter.CastAs(objectType);
                emitter.StoreLocal(obj);

                for (int i = 0; i < props.Length; ++i)
                {
                    Property prop = props[i];

                    emitter.LoadLocal(obj);
                    emitter.Chain(prop);

                    Type active = emitter.Active;

                    MethodInfo getHashCode = active.GetMethod("GetHashCode", Type.EmptyTypes);

                    if (getHashCode == null)
                        getHashCode = typeof(object).GetMethod("GetHashCode", Type.EmptyTypes);

                    if (active != typeof(int))
                    {
                        if (!active.IsValueType)
                        {
                            LocalBuilder value = emitter.AcquireTemp(active);

                            Label valueNotNull = emitter.CreateLabel();
                            Label done = emitter.CreateLabel();

                            emitter.StoreLocal(value);
                            emitter.LoadLocal(value);

                            emitter.BranchIfTrue(valueNotNull);

                            emitter.Load(0);
                            emitter.Pop(typeof(int));

                            emitter.Branch(done);

                            emitter.MarkLabel(valueNotNull);

                            emitter.LoadLocal(value);
                            emitter.Call(getHashCode);

                            emitter.ReleaseTemp(value);

                            emitter.MarkLabel(done);
                        }
                        else
                        {
                            emitter.Call(getHashCode);
                        }
                    }

                    if (i > 0)
                        emitter.Xor();
                }

                emitter.Return();

                typeBuilder.DefineMethodOverride(
                        emitter.Method,
                        typeof(IEqualityComparer<object>).GetMethod(
                            "GetHashCode",
                            new Type[]
							{
								typeof( object )
							}
                        )
                    );
            }
            #endregion
            #endregion

            Type comparerType = typeBuilder.CreateType();

            return (IComparer)Activator.CreateInstance(comparerType);
        }
    }
}