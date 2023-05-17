using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PuturidzeElectroMob : MonoBehaviour
{
    public NavMeshAgent Agent;
    public float Damage;

    private BasePlayer player;
    private bool isAttack = false;
    private float updateDestinationTime = .5f;

    public void StartAttack(BasePlayer player)
    {
        this.player = player;
        isAttack = true;
    }

    private void Update()
    {
        if(isAttack)
        {
            updateDestinationTime -= Time.deltaTime;
            if (updateDestinationTime <= 0)
            {
                Agent.SetDestination(player.transform.position);
                updateDestinationTime = .5f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("player"))
        {
            player.GetDamage(Damage);
            Destroy(gameObject);
        }
        else if(collision.collider.CompareTag("ingamebutton") && !TryGetComponent<BaseWeapon>(out _))
        {
            Destroy(gameObject);
        }
    }
}
