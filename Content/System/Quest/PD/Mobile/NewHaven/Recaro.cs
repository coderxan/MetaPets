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
    public class Recaro : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078187); // The art of fencing requires a dexterous hand, a quick wit and fleet feet.
        }

        [Constructable]
        public Recaro()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Recaro";
            Title = "the Fencer Instructor";
            BodyValue = 0x190;
            Hue = 0x83EA;
            HairItemID = 0x203C;
            HairHue = 0x455;
            FacialHairItemID = 0x204D;
            FacialHairHue = 0x455;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Parry, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Fencing, 120.0);
            SetSkill(SkillName.Focus, 120.0);

            AddItem(new Backpack());
            AddItem(new Shoes(0x455));
            AddItem(new WarFork());

            Item item;

            item = new StuddedLegs();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedGloves();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedGorget();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedChest();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedArms();
            item.Hue = 0x455;
            AddItem(item);
        }

        public Recaro(Serial serial)
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