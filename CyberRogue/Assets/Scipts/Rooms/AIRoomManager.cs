using UnityEngine;

public class AIRoomManager : AIManager
{
    public GameObject[] CasePrefabs;
    public LevelRoom HeadRoom;
    public AudioClip[] Musics;

    private bool isActiveRoom = false;
    protected GameManager game;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player") && !isActiveRoom && !Player.isFighting)
        {
            HeadRoom.ActiveNearRooms();
            if (Enemies.Length == 0)
            {
                isActiveRoom = true;
                return;
            }
            game = FindObjectOfType<GameManager>();
            game.UpdateLocalAIManager(this);
            for (int i = 0; i < Enemies.Length; i++)
            {
                Enemies[i].Game = game;
                Enemies[i].Agent.enabled = true;
            }
            isActiveRoom = true;
            HeadRoom.StartEndWave(true);
            StartWave();
            Player.PlayMusic(Musics[Random.Range(0, Musics.Length)], true);
        }
        else if (other.gameObject.CompareTag("player"))
            HeadRoom.ActiveNearRooms();
    }

    protected override void EndWave()
    {
        HeadRoom.StartEndWave(false);
        game.EndWave(HeadRoom);
        base.EndWave();
        if (game.IsAutoSafe)
            game.SaveData();
    }

    protected override void LastEnemyDied(Vector3 pos)
    {
        Ray ray = new Ray(pos + Vector3.up, Vector3.down);
        Vector3 endPos = pos;
        if (Physics.Raycast(ray, out RaycastHit hit, 3, LayerMask.GetMask(new string[] { "Wall", "Default" })))
            endPos = hit.point;
        Instantiate(CasePrefabs[Random.Range(0, CasePrefabs.Length)], endPos, Quaternion.identity, null);
    }
}
