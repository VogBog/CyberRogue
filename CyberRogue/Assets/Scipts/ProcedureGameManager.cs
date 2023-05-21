using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProcedureGameManager : GameManager
{
    public Slider LoadSlider;
    public GameObject BlackScreen;
    public Transform StartExit;
    public float StartExitScale;
    public LevelRoom[] RoomPrefabs;
    public LevelRoom[] SpecialRoomPrefabsInOrder;
    public LevelRoom BossRoom;
    public int DefaultRoomCount;
    public Transform AllRoomsParent;
    public int MergeChance;
    public float MergeDistance;
    public float MinRoomItemsMultiply, MaxRoomItemsMultiply;
    public float EnemiesCountMultiply;
    public float MinEnemiesCountMultiply;
    public Transform NearPlayerSpotsParent;
    public NearPlayerSpot[] NearPlayerSpots;
    public NavMeshSurface Surface;

    private int LastRoom = -1;

    private bool isStartRoomFree = true, isFirstWave = true;
    private LevelRoom[] allExistedRooms;

    protected override void AfterStart()
    {
        for(int i = 0; i < RoomPrefabs.Length; i++)
        {
            for (int j = 0; j < RoomPrefabs[i].Exits.Length; j++)
                RoomPrefabs[i].Exits[j].IsFree = true;
        }

        StartCoroutine(GenerateAllIE());
    }

    IEnumerator GenerateAllIE()
    {
        List<LevelRoom> rooms = new List<LevelRoom>();
        LoadSlider.DOValue(.1f, 2);

        yield return new WaitForSeconds(2);

        MyDataStream reader = new MyDataStream(AllSettings.SaveSlot, MyDataStream.MyDataStreamType.Open);
        bool isLoadData = int.Parse(reader.ReadLine()) == SceneManager.GetActiveScene().buildIndex;
        isLoadData = reader.ReadLine() != "None" && isLoadData;
        if (!isLoadData)
        {
            reader.Close();

            //Count sum of all special rooms
            int specialRoomStep = Mathf.Clamp(DefaultRoomCount / SpecialRoomPrefabsInOrder.Length, 0, DefaultRoomCount);
            DefaultRoomCount += SpecialRoomPrefabsInOrder.Length;
            List<LevelRoom> specialRoomsList = new List<LevelRoom>(SpecialRoomPrefabsInOrder);
            List<LevelRoom> freeRoomList = new List<LevelRoom>();

            while (true)
            {
                //Generate rooms
                for (int i = 0; i < DefaultRoomCount + 1; i++)
                {
                    float lastExitScale;
                    LevelRoom curRoom = FindFreeRoom(freeRoomList, out lastExitScale);
                    if (curRoom == null && !isStartRoomFree)
                        break;
                    float nextExitScale;
                    float y;
                    int exitIndex;
                    int lastExitIndex;

                    List<LevelRoom> nextRoomsList = new List<LevelRoom>(RoomPrefabs);
                    if (i % specialRoomStep == 0 && i != 0 && specialRoomsList.Count > 0)
                    {
                        nextRoomsList = new List<LevelRoom>
                        {
                            specialRoomsList[0]
                        };
                    }
                    else if (i == DefaultRoomCount)
                        nextRoomsList = new List<LevelRoom>(new LevelRoom[] { BossRoom });

                    LevelRoom nextRoom = FindFreeRoom(nextRoomsList, curRoom, out nextExitScale, out y, out exitIndex, out lastExitIndex);

                    if (y == -1 || nextExitScale == -1 || exitIndex == -1)
                    {
                        yield return null;
                        if (freeRoomList.Count == 0) break;
                        freeRoomList.Remove(curRoom);
                        i--;
                        continue;
                    }

                    if (i % specialRoomStep == 0 && i != 0 && specialRoomsList.Count > 0)
                        specialRoomsList.RemoveAt(0);

                    yield return null;

                    LevelRoom resultRoom = Instantiate(nextRoom, Vector3.zero, new Quaternion(), AllRoomsParent);
                    if (curRoom != null)
                    {
                        resultRoom.ConnectToRoom(curRoom);
                        curRoom.ConnectToRoom(resultRoom);
                    }

                    Transform curExitTransform = resultRoom.Exits[exitIndex].ExitPos;

                    resultRoom.transform.rotation = Quaternion.Euler(0, y, 0);
                    Vector3 lastExitPos = StartExit.position;
                    if(curRoom != null)
                        lastExitPos = curRoom.Exits[lastExitIndex].ExitPos.position;
                    resultRoom.transform.position = lastExitPos + resultRoom.transform.position - curExitTransform.position;
                    rooms.Add(resultRoom);
                    freeRoomList.Add(resultRoom);
                    isStartRoomFree = false;
                    resultRoom.Exits[exitIndex].IsFree = false;
                    if (curRoom != null)
                    {
                        curRoom.Exits[lastExitIndex].IsFree = false;
                        curRoom.AddNewWay(lastExitIndex);
                    }
                    resultRoom.AddNewWay(exitIndex);

                    resultRoom.WaitForActive();
                    yield return null;
                    while (!resultRoom.IsActiveAndReadyForRaycast)
                        yield return null;
                }
                LoadSlider.value = .4f;

                //CheckForSpecialRoomsCount
                if (specialRoomsList.Count > 0)
                {
                    specialRoomsList = new List<LevelRoom>(SpecialRoomPrefabsInOrder);
                    for (int j = 0; j < rooms.Count; j++)
                        Destroy(rooms[j].gameObject);
                    rooms.Clear();
                    LoadSlider.value = .1f;
                }
                else break;
            }
            Debug.Log(rooms.Count);
            //Merge some rooms
            for (int startRoom = 0; startRoom < rooms.Count; startRoom++)
            {
                for (int secondRoom = startRoom + 1; secondRoom < rooms.Count; secondRoom++)
                {
                    for (int startExit = 0; startExit < rooms[startRoom].Exits.Length; startExit++)
                    {
                        for (int secondExit = 0; secondExit < rooms[secondRoom].Exits.Length; secondExit++)
                        {
                            if (!rooms[startRoom].Exits[startExit].IsFree || !rooms[secondRoom].Exits[secondExit].IsFree)
                                continue;
                            Vector3 startPos = rooms[startRoom].Exits[startExit].ExitPos.position;
                            Vector3 secondPos = rooms[secondRoom].Exits[secondExit].ExitPos.position;
                            if (Vector3.Distance(startPos, secondPos) <= MergeDistance && Random.Range(0, 100) <= MergeChance)
                            {
                                rooms[startRoom].Exits[startExit].IsFree = false;
                                rooms[secondRoom].Exits[secondExit].IsFree = false;
                                rooms[startRoom].AddNewWay(startExit);
                                rooms[secondRoom].AddNewWay(secondExit);
                            }
                        }
                    }
                    yield return null;
                }
            }

            //SetIndexes
            for (int i = 0; i < rooms.Count; i++)
                rooms[i].InLevelIndex = i;
        }
        else
        {
            rooms = ReadDataAndSetRooms(reader);
            reader.Close();
        }
        LoadSlider.value = .6f;
        //SetRoomItems

        for (int i = 0; i < rooms.Count; i++)
        {
            float enemiesCountMultiply = EnemiesCountMultiply;
            if(rooms.Count > 1)
                enemiesCountMultiply = (EnemiesCountMultiply - MinEnemiesCountMultiply) * ((float)i / (rooms.Count - 1)) + MinEnemiesCountMultiply;
            rooms[i].SetRoomItems(MinRoomItemsMultiply, MaxRoomItemsMultiply, enemiesCountMultiply, Player, NearPlayerSpotsParent, NearPlayerSpots, this);
        }
        yield return null;
        LoadSlider.value = .7f;
        allExistedRooms = rooms.ToArray();

        StartCoroutine(AfterBaseGenerate(rooms));
    }

    protected virtual IEnumerator AfterBaseGenerate(List<LevelRoom> rooms)
    {
        //Optimize rooms and delete unused items
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].DeleteAllUnused();
            yield return null;
            yield return null;
            yield return null; //Very hard operations, its for more easily work
        }
        LoadSlider.value = .75f;

        yield return null;

        Surface.BuildNavMesh();
        LoadSlider.value = .9f;

        yield return null;

        for (int i = 0; i < rooms.Count; i++)
            rooms[i].DeactivateRoom();

        LoadSlider.value = 1;

        if (LastRoom != -1)
        {
            Player.TeleportPlayerTo(GetFreePosInLastRoom(rooms[LastRoom]));
            rooms[LastRoom].ActiveNearRooms();
        }
        else
            rooms[0].ActiveNearRooms();

        yield return null;
        yield return null;
        yield return null;

        GenerateDone();
    }

    private LevelRoom FindFreeRoom(List<LevelRoom> rooms, out float lastExitScale)
    {
        LevelRoom res = FindFreeRoom(rooms, null, out lastExitScale, out _, out _, out _);
        return res;
    }

    private LevelRoom FindFreeRoom(List<LevelRoom> rooms, LevelRoom lastRoom, out float lastExitScale, out float y, out int exitIndex, out int lastExitIndex)
    {
        y = -1;
        exitIndex = -1;
        lastExitScale = -1;
        lastExitIndex = -1;
        for (int roomAttempt = 0; roomAttempt < rooms.Count; roomAttempt++)
        {
            int randRoom = Random.Range(0, rooms.Count);
            FindFreeOutputExit(rooms[randRoom], lastRoom, out lastExitScale, out y, out exitIndex, out lastExitIndex);
            if (lastExitScale != -1)
                return rooms[randRoom];
        }
        for (int roomIndx = 0; roomIndx < rooms.Count; roomIndx++)
        {
            FindFreeOutputExit(rooms[roomIndx], lastRoom, out lastExitScale, out y, out exitIndex, out lastExitIndex );
            if (lastExitScale != -1)
            {
                return rooms[roomIndx];
            }
        }
        if (isStartRoomFree)
            lastExitScale = StartExitScale;

        return null;
    }

    private void FindFreeOutputExit(LevelRoom curLevel, LevelRoom lastRoom, out float lastExitScale, out float y, out int exitIndex, out int lastExitIndex)
    {
        if(lastRoom == null)
        {
            for(int attempt = 0; attempt < 10; attempt++)
            {
                int randIndx = Random.Range(0, curLevel.Exits.Length);
                if (curLevel.Exits[randIndx].IsFree)
                {
                    lastExitScale = curLevel.Exits[randIndx].ExitSize;
                    y = -curLevel.Exits[randIndx].YAngle;
                    exitIndex = randIndx;
                    lastExitIndex = -1;
                    return;
                }
            }
            for(int i = 0; i < curLevel.Exits.Length; i++)
            {
                if (curLevel.Exits[i].IsFree)
                {
                    lastExitScale = curLevel.Exits[i].ExitSize;
                    y = -curLevel.Exits[i].YAngle;
                    exitIndex = i;
                    lastExitIndex = -1;
                    return;
                }
            }
        }
        else
        {
            for(int attempt = 0; attempt < 10; attempt++)
            {
                int randIndx = Random.Range(0, lastRoom.Exits.Length);
                if (lastRoom.Exits[randIndx].IsFree)
                {
                    Transform thatPivot = lastRoom.Exits[randIndx].ExitPos;
                    float mustBeThatScale = lastRoom.Exits[randIndx].ExitSize;
                    float YAngle = lastRoom.transform.rotation.eulerAngles.y + lastRoom.Exits[randIndx].YAngle + 180;
                    lastExitIndex = randIndx;
                    FindFreeExit(curLevel, out lastExitScale, mustBeThatScale, thatPivot, out y, out exitIndex, YAngle);
                    if (y != -1)
                        return;
                    else lastRoom.Exits[randIndx].IsFree = false;
                }
            }
            for(int i = 0; i < lastRoom.Exits.Length; i++)
            {
                if (lastRoom.Exits[i].IsFree)
                {
                    Transform thatPivot = lastRoom.Exits[i].ExitPos;
                    float mustBeThatScale = lastRoom.Exits[i].ExitSize;
                    float YAngle = lastRoom.transform.rotation.eulerAngles.y + lastRoom.Exits[i].YAngle + 180;
                    lastExitIndex = i;
                    FindFreeExit(curLevel, out lastExitScale, mustBeThatScale, thatPivot, out y, out exitIndex, YAngle);
                    if (y != -1)
                        return;
                    else lastRoom.Exits[i].IsFree = false;
                }
            }
        }

        lastExitScale = -1;
        y = -1;
        exitIndex = -1;
        lastExitIndex = -1;
    }

    private float FindYRot(LevelRoom room, Transform lastExit, int curExit, float YAngle)
    {
        LevelRoom curRoom = Instantiate(room, Vector3.zero, new Quaternion(), AllRoomsParent);
        float y = YAngle - room.Exits[curExit].YAngle;
        curRoom.transform.Rotate(Vector3.up, y);
        curRoom.transform.position = lastExit.position + curRoom.transform.position - curRoom.Exits[curExit].ExitPos.position;
        if (!curRoom.IsCollideWithSmth())
        {
            Destroy(curRoom.gameObject);
            return y;
        }
        Destroy(curRoom.gameObject);
        return -1;
    }

    private void FindFreeExit(LevelRoom room, out float lastExitScale, float mustBeThatScale, Transform thatPivot, out float y, out int exitIndex, float YAngle)
    {
        y = -1;
        for (int exitAttempt = 0; exitAttempt < room.Exits.Length; exitAttempt++)
        {
            int randExit = Random.Range(0, room.Exits.Length);
            if (room.Exits[randExit].IsFree && (mustBeThatScale == 0 ||
                mustBeThatScale == room.Exits[randExit].ExitSize))
            {
                if(mustBeThatScale != 0)
                {
                    y = FindYRot(room, thatPivot, randExit, YAngle);
                    if (y == -1) continue;
                }
                lastExitScale = room.Exits[randExit].ExitSize;
                exitIndex = randExit;
                return;
            }
        }
        for (int exit = 0; exit < room.Exits.Length; exit++)
        {
            if (room.Exits[exit].IsFree && (mustBeThatScale == 0 ||
                mustBeThatScale == room.Exits[exit].ExitSize))
            {
                if (mustBeThatScale != 0)
                {
                    y = FindYRot(room, thatPivot, exit, YAngle);
                    if (y == -1) continue;
                }
                lastExitScale = room.Exits[exit].ExitSize;
                exitIndex = exit;
                return;
            }
        }
        lastExitScale = -1;
        exitIndex = -1;
        y = -1;
    }

    protected virtual void GenerateDone()
    {
        Destroy(BlackScreen);
    }

    public override void ThingIsDestroyed(DestroyableByWeapon thing)
    {
        if (thing is BaseEnemy enemy)
        {
            localAIManager.OneEnemyDied(enemy);
            if(!enemy.IsNotRealEnemy)
                Player.EnemyKilled();
        }
        else if (thing is ExplosionBarrel)
        {
            float damage = CountExplosionDamage(Player.transform.position, thing.transform.position);
            if (damage > 0 && !Player.HasThisAbility(Ability.AbilityType.NoExplosions))
                Player.GetDamage(damage * .6f);
            if (localAIManager != null)
            {
                List<BaseEnemy> list = localAIManager.GetEnemiesList;
                BaseEnemy[] enemies = new BaseEnemy[list.Count];
                list.CopyTo(enemies);
                for (int i = 0; i < enemies.Length; i++)
                {
                    damage = CountExplosionDamage(enemies[i].transform.position, thing.transform.position);
                    if (enemies[i] && damage > 0)
                        enemies[i].Hitted(damage, null);
                }
            }
        }
    }

    private float CountExplosionDamage(Vector3 targetPos, Vector3 barrelPos)
    {
        float dist = Vector3.Distance(targetPos, barrelPos);
        if (dist >= 15) return 0;
        return 150 - 10 * dist;
    }

    public override void UpdateLocalAIManager(AIManager ai)
    {
        localAIManager = ai;
        if (ai == null) return;
        ai.Player = Player;
        ai.NearPlayerSpots = NearPlayerSpots;
        ai.NearPlayerSpotsParent = NearPlayerSpotsParent;
    }

    public override void SaveData()
    {
        MyDataStream writer = new MyDataStream(AllSettings.SaveSlot, MyDataStream.MyDataStreamType.Write);
        string text = SceneManager.GetActiveScene().buildIndex.ToString() + "\n";
        text += "Yes\n";
        text += isFirstWave ? "No\n" : "Yes\n";
        text += $"{LastRoom}\n";
        text += $"{Player.MaxHealth} {Player.curHealth} {Player.DamageMultiply} {Player.AbilityMultiply}";
        writer.WriteLine(text);
        writer.WriteLine(Player.GetWeaponSaveData());
        writer.WriteLine(allExistedRooms.Length);
        for(int i = 0; i < allExistedRooms.Length; i++)
        {
            writer.WriteLine(allExistedRooms[i].RoomIndex);
            writer.WriteLine($"{allExistedRooms[i].transform.position.x}");
            writer.WriteLine($"{allExistedRooms[i].transform.position.y}");
            writer.WriteLine($"{allExistedRooms[i].transform.position.z}");
            writer.WriteLine($"{allExistedRooms[i].transform.rotation.eulerAngles.y}");
            writer.WriteLine(allExistedRooms[i].GetSaveData());
        }
        writer.Close();
        Player.SetNotice("¬ы сохранились.");
    }

    private Vector3 GetFreePosInLastRoom(LevelRoom room)
    {
        for(int i = 0; i < room.AI.EnemiesSpawnPoints.Length; i++)
        {
            if (room.AI.EnemiesSpawnPoints[i].IsFree)
                return room.AI.EnemiesSpawnPoints[i].transform.position;
        }
        return new Vector3(8.5f, 3, -2.5f);
    }

    private List<LevelRoom> ReadDataAndSetRooms(MyDataStream reader)
    {
        isFirstWave = reader.ReadLine() == "No";
        if (!int.TryParse(reader.ReadLine(), out LastRoom))
            LastRoom = -1;
        List<LevelRoom> result = new List<LevelRoom>();
        Player.ReadAndApplyData(reader);
        int count = 0;
        int.TryParse(reader.ReadLine(), out count);
        for(int i = 0; i < count; i++)
        {
            int.TryParse(reader.ReadLine(), out int type);
            LevelRoom room = Instantiate(StaticSaveData.GetLevelRoomByIndex(type), AllRoomsParent);
            Vector3 pos = Vector3.zero;
            float.TryParse(reader.ReadLine(), out pos.x);
            float.TryParse(reader.ReadLine(), out pos.y);
            float.TryParse(reader.ReadLine(), out pos.z);
            float.TryParse(reader.ReadLine(), out float yRot);
            room.transform.position = pos;
            room.transform.Rotate(Vector3.up, yRot);
            room.ApplySaveData(reader);
            room.InLevelIndex = i;
            result.Add(room);
        }
        for (int i = 0; i < result.Count; i++)
            result[i].ApplyConnectedRooms(result);
        return result;
    }

    public void SetLastRoom(LevelRoom room)
    {
        for(int i = 0; i < allExistedRooms.Length; i++)
        {
            if (allExistedRooms[i] == room)
            {
                LastRoom = i;
                break;
            }
        }
    }

    public override void EndWave(LevelRoom whatRoom)
    {
        base.EndWave(whatRoom);
        SetLastRoom(whatRoom);
        if(isFirstWave)
        {
            isFirstWave = false;
            Player.SetNotice(" ейсы, выпавшие после окончани€ бо€, открываютс€ так же, как двери");
            Player.SetNotice("“акже можно открывать мусорные баки, из которых может выпасть лут.");
            Player.SetNotice("— рычагами можно взаимодействовать так же, как и с дверьми.");
        }
    }
}
