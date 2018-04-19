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
    public class Robyn : Bowyer
    {
        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078202); // Archery requires a steady aim and dexterous fingers.
        }

        [Constructable]
        public Robyn()
        {
            Name = "Robyn";
            Title = "the Archery Instructor";
            BodyValue = 0x191;
            Hue = 0x83EA;
            HairItemID = 0x203C;
            HairHue = 0x47D;
            Female = true;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Parry, 120.0);
            SetSkill(SkillName.Fletching, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Archery, 120.0);
            SetSkill(SkillName.Focus, 120.0);

        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Boots(0x592));
            AddItem(new Cloak(0x592));
            AddItem(new Bandana(0x592));
            AddItem(new CompositeBow());

            Item item;

            item = new StuddedLegs();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedGloves();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedGorget();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedChest();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedArms();
            item.Hue = 0x592;
            AddItem(item);
        }

        public Robyn(Serial serial)
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