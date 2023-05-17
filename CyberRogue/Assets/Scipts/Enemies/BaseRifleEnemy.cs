using UnityEngine;

public class BaseRifleEnemy : BaseAnimatedEnemy
{
    public AudioSource[] FireSources;

    private int curFireSoundIndex = 0;

    protected override void PlayFireSound()
    {
        FireSources[curFireSoundIndex].Stop();
        FireSources[curFireSoundIndex].clip = FireSound[Random.Range(0, FireSound.Length)];
        FireSources[curFireSoundIndex].Play();
        curFireSoundIndex = (curFireSoundIndex + 1) % FireSources.Length;
    }
}
