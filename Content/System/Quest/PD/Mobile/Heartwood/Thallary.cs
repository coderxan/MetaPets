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
    [QuesterName("Thallary (The Heartwood)")]
    public class Thallary : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074221, // Greetings!  I have a small task for you good traveler.
                1074212 // *yawn* You busy?
            ));
        }

        [Constructable]
        public Thallary()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Thallary";
            Title = "the cloth weaver";
            Race = Race.Elf;
            BodyValue = 0x25D;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new Sandals(0x901));
            AddItem(new LongPants(0x72E));
            AddItem(new Cloak(0x3B3));
            AddItem(new FancyShirt(0x13));

        }

        public Thallary(Serial serial)
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