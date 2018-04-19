using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Items;
using Server.Regions;

using BunnyHole = Server.Mobiles.VorpalBunny.BunnyHole;

namespace Server.Mobiles
{
    public class SummonedLavaSerpent : BaseTalismanSummon
    {
        [Constructable]
        public SummonedLavaSerpent()
            : base()
        {
            Name = "a lava serpent";
            Body = 90;
            BaseSoundID = 219;
        }

        public SummonedLavaSerpent(Serial serial)
            : base(serial)
        {
        }

        public override void OnThink()
        {
            /*
            if ( m_NextWave < DateTime.UtcNow )
                AreaHeatDamage();
            */
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }

        /*
        // An area attack that only damages staff, wtf?

        private DateTime m_NextWave;

        public void AreaHeatDamage()
        {
            Mobile mob = ControlMaster;

            if ( mob != null )
            {
                if ( mob.InRange( Location, 2 ) )
                {
                    if ( mob.AccessLevel != AccessLevel.Player )
                    {
                        AOS.Damage( mob, Utility.Random( 2, 3 ), 0, 100, 0, 0, 0 );
                        mob.SendLocalizedMessage( 1008112 ); // The intense heat is damaging you!
                    }
                }

                GuardedRegion r = Region as GuardedRegion;
				
                if ( r != null && mob.Alive )
                {
                    foreach ( Mobile m in GetMobilesInRange( 2 ) )
                    {
                        if ( !mob.CanBeHarmful( m ) )
                            mob.CriminalAction( false );
                    }
                }
            }

            m_NextWave = DateTime.UtcNow + TimeSpan.FromSeconds( 3 );
        }
        */
    }
}