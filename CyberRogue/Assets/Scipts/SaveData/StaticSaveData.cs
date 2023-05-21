using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Audio;

public static class StaticSaveData
{
    public static BaseWeapon Glock;
    public static BaseWeapon USP;
    public static BaseWeapon[] AllWeapons;
    public static LevelRoom[] AllLevelRooms;
    public static Ability[] AllAbilities;
    public static AudioMixer Mixer;

    public static string SettingsDataPath { get; } = Application.persistentDataPath + "/SaveSlots/SettingsData.dat";

    public static string GetPath(int index)
    {
        if (index < 0 || index > 4)
            return Application.persistentDataPath + "/SaveSlots/SaveSlot1.dat";
        return Application.persistentDataPath + "/SaveSlots/SaveSlot" + index + ".dat";
    }

    public static BaseWeapon GetWeaponByName(string name)
    {
        for(int i = 0; i < AllWeapons.Length; i++)
        {
            if (AllWeapons[i].ItemName == name)
                return AllWeapons[i];
        }
        return Glock;
    }

    public static Ability GetAbilityByType(Ability.AbilityType type)
    {
        for(int i = 0; i < AllAbilities.Length; i++)
        {
            if (AllAbilities[i].Type == type)
                return AllAbilities[i];
        }
        return AllAbilities[0];
    }

    public static LevelRoom GetLevelRoomByIndex(int indx)
    {
        for(int i = 0; i < AllLevelRooms.Length; i++)
        {
            if (AllLevelRooms[i].RoomIndex == indx)
                return AllLevelRooms[i];
        }
        return AllLevelRooms[0];
    }

    public static void SaveSettingsData()
    {
        FileStream stream = File.OpenWrite(SettingsDataPath);
        BinaryFormatter bf = new BinaryFormatter();
        string res = $"{AllSettings.SaveSlot}\n{AllSettings.Sensivity}\n{AllSettings.SoundEffects}\n{AllSettings.Music}\n{AllSettings.Ambient}" +
                    "\n" + QualitySettings.GetQualityLevel();
        bf.Serialize(stream, res);
        stream.Close();
    }

    public static void LoadSettingsData()
    {
        FileStream file = File.OpenRead(SettingsDataPath);
        BinaryFormatter bf = new BinaryFormatter();
        string[] lines = ((string)bf.Deserialize(file)).Split('\n');
        if (int.TryParse(lines[0], out int slot))
            AllSettings.SaveSlot = slot;
        if (float.TryParse(lines[1], out float data))
            AllSettings.Sensivity = data;
        if (float.TryParse(lines[2], out data))
            AllSettings.SoundEffects = data;
        if (float.TryParse(lines[3], out data))
            AllSettings.Music = data;
        if (float.TryParse(lines[4], out data))
            AllSettings.Ambient = data;
        if (int.TryParse(lines[5], out slot))
            QualitySettings.SetQualityLevel(slot);

        Mixer.SetFloat("Effects", AllSettings.SoundEffects);
        Mixer.SetFloat("Music", AllSettings.Music);
        Mixer.SetFloat("Ambient", AllSettings.Ambient);

        file.Close();
    }
}
