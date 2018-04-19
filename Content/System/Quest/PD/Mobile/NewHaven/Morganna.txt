using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class Morganna : Mage
    {
        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078132); // Want to learn how to channel the supernatural?
        }

        [Constructable]
        public Morganna()
        {
            Name = "Morganna";
            Title = "the Spirit Speak Instructor";
            BodyValue = 0x191;
            Female = true;
            Hue = 0x83EA;
            HairItemID = 0x203C;
            HairHue = 0x455;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Magery, 120.0);
            SetSkill(SkillName.MagicResist, 120.0);
            SetSkill(SkillName.SpiritSpeak, 120.0);
            SetSkill(SkillName.Swords, 120.0);
            SetSkill(SkillName.Meditation, 120.0);
            SetSkill(SkillName.Necromancy, 120.0);
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Sandals());
            AddItem(new Robe(0x47D));
            AddItem(new SkullCap(0x455));
        }

        public Morganna(Serial serial)
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
}