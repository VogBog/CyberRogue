using TMPro;
using UnityEngine;

public class BaseBossText : MonoBehaviour
{
    public TextMeshPro NameAndHealthTxt;
    public Vector3 Offset;
    public Transform Boss;

    private Transform player;

    private void Update()
    {
        if (player != null)
        {
            transform.LookAt(player.position);
            transform.Rotate(Vector3.up, 180);
            transform.position = Boss.position + Offset;
        }
        else player = GameObject.FindGameObjectWithTag("player").transform;
    }
}
