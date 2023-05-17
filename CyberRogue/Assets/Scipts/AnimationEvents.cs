using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public AudioSource Source;
    public AudioClip[] StepSounds;

    public void PlayStep()
    {
        if(StepSounds.Length > 0)
        {
            Source.Stop();
            Source.clip = StepSounds[Random.Range(0, StepSounds.Length)];
            Source.Play();
        }
    }
}
