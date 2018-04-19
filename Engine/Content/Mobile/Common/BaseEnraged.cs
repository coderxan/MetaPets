using System;

using Server;
using Server.Network;

namespace Server.Mobiles
{
    public class BaseEnraged : BaseCreature
    {
        public BaseEnraged(Mobile summoner)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            SetStr(50, 200);
            SetDex(50, 200);
            SetHits(50, 200);
            SetStam(50, 200);

            /* 
                On OSI, all stats are random 50-200, but
                str is never less than hits, and dex is never
                less than stam.
            */

            if (Str < Hits)
                Str = Hits;
            if (Dex < Stam)
                Dex = Stam;

            Karma = -1000;
            Tamable = false;

            SummonMaster = summoner;
        }

        public override void OnThink()
        {

            if (SummonMaster == null || SummonMaster.Deleted)
            {
                Delete();
            }

            /*
                On OSI, without combatant, they behave as if they have been
                given "come" command, ie they wander towards their summoner,
                but never actually "follow".
            */

            else if (!Combat(this))
            {
                if (AIObject != null)
                {
                    AIObject.MoveTo(SummonMaster, false, 5);
                }
            }

            /*
                On OSI, if the summon attacks a mobile, the summoner meer also
                attacks them, regardless of karma, etc. as long as the combatant
                is a player or controlled/summoned, and the summoner is not already
                engaged in combat.
            */

            else if (!Combat(SummonMaster))
            {
                BaseCreature bc = null;
                if (Combatant is BaseCreature)
                {
                    bc = (BaseCreature)Combatant;
                }
                if (Combatant.Player || (bc != null && (bc.Controlled || bc.SummonMaster != null)))
                {
                    SummonMaster.Combatant = Combatant;
                }
            }
            else
            {
                base.OnThink();
            }
        }

        private bool Combat(Mobile mobile)
        {
            Mobile combatant = mobile.Combatant;
            if (combatant == null || combatant.Deleted)
            {
                return false;
            }
            else if (combatant.IsDeadBondedPet || !combatant.Alive)
            {
                return false;
            }
            return true;
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);
            PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1060768, from.NetState); // enraged
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1060768); // enraged
        }

        public BaseEnraged(Serial serial)
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