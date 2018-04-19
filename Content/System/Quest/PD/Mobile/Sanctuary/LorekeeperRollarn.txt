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
    [QuesterName("Rollarn (Sanctuary)")]
    public class LorekeeperRollarn : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074196, // Excuse me! I’m sorry to interrupt but I urgently need some assistance.
                1074197  // Pardon me, but if you could spare some time I’d greatly appreciate it.
            ));
        }

        [Constructable]
        public LorekeeperRollarn()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Lorekeeper Rollarn";
            Title = "the keeper of tradition";
            Race = Race.Elf;
            BodyValue = 0x25D;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new Sandals(0x1BB));
            AddItem(new Cloak(0x296));
            AddItem(new Circlet());
            AddItem(new LeafChest());

            Item item;

            item = new LeafLegs();
            item.Hue = 0x71A;
            AddItem(item);
        }

        public LorekeeperRollarn(Serial serial)
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