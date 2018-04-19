using System;
using System.Text;

using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Misc
{
    public class TestCenter
    {
        private const bool m_Enabled = false;
        public static bool Enabled { get { return m_Enabled; } }

        public static void Initialize()
        {
            // Register our speech handler
            if (Enabled)
                EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
        }

        private static void EventSink_Speech(SpeechEventArgs args)
        {
            if (!args.Handled)
            {
                if (Insensitive.StartsWith(args.Speech, "set"))
                {
                    Mobile from = args.Mobile;

                    string[] split = args.Speech.Split(' ');

                    if (split.Length == 3)
                    {
                        try
                        {
                            string name = split[1];
                            double value = Convert.ToDouble(split[2]);

                            if (Insensitive.Equals(name, "str"))
                                ChangeStrength(from, (int)value);
                            else if (Insensitive.Equals(name, "dex"))
                                ChangeDexterity(from, (int)value);
                            else if (Insensitive.Equals(name, "int"))
                                ChangeIntelligence(from, (int)value);
                            else
                                ChangeSkill(from, name, value);
                        }
                        catch
                        {
                        }
                    }
                }
                else if (Insensitive.Equals(args.Speech, "help"))
                {
                    args.Mobile.SendGump(new TCHelpGump());

                    args.Handled = true;
                }
            }
        }

        private static void ChangeStrength(Mobile from, int value)
        {
            if (value < 10 || value > 125)
            {
                from.SendLocalizedMessage(1005628); // Stats range between 10 and 125.
            }
            else
            {
                if ((value + from.RawDex + from.RawInt) > from.StatCap)
                {
                    from.SendLocalizedMessage(1005629); // You can not exceed the stat cap.  Try setting another stat lower first.
                }
                else
                {
                    from.RawStr = value;
                    from.SendLocalizedMessage(1005630); // Your stats have been adjusted.
                }
            }
        }

        private static void ChangeDexterity(Mobile from, int value)
        {
            if (value < 10 || value > 125)
            {
                from.SendLocalizedMessage(1005628); // Stats range between 10 and 125.
            }
            else
            {
                if ((from.RawStr + value + from.RawInt) > from.StatCap)
                {
                    from.SendLocalizedMessage(1005629); // You can not exceed the stat cap.  Try setting another stat lower first.
                }
                else
                {
                    from.RawDex = value;
                    from.SendLocalizedMessage(1005630); // Your stats have been adjusted.
                }
            }
        }

        private static void ChangeIntelligence(Mobile from, int value)
        {
            if (value < 10 || value > 125)
            {
                from.SendLocalizedMessage(1005628); // Stats range between 10 and 125.
            }
            else
            {
                if ((from.RawStr + from.RawDex + value) > from.StatCap)
                {
                    from.SendLocalizedMessage(1005629); // You can not exceed the stat cap.  Try setting another stat lower first.
                }
                else
                {
                    from.RawInt = value;
                    from.SendLocalizedMessage(1005630); // Your stats have been adjusted.
                }
            }
        }

        private static void ChangeSkill(Mobile from, string name, double value)
        {
            SkillName index;

            if (!Enum.TryParse(name, true, out index) || (!Core.SE && (int)index > 51) || (!Core.AOS && (int)index > 48))
            {
                from.SendLocalizedMessage(1005631); // You have specified an invalid skill to set.
                return;
            }

            Skill skill = from.Skills[index];

            if (skill != null)
            {
                if (value < 0 || value > skill.Cap)
                {
                    from.SendMessage(String.Format("Your skill in {0} is capped at {1:F1}.", skill.Info.Name, skill.Cap));
                }
                else
                {
                    int newFixedPoint = (int)(value * 10.0);
                    int oldFixedPoint = skill.BaseFixedPoint;

                    if (((skill.Owner.Total - oldFixedPoint) + newFixedPoint) > skill.Owner.Cap)
                    {
                        from.SendMessage("You can not exceed the skill cap.  Try setting another skill lower first.");
                    }
                    else
                    {
                        skill.BaseFixedPoint = newFixedPoint;
                    }
                }
            }
            else
            {
                from.SendLocalizedMessage(1005631); // You have specified an invalid skill to set.
            }
        }


        public class TCHelpGump : Gump
        {
            public TCHelpGump()
                : base(40, 40)
            {
                AddPage(0);
                AddBackground(0, 0, 160, 120, 5054);

                AddButton(10, 10, 0xFB7, 0xFB9, 1, GumpButtonType.Reply, 0);
                AddLabel(45, 10, 0x34, "RunUO");

                AddButton(10, 35, 0xFB7, 0xFB9, 2, GumpButtonType.Reply, 0);
                AddLabel(45, 35, 0x34, "List of skills");

                AddButton(10, 60, 0xFB7, 0xFB9, 3, GumpButtonType.Reply, 0);
                AddLabel(45, 60, 0x34, "Command list");

                AddButton(10, 85, 0xFB1, 0xFB3, 0, GumpButtonType.Reply, 0);
                AddLabel(45, 85, 0x34, "Close");
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                switch (info.ButtonID)
                {
                    case 1: // RunUO
                        {
                            sender.LaunchBrowser("https://github.com/runuo/");
                            break;
                        }
                    case 2: // List of skills
                        {
                            string[] strings = Enum.GetNames(typeof(SkillName));

                            Array.Sort(strings);

                            StringBuilder sb = new StringBuilder();

                            if (strings.Length > 0)
                                sb.Append(strings[0]);

                            for (int i = 1; i < strings.Length; ++i)
                            {
                                string v = strings[i];

                                if ((sb.Length + 1 + v.Length) >= 256)
                                {
                                    sender.Send(new AsciiMessage(Server.Serial.MinusOne, -1, MessageType.Label, 0x35, 3, "System", sb.ToString()));
                                    sb = new StringBuilder();
                                    sb.Append(v);
                                }
                                else
                                {
                                    sb.Append(' ');
                                    sb.Append(v);
                                }
                            }

                            if (sb.Length > 0)
                            {
                                sender.Send(new AsciiMessage(Server.Serial.MinusOne, -1, MessageType.Label, 0x35, 3, "System", sb.ToString()));
                            }

                            break;
                        }
                    case 3: // Command list
                        {
                            sender.Mobile.SendAsciiMessage(0x482, "The command prefix is \"{0}\"", CommandSystem.Prefix);
                            CommandHandlers.Help_OnCommand(new CommandEventArgs(sender.Mobile, "help", "", new string[0]));

                            break;
                        }
                }
            }
        }
    }
}

namespace Server.Mobiles
{
    /// <summary>
    /// This is a customizable test dummy engine that you can experiment with in your TestCenter: 
    /// 
    ///     [x] You can set its stats in-game!
    ///     [x] It will die after 5 minutes so your test center stays clean!
    ///     [x] You can create a macro to help your creation 
    ///         [>] Example: [add Dummy 1 15 7 -1 0.5 2 
    ///     [x] An iTeam of negative will set it to a random faction
    ///     [x] Say 'Kill' if you want this mobile to die
    ///     
    /// </summary>

    #region Generic Test Dummy

    public class Dummy : BaseCreature
    {
        public Timer m_Timer;

        [Constructable]
        public Dummy(AIType iAI, FightMode iFightMode, int iRangePerception, int iRangeFight, double dActiveSpeed, double dPassiveSpeed)
            : base(iAI, iFightMode, iRangePerception, iRangeFight, dActiveSpeed, dPassiveSpeed)
        {
            this.Body = 400 + Utility.Random(2);
            this.Hue = Utility.RandomSkinHue();

            this.Skills[SkillName.DetectHidden].Base = 100;
            this.Skills[SkillName.MagicResist].Base = 120;

            Team = Utility.Random(3);

            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            Utility.AssignRandomHair(this, iHue);

            LeatherGloves glv = new LeatherGloves();
            glv.Hue = iHue;
            glv.LootType = LootType.Newbied;
            AddItem(glv);

            Container pack = new Backpack();

            pack.Movable = false;

            AddItem(pack);

            m_Timer = new AutokillTimer(this);
            m_Timer.Start();
        }

        public Dummy(Serial serial)
            : base(serial)
        {
            m_Timer = new AutokillTimer(this);
            m_Timer.Start();
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

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            return base.HandlesOnSpeech(from);
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            if (e.Mobile.AccessLevel >= AccessLevel.GameMaster)
            {
                if (e.Speech == "kill")
                {
                    m_Timer.Stop();
                    m_Timer.Delay = TimeSpan.FromSeconds(Utility.Random(1, 5));
                    m_Timer.Start();
                }
            }
        }

        public override void OnTeamChange()
        {
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            Item item = FindItemOnLayer(Layer.OuterTorso);

            if (item != null)
                item.Hue = jHue;

            item = FindItemOnLayer(Layer.Helm);

            if (item != null)
                item.Hue = iHue;

            item = FindItemOnLayer(Layer.Gloves);

            if (item != null)
                item.Hue = iHue;

            item = FindItemOnLayer(Layer.Shoes);

            if (item != null)
                item.Hue = iHue;

            HairHue = iHue;

            item = FindItemOnLayer(Layer.MiddleTorso);

            if (item != null)
                item.Hue = iHue;

            item = FindItemOnLayer(Layer.OuterLegs);

            if (item != null)
                item.Hue = iHue;
        }

        private class AutokillTimer : Timer
        {
            private Dummy m_Owner;

            public AutokillTimer(Dummy owner)
                : base(TimeSpan.FromMinutes(5.0))
            {
                m_Owner = owner;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Owner.Kill();
                Stop();
            }
        }
    }

    #endregion

    #region Test Dummy Mobiles

    public class DummyAssassin : Dummy
    {
        [Constructable]
        public DummyAssassin()
            : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Hybrid Assassin
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(105, 105, 105);
            this.Skills[SkillName.Magery].Base = 120;
            this.Skills[SkillName.EvalInt].Base = 120;
            this.Skills[SkillName.Swords].Base = 120;
            this.Skills[SkillName.Tactics].Base = 120;
            this.Skills[SkillName.Meditation].Base = 120;
            this.Skills[SkillName.Poisoning].Base = 100;

            // Name
            this.Name = "Hybrid Assassin";

            // Equip
            Spellbook book = new Spellbook();
            book.Movable = false;
            book.LootType = LootType.Newbied;
            book.Content = 0xFFFFFFFFFFFFFFFF;
            AddToBackpack(book);

            Katana kat = new Katana();
            kat.Movable = false;
            kat.LootType = LootType.Newbied;
            kat.Crafter = this;
            kat.Poison = Poison.Deadly;
            kat.PoisonCharges = 12;
            kat.Quality = WeaponQuality.Regular;
            AddToBackpack(kat);

            LeatherArms lea = new LeatherArms();
            lea.Movable = false;
            lea.LootType = LootType.Newbied;
            lea.Crafter = this;
            lea.Quality = ArmorQuality.Regular;
            AddItem(lea);

            LeatherChest lec = new LeatherChest();
            lec.Movable = false;
            lec.LootType = LootType.Newbied;
            lec.Crafter = this;
            lec.Quality = ArmorQuality.Regular;
            AddItem(lec);

            LeatherGorget leg = new LeatherGorget();
            leg.Movable = false;
            leg.LootType = LootType.Newbied;
            leg.Crafter = this;
            leg.Quality = ArmorQuality.Regular;
            AddItem(leg);

            LeatherLegs lel = new LeatherLegs();
            lel.Movable = false;
            lel.LootType = LootType.Newbied;
            lel.Crafter = this;
            lel.Quality = ArmorQuality.Regular;
            AddItem(lel);

            Sandals snd = new Sandals();
            snd.Hue = iHue;
            snd.LootType = LootType.Newbied;
            AddItem(snd);

            Cap cap = new Cap();
            cap.Hue = iHue;
            AddItem(cap);

            Robe robe = new Robe();
            robe.Hue = iHue;
            AddItem(robe);

            DeadlyPoisonPotion pota = new DeadlyPoisonPotion();
            pota.LootType = LootType.Newbied;
            AddToBackpack(pota);

            DeadlyPoisonPotion potb = new DeadlyPoisonPotion();
            potb.LootType = LootType.Newbied;
            AddToBackpack(potb);

            DeadlyPoisonPotion potc = new DeadlyPoisonPotion();
            potc.LootType = LootType.Newbied;
            AddToBackpack(potc);

            DeadlyPoisonPotion potd = new DeadlyPoisonPotion();
            potd.LootType = LootType.Newbied;
            AddToBackpack(potd);

            Bandage band = new Bandage(50);
            AddToBackpack(band);

        }

        public DummyAssassin(Serial serial)
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

    public class DummyFence : Dummy
    {
        [Constructable]
        public DummyFence()
            : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Fencer
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(125, 125, 90);
            this.Skills[SkillName.Fencing].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 120;
            this.Skills[SkillName.Healing].Base = 120;
            this.Skills[SkillName.Tactics].Base = 120;

            // Name
            this.Name = "Fencer";

            // Equip
            Spear ssp = new Spear();
            ssp.Movable = true;
            ssp.Crafter = this;
            ssp.Quality = WeaponQuality.Regular;
            AddItem(ssp);

            Boots snd = new Boots();
            snd.Hue = iHue;
            snd.LootType = LootType.Newbied;
            AddItem(snd);

            ChainChest cht = new ChainChest();
            cht.Movable = false;
            cht.LootType = LootType.Newbied;
            cht.Crafter = this;
            cht.Quality = ArmorQuality.Regular;
            AddItem(cht);

            ChainLegs chl = new ChainLegs();
            chl.Movable = false;
            chl.LootType = LootType.Newbied;
            chl.Crafter = this;
            chl.Quality = ArmorQuality.Regular;
            AddItem(chl);

            PlateArms pla = new PlateArms();
            pla.Movable = false;
            pla.LootType = LootType.Newbied;
            pla.Crafter = this;
            pla.Quality = ArmorQuality.Regular;
            AddItem(pla);

            Bandage band = new Bandage(50);
            AddToBackpack(band);
        }

        public DummyFence(Serial serial)
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

    public class DummyHealer : Dummy
    {
        [Constructable]
        public DummyHealer()
            : base(AIType.AI_Healer, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Healer Mage
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(125, 125, 125);
            this.Skills[SkillName.Magery].Base = 120;
            this.Skills[SkillName.EvalInt].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 120;
            this.Skills[SkillName.Wrestling].Base = 120;
            this.Skills[SkillName.Meditation].Base = 120;
            this.Skills[SkillName.Healing].Base = 100;

            // Name
            this.Name = "Healer";

            // Equip
            Spellbook book = new Spellbook();
            book.Movable = false;
            book.LootType = LootType.Newbied;
            book.Content = 0xFFFFFFFFFFFFFFFF;
            AddItem(book);

            LeatherArms lea = new LeatherArms();
            lea.Movable = false;
            lea.LootType = LootType.Newbied;
            lea.Crafter = this;
            lea.Quality = ArmorQuality.Regular;
            AddItem(lea);

            LeatherChest lec = new LeatherChest();
            lec.Movable = false;
            lec.LootType = LootType.Newbied;
            lec.Crafter = this;
            lec.Quality = ArmorQuality.Regular;
            AddItem(lec);

            LeatherGorget leg = new LeatherGorget();
            leg.Movable = false;
            leg.LootType = LootType.Newbied;
            leg.Crafter = this;
            leg.Quality = ArmorQuality.Regular;
            AddItem(leg);

            LeatherLegs lel = new LeatherLegs();
            lel.Movable = false;
            lel.LootType = LootType.Newbied;
            lel.Crafter = this;
            lel.Quality = ArmorQuality.Regular;
            AddItem(lel);

            Sandals snd = new Sandals();
            snd.Hue = iHue;
            snd.LootType = LootType.Newbied;
            AddItem(snd);

            Cap cap = new Cap();
            cap.Hue = iHue;
            AddItem(cap);

            Robe robe = new Robe();
            robe.Hue = iHue;
            AddItem(robe);

        }

        public DummyHealer(Serial serial)
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

    public class DummyMace : Dummy
    {
        [Constructable]
        public DummyMace()
            : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Macer
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(125, 125, 90);
            this.Skills[SkillName.Macing].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 120;
            this.Skills[SkillName.Healing].Base = 120;
            this.Skills[SkillName.Tactics].Base = 120;


            // Name
            this.Name = "Macer";

            // Equip
            WarHammer war = new WarHammer();
            war.Movable = true;
            war.Crafter = this;
            war.Quality = WeaponQuality.Regular;
            AddItem(war);

            Boots bts = new Boots();
            bts.Hue = iHue;
            AddItem(bts);

            ChainChest cht = new ChainChest();
            cht.Movable = false;
            cht.LootType = LootType.Newbied;
            cht.Crafter = this;
            cht.Quality = ArmorQuality.Regular;
            AddItem(cht);

            ChainLegs chl = new ChainLegs();
            chl.Movable = false;
            chl.LootType = LootType.Newbied;
            chl.Crafter = this;
            chl.Quality = ArmorQuality.Regular;
            AddItem(chl);

            PlateArms pla = new PlateArms();
            pla.Movable = false;
            pla.LootType = LootType.Newbied;
            pla.Crafter = this;
            pla.Quality = ArmorQuality.Regular;
            AddItem(pla);

            Bandage band = new Bandage(50);
            AddToBackpack(band);
        }

        public DummyMace(Serial serial)
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

    public class DummyNox : Dummy
    {
        [Constructable]
        public DummyNox()
            : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
        {

            // A Dummy Nox or Pure Mage
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(90, 90, 125);
            this.Skills[SkillName.Magery].Base = 120;
            this.Skills[SkillName.EvalInt].Base = 120;
            this.Skills[SkillName.Inscribe].Base = 100;
            this.Skills[SkillName.Wrestling].Base = 120;
            this.Skills[SkillName.Meditation].Base = 120;
            this.Skills[SkillName.Poisoning].Base = 100;


            // Name
            this.Name = "Nox Mage";

            // Equip
            Spellbook book = new Spellbook();
            book.Movable = false;
            book.LootType = LootType.Newbied;
            book.Content = 0xFFFFFFFFFFFFFFFF;
            AddItem(book);

            Kilt kilt = new Kilt();
            kilt.Hue = jHue;
            AddItem(kilt);

            Sandals snd = new Sandals();
            snd.Hue = iHue;
            snd.LootType = LootType.Newbied;
            AddItem(snd);

            SkullCap skc = new SkullCap();
            skc.Hue = iHue;
            AddItem(skc);

            // Spells
            AddSpellAttack(typeof(Spells.First.MagicArrowSpell));
            AddSpellAttack(typeof(Spells.First.WeakenSpell));
            AddSpellAttack(typeof(Spells.Third.FireballSpell));
            AddSpellDefense(typeof(Spells.Third.WallOfStoneSpell));
            AddSpellDefense(typeof(Spells.First.HealSpell));
        }

        public DummyNox(Serial serial)
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

    public class DummyStun : Dummy
    {

        [Constructable]
        public DummyStun()
            : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
        {

            // A Dummy Stun Mage
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(90, 90, 125);
            this.Skills[SkillName.Magery].Base = 100;
            this.Skills[SkillName.EvalInt].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 80;
            this.Skills[SkillName.Wrestling].Base = 80;
            this.Skills[SkillName.Meditation].Base = 100;
            this.Skills[SkillName.Poisoning].Base = 100;


            // Name
            this.Name = "Stun Mage";

            // Equip
            Spellbook book = new Spellbook();
            book.Movable = false;
            book.LootType = LootType.Newbied;
            book.Content = 0xFFFFFFFFFFFFFFFF;
            AddItem(book);

            LeatherArms lea = new LeatherArms();
            lea.Movable = false;
            lea.LootType = LootType.Newbied;
            lea.Crafter = this;
            lea.Quality = ArmorQuality.Regular;
            AddItem(lea);

            LeatherChest lec = new LeatherChest();
            lec.Movable = false;
            lec.LootType = LootType.Newbied;
            lec.Crafter = this;
            lec.Quality = ArmorQuality.Regular;
            AddItem(lec);

            LeatherGorget leg = new LeatherGorget();
            leg.Movable = false;
            leg.LootType = LootType.Newbied;
            leg.Crafter = this;
            leg.Quality = ArmorQuality.Regular;
            AddItem(leg);

            LeatherLegs lel = new LeatherLegs();
            lel.Movable = false;
            lel.LootType = LootType.Newbied;
            lel.Crafter = this;
            lel.Quality = ArmorQuality.Regular;
            AddItem(lel);

            Boots bts = new Boots();
            bts.Hue = iHue;
            AddItem(bts);

            Cap cap = new Cap();
            cap.Hue = iHue;
            AddItem(cap);

            // Spells
            AddSpellAttack(typeof(Spells.First.MagicArrowSpell));
            AddSpellAttack(typeof(Spells.First.WeakenSpell));
            AddSpellAttack(typeof(Spells.Third.FireballSpell));
            AddSpellDefense(typeof(Spells.Third.WallOfStoneSpell));
            AddSpellDefense(typeof(Spells.First.HealSpell));
        }

        public DummyStun(Serial serial)
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

    public class DummySuper : Dummy
    {

        [Constructable]
        public DummySuper()
            : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Super Mage
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(125, 125, 125);
            this.Skills[SkillName.Magery].Base = 120;
            this.Skills[SkillName.EvalInt].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 120;
            this.Skills[SkillName.Wrestling].Base = 120;
            this.Skills[SkillName.Meditation].Base = 120;
            this.Skills[SkillName.Poisoning].Base = 100;
            this.Skills[SkillName.Inscribe].Base = 100;

            // Name
            this.Name = "Super Mage";

            // Equip
            Spellbook book = new Spellbook();
            book.Movable = false;
            book.LootType = LootType.Newbied;
            book.Content = 0xFFFFFFFFFFFFFFFF;
            AddItem(book);

            LeatherArms lea = new LeatherArms();
            lea.Movable = false;
            lea.LootType = LootType.Newbied;
            lea.Crafter = this;
            lea.Quality = ArmorQuality.Regular;
            AddItem(lea);

            LeatherChest lec = new LeatherChest();
            lec.Movable = false;
            lec.LootType = LootType.Newbied;
            lec.Crafter = this;
            lec.Quality = ArmorQuality.Regular;
            AddItem(lec);

            LeatherGorget leg = new LeatherGorget();
            leg.Movable = false;
            leg.LootType = LootType.Newbied;
            leg.Crafter = this;
            leg.Quality = ArmorQuality.Regular;
            AddItem(leg);

            LeatherLegs lel = new LeatherLegs();
            lel.Movable = false;
            lel.LootType = LootType.Newbied;
            lel.Crafter = this;
            lel.Quality = ArmorQuality.Regular;
            AddItem(lel);

            Sandals snd = new Sandals();
            snd.Hue = iHue;
            snd.LootType = LootType.Newbied;
            AddItem(snd);

            JesterHat jhat = new JesterHat();
            jhat.Hue = iHue;
            AddItem(jhat);

            Doublet dblt = new Doublet();
            dblt.Hue = iHue;
            AddItem(dblt);

            // Spells
            AddSpellAttack(typeof(Spells.First.MagicArrowSpell));
            AddSpellAttack(typeof(Spells.First.WeakenSpell));
            AddSpellAttack(typeof(Spells.Third.FireballSpell));
            AddSpellDefense(typeof(Spells.Third.WallOfStoneSpell));
            AddSpellDefense(typeof(Spells.First.HealSpell));
        }

        public DummySuper(Serial serial)
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

    public class DummySword : Dummy
    {
        [Constructable]
        public DummySword()
            : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Swordsman
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(125, 125, 90);
            this.Skills[SkillName.Swords].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 120;
            this.Skills[SkillName.Healing].Base = 120;
            this.Skills[SkillName.Tactics].Base = 120;
            this.Skills[SkillName.Parry].Base = 120;


            // Name
            this.Name = "Swordsman";

            // Equip
            Katana kat = new Katana();
            kat.Crafter = this;
            kat.Movable = true;
            kat.Quality = WeaponQuality.Regular;
            AddItem(kat);

            Boots bts = new Boots();
            bts.Hue = iHue;
            AddItem(bts);

            ChainChest cht = new ChainChest();
            cht.Movable = false;
            cht.LootType = LootType.Newbied;
            cht.Crafter = this;
            cht.Quality = ArmorQuality.Regular;
            AddItem(cht);

            ChainLegs chl = new ChainLegs();
            chl.Movable = false;
            chl.LootType = LootType.Newbied;
            chl.Crafter = this;
            chl.Quality = ArmorQuality.Regular;
            AddItem(chl);

            PlateArms pla = new PlateArms();
            pla.Movable = false;
            pla.LootType = LootType.Newbied;
            pla.Crafter = this;
            pla.Quality = ArmorQuality.Regular;
            AddItem(pla);

            Bandage band = new Bandage(50);
            AddToBackpack(band);
        }

        public DummySword(Serial serial)
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

    [TypeAlias("Server.Mobiles.DummyTheif")]
    public class DummyThief : Dummy
    {
        [Constructable]
        public DummyThief()
            : base(AIType.AI_Thief, FightMode.Closest, 15, 1, 0.2, 0.6)
        {
            // A Dummy Hybrid Thief
            int iHue = 20 + Team * 40;
            int jHue = 25 + Team * 40;

            // Skills and Stats
            this.InitStats(105, 105, 105);
            this.Skills[SkillName.Healing].Base = 120;
            this.Skills[SkillName.Anatomy].Base = 120;
            this.Skills[SkillName.Stealing].Base = 120;
            this.Skills[SkillName.ArmsLore].Base = 100;
            this.Skills[SkillName.Meditation].Base = 120;
            this.Skills[SkillName.Wrestling].Base = 120;

            // Name
            this.Name = "Hybrid Thief";

            // Equip
            Spellbook book = new Spellbook();
            book.Movable = false;
            book.LootType = LootType.Newbied;
            book.Content = 0xFFFFFFFFFFFFFFFF;
            AddItem(book);

            LeatherArms lea = new LeatherArms();
            lea.Movable = false;
            lea.LootType = LootType.Newbied;
            lea.Crafter = this;
            lea.Quality = ArmorQuality.Regular;
            AddItem(lea);

            LeatherChest lec = new LeatherChest();
            lec.Movable = false;
            lec.LootType = LootType.Newbied;
            lec.Crafter = this;
            lec.Quality = ArmorQuality.Regular;
            AddItem(lec);

            LeatherGorget leg = new LeatherGorget();
            leg.Movable = false;
            leg.LootType = LootType.Newbied;
            leg.Crafter = this;
            leg.Quality = ArmorQuality.Regular;
            AddItem(leg);

            LeatherLegs lel = new LeatherLegs();
            lel.Movable = false;
            lel.LootType = LootType.Newbied;
            lel.Crafter = this;
            lel.Quality = ArmorQuality.Regular;
            AddItem(lel);

            Sandals snd = new Sandals();
            snd.Hue = iHue;
            snd.LootType = LootType.Newbied;
            AddItem(snd);

            Cap cap = new Cap();
            cap.Hue = iHue;
            AddItem(cap);

            Robe robe = new Robe();
            robe.Hue = iHue;
            AddItem(robe);

            Bandage band = new Bandage(50);
            AddToBackpack(band);
        }

        public DummyThief(Serial serial)
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

    #endregion
}