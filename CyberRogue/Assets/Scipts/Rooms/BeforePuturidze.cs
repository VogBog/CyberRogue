using System.Collections;
using UnityEngine;

public class BeforePuturidze : BeforeBossTrigger
{
    public AudioSource[] AudioSources;

    protected override void PlayerIsCollided()
    {
        for(int i = 0; i < AudioSources.Length; i++)
        {
            AudioSources[i].Play();
        }
    }

    private void Start()
    {
        StartCoroutine(StartSoundsIE());
    }

    IEnumerator StartSoundsIE()
    {
        for(int i = 0; i < AudioSources.Length; i++)
        {
            AudioSources[i].Play();
            yield return new WaitForSeconds(20);
        }
        for (int i = 0; i < AudioSources.Length; i++)
            AudioSources[i].Pause();
    }
}
