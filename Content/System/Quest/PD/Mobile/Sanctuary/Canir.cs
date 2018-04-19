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
    [QuesterName("Canir (Sanctuary)")]
    public class Canir : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }
        public override bool CanTeach { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, Utility.RandomList(
                1074193, // You there! Yes you. Stop looking about like a toadie and come here.
                1074186  // Come here, I have a task.
            ));
        }

        [Constructable]
        public Canir()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Canir";
            Title = "the thaumaturgist";
            Race = Race.Elf;
            BodyValue = 0x25E;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.Focus, 60.0, 80.0);

            AddItem(new Sandals(0x1BB));
            AddItem(new FemaleElvenRobe(0x5A7));
            AddItem(new GemmedCirclet());
            AddItem(new MagicWand());
        }

        public Canir(Serial serial)
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