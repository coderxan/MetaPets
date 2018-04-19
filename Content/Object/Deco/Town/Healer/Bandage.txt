using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Gumps
{
    public enum ResurrectMessage
    {
        ChaosShrine = 0,
        VirtueShrine = 1,
        Healer = 2,
        Generic = 3,
    }

    public class ResurrectGump : Gump
    {
        private Mobile m_Healer;
        private int m_Price;
        private bool m_FromSacrifice;
        private double m_HitsScalar;

        public ResurrectGump(Mobile owner)
            : this(owner, owner, ResurrectMessage.Generic, false)
        {
        }

        public ResurrectGump(Mobile owner, double hitsScalar)
            : this(owner, owner, ResurrectMessage.Generic, false, hitsScalar)
        {
        }

        public ResurrectGump(Mobile owner, bool fromSacrifice)
            : this(owner, owner, ResurrectMessage.Generic, fromSacrifice)
        {
        }

        public ResurrectGump(Mobile owner, Mobile healer)
            : this(owner, healer, ResurrectMessage.Generic, false)
        {
        }

        public ResurrectGump(Mobile owner, ResurrectMessage msg)
            : this(owner, owner, msg, false)
        {
        }

        public ResurrectGump(Mobile owner, Mobile healer, ResurrectMessage msg)
            : this(owner, healer, msg, false)
        {
        }

        public ResurrectGump(Mobile owner, Mobile healer, ResurrectMessage msg, bool fromSacrifice)
            : this(owner, healer, msg, fromSacrifice, 0.0)
        {
        }

        public ResurrectGump(Mobile owner, Mobile healer, ResurrectMessage msg, bool fromSacrifice, double hitsScalar)
            : base(100, 0)
        {
            m_Healer = healer;
            m_FromSacrifice = fromSacrifice;
            m_HitsScalar = hitsScalar;

            AddPage(0);

            AddBackground(0, 0, 400, 350, 2600);

            AddHtmlLocalized(0, 20, 400, 35, 1011022, false, false); // <center>Resurrection</center>

            AddHtmlLocalized(50, 55, 300, 140, 1011023 + (int)msg, true, true); /* It is possible for you to be resurrected here by this healer. Do you wish to try?<br>
																				   * CONTINUE - You chose to try to come back to life now.<br>
																				   * CANCEL - You prefer to remain a ghost for now.
																				   */

            AddButton(200, 227, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(235, 230, 110, 35, 1011012, false, false); // CANCEL

            AddButton(65, 227, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(100, 230, 110, 35, 1011011, false, false); // CONTINUE
        }

        public ResurrectGump(Mobile owner, Mobile healer, int price)
            : base(150, 50)
        {
            m_Healer = healer;
            m_Price = price;

            Closable = false;

            AddPage(0);

            AddImage(0, 0, 3600);

            AddImageTiled(0, 14, 15, 200, 3603);
            AddImageTiled(380, 14, 14, 200, 3605);

            AddImage(0, 201, 3606);

            AddImageTiled(15, 201, 370, 16, 3607);
            AddImageTiled(15, 0, 370, 16, 3601);

            AddImage(380, 0, 3602);

            AddImage(380, 201, 3608);

            AddImageTiled(15, 15, 365, 190, 2624);

            AddRadio(30, 140, 9727, 9730, true, 1);
            AddHtmlLocalized(65, 145, 300, 25, 1060015, 0x7FFF, false, false); // Grudgingly pay the money

            AddRadio(30, 175, 9727, 9730, false, 0);
            AddHtmlLocalized(65, 178, 300, 25, 1060016, 0x7FFF, false, false); // I'd rather stay dead, you scoundrel!!!

            AddHtmlLocalized(30, 20, 360, 35, 1060017, 0x7FFF, false, false); // Wishing to rejoin the living, are you?  I can restore your body... for a price of course...

            AddHtmlLocalized(30, 105, 345, 40, 1060018, 0x5B2D, false, false); // Do you accept the fee, which will be withdrawn from your bank?

            AddImage(65, 72, 5605);

            AddImageTiled(80, 90, 200, 1, 9107);
            AddImageTiled(95, 92, 200, 1, 9157);

            AddLabel(90, 70, 1645, price.ToString());
            AddHtmlLocalized(140, 70, 100, 25, 1023823, 0x7FFF, false, false); // gold coins

            AddButton(290, 175, 247, 248, 2, GumpButtonType.Reply, 0);

            AddImageTiled(15, 14, 365, 1, 9107);
            AddImageTiled(380, 14, 1, 190, 9105);
            AddImageTiled(15, 205, 365, 1, 9107);
            AddImageTiled(15, 14, 1, 190, 9105);
            AddImageTiled(0, 0, 395, 1, 9157);
            AddImageTiled(394, 0, 1, 217, 9155);
            AddImageTiled(0, 216, 395, 1, 9157);
            AddImageTiled(0, 0, 1, 217, 9155);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            from.CloseGump(typeof(ResurrectGump));

            if (info.ButtonID == 1 || info.ButtonID == 2)
            {
                if (from.Map == null || !from.Map.CanFit(from.Location, 16, false, false))
                {
                    from.SendLocalizedMessage(502391); // Thou can not be resurrected there!
                    return;
                }

                if (m_Price > 0)
                {
                    if (info.IsSwitched(1))
                    {
                        if (Banker.Withdraw(from, m_Price))
                        {
                            from.SendLocalizedMessage(1060398, m_Price.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                            from.SendLocalizedMessage(1060022, Banker.GetBalance(from).ToString()); // You have ~1_AMOUNT~ gold in cash remaining in your bank box.
                        }
                        else
                        {
                            from.SendLocalizedMessage(1060020); // Unfortunately, you do not have enough cash in your bank to cover the cost of the healing.
                            return;
                        }
                    }
                    else
                    {
                        from.SendLocalizedMessage(1060019); // You decide against paying the healer, and thus remain dead.
                        return;
                    }
                }

                from.PlaySound(0x214);
                from.FixedEffect(0x376A, 10, 16);

                from.Resurrect();

                if (m_Healer != null && from != m_Healer)
                {
                    VirtueLevel level = VirtueHelper.GetLevel(m_Healer, VirtueName.Compassion);

                    switch (level)
                    {
                        case VirtueLevel.Seeker: from.Hits = AOS.Scale(from.HitsMax, 20); break;
                        case VirtueLevel.Follower: from.Hits = AOS.Scale(from.HitsMax, 40); break;
                        case VirtueLevel.Knight: from.Hits = AOS.Scale(from.HitsMax, 80); break;
                    }
                }

                if (m_FromSacrifice && from is PlayerMobile)
                {
                    ((PlayerMobile)from).AvailableResurrects -= 1;

                    Container pack = from.Backpack;
                    Container corpse = from.Corpse;

                    if (pack != null && corpse != null)
                    {
                        List<Item> items = new List<Item>(corpse.Items);

                        for (int i = 0; i < items.Count; ++i)
                        {
                            Item item = items[i];

                            if (item.Layer != Layer.Hair && item.Layer != Layer.FacialHair && item.Movable)
                                pack.DropItem(item);
                        }
                    }
                }

                if (from.Fame > 0)
                {
                    int amount = from.Fame / 10;

                    Misc.Titles.AwardFame(from, -amount, true);
                }

                if (!Core.AOS && from.ShortTermMurders >= 5)
                {
                    double loss = (100.0 - (4.0 + (from.ShortTermMurders / 5.0))) / 100.0; // 5 to 15% loss

                    if (loss < 0.85)
                        loss = 0.85;
                    else if (loss > 0.95)
                        loss = 0.95;

                    if (from.RawStr * loss > 10)
                        from.RawStr = (int)(from.RawStr * loss);
                    if (from.RawInt * loss > 10)
                        from.RawInt = (int)(from.RawInt * loss);
                    if (from.RawDex * loss > 10)
                        from.RawDex = (int)(from.RawDex * loss);

                    for (int s = 0; s < from.Skills.Length; s++)
                    {
                        if (from.Skills[s].Base * loss > 35)
                            from.Skills[s].Base *= loss;
                    }
                }

                if (from.Alive && m_HitsScalar > 0)
                    from.Hits = (int)(from.HitsMax * m_HitsScalar);
            }
        }
    }

    public class PetResurrectGump : Gump
    {
        private BaseCreature m_Pet;
        private double m_HitsScalar;

        public PetResurrectGump(Mobile from, BaseCreature pet)
            : this(from, pet, 0.0)
        {
        }

        public PetResurrectGump(Mobile from, BaseCreature pet, double hitsScalar)
            : base(50, 50)
        {
            from.CloseGump(typeof(PetResurrectGump));

            m_Pet = pet;
            m_HitsScalar = hitsScalar;

            AddPage(0);

            AddBackground(10, 10, 265, 140, 0x242C);

            AddItem(205, 40, 0x4);
            AddItem(227, 40, 0x5);

            AddItem(180, 78, 0xCAE);
            AddItem(195, 90, 0xCAD);
            AddItem(218, 95, 0xCB0);

            AddHtmlLocalized(30, 30, 150, 75, 1049665, false, false); // <div align=center>Wilt thou sanctify the resurrection of:</div>
            AddHtml(30, 70, 150, 25, String.Format("<div align=CENTER>{0}</div>", pet.Name), true, false);

            AddButton(40, 105, 0x81A, 0x81B, 0x1, GumpButtonType.Reply, 0); // Okay
            AddButton(110, 105, 0x819, 0x818, 0x2, GumpButtonType.Reply, 0); // Cancel
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (m_Pet.Deleted || !m_Pet.IsBonded || !m_Pet.IsDeadPet)
                return;

            Mobile from = state.Mobile;

            if (info.ButtonID == 1)
            {
                if (m_Pet.Map == null || !m_Pet.Map.CanFit(m_Pet.Location, 16, false, false))
                {
                    from.SendLocalizedMessage(503256); // You fail to resurrect the creature.
                    return;
                }
                else if (m_Pet.Region != null && m_Pet.Region.IsPartOf("Khaldun"))	//TODO: Confirm for pets, as per Bandage's script.
                {
                    from.SendLocalizedMessage(1010395); // The veil of death in this area is too strong and resists thy efforts to restore life.
                    return;
                }

                m_Pet.PlaySound(0x214);
                m_Pet.FixedEffect(0x376A, 10, 16);
                m_Pet.ResurrectPet();

                double decreaseAmount;

                if (from == m_Pet.ControlMaster)
                    decreaseAmount = 0.1;
                else
                    decreaseAmount = 0.2;

                for (int i = 0; i < m_Pet.Skills.Length; ++i)	//Decrease all skills on pet.
                    m_Pet.Skills[i].Base -= decreaseAmount;

                if (!m_Pet.IsDeadPet && m_HitsScalar > 0)
                    m_Pet.Hits = (int)(m_Pet.HitsMax * m_HitsScalar);
            }

        }
    }
}

namespace Server.Items
{
    public class Bandage : Item, IDyable
    {
        public static int Range = (Core.AOS ? 2 : 1);

        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        public static void Initialize()
        {
            EventSink.BandageTargetRequest += new BandageTargetRequestEventHandler(EventSink_BandageTargetRequest);
        }

        [Constructable]
        public Bandage()
            : this(1)
        {
        }

        [Constructable]
        public Bandage(int amount)
            : base(0xE21)
        {
            Stackable = true;
            Amount = amount;
        }

        public Bandage(Serial serial)
            : base(serial)
        {
        }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;

            return true;
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

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), Range))
            {
                from.RevealingAction();

                from.SendLocalizedMessage(500948); // Who will you use the bandages on?

                from.Target = new InternalTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(500295); // You are too far away to do that.
            }
        }

        private static void EventSink_BandageTargetRequest(BandageTargetRequestEventArgs e)
        {
            Bandage b = e.Bandage as Bandage;

            if (b == null || b.Deleted)
                return;

            Mobile from = e.Mobile;

            if (from.InRange(b.GetWorldLocation(), Range))
            {
                Target t = from.Target;

                if (t != null)
                {
                    Target.Cancel(from);
                    from.Target = null;
                }

                from.RevealingAction();

                from.SendLocalizedMessage(500948); // Who will you use the bandages on?

                new InternalTarget(b).Invoke(from, e.Target);
            }
            else
            {
                from.SendLocalizedMessage(500295); // You are too far away to do that.
            }
        }

        private class InternalTarget : Target
        {
            private Bandage m_Bandage;

            public InternalTarget(Bandage bandage)
                : base(Bandage.Range, false, TargetFlags.Beneficial)
            {
                m_Bandage = bandage;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Bandage.Deleted)
                    return;

                if (targeted is Mobile)
                {
                    if (from.InRange(m_Bandage.GetWorldLocation(), Bandage.Range))
                    {
                        if (BandageContext.BeginHeal(from, (Mobile)targeted) != null)
                        {
                            if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                                m_Bandage.Consume();
                        }
                    }
                    else
                    {
                        from.SendLocalizedMessage(500295); // You are too far away to do that.
                    }
                }
                else if (targeted is PlagueBeastInnard)
                {
                    if (((PlagueBeastInnard)targeted).OnBandage(from))
                        m_Bandage.Consume();
                }
                else
                {
                    from.SendLocalizedMessage(500970); // Bandages can not be used on that.
                }
            }

            protected override void OnNonlocalTarget(Mobile from, object targeted)
            {
                if (targeted is PlagueBeastInnard)
                {
                    if (((PlagueBeastInnard)targeted).OnBandage(from))
                        m_Bandage.Consume();
                }
                else
                    base.OnNonlocalTarget(from, targeted);
            }
        }
    }

    public class BandageContext
    {
        private Mobile m_Healer;
        private Mobile m_Patient;
        private int m_Slips;
        private Timer m_Timer;

        public Mobile Healer { get { return m_Healer; } }
        public Mobile Patient { get { return m_Patient; } }
        public int Slips { get { return m_Slips; } set { m_Slips = value; } }
        public Timer Timer { get { return m_Timer; } }

        public void Slip()
        {
            m_Healer.SendLocalizedMessage(500961); // Your fingers slip!
            ++m_Slips;
        }

        public BandageContext(Mobile healer, Mobile patient, TimeSpan delay)
        {
            m_Healer = healer;
            m_Patient = patient;

            m_Timer = new InternalTimer(this, delay);
            m_Timer.Start();
        }

        public void StopHeal()
        {
            m_Table.Remove(m_Healer);

            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        private static Dictionary<Mobile, BandageContext> m_Table = new Dictionary<Mobile, BandageContext>();

        public static BandageContext GetContext(Mobile healer)
        {
            BandageContext bc = null;
            m_Table.TryGetValue(healer, out bc);
            return bc;
        }

        public static SkillName GetPrimarySkill(Mobile m)
        {
            if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
                return SkillName.Veterinary;
            else
                return SkillName.Healing;
        }

        public static SkillName GetSecondarySkill(Mobile m)
        {
            if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
                return SkillName.AnimalLore;
            else
                return SkillName.Anatomy;
        }

        public void EndHeal()
        {
            StopHeal();

            int healerNumber = -1, patientNumber = -1;
            bool playSound = true;
            bool checkSkills = false;

            SkillName primarySkill = GetPrimarySkill(m_Patient);
            SkillName secondarySkill = GetSecondarySkill(m_Patient);

            BaseCreature petPatient = m_Patient as BaseCreature;

            if (!m_Healer.Alive)
            {
                healerNumber = 500962; // You were unable to finish your work before you died.
                patientNumber = -1;
                playSound = false;
            }
            else if (!m_Healer.InRange(m_Patient, Bandage.Range))
            {
                healerNumber = 500963; // You did not stay close enough to heal your target.
                patientNumber = -1;
                playSound = false;
            }
            else if (!m_Patient.Alive || (petPatient != null && petPatient.IsDeadPet))
            {
                double healing = m_Healer.Skills[primarySkill].Value;
                double anatomy = m_Healer.Skills[secondarySkill].Value;
                double chance = ((healing - 68.0) / 50.0) - (m_Slips * 0.02);

                if (((checkSkills = (healing >= 80.0 && anatomy >= 80.0)) && chance > Utility.RandomDouble())
                      || (Core.SE && petPatient is Factions.FactionWarHorse && petPatient.ControlMaster == m_Healer))	//TODO: Dbl check doesn't check for faction of the horse here?
                {
                    if (m_Patient.Map == null || !m_Patient.Map.CanFit(m_Patient.Location, 16, false, false))
                    {
                        healerNumber = 501042; // Target can not be resurrected at that location.
                        patientNumber = 502391; // Thou can not be resurrected there!
                    }
                    else if (m_Patient.Region != null && m_Patient.Region.IsPartOf("Khaldun"))
                    {
                        healerNumber = 1010395; // The veil of death in this area is too strong and resists thy efforts to restore life.
                        patientNumber = -1;
                    }
                    else
                    {
                        healerNumber = 500965; // You are able to resurrect your patient.
                        patientNumber = -1;

                        m_Patient.PlaySound(0x214);
                        m_Patient.FixedEffect(0x376A, 10, 16);

                        if (petPatient != null && petPatient.IsDeadPet)
                        {
                            Mobile master = petPatient.ControlMaster;

                            if (master != null && m_Healer == master)
                            {
                                petPatient.ResurrectPet();

                                for (int i = 0; i < petPatient.Skills.Length; ++i)
                                {
                                    petPatient.Skills[i].Base -= 0.1;
                                }
                            }
                            else if (master != null && master.InRange(petPatient, 3))
                            {
                                healerNumber = 503255; // You are able to resurrect the creature.

                                master.CloseGump(typeof(PetResurrectGump));
                                master.SendGump(new PetResurrectGump(m_Healer, petPatient));
                            }
                            else
                            {
                                bool found = false;

                                List<Mobile> friends = petPatient.Friends;

                                for (int i = 0; friends != null && i < friends.Count; ++i)
                                {
                                    Mobile friend = friends[i];

                                    if (friend.InRange(petPatient, 3))
                                    {
                                        healerNumber = 503255; // You are able to resurrect the creature.

                                        friend.CloseGump(typeof(PetResurrectGump));
                                        friend.SendGump(new PetResurrectGump(m_Healer, petPatient));

                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                    healerNumber = 1049670; // The pet's owner must be nearby to attempt resurrection.
                            }
                        }
                        else
                        {
                            m_Patient.CloseGump(typeof(ResurrectGump));
                            m_Patient.SendGump(new ResurrectGump(m_Patient, m_Healer));
                        }
                    }
                }
                else
                {
                    if (petPatient != null && petPatient.IsDeadPet)
                        healerNumber = 503256; // You fail to resurrect the creature.
                    else
                        healerNumber = 500966; // You are unable to resurrect your patient.

                    patientNumber = -1;
                }
            }
            else if (m_Patient.Poisoned)
            {
                m_Healer.SendLocalizedMessage(500969); // You finish applying the bandages.

                double healing = m_Healer.Skills[primarySkill].Value;
                double anatomy = m_Healer.Skills[secondarySkill].Value;
                double chance = ((healing - 30.0) / 50.0) - (m_Patient.Poison.Level * 0.1) - (m_Slips * 0.02);

                if ((checkSkills = (healing >= 60.0 && anatomy >= 60.0)) && chance > Utility.RandomDouble())
                {
                    if (m_Patient.CurePoison(m_Healer))
                    {
                        healerNumber = (m_Healer == m_Patient) ? -1 : 1010058; // You have cured the target of all poisons.
                        patientNumber = 1010059; // You have been cured of all poisons.
                    }
                    else
                    {
                        healerNumber = -1;
                        patientNumber = -1;
                    }
                }
                else
                {
                    healerNumber = 1010060; // You have failed to cure your target!
                    patientNumber = -1;
                }
            }
            else if (BleedAttack.IsBleeding(m_Patient))
            {
                healerNumber = 1060088; // You bind the wound and stop the bleeding
                patientNumber = 1060167; // The bleeding wounds have healed, you are no longer bleeding!

                BleedAttack.EndBleed(m_Patient, false);
            }
            else if (MortalStrike.IsWounded(m_Patient))
            {
                healerNumber = (m_Healer == m_Patient ? 1005000 : 1010398);
                patientNumber = -1;
                playSound = false;
            }
            else if (m_Patient.Hits == m_Patient.HitsMax)
            {
                healerNumber = 500967; // You heal what little damage your patient had.
                patientNumber = -1;
            }
            else
            {
                checkSkills = true;
                patientNumber = -1;

                double healing = m_Healer.Skills[primarySkill].Value;
                double anatomy = m_Healer.Skills[secondarySkill].Value;
                double chance = ((healing + 10.0) / 100.0) - (m_Slips * 0.02);

                if (chance > Utility.RandomDouble())
                {
                    healerNumber = 500969; // You finish applying the bandages.

                    double min, max;

                    if (Core.AOS)
                    {
                        min = (anatomy / 8.0) + (healing / 5.0) + 4.0;
                        max = (anatomy / 6.0) + (healing / 2.5) + 4.0;
                    }
                    else
                    {
                        min = (anatomy / 5.0) + (healing / 5.0) + 3.0;
                        max = (anatomy / 5.0) + (healing / 2.0) + 10.0;
                    }

                    double toHeal = min + (Utility.RandomDouble() * (max - min));

                    if (m_Patient.Body.IsMonster || m_Patient.Body.IsAnimal)
                        toHeal += m_Patient.HitsMax / 100;

                    if (Core.AOS)
                        toHeal -= toHeal * m_Slips * 0.35; // TODO: Verify algorithm
                    else
                        toHeal -= m_Slips * 4;

                    if (toHeal < 1)
                    {
                        toHeal = 1;
                        healerNumber = 500968; // You apply the bandages, but they barely help.
                    }

                    m_Patient.Heal((int)toHeal, m_Healer, false);
                }
                else
                {
                    healerNumber = 500968; // You apply the bandages, but they barely help.
                    playSound = false;
                }
            }

            if (healerNumber != -1)
                m_Healer.SendLocalizedMessage(healerNumber);

            if (patientNumber != -1)
                m_Patient.SendLocalizedMessage(patientNumber);

            if (playSound)
                m_Patient.PlaySound(0x57);

            if (checkSkills)
            {
                m_Healer.CheckSkill(secondarySkill, 0.0, 120.0);
                m_Healer.CheckSkill(primarySkill, 0.0, 120.0);
            }
        }

        private class InternalTimer : Timer
        {
            private BandageContext m_Context;

            public InternalTimer(BandageContext context, TimeSpan delay)
                : base(delay)
            {
                m_Context = context;
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                m_Context.EndHeal();
            }
        }

        public static BandageContext BeginHeal(Mobile healer, Mobile patient)
        {
            bool isDeadPet = (patient is BaseCreature && ((BaseCreature)patient).IsDeadPet);

            if (patient is Golem)
            {
                healer.SendLocalizedMessage(500970); // Bandages cannot be used on that.
            }
            else if (patient is BaseCreature && ((BaseCreature)patient).IsAnimatedDead)
            {
                healer.SendLocalizedMessage(500951); // You cannot heal that.
            }
            else if (!patient.Poisoned && patient.Hits == patient.HitsMax && !BleedAttack.IsBleeding(patient) && !isDeadPet)
            {
                healer.SendLocalizedMessage(500955); // That being is not damaged!
            }
            else if (!patient.Alive && (patient.Map == null || !patient.Map.CanFit(patient.Location, 16, false, false)))
            {
                healer.SendLocalizedMessage(501042); // Target cannot be resurrected at that location.
            }
            else if (healer.CanBeBeneficial(patient, true, true))
            {
                healer.DoBeneficial(patient);

                bool onSelf = (healer == patient);
                int dex = healer.Dex;

                double seconds;
                double resDelay = (patient.Alive ? 0.0 : 5.0);

                if (onSelf)
                {
                    if (Core.AOS)
                        seconds = 5.0 + (0.5 * ((double)(120 - dex) / 10)); // TODO: Verify algorithm
                    else
                        seconds = 9.4 + (0.6 * ((double)(120 - dex) / 10));
                }
                else
                {
                    if (Core.AOS && GetPrimarySkill(patient) == SkillName.Veterinary)
                    {
                        seconds = 2.0;
                    }
                    else if (Core.AOS)
                    {
                        if (dex < 204)
                        {
                            seconds = 3.2 - (Math.Sin((double)dex / 130) * 2.5) + resDelay;
                        }
                        else
                        {
                            seconds = 0.7 + resDelay;
                        }
                    }
                    else
                    {
                        if (dex >= 100)
                            seconds = 3.0 + resDelay;
                        else if (dex >= 40)
                            seconds = 4.0 + resDelay;
                        else
                            seconds = 5.0 + resDelay;
                    }
                }

                BandageContext context = GetContext(healer);

                if (context != null)
                    context.StopHeal();
                seconds *= 1000;

                context = new BandageContext(healer, patient, TimeSpan.FromMilliseconds(seconds));

                m_Table[healer] = context;

                if (!onSelf)
                    patient.SendLocalizedMessage(1008078, false, healer.Name); //  : Attempting to heal you.

                healer.SendLocalizedMessage(500956); // You begin applying the bandages.
                return context;
            }

            return null;
        }
    }
}