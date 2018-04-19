using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Server;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using Mat = Server.Engines.BulkOrders.BulkMaterialType;

namespace Server.Engines.BulkOrders
{
    public enum BulkMaterialType
    {
        None,
        DullCopper,
        ShadowIron,
        Copper,
        Bronze,
        Gold,
        Agapite,
        Verite,
        Valorite,
        Spined,
        Horned,
        Barbed
    }

    public enum BulkGenericType
    {
        Iron,
        Cloth,
        Leather
    }

    public class BGTClassifier
    {
        public static BulkGenericType Classify(BODType deedType, Type itemType)
        {
            if (deedType == BODType.Tailor)
            {
                if (itemType == null || itemType.IsSubclassOf(typeof(BaseArmor)) || itemType.IsSubclassOf(typeof(BaseShoes)))
                    return BulkGenericType.Leather;

                return BulkGenericType.Cloth;
            }

            return BulkGenericType.Iron;
        }
    }

    #region Small Bulk Orders

    [TypeAlias("Scripts.Engines.BulkOrders.SmallBOD")]
    public abstract class SmallBOD : Item
    {
        private int m_AmountCur, m_AmountMax;
        private Type m_Type;
        private int m_Number;
        private int m_Graphic;
        private bool m_RequireExceptional;
        private BulkMaterialType m_Material;

        [CommandProperty(AccessLevel.GameMaster)]
        public int AmountCur { get { return m_AmountCur; } set { m_AmountCur = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AmountMax { get { return m_AmountMax; } set { m_AmountMax = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Type Type { get { return m_Type; } set { m_Type = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Number { get { return m_Number; } set { m_Number = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Graphic { get { return m_Graphic; } set { m_Graphic = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RequireExceptional { get { return m_RequireExceptional; } set { m_RequireExceptional = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public BulkMaterialType Material { get { return m_Material; } set { m_Material = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Complete { get { return (m_AmountCur == m_AmountMax); } }

        public override int LabelNumber { get { return 1045151; } } // a bulk order deed

        [Constructable]
        public SmallBOD(int hue, int amountMax, Type type, int number, int graphic, bool requireExeptional, BulkMaterialType material)
            : base(Core.AOS ? 0x2258 : 0x14EF)
        {
            Weight = 1.0;
            Hue = hue; // Blacksmith: 0x44E; Tailoring: 0x483
            LootType = LootType.Blessed;

            m_AmountMax = amountMax;
            m_Type = type;
            m_Number = number;
            m_Graphic = graphic;
            m_RequireExceptional = requireExeptional;
            m_Material = material;
        }

        public SmallBOD()
            : base(Core.AOS ? 0x2258 : 0x14EF)
        {
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public static BulkMaterialType GetRandomMaterial(BulkMaterialType start, double[] chances)
        {
            double random = Utility.RandomDouble();

            for (int i = 0; i < chances.Length; ++i)
            {
                if (random < chances[i])
                    return (i == 0 ? BulkMaterialType.None : start + (i - 1));

                random -= chances[i];
            }

            return BulkMaterialType.None;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060654); // small bulk order

            if (m_RequireExceptional)
                list.Add(1045141); // All items must be exceptional.

            if (m_Material != BulkMaterialType.None)
                list.Add(SmallBODGump.GetMaterialNumberFor(m_Material)); // All items must be made with x material.

            list.Add(1060656, m_AmountMax.ToString()); // amount to make: ~1_val~
            list.Add(1060658, "#{0}\t{1}", m_Number, m_AmountCur); // ~1_val~: ~2_val~
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack) || InSecureTrade || RootParent is PlayerVendor)
                from.SendGump(new SmallBODGump(from, this));
            else
                from.SendLocalizedMessage(1045156); // You must have the deed in your backpack to use it.
        }

        public override void OnDoubleClickNotAccessible(Mobile from)
        {
            OnDoubleClick(from);
        }

        public override void OnDoubleClickSecureTrade(Mobile from)
        {
            OnDoubleClick(from);
        }

        public void BeginCombine(Mobile from)
        {
            if (m_AmountCur < m_AmountMax)
                from.Target = new SmallBODTarget(this);
            else
                from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
        }

        public abstract List<Item> ComputeRewards(bool full);
        public abstract int ComputeGold();
        public abstract int ComputeFame();

        public virtual void GetRewards(out Item reward, out int gold, out int fame)
        {
            reward = null;
            gold = ComputeGold();
            fame = ComputeFame();

            List<Item> rewards = ComputeRewards(false);

            if (rewards.Count > 0)
            {
                reward = rewards[Utility.Random(rewards.Count)];

                for (int i = 0; i < rewards.Count; ++i)
                {
                    if (rewards[i] != reward)
                        rewards[i].Delete();
                }
            }
        }

        public static BulkMaterialType GetMaterial(CraftResource resource)
        {
            switch (resource)
            {
                case CraftResource.DullCopper: return BulkMaterialType.DullCopper;
                case CraftResource.ShadowIron: return BulkMaterialType.ShadowIron;
                case CraftResource.Copper: return BulkMaterialType.Copper;
                case CraftResource.Bronze: return BulkMaterialType.Bronze;
                case CraftResource.Gold: return BulkMaterialType.Gold;
                case CraftResource.Agapite: return BulkMaterialType.Agapite;
                case CraftResource.Verite: return BulkMaterialType.Verite;
                case CraftResource.Valorite: return BulkMaterialType.Valorite;
                case CraftResource.SpinedLeather: return BulkMaterialType.Spined;
                case CraftResource.HornedLeather: return BulkMaterialType.Horned;
                case CraftResource.BarbedLeather: return BulkMaterialType.Barbed;
            }

            return BulkMaterialType.None;
        }

        public void EndCombine(Mobile from, object o)
        {
            if (o is Item && ((Item)o).IsChildOf(from.Backpack))
            {
                Type objectType = o.GetType();

                if (m_AmountCur >= m_AmountMax)
                {
                    from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
                }
                else if (m_Type == null || (objectType != m_Type && !objectType.IsSubclassOf(m_Type)) || (!(o is BaseWeapon) && !(o is BaseArmor) && !(o is BaseClothing)))
                {
                    from.SendLocalizedMessage(1045169); // The item is not in the request.
                }
                else
                {
                    BulkMaterialType material = BulkMaterialType.None;

                    if (o is BaseArmor)
                        material = GetMaterial(((BaseArmor)o).Resource);
                    else if (o is BaseClothing)
                        material = GetMaterial(((BaseClothing)o).Resource);

                    if (m_Material >= BulkMaterialType.DullCopper && m_Material <= BulkMaterialType.Valorite && material != m_Material)
                    {
                        from.SendLocalizedMessage(1045168); // The item is not made from the requested ore.
                    }
                    else if (m_Material >= BulkMaterialType.Spined && m_Material <= BulkMaterialType.Barbed && material != m_Material)
                    {
                        from.SendLocalizedMessage(1049352); // The item is not made from the requested leather type.
                    }
                    else
                    {
                        bool isExceptional = false;

                        if (o is BaseWeapon)
                            isExceptional = (((BaseWeapon)o).Quality == WeaponQuality.Exceptional);
                        else if (o is BaseArmor)
                            isExceptional = (((BaseArmor)o).Quality == ArmorQuality.Exceptional);
                        else if (o is BaseClothing)
                            isExceptional = (((BaseClothing)o).Quality == ClothingQuality.Exceptional);

                        if (m_RequireExceptional && !isExceptional)
                        {
                            from.SendLocalizedMessage(1045167); // The item must be exceptional.
                        }
                        else
                        {
                            ((Item)o).Delete();
                            ++AmountCur;

                            from.SendLocalizedMessage(1045170); // The item has been combined with the deed.

                            from.SendGump(new SmallBODGump(from, this));

                            if (m_AmountCur < m_AmountMax)
                                BeginCombine(from);
                        }
                    }
                }
            }
            else
            {
                from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
            }
        }

        public SmallBOD(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_AmountCur);
            writer.Write(m_AmountMax);
            writer.Write(m_Type == null ? null : m_Type.FullName);
            writer.Write(m_Number);
            writer.Write(m_Graphic);
            writer.Write(m_RequireExceptional);
            writer.Write((int)m_Material);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_AmountCur = reader.ReadInt();
                        m_AmountMax = reader.ReadInt();

                        string type = reader.ReadString();

                        if (type != null)
                            m_Type = ScriptCompiler.FindTypeByFullName(type);

                        m_Number = reader.ReadInt();
                        m_Graphic = reader.ReadInt();
                        m_RequireExceptional = reader.ReadBool();
                        m_Material = (BulkMaterialType)reader.ReadInt();

                        break;
                    }
            }

            if (Weight == 0.0)
                Weight = 1.0;

            if (Core.AOS && ItemID == 0x14EF)
                ItemID = 0x2258;

            if (Parent == null && Map == Map.Internal && Location == Point3D.Zero)
                Delete();
        }
    }

    public class SmallBODAcceptGump : Gump
    {
        private SmallBOD m_Deed;
        private Mobile m_From;

        public SmallBODAcceptGump(Mobile from, SmallBOD deed)
            : base(50, 50)
        {
            m_From = from;
            m_Deed = deed;

            m_From.CloseGump(typeof(LargeBODAcceptGump));
            m_From.CloseGump(typeof(SmallBODAcceptGump));

            AddPage(0);

            AddBackground(25, 10, 430, 264, 5054);

            AddImageTiled(33, 20, 413, 245, 2624);
            AddAlphaRegion(33, 20, 413, 245);

            AddImage(20, 5, 10460);
            AddImage(430, 5, 10460);
            AddImage(20, 249, 10460);
            AddImage(430, 249, 10460);

            AddHtmlLocalized(190, 25, 120, 20, 1045133, 0x7FFF, false, false); // A bulk order
            AddHtmlLocalized(40, 48, 350, 20, 1045135, 0x7FFF, false, false); // Ah!  Thanks for the goods!  Would you help me out?

            AddHtmlLocalized(40, 72, 210, 20, 1045138, 0x7FFF, false, false); // Amount to make:
            AddLabel(250, 72, 1152, deed.AmountMax.ToString());

            AddHtmlLocalized(40, 96, 120, 20, 1045136, 0x7FFF, false, false); // Item requested:
            AddItem(385, 96, deed.Graphic);
            AddHtmlLocalized(40, 120, 210, 20, deed.Number, 0xFFFFFF, false, false);

            if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
            {
                AddHtmlLocalized(40, 144, 210, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:

                if (deed.RequireExceptional)
                    AddHtmlLocalized(40, 168, 350, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.

                if (deed.Material != BulkMaterialType.None)
                    AddHtmlLocalized(40, deed.RequireExceptional ? 192 : 168, 350, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.
            }

            AddHtmlLocalized(40, 216, 350, 20, 1045139, 0x7FFF, false, false); // Do you want to accept this order?

            AddButton(100, 240, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(135, 240, 120, 20, 1006044, 0x7FFF, false, false); // Ok

            AddButton(275, 240, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(310, 240, 120, 20, 1011012, 0x7FFF, false, false); // CANCEL
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) // Ok
            {
                if (m_From.PlaceInBackpack(m_Deed))
                {
                    m_From.SendLocalizedMessage(1045152); // The bulk order deed has been placed in your backpack.
                }
                else
                {
                    m_From.SendLocalizedMessage(1045150); // There is not enough room in your backpack for the deed.
                    m_Deed.Delete();
                }
            }
            else
            {
                m_Deed.Delete();
            }
        }

        public static int GetMaterialNumberFor(BulkMaterialType material)
        {
            if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
                return 1045142 + (int)(material - BulkMaterialType.DullCopper);
            else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
                return 1049348 + (int)(material - BulkMaterialType.Spined);

            return 0;
        }
    }

    public class SmallBODGump : Gump
    {
        private SmallBOD m_Deed;
        private Mobile m_From;

        public SmallBODGump(Mobile from, SmallBOD deed)
            : base(25, 25)
        {
            m_From = from;
            m_Deed = deed;

            m_From.CloseGump(typeof(LargeBODGump));
            m_From.CloseGump(typeof(SmallBODGump));

            AddPage(0);

            AddBackground(50, 10, 455, 260, 5054);
            AddImageTiled(58, 20, 438, 241, 2624);
            AddAlphaRegion(58, 20, 438, 241);

            AddImage(45, 5, 10460);
            AddImage(480, 5, 10460);
            AddImage(45, 245, 10460);
            AddImage(480, 245, 10460);

            AddHtmlLocalized(225, 25, 120, 20, 1045133, 0x7FFF, false, false); // A bulk order

            AddHtmlLocalized(75, 48, 250, 20, 1045138, 0x7FFF, false, false); // Amount to make:
            AddLabel(275, 48, 1152, deed.AmountMax.ToString());

            AddHtmlLocalized(275, 76, 200, 20, 1045153, 0x7FFF, false, false); // Amount finished:
            AddHtmlLocalized(75, 72, 120, 20, 1045136, 0x7FFF, false, false); // Item requested:

            AddItem(410, 72, deed.Graphic);

            AddHtmlLocalized(75, 96, 210, 20, deed.Number, 0x7FFF, false, false);
            AddLabel(275, 96, 0x480, deed.AmountCur.ToString());

            if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
                AddHtmlLocalized(75, 120, 200, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:

            if (deed.RequireExceptional)
                AddHtmlLocalized(75, 144, 300, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.

            if (deed.Material != BulkMaterialType.None)
                AddHtmlLocalized(75, deed.RequireExceptional ? 168 : 144, 300, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.

            AddButton(125, 192, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtmlLocalized(160, 192, 300, 20, 1045154, 0x7FFF, false, false); // Combine this deed with the item requested.

            AddButton(125, 216, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(160, 216, 120, 20, 1011441, 0x7FFF, false, false); // EXIT
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Deed.Deleted || !m_Deed.IsChildOf(m_From.Backpack))
                return;

            if (info.ButtonID == 2) // Combine
            {
                m_From.SendGump(new SmallBODGump(m_From, m_Deed));
                m_Deed.BeginCombine(m_From);
            }
        }

        public static int GetMaterialNumberFor(BulkMaterialType material)
        {
            if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
                return 1045142 + (int)(material - BulkMaterialType.DullCopper);
            else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
                return 1049348 + (int)(material - BulkMaterialType.Spined);

            return 0;
        }
    }

    public class SmallBODTarget : Target
    {
        private SmallBOD m_Deed;

        public SmallBODTarget(SmallBOD deed)
            : base(18, false, TargetFlags.None)
        {
            m_Deed = deed;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Deed.Deleted || !m_Deed.IsChildOf(from.Backpack))
                return;

            m_Deed.EndCombine(from, targeted);
        }
    }

    public class SmallBulkEntry
    {
        private Type m_Type;
        private int m_Number;
        private int m_Graphic;

        public Type Type { get { return m_Type; } }
        public int Number { get { return m_Number; } }
        public int Graphic { get { return m_Graphic; } }

        public SmallBulkEntry(Type type, int number, int graphic)
        {
            m_Type = type;
            m_Number = number;
            m_Graphic = graphic;
        }

        #region Bulk Order Data Directory

        public static SmallBulkEntry[] BlacksmithWeapons
        {
            get { return GetEntries("Blacksmith", "weapons"); }
        }

        public static SmallBulkEntry[] BlacksmithArmor
        {
            get { return GetEntries("Blacksmith", "armor"); }
        }

        public static SmallBulkEntry[] TailorCloth
        {
            get { return GetEntries("Tailor", "cloth"); }
        }

        public static SmallBulkEntry[] TailorLeather
        {
            get { return GetEntries("Tailor", "leather"); }
        }

        private static Dictionary<string, Dictionary<string, SmallBulkEntry[]>> m_Cache;

        public static SmallBulkEntry[] GetEntries(string type, string name)
        {
            if (m_Cache == null)
                m_Cache = new Dictionary<string, Dictionary<string, SmallBulkEntry[]>>();

            Dictionary<string, SmallBulkEntry[]> table = null;

            if (!m_Cache.TryGetValue(type, out table))
                m_Cache[type] = table = new Dictionary<string, SmallBulkEntry[]>();

            SmallBulkEntry[] entries = null;

            if (!table.TryGetValue(name, out entries))
                table[name] = entries = LoadEntries(type, name);

            return entries;
        }

        public static SmallBulkEntry[] LoadEntries(string type, string name)
        {
            return LoadEntries(String.Format("Data/Vocation/{0}/{1}.cfg", type, name));
            //return LoadEntries(String.Format("Data/Bulk Orders/{0}/{1}.cfg", type, name));
        }

        #endregion

        public static SmallBulkEntry[] LoadEntries(string path)
        {
            path = Path.Combine(Core.BaseDirectory, path);

            List<SmallBulkEntry> list = new List<SmallBulkEntry>();

            if (File.Exists(path))
            {
                using (StreamReader ip = new StreamReader(path))
                {
                    string line;

                    while ((line = ip.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#"))
                            continue;

                        try
                        {
                            string[] split = line.Split('\t');

                            if (split.Length >= 2)
                            {
                                Type type = ScriptCompiler.FindTypeByName(split[0]);
                                int graphic = Utility.ToInt32(split[split.Length - 1]);

                                if (type != null && graphic > 0)
                                    list.Add(new SmallBulkEntry(type, graphic < 0x4000 ? 1020000 + graphic : 1078872 + graphic, graphic));
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return list.ToArray();
        }
    }

    [TypeAlias("Scripts.Engines.BulkOrders.SmallSmithBOD")]
    public class SmallSmithBOD : SmallBOD
    {
        public static double[] m_BlacksmithMaterialChances = new double[]
			{
				0.501953125, // None
				0.250000000, // Dull Copper
				0.125000000, // Shadow Iron
				0.062500000, // Copper
				0.031250000, // Bronze
				0.015625000, // Gold
				0.007812500, // Agapite
				0.003906250, // Verite
				0.001953125  // Valorite
			};

        public override int ComputeFame()
        {
            return SmithRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return SmithRewardCalculator.Instance.ComputeGold(this);
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = SmithRewardCalculator.Instance.LookupRewards(SmithRewardCalculator.Instance.ComputePoints(this));

            if (rewardGroup != null)
            {
                if (full)
                {
                    for (int i = 0; i < rewardGroup.Items.Length; ++i)
                    {
                        Item item = rewardGroup.Items[i].Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
                else
                {
                    RewardItem rewardItem = rewardGroup.AcquireItem();

                    if (rewardItem != null)
                    {
                        Item item = rewardItem.Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
            }

            return list;
        }

        public static SmallSmithBOD CreateRandomFor(Mobile m)
        {
            SmallBulkEntry[] entries;
            bool useMaterials;

            if (useMaterials = Utility.RandomBool())
                entries = SmallBulkEntry.BlacksmithArmor;
            else
                entries = SmallBulkEntry.BlacksmithWeapons;

            if (entries.Length > 0)
            {
                double theirSkill = m.Skills[SkillName.Blacksmith].Base;
                int amountMax;

                if (theirSkill >= 70.1)
                    amountMax = Utility.RandomList(10, 15, 20, 20);
                else if (theirSkill >= 50.1)
                    amountMax = Utility.RandomList(10, 15, 15, 20);
                else
                    amountMax = Utility.RandomList(10, 10, 15, 20);

                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials && theirSkill >= 70.1)
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        BulkMaterialType check = GetRandomMaterial(BulkMaterialType.DullCopper, m_BlacksmithMaterialChances);
                        double skillReq = 0.0;

                        switch (check)
                        {
                            case BulkMaterialType.DullCopper: skillReq = 65.0; break;
                            case BulkMaterialType.ShadowIron: skillReq = 70.0; break;
                            case BulkMaterialType.Copper: skillReq = 75.0; break;
                            case BulkMaterialType.Bronze: skillReq = 80.0; break;
                            case BulkMaterialType.Gold: skillReq = 85.0; break;
                            case BulkMaterialType.Agapite: skillReq = 90.0; break;
                            case BulkMaterialType.Verite: skillReq = 95.0; break;
                            case BulkMaterialType.Valorite: skillReq = 100.0; break;
                            case BulkMaterialType.Spined: skillReq = 65.0; break;
                            case BulkMaterialType.Horned: skillReq = 80.0; break;
                            case BulkMaterialType.Barbed: skillReq = 99.0; break;
                        }

                        if (theirSkill >= skillReq)
                        {
                            material = check;
                            break;
                        }
                    }
                }

                double excChance = 0.0;

                if (theirSkill >= 70.1)
                    excChance = (theirSkill + 80.0) / 200.0;

                bool reqExceptional = (excChance > Utility.RandomDouble());

                CraftSystem system = DefBlacksmithy.CraftSystem;

                List<SmallBulkEntry> validEntries = new List<SmallBulkEntry>();

                for (int i = 0; i < entries.Length; ++i)
                {
                    CraftItem item = system.CraftItems.SearchFor(entries[i].Type);

                    if (item != null)
                    {
                        bool allRequiredSkills = true;
                        double chance = item.GetSuccessChance(m, null, system, false, ref allRequiredSkills);

                        if (allRequiredSkills && chance >= 0.0)
                        {
                            if (reqExceptional)
                                chance = item.GetExceptionalChance(system, chance, m);

                            if (chance > 0.0)
                                validEntries.Add(entries[i]);
                        }
                    }
                }

                if (validEntries.Count > 0)
                {
                    SmallBulkEntry entry = validEntries[Utility.Random(validEntries.Count)];
                    return new SmallSmithBOD(entry, material, amountMax, reqExceptional);
                }
            }

            return null;
        }

        private SmallSmithBOD(SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional)
        {
            this.Hue = 0x44E;
            this.AmountMax = amountMax;
            this.Type = entry.Type;
            this.Number = entry.Number;
            this.Graphic = entry.Graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        [Constructable]
        public SmallSmithBOD()
        {
            SmallBulkEntry[] entries;
            bool useMaterials;

            if (useMaterials = Utility.RandomBool())
                entries = SmallBulkEntry.BlacksmithArmor;
            else
                entries = SmallBulkEntry.BlacksmithWeapons;

            if (entries.Length > 0)
            {
                int hue = 0x44E;
                int amountMax = Utility.RandomList(10, 15, 20);

                BulkMaterialType material;

                if (useMaterials)
                    material = GetRandomMaterial(BulkMaterialType.DullCopper, m_BlacksmithMaterialChances);
                else
                    material = BulkMaterialType.None;

                bool reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

                SmallBulkEntry entry = entries[Utility.Random(entries.Length)];

                this.Hue = hue;
                this.AmountMax = amountMax;
                this.Type = entry.Type;
                this.Number = entry.Number;
                this.Graphic = entry.Graphic;
                this.RequireExceptional = reqExceptional;
                this.Material = material;
            }
        }

        public SmallSmithBOD(int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat)
        {
            this.Hue = 0x44E;
            this.AmountMax = amountMax;
            this.AmountCur = amountCur;
            this.Type = type;
            this.Number = number;
            this.Graphic = graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
        }

        public SmallSmithBOD(Serial serial)
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

    public class SmallTailorBOD : SmallBOD
    {
        public static double[] m_TailoringMaterialChances = new double[]
			{
				0.857421875, // None
				0.125000000, // Spined
				0.015625000, // Horned
				0.001953125  // Barbed
			};

        public override int ComputeFame()
        {
            return TailorRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return TailorRewardCalculator.Instance.ComputeGold(this);
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = TailorRewardCalculator.Instance.LookupRewards(TailorRewardCalculator.Instance.ComputePoints(this));

            if (rewardGroup != null)
            {
                if (full)
                {
                    for (int i = 0; i < rewardGroup.Items.Length; ++i)
                    {
                        Item item = rewardGroup.Items[i].Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
                else
                {
                    RewardItem rewardItem = rewardGroup.AcquireItem();

                    if (rewardItem != null)
                    {
                        Item item = rewardItem.Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
            }

            return list;
        }

        public static SmallTailorBOD CreateRandomFor(Mobile m)
        {
            SmallBulkEntry[] entries;
            bool useMaterials = Utility.RandomBool();

            double theirSkill = m.Skills[SkillName.Tailoring].Base;
            if (useMaterials && theirSkill >= 6.2) // Ugly, but the easiest leather BOD is Leather Cap which requires at least 6.2 skill.
                entries = SmallBulkEntry.TailorLeather;
            else
                entries = SmallBulkEntry.TailorCloth;

            if (entries.Length > 0)
            {
                int amountMax;

                if (theirSkill >= 70.1)
                    amountMax = Utility.RandomList(10, 15, 20, 20);
                else if (theirSkill >= 50.1)
                    amountMax = Utility.RandomList(10, 15, 15, 20);
                else
                    amountMax = Utility.RandomList(10, 10, 15, 20);

                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials && theirSkill >= 70.1)
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        BulkMaterialType check = GetRandomMaterial(BulkMaterialType.Spined, m_TailoringMaterialChances);
                        double skillReq = 0.0;

                        switch (check)
                        {
                            case BulkMaterialType.DullCopper: skillReq = 65.0; break;
                            case BulkMaterialType.Bronze: skillReq = 80.0; break;
                            case BulkMaterialType.Gold: skillReq = 85.0; break;
                            case BulkMaterialType.Agapite: skillReq = 90.0; break;
                            case BulkMaterialType.Verite: skillReq = 95.0; break;
                            case BulkMaterialType.Valorite: skillReq = 100.0; break;
                            case BulkMaterialType.Spined: skillReq = 65.0; break;
                            case BulkMaterialType.Horned: skillReq = 80.0; break;
                            case BulkMaterialType.Barbed: skillReq = 99.0; break;
                        }

                        if (theirSkill >= skillReq)
                        {
                            material = check;
                            break;
                        }
                    }
                }

                double excChance = 0.0;

                if (theirSkill >= 70.1)
                    excChance = (theirSkill + 80.0) / 200.0;

                bool reqExceptional = (excChance > Utility.RandomDouble());


                CraftSystem system = DefTailoring.CraftSystem;

                List<SmallBulkEntry> validEntries = new List<SmallBulkEntry>();

                for (int i = 0; i < entries.Length; ++i)
                {
                    CraftItem item = system.CraftItems.SearchFor(entries[i].Type);

                    if (item != null)
                    {
                        bool allRequiredSkills = true;
                        double chance = item.GetSuccessChance(m, null, system, false, ref allRequiredSkills);

                        if (allRequiredSkills && chance >= 0.0)
                        {
                            if (reqExceptional)
                                chance = item.GetExceptionalChance(system, chance, m);

                            if (chance > 0.0)
                                validEntries.Add(entries[i]);
                        }
                    }
                }

                if (validEntries.Count > 0)
                {
                    SmallBulkEntry entry = validEntries[Utility.Random(validEntries.Count)];
                    return new SmallTailorBOD(entry, material, amountMax, reqExceptional);
                }
            }

            return null;
        }

        private SmallTailorBOD(SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional)
        {
            this.Hue = 0x483;
            this.AmountMax = amountMax;
            this.Type = entry.Type;
            this.Number = entry.Number;
            this.Graphic = entry.Graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        [Constructable]
        public SmallTailorBOD()
        {
            SmallBulkEntry[] entries;
            bool useMaterials;

            if (useMaterials = Utility.RandomBool())
                entries = SmallBulkEntry.TailorLeather;
            else
                entries = SmallBulkEntry.TailorCloth;

            if (entries.Length > 0)
            {
                int hue = 0x483;
                int amountMax = Utility.RandomList(10, 15, 20);

                BulkMaterialType material;

                if (useMaterials)
                    material = GetRandomMaterial(BulkMaterialType.Spined, m_TailoringMaterialChances);
                else
                    material = BulkMaterialType.None;

                bool reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

                SmallBulkEntry entry = entries[Utility.Random(entries.Length)];

                this.Hue = hue;
                this.AmountMax = amountMax;
                this.Type = entry.Type;
                this.Number = entry.Number;
                this.Graphic = entry.Graphic;
                this.RequireExceptional = reqExceptional;
                this.Material = material;
            }
        }

        public SmallTailorBOD(int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat)
        {
            this.Hue = 0x483;
            this.AmountMax = amountMax;
            this.AmountCur = amountCur;
            this.Type = type;
            this.Number = number;
            this.Graphic = graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
        }

        public SmallTailorBOD(Serial serial)
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

    #endregion

    #region Large Bulk Orders

    [TypeAlias("Scripts.Engines.BulkOrders.LargeBOD")]
    public abstract class LargeBOD : Item
    {
        private int m_AmountMax;
        private bool m_RequireExceptional;
        private BulkMaterialType m_Material;
        private LargeBulkEntry[] m_Entries;

        [CommandProperty(AccessLevel.GameMaster)]
        public int AmountMax { get { return m_AmountMax; } set { m_AmountMax = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RequireExceptional { get { return m_RequireExceptional; } set { m_RequireExceptional = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public BulkMaterialType Material { get { return m_Material; } set { m_Material = value; InvalidateProperties(); } }

        public LargeBulkEntry[] Entries { get { return m_Entries; } set { m_Entries = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Complete
        {
            get
            {
                for (int i = 0; i < m_Entries.Length; ++i)
                {
                    if (m_Entries[i].Amount < m_AmountMax)
                        return false;
                }

                return true;
            }
        }

        public abstract List<Item> ComputeRewards(bool full);
        public abstract int ComputeGold();
        public abstract int ComputeFame();

        public virtual void GetRewards(out Item reward, out int gold, out int fame)
        {
            reward = null;
            gold = ComputeGold();
            fame = ComputeFame();

            List<Item> rewards = ComputeRewards(false);

            if (rewards.Count > 0)
            {
                reward = rewards[Utility.Random(rewards.Count)];

                for (int i = 0; i < rewards.Count; ++i)
                {
                    if (rewards[i] != reward)
                        rewards[i].Delete();
                }
            }
        }

        public static BulkMaterialType GetRandomMaterial(BulkMaterialType start, double[] chances)
        {
            double random = Utility.RandomDouble();

            for (int i = 0; i < chances.Length; ++i)
            {
                if (random < chances[i])
                    return (i == 0 ? BulkMaterialType.None : start + (i - 1));

                random -= chances[i];
            }

            return BulkMaterialType.None;
        }

        public override int LabelNumber { get { return 1045151; } } // a bulk order deed

        public LargeBOD(int hue, int amountMax, bool requireExeptional, BulkMaterialType material, LargeBulkEntry[] entries)
            : base(Core.AOS ? 0x2258 : 0x14EF)
        {
            Weight = 1.0;
            Hue = hue; // Blacksmith: 0x44E; Tailoring: 0x483
            LootType = LootType.Blessed;

            m_AmountMax = amountMax;
            m_RequireExceptional = requireExeptional;
            m_Material = material;
            m_Entries = entries;
        }

        public LargeBOD()
            : base(Core.AOS ? 0x2258 : 0x14EF)
        {
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060655); // large bulk order

            if (m_RequireExceptional)
                list.Add(1045141); // All items must be exceptional.

            if (m_Material != BulkMaterialType.None)
                list.Add(LargeBODGump.GetMaterialNumberFor(m_Material)); // All items must be made with x material.

            list.Add(1060656, m_AmountMax.ToString()); // amount to make: ~1_val~

            for (int i = 0; i < m_Entries.Length; ++i)
                list.Add(1060658 + i, "#{0}\t{1}", m_Entries[i].Details.Number, m_Entries[i].Amount); // ~1_val~: ~2_val~
        }

        public override void OnDoubleClickNotAccessible(Mobile from)
        {
            OnDoubleClick(from);
        }

        public override void OnDoubleClickSecureTrade(Mobile from)
        {
            OnDoubleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack) || InSecureTrade || RootParent is PlayerVendor)
                from.SendGump(new LargeBODGump(from, this));
            else
                from.SendLocalizedMessage(1045156); // You must have the deed in your backpack to use it.
        }

        public void BeginCombine(Mobile from)
        {
            if (!Complete)
                from.Target = new LargeBODTarget(this);
            else
                from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
        }

        public void EndCombine(Mobile from, object o)
        {
            if (o is Item && ((Item)o).IsChildOf(from.Backpack))
            {
                if (o is SmallBOD)
                {
                    SmallBOD small = (SmallBOD)o;

                    LargeBulkEntry entry = null;

                    for (int i = 0; entry == null && i < m_Entries.Length; ++i)
                    {
                        if (m_Entries[i].Details.Type == small.Type)
                            entry = m_Entries[i];
                    }

                    if (entry == null)
                    {
                        from.SendLocalizedMessage(1045160); // That is not a bulk order for this large request.
                    }
                    else if (m_RequireExceptional && !small.RequireExceptional)
                    {
                        from.SendLocalizedMessage(1045161); // Both orders must be of exceptional quality.
                    }
                    else if (m_Material >= BulkMaterialType.DullCopper && m_Material <= BulkMaterialType.Valorite && small.Material != m_Material)
                    {
                        from.SendLocalizedMessage(1045162); // Both orders must use the same ore type.
                    }
                    else if (m_Material >= BulkMaterialType.Spined && m_Material <= BulkMaterialType.Barbed && small.Material != m_Material)
                    {
                        from.SendLocalizedMessage(1049351); // Both orders must use the same leather type.
                    }
                    else if (m_AmountMax != small.AmountMax)
                    {
                        from.SendLocalizedMessage(1045163); // The two orders have different requested amounts and cannot be combined.
                    }
                    else if (small.AmountCur < small.AmountMax)
                    {
                        from.SendLocalizedMessage(1045164); // The order to combine with is not completed.
                    }
                    else if (entry.Amount >= m_AmountMax)
                    {
                        from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
                    }
                    else
                    {
                        entry.Amount += small.AmountCur;
                        small.Delete();

                        from.SendLocalizedMessage(1045165); // The orders have been combined.

                        from.SendGump(new LargeBODGump(from, this));

                        if (!Complete)
                            BeginCombine(from);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1045159); // That is not a bulk order.
                }
            }
            else
            {
                from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
            }
        }

        public LargeBOD(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_AmountMax);
            writer.Write(m_RequireExceptional);
            writer.Write((int)m_Material);

            writer.Write((int)m_Entries.Length);

            for (int i = 0; i < m_Entries.Length; ++i)
                m_Entries[i].Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_AmountMax = reader.ReadInt();
                        m_RequireExceptional = reader.ReadBool();
                        m_Material = (BulkMaterialType)reader.ReadInt();

                        m_Entries = new LargeBulkEntry[reader.ReadInt()];

                        for (int i = 0; i < m_Entries.Length; ++i)
                            m_Entries[i] = new LargeBulkEntry(this, reader);

                        break;
                    }
            }

            if (Weight == 0.0)
                Weight = 1.0;

            if (Core.AOS && ItemID == 0x14EF)
                ItemID = 0x2258;

            if (Parent == null && Map == Map.Internal && Location == Point3D.Zero)
                Delete();
        }
    }

    public class LargeBODAcceptGump : Gump
    {
        private LargeBOD m_Deed;
        private Mobile m_From;

        public LargeBODAcceptGump(Mobile from, LargeBOD deed)
            : base(50, 50)
        {
            m_From = from;
            m_Deed = deed;

            m_From.CloseGump(typeof(LargeBODAcceptGump));
            m_From.CloseGump(typeof(SmallBODAcceptGump));

            LargeBulkEntry[] entries = deed.Entries;

            AddPage(0);

            AddBackground(25, 10, 430, 240 + (entries.Length * 24), 5054);

            AddImageTiled(33, 20, 413, 221 + (entries.Length * 24), 2624);
            AddAlphaRegion(33, 20, 413, 221 + (entries.Length * 24));

            AddImage(20, 5, 10460);
            AddImage(430, 5, 10460);
            AddImage(20, 225 + (entries.Length * 24), 10460);
            AddImage(430, 225 + (entries.Length * 24), 10460);

            AddHtmlLocalized(180, 25, 120, 20, 1045134, 0x7FFF, false, false); // A large bulk order

            AddHtmlLocalized(40, 48, 350, 20, 1045135, 0x7FFF, false, false); // Ah!  Thanks for the goods!  Would you help me out?

            AddHtmlLocalized(40, 72, 210, 20, 1045138, 0x7FFF, false, false); // Amount to make:
            AddLabel(250, 72, 1152, deed.AmountMax.ToString());

            AddHtmlLocalized(40, 96, 120, 20, 1045137, 0x7FFF, false, false); // Items requested:

            int y = 120;

            for (int i = 0; i < entries.Length; ++i, y += 24)
                AddHtmlLocalized(40, y, 210, 20, entries[i].Details.Number, 0x7FFF, false, false);

            if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
            {
                AddHtmlLocalized(40, y, 210, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:
                y += 24;

                if (deed.RequireExceptional)
                {
                    AddHtmlLocalized(40, y, 350, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
                    y += 24;
                }

                if (deed.Material != BulkMaterialType.None)
                {
                    AddHtmlLocalized(40, y, 350, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.
                    y += 24;
                }
            }

            AddHtmlLocalized(40, 192 + (entries.Length * 24), 350, 20, 1045139, 0x7FFF, false, false); // Do you want to accept this order?

            AddButton(100, 216 + (entries.Length * 24), 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(135, 216 + (entries.Length * 24), 120, 20, 1006044, 0x7FFF, false, false); // Ok

            AddButton(275, 216 + (entries.Length * 24), 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(310, 216 + (entries.Length * 24), 120, 20, 1011012, 0x7FFF, false, false); // CANCEL
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) // Ok
            {
                if (m_From.PlaceInBackpack(m_Deed))
                {
                    m_From.SendLocalizedMessage(1045152); // The bulk order deed has been placed in your backpack.
                }
                else
                {
                    m_From.SendLocalizedMessage(1045150); // There is not enough room in your backpack for the deed.
                    m_Deed.Delete();
                }
            }
            else
            {
                m_Deed.Delete();
            }
        }

        public static int GetMaterialNumberFor(BulkMaterialType material)
        {
            if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
                return 1045142 + (int)(material - BulkMaterialType.DullCopper);
            else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
                return 1049348 + (int)(material - BulkMaterialType.Spined);

            return 0;
        }
    }

    public class LargeBODGump : Gump
    {
        private LargeBOD m_Deed;
        private Mobile m_From;

        public LargeBODGump(Mobile from, LargeBOD deed)
            : base(25, 25)
        {
            m_From = from;
            m_Deed = deed;

            m_From.CloseGump(typeof(LargeBODGump));
            m_From.CloseGump(typeof(SmallBODGump));

            LargeBulkEntry[] entries = deed.Entries;

            AddPage(0);

            AddBackground(50, 10, 455, 236 + (entries.Length * 24), 5054);

            AddImageTiled(58, 20, 438, 217 + (entries.Length * 24), 2624);
            AddAlphaRegion(58, 20, 438, 217 + (entries.Length * 24));

            AddImage(45, 5, 10460);
            AddImage(480, 5, 10460);
            AddImage(45, 221 + (entries.Length * 24), 10460);
            AddImage(480, 221 + (entries.Length * 24), 10460);

            AddHtmlLocalized(225, 25, 120, 20, 1045134, 0x7FFF, false, false); // A large bulk order

            AddHtmlLocalized(75, 48, 250, 20, 1045138, 0x7FFF, false, false); // Amount to make:
            AddLabel(275, 48, 1152, deed.AmountMax.ToString());

            AddHtmlLocalized(75, 72, 120, 20, 1045137, 0x7FFF, false, false); // Items requested:
            AddHtmlLocalized(275, 76, 200, 20, 1045153, 0x7FFF, false, false); // Amount finished:

            int y = 96;

            for (int i = 0; i < entries.Length; ++i)
            {
                LargeBulkEntry entry = entries[i];
                SmallBulkEntry details = entry.Details;

                AddHtmlLocalized(75, y, 210, 20, details.Number, 0x7FFF, false, false);
                AddLabel(275, y, 0x480, entry.Amount.ToString());

                y += 24;
            }

            if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
            {
                AddHtmlLocalized(75, y, 200, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:
                y += 24;
            }

            if (deed.RequireExceptional)
            {
                AddHtmlLocalized(75, y, 300, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
                y += 24;
            }

            if (deed.Material != BulkMaterialType.None)
                AddHtmlLocalized(75, y, 300, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.

            AddButton(125, 168 + (entries.Length * 24), 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtmlLocalized(160, 168 + (entries.Length * 24), 300, 20, 1045155, 0x7FFF, false, false); // Combine this deed with another deed.

            AddButton(125, 192 + (entries.Length * 24), 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(160, 192 + (entries.Length * 24), 120, 20, 1011441, 0x7FFF, false, false); // EXIT
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Deed.Deleted || !m_Deed.IsChildOf(m_From.Backpack))
                return;

            if (info.ButtonID == 2) // Combine
            {
                m_From.SendGump(new LargeBODGump(m_From, m_Deed));
                m_Deed.BeginCombine(m_From);
            }
        }

        public static int GetMaterialNumberFor(BulkMaterialType material)
        {
            if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
                return 1045142 + (int)(material - BulkMaterialType.DullCopper);
            else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
                return 1049348 + (int)(material - BulkMaterialType.Spined);

            return 0;
        }
    }

    public class LargeBODTarget : Target
    {
        private LargeBOD m_Deed;

        public LargeBODTarget(LargeBOD deed)
            : base(18, false, TargetFlags.None)
        {
            m_Deed = deed;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Deed.Deleted || !m_Deed.IsChildOf(from.Backpack))
                return;

            m_Deed.EndCombine(from, targeted);
        }
    }

    public class LargeBulkEntry
    {
        private LargeBOD m_Owner;
        private int m_Amount;
        private SmallBulkEntry m_Details;

        public LargeBOD Owner { get { return m_Owner; } set { m_Owner = value; } }
        public int Amount { get { return m_Amount; } set { m_Amount = value; if (m_Owner != null) m_Owner.InvalidateProperties(); } }
        public SmallBulkEntry Details { get { return m_Details; } }

        #region Bulk Order Data Directory

        public static SmallBulkEntry[] LargeRing
        {
            get { return GetEntries("Blacksmith", "largering"); }
        }

        public static SmallBulkEntry[] LargePlate
        {
            get { return GetEntries("Blacksmith", "largeplate"); }
        }

        public static SmallBulkEntry[] LargeChain
        {
            get { return GetEntries("Blacksmith", "largechain"); }
        }

        public static SmallBulkEntry[] LargeAxes
        {
            get { return GetEntries("Blacksmith", "largeaxes"); }
        }

        public static SmallBulkEntry[] LargeFencing
        {
            get { return GetEntries("Blacksmith", "largefencing"); }
        }

        public static SmallBulkEntry[] LargeMaces
        {
            get { return GetEntries("Blacksmith", "largemaces"); }
        }

        public static SmallBulkEntry[] LargePolearms
        {
            get { return GetEntries("Blacksmith", "largepolearms"); }
        }

        public static SmallBulkEntry[] LargeSwords
        {
            get { return GetEntries("Blacksmith", "largeswords"); }
        }

        public static SmallBulkEntry[] BoneSet
        {
            get { return GetEntries("Tailor", "boneset"); }
        }

        public static SmallBulkEntry[] Farmer
        {
            get { return GetEntries("Tailor", "farmer"); }
        }

        public static SmallBulkEntry[] FemaleLeatherSet
        {
            get { return GetEntries("Tailor", "femaleleatherset"); }
        }

        public static SmallBulkEntry[] FisherGirl
        {
            get { return GetEntries("Tailor", "fishergirl"); }
        }

        public static SmallBulkEntry[] Gypsy
        {
            get { return GetEntries("Tailor", "gypsy"); }
        }

        public static SmallBulkEntry[] HatSet
        {
            get { return GetEntries("Tailor", "hatset"); }
        }

        public static SmallBulkEntry[] Jester
        {
            get { return GetEntries("Tailor", "jester"); }
        }

        public static SmallBulkEntry[] Lady
        {
            get { return GetEntries("Tailor", "lady"); }
        }

        public static SmallBulkEntry[] MaleLeatherSet
        {
            get { return GetEntries("Tailor", "maleleatherset"); }
        }

        public static SmallBulkEntry[] Pirate
        {
            get { return GetEntries("Tailor", "pirate"); }
        }

        public static SmallBulkEntry[] ShoeSet
        {
            get { return GetEntries("Tailor", "shoeset"); }
        }

        public static SmallBulkEntry[] StuddedSet
        {
            get { return GetEntries("Tailor", "studdedset"); }
        }

        public static SmallBulkEntry[] TownCrier
        {
            get { return GetEntries("Tailor", "towncrier"); }
        }

        public static SmallBulkEntry[] Wizard
        {
            get { return GetEntries("Tailor", "wizard"); }
        }

        #endregion

        private static Dictionary<string, Dictionary<string, SmallBulkEntry[]>> m_Cache;

        public static SmallBulkEntry[] GetEntries(string type, string name)
        {
            if (m_Cache == null)
                m_Cache = new Dictionary<string, Dictionary<string, SmallBulkEntry[]>>();

            Dictionary<string, SmallBulkEntry[]> table = null;

            if (!m_Cache.TryGetValue(type, out table))
                m_Cache[type] = table = new Dictionary<string, SmallBulkEntry[]>();

            SmallBulkEntry[] entries = null;

            if (!table.TryGetValue(name, out entries))
                table[name] = entries = SmallBulkEntry.LoadEntries(type, name);

            return entries;
        }

        public static LargeBulkEntry[] ConvertEntries(LargeBOD owner, SmallBulkEntry[] small)
        {
            LargeBulkEntry[] large = new LargeBulkEntry[small.Length];

            for (int i = 0; i < small.Length; ++i)
                large[i] = new LargeBulkEntry(owner, small[i]);

            return large;
        }

        public LargeBulkEntry(LargeBOD owner, SmallBulkEntry details)
        {
            m_Owner = owner;
            m_Details = details;
        }

        public LargeBulkEntry(LargeBOD owner, GenericReader reader)
        {
            m_Owner = owner;
            m_Amount = reader.ReadInt();

            Type realType = null;

            string type = reader.ReadString();

            if (type != null)
                realType = ScriptCompiler.FindTypeByFullName(type);

            m_Details = new SmallBulkEntry(realType, reader.ReadInt(), reader.ReadInt());
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(m_Amount);
            writer.Write(m_Details.Type == null ? null : m_Details.Type.FullName);
            writer.Write(m_Details.Number);
            writer.Write(m_Details.Graphic);
        }
    }

    [TypeAlias("Scripts.Engines.BulkOrders.LargeSmithBOD")]
    public class LargeSmithBOD : LargeBOD
    {
        public static double[] m_BlacksmithMaterialChances = new double[]
			{
				0.501953125, // None
				0.250000000, // Dull Copper
				0.125000000, // Shadow Iron
				0.062500000, // Copper
				0.031250000, // Bronze
				0.015625000, // Gold
				0.007812500, // Agapite
				0.003906250, // Verite
				0.001953125  // Valorite
			};

        public override int ComputeFame()
        {
            return SmithRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return SmithRewardCalculator.Instance.ComputeGold(this);
        }

        [Constructable]
        public LargeSmithBOD()
        {
            LargeBulkEntry[] entries;
            bool useMaterials = true;

            int rand = Utility.Random(8);

            switch (rand)
            {
                default:
                case 0: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeRing); break;
                case 1: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargePlate); break;
                case 2: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeChain); break;
                case 3: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeAxes); break;
                case 4: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeFencing); break;
                case 5: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeMaces); break;
                case 6: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargePolearms); break;
                case 7: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeSwords); break;
            }

            if (rand > 2 && rand < 8)
                useMaterials = false;

            int hue = 0x44E;
            int amountMax = Utility.RandomList(10, 15, 20, 20);
            bool reqExceptional = (0.825 > Utility.RandomDouble());

            BulkMaterialType material;

            if (useMaterials)
                material = GetRandomMaterial(BulkMaterialType.DullCopper, m_BlacksmithMaterialChances);
            else
                material = BulkMaterialType.None;

            this.Hue = hue;
            this.AmountMax = amountMax;
            this.Entries = entries;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        public LargeSmithBOD(int amountMax, bool reqExceptional, BulkMaterialType mat, LargeBulkEntry[] entries)
        {
            this.Hue = 0x44E;
            this.AmountMax = amountMax;
            this.Entries = entries;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = SmithRewardCalculator.Instance.LookupRewards(SmithRewardCalculator.Instance.ComputePoints(this));

            if (rewardGroup != null)
            {
                if (full)
                {
                    for (int i = 0; i < rewardGroup.Items.Length; ++i)
                    {
                        Item item = rewardGroup.Items[i].Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
                else
                {
                    RewardItem rewardItem = rewardGroup.AcquireItem();

                    if (rewardItem != null)
                    {
                        Item item = rewardItem.Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
            }

            return list;
        }

        public LargeSmithBOD(Serial serial)
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

    public class LargeTailorBOD : LargeBOD
    {
        public static double[] m_TailoringMaterialChances = new double[]
			{
				0.857421875, // None
				0.125000000, // Spined
				0.015625000, // Horned
				0.001953125  // Barbed
			};

        public override int ComputeFame()
        {
            return TailorRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return TailorRewardCalculator.Instance.ComputeGold(this);
        }

        [Constructable]
        public LargeTailorBOD()
        {
            LargeBulkEntry[] entries;
            bool useMaterials = false;

            switch (Utility.Random(14))
            {
                default:
                case 0: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.Farmer); break;
                case 1: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.FemaleLeatherSet); useMaterials = true; break;
                case 2: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.FisherGirl); break;
                case 3: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.Gypsy); break;
                case 4: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.HatSet); break;
                case 5: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.Jester); break;
                case 6: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.Lady); break;
                case 7: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.MaleLeatherSet); useMaterials = true; break;
                case 8: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.Pirate); break;
                case 9: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.ShoeSet); useMaterials = Core.ML; break;
                case 10: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.StuddedSet); useMaterials = true; break;
                case 11: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.TownCrier); break;
                case 12: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.Wizard); break;
                case 13: entries = LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.BoneSet); useMaterials = true; break;
            }

            int hue = 0x483;
            int amountMax = Utility.RandomList(10, 15, 20, 20);
            bool reqExceptional = (0.825 > Utility.RandomDouble());

            BulkMaterialType material;

            if (useMaterials)
                material = GetRandomMaterial(BulkMaterialType.Spined, m_TailoringMaterialChances);
            else
                material = BulkMaterialType.None;

            this.Hue = hue;
            this.AmountMax = amountMax;
            this.Entries = entries;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        public LargeTailorBOD(int amountMax, bool reqExceptional, BulkMaterialType mat, LargeBulkEntry[] entries)
        {
            this.Hue = 0x483;
            this.AmountMax = amountMax;
            this.Entries = entries;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = TailorRewardCalculator.Instance.LookupRewards(TailorRewardCalculator.Instance.ComputePoints(this));

            if (rewardGroup != null)
            {
                if (full)
                {
                    for (int i = 0; i < rewardGroup.Items.Length; ++i)
                    {
                        Item item = rewardGroup.Items[i].Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
                else
                {
                    RewardItem rewardItem = rewardGroup.AcquireItem();

                    if (rewardItem != null)
                    {
                        Item item = rewardItem.Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
            }

            return list;
        }

        public LargeTailorBOD(Serial serial)
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

    #endregion

    #region Bulk Order Reward

    public delegate Item ConstructCallback(int type);

    public sealed class RewardType
    {
        private int m_Points;
        private Type[] m_Types;

        public int Points { get { return m_Points; } }
        public Type[] Types { get { return m_Types; } }

        public RewardType(int points, params Type[] types)
        {
            m_Points = points;
            m_Types = types;
        }

        public bool Contains(Type type)
        {
            for (int i = 0; i < m_Types.Length; ++i)
            {
                if (m_Types[i] == type)
                    return true;
            }

            return false;
        }
    }

    public sealed class RewardItem
    {
        private int m_Weight;
        private ConstructCallback m_Constructor;
        private int m_Type;

        public int Weight { get { return m_Weight; } }
        public ConstructCallback Constructor { get { return m_Constructor; } }
        public int Type { get { return m_Type; } }

        public RewardItem(int weight, ConstructCallback constructor)
            : this(weight, constructor, 0)
        {
        }

        public RewardItem(int weight, ConstructCallback constructor, int type)
        {
            m_Weight = weight;
            m_Constructor = constructor;
            m_Type = type;
        }

        public Item Construct()
        {
            try { return m_Constructor(m_Type); }
            catch { return null; }
        }
    }

    public sealed class RewardGroup
    {
        private int m_Points;
        private RewardItem[] m_Items;

        public int Points { get { return m_Points; } }
        public RewardItem[] Items { get { return m_Items; } }

        public RewardGroup(int points, params RewardItem[] items)
        {
            m_Points = points;
            m_Items = items;
        }

        public RewardItem AcquireItem()
        {
            if (m_Items.Length == 0)
                return null;
            else if (m_Items.Length == 1)
                return m_Items[0];

            int totalWeight = 0;

            for (int i = 0; i < m_Items.Length; ++i)
                totalWeight += m_Items[i].Weight;

            int randomWeight = Utility.Random(totalWeight);

            for (int i = 0; i < m_Items.Length; ++i)
            {
                RewardItem item = m_Items[i];

                if (randomWeight < item.Weight)
                    return item;

                randomWeight -= item.Weight;
            }

            return null;
        }
    }

    public abstract class RewardCalculator
    {
        private RewardGroup[] m_Groups;

        public RewardGroup[] Groups { get { return m_Groups; } set { m_Groups = value; } }

        public abstract int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type);
        public abstract int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type);

        public virtual int ComputeFame(SmallBOD bod)
        {
            int points = ComputePoints(bod) / 50;

            return points * points;
        }

        public virtual int ComputeFame(LargeBOD bod)
        {
            int points = ComputePoints(bod) / 50;

            return points * points;
        }

        public virtual int ComputePoints(SmallBOD bod)
        {
            return ComputePoints(bod.AmountMax, bod.RequireExceptional, bod.Material, 1, bod.Type);
        }

        public virtual int ComputePoints(LargeBOD bod)
        {
            return ComputePoints(bod.AmountMax, bod.RequireExceptional, bod.Material, bod.Entries.Length, bod.Entries[0].Details.Type);
        }

        public virtual int ComputeGold(SmallBOD bod)
        {
            return ComputeGold(bod.AmountMax, bod.RequireExceptional, bod.Material, 1, bod.Type);
        }

        public virtual int ComputeGold(LargeBOD bod)
        {
            return ComputeGold(bod.AmountMax, bod.RequireExceptional, bod.Material, bod.Entries.Length, bod.Entries[0].Details.Type);
        }

        public virtual RewardGroup LookupRewards(int points)
        {
            for (int i = m_Groups.Length - 1; i >= 1; --i)
            {
                RewardGroup group = m_Groups[i];

                if (points >= group.Points)
                    return group;
            }

            return m_Groups[0];
        }

        public virtual int LookupTypePoints(RewardType[] types, Type type)
        {
            for (int i = 0; i < types.Length; ++i)
            {
                if (types[i].Contains(type))
                    return types[i].Points;
            }

            return 0;
        }

        public RewardCalculator()
        {
        }
    }

    public sealed class SmithRewardCalculator : RewardCalculator
    {
        #region Constructors
        private static readonly ConstructCallback SturdyShovel = new ConstructCallback(CreateSturdyShovel);
        private static readonly ConstructCallback SturdyPickaxe = new ConstructCallback(CreateSturdyPickaxe);
        private static readonly ConstructCallback MiningGloves = new ConstructCallback(CreateMiningGloves);
        private static readonly ConstructCallback GargoylesPickaxe = new ConstructCallback(CreateGargoylesPickaxe);
        private static readonly ConstructCallback ProspectorsTool = new ConstructCallback(CreateProspectorsTool);
        private static readonly ConstructCallback PowderOfTemperament = new ConstructCallback(CreatePowderOfTemperament);
        private static readonly ConstructCallback RunicHammer = new ConstructCallback(CreateRunicHammer);
        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);
        private static readonly ConstructCallback ColoredAnvil = new ConstructCallback(CreateColoredAnvil);
        private static readonly ConstructCallback AncientHammer = new ConstructCallback(CreateAncientHammer);

        private static Item CreateSturdyShovel(int type)
        {
            return new SturdyShovel();
        }

        private static Item CreateSturdyPickaxe(int type)
        {
            return new SturdyPickaxe();
        }

        private static Item CreateMiningGloves(int type)
        {
            if (type == 1)
                return new LeatherGlovesOfMining(1);
            else if (type == 3)
                return new StuddedGlovesOfMining(3);
            else if (type == 5)
                return new RingmailGlovesOfMining(5);

            throw new InvalidOperationException();
        }

        private static Item CreateGargoylesPickaxe(int type)
        {
            return new GargoylesPickaxe();
        }

        private static Item CreateProspectorsTool(int type)
        {
            return new ProspectorsTool();
        }

        private static Item CreatePowderOfTemperament(int type)
        {
            return new PowderOfTemperament();
        }

        private static Item CreateRunicHammer(int type)
        {
            if (type >= 1 && type <= 8)
                return new RunicHammer(CraftResource.Iron + type, Core.AOS ? (55 - (type * 5)) : 50);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScroll(SkillName.Blacksmith, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreateColoredAnvil(int type)
        {
            // Generate an anvil deed, not an actual anvil.
            //return new ColoredAnvilDeed();

            return new ColoredAnvil();
        }

        private static Item CreateAncientHammer(int type)
        {
            if (type == 10 || type == 15 || type == 30 || type == 60)
                return new AncientSmithyHammer(type);

            throw new InvalidOperationException();
        }
        #endregion

        public static readonly SmithRewardCalculator Instance = new SmithRewardCalculator();

        private RewardType[] m_Types = new RewardType[]
			{
				// Armors
				new RewardType( 200, typeof( RingmailGloves ), typeof( RingmailChest ), typeof( RingmailArms ), typeof( RingmailLegs ) ),
				new RewardType( 300, typeof( ChainCoif ), typeof( ChainLegs ), typeof( ChainChest ) ),
				new RewardType( 400, typeof( PlateArms ), typeof( PlateLegs ), typeof( PlateHelm ), typeof( PlateGorget ), typeof( PlateGloves ), typeof( PlateChest ) ),

				// Weapons
				new RewardType( 200, typeof( Bardiche ), typeof( Halberd ) ),
				new RewardType( 300, typeof( Dagger ), typeof( ShortSpear ), typeof( Spear ), typeof( WarFork ), typeof( Kryss ) ),	//OSI put the dagger in there.  Odd, ain't it.
				new RewardType( 350, typeof( Axe ), typeof( BattleAxe ), typeof( DoubleAxe ), typeof( ExecutionersAxe ), typeof( LargeBattleAxe ), typeof( TwoHandedAxe ) ),
				new RewardType( 350, typeof( Broadsword ), typeof( Cutlass ), typeof( Katana ), typeof( Longsword ), typeof( Scimitar ), /*typeof( ThinLongsword ),*/ typeof( VikingSword ) ),
				new RewardType( 350, typeof( WarAxe ), typeof( HammerPick ), typeof( Mace ), typeof( Maul ), typeof( WarHammer ), typeof( WarMace ) )
			};

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity == 10)
                points += 10;
            else if (quantity == 15)
                points += 25;
            else if (quantity == 20)
                points += 50;

            if (exceptional)
                points += 200;

            if (itemCount > 1)
                points += LookupTypePoints(m_Types, type);

            if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
                points += 200 + (50 * (material - BulkMaterialType.DullCopper));

            return points;
        }

        private static int[][][] m_GoldTable = new int[][][]
			{
				new int[][] // 1-part (regular)
				{
					new int[]{ 150, 250, 250, 400,  400,  750,  750, 1200, 1200 },
					new int[]{ 225, 375, 375, 600,  600, 1125, 1125, 1800, 1800 },
					new int[]{ 300, 500, 750, 800, 1050, 1500, 2250, 2400, 4000 }
				},
				new int[][] // 1-part (exceptional)
				{
					new int[]{ 250, 400,  400,  750,  750, 1500, 1500, 3000,  3000 },
					new int[]{ 375, 600,  600, 1125, 1125, 2250, 2250, 4500,  4500 },
					new int[]{ 500, 800, 1200, 1500, 2500, 3000, 6000, 6000, 12000 }
				},
				new int[][] // Ringmail (regular)
				{
					new int[]{ 3000,  5000,  5000,  7500,  7500, 10000, 10000, 15000, 15000 },
					new int[]{ 4500,  7500,  7500, 11250, 11500, 15000, 15000, 22500, 22500 },
					new int[]{ 6000, 10000, 15000, 15000, 20000, 20000, 30000, 30000, 50000 }
				},
				new int[][] // Ringmail (exceptional)
				{
					new int[]{  5000, 10000, 10000, 15000, 15000, 25000,  25000,  50000,  50000 },
					new int[]{  7500, 15000, 15000, 22500, 22500, 37500,  37500,  75000,  75000 },
					new int[]{ 10000, 20000, 30000, 30000, 50000, 50000, 100000, 100000, 200000 }
				},
				new int[][] // Chainmail (regular)
				{
					new int[]{ 4000,  7500,  7500, 10000, 10000, 15000, 15000, 25000,  25000 },
					new int[]{ 6000, 11250, 11250, 15000, 15000, 22500, 22500, 37500,  37500 },
					new int[]{ 8000, 15000, 20000, 20000, 30000, 30000, 50000, 50000, 100000 }
				},
				new int[][] // Chainmail (exceptional)
				{
					new int[]{  7500, 15000, 15000, 25000,  25000,  50000,  50000, 100000, 100000 },
					new int[]{ 11250, 22500, 22500, 37500,  37500,  75000,  75000, 150000, 150000 },
					new int[]{ 15000, 30000, 50000, 50000, 100000, 100000, 200000, 200000, 200000 }
				},
				new int[][] // Platemail (regular)
				{
					new int[]{  5000, 10000, 10000, 15000, 15000, 25000,  25000,  50000,  50000 },
					new int[]{  7500, 15000, 15000, 22500, 22500, 37500,  37500,  75000,  75000 },
					new int[]{ 10000, 20000, 30000, 30000, 50000, 50000, 100000, 100000, 200000 }
				},
				new int[][] // Platemail (exceptional)
				{
					new int[]{ 10000, 25000,  25000,  50000,  50000, 100000, 100000, 100000, 100000 },
					new int[]{ 15000, 37500,  37500,  75000,  75000, 150000, 150000, 150000, 150000 },
					new int[]{ 20000, 50000, 100000, 100000, 200000, 200000, 200000, 200000, 200000 }
				},
				new int[][] // 2-part weapons (regular)
				{
					new int[]{ 3000, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 4500, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 6000, 0, 0, 0, 0, 0, 0, 0, 0 }
				},
				new int[][] // 2-part weapons (exceptional)
				{
					new int[]{ 5000, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 7500, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 10000, 0, 0, 0, 0, 0, 0, 0, 0 }
				},
				new int[][] // 5-part weapons (regular)
				{
					new int[]{ 4000, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 6000, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 8000, 0, 0, 0, 0, 0, 0, 0, 0 }
				},
				new int[][] // 5-part weapons (exceptional)
				{
					new int[]{ 7500, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 11250, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 15000, 0, 0, 0, 0, 0, 0, 0, 0 }
				},
				new int[][] // 6-part weapons (regular)
				{
					new int[]{ 4000, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 6000, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 10000, 0, 0, 0, 0, 0, 0, 0, 0 }
				},
				new int[][] // 6-part weapons (exceptional)
				{
					new int[]{ 7500, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 11250, 0, 0, 0, 0, 0, 0, 0, 0 },
					new int[]{ 15000, 0, 0, 0, 0, 0, 0, 0, 0 }
				}
			};

        private int ComputeType(Type type, int itemCount)
        {
            // Item count of 1 means it's a small BOD.
            if (itemCount == 1)
                return 0;

            int typeIdx;

            // Loop through the RewardTypes defined earlier and find the correct one.
            for (typeIdx = 0; typeIdx < 7; ++typeIdx)
            {
                if (m_Types[typeIdx].Contains(type))
                    break;
            }

            // Types 5, 6 and 7 are Large Weapon BODs with the same rewards.
            if (typeIdx > 5)
                typeIdx = 5;

            return (typeIdx + 1) * 2;
        }

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int[][][] goldTable = m_GoldTable;

            int typeIndex = ComputeType(type, itemCount);
            int quanIndex = (quantity == 20 ? 2 : quantity == 15 ? 1 : 0);
            int mtrlIndex = (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite) ? 1 + (int)(material - BulkMaterialType.DullCopper) : 0;

            if (exceptional)
                typeIndex++;

            int gold = goldTable[typeIndex][quanIndex][mtrlIndex];

            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }

        public SmithRewardCalculator()
        {
            Groups = new RewardGroup[]
				{
					new RewardGroup(    0, new RewardItem( 1, SturdyShovel ) ),
					new RewardGroup(   25, new RewardItem( 1, SturdyPickaxe ) ),
					new RewardGroup(   50, new RewardItem( 45, SturdyShovel ), new RewardItem( 45, SturdyPickaxe ), new RewardItem( 10, MiningGloves, 1 ) ),
					new RewardGroup(  200, new RewardItem( 45, GargoylesPickaxe ), new RewardItem( 45, ProspectorsTool ), new RewardItem( 10, MiningGloves, 3 ) ),
					new RewardGroup(  400, new RewardItem( 2, GargoylesPickaxe ), new RewardItem( 2, ProspectorsTool ), new RewardItem( 1, PowderOfTemperament ) ),
					new RewardGroup(  450, new RewardItem( 9, PowderOfTemperament ), new RewardItem( 1, MiningGloves, 5 ) ),
					new RewardGroup(  500, new RewardItem( 1, RunicHammer, 1 ) ),
					new RewardGroup(  550, new RewardItem( 3, RunicHammer, 1 ), new RewardItem( 2, RunicHammer, 2 ) ),
					new RewardGroup(  600, new RewardItem( 1, RunicHammer, 2 ) ),
					new RewardGroup(  625, new RewardItem( 3, RunicHammer, 2 ), new RewardItem( 6, PowerScroll, 5 ), new RewardItem( 1, ColoredAnvil ) ),
					new RewardGroup(  650, new RewardItem( 1, RunicHammer, 3 ) ),
					new RewardGroup(  675, new RewardItem( 1, ColoredAnvil ), new RewardItem( 6, PowerScroll, 10 ), new RewardItem( 3, RunicHammer, 3 ) ),
					new RewardGroup(  700, new RewardItem( 1, RunicHammer, 4 ) ),
					new RewardGroup(  750, new RewardItem( 1, AncientHammer, 10 ) ),
					new RewardGroup(  800, new RewardItem( 1, PowerScroll, 15 ) ),
					new RewardGroup(  850, new RewardItem( 1, AncientHammer, 15 ) ),
					new RewardGroup(  900, new RewardItem( 1, PowerScroll, 20 ) ),
					new RewardGroup(  950, new RewardItem( 1, RunicHammer, 5 ) ),
					new RewardGroup( 1000, new RewardItem( 1, AncientHammer, 30 ) ),
					new RewardGroup( 1050, new RewardItem( 1, RunicHammer, 6 ) ),
					new RewardGroup( 1100, new RewardItem( 1, AncientHammer, 60 ) ),
					new RewardGroup( 1150, new RewardItem( 1, RunicHammer, 7 ) ),
					new RewardGroup( 1200, new RewardItem( 1, RunicHammer, 8 ) )
				};
        }
    }

    public sealed class TailorRewardCalculator : RewardCalculator
    {
        #region Constructors
        private static readonly ConstructCallback Cloth = new ConstructCallback(CreateCloth);
        private static readonly ConstructCallback Sandals = new ConstructCallback(CreateSandals);
        private static readonly ConstructCallback StretchedHide = new ConstructCallback(CreateStretchedHide);
        private static readonly ConstructCallback RunicKit = new ConstructCallback(CreateRunicKit);
        private static readonly ConstructCallback Tapestry = new ConstructCallback(CreateTapestry);
        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);
        private static readonly ConstructCallback BearRug = new ConstructCallback(CreateBearRug);
        private static readonly ConstructCallback ClothingBlessDeed = new ConstructCallback(CreateCBD);

        private static int[][] m_ClothHues = new int[][]
			{
				new int[]{ 0x483, 0x48C, 0x488, 0x48A },
				new int[]{ 0x495, 0x48B, 0x486, 0x485 },
				new int[]{ 0x48D, 0x490, 0x48E, 0x491 },
				new int[]{ 0x48F, 0x494, 0x484, 0x497 },
				new int[]{ 0x489, 0x47F, 0x482, 0x47E }
			};

        private static Item CreateCloth(int type)
        {
            if (type >= 0 && type < m_ClothHues.Length)
            {
                UncutCloth cloth = new UncutCloth(100);
                cloth.Hue = m_ClothHues[type][Utility.Random(m_ClothHues[type].Length)];
                return cloth;
            }

            throw new InvalidOperationException();
        }

        private static int[] m_SandalHues = new int[]
			{
				0x489, 0x47F, 0x482,
				0x47E, 0x48F, 0x494,
				0x484, 0x497
			};

        private static Item CreateSandals(int type)
        {
            return new Sandals(m_SandalHues[Utility.Random(m_SandalHues.Length)]);
        }

        private static Item CreateStretchedHide(int type)
        {
            switch (Utility.Random(4))
            {
                default:
                case 0: return new SmallStretchedHideEastDeed();
                case 1: return new SmallStretchedHideSouthDeed();
                case 2: return new MediumStretchedHideEastDeed();
                case 3: return new MediumStretchedHideSouthDeed();
            }
        }

        private static Item CreateTapestry(int type)
        {
            switch (Utility.Random(4))
            {
                default:
                case 0: return new LightFlowerTapestryEastDeed();
                case 1: return new LightFlowerTapestrySouthDeed();
                case 2: return new DarkFlowerTapestryEastDeed();
                case 3: return new DarkFlowerTapestrySouthDeed();
            }
        }

        private static Item CreateBearRug(int type)
        {
            switch (Utility.Random(4))
            {
                default:
                case 0: return new BrownBearRugEastDeed();
                case 1: return new BrownBearRugSouthDeed();
                case 2: return new PolarBearRugEastDeed();
                case 3: return new PolarBearRugSouthDeed();
            }
        }

        private static Item CreateRunicKit(int type)
        {
            if (type >= 1 && type <= 3)
                return new RunicSewingKit(CraftResource.RegularLeather + type, 60 - (type * 15));

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScroll(SkillName.Tailoring, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreateCBD(int type)
        {
            return new ClothingBlessDeed();
        }
        #endregion

        public static readonly TailorRewardCalculator Instance = new TailorRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity == 10)
                points += 10;
            else if (quantity == 15)
                points += 25;
            else if (quantity == 20)
                points += 50;

            if (exceptional)
                points += 100;

            if (itemCount == 4)
                points += 300;
            else if (itemCount == 5)
                points += 400;
            else if (itemCount == 6)
                points += 500;

            if (material == BulkMaterialType.Spined)
                points += 50;
            else if (material == BulkMaterialType.Horned)
                points += 100;
            else if (material == BulkMaterialType.Barbed)
                points += 150;

            return points;
        }

        private static int[][][] m_AosGoldTable = new int[][][]
			{
				new int[][] // 1-part (regular)
				{
					new int[]{ 150, 150, 300, 300 },
					new int[]{ 225, 225, 450, 450 },
					new int[]{ 300, 400, 600, 750 }
				},
				new int[][] // 1-part (exceptional)
				{
					new int[]{ 300, 300,  600,  600 },
					new int[]{ 450, 450,  900,  900 },
					new int[]{ 600, 750, 1200, 1800 }
				},
				new int[][] // 4-part (regular)
				{
					new int[]{  4000,  4000,  5000,  5000 },
					new int[]{  6000,  6000,  7500,  7500 },
					new int[]{  8000, 10000, 10000, 15000 }
				},
				new int[][] // 4-part (exceptional)
				{
					new int[]{  5000,  5000,  7500,  7500 },
					new int[]{  7500,  7500, 11250, 11250 },
					new int[]{ 10000, 15000, 15000, 20000 }
				},
				new int[][] // 5-part (regular)
				{
					new int[]{  5000,  5000,  7500,  7500 },
					new int[]{  7500,  7500, 11250, 11250 },
					new int[]{ 10000, 15000, 15000, 20000 }
				},
				new int[][] // 5-part (exceptional)
				{
					new int[]{  7500,  7500, 10000, 10000 },
					new int[]{ 11250, 11250, 15000, 15000 },
					new int[]{ 15000, 20000, 20000, 30000 }
				},
				new int[][] // 6-part (regular)
				{
					new int[]{  7500,  7500, 10000, 10000 },
					new int[]{ 11250, 11250, 15000, 15000 },
					new int[]{ 15000, 20000, 20000, 30000 }
				},
				new int[][] // 6-part (exceptional)
				{
					new int[]{ 10000, 10000, 15000, 15000 },
					new int[]{ 15000, 15000, 22500, 22500 },
					new int[]{ 20000, 30000, 30000, 50000 }
				}
			};

        private static int[][][] m_OldGoldTable = new int[][][]
			{
				new int[][] // 1-part (regular)
				{
					new int[]{ 150, 150, 300, 300 },
					new int[]{ 225, 225, 450, 450 },
					new int[]{ 300, 400, 600, 750 }
				},
				new int[][] // 1-part (exceptional)
				{
					new int[]{ 300, 300,  600,  600 },
					new int[]{ 450, 450,  900,  900 },
					new int[]{ 600, 750, 1200, 1800 }
				},
				new int[][] // 4-part (regular)
				{
					new int[]{  3000,  3000,  4000,  4000 },
					new int[]{  4500,  4500,  6000,  6000 },
					new int[]{  6000,  8000,  8000, 10000 }
				},
				new int[][] // 4-part (exceptional)
				{
					new int[]{  4000,  4000,  5000,  5000 },
					new int[]{  6000,  6000,  7500,  7500 },
					new int[]{  8000, 10000, 10000, 15000 }
				},
				new int[][] // 5-part (regular)
				{
					new int[]{  4000,  4000,  5000,  5000 },
					new int[]{  6000,  6000,  7500,  7500 },
					new int[]{  8000, 10000, 10000, 15000 }
				},
				new int[][] // 5-part (exceptional)
				{
					new int[]{  5000,  5000,  7500,  7500 },
					new int[]{  7500,  7500, 11250, 11250 },
					new int[]{ 10000, 15000, 15000, 20000 }
				},
				new int[][] // 6-part (regular)
				{
					new int[]{  5000,  5000,  7500,  7500 },
					new int[]{  7500,  7500, 11250, 11250 },
					new int[]{ 10000, 15000, 15000, 20000 }
				},
				new int[][] // 6-part (exceptional)
				{
					new int[]{  7500,  7500, 10000, 10000 },
					new int[]{ 11250, 11250, 15000, 15000 },
					new int[]{ 15000, 20000, 20000, 30000 }
				}
			};

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int[][][] goldTable = (Core.AOS ? m_AosGoldTable : m_OldGoldTable);

            int typeIndex = ((itemCount == 6 ? 3 : itemCount == 5 ? 2 : itemCount == 4 ? 1 : 0) * 2) + (exceptional ? 1 : 0);
            int quanIndex = (quantity == 20 ? 2 : quantity == 15 ? 1 : 0);
            int mtrlIndex = (material == BulkMaterialType.Barbed ? 3 : material == BulkMaterialType.Horned ? 2 : material == BulkMaterialType.Spined ? 1 : 0);

            int gold = goldTable[typeIndex][quanIndex][mtrlIndex];

            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }

        public TailorRewardCalculator()
        {
            Groups = new RewardGroup[]
				{
					new RewardGroup(   0, new RewardItem( 1, Cloth, 0 ) ),
					new RewardGroup(  50, new RewardItem( 1, Cloth, 1 ) ),
					new RewardGroup( 100, new RewardItem( 1, Cloth, 2 ) ),
					new RewardGroup( 150, new RewardItem( 9, Cloth, 3 ), new RewardItem( 1, Sandals ) ),
					new RewardGroup( 200, new RewardItem( 4, Cloth, 4 ), new RewardItem( 1, Sandals ) ),
					new RewardGroup( 300, new RewardItem( 1, StretchedHide ) ),
					new RewardGroup( 350, new RewardItem( 1, RunicKit, 1 ) ),
					new RewardGroup( 400, new RewardItem( 2, PowerScroll, 5 ), new RewardItem( 3, Tapestry ) ),
					new RewardGroup( 450, new RewardItem( 1, BearRug ) ),
					new RewardGroup( 500, new RewardItem( 1, PowerScroll, 10 ) ),
					new RewardGroup( 550, new RewardItem( 1, ClothingBlessDeed ) ),
					new RewardGroup( 575, new RewardItem( 1, PowerScroll, 15 ) ),
					new RewardGroup( 600, new RewardItem( 1, RunicKit, 2 ) ),
					new RewardGroup( 650, new RewardItem( 1, PowerScroll, 20 ) ),
					new RewardGroup( 700, new RewardItem( 1, RunicKit, 3 ) )
				};
        }
    }

    #endregion
}