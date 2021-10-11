using System.IO;
using BepInEx;
using BepInEx.Configuration;
using WeaponRandomizerPlugin.WeaponRandomizer.@enum;

namespace WeaponRandomizerPlugin.WeaponRandomizer
{
    public static class ConfigManager
    {
        private static readonly ConfigFile ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "WeaponRandomizer.cfg"), true);

        private static readonly ConfigEntry<DistributionType> DistType = ConfigFile
            .Bind("Randomization Types", nameof (DistributionType), DistributionType.Random, 
                "Chooses how weapons are distributed amongst players.\n" +
                "Random: Every player gets random weapons.\n" +
                "Unique: Every player gets a unique weapon.\n" +
                "Equal: Every player gets the same weapon.");

        private static readonly ConfigEntry<SelectionType> SelectType = ConfigFile
            .Bind("Randomization Types", nameof (SelectionType), SelectionType.SemiRandom, 
                "Chooses how weapons are picked for individual players.\n" +
                "Random: Randomly picks a weapon, can have repeating weapons in a row.\n" +
                "SemiRandom: Picks a weapon from a shuffled queue. Very unlikely to have repeating weapons in a row " +
                "and will go through every weapon available at a random order.");

        private static readonly ConfigEntry<bool> Door = ConfigFile
            .Bind("Randomization Triggers", nameof (RandomizeOnSecDoorOpen), false, "Randomize on security door open.");

        private static readonly ConfigEntry<int> Timer = ConfigFile
            .Bind("Randomization Triggers", nameof (RandomizeByInterval), 180, "In seconds, randomize on this interval. 0s for Not at all");

        private static readonly ConfigEntry<int> Alter = ConfigFile
            .Bind("Randomization Triggers", nameof (AlterInterval), 0, 
                "If randomizing by interval, alter the interval duration every interval.\n" +
                "Eg. if Interval is 60s and alter is set to 10s, the interval will be randomly set on every trigger to be a time between 50s or 70s");

        private static readonly ConfigEntry<bool> Melee = ConfigFile
            .Bind("Randomization Slots", nameof (RandomizeMelee), true, "Randomize Melee weapons.");

        private static readonly ConfigEntry<bool> Standard = ConfigFile
            .Bind("Randomization Slots", nameof (RandomizePrimary), true, "Randomize Primary weapons.");

        private static readonly ConfigEntry<bool> Special = ConfigFile
            .Bind("Randomization Slots", nameof (RandomizeSecondary), true, "Randomize Secondary weapons.");

        private static readonly ConfigEntry<bool> Class = ConfigFile
            .Bind("Randomization Slots", nameof (RandomizeTool), true, "Randomize Tools.");

        private static readonly ConfigEntry<bool> SentryDeployment = ConfigFile
            .Bind("Sentry Guns", nameof (PickUpSentryOnSwitch), false, 
                "Auto pick up sentry when switching tools.\n " +
                "If false, this will leave you with zero tool ammo on change but will keep the sentry deployed \n" +
                "(Your deployed sentry can still be picked up to retrieve its tool even if you no longer have a sentry)");

        private static readonly ConfigEntry<bool> SentryLimit = ConfigFile
            .Bind("Sentry Guns", nameof (TreatSentriesAsOne), true, 
                "Reduces the chances of getting a sentry gun such that it is equal to the chance of selecting other tool types.");

        public static DistributionType DistributionType => DistType.Value;
        public static SelectionType SelectionType => SelectType.Value;
        public static bool RandomizeOnSecDoorOpen => Door.Value;
        public static int RandomizeByInterval => Timer.Value;
        public static int AlterInterval => Alter.Value;
        public static bool RandomizeMelee => Melee.Value;
        public static bool RandomizePrimary => Standard.Value;
        public static bool RandomizeSecondary => Special.Value;
        public static bool RandomizeTool => Class.Value;
        public static bool PickUpSentryOnSwitch => SentryDeployment.Value;
        public static bool TreatSentriesAsOne => SentryLimit.Value;
    }
}