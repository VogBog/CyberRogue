using DG.Tweening;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class Glock : BaseWeapon
{
    public LineRenderer[] LineRenderers;
    public float LineLifeTime;
    public ParticleSystem[] FireParticles;
    public AudioSource[] PistolAudioSources;
    public AudioSource PistolAdditionalSounds;
    public AudioClip[] ReloadSounds;
    public TextMeshPro AmmoTxt;
    public GameObject Magazine;

    public Vector3 MagazineInLeftHandLocalPos, MagazineInLeftHandLocalRot;
    public Vector3 LeftHandWhenReloadLocalPos, LeftHandWhenReloadLocalRot;

    private int audioSourceIndex;
    private float[] lineTimes;
    private int currentIndex = 0;
    private Color startColor;
    private Color endColor;
    private Vector3 magazinePos;

    public override bool Fire()
    {
        if (currentCooldown > 0) return false;
        if(curAmmo == 0)
        {
            PistolAdditionalSounds.Stop();
            PistolAdditionalSounds.clip = ReloadSounds[0];
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
        LineRenderers[currentIndex].startColor = startColor;
        LineRenderers[currentIndex].endColor = endColor;
        LineRenderers[currentIndex].SetPosition(0, Barrel.position);

        Vector3 accVec = transform.up * Random.Range(-Accuracy, Accuracy) + transform.right * Random.Range(-Accuracy, Accuracy);
        Accuracy = Accuracy / AccuracyMultiplier + AccuracyPlusAfterFire;
        Ray ray = new Ray(Barrel.position, transform.forward + accVec);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, MaxDistance, Mask))
        {
            LineRenderers[currentIndex].SetPosition(1, hit.point);
            if (hit.collider.CompareTag("destroyable"))
                hit.collider.GetComponent<DestroyablePart>().GetDamage(this, hit.point);
            else if (hit.collider.CompareTag("shottrough"))
            {
                ray = new Ray(hit.point, transform.forward + accVec);
                if (Physics.Raycast(ray, out hit, MaxDistance - Vector3.Distance(Barrel.position, hit.point), Mask))
                {
                    LineRenderers[currentIndex].SetPosition(1, hit.point);
                    if (hit.collider.CompareTag("destroyable"))
                    {
                        float initDamage = Damage;
                        Damage /= 3;
                        hit.collider.GetComponent<DestroyablePart>().GetDamage(this, hit.point);
                        Damage = initDamage;
                    }
                }
                else LineRenderers[currentIndex].SetPosition(1, Barrel.position + (transform.forward + accVec) * MaxDistance);
            }
        }
        else LineRenderers[currentIndex].SetPosition(1, Barrel.position + (transform.forward + accVec) * MaxDistance);

        lineTimes[currentIndex] = LineLifeTime;
        currentIndex = (currentIndex + 1) % LineRenderers.Length;

        return true;
    }

    protected override void AfterStart()
    {
        base.AfterStart();
        AmmoTxt.gameObject.SetActive(false);
        lineTimes = new float[LineRenderers.Length];
        startColor = LineRenderers[0].startColor;
        endColor = LineRenderers[0].endColor;
        for(int i = 0; i < LineRenderers.Length; i++)
        {
            LineRenderers[i].startColor = new Color();
            LineRenderers[i].endColor = new Color();
        }
        for (int i = 0; i < FireParticles.Length; i++)
            FireParticles[i].transform.SetParent(null);
        magazinePos = Magazine.transform.localPosition;
    }

    protected override void AfterUpdate()
    {
        for(int i = 0; i < LineRenderers.Length; i++)
        {
            if (lineTimes[i] > 0) lineTimes[i] -= Time.deltaTime;
            else
            {
                LineRenderers[i].startColor = new Color();
                LineRenderers[i].endColor = new Color();
            }
        }
        if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
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
        PistolAdditionalSounds.clip = ReloadSounds[2];
        PistolAdditionalSounds.Play();

        curAmmo = Mathf.Clamp(curAmmoCount, 0, MaxAmmo);
        curAmmoCount -= curAmmo;
        AmmoTxt.text = $"{curAmmo}/{curAmmoCount}";
        Magazine.transform.SetParent(transform);
        Magazine.transform.localRotation = new Quaternion();
        Magazine.transform.localPosition = magazinePos;
    }

    public override void PlayerTookMyAmmo()
    {
        isAmmoInMe = false;
        curAmmoCount += curAmmo;
        if (player.HasThisAbility(Ability.AbilityType.InfinityPistol))
            curAmmoCount += MaxAmmo - curAmmo;
        curAmmo = 0;
        AmmoTxt.text = $"0/{curAmmoCount}";

        PistolAdditionalSounds.Stop();
        PistolAdditionalSounds.clip = ReloadSounds[1];
        PistolAdditionalSounds.Play();

        GameObject decal = Instantiate(Magazine, Magazine.transform.position, Magazine.transform.rotation, null);
        decal.transform.localScale = Magazine.transform.lossyScale;
        Rigidbody decalBody = decal.AddComponent<Rigidbody>();
        decalBody.useGravity = true;
        Destroy(decal, 1);
    }

    public override void ReloadAnimForPC(Transform leftHand, Transform rightHand, Transform AmmoPos, PCPlayerController pc)
    {
        if (reloadCor != null) StopCoroutine(reloadCor);
        reloadCor = StartCoroutine(ReloadAnimForPCIE(leftHand, rightHand, AmmoPos, pc));
    }

    protected virtual IEnumerator ReloadAnimForPCIE(Transform leftHand, Transform rightHand, Transform AmmoPos, PCPlayerController pc)
    {
        if (!isAmmoInMe)
            yield return new WaitForSeconds(.5f);

        if (isPlayerHoldMe)
        {
            pc.SetReloadAnim(true);

            Vector3 rightHandPos = rightHand.localPosition + new Vector3(-.1f, .1f, 0);
            bool OKLetsGo = false;

            if (isAmmoInMe)
            {
                rightHand.DOLocalRotate(new Vector3(-50, -30, 0), .5f);
                rightHand.DOLocalMove(rightHandPos, .5f);
                yield return new WaitForSeconds(.5f);
                rightHand.Rotate(new Vector3(50, 0, 0), Space.Self);
                PlayerTookMyAmmo();
                rightHand.DOLocalRotate(new Vector3(-50, -30, 0), .6f);
                OKLetsGo = true;
            }

            if (isPlayerHoldMe)
            {
                if (!OKLetsGo)
                {
                    rightHand.DOLocalMove(rightHandPos, .1f);
                    rightHand.DOLocalRotate(new Vector3(-50, -30, 0), .1f);
                }

                Magazine.transform.SetParent(AmmoPos);
                Magazine.transform.position = AmmoPos.position;
                Magazine.transform.rotation = new Quaternion();

                leftHand.DOLocalMove(AmmoPos.localPosition + Vector3.down / 2, .3f);

                yield return new WaitForSeconds(.3f);

                if (isPlayerHoldMe)
                {
                    Quaternion rightHandRot = Quaternion.Euler(new Vector3(-50, -30, 0));

                    StartCoroutine(ReloadAmmoAnimPCState1(leftHand, rightHand));
                    yield return new WaitForSeconds(.6f);

                    if (isPlayerHoldMe)
                    {
                        PlayerGiveMeAmmo();
                        isAmmoInMe = false;
                        leftHand.DOLocalMove(new Vector3(-1, -1, 0), .2f);
                        pc.UpdateRightHandAnim(true);
                        pc.UpdateLeftHandAnim(HoldWithLeftHand);
                        pc.SetReloadAnim(false);
                        rightHand.DOLocalRotate(Vector3.zero, .2f);

                        yield return new WaitForSeconds(.1f);

                        isAmmoInMe = true;
                    }
                }
            }
        }
    }

    protected virtual IEnumerator ReloadAmmoAnimPCState1(Transform leftHand, Transform rightHand)
    {
        Magazine.transform.SetParent(leftHand);
        Magazine.transform.localPosition = MagazineInLeftHandLocalPos;
        Magazine.transform.localRotation = Quaternion.Euler(MagazineInLeftHandLocalRot);
        leftHand.localPosition += Vector3.down / 2;
        leftHand.localRotation = Quaternion.Euler(LeftHandWhenReloadLocalRot);
        leftHand.DOLocalMove(LeftHandWhenReloadLocalPos + Vector3.down / 3, .6f);

        yield return new WaitForSeconds(.5f);
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
