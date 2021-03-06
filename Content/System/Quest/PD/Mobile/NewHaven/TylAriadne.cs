﻿using System;
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
    public class TylAriadne : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078140); // Want to learn how to parry blows?
        }

        [Constructable]
        public TylAriadne()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Tyl Ariadne";
            Title = "the Parrying Instructor";
            BodyValue = 0x190;
            Hue = 0x8374;
            HairItemID = 0;

            InitStats(100, 100, 25);

            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Parry, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Swords, 120.0);
            SetSkill(SkillName.Meditation, 120.0);
            SetSkill(SkillName.Focus, 120.0);

            AddItem(new Backpack());
            AddItem(new ElvenBoots(0x96D));

            Item item;

            item = new StuddedLegs();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedGloves();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedGorget();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedChest();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedArms();
            item.Hue = 0x96D;
            AddItem(item);

            item = new DiamondMace();
            item.Hue = 0x96D;
            AddItem(item);
        }

        public TylAriadne(Serial serial)
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