using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStaticSaveData : MonoBehaviour
{
    public BaseWeapon Glock;
    public BaseWeapon USP;
    public BaseWeapon[] AllWeapons;
    public LevelRoom[] AllLevelRooms;
    public Ability[] AllAbilities;

    private void Start()
    {
        StaticSaveData.Glock = Glock;
        StaticSaveData.USP = USP;
        StaticSaveData.AllWeapons = AllWeapons;
        StaticSaveData.AllLevelRooms = AllLevelRooms;
        StaticSaveData.AllAbilities = AllAbilities;
    }
}
