using System;
using System.Collections.Generic;
using System.Reflection;

using Server;
using Server.Commands;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
    public class FlipCommandHandlers
    {
        public static void Initialize()
        {
            CommandSystem.Register("Flip", AccessLevel.GameMaster, new CommandEventHandler(Flip_OnCommand));
        }

        [Usage("Flip")]
        [Description("Turns an item.")]
        public static void Flip_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new FlipTarget();
        }

        private class FlipTarget : Target
        {
            public FlipTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item)
                {
                    Item item = (Item)targeted;

                    if (item.Movable == false && from.AccessLevel == AccessLevel.Player)
                        return;

                    Type type = targeted.GetType();

                    FlipableAttribute[] AttributeArray = (FlipableAttribute[])type.GetCustomAttributes(typeof(FlipableAttribute), false);

                    if (AttributeArray.Length == 0)
                    {
                        return;
                    }

                    FlipableAttribute fa = AttributeArray[0];

                    fa.Flip((Item)targeted);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DynamicFlipingAttribute : Attribute
    {
        public DynamicFlipingAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FlipableAttribute : Attribute
    {
        private int[] m_ItemIDs;

        public int[] ItemIDs
        {
            get { return m_ItemIDs; }
        }

        public FlipableAttribute()
            : this(null)
        {
        }

        public FlipableAttribute(params int[] itemIDs)
        {
            m_ItemIDs = itemIDs;
        }

        public virtual void Flip(Item item)
        {
            if (m_ItemIDs == null)
            {
                try
                {
                    MethodInfo flipMethod = item.GetType().GetMethod("Flip", Type.EmptyTypes);
                    if (flipMethod != null)
                        flipMethod.Invoke(item, new object[0]);
                }
                catch
                {
                }

            }
            else
            {
                int index = 0;
                for (int i = 0; i < m_ItemIDs.Length; i++)
                {
                    if (item.ItemID == m_ItemIDs[i])
                    {
                        index = i + 1;
                        break;
                    }
                }

                if (index > m_ItemIDs.Length - 1)
                    index = 0;

                item.ItemID = m_ItemIDs[index];
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FlipableAddonAttribute : Attribute
    {
        private static string m_MethodName = "Flip";

        private static Type[] m_Params = new Type[]
		{
			typeof( Mobile ), typeof( Direction )
		};

        private Direction[] m_Directions;

        public Direction[] Directions
        {
            get { return m_Directions; }
        }

        public FlipableAddonAttribute(params Direction[] directions)
        {
            m_Directions = directions;
        }

        public virtual void Flip(Mobile from, Item addon)
        {
            if (m_Directions != null && m_Directions.Length > 1)
            {
                try
                {
                    MethodInfo flipMethod = addon.GetType().GetMethod(m_MethodName, m_Params);

                    if (flipMethod != null)
                    {
                        int index = 0;

                        for (int i = 0; i < m_Directions.Length; i++)
                        {
                            if (addon.Direction == m_Directions[i])
                            {
                                index = i + 1;
                                break;
                            }
                        }

                        if (index >= m_Directions.Length)
                            index = 0;

                        ClearComponents(addon);

                        flipMethod.Invoke(addon, new object[2] { from, m_Directions[index] });

                        BaseHouse house = null;
                        AddonFitResult result = AddonFitResult.Valid;

                        addon.Map = Map.Internal;

                        if (addon is BaseAddon)
                            result = ((BaseAddon)addon).CouldFit(addon.Location, from.Map, from, ref house);
                        else if (addon is BaseAddonContainer)
                            result = ((BaseAddonContainer)addon).CouldFit(addon.Location, from.Map, from, ref house);

                        addon.Map = from.Map;

                        if (result != AddonFitResult.Valid)
                        {
                            if (index == 0)
                                index = m_Directions.Length - 1;
                            else
                                index -= 1;

                            ClearComponents(addon);

                            flipMethod.Invoke(addon, new object[2] { from, m_Directions[index] });

                            if (result == AddonFitResult.Blocked)
                                from.SendLocalizedMessage(500269); // You cannot build that there.
                            else if (result == AddonFitResult.NotInHouse)
                                from.SendLocalizedMessage(500274); // You can only place this in a house that you own!
                            else if (result == AddonFitResult.DoorsNotClosed)
                                from.SendMessage("You must close all house doors before placing this.");
                            else if (result == AddonFitResult.DoorTooClose)
                                from.SendLocalizedMessage(500271); // You cannot build near the door.
                            else if (result == AddonFitResult.NoWall)
                                from.SendLocalizedMessage(500268); // This object needs to be mounted on something.
                        }

                        addon.Direction = m_Directions[index];
                    }
                }
                catch
                {
                }
            }
        }

        private void ClearComponents(Item item)
        {
            if (item is BaseAddon)
            {
                BaseAddon addon = (BaseAddon)item;

                foreach (AddonComponent c in addon.Components)
                {
                    c.Addon = null;
                    c.Delete();
                }

                addon.Components.Clear();
            }
            else if (item is BaseAddonContainer)
            {
                BaseAddonContainer addon = (BaseAddonContainer)item;

                foreach (AddonContainerComponent c in addon.Components)
                {
                    c.Addon = null;
                    c.Delete();
                }

                addon.Components.Clear();
            }
        }
    }
}