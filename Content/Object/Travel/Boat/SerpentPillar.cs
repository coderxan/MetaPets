﻿using System;
using System.Net;
using System.Text;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Items
{
    public class SerpentPillar : Item
    {
        private bool m_Active;
        private string m_Word;
        private Rectangle2D m_Destination;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get { return m_Active; }
            set { m_Active = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Word
        {
            get { return m_Word; }
            set { m_Word = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }

        [Constructable]
        public SerpentPillar()
            : this(null, new Rectangle2D(), false)
        {
        }

        public SerpentPillar(string word, Rectangle2D destination)
            : this(word, destination, true)
        {
        }

        public SerpentPillar(string word, Rectangle2D destination, bool active)
            : base(0x233F)
        {
            Movable = false;

            m_Active = active;
            m_Word = word;
            m_Destination = destination;
        }

        public override bool HandlesOnSpeech { get { return true; } }

        public override void OnSpeech(SpeechEventArgs e)
        {
            Mobile from = e.Mobile;

            if (!e.Handled && from.InRange(this, 10) && e.Speech.ToLower() == this.Word)
            {
                BaseBoat boat = BaseBoat.FindBoatAt(from, from.Map);

                if (boat == null)
                    return;

                if (!this.Active)
                {
                    if (boat.TillerMan != null)
                        boat.TillerMan.Say(502507); // Ar, Legend has it that these pillars are inactive! No man knows how it might be undone!

                    return;
                }

                Map map = from.Map;

                for (int i = 0; i < 5; i++) // Try 5 times
                {
                    int x = Utility.Random(Destination.X, Destination.Width);
                    int y = Utility.Random(Destination.Y, Destination.Height);
                    int z = map.GetAverageZ(x, y);

                    Point3D dest = new Point3D(x, y, z);

                    if (boat.CanFit(dest, map, boat.ItemID))
                    {
                        int xOffset = x - boat.X;
                        int yOffset = y - boat.Y;
                        int zOffset = z - boat.Z;

                        boat.Teleport(xOffset, yOffset, zOffset);

                        return;
                    }
                }

                if (boat.TillerMan != null)
                    boat.TillerMan.Say(502508); // Ar, I refuse to take that matey through here!
            }
        }

        public SerpentPillar(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((bool)m_Active);
            writer.Write((string)m_Word);
            writer.Write((Rectangle2D)m_Destination);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Active = reader.ReadBool();
            m_Word = reader.ReadString();
            m_Destination = reader.ReadRect2D();
        }
    }

    public class SerpentPillarAddon : BaseAddon
    {
        [Constructable]
        public SerpentPillarAddon()
        {
            AddComponent(new AddonComponent(9020), -2, +1, 0);
            AddComponent(new AddonComponent(9021), -2, +0, 0);
            AddComponent(new AddonComponent(9022), -2, -1, 0);
            AddComponent(new AddonComponent(9023), -2, -2, 0);
            AddComponent(new AddonComponent(9024), -1, -2, 0);
            AddComponent(new AddonComponent(9025), +0, -2, 0);
            AddComponent(new AddonComponent(9026), +1, -2, 0);

            AddComponent(new AddonComponent(9027), -1, +1, 0);
            AddComponent(new AddonComponent(9028), -1, +0, 0);
            AddComponent(new AddonComponent(9029), -1, -1, 0);

            AddComponent(new AddonComponent(9030), +0, +1, 0);
            AddComponent(new AddonComponent(9031), +0, +0, 0);
            AddComponent(new AddonComponent(9032), +0, -1, 0);

            AddComponent(new AddonComponent(9033), +1, +0, 0);
            AddComponent(new AddonComponent(9034), +1, -1, 0);
        }

        public SerpentPillarAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((byte)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadByte();
        }
    }
}