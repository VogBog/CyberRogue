using System.Collections;
using TMPro;
using UnityEngine;

public class BaseAnimatedEnemy : BaseEnemy
{
    public Transform ModelTransform;
    public Animator Anim;
    public float RunSpeed, WalkSpeed, LookPlayerAfterFireTimer;
    public KillMessage KillTxt;
    public GameObject[] Loot;
    public float LootChance;

    private Coroutine speedCor;
    private bool isLookAtPlayer = false;

    protected override void Fire()
    {
        base.Fire();
        Anim.SetInteger("State", 2);
        RunSpeedCor();
        StartCoroutine(SetAnimStateIE());
    }

    protected override void Reload()
    {
        base.Reload();
        Anim.SetInteger("State", 3);
        if (speedCor != null) StopCoroutine(speedCor);
        StartCoroutine(ReloadIE());
        StartCoroutine(SetAnimStateIE());
    }

    IEnumerator ReloadIE()
    {
        Agent.speed = 0;
        yield return new WaitForSeconds(ReloadTime);
        Agent.speed = RunSpeed;
    }

    public override void Hitted(float damage, AudioClip clip)
    {
        base.Hitted(damage, clip);
        Anim.SetInteger("State", 4);
        StartCoroutine(SetAnimStateIE());
    }

    protected override void Update()
    {
        if (isFighting)
        {
            base.Update();
            if (isLookAtPlayer)
            {
                ModelTransform.LookAt(player.transform.position);
                float rotY = ModelTransform.rotation.eulerAngles.y;
                ModelTransform.rotation = Quaternion.Euler(0, rotY, 0);
            }
            else if(Agent.velocity.magnitude > .1f) ModelTransform.rotation = transform.rotation;
            Vector2 move = GetMoveVectorProjection(Agent.velocity, ModelTransform.forward);
            Anim.SetFloat("moveX", move.x);
            Anim.SetFloat("moveY", move.y);
        }
    }

    protected virtual Vector2 GetMoveVectorProjection(Vector3 move, Vector3 lookVec)
    {
        if (move.magnitude == 0) return Vector2.zero;
        move.Normalize();
        lookVec.Normalize();
        float x = lookVec.z * move.x - lookVec.x * move.z;
        float y = lookVec.x * move.x + lookVec.z * move.z;
        return new Vector2(x, y);
    }

    protected void RunSpeedCor()
    {
        if (speedCor != null) StopCoroutine(speedCor);
        speedCor = StartCoroutine(SpeedIE());
    }

    IEnumerator SpeedIE()
    {
        isLookAtPlayer = true;
        Agent.speed = WalkSpeed;
        yield return new WaitForSeconds(LookPlayerAfterFireTimer);
        isLookAtPlayer = false;
        Anim.SetInteger("State", 1);
        Agent.speed = RunSpeed;
    }

    IEnumerator SetAnimStateIE()
    {
        yield return null;
        Anim.SetInteger("State", 0);
    }

    protected override void Death()
    {
        if (!IsDead)
        {
            if (KillTxt)
            {
                Vector3 pos = KillTxt.transform.position;
                KillTxt.transform.SetParent(null);
                KillTxt.gameObject.SetActive(true);
                KillTxt.transform.position = pos;
                KillTxt.Activate();
            }
            if (Random.Range(0, 100) <= LootChance && Loot.Length > 0)
            {
                Instantiate(Loot[Random.Range(0, Loot.Length)], transform.position, Quaternion.identity, null);
            }
            if (player.HasThisAbility(Ability.AbilityType.HealAfterKill))
                player.Heal(10);
            base.Death();
        }
    }
}
