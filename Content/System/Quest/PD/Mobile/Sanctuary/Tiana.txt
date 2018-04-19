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
    [QuesterName("Tiana (Sanctuary)")]
    public class Tiana : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074214, // Knave! Come here right now!
                1074218  // Hey!  I want to talk to you, now.
            ));
        }

        [Constructable]
        public Tiana()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Tiana";
            Title = "the guard";
            Race = Race.Elf;
            BodyValue = 0x25E;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new ElvenBoots());
            AddItem(new HidePants());
            AddItem(new HideFemaleChest());
            AddItem(new HidePauldrons());

            Item item;

            item = new WoodlandBelt();
            item.Hue = 0x673;
            AddItem(item);

            item = new RavenHelm();
            item.Hue = 0x443;
            AddItem(item);
        }

        public Tiana(Serial serial)
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