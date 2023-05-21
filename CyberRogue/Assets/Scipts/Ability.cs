using TMPro;

public class Ability : PickableItem
{
    public enum AbilityType 
    { 
        None, InfinityPistol, HealAfterKill, OneMoreLife, NoFlash,
        JesusNotDie, Morningstar, FreezeHead, Shield, NoExplosions,
        OneMoreWeapon, MoreHealth, MoreBullets, ShotgunShield
    }

    public AbilityType Type;
    public string Text;
    public TextMeshPro PickText;

    protected override bool canPick(BasePlayer player)
    {
        return player.HasFreeAbilitySlot();
    }

    protected override void PickItem(BasePlayer player)
    {
        if (player.TryPickNewAbility(this))
            gameObject.SetActive(false);
    }

    protected override void DropItem()
    {
        gameObject.SetActive(true);
    }

    protected override void AfterUpdate()
    {
        transform.LookAt(player.transform.position);
    }

    protected override void AfterStart()
    {
        PickText.text = $"[ {ItemName} ]\n{Text}";
        PickText.gameObject.SetActive(false);
    }
}
