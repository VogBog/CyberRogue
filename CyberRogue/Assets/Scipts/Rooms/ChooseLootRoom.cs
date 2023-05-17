using System.Collections.Generic;
using UnityEngine;

public class ChooseLootRoom : SpecialRoom
{
    [System.Serializable]
    public class Loot
    {
        public PickableItem Item;
        public int Cost;
    }

    public Transform[] LootPoses;
    public Loot[] AllLoot;

    private PickableItem[] allItemsInRoom;
    private bool isActivated = false;

    protected override void ActivateRoom()
    {
        if (isActivated || HeadRoom.isDone) return;
        isActivated = true;

        List<Loot> availableItems = new List<Loot>(AllLoot);
        for(int i = 0; i < availableItems.Count; i++)
        {
            if (availableItems[i].Item is Ability ability && player.HasThisAbility(ability.Type))
            {
                availableItems.RemoveAt(i);
                i--;
            }    
        }
        AllLoot = availableItems.ToArray();

        allItemsInRoom = new PickableItem[LootPoses.Length];

        int allCostes = 0;
        foreach(Loot loot in AllLoot)
            allCostes += loot.Cost;

        for(int pose = 0; pose < LootPoses.Length; pose++)
        {
            int randomCost = Random.Range(0, allCostes);
            for(int i = 0; i < AllLoot.Length; i++)
            {
                if (randomCost > AllLoot[i].Cost)
                    randomCost -= AllLoot[i].Cost;
                else
                {
                    bool isExist = false;
                    for(int j = 0; j < allItemsInRoom.Length; j++)
                    {
                        if (allItemsInRoom[j] != null && allItemsInRoom[j].ItemName == AllLoot[i].Item.ItemName)
                        {
                            isExist = true;
                            break;
                        }
                    }

                    if(isExist)
                    {
                        pose--;
                        break;
                    }

                    PickableItem item = Instantiate(AllLoot[i].Item, LootPoses[pose].position, Quaternion.identity, null);
                    item.HeadSpecialRoom = this;
                    allItemsInRoom[pose] = item;
                    break;
                }
            }
        }
    }

    public override void PlayerPickItem(PickableItem item)
    {
        for(int i = 0; i < allItemsInRoom.Length; i++)
        {
            if (allItemsInRoom[i] != item)
                Destroy(allItemsInRoom[i].gameObject);
        }
        HeadRoom.isDone = true;
    }
}
