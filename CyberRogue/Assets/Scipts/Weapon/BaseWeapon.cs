using System.IO;
using UnityEngine;

public abstract class BaseWeapon : PickableItem
{
    public Vector3 TargetingOffset;
    public Transform LeftHandPos;
    public Vector3 HandOffset;
    public bool HoldWithLeftHand;
    public Transform Barrel;
    public float MaxDistance;
    public LayerMask Mask;
    public float Cooldown;
    public float FireCallbackAngle, FireCallbackVector, FireCallbackTime;
    public float AccuracyMultiplier, AccuracyPlusAfterFire;
    public bool IsBurnType;
    public int MaxAmmo, InitialAmmoCount;
    public float Damage;
    public WeaponAmmo MyAmmoPrefab;
    public float AccuracyDownSpeed;

    public float ActualDamage { get { return Damage * player.DamageMultiply; } }

    protected int curAmmo, curAmmoCount;
    protected bool isAmmoInMe = true;

    private float _accuracy = 0;
    public float Accuracy
    {
        get { return _accuracy; }
        set { _accuracy = Mathf.Clamp(value * AccuracyMultiplier, 0, .7f); }
    }

    protected bool isPlayerHoldMe = false;

    protected float currentCooldown = 0;
    protected Coroutine reloadCor;

    protected override bool canPick(BasePlayer player) => true;

    protected override void PickItem(BasePlayer player)
    {
        if (player.CurrentWeapon && player.CurrentWeapon.ItemName == ItemName)
        {
            WeaponAmmo am = Instantiate(MyAmmoPrefab, transform.position, Quaternion.identity, null);
            am.AmmoCount = curAmmoCount + curAmmo;
            Destroy(gameObject);
        }
        else
        {
            player.PickNewWeapon(this);
            isPlayerHoldMe = true;
        }
    }

    protected override void DropItem()
    {
        isPlayerHoldMe = false;
    }

    public abstract bool Fire();

    protected override void AfterUpdate()
    {
        if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
        _accuracy = Mathf.Clamp(_accuracy - Time.deltaTime * AccuracyDownSpeed, 0, .7f);
    }

    protected override void AfterStart()
    {
        curAmmo = MaxAmmo;
        curAmmoCount = InitialAmmoCount;
    }

    public abstract void PlayerTookMyAmmo();

    public abstract void PlayerGiveMeAmmo();

    public virtual bool CanReloadMe() => curAmmo < MaxAmmo && curAmmoCount > 0 && isAmmoInMe && currentCooldown <= 0;

    public void PlayerSwitchedMePC(bool isIAmInHand, BasePlayer player, PCPlayerController pc)
    {
        isPlayerHoldMe = isIAmInHand;
        if (curAmmo == 0 && curAmmoCount > 0 && isPlayerHoldMe)
            ReloadAnimForPC(player.LeftHand.transform, player.RightHand.transform, player.AmmoPose, pc);
        else if (!isPlayerHoldMe && reloadCor != null) StopCoroutine(reloadCor);
    }

    public abstract void ReloadAnimForPC(Transform leftHand, Transform rightHand, Transform AmmoPos, PCPlayerController pc);

    public virtual void AddMeAmmo(int count) => curAmmoCount += count;

    public abstract string GetSaveData();

    public abstract void LoadSaveData(MyDataStream reader);
}
