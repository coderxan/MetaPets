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
    public class Mulcivikh : Mage
    {
        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078131); // Allured by dark magic, aren't you?
        }

        [Constructable]
        public Mulcivikh()
        {
            Name = "Mulcivikh";
            Title = "the Necromancy Instructor";
            BodyValue = 0x190;
            Hue = 0x83EA;
            HairItemID = 0x203D;
            HairHue = 0x457;

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
            AddItem(new Sandals(0x8FD));
            AddItem(new BoneHelm());

            Item item;

            item = new LeatherLegs();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherGloves();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherGorget();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherChest();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherArms();
            item.Hue = 0x2C3;
            AddItem(item);
        }

        public Mulcivikh(Serial serial)
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