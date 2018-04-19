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
    public class TheyreBreedingLikeRabbits : MLQuest
    {
        public TheyreBreedingLikeRabbits()
        {
            Activated = true;
            Title = 1072244; // They're Breeding Like Rabbits
            Description = 1072259; // Aaaahhhh!  They're everywhere!  Aaaaahhh!  Ahem.  Actually, friend, how do you feel about rabbits? Well, we're being overrun by them.  We're finding fuzzy bunnies everywhere. Aaaaahhh!
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Rabbit) }, "rabbits"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Saril"), new Point3D(7075, 376, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Saril"), new Point3D(7075, 376, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Cailla"), new Point3D(7075, 377, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Cailla"), new Point3D(7075, 377, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Tamm"), new Point3D(7075, 378, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Tamm"), new Point3D(7075, 378, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Landy"), new Point3D(7089, 390, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Landy"), new Point3D(7089, 390, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Alejaha"), new Point3D(7043, 387, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Alejaha"), new Point3D(7043, 387, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Mielan"), new Point3D(7063, 350, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Mielan"), new Point3D(7063, 350, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Ciala"), new Point3D(7031, 411, 7), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Ciala"), new Point3D(7031, 411, 7), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Aniel"), new Point3D(7034, 412, 6), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Aniel"), new Point3D(7034, 412, 6), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Aulan"), new Point3D(6986, 340, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Aulan"), new Point3D(6986, 340, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Brinnae"), new Point3D(6996, 351, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Brinnae"), new Point3D(6996, 351, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Caelas"), new Point3D(7039, 390, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Caelas"), new Point3D(7039, 390, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Clehin"), new Point3D(7092, 390, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Clehin"), new Point3D(7092, 390, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Cloorne"), new Point3D(7010, 364, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Cloorne"), new Point3D(7010, 364, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Salaenih"), new Point3D(7009, 362, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Salaenih"), new Point3D(7009, 362, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Vilo"), new Point3D(7029, 377, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Vilo"), new Point3D(7029, 377, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Tholef"), new Point3D(6986, 386, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Tholef"), new Point3D(6986, 386, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Tillanil"), new Point3D(6987, 388, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Tillanil"), new Point3D(6987, 388, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Waelian"), new Point3D(6996, 381, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Waelian"), new Point3D(6996, 381, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Sleen"), new Point3D(6997, 381, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Sleen"), new Point3D(6997, 381, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Unoelil"), new Point3D(7010, 388, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Unoelil"), new Point3D(7010, 388, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Anolly"), new Point3D(7009, 388, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Anolly"), new Point3D(7009, 388, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Jusae"), new Point3D(7042, 377, 2), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Jusae"), new Point3D(7042, 377, 2), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Cillitha"), new Point3D(7043, 377, 2), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Cillitha"), new Point3D(7043, 377, 2), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Lohn"), new Point3D(7062, 410, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Lohn"), new Point3D(7062, 410, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Olla"), new Point3D(7063, 410, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Olla"), new Point3D(7063, 410, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Thallary"), new Point3D(7032, 439, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Thallary"), new Point3D(7032, 439, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Ahie"), new Point3D(7033, 440, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Ahie"), new Point3D(7033, 440, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Tyeelor"), new Point3D(7010, 364, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Tyeelor"), new Point3D(7010, 364, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Athailon"), new Point3D(7011, 365, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Athailon"), new Point3D(7011, 365, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderTaellia"), new Point3D(7038, 387, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderTaellia"), new Point3D(7038, 387, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderMallew"), new Point3D(7047, 390, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderMallew"), new Point3D(7047, 390, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderAbbein"), new Point3D(7043, 390, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderAbbein"), new Point3D(7043, 390, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderVicaie"), new Point3D(7054, 390, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderVicaie"), new Point3D(7054, 390, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderJothan"), new Point3D(7056, 383, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderJothan"), new Point3D(7056, 383, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "ElderAlethanian"), new Point3D(7056, 380, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "ElderAlethanian"), new Point3D(7056, 380, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Rebinil"), new Point3D(7089, 380, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Rebinil"), new Point3D(7089, 380, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Aluniol"), new Point3D(7089, 383, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Aluniol"), new Point3D(7089, 383, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Olaeni"), new Point3D(7080, 363, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Olaeni"), new Point3D(7080, 363, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Bolaevin"), new Point3D(7066, 351, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Bolaevin"), new Point3D(7066, 351, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "LorekeeperAneen"), new Point3D(7053, 337, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "LorekeeperAneen"), new Point3D(7053, 337, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Daelas"), new Point3D(7036, 412, 7), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Daelas"), new Point3D(7036, 412, 7), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Alelle"), new Point3D(7028, 406, 7), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Alelle"), new Point3D(7028, 406, 7), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "LorekeeperNillaen"), new Point3D(7061, 370, 14), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "LorekeeperNillaen"), new Point3D(7061, 370, 14), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "LorekeeperRyal"), new Point3D(7009, 375, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "LorekeeperRyal"), new Point3D(7009, 375, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Braen"), new Point3D(7081, 366, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Braen"), new Point3D(7081, 366, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderAcob"), new Point3D(7037, 387, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "ElderAcob"), new Point3D(7037, 387, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "LorekeeperCalendor"), new Point3D(7062, 370, 14), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "LorekeeperCalendor"), new Point3D(7062, 370, 14), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "LorekeeperSiarra"), new Point3D(7051, 339, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "LorekeeperSiarra"), new Point3D(7051, 339, 0), Map.Felucca);
        }
    }
}