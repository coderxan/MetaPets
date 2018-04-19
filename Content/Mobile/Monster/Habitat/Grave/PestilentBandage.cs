﻿using System;
using System.Collections;

using Server.Items;
using Server.Misc;
using Server.Targeting;

namespace Server.Mobiles
{
    [CorpseName("a pestilent bandage corpse")]
    public class PestilentBandage : BaseCreature
    {
        [Constructable]
        public PestilentBandage()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a pestilent bandage";
            Body = 154;
            Hue = 0x515;
            BaseSoundID = 471;

            SetStr(691, 740);
            SetDex(141, 180);
            SetInt(51, 80);

            SetHits(415, 445);

            SetDamage(13, 23);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Cold, 20);
            SetDamageType(ResistanceType.Poison, 40);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.Poisoning, 0.0, 10.0);
            SetSkill(SkillName.Anatomy, 0);
            SetSkill(SkillName.MagicResist, 75.0, 80.0);
            SetSkill(SkillName.Tactics, 80.0, 85.0);
            SetSkill(SkillName.Wrestling, 70.0, 75.0);

            Fame = 20000;
            Karma = -20000;

            VirtualArmor = 28;

            PackItem(new Bandage(5));
        }

        public override Poison HitPoison { get { return Poison.Greater; } }
        public override bool CanHeal { get { return true; } }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);  // Need to verify
        }

        public PestilentBandage(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}