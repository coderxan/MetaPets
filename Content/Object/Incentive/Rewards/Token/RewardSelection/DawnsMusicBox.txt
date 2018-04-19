using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Gumps
{
    public class DawnsMusicBoxGump : Gump
    {
        private DawnsMusicBox m_Box;

        public DawnsMusicBoxGump(DawnsMusicBox box)
            : base(60, 36)
        {
            m_Box = box;

            AddPage(0);

            AddBackground(0, 0, 273, 324, 0x13BE);
            AddImageTiled(10, 10, 253, 20, 0xA40);
            AddImageTiled(10, 40, 253, 244, 0xA40);
            AddImageTiled(10, 294, 253, 20, 0xA40);
            AddAlphaRegion(10, 10, 253, 304);
            AddButton(10, 294, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(45, 296, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL
            AddHtmlLocalized(14, 12, 273, 20, 1075130, 0x7FFF, false, false); // Choose a track to play

            int page = 1;
            int i, y = 49;

            AddPage(page);

            for (i = 0; i < m_Box.Tracks.Count; i++, y += 24)
            {
                DawnsMusicInfo info = DawnsMusicBox.GetInfo(m_Box.Tracks[i]);

                if (i > 0 && i % 10 == 0)
                {
                    AddButton(228, 294, 0xFA5, 0xFA6, 0, GumpButtonType.Page, page + 1);

                    AddPage(page + 1);
                    y = 49;

                    AddButton(193, 294, 0xFAE, 0xFAF, 0, GumpButtonType.Page, page);

                    page++;
                }

                if (info == null)
                    continue;

                AddButton(19, y, 0x845, 0x846, 100 + i, GumpButtonType.Reply, 0);
                AddHtmlLocalized(44, y - 2, 213, 20, info.Name, 0x7FFF, false, false);
            }

            if (i % 10 == 0)
            {
                AddButton(228, 294, 0xFA5, 0xFA6, 0, GumpButtonType.Page, page + 1);

                AddPage(page + 1);
                y = 49;

                AddButton(193, 294, 0xFAE, 0xFAF, 0, GumpButtonType.Page, page);
            }

            AddButton(19, y, 0x845, 0x846, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(44, y - 2, 213, 20, 1075207, 0x7FFF, false, false); // Stop Song
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Box == null || m_Box.Deleted)
                return;

            Mobile m = sender.Mobile;

            if (!m_Box.IsChildOf(m.Backpack) && !m_Box.IsLockedDown)
                m.SendLocalizedMessage(1061856); // You must have the item in your backpack or locked down in order to use it.
            else if (m_Box.IsLockedDown && !m_Box.HasAccces(m))
                m.SendLocalizedMessage(502691); // You must be the owner to use this.
            else if (info.ButtonID == 1)
                m_Box.EndMusic(m);
            else if (info.ButtonID >= 100 && info.ButtonID - 100 < m_Box.Tracks.Count)
                m_Box.PlayMusic(m, m_Box.Tracks[info.ButtonID - 100]);
        }
    }
}

namespace Server.Items
{
    public enum DawnsMusicRarity
    {
        Common,
        Uncommon,
        Rare,
    }

    public class DawnsMusicInfo
    {
        private int m_Name;

        public int Name
        {
            get { return m_Name; }
        }

        private DawnsMusicRarity m_Rarity;

        public DawnsMusicRarity Rarity
        {
            get { return m_Rarity; }
        }

        public DawnsMusicInfo(int name, DawnsMusicRarity rarity)
        {
            m_Name = name;
            m_Rarity = rarity;
        }
    }

    [Flipable(0x1053, 0x1054)]
    public class DawnsMusicGear : Item
    {
        public static DawnsMusicGear RandomCommon
        {
            get { return new DawnsMusicGear(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Common)); }
        }

        public static DawnsMusicGear RandomUncommon
        {
            get { return new DawnsMusicGear(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Uncommon)); }
        }

        public static DawnsMusicGear RandomRare
        {
            get { return new DawnsMusicGear(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Rare)); }
        }

        private MusicName m_Music;

        [CommandProperty(AccessLevel.GameMaster)]
        public MusicName Music
        {
            get { return m_Music; }
            set { m_Music = value; }
        }

        [Constructable]
        public DawnsMusicGear()
            : this(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Common))
        {
        }

        [Constructable]
        public DawnsMusicGear(MusicName music)
            : base(0x1053)
        {
            m_Music = music;

            Weight = 1.0;
        }

        public DawnsMusicGear(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            DawnsMusicInfo info = DawnsMusicBox.GetInfo(m_Music);

            if (info != null)
            {
                if (info.Rarity == DawnsMusicRarity.Common)
                    list.Add(1075204); // Gear for Dawn's Music Box (Common)
                else if (info.Rarity == DawnsMusicRarity.Uncommon)
                    list.Add(1075205); // Gear for Dawn's Music Box (Uncommon)
                else if (info.Rarity == DawnsMusicRarity.Rare)
                    list.Add(1075206); // Gear for Dawn's Music Box (Rare)

                list.Add(info.Name);
            }
            else
                base.AddNameProperty(list);
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.Target = new InternalTarget(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)1); // version

            writer.Write((int)m_Music);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_Music = (MusicName)reader.ReadInt();
                        break;
                    }
            }

            if (version == 0) // Music wasn't serialized in version 0, pick a new track of random rarity
            {
                DawnsMusicRarity rarity;
                double rand = Utility.RandomDouble();

                if (rand < 0.025)
                    rarity = DawnsMusicRarity.Rare;
                else if (rand < 0.225)
                    rarity = DawnsMusicRarity.Uncommon;
                else
                    rarity = DawnsMusicRarity.Common;

                m_Music = DawnsMusicBox.RandomTrack(rarity);
            }
        }

        public class InternalTarget : Target
        {
            private DawnsMusicGear m_Gear;

            public InternalTarget(DawnsMusicGear gear)
                : base(2, false, TargetFlags.None)
            {
                m_Gear = gear;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Gear == null || m_Gear.Deleted)
                    return;

                DawnsMusicBox box = targeted as DawnsMusicBox;

                if (box != null)
                {
                    if (!box.Tracks.Contains(m_Gear.Music))
                    {
                        box.Tracks.Add(m_Gear.Music);
                        box.InvalidateProperties();

                        m_Gear.Delete();

                        from.SendLocalizedMessage(1071961); // This song has been added to the musicbox.
                    }
                    else
                        from.SendLocalizedMessage(1071962); // This song track is already in the musicbox.
                }
                else
                    from.SendLocalizedMessage(1071964); // Gears can only be put into a musicbox.
            }
        }
    }

    [Flipable(0x2AF9, 0x2AFD)]
    public class DawnsMusicBox : Item, ISecurable
    {
        public override int LabelNumber { get { return 1075198; } } // Dawn’s Music Box

        private List<MusicName> m_Tracks;

        public List<MusicName> Tracks
        {
            get { return m_Tracks; }
        }

        private SecureLevel m_Level;

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        [Constructable]
        public DawnsMusicBox()
            : base(0x2AF9)
        {
            Weight = 1.0;

            m_Tracks = new List<MusicName>();

            while (m_Tracks.Count < 4)
            {
                MusicName name = RandomTrack(DawnsMusicRarity.Common);

                if (!m_Tracks.Contains(name))
                    m_Tracks.Add(name);
            }
        }

        public DawnsMusicBox(Serial serial)
            : base(serial)
        {
        }

        public override void OnAfterDuped(Item newItem)
        {
            DawnsMusicBox box = newItem as DawnsMusicBox;

            if (box == null)
                return;

            box.m_Tracks = new List<MusicName>();
            box.m_Tracks.AddRange(m_Tracks);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            int commonSongs = 0;
            int uncommonSongs = 0;
            int rareSongs = 0;

            for (int i = 0; i < m_Tracks.Count; i++)
            {
                DawnsMusicInfo info = GetInfo(m_Tracks[i]);

                switch (info.Rarity)
                {
                    case DawnsMusicRarity.Common: commonSongs++; break;
                    case DawnsMusicRarity.Uncommon: uncommonSongs++; break;
                    case DawnsMusicRarity.Rare: rareSongs++; break;
                }
            }

            if (commonSongs > 0)
                list.Add(1075234, commonSongs.ToString()); // ~1_NUMBER~ Common Tracks
            if (uncommonSongs > 0)
                list.Add(1075235, uncommonSongs.ToString()); // ~1_NUMBER~ Uncommon Tracks
            if (rareSongs > 0)
                list.Add(1075236, rareSongs.ToString()); // ~1_NUMBER~ Rare Tracks
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            SetSecureLevelEntry.AddTo(from, this, list); // Set secure level
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack) && !IsLockedDown)
                from.SendLocalizedMessage(1061856); // You must have the item in your backpack or locked down in order to use it.
            else if (IsLockedDown && !HasAccces(from))
                from.SendLocalizedMessage(502436); // That is not accessible.
            else
            {
                from.CloseGump(typeof(DawnsMusicBoxGump));
                from.SendGump(new DawnsMusicBoxGump(this));
            }
        }

        public bool HasAccces(Mobile m)
        {
            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            BaseHouse house = BaseHouse.FindHouseAt(this);

            return (house != null && house.HasAccess(m));
        }

        private Timer m_Timer;
        private int m_ItemID = 0;
        private int m_Count = 0;

        public void PlayMusic(Mobile m, MusicName music)
        {
            if (m_Timer != null && m_Timer.Running)
                EndMusic(m);
            else
                m_ItemID = ItemID;

            m.Send(new PlayMusic(music));
            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5), 4, new TimerCallback(Animate));
        }

        public void EndMusic(Mobile m)
        {
            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();

            m.Send(StopMusic.Instance);

            if (m_Count > 0)
                ItemID = m_ItemID;

            m_Count = 0;
        }

        private void Animate()
        {
            m_Count++;

            if (m_Count >= 4)
            {
                m_Count = 0;
                ItemID = m_ItemID;
            }
            else
                ItemID++;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((int)m_Tracks.Count);

            for (int i = 0; i < m_Tracks.Count; i++)
                writer.Write((int)m_Tracks[i]);

            writer.Write((int)m_Level);
            writer.Write((int)m_ItemID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            int count = reader.ReadInt();
            m_Tracks = new List<MusicName>();

            for (int i = 0; i < count; i++)
                m_Tracks.Add((MusicName)reader.ReadInt());

            m_Level = (SecureLevel)reader.ReadInt();
            m_ItemID = reader.ReadInt();
        }

        private static Dictionary<MusicName, DawnsMusicInfo> m_Info = new Dictionary<MusicName, DawnsMusicInfo>();

        public static void Initialize()
        {
            m_Info.Add(MusicName.Samlethe, new DawnsMusicInfo(1075152, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Sailing, new DawnsMusicInfo(1075163, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Britain2, new DawnsMusicInfo(1075145, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Britain1, new DawnsMusicInfo(1075144, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Bucsden, new DawnsMusicInfo(1075146, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Forest_a, new DawnsMusicInfo(1075161, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Cove, new DawnsMusicInfo(1075176, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Death, new DawnsMusicInfo(1075171, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Dungeon9, new DawnsMusicInfo(1075160, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Dungeon2, new DawnsMusicInfo(1075175, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Cave01, new DawnsMusicInfo(1075159, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Combat3, new DawnsMusicInfo(1075170, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Combat1, new DawnsMusicInfo(1075168, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Combat2, new DawnsMusicInfo(1075169, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Jhelom, new DawnsMusicInfo(1075147, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Linelle, new DawnsMusicInfo(1075185, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.LBCastle, new DawnsMusicInfo(1075148, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Minoc, new DawnsMusicInfo(1075150, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Moonglow, new DawnsMusicInfo(1075177, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Magincia, new DawnsMusicInfo(1075149, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Nujelm, new DawnsMusicInfo(1075174, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.BTCastle, new DawnsMusicInfo(1075173, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Tavern04, new DawnsMusicInfo(1075167, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Skarabra, new DawnsMusicInfo(1075154, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Stones2, new DawnsMusicInfo(1075143, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Serpents, new DawnsMusicInfo(1075153, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Taiko, new DawnsMusicInfo(1075180, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Tavern01, new DawnsMusicInfo(1075164, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Tavern02, new DawnsMusicInfo(1075165, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Tavern03, new DawnsMusicInfo(1075166, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.TokunoDungeon, new DawnsMusicInfo(1075179, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Trinsic, new DawnsMusicInfo(1075155, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.OldUlt01, new DawnsMusicInfo(1075142, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Ocllo, new DawnsMusicInfo(1075151, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Vesper, new DawnsMusicInfo(1075156, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Victory, new DawnsMusicInfo(1075172, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Mountn_a, new DawnsMusicInfo(1075162, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Wind, new DawnsMusicInfo(1075157, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Yew, new DawnsMusicInfo(1075158, DawnsMusicRarity.Common));
            m_Info.Add(MusicName.Zento, new DawnsMusicInfo(1075178, DawnsMusicRarity.Common));

            m_Info.Add(MusicName.GwennoConversation, new DawnsMusicInfo(1075131, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.DreadHornArea, new DawnsMusicInfo(1075181, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.ElfCity, new DawnsMusicInfo(1075182, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.GoodEndGame, new DawnsMusicInfo(1075132, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.GoodVsEvil, new DawnsMusicInfo(1075133, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.GreatEarthSerpents, new DawnsMusicInfo(1075134, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.GrizzleDungeon, new DawnsMusicInfo(1075186, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.Humanoids_U9, new DawnsMusicInfo(1075135, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.MelisandesLair, new DawnsMusicInfo(1075183, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.MinocNegative, new DawnsMusicInfo(1075136, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.ParoxysmusLair, new DawnsMusicInfo(1075184, DawnsMusicRarity.Uncommon));
            m_Info.Add(MusicName.Paws, new DawnsMusicInfo(1075137, DawnsMusicRarity.Uncommon));

            m_Info.Add(MusicName.SelimsBar, new DawnsMusicInfo(1075138, DawnsMusicRarity.Rare));
            m_Info.Add(MusicName.SerpentIsleCombat_U7, new DawnsMusicInfo(1075139, DawnsMusicRarity.Rare));
            m_Info.Add(MusicName.ValoriaShips, new DawnsMusicInfo(1075140, DawnsMusicRarity.Rare));
        }

        public static MusicName[] m_CommonTracks = new MusicName[]
		{
			MusicName.Samlethe,	MusicName.Sailing,	MusicName.Britain2,			MusicName.Britain1,
			MusicName.Bucsden,	MusicName.Forest_a,	MusicName.Cove,				MusicName.Death,
			MusicName.Dungeon9,	MusicName.Dungeon2,	MusicName.Cave01,			MusicName.Combat3,
			MusicName.Combat1,	MusicName.Combat2,	MusicName.Jhelom,			MusicName.Linelle,
			MusicName.LBCastle,	MusicName.Minoc,	MusicName.Moonglow,			MusicName.Magincia,
			MusicName.Nujelm,	MusicName.BTCastle,	MusicName.Tavern04,			MusicName.Skarabra,
			MusicName.Stones2,	MusicName.Serpents,	MusicName.Taiko,			MusicName.Tavern01,
			MusicName.Tavern02,	MusicName.Tavern03,	MusicName.TokunoDungeon,	MusicName.Trinsic,
			MusicName.OldUlt01,	MusicName.Ocllo,	MusicName.Vesper,			MusicName.Victory,
			MusicName.Mountn_a,	MusicName.Wind,		MusicName.Yew,				MusicName.Zento
		};

        public static MusicName[] m_UncommonTracks = new MusicName[]
		{
			MusicName.GwennoConversation,	MusicName.DreadHornArea,	MusicName.ElfCity,
			MusicName.GoodEndGame,			MusicName.GoodVsEvil,		MusicName.GreatEarthSerpents,
			MusicName.GrizzleDungeon,		MusicName.Humanoids_U9,		MusicName.MelisandesLair,
			MusicName.MinocNegative,		MusicName.ParoxysmusLair,	MusicName.Paws
		};

        public static MusicName[] m_RareTracks = new MusicName[]
		{
			MusicName.SelimsBar,		MusicName.SerpentIsleCombat_U7,	MusicName.ValoriaShips
		};

        public static DawnsMusicInfo GetInfo(MusicName name)
        {
            if (m_Info.ContainsKey(name))
                return m_Info[name];

            return null;
        }

        public static MusicName RandomTrack(DawnsMusicRarity rarity)
        {
            MusicName[] list = null;

            switch (rarity)
            {
                default:
                case DawnsMusicRarity.Common: list = m_CommonTracks; break;
                case DawnsMusicRarity.Uncommon: list = m_UncommonTracks; break;
                case DawnsMusicRarity.Rare: list = m_RareTracks; break;
            }

            return list[Utility.Random(list.Length)];
        }
    }

    public sealed class StopMusic : Packet
    {
        public static readonly Packet Instance = Packet.SetStatic(new StopMusic());

        public StopMusic()
            : base(0x6D, 3)
        {
            m_Stream.Write((short)0x1FFF);
        }
    }
}