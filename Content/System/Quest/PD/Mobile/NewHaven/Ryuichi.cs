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
    public class Ryuichi : BaseVendor
    {
        private List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1078155); // I can teach you Ninjitsu. The Art of Stealth.
        }

        [Constructable]
        public Ryuichi()
            : base("the Ninjitsu Instructor")
        {
            Name = "Ryuichi";
            Hue = 0x8403;

            SetSkill(SkillName.Hiding, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.Tracking, 120.0);
            SetSkill(SkillName.Fencing, 120.0);
            SetSkill(SkillName.Stealth, 120.0);
            SetSkill(SkillName.Ninjitsu, 120.0);
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBNinja());
        }

        public override bool GetGender()
        {
            return false;
        }

        public override void InitOutfit()
        {
            HairItemID = 0x203B;
            HairHue = 0x455;

            AddItem(new SamuraiTabi());
            AddItem(new LeatherNinjaPants());
            AddItem(new LeatherNinjaMitts());
            AddItem(new LeatherNinjaHood());
            AddItem(new LeatherNinjaJacket());
            AddItem(new LeatherNinjaBelt());

            PackGold(100, 200);
        }

        public Ryuichi(Serial serial)
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