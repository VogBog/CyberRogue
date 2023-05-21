using DG.Tweening;
using TMPro;
using UnityEngine;

public class DestroyablePart : MonoBehaviour
{
    public float DamageMultiply;
    public DestroyableByWeapon HeadObject;
    public TextMeshPro DamageTxt;
    public AudioClip[] Clip;
    public ParticleSystem Particles;

    public void GetDamage(BaseWeapon weapon, Vector3 point)
    {
        if (Particles != null)
        {
            Particles.transform.position = point;
            Particles.Stop();
            Particles.Play();
        }
        if (DamageTxt)
        {
            TextMeshPro decal = Instantiate(DamageTxt, transform.position, Quaternion.identity, null);
            decal.transform.LookAt(weapon.transform.position);
            decal.transform.Rotate(decal.transform.up, 180);
            decal.text = Mathf.RoundToInt(weapon.ActualDamage * DamageMultiply).ToString();
            float randX = Random.Range(-1f, 1f);
            float randZ = Random.Range(-1f, 1f);
            Vector3 moveVec = weapon.transform.position - transform.position;
            moveVec.Normalize();
            moveVec.x += randX;
            moveVec.z += randZ;
            decal.transform.DOMoveX(decal.transform.position.x + moveVec.x, .5f);
            decal.transform.DOMoveZ(decal.transform.position.z + moveVec.z, .5f);
            decal.GetComponent<DestroyableParticle>().StartTimer(decal.transform.position.y + 1, decal.transform.position.y - 1);
        }
        HeadObject.Hitted(weapon.ActualDamage * DamageMultiply, Clip[Random.Range(0, Clip.Length)]);
    }
}
