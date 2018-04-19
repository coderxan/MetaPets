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
    public class Chiyo : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078165); // To be undetected means you cannot be harmed.
        }

        [Constructable]
        public Chiyo()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Chiyo";
            Title = "the Hiding Instructor";
            BodyValue = 0xF7;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Hiding, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Tracking, 120.0);
            SetSkill(SkillName.Fencing, 120.0);
            SetSkill(SkillName.Stealth, 120.0);
            SetSkill(SkillName.Ninjitsu, 120.0);
        }

        public Chiyo(Serial serial)
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