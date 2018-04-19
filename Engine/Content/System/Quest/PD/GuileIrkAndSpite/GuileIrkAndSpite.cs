using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class GuileIrkAndSpite : MLQuest
    {
        public GuileIrkAndSpite()
        {
            Activated = true;
            Title = 1074739; // Guile, Irk and Spite
            Description = 1074740; // You know them, don't you.  The three?  They look like you, you'll see. They looked like me, I remember, they looked like, well, you'll see.  The three.  They'll drive you mad too, if you let them.  They are trouble, and they need to be slain.  Seek them out.
            RefusalMessage = 1074745; // You just don't understand the gravity of the situation.  If you did, you'd agree to my task.
            InProgressMessage = 1074746; // Perhaps I was unclear.  You'll know them when you see them, because you'll see you, and you, and you.  Hurry now.
            CompletionMessage = 1074747; // Are you one of THEM?  Ahhhh!  Oh, wait, if you were them, then you'd be me.  So you're -- you.  Good job!

            Objectives.Add(new KillObjective(1, new Type[] { typeof(Guile) }, "Guile"));
            Objectives.Add(new KillObjective(1, new Type[] { typeof(Irk) }, "Irk"));
            Objectives.Add(new KillObjective(1, new Type[] { typeof(Spite) }, "Spite"));

            Rewards.Add(ItemReward.Strongbox);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Yorus"), new Point3D(1389, 423, -24), Map.Ilshenar);
        }
    }
}