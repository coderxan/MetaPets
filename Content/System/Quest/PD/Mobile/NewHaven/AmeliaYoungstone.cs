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
    public class AmeliaYoungstone : Tinker
    {
        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078123); // Tinkering is very useful for a blacksmith. You can make your own tools.
        }

        [Constructable]
        public AmeliaYoungstone()
        {
            Name = "Amelia Youngstone";
            Title = "the Tinkering Instructor";
            BodyValue = 0x191;
            Female = true;
            Hue = 0x83EA;
            HairItemID = 0x203D;
            HairHue = 0x46C;

            InitStats(100, 100, 25);

            SetSkill(SkillName.ArmsLore, 120.0);
            SetSkill(SkillName.Blacksmith, 120.0);
            SetSkill(SkillName.Magery, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Swords, 120.0);
            SetSkill(SkillName.Tinkering, 120.0);
            SetSkill(SkillName.Mining, 120.0);
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Sandals());
            AddItem(new Doublet());
            AddItem(new ShortPants());
            AddItem(new HalfApron(0x8AB));
        }

        public AmeliaYoungstone(Serial serial)
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