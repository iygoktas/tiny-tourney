using System.Collections.Generic;
using System.Linq;
using Godot;
using TinyTourney.Data;

namespace TinyTourney.Progression;

public static class ContentRepository
{
    private static List<WeaponData> _allWeapons;
    private static List<SpellData> _allSpells;
    private static List<StatRollData> _allStatRolls;
    private static List<RaceData> _allRaces;

    public static List<WeaponData> AllWeapons => _allWeapons ??= LoadAllOfType<WeaponData>("res://data/weapons/");
    public static List<SpellData> AllSpells => _allSpells ??= LoadAllOfType<SpellData>("res://data/spells/");
    public static List<StatRollData> AllStatRolls => _allStatRolls ??= LoadAllOfType<StatRollData>("res://data/stat_rolls/");
    public static List<RaceData> AllRaces => _allRaces ??= LoadAllOfType<RaceData>("res://data/races/");

    public static WeaponData GetWeaponById(string id) => AllWeapons.FirstOrDefault(w => w.Id == id);
    public static SpellData GetSpellById(string id) => AllSpells.FirstOrDefault(s => s.Id == id);
    public static RaceData GetRaceById(string id) => AllRaces.FirstOrDefault(r => r.Id == id);

    private static List<T> LoadAllOfType<T>(string folder) where T : Resource
    {
        var results = new List<T>();

        using var dir = DirAccess.Open(folder);
        if (dir == null)
        {
            return results;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
            {
                var resource = GD.Load<T>(folder + fileName);
                if (resource != null)
                {
                    results.Add(resource);
                }
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        return results;
    }
}
