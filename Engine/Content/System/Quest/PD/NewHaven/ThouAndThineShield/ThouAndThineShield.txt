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
    public class ThouAndThineShield : MLQuest
    {
        public ThouAndThineShield()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077704; // Thou and Thine Shield
            Description = 1077707; // Head East out of town and go to Old Haven. Battle monsters, or simply let them hit you, while holding a shield or a weapon until you have raised your Parrying skill to 50. Oh, hello. You probably want me to teach you how to parry, don't you? Very Well. First, you'll need a weapon or a shield. Obviously shields work best of all, but you can parry with a 2-handed weapon. Or if you're feeling particularly brave, a 1-handed weapon will do in a pinch, I'd advise you to go to Old Haven, which you'll find to the East, and practice blocking incoming blows from the undead there. You'll learn quickly if you have more than one opponent attacking you at the same time to practice parrying lots of blows at once. That's the quickest way to master the art of parrying. If you manage to improve your skill enough, i have a shield that you might find useful. Come back to me when you've trained to an apprentice level.
            RefusalMessage = 1077708; // It's your choice, obviously, but I'd highly suggest that you learn to parry before adventuring out into the world. Come talk to me again when you get tired of being beat on by your opponents
            InProgressMessage = 1077709; // You're doing well, but in my opinion, I Don't think you really want to continue on without improving your parrying skill a bit more. Go to Old Haven, to the East, and practice blocking blows with a shield.
            CompletionMessage = 1077711; // Well done! You're much better at parrying blows than you were when we first met. You should be proud of your new ability and I bet your body is greatful to you aswell. *Tyl Ariadne laughs loudly at his ownn (mostly lame) joke*	Oh yes, I did promise you a shield if I thought you were worthy of having it, so here you go. My father made these shields for the guards who served my father faithfully for many years, and I just happen to have obe that i can part with. You should find it useful as you explore the lands.Good luck, and may the Virtues be your guide.
            CompletionNotice = 1077710; // You have achieved the rank of Apprentice Warrior (for Parrying). Return to Tyl Ariadne in New Haven as soon as you can to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Parry, 500, true, true));

            Rewards.Add(new ItemReward(1077694, typeof(EscutcheonDeAriadne))); // Escutcheon de Ariadne
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "TylAriadne"), new Point3D(3525, 2556, 20), Map.Trammel);
        }
    }
}