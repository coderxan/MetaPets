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
    public class AldenArmstrong : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078136); // There is an art to slaying your enemies swiftly. It's called tactics, and I can teach it to you.
        }

        [Constructable]
        public AldenArmstrong()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Alden Armstrong";
            Title = "the Tactics Instructor";
            BodyValue = 0x190;
            Hue = 0x83EA;
            HairItemID = 0x203B;
            HairHue = 0x44E;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Parry, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Swords, 120.0);
            SetSkill(SkillName.Focus, 120.0);

            AddItem(new Backpack());
            AddItem(new Shoes());
            AddItem(new StuddedLegs());
            AddItem(new StuddedGloves());
            AddItem(new StuddedGorget());
            AddItem(new StuddedChest());
            AddItem(new StuddedArms());
            AddItem(new Katana());
        }

        public AldenArmstrong(Serial serial)
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