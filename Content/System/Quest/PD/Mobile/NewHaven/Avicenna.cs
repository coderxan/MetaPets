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
    public class Avicenna : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078137); // A warrior needs to learn how to apply bandages to wounds.
        }

        [Constructable]
        public Avicenna()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Avicenna";
            Title = "the Healing Instructor";
            BodyValue = 0x190;
            Hue = 0x83EA;
            HairItemID = 0x203B;
            HairHue = 0x477;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Parry, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Swords, 120.0);
            SetSkill(SkillName.Focus, 120.0);

            AddItem(new Backpack());
            AddItem(new Robe(0x66D));
            AddItem(new Boots());
            AddItem(new GnarledStaff());
        }

        public Avicenna(Serial serial)
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