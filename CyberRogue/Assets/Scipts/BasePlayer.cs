using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BasePlayer : MonoBehaviour
{
    public CharacterController CharacterController;
    public Transform PhysiscSphere;
    public Transform CameraPos;
    public float MaxSpeed;
    public float Gravity;
    public float ChooseInGameUIDistance;
    public float MaxHealth;
    public LayerMask GravityMask;
    public LayerMask ChooseInGameUIMask;
    public GameObject LeftHand, RightHand;
    public Animator LeftHandAnimator, RightHandAnimator;
    public GameObject MiniMap;
    public AudioSource MusicSource;

    public GameObject SettingsGO;
    public TextMeshPro Notice, HealthTxt;
    private Coroutine noticeCoroutine, undeadCor; //Visual studio подчёркивает, что переменная не используется, но Visual Studio верить нельзя
    private List<string> noticeList = new List<string>();
    private Vector3 noticeStartPos;
    private bool isUndead = false;
    private Coroutine musicCor;

    public Transform[] WeaponPoses;
    public Transform AmmoPose;

    public AudioSource PlayerAudioSource, StepsAudioSource, HitAudioSource, BlindAudioSource;
    public AudioClip TookWeaponClip, TookItemClip, TookAmmoClip;
    public AudioClip[] StepSounds;
    public AudioClip[] HitSounds;
    public AudioClip HealSound, UndeadSound;

    [Header("PC Moments")]
    public Transform DefaultLeftHandPos;
    public Transform MapLeftHandPos, DefaultRightHandPos, FightRightHandPos;

    public Transform MainCamera { get; private set; }
    private Transform MiniMapCamera;
    private GameManager game;

    private PlayerController controller;
    private Settings settings;

    protected BaseWeapon[] weapons;
    private int currentWeaponIndex = 0;
    protected Ability[] abilities;

    //Properties

    public float curHealth { get; protected set; }

    public bool isFighting { get; set; } = false;
    public bool isSettingsOpened { get; private set; } = false;

    public float CurSpeed { get; private set; }
    public GameObject ChoosenInGameButtonGO { get; private set; }
    public InGameButton ChoosenInGameButton { get; private set; }
    public GameObject PCCursor { get; private set; }
    public GameObject PCChooseCursor { get; private set; }
    public bool HasAnyWeapon
    {
        get
        {
            for(int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i]) return true;
            }
            return false;
        }
    }
    public BaseWeapon CurrentWeapon { get { return weapons[currentWeaponIndex]; } }
    public bool HasExtraLife { get; private set; }
    public float AbilityMultiply { get; protected set; }
    public float DamageMultiply { get; private set; }

    private void Start()
    {
        game = FindObjectOfType<GameManager>();

        MainCamera = game.MainCamera.transform;
        MainCamera.position = CameraPos.position;
        controller = transform.AddComponent<PCPlayerController>();
        controller.SetData(this);
        CurSpeed = MaxSpeed;
        settings = SettingsGO.GetComponentInChildren<Settings>();
        noticeStartPos = Notice.transform.localPosition;
        settings.SetPlayer(this);
        MiniMapCamera = game.MiniMapCamera.transform;
        MiniMapCamera.SetParent(transform);
        MiniMapCamera.localPosition = new Vector3(0, 10, 0);
        PCCursor = game.PCCursor;
        PCChooseCursor = game.PCChooseCursor;
        weapons = new BaseWeapon[3];
        abilities = new Ability[5];
        curHealth = MaxHealth;
        AbilityMultiply = 1;
        DamageMultiply = 1;
    }

    public bool HasFreeAbilitySlot()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] == null)
                return true;
        }
        return false;
    }

    public bool TryPickNewAbility(Ability ability)
    {
        if (HasThisAbility(ability.Type))
            return false;

        for(int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] == null)
            {
                abilities[i] = ability;

                if(ability.Type == Ability.AbilityType.OneMoreWeapon)
                {
                    BaseWeapon[] newWeaponArr = new BaseWeapon[weapons.Length + 1];
                    for (int weapon = 0; weapon < weapons.Length; weapon++)
                        newWeaponArr[weapon] = weapons[weapon];
                    weapons = newWeaponArr;
                }

                return true;
            }
        }
        return false;
    }

    public bool HasThisAbility(Ability.AbilityType type)
    {
        for(int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null && abilities[i].Type == type)
                return true;
        }
        return false;
    }

    public string[] GetAbilitiesTexts()
    {
        List<string> list = new List<string>();
        for(int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null)
                list.Add($"[ {abilities[i].ItemName} ] {abilities[i].Text}");
            else
                list.Add("[ Пусто ]");
        }
        return list.ToArray();
    }

    public void ChooseInGameButton(GameObject button)
    {
        if(button != null)
        {
            ChoosenInGameButtonGO = button;
            if(ChoosenInGameButtonGO.TryGetComponent(out InGameButton btn))
                ChoosenInGameButton = btn;
            else ChoosenInGameButton = ChoosenInGameButtonGO.GetComponentInParent<InGameButton>();
            ChoosenInGameButton.OnChoosen();
            PCChooseCursor.SetActive(true);
        }
        else
        {
            ChoosenInGameButton.OnUnchoosen();
            ChoosenInGameButtonGO = null;
            ChoosenInGameButton = null;
            PCChooseCursor.SetActive(false);
        }
    }

    public void PlayStepSound()
    {
        StepsAudioSource.Stop();
        StepsAudioSource.clip = StepSounds[Random.Range(0, StepSounds.Length)];
        StepsAudioSource.Play();
    }

    public void OnSettingsButtonPressed()
    {
        if(!isFighting)
        {
            isSettingsOpened = !isSettingsOpened;
            SettingsGO.SetActive(isSettingsOpened);
            settings.UpdateInfo();
            UpdateHandAnimation(true, false);
        }
    }

    public void SetNotice(string message)
    {
        noticeList.Add(message);
        noticeCoroutine ??= StartCoroutine(NoticeIE());
    }

    IEnumerator NoticeIE()
    {
        Notice.gameObject.SetActive(true);
        while(noticeList.Count > 0)
        {
            Notice.transform.localPosition = noticeStartPos + Vector3.down;
            Notice.transform.DOLocalMove(noticeStartPos, .3f);

            Notice.text = noticeList[0];
            noticeList.RemoveAt(0);

            yield return new WaitForSeconds(3);
        }
        Notice.gameObject.SetActive(false);
        noticeCoroutine = null;
    }

    public void PickNewWeapon(BaseWeapon weapon)
    {
        if(currentWeaponIndex == weapons.Length - 1)
        {
            currentWeaponIndex = 0;
            for(int i = 0; i < weapons.Length - 1; i++)
            {
                if (weapons[i] == null)
                {
                    currentWeaponIndex = i;
                    break;
                }
            }
        }
        if (weapons[currentWeaponIndex])
            DropCurrentWeapon();
        weapons[currentWeaponIndex] = weapon;
        weapon.transform.SetParent(RightHand.transform);
        weapon.transform.localPosition = weapon.HandOffset;
        weapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
        PCCursor.SetActive(false);
        UpdateHandAnimation(true, true);

        PlayerAudioSource.Stop();
        PlayerAudioSource.clip = TookWeaponClip;
        PlayerAudioSource.Play();
    }

    public void DropCurrentWeapon()
    {
        weapons[currentWeaponIndex].transform.SetParent(null);
        weapons[currentWeaponIndex].PlayerDropMe();
        weapons[currentWeaponIndex] = null;
        PCCursor.SetActive(true);
        UpdateHandAnimation(true, true);

        PlayerAudioSource.Stop();
        PlayerAudioSource.clip = TookWeaponClip;
        PlayerAudioSource.Play();
    }

    public void TakeMyWeaponVR(int index)
    {
        //Its for VR
    }

    public void SwapWeaponPC()
    {
        if (CurrentWeapon)
        {
            CurrentWeapon.transform.SetParent(WeaponPoses[currentWeaponIndex]);
            CurrentWeapon.transform.DOLocalMove(WeaponPoses[currentWeaponIndex].localPosition, .2f);
            CurrentWeapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
            CurrentWeapon.transform.Rotate(transform.right, 90);
            CurrentWeapon.PlayerSwitchedMePC(false, this, (PCPlayerController)controller);
        }
        currentWeaponIndex = (currentWeaponIndex + 1) % (weapons.Length - 1);
        if (CurrentWeapon)
        {
            CurrentWeapon.transform.SetParent(RightHand.transform);
            CurrentWeapon.transform.DOLocalMove(CurrentWeapon.HandOffset, .2f);
            CurrentWeapon.transform.rotation = new Quaternion(0, 0, 0, 0);
            PCCursor.SetActive(false);
            CurrentWeapon.PlayerSwitchedMePC(true, this, (PCPlayerController)controller);
        }
        else PCCursor.SetActive(true);
        StartCoroutine(SwapWeaponPCIE());
        RightHand.transform.DOComplete();
        RightHand.transform.DOLocalRotate(Vector3.zero, .1f);

        PlayerAudioSource.Stop();
        PlayerAudioSource.clip = TookWeaponClip;
        PlayerAudioSource.Play();
    }

    public void ChooseFreeHandForPC()
    {
        if (CurrentWeapon)
        {
            CurrentWeapon.transform.SetParent(WeaponPoses[currentWeaponIndex]);
            CurrentWeapon.transform.DOLocalMove(WeaponPoses[currentWeaponIndex].localPosition, .2f);
            CurrentWeapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
            CurrentWeapon.transform.Rotate(transform.right, 90);
            CurrentWeapon.PlayerSwitchedMePC(false, this, (PCPlayerController)controller);
        }
        currentWeaponIndex = weapons.Length - 1;

        PCCursor.SetActive(true);
        StartCoroutine(SwapWeaponPCIE());
        RightHand.transform.DOComplete();
        RightHand.transform.DOLocalRotate(Vector3.zero, .1f);

        PlayerAudioSource.Stop();
        PlayerAudioSource.clip = TookWeaponClip;
        PlayerAudioSource.Play();
    }

    IEnumerator SwapWeaponPCIE()
    {
        RightHand.transform.DOLocalMove(DefaultRightHandPos.localPosition - CameraPos.localPosition, .1f);
        yield return new WaitForSeconds(.1f);
        controller.UpdateRightHandAnim(CurrentWeapon, .1f);
        controller.UpdateLeftHandAnim(CurrentWeapon && CurrentWeapon.HoldWithLeftHand, .1f);
    }

    private void UpdateHandAnimation(bool leftHand, bool rightHand)
    {
        if(leftHand)
        {
            int leftHandState = isSettingsOpened ? 1 : ((CurrentWeapon && CurrentWeapon.HoldWithLeftHand) ? 3 : 0);
            LeftHandAnimator.SetInteger("State", leftHandState);
            controller.UpdateLeftHandAnim(CurrentWeapon && CurrentWeapon.HoldWithLeftHand);
        }
        if(rightHand)
        {
            RightHandAnimator.SetInteger("State", CurrentWeapon ? 2 : 0);
            controller.UpdateRightHandAnim(CurrentWeapon);
        }
    }

    public bool HasWeaponWithName(string name)
    {
        for(int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] && weapons[i].ItemName == name)
                return true;
        }
        return false;
    }

    public void PickAmmoForWeapon(WeaponAmmo ammo)
    {
        PlayerAudioSource.Stop();
        PlayerAudioSource.clip = TookAmmoClip;
        PlayerAudioSource.Play();

        for(int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] && weapons[i].ItemName == ammo.WeaponName)
            {
                int count = HasThisAbility(Ability.AbilityType.MoreBullets) ? Mathf.FloorToInt(ammo.AmmoCount * AbilityMultiply) : ammo.AmmoCount;
                weapons[i].AddMeAmmo(count);
                Destroy(ammo.gameObject);
                break;
            }
        }
    }

    private bool isShieldActivate()
    {
        return HasThisAbility(Ability.AbilityType.Shield) && Random.Range(0, 100) <= AbilityMultiply * 10;
    }

    public void GetDamage(float damage)
    {
        if (!isUndead && !isShieldActivate())
        {
            curHealth -= damage;
            HealthTxt.text = Mathf.RoundToInt(curHealth).ToString();
            float g = curHealth / MaxHealth;
            HealthTxt.color = new Color(1 - g, g, 0, 1);

            HitAudioSource.Stop();
            HitAudioSource.clip = HitSounds[Random.Range(0, HitSounds.Length)];
            HitAudioSource.Play();

            if (curHealth <= 0)
            {
                if (HasExtraLife)
                {
                    if (HasThisAbility(Ability.AbilityType.OneMoreLife))
                        curHealth = 20 + 10 * AbilityMultiply;
                    else curHealth = 1;

                    if (HasThisAbility(Ability.AbilityType.JesusNotDie))
                        SetUndead(AbilityMultiply * 3);

                    HasExtraLife = false;
                }
                else
                {
                    game.PlayerDead();
                    curHealth = MaxHealth;
                    SetNotice("Вы погибли!");
                }
            }
        }
        else
        {
            HitAudioSource.Stop();
            HitAudioSource.clip = UndeadSound;
            HitAudioSource.Play();
        }
    }

    public void Heal(float heal)
    {
        curHealth = Mathf.Clamp(curHealth + heal, 0, MaxHealth);
        HealthTxt.text = Mathf.RoundToInt(curHealth).ToString();
        float g = curHealth / MaxHealth;
        HealthTxt.color = new Color(1 - g, g, 0, 1);

        PlayerAudioSource.Stop();
        PlayerAudioSource.clip = HealSound;
        PlayerAudioSource.Play();
    }

    public void StartWave()
    {
        isFighting = true;
        HasExtraLife = HasThisAbility(Ability.AbilityType.OneMoreLife) || HasThisAbility(Ability.AbilityType.JesusNotDie);
    }

    public void EndWave()
    {
        isFighting = false;
    }

    public void EnemyKilled()
    {
        if (HasThisAbility(Ability.AbilityType.HealAfterKill))
            Heal(10 * AbilityMultiply);
    }

    public void TookCharacteristicsCard(PlayerCharacteristicsCard card)
    {
        MaxHealth += card.Health;
        if (curHealth > MaxHealth)
            GetDamage(MaxHealth - curHealth);
        if (MaxHealth <= 0) GetDamage(1);
        DamageMultiply += card.Damage;
        AbilityMultiply += card.Abilities;
    }

    public void SetUndead(float time)
    {
        if (undeadCor != null) StopCoroutine(undeadCor);
        undeadCor = StartCoroutine(UndeadIE(time));
    }

    IEnumerator UndeadIE(float time)
    {
        isUndead = true;
        yield return new WaitForSeconds(time);
        isUndead = false;
    }

    public string GetWeaponSaveData()
    {
        string res = weapons.Length + "\n";
        for(int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                res += "None\n";
                continue;
            }
            if (weapons[i].ItemName == "Glock")
            {
                if (weapons[i].MaxAmmo < 15)
                    res += "USP\n";
                else res += "Glock\n";
            }
            else res += weapons[i].ItemName + "\n";
            res += weapons[i].GetSaveData();
            res += "Next\n";
        }
        res += "End weapon\n";
        res += abilities.Length + "\n";
        for(int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null)
                res += (int)abilities[i].Type + " ";
            else res += "0 ";
        }
        return res;
    }

    public void ReadAndApplyData(MyDataStream reader)
    {
        string line = reader.ReadLine();
        string[] data = line.Split();
        if (float.TryParse(data[0], out float res))
            MaxHealth = res;
        if (float.TryParse(data[1], out res))
            curHealth = res;
        else curHealth = MaxHealth;
        if (float.TryParse(data[2], out res))
            DamageMultiply = res;
        if (float.TryParse(data[3], out res))
            AbilityMultiply = res;

        line = reader.ReadLine();
        if (!int.TryParse(line, out int count))
            count = 3;
        weapons = new BaseWeapon[count];
        for(int i = 0; i <= count; i++)
        {
            line = reader.ReadLine();
            if (line == "End weapon") break;
            else if (line == "None")
                continue;
            BaseWeapon wpn;
            if (line == "USP")
                wpn = Instantiate(StaticSaveData.USP, transform.position, Quaternion.identity, null);
            else if (line == "Glock")
                wpn = Instantiate(StaticSaveData.Glock, transform.position, Quaternion.identity, null);
            else
                wpn = Instantiate(StaticSaveData.GetWeaponByName(line), transform.position, Quaternion.identity, null);
            wpn.LoadSaveData(reader);
            wpn.OnClick(this);
            SwapWeaponPC();
            reader.ReadLine();
        }

        if (!int.TryParse(reader.ReadLine(), out count))
            count = 5;
        abilities = new Ability[count];
        line = reader.ReadLine();
        data = line.Split();
        Ability ability;
        for(int i = 0; i < count; i++)
        {
            if(i < data.Length)
            {
                int type;
                int.TryParse(data[i], out type);
                if (type != 0)
                {
                    Ability.AbilityType a_type = (Ability.AbilityType)type;
                    ability = Instantiate(StaticSaveData.GetAbilityByType(a_type), transform.position, Quaternion.identity, null);
                    ability.OnClick(this);
                }
            }
        }
    }

    public void PlayMusic(AudioClip clip, bool isLoop)
    {
        if (musicCor != null) StopCoroutine(musicCor);
        musicCor = StartCoroutine(PlayMusicIE(clip, isLoop));
    }

    public void StopMusic()
    {
        if (musicCor != null) StopCoroutine(musicCor);
        musicCor = StartCoroutine(StopMusicIE());
    }

    IEnumerator PlayMusicIE(AudioClip clip, bool isLoop)
    {
        MusicSource.DOFade(0, 2);
        yield return new WaitForSeconds(2);
        MusicSource.Stop();
        MusicSource.clip = clip;
        MusicSource.loop = isLoop;
        MusicSource.Play();
        MusicSource.volume = 1;
    }

    IEnumerator StopMusicIE()
    {
        MusicSource.DOFade(0, 2);
        yield return new WaitForSeconds(2);
        MusicSource.Stop();
    }
}
