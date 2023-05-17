using TMPro;
using UnityEngine;

public class PlayerCharacteristicsCard : PickableItem
{
    public TextMeshPro Text;

    public float Health { get; private set; } = 0;
    public float Damage { get; private set; } = 0;
    public float Abilities { get; private set; } = 0;

    protected override bool canPick(BasePlayer player) => true;

    protected override void AfterStart()
    {
        int characteristicsCount = Random.Range(2, 4);
        float[] data = new float[] { 0, 0, 0 };
        float sum = 0;
        for (int i = 0; i < characteristicsCount; i++)
        {
            int index = -1;
            for(int j = 0; j < 100; j++)
            {
                index = Random.Range(0, 3);
                if (data[index] == 0) break;
            }

            float range = Random.Range(.2f, .5f);
            if (Random.Range(0, 2) == 0)
                range *= -1;

            if (i == characteristicsCount - 1)
            {
                data[index] = index == 0 ? -sum * 80 : -sum;
                if (data[index] < 0) data[index] *= .65f;
                range *= .35f;
            }

            data[index] += index == 0 ? range * 80 : range;
            if (data[index] < 0)
                data[index] *= .65f;
            sum += range;
        }

        Health = data[0];
        Damage = data[1];
        Abilities = data[2];

        Text.text = SetStringByNumber("Здоровье: ", Health) + SetStringByNumber("Урон: ", Damage) + SetStringByNumber("Способности: ", Abilities);
    }

    private string SetStringByNumber(string text, float num)
    {
        if (num == 0)
            return "";
        else if (num > 0)
            return $"{text}<color=green>+{num:0.0}</color>\n";
        else
            return $"{text}<color=red>-{-num:0.0}</color>\n";
    }

    protected override void PickItem(BasePlayer player)
    {
        player.TookCharacteristicsCard(this);
        Destroy(gameObject);
    }

    protected override void AfterUpdate()
    {
        transform.LookAt(player.transform.position);
    }
}
