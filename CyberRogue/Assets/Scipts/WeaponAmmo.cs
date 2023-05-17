using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAmmo : PickableItem
{
    public int AmmoCount;
    public string WeaponName;

    protected override bool canPick(BasePlayer player) => player.HasWeaponWithName(WeaponName);

    protected override void PickItem(BasePlayer player)
    {
        player.PickAmmoForWeapon(this);
    }
}
