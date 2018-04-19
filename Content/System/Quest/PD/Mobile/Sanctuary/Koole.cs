using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Koole (Sanctuary)")]
    public class Koole : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074186, // Come here, I have a task.
                1074218  // Hey!  I want to talk to you, now.
            ));
        }

        [Constructable]
        public Koole()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2.0)
        {
            Name = "Koole";
            Title = "the arcanist";
            Race = Race.Elf;
            Body = 605;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            Item item;

            item = new LeafChest();
            item.Hue = 443;
            AddItem(item);

            item = new LeafArms();
            item.Hue = 443;
            AddItem(item);

            AddItem(new LeafTonlet());
            AddItem(new ThighBoots(Utility.RandomAnimalHue()));
            AddItem(new RoyalCirclet());
        }

        public Koole(Serial serial)
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