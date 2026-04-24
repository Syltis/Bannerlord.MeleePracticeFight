# MeleePracticeFight

A Mount & Blade II Bannerlord mod (v1.4.1) that enables melee-only arena practice fights

## Project Structure

- `src/MeleePracticeFightSubModule.cs` — entry point, registers Harmony and campaign behavior
- `src/MeleePracticeFightBehavior.cs` — campaign behavior, injects the arena menu option
- `src/Patches.cs` — Harmony patches and `MeleePracticeWeaponFilter` mission behavior
- `SubModule.xml` — Bannerlord module manifest

## Build

Requires the `BANNERLORD_GAME_DIR` system environment variable (no trailing semicolon).

```
cd src
dotnet build
```

## Code Style
- Use `is` instead of `==` when comparing types and checking for null
- Prefer using Harmony patches instead of reflection when possible

## Key Design Decisions

- **Menu injection**: `CampaignEvents.OnSessionLaunchedEvent` → `AddGameMenuOption("town_arena", ...)`
- **Mission launch**: `PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("arena"), null, null, null)`
- **`_enteredPracticeFightFromMenu`**: set via reflection on `ArenaMasterCampaignBehavior` so rewards behave identically to vanilla
- **Weapon filtering**: Harmony PREFIX on `Mission.SpawnAgent` strips ranged slots from `AgentBuildData.AgentOverridenSpawnEquipment` before the agent is built
- **Flag reset**: `MeleePracticeWeaponFilter.OnEndMission` resets `IsMeleePracticeActive`
- **`Mission.AfterStart` PREFIX**: injects `MeleePracticeWeaponFilter` before the behavior list is iterated — must be prefix to avoid `InvalidOperationException: Collection was modified`
