using System.Collections.Generic;
using Godot;
using TinyTourney.Core;
using TinyTourney.Data;

namespace TinyTourney.Progression;

public class LevelUpResult
{
    public int NewLevel { get; set; }
    public WheelResult WheelResult { get; set; }
}

public static class ProgressionManager
{
    public static List<LevelUpResult> AwardXp(SaveSlotData save, bool won)
    {
        save.CurrentXp += won ? 3 : 1;
        var results = new List<LevelUpResult>();

        while (save.CurrentXp >= XpCurve.XpRequiredForLevel(save.Level))
        {
            save.CurrentXp -= XpCurve.XpRequiredForLevel(save.Level);
            save.Level++;

            if (save.Level > save.Statistics.HighestLevelReached)
            {
                save.Statistics.HighestLevelReached = save.Level;
            }

            var wheelResult = WheelSpinner.Spin(save);
            ApplyWheelResult(save, wheelResult);

            results.Add(new LevelUpResult { NewLevel = save.Level, WheelResult = wheelResult });
        }

        return results;
    }

    public static void ApplyWheelResult(SaveSlotData save, WheelResult result)
    {
        switch (result.Category)
        {
            case WheelCategory.Weapon:
                SaveSlotMutations.MarkWeaponObtained(save, result.Weapon.Id);
                SaveSlotMutations.EquipWeapon(save, result.Weapon.Id);
                break;
            case WheelCategory.Spell:
                SaveSlotMutations.MarkSpellObtained(save, result.Spell.Id);
                SaveSlotMutations.EquipSpell(save, result.Spell.Id);
                break;
            case WheelCategory.Stat:
                SaveSlotMutations.ApplyStatRoll(save, result.StatRoll);
                break;
        }
    }

    public static void RunSelfTest()
    {
        var humanRace = GD.Load<RaceData>("res://data/races/human.tres");
        var save = new SaveSlotData
        {
            RaceId = "human",
            CharacterName = "Progression Tester",
            Level = 1,
            CurrentXp = 0,
            CurrentStats = RuntimeStatBlock.FromDesignStats(humanRace.BaseStats)
        };

        for (int battle = 1; battle <= 200 && save.Level < 10; battle++)
        {
            var levelUps = AwardXp(save, won: true);
            foreach (var result in levelUps)
            {
                var wheel = result.WheelResult;
                string detail = wheel.Category switch
                {
                    WheelCategory.Weapon => $"Weapon: {wheel.Weapon?.DisplayName}",
                    WheelCategory.Spell => $"Spell: {wheel.Spell?.DisplayName}",
                    _ => $"Stat: {wheel.StatRoll?.Stat} +{wheel.StatRoll?.Amount}"
                };
                GD.Print($"[ProgressionManager.RunSelfTest] Level {result.NewLevel} reached (endgame={wheel.IsEndgameMode}) -> {detail}");
            }
        }

        GD.Print($"[ProgressionManager.RunSelfTest] Final: Level={save.Level}, XP={save.CurrentXp}, WeaponsObtained=[{string.Join(",", save.ObtainedWeaponIds)}], SpellsObtained=[{string.Join(",", save.ObtainedSpellIds)}]");
    }
}
