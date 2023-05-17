using UnityEngine;

public static class StaticSaveData
{
    public static BaseWeapon Glock;
    public static BaseWeapon USP;
    public static BaseWeapon[] AllWeapons;
    public static LevelRoom[] AllLevelRooms;
    public static Ability[] AllAbilities;

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
}
