using UnityEngine;
using UnityEngine.Audio;

public class SetStaticSaveData : MonoBehaviour
{
    public BaseWeapon Glock;
    public BaseWeapon USP;
    public BaseWeapon[] AllWeapons;
    public LevelRoom[] AllLevelRooms;
    public Ability[] AllAbilities;
    public AudioMixer Mixer;

    private void Start()
    {
        StaticSaveData.Glock = Glock;
        StaticSaveData.USP = USP;
        StaticSaveData.AllWeapons = AllWeapons;
        StaticSaveData.AllLevelRooms = AllLevelRooms;
        StaticSaveData.AllAbilities = AllAbilities;
        StaticSaveData.Mixer = Mixer;
    }
}
