using UnityEngine;

public class OpenEndWaveCase : HandMoveObject
{
    [System.Serializable]
    public class LootObject
    {
        public GameObject Object;
        public int Cost;
    }

    public GameObject Parent;
    public Transform[] LootSpawnPoses;
    public LootObject[] Loot;

    private bool isOpened = false;

    protected override void FullOpened()
    {
        if(!isOpened)
        {
            int curSpawnPosIndx = 0;
            isOpened = true;
            int sum = 0;
            for(int i = 0; i < Loot.Length; i++)
                sum += Loot[i].Cost;
            for (int att = 0; att < LootSpawnPoses.Length; att++)
            {
                int indx = Random.Range(0, sum);
                for (int i = 0; i < Loot.Length; i++)
                {
                    if (indx >= Loot[i].Cost)
                        indx -= Loot[i].Cost;
                    else
                    {
                        Instantiate(Loot[i].Object, LootSpawnPoses[curSpawnPosIndx].position, Quaternion.identity, null);
                        curSpawnPosIndx++;
                        break;
                    }
                }
            }
        }
    }

    public override void StopDraging()
    {
        if (isOpened)
            Destroy(Parent);
    }
}
