using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : DestroyableByWeapon
{
    public NavMeshAgent Agent;
    public enum EnemyType { Sniper, Attacker, Universal }
    public EnemyType Type;
    public float FireCooldown, ReloadTime, Damage;
    public int MaxAmmo;
    public LayerMask TargetingLayerMask;
    public Transform BulletPos;
    public LineRenderer[] BulletLines;
    public float LinesLifeTime;
    public AudioClip[] FireSound;
    public AudioClip ReloadSound;
    public float Accuracy;
    public Transform EyesPos;

    protected BasePlayer player;
    protected bool isAttacker;
    protected Transform[] allSpots;
    protected NearPlayerSpot[] nearPlayerSpots;
    protected bool isFighting = false;

    protected bool isFreeze { get; private set; }

    private float attackerGoTime = 0;
    private float curCooldown = 1;
    private int curAmmo;
    private int currentLineRenderer = 0;
    private Color lineStartColor, lineEndColor;
    private Coroutine freezeCor;
    private Coroutine[] fireLinesCor;

    public void StartWave(Transform[] spots, NearPlayerSpot[] nearPlayer, bool isAttacker, BasePlayer player)
    {
        Agent.enabled = true;
        this.player = player;
        this.isAttacker = isAttacker;
        nearPlayerSpots = nearPlayer;
        allSpots = spots;
        isFighting = true;
        curAmmo = MaxAmmo;
        lineStartColor = BulletLines[0].startColor;
        lineEndColor = BulletLines[0].endColor;
        fireLinesCor = new Coroutine[BulletLines.Length];
        for (int i = 0; i < BulletLines.Length; i++)
        {
            BulletLines[i].startColor = new Color();
            BulletLines[i].endColor = new Color();
        }
        Agent.SetDestination(transform.position);
    }

    public void UpdateWaveLikeAttacker() => isAttacker = true;

    private Vector3 GetNearPlayerPosition()
    {
        int rand = Random.Range(0, nearPlayerSpots.Length);
        if (nearPlayerSpots[rand].IsFree)
            return nearPlayerSpots[rand].transform.position;
        for(int i = 0; i < nearPlayerSpots.Length; i++)
        {
            if (nearPlayerSpots[i].IsFree)
                return nearPlayerSpots[i].transform.position;
        }
        return player.transform.position;
    }

    protected virtual void Reload()
    {
        curAmmo = MaxAmmo;
        curCooldown = ReloadTime;
        SoundSource.Stop();
        SoundSource.clip = ReloadSound;
        SoundSource.Play();
    }

    protected virtual void Target()
    {
        if(curAmmo == 0)
        {
            Reload();
            return;
        }
        if (isFreeze) return;
        Ray ray = new Ray(EyesPos.position, player.CameraPos.transform.position - EyesPos.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, TargetingLayerMask) && hit.collider.CompareTag("player"))
        {
            curCooldown = FireCooldown;
            curAmmo--;
            Fire();
        }
        else curCooldown = FireCooldown > .5f ? .5f : FireCooldown;
    }

    protected virtual void PlayFireSound()
    {
        SoundSource.Stop();
        SoundSource.clip = FireSound[Random.Range(0, FireSound.Length)];
        SoundSource.Play();
    }

    protected virtual void Fire()
    {
        PlayFireSound();
        float chance = Random.Range(0, 10);
        Vector3 targetPos = player.transform.position;
        if (chance >= 7)
            targetPos += Vector3.up * .2f;
        targetPos -= BulletPos.position;
        float randX = Random.Range(-Accuracy, Accuracy) * targetPos.magnitude;
        float randY = Random.Range(-Accuracy, Accuracy) * targetPos.magnitude;
        Ray ray = new Ray(BulletPos.position, targetPos + transform.up * randY + transform.right * randX);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, TargetingLayerMask))
        {
            if (hit.collider.CompareTag("player"))
                player.GetDamage(chance >= 7 ? Damage * 2 : Damage);
            if (fireLinesCor[currentLineRenderer] != null) StopCoroutine(fireLinesCor[currentLineRenderer]);
            fireLinesCor[currentLineRenderer] = StartCoroutine(LineRendererIE(currentLineRenderer, hit.point));
        }
    }

    protected IEnumerator LineRendererIE(int index, Vector3 pos)
    {
        BulletLines[index].startColor = lineStartColor;
        BulletLines[index].endColor = lineEndColor;
        BulletLines[index].SetPosition(0, BulletPos.position);
        BulletLines[index].SetPosition(1, pos);

        yield return new WaitForSeconds(LinesLifeTime);

        BulletLines[index].startColor = new Color();
        BulletLines[index].endColor = new Color();
    }

    protected virtual void Update()
    {
        if (isFighting && !isFreeze)
        {
            bool isDonePath = Vector3.Distance(Agent.destination, transform.position) < 2.5f;
            if (isAttacker)
            {
                if (attackerGoTime <= 0 || isDonePath)
                {
                    Agent.SetDestination(GetNearPlayerPosition());
                    attackerGoTime = 1;
                }
                else attackerGoTime -= Time.deltaTime;
            }
            else
            {
                if (isDonePath && attackerGoTime <= 0)
                {
                    attackerGoTime = 10;
                    Agent.SetDestination(allSpots[Random.Range(0, allSpots.Length)].position);
                }
                else if (isDonePath) attackerGoTime -= Time.deltaTime;
            }

            if (curCooldown > 0) curCooldown -= Time.deltaTime;
            else Target();
        }
    }

    protected override void Death()
    {
        Agent.isStopped = true;
        base.Death();
    }

    public void SetFreeze(float time)
    {
        if (freezeCor != null)
            StopCoroutine(freezeCor);
        freezeCor = StartCoroutine(SetFreezeIE(time));
    }

    IEnumerator SetFreezeIE(float time)
    {
        isFreeze = true;
        Agent.isStopped = true;
        yield return new WaitForSeconds(time);
        isFreeze = false;
        Agent.isStopped = false;
    }

    public override void Hitted(float damage, AudioClip clip)
    {
        if (!isFighting) return;
        if (player.HasThisAbility(Ability.AbilityType.FreezeHead))
            SetFreeze(player.AbilityMultiply * .5f);
        base.Hitted(damage, clip);
    }
}
