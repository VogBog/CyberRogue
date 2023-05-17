using UnityEngine;

public class AIManagerTrigger : MonoBehaviour
{
    public AIRoomManager AI;

    public void OnTriggerEnter(Collider other)
    {
        AI.OnTriggerEnter(other);
    }
}
