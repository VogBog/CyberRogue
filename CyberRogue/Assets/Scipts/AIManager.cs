using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public BasePlayer Player;
    public NearPlayerSpot[] NearPlayerSpots;
    public NearPlayerSpot[] SniperSpots;
    public BaseEnemy[] Enemies;
    public int[] EnemiesCosts;
    public NearPlayerSpot[] EnemiesSpawnPoints;
    public int EnemiesMaxCount;
    public Transform NearPlayerSpotsParent;

    private List<BaseEnemy> AllAttackers = new List<BaseEnemy>();
    private List<BaseEnemy> AllEnemies = new List<BaseEnemy>();
    private bool isEnemyReady = false;

    public List<BaseEnemy> GetEnemiesList { get { return AllEnemies; } }

    public void SetEnemies(List<BaseEnemy> enemies)
    {
        Enemies = enemies.ToArray();
        isEnemyReady = true;
    }

    public void StartWave()
    {
        if (Enemies.Length == 0 || !isEnemyReady)
            return;
        Player.StartWave();
        List<Transform> spots = new List<Transform>();
        for(int i = 0; i < SniperSpots.Length; i++)
        {
            if (SniperSpots[i].IsFree)
                spots.Add(SniperSpots[i].transform);
        }

        for (int i = 0; i < Enemies.Length; i++)
            Enemies[i].gameObject.SetActive(true);

        AllAttackers = new List<BaseEnemy>();
        AllEnemies = new List<BaseEnemy>(Enemies);
        for(int i = 0; i < Enemies.Length; i++)
        {
            if (Enemies[i].Type == BaseEnemy.EnemyType.Attacker)
                AllAttackers.Add(Enemies[i]);
        }
        for(int i = 0; i < Enemies.Length && AllAttackers.Count < Enemies.Length / 3; i++)
        {
            if (Enemies[i].Type == BaseEnemy.EnemyType.Universal)
                AllAttackers.Add(Enemies[i]);
        }
        for(int i = 0; i < Enemies.Length; i++)
        {
            if (AllAttackers.Contains(Enemies[i]))
                StartCoroutine(EnemyStartWaveIE(Enemies[i], spots.ToArray(), NearPlayerSpots, true, Player, i));
            StartCoroutine(EnemyStartWaveIE(Enemies[i], spots.ToArray(), NearPlayerSpots, false, Player, i));
        }
    }

    IEnumerator EnemyStartWaveIE(BaseEnemy enemy, Transform[] spots, NearPlayerSpot[] nearPlayer, bool isAttacker, BasePlayer player, int index)
    {
        for(int i = 0; i < (index - 1) * 3; i++)
            yield return null;
        enemy.StartWave(spots, nearPlayer, isAttacker, player);
    }

    public void AddNewEnemy(BaseEnemy newEnemy, Vector3 position)
    {
        BaseEnemy enemy = Instantiate(newEnemy, position, Quaternion.identity, null);
        enemy.Agent.enabled = true;

        List<Transform> spots = new List<Transform>();
        for (int i = 0; i < SniperSpots.Length; i++)
        {
            if (SniperSpots[i].IsFree)
                spots.Add(SniperSpots[i].transform);
        }

        AllEnemies.Add(enemy);
        if (enemy.Type == BaseEnemy.EnemyType.Attacker)
            AllAttackers.Add(enemy);
        enemy.StartWave(spots.ToArray(), NearPlayerSpots, enemy.Type == BaseEnemy.EnemyType.Attacker, Player);
    }

    public void OneEnemyDied(BaseEnemy enemy)
    {
        Vector3 pos = enemy.transform.position;
        if(AllAttackers.Contains(enemy))
        {
            AllAttackers.Remove(enemy);
            for(int i = 0; i < AllEnemies.Count; i++)
            {
                if (AllEnemies[i].Type == BaseEnemy.EnemyType.Universal)
                {
                    AllAttackers.Add(AllEnemies[i]);
                    AllEnemies[i].UpdateWaveLikeAttacker();
                    break;
                }
            }
        }
        AllEnemies.Remove(enemy);
        if (AllEnemies.Count == 0)
        {
            EndWave();
            LastEnemyDied(pos);
        }
    }

    protected virtual void EndWave()
    {
        Player.EndWave();
        FindObjectOfType<GameManager>().UpdateLocalAIManager(null);
        Debug.Log("THE END!");
    }

    private void Update()
    {
        if (Player)
            NearPlayerSpotsParent.position = Player.transform.position;
        if (Input.GetKeyDown(KeyCode.P))
            ExtraEndWave();
    }

    private void ExtraEndWave()
    {
        if(Player.isFighting)
        {
            for(int i = 0; i < AllEnemies.Count; i++)
            {
                if (AllEnemies[i] != null)
                {
                    AllEnemies[i].Hitted(1000, null);
                    break;
                }
            }
        }
    }

    protected virtual void LastEnemyDied(Vector3 pos) { }
}
