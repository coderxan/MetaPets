using System;

using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private Type[][] m_Types;

        public OppositionGroup(Type[][] types)
        {
            m_Types = types;
        }

        public bool IsEnemy(object from, object target)
        {
            int fromGroup = IndexOf(from);
            int targGroup = IndexOf(target);

            return (fromGroup != -1 && targGroup != -1 && fromGroup != targGroup);
        }

        public int IndexOf(object obj)
        {
            if (obj == null)
                return -1;

            Type type = obj.GetType();

            for (int i = 0; i < m_Types.Length; ++i)
            {
                Type[] group = m_Types[i];

                bool contains = false;

                for (int j = 0; !contains && j < group.Length; ++j)
                    contains = group[j].IsAssignableFrom(type);

                if (contains)
                    return i;
            }

            return -1;
        }
    }
}