using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DestroyInGameButtonsObject : MonoBehaviour
{
    public ParticleSystem DestroyParticles;
    public AudioSource DestroySource;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("ingamebutton") && TryGetComponent<HandMoveObject>(out _))
        {
            DestroySource.Play();
            DestroyParticles.transform.position = collision.transform.position;
            DestroyParticles.Play();
            Destroy(collision.gameObject);
        }    
    }
}
