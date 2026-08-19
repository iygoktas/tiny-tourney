using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace TinyTourney.Core;

public static class SaveManager
{
    public const int MaxSlots = 5;
    private const string SaveDir = "user://saves/";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static string SlotPath(int slotIndex) => $"{SaveDir}slot_{slotIndex}.json";

    public static bool SlotExists(int slotIndex)
    {
        return FileAccess.FileExists(SlotPath(slotIndex));
    }

    public static SaveSlotData LoadSlot(int slotIndex)
    {
        string path = SlotPath(slotIndex);
        if (!FileAccess.FileExists(path))
        {
            return null;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        return JsonSerializer.Deserialize<SaveSlotData>(json, JsonOptions);
    }

    public static void SaveSlot(int slotIndex, SaveSlotData data)
    {
        DirAccess.MakeDirRecursiveAbsolute(SaveDir);
        string json = JsonSerializer.Serialize(data, JsonOptions);
        using var file = FileAccess.Open(SlotPath(slotIndex), FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }

    public static void DeleteSlot(int slotIndex)
    {
        string path = SlotPath(slotIndex);
        if (FileAccess.FileExists(path))
        {
            DirAccess.RemoveAbsolute(path);
        }
    }

    public static List<int> ListUsedSlots()
    {
        var used = new List<int>();
        for (int i = 0; i < MaxSlots; i++)
        {
            if (SlotExists(i))
            {
                used.Add(i);
            }
        }
        return used;
    }
}
