using UnityEngine;

public class ExplosionBarrel : DestroyableByWeapon
{
    public GameObject Explosion;

    protected override void Death()
    {
        Instantiate(Explosion, transform.position, Quaternion.identity);
        base.Death();
    }
}
