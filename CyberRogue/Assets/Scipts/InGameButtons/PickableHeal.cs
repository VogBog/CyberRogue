public class PickableHeal : PickableItem
{
    public float Heal;

    protected override bool canPick(BasePlayer player)
    {
        return player.curHealth < player.MaxHealth || player.ChoosenInGameButton == this;
    }

    protected override void PickItem(BasePlayer player)
    {
        float curHeal = player.HasThisAbility(Ability.AbilityType.MoreHealth) ? Heal * player.AbilityMultiply : Heal;
        player.Heal(curHeal);
        Destroy(gameObject);
    }
}
