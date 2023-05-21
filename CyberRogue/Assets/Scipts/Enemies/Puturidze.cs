using System.Collections;
using UnityEngine;

public class Puturidze : BaseRifleEnemy
{
    public string BossName;
    public BaseBossText BossText;
    public float SuperAttackCooldown, SuperAttackFreezeTime;
    public ParticleSystem ElectroParticles;
    public float TimeBeforeSpawnMobs, TimeStepBetweenMobs, TimeForKill;
    public float ElectroAreasCooldown, ElectroAreasHealthStep;
    public int ElectroAreasLifeTime;
    public float ElectroAreasDamage, ElectroAreasRadius;
    public GameObject[] ElectroAreas;
    public PuturidzeElectroMob ElectroMobPrefab;
    public int MobsCount;
    public GameObject Shield;
    public AudioClip StartSuperAttack;

    private float curSuperAttackCooldown, curElectroAreasCooldown;
    private bool isUndead = false;

    public override void Hitted(float damage, AudioClip clip)
    {
        if (!isUndead)
        {
            base.Hitted(damage, clip);
            BossText.NameAndHealthTxt.text = $"{BossName}\n{Mathf.FloorToInt(Health)}";
        }
    }

    protected override void Death()
    {
        PuturidzeElectroMob[] mobs = FindObjectsOfType<PuturidzeElectroMob>();
        for (int i = 0; i < mobs.Length; i++)
            Destroy(mobs[i].gameObject);

        Game.BossIsDead();
        base.Death();
    }

    private void Start()
    {
        curSuperAttackCooldown = SuperAttackCooldown;
    }

    protected override void Update()
    {
        if (isFighting)
        {
            base.Update();
            curSuperAttackCooldown -= Time.deltaTime;
            if (curSuperAttackCooldown <= 0)
            {
                curSuperAttackCooldown = SuperAttackCooldown;
                SuperAttack();
            }
            if(Health <= ElectroAreasHealthStep)
            {
                curElectroAreasCooldown -= Time.deltaTime;
                if(curElectroAreasCooldown <= 0)
                {
                    curElectroAreasCooldown = ElectroAreasCooldown;
                    StartCoroutine(AttackElectroAreasIE());
                }
            }
        }
    }

    IEnumerator AttackElectroAreasIE()
    {
        for(int i = 0; i < ElectroAreas.Length; i++)
        {
            ElectroAreas[i].transform.position = new Vector3(player.transform.position.x, .85f, player.transform.position.z);
            ElectroAreas[i].SetActive(true);
            StartCoroutine(CurrentElectroAreaIE(i));
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator CurrentElectroAreaIE(int index)
    {
        for(int i = 0; i < ElectroAreasLifeTime; i++)
        {
            yield return new WaitForSeconds(1);
            if (Vector3.Distance(ElectroAreas[index].transform.position, player.transform.position) <= ElectroAreasRadius)
                player.GetDamage(ElectroAreasDamage);
        }
        ElectroAreas[index].SetActive(false);
    }

    private void SuperAttack()
    {
        SetFreeze(SuperAttackFreezeTime);
        ElectroParticles.Play();
        SoundSource.Stop();
        SoundSource.clip = StartSuperAttack;
        SoundSource.Play();

        StartCoroutine(SuperAttackIE());
    }

    IEnumerator SuperAttackIE()
    {
        yield return new WaitForSeconds(TimeBeforeSpawnMobs);
        for(int i = 0; i < MobsCount; i++)
        {
            PuturidzeElectroMob mob = Instantiate(ElectroMobPrefab, transform.position, Quaternion.identity, null);
            mob.StartAttack(player);
            yield return new WaitForSeconds(TimeStepBetweenMobs);
        }
        Shield.SetActive(false);
        isUndead = false;

        yield return new WaitForSeconds(TimeForKill);

        isUndead = true;
        ElectroParticles.Stop();
        Shield.SetActive(true);
    }
}
