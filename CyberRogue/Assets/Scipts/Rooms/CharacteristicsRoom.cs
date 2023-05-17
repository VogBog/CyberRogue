using TMPro;
using UnityEngine;

public class CharacteristicsRoom : SpecialRoom
{
    public TextMeshPro CountTxt;
    public Transform CardSpawnPoint;
    public PlayerCharacteristicsCard CardPrefab;

    private int attempts = 5;
    private PlayerCharacteristicsCard myCard;

    public override void PlayerPickItem(PickableItem item)
    {
        CountTxt.gameObject.SetActive(false);
        attempts = 0;
        HeadRoom.isDone = true;
    }

    protected override void ActivateRoom()
    {
        if (HeadRoom.isDone) return;
        CountTxt.text = attempts.ToString();
        CountTxt.gameObject.SetActive(true);
    }

    public void SwitchOpened()
    {
        if(attempts > 0 && !HeadRoom.isDone)
        {
            attempts--;
            CountTxt.text = attempts.ToString();
            if(myCard != null)
                Destroy(myCard.gameObject);
            myCard = Instantiate(CardPrefab, CardSpawnPoint.position, Quaternion.identity);
            myCard.HeadSpecialRoom = this;
        }
        if (attempts == 0)
            HeadRoom.isDone = false;
    }
}
