using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Items;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Master Gnosos (Bedlam)")]
    public class MasterGnosos : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1074186); // Come here, I have a task.
        }

        [Constructable]
        public MasterGnosos()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Master Gnosos";
            Title = "the necromancer";
            Race = Race.Human;
            BodyValue = 0x190;
            Female = false;
            Hue = 0x83E8;
            InitStats(100, 100, 25);

            HairItemID = 0x2049;
            FacialHairItemID = 0x204B;

            AddItem(new Backpack());
            AddItem(new Shoes(0x485));
            AddItem(new Robe(0x497));

            SetSkill(SkillName.EvalInt, 60.0, 80.0);
            SetSkill(SkillName.Inscribe, 60.0, 80.0);
            SetSkill(SkillName.MagicResist, 60.0, 80.0);
            SetSkill(SkillName.SpiritSpeak, 60.0, 80.0);
            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Necromancy, 60.0, 80.0);
        }

        public MasterGnosos(Serial serial)
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