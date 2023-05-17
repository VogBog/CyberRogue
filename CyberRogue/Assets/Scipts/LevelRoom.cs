using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRoom : MonoBehaviour
{
    [System.Serializable]
    public class RoomExit
    {
        public float ExitSize;
        public Transform ExitPos;
        public GameObject BlockZone;
        public float YAngle;
        public bool IsFree { get; set; } = true;
    }

    [System.Serializable]
    public class RoomItemsPoses
    {
        public GameObject[] Items;
    }

    public int RoomIndex;
    public RoomExit[] Exits;
    public Transform[] Rays;
    public GameObject[] WaysForExits;
    public GameObject[] WallsForExits;
    public LayerMask RayMask;
    public Collider Coll;
    public Transform CentralRay;
    public RoomItemsPoses[] RoomItems;
    public AIManager AI;
    public GameObject[] MapIcons;

    protected List<LevelRoom> connectedRooms = new List<LevelRoom>();

    public bool isDone { get; set; } = false;

    public bool IsActiveAndReadyForRaycast { get; private set; } = false;

    public int InLevelIndex { get; set; } = 0;

    private int[] connectedIndexes;

    public bool IsCollideWithSmth()
    {
        Coll.enabled = false;
        foreach(Transform pos in Rays)
        {
            Ray ray = new Ray(pos.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10, RayMask))
            {
                Debug.DrawLine(pos.position, hit.point, Color.red);
                return true;
            }
            else Debug.DrawLine(pos.position, pos.position + Vector3.down * 10, Color.blue);
        }
        return false;
    }

    public void AddNewWay(int index)
    {
        WaysForExits[index].SetActive(true);
        if(WallsForExits.Length != 0)
            WallsForExits[index].SetActive(false);
    }

    public void ConnectToRoom(LevelRoom connectedRoom)
    {
        connectedRooms.Add(connectedRoom);
    }

    public void ActiveNearRooms(LevelRoom lastRoom = null)
    {
        for(int i = 0; i < connectedRooms.Count; i++)
        {
            if (lastRoom != null && connectedRooms[i] == this)
                continue;
            if (lastRoom == null)
                connectedRooms[i].ActiveNearRooms(this);
            else connectedRooms[i].DeactivateRoom();
        }
        for(int i = 0; i < RoomItems.Length; i++)
        {
            for(int j = 0; j < RoomItems[i].Items.Length; j++)
            {
                if (RoomItems[i].Items[j] != null)
                    RoomItems[i].Items[j].SetActive(true);
            }
        }
    }

    public void DeactivateRoom()
    {
        for (int i = 0; i < RoomItems.Length; i++)
        {
            for (int j = 0; j < RoomItems[i].Items.Length; j++)
            {
                if (RoomItems[i].Items[j] != null)
                    RoomItems[i].Items[j].SetActive(false);
            }
        }
    }

    public void WaitForActive()
    {
        IsActiveAndReadyForRaycast = false;
        StartCoroutine(WaitForActiveIE());
    }

    IEnumerator WaitForActiveIE()
    {
        Ray ray = new Ray(CentralRay.position, Vector3.down);
        while (!IsActiveAndReadyForRaycast)
        {
            if (Physics.Raycast(ray, out _, 10, RayMask))
                IsActiveAndReadyForRaycast = true;

            yield return null;
        }
    }

    public void SetRoomItems(float minMultiply, float maxMultiply, float enemiesMultiply, BasePlayer player, Transform spotsParent, NearPlayerSpot[] spots, GameManager game)
    {
        int min = Mathf.FloorToInt(RoomItems.Length * minMultiply);
        int max = Mathf.RoundToInt(RoomItems.Length * maxMultiply);
        min = Mathf.Clamp(min, 0, RoomItems.Length);
        max = Mathf.Clamp(max, 0, RoomItems.Length);
        int count = Random.Range(min, max);
        List<RoomItemsPoses> poses = new List<RoomItemsPoses>(RoomItems);
        for(int i = 0; i < count; i++)
        {
            int rand = Random.Range(0, poses.Count);
            int randItem = Random.Range(0, poses[rand].Items.Length);
            poses[rand].Items[randItem].gameObject.SetActive(true);
            poses.Remove(poses[rand]);
        }
        if(AI != null && !isDone)
        {
            int enemiesCount = Mathf.RoundToInt(AI.EnemiesMaxCount * enemiesMultiply);

            if (AI.EnemiesCosts.Length == 1 && AI.EnemiesMaxCount == 0)
                enemiesCount = AI.EnemiesCosts[0] + 1;

            List<NearPlayerSpot> enemiesPoints = new List<NearPlayerSpot>(AI.EnemiesSpawnPoints);
            List<BaseEnemy> enemies = new List<BaseEnemy>();

            for(int attempt = 0; attempt < 50 && enemiesCount > Mathf.Min(AI.EnemiesCosts); attempt++)
            {
                int pointIndx = Random.Range(0, enemiesPoints.Count);
                int indx = Random.Range(0, AI.Enemies.Length);
                if (enemiesPoints.Count == 0) break;
                if (!enemiesPoints[pointIndx].IsFree)
                {
                    enemiesPoints.RemoveAt(pointIndx);
                    continue;
                }
                if (AI.EnemiesCosts[indx] > enemiesCount)
                {
                    attempt++;
                    continue;
                }

                BaseEnemy enemy = Instantiate(AI.Enemies[indx], enemiesPoints[pointIndx].transform.position, Quaternion.identity, transform);
                enemy.Game = game;
                enemy.gameObject.SetActive(false);
                enemiesPoints.RemoveAt(pointIndx);
                enemies.Add(enemy);
                enemiesCount -= AI.EnemiesCosts[indx];
            }
            AI.SetEnemies(enemies);
            AI.Player = player;
            AI.NearPlayerSpotsParent = spotsParent;
            AI.NearPlayerSpots = spots;
        }
        else if(AI != null && isDone)
        {
            AI.SetEnemies(new List<BaseEnemy>());
            AI.Player = player;
            AI.NearPlayerSpotsParent = spotsParent;
            AI.NearPlayerSpots = spots;
        }
    }

    public void StartEndWave(bool isStart)
    {
        for (int i = 0; i < MapIcons.Length; i++)
        {
            if (MapIcons[i] != null)
                MapIcons[i].SetActive(true);
        }
        for(int i = 0; i < Exits.Length; i++)
        {
            if (!Exits[i].IsFree)
                Exits[i].BlockZone.SetActive(isStart);
        }
        isDone = !isStart;
    }

    public virtual void DeleteAllUnused()
    {
        for(int i = 0; i < WaysForExits.Length; i++)
        {
            if (!WaysForExits[i].activeSelf)
                Destroy(WaysForExits[i]);
        }
        for(int i = 0; i < WallsForExits.Length; i++)
        {
            if (!WallsForExits[i].activeSelf)
                Destroy(WallsForExits[i]);
        }
        for (int i = 0; i < Rays.Length; i++)
            Destroy(Rays[i].gameObject);
        Rays = null;
        for(int i = 0; i < RoomItems.Length; i++)
        {
            for(int j = 0; j < RoomItems[i].Items.Length; j++)
            {
                if (!RoomItems[i].Items[j].activeSelf)
                    Destroy(RoomItems[i].Items[j]);
            }
        }
    }

    public string GetSaveData()
    {
        string res = "";
        for(int i = 0; i < Exits.Length; i++)
        {
            res += Exits[i].IsFree ? "1 " : "0 ";
            res += WaysForExits[i] && WaysForExits[i].activeSelf ? "1 " : "0 ";
        }
        res += isDone ? "1\n" : "0\n";
        res += connectedRooms.Count + "\n";
        for(int i = 0; i < connectedRooms.Count; i++)
        {
            res += connectedRooms[i].InLevelIndex;
            if (i < connectedRooms.Count - 1)
                res += " ";
        }
        return res;
    }

    public void ApplySaveData(MyDataStream reader)
    {
        string[] line = reader.ReadLine().Split();
        foreach (string line1 in line) Debug.Log(line1);
        for(int i = 0; i < Exits.Length; i++)
        {
            int exit, way;
            int.TryParse(line[2 * i], out exit);
            int.TryParse(line[2 * i + 1], out way);
            Exits[i].IsFree = exit == 1;
            WaysForExits[i].SetActive(way == 1);
            if (way == 1)
                AddNewWay(i);
        }
        int.TryParse(line[Exits.Length * 2], out int done);
        isDone = done == 1;
        if(isDone)
        {
            for(int i = 0; i < MapIcons.Length; i++)
            {
                if (MapIcons[i] != null)
                    MapIcons[i].SetActive(true);
            }    
        }

        int count;
        if (!int.TryParse(reader.ReadLine(), out count))
            count = 0;
        line = reader.ReadLine().Split();
        connectedIndexes = new int[count];
        for (int i = 0; i < count; i++)
            int.TryParse(line[i], out connectedIndexes[i]);
    }

    public void ApplyConnectedRooms(List<LevelRoom> rooms)
    {
        for(int i = 0; i < connectedIndexes.Length; i++)
        {
            for(int j = 0; j < rooms.Count; j++)
            {
                if (rooms[j].InLevelIndex == connectedIndexes[i])
                    connectedRooms.Add(rooms[j]);
            }
        }
    }
}
