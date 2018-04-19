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
    public class Churchill : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078141); // Don't listen to Jockles. Real warriors wield mace weapons!
        }

        [Constructable]
        public Churchill()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Churchill";
            Title = "the Mace Fighting Instructor";
            BodyValue = 0x190;
            Hue = 0x83EA;
            HairItemID = 0x203C;
            HairHue = 0x455;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Parry, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Macing, 120.0);
            SetSkill(SkillName.Focus, 120.0);

            AddItem(new Backpack());
            AddItem(new OrderShield());
            AddItem(new WarMace());

            Item item;

            item = new PlateLegs();
            item.Hue = 0x966;
            AddItem(item);

            item = new PlateGloves();
            item.Hue = 0x966;
            AddItem(item);

            item = new PlateGorget();
            item.Hue = 0x966;
            AddItem(item);

            item = new PlateChest();
            item.Hue = 0x966;
            AddItem(item);

            item = new PlateArms();
            item.Hue = 0x966;
            AddItem(item);
        }

        public Churchill(Serial serial)
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