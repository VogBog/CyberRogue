using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Shotgun : BaseWeapon
{
    [Serializable]
    public class FireLineRenderers
    {
        public LineRenderer[] Lines;

        public void SetStartColor(Color startColor, Color endColor)
        {
            SetStartPoses(startColor, endColor, Vector3.zero);
        }

        public void SetStartPoses(Color startColor, Color endColor, Vector3 pos)
        {
            for(int i = 0; i < Lines.Length; i++)
            {
                Lines[i].startColor = startColor;
                Lines[i].endColor = endColor;
                Lines[i].SetPosition(0, pos);
            }
        }

        public void SetPosition(int index, Vector3 pos)
        {
            Lines[index].SetPosition(1, pos);
        }
    }

    public FireLineRenderers[] LineRenderers;
    public float LineLifeTime;
    public ParticleSystem[] FireParticles;
    public AudioSource[] PistolAudioSources;
    public AudioSource PistolAdditionalSounds;
    public AudioClip[] ReloadSounds;
    public TextMeshPro AmmoTxt;
    public int OneFireBulletsCount;

    public Vector3 LeftHandWhenReloadLocalPos, LeftHandWhenReloadLocalRot;

    private int audioSourceIndex;
    private float[] lineTimes;
    private int currentIndex = 0;
    private Color startColor;
    private Color endColor;
    private float lastFireTime = 0;

    private PCPlayerController PC;

    public override bool Fire()
    {
        lastFireTime = 5;
        if (PC != null)
        {
            PC.UpdateLeftHandAnim(HoldWithLeftHand);
            PC = null;
        }
        if (reloadCor != null) StopCoroutine(reloadCor);

        if (currentCooldown > 0) return false;
        if (curAmmo == 0)
        {
            PistolAdditionalSounds.Stop();
            PistolAdditionalSounds.clip = ReloadSounds[0]; //Это чё за пиздец, кто это написал? Я?
            PistolAdditionalSounds.Play();
            return false;
        }
        currentCooldown = Cooldown;
        curAmmo--;
        AmmoTxt.text = $"{curAmmo}/{curAmmoCount}";

        for (int i = 0; i < FireParticles.Length; i++)
        {
            FireParticles[i].transform.position = Barrel.position;
            FireParticles[i].transform.rotation = Barrel.rotation;
            FireParticles[i].Stop();
            FireParticles[i].Play();
        }

        PistolAudioSources[audioSourceIndex].Play();
        audioSourceIndex = (audioSourceIndex + 1) % PistolAudioSources.Length;
        LineRenderers[currentIndex].SetStartPoses(startColor, endColor, Barrel.position);

        for (int i = 0; i < OneFireBulletsCount; i++)
        {
            Vector3 accVec = transform.up * UnityEngine.Random.Range(-Accuracy, Accuracy) + transform.right * UnityEngine.Random.Range(-Accuracy, Accuracy);
            Ray ray = new Ray(Barrel.position, transform.forward + accVec);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, MaxDistance, Mask))
            {
                LineRenderers[currentIndex].SetPosition(i, hit.point);
                if (hit.collider.CompareTag("destroyable"))
                    hit.collider.GetComponent<DestroyablePart>().GetDamage(this, hit.point);
                else if (hit.collider.CompareTag("shottrough"))
                {
                    ray = new Ray(hit.point, transform.forward + accVec);
                    if (Physics.Raycast(ray, out hit, MaxDistance - Vector3.Distance(Barrel.position, hit.point), Mask))
                    {
                        LineRenderers[currentIndex].SetPosition(i, hit.point);
                        if (hit.collider.CompareTag("destroyable"))
                        {
                            float initDamage = Damage;
                            Damage /= 3;
                            hit.collider.GetComponent<DestroyablePart>().GetDamage(this, hit.point);
                            Damage = initDamage;
                        }
                    }
                    else LineRenderers[currentIndex].SetPosition(i, Barrel.position + (transform.forward + accVec) * MaxDistance);
                }
            }
            else LineRenderers[currentIndex].SetPosition(i, Barrel.position + (transform.forward + accVec) * MaxDistance);
        }
        Accuracy = Accuracy / AccuracyMultiplier + AccuracyPlusAfterFire;

        lineTimes[currentIndex] = LineLifeTime;
        currentIndex = (currentIndex + 1) % LineRenderers.Length;

        return true;
    }

    public override bool IsShotgunShieldActivate() => lastFireTime > 0;

    protected override void AfterStart()
    {
        base.AfterStart();
        lineTimes = new float[LineRenderers.Length];
        startColor = LineRenderers[0].Lines[0].startColor;
        endColor = LineRenderers[0].Lines[0].endColor;
        for (int i = 0; i < LineRenderers.Length; i++)
        {
            LineRenderers[i].SetStartColor(new Color(), new Color());
        }
        for (int i = 0; i < FireParticles.Length; i++)
            FireParticles[i].transform.SetParent(null);
    }

    protected override void AfterUpdate()
    {
        for (int i = 0; i < LineRenderers.Length; i++)
        {
            if (lineTimes[i] > 0)
                lineTimes[i] -= Time.deltaTime;
            else
                LineRenderers[i].SetStartColor(new Color(), new Color());
        }
        if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
        if (lastFireTime > 0) lastFireTime -= Time.deltaTime;
        Accuracy = Mathf.Clamp(Accuracy / AccuracyMultiplier - Time.deltaTime * AccuracyDownSpeed, 0, .7f);
    }

    protected override void PickItem(BasePlayer player)
    {
        base.PickItem(player);
        AmmoTxt.gameObject.SetActive(true);
        AmmoTxt.text = $"{curAmmo}/{curAmmoCount}";
    }

    protected override void DropItem()
    {
        base.DropItem();
        AmmoTxt.gameObject.SetActive(false);
    }

    public override void PlayerGiveMeAmmo()
    {
        PistolAdditionalSounds.Stop();
        PistolAdditionalSounds.clip = ReloadSounds[1];
        PistolAdditionalSounds.Play();

        curAmmo = Mathf.Clamp(curAmmo + 1, 0, MaxAmmo);
        curAmmoCount--;
        AmmoTxt.text = $"{curAmmo}/{curAmmoCount}";
    }

    public override void PlayerTookMyAmmo() { }

    public override void ReloadAnimForPC(Transform leftHand, Transform rightHand, Transform AmmoPos, PCPlayerController pc)
    {
        if (reloadCor != null) StopCoroutine(reloadCor);
        reloadCor = StartCoroutine(ReloadAnimForPCIE(leftHand, rightHand, AmmoPos, pc));
    }

    protected virtual IEnumerator ReloadAnimForPCIE(Transform leftHand, Transform rightHand, Transform AmmoPos, PCPlayerController pc)
    {
        PC = pc;
        if (isPlayerHoldMe)
        {
            pc.SetReloadAnim(true);

            while (curAmmo < MaxAmmo && curAmmoCount > 0 && isPlayerHoldMe)
            {
                if (isPlayerHoldMe)
                {
                    leftHand.DOLocalMove(AmmoPos.localPosition + Vector3.down / 2, .3f);

                    yield return new WaitForSeconds(.3f);

                    if (isPlayerHoldMe)
                    {
                        StartCoroutine(ReloadAmmoAnimPCState1(leftHand, rightHand));
                        yield return new WaitForSeconds(.3f);

                        if (isPlayerHoldMe)
                        {
                            PlayerGiveMeAmmo();
                            leftHand.DOLocalMove(new Vector3(-1, -1, 0), .2f);
                            pc.UpdateRightHandAnim(true);
                            pc.SetReloadAnim(false);
                            rightHand.DOLocalRotate(Vector3.zero, .2f);

                            yield return new WaitForSeconds(.1f);

                            isAmmoInMe = true;
                        }
                    }
                }
            }
            pc.UpdateLeftHandAnim(HoldWithLeftHand);
            PC = null;
        }
    }

    protected virtual IEnumerator ReloadAmmoAnimPCState1(Transform leftHand, Transform rightHand)
    {
        leftHand.localPosition += Vector3.down / 2;
        leftHand.localRotation = Quaternion.Euler(LeftHandWhenReloadLocalRot);
        leftHand.DOLocalMove(LeftHandWhenReloadLocalPos + Vector3.down / 3, .2f);

        yield return new WaitForSeconds(.2f);
        leftHand.DOLocalMove(LeftHandWhenReloadLocalPos, .1f);
    }

    public override void AddMeAmmo(int count)
    {
        base.AddMeAmmo(count);
        AmmoTxt.text = $"{curAmmo}/{curAmmoCount}";
    }

    public override bool CanReloadMe() => base.CanReloadMe() || player.HasThisAbility(Ability.AbilityType.InfinityPistol);

    public override string GetSaveData()
    {
        return $"{curAmmo} {curAmmoCount}\n";
    }

    public override void LoadSaveData(MyDataStream reader)
    {
        string[] splittedData = reader.ReadLine().Split();
        StartCoroutine(AfterLoadSaveData(splittedData));
    }

    IEnumerator AfterLoadSaveData(string[] splittedData)
    {
        yield return null;
        yield return null;
        yield return null;

        if (int.TryParse(splittedData[0], out int num))
            curAmmo = num;
        else curAmmo = MaxAmmo;
        if (int.TryParse(splittedData[1], out int num2))
            curAmmoCount = num2;
        else curAmmoCount = InitialAmmoCount;

        AmmoTxt.text = $"{curAmmo}/{curAmmoCount}";
    }
}
