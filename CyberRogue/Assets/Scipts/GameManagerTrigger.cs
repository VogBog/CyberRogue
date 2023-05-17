using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTrigger : MonoBehaviour
{
    public string TriggerName;
    public GameManager Game;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("player"))
            Game.GameManagerTriggerIsEnter(TriggerName);
    }
}
