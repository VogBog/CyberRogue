using System.Collections.Generic;
using UnityEngine;

public class DestroyableByWeapon : MonoBehaviour
{
    public float Health;
    public AudioSource SoundSource;
    public GameManager Game;

    public GameManager SafeGame
    {
        get
        {
            if(!Game) Game = FindObjectOfType<GameManager>();
            return Game;
        }
    }

    public bool IsDead { get; protected set; } = false;

    public virtual void Hitted(float damage, AudioClip clip)
    {
        if (clip != null)
        {
            SoundSource.Stop();
            SoundSource.clip = clip;
            SoundSource.Play();
        }
        Health -= damage;
        if (Health <= 0)
            Death();
    }

    protected virtual void Death()
    {
        IsDead = true;
        SafeGame.ThingIsDestroyed(this);
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
