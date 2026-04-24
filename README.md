# Melee Practice Fight – Bannerlord Mod
**Target game version:** Mount & Blade II: Bannerlord **v1.4.0**  
**Dependency:** [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006)

---

## What It Does

Adds a new dialogue option when speaking to the **Arena Master** in any town with an arena:

> *"I want a melee-only practice fight."*

This starts a normal Arena Practice Fight session, but **all ranged weapons (bows, crossbows, thrown weapons, arrows, bolts) are stripped from every fighter** – including the player and all NPC opponents – before they spawn.  

Everything else remains vanilla:
- Standard knockout rules and scoring
- Normal skill XP gains
- Same arena scene and wave logic
- Compatible with save games (no data serialized)

---

## Project Structure

```
MeleePracticeFight/
├── SubModule.xml                   ← Mod manifest (copy to Modules folder)
└── src/
    ├── MeleePracticeFight.csproj   ← VS / dotnet build project
    ├── MeleePracticeFightSubModule.cs   ← Entry point, Harmony init
    ├── MeleePracticeFightBehavior.cs    ← Dialogue + mission launch logic
    └── Patches.cs                       ← Three Harmony patches
```

---

## Building

### Prerequisites
- Visual Studio 2019/2022 **or** .NET SDK (net472 / .NET Framework 4.7.2)
- Bannerlord v1.4.0 installed
- [Harmony mod](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006) installed in your Modules folder

### Steps

1. **Clone / copy** the `src/` folder somewhere convenient.

2. **Set your game path.** Open `MeleePracticeFight.csproj` and update:
   ```xml
   <BannerlordPath>C:\Program Files (x86)\Steam\steamapps\common\Mount &amp; Blade II Bannerlord</BannerlordPath>
   ```
   Or pass it on the command line (see below).

3. **Build:**
   ```bash
   cd src
   dotnet build -c Release -p:BannerlordPath="C:\...\Mount & Blade II Bannerlord"
   ```
   This will:
   - Compile `MeleePracticeFight.dll`
   - Copy it to `<BannerlordPath>\Modules\MeleePracticeFight\bin\Win64_Shipping_Client\`
   - Copy `SubModule.xml` to `<BannerlordPath>\Modules\MeleePracticeFight\`

4. **Verify the output folder** looks like:
   ```
   Modules\MeleePracticeFight\
   ├── SubModule.xml
   └── bin\Win64_Shipping_Client\
       └── MeleePracticeFight.dll
   ```

---

## Manual Installation (pre-built)

1. Create the folder:
   ```
   <BannerlordInstall>\Modules\MeleePracticeFight\
   ```
2. Copy `SubModule.xml` into it.
3. Create the subfolder:
   ```
   <BannerlordInstall>\Modules\MeleePracticeFight\bin\Win64_Shipping_Client\
   ```
4. Copy the compiled `MeleePracticeFight.dll` into that subfolder.
5. Launch the Bannerlord launcher, go to **Singleplayer → Mods**.
6. Enable **Harmony** (must be loaded first) and **Melee Practice Fight**.

---

## How It Works (Technical Summary)

| File | Role |
|------|------|
| `MeleePracticeFightSubModule.cs` | `MBSubModuleBase` entry point. Calls `harmony.PatchAll()` and registers the campaign behavior. |
| `MeleePracticeFightBehavior.cs` | `CampaignBehaviorBase`. Hooks into `OnSessionLaunched` to add three dialogue lines to the Arena Master conversation tree (`arena_master_talk` hub). Sets a static flag and calls `CampaignMission.OpenArenaPracticeFightMission`. |
| `Patches.cs` – `Patch_AgentBuildData_Build` | **Prefix** patch on `AgentBuildData.Build`. When the flag is set, iterates the agent's four weapon slots and nulls out any item whose `WeaponClass` falls in the ranged set (Bow, Crossbow, Stone, ThrowingAxe, ThrowingKnife, Javelin, Arrow, Bolt). Applied to both the player and every NPC. |
| `Patches.cs` – `Patch_ArenaMission_OnMissionResultReady` | **Postfix** on `ArenaPracticeFightMissionController.OnMissionResultReady`. Clears the flag when the fight concludes normally. |
| `Patches.cs` – `Patch_Mission_OnMissionStateDeactivateEnd` | **Postfix** on `Mission.OnMissionStateDeactivateEnd`. Belt-and-suspenders flag reset for unexpected mission exits. |

### Dialogue token chain
```
arena_master_talk  (vanilla hub)
  └─ [player] "I want a melee-only practice fight."   → melee_practice_confirm
       └─ [NPC]  "Very well. No bows..."              → melee_practice_player_choice
            ├─ [player] "I am ready."  → close_window  (starts mission)
            └─ [player] "Let me reconsider."  → arena_master_talk
```

---

## Compatibility

- **Save game safe** – no data is serialized; the mod can be added or removed mid-campaign.
- **Harmony-based** – non-destructive patches; compatible with most other arena mods as long as they don't also patch `AgentBuildData.Build`.
- Known potential conflict: **Arena Expansion** (also patches equipment selection). Load order adjustment or a compatibility patch may be needed.

---

## License

MIT – free to use, modify, and redistribute with credit.
