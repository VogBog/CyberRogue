using UnityEngine;

public abstract class SpecialRoom : MonoBehaviour
{
    public LevelRoom HeadRoom;
    public GameObject[] MapIcons;

    protected BasePlayer player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("player"))
        {
            for (int i = 0; i < MapIcons.Length; i++)
            {
                if (MapIcons[i] != null)
                    MapIcons[i].SetActive(true);
            }
            player = other.GetComponent<BasePlayer>();
            HeadRoom.ActiveNearRooms();
            ActivateRoom();
        }
    }

    protected abstract void ActivateRoom();

    public abstract void PlayerPickItem(PickableItem item);
}
