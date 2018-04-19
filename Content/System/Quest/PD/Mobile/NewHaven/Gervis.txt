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
    public class Gervis : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074205, // Oh great adventurer, would you please assist a weak soul in need of aid?
                1074213, // Hey buddy.  Looking for work?
                1074211 // I could use some help.
            ));
        }

        [Constructable]
        public Gervis()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Gervis";
            Title = "the blacksmith trainer";
            Race = Race.Human;
            BodyValue = 0x190;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Blacksmith, 60.0, 80.0);

            AddItem(new Backpack());
            AddItem(new Boots(0x3B3));
            AddItem(new ShortPants(0x1BB));
            AddItem(new Doublet(0x652));
            AddItem(new SmithHammer());

            Item item;

            item = new LeatherGloves();
            item.Hue = 0x3B2;
            AddItem(item);
        }

        public Gervis(Serial serial)
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