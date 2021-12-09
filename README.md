# GTFO - Weapon Randomizer 
Let RNGesus take the wheel 

## Randomize Weapon Loadouts During An Expedition  
Whilst in an expedition this plugin will randomly change your team's loadout when triggered (triggers are configurable).

By default the plugin will randomly change your team's load out every 3 min without repeating weapons and deployed sentries set to stay deployed on switch

## Configurable settings 
*Note that the plugin will follow the host's settings and client config will not effect play
### Randomization Triggers
- **RandomizeByInterval** [default 180]: Randomize on every n seconds
- **AlterInterval** [default 0]: Randomize the interval on every trigger by n seconds (e.g. interval 60s and alt 10s will change the interval of the trigger to be anywhere in between 50s or 70s on every trigger)
- **RandomizeOnSecDoorOpen** [default false]: Randomize weapons when a security door opens 

### Radomization Types
- **DistributionType** [default Random]: Chooses how weapons are distributed amongst players.
Random, Every player gets random weapons.
Unique, Every player gets a unique weapon.
Equal, Every player gets the same weapon.

- **SelectionType** [default SemiRandom]: Chooses how weapons are picked for individual players.
Random, Randomly picks a weapon, can have repeating weapons in a row.
SemiRandom, Picks a weapon from a shuffled queue. Very unlikely to have repeating weapons in a row and will go through every weapon available at a random order.

### Randomization Slots
- **RandomizeMelee** [default true]: Allow melee to be randomized
- **RandomizePrimary** [default true]: Allow primary to be randomized
- **RandomizeSecondary** [default true]: Allow secondary to be randomized
- **RandomizeTool** [default true]: Allow tool to be randomized

### Sentry Guns
- **PickUpSentryOnSwitch** [default false]: Auto pick up sentry when switching tools.
If false, this will leave you with zero tool ammo on change but will keep the sentry deployed
(Your deployed sentry can still be picked up to retrieve its tool even if you no longer have a sentry)
- **TreatSentriesAsOne** [default true]: Reduces the chances of getting a sentry gun such that it is equal to the  chance of selecting other tool types

### Credits

- Developed by Cookie_K
- Special thanks to Basrijs, iRekia, and Kolfo for testing 

---

### Patches
- 1.0.1: Updated networking to use GTFO API and removed use of Nidhogg
