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
    public class Dimethro : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078128); // You there! Wanna master hand to hand defense? Of course you do!
        }

        [Constructable]
        public Dimethro()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Dimethro";
            Title = "the Wrestling Instructor";
            BodyValue = 0x190;
            Hue = 0x83EA;
            HairItemID = 0x203D;
            HairHue = 0x455;
            FacialHairItemID = 0x204D;
            FacialHairHue = 0x455;

            InitStats(100, 100, 25);

            SetSkill(SkillName.EvalInt, 120.0);
            SetSkill(SkillName.Inscribe, 120.0);
            SetSkill(SkillName.Magery, 120.0);
            SetSkill(SkillName.MagicResist, 120.0);
            SetSkill(SkillName.Wrestling, 120.0);
            SetSkill(SkillName.Meditation, 120.0);

            AddItem(new Backpack());
            AddItem(new Sandals(0x455));
            AddItem(new BodySash(0x455));
            AddItem(new LongPants(0x455));
        }

        public Dimethro(Serial serial)
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