using System;

using Server;

namespace Server.Factions
{
    // FactionExplosionTrap
    public class FactionExplosionTrap : BaseFactionTrap
    {
        public override int LabelNumber { get { return 1044599; } } // faction explosion trap

        public override int AttackMessage { get { return 1010543; } } // You are enveloped in an explosion of fire!
        public override int DisarmMessage { get { return 1010539; } } // You carefully remove the pressure trigger and disable the trap.
        public override int EffectSound { get { return 0x307; } }
        public override int MessageHue { get { return 0x78; } }

        public override AllowedPlacing AllowedPlacing { get { return AllowedPlacing.AnyFactionTown; } }

        public override void DoVisibleEffect()
        {
            Effects.SendLocationEffect(GetWorldLocation(), Map, 0x36BD, 15, 10);
        }

        public override void DoAttackEffect(Mobile m)
        {
            m.Damage(Utility.Dice(6, 10, 40), m);
        }

        [Constructable]
        public FactionExplosionTrap()
            : this(null)
        {
        }

        public FactionExplosionTrap(Faction f)
            : this(f, null)
        {
        }

        public FactionExplosionTrap(Faction f, Mobile m)
            : base(f, m, 0x11C1)
        {
        }

        public FactionExplosionTrap(Serial serial)
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

    public class FactionExplosionTrapDeed : BaseFactionTrapDeed
    {
        public override Type TrapType { get { return typeof(FactionExplosionTrap); } }
        public override int LabelNumber { get { return 1044603; } } // faction explosion trap deed

        public FactionExplosionTrapDeed()
            : base(0x36D2)
        {
        }

        public FactionExplosionTrapDeed(Serial serial)
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

    // FactionGasTrap
    public class FactionGasTrap : BaseFactionTrap
    {
        public override int LabelNumber { get { return 1044598; } } // faction gas trap

        public override int AttackMessage { get { return 1010542; } } // A noxious green cloud of poison gas envelops you!
        public override int DisarmMessage { get { return 502376; } } // The poison leaks harmlessly away due to your deft touch.
        public override int EffectSound { get { return 0x230; } }
        public override int MessageHue { get { return 0x44; } }

        public override AllowedPlacing AllowedPlacing { get { return AllowedPlacing.FactionStronghold; } }

        public override void DoVisibleEffect()
        {
            Effects.SendLocationEffect(this.Location, this.Map, 0x3709, 28, 10, 0x1D3, 5);
        }

        public override void DoAttackEffect(Mobile m)
        {
            m.ApplyPoison(m, Poison.Lethal);
        }

        [Constructable]
        public FactionGasTrap()
            : this(null)
        {
        }

        public FactionGasTrap(Faction f)
            : this(f, null)
        {
        }

        public FactionGasTrap(Faction f, Mobile m)
            : base(f, m, 0x113C)
        {
        }

        public FactionGasTrap(Serial serial)
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

    public class FactionGasTrapDeed : BaseFactionTrapDeed
    {
        public override Type TrapType { get { return typeof(FactionGasTrap); } }
        public override int LabelNumber { get { return 1044602; } } // faction gas trap deed

        public FactionGasTrapDeed()
            : base(0x11AB)
        {
        }

        public FactionGasTrapDeed(Serial serial)
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

    // FactionSawTrap
    public class FactionSawTrap : BaseFactionTrap
    {
        public override int LabelNumber { get { return 1041047; } } // faction saw trap

        public override int AttackMessage { get { return 1010544; } } // The blade cuts deep into your skin!
        public override int DisarmMessage { get { return 1010540; } } // You carefully dismantle the saw mechanism and disable the trap.
        public override int EffectSound { get { return 0x218; } }
        public override int MessageHue { get { return 0x5A; } }

        public override AllowedPlacing AllowedPlacing { get { return AllowedPlacing.ControlledFactionTown; } }

        public override void DoVisibleEffect()
        {
            Effects.SendLocationEffect(this.Location, this.Map, 0x11AD, 25, 10);
        }

        public override void DoAttackEffect(Mobile m)
        {
            m.Damage(Utility.Dice(6, 10, 40), m);
        }

        [Constructable]
        public FactionSawTrap()
            : this(null)
        {
        }

        public FactionSawTrap(Serial serial)
            : base(serial)
        {
        }

        public FactionSawTrap(Faction f)
            : this(f, null)
        {
        }

        public FactionSawTrap(Faction f, Mobile m)
            : base(f, m, 0x11AC)
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

    public class FactionSawTrapDeed : BaseFactionTrapDeed
    {
        public override Type TrapType { get { return typeof(FactionSawTrap); } }
        public override int LabelNumber { get { return 1044604; } } // faction saw trap deed

        public FactionSawTrapDeed()
            : base(0x1107)
        {
        }

        public FactionSawTrapDeed(Serial serial)
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

    // FactionSpikeTrap
    public class FactionSpikeTrap : BaseFactionTrap
    {
        public override int LabelNumber { get { return 1044601; } } // faction spike trap

        public override int AttackMessage { get { return 1010545; } } // Large spikes in the ground spring up piercing your skin!
        public override int DisarmMessage { get { return 1010541; } } // You carefully dismantle the trigger on the spikes and disable the trap.
        public override int EffectSound { get { return 0x22E; } }
        public override int MessageHue { get { return 0x5A; } }

        public override AllowedPlacing AllowedPlacing { get { return AllowedPlacing.ControlledFactionTown; } }

        public override void DoVisibleEffect()
        {
            Effects.SendLocationEffect(this.Location, this.Map, 0x11A4, 12, 6);
        }

        public override void DoAttackEffect(Mobile m)
        {
            m.Damage(Utility.Dice(6, 10, 40), m);
        }

        [Constructable]
        public FactionSpikeTrap()
            : this(null)
        {
        }

        public FactionSpikeTrap(Faction f)
            : this(f, null)
        {
        }

        public FactionSpikeTrap(Faction f, Mobile m)
            : base(f, m, 0x11A0)
        {
        }

        public FactionSpikeTrap(Serial serial)
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

    public class FactionSpikeTrapDeed : BaseFactionTrapDeed
    {
        public override Type TrapType { get { return typeof(FactionSpikeTrap); } }
        public override int LabelNumber { get { return 1044605; } } // faction spike trap deed

        public FactionSpikeTrapDeed()
            : base(0x11A5)
        {
        }

        public FactionSpikeTrapDeed(Serial serial)
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