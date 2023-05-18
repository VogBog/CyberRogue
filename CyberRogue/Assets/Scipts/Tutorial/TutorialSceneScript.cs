using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSceneScript : GameManager
{
    public Material mat1, mat2;
    public GameObject wall1, wall2;
    public GameObject[] particles1, particles2;
    public GameObject[] AfterOneIterationObjects, AfterSecondIterationObjects;
    public GameObject[] LoadSimulationShader, DefaultShader;
    public GameObject GlockGO;
    public GameObject[] FightWalls;
    public GameObject[] LastIterationObjects;
    public DestroyableByWeapon[] Targets;
    public GameObject GlockAmmoPrefab, SkyBoxGO;
    public AudioSource StartSource;
    public AIManager AI;

    private bool isWaitingForButton = false;
    private bool isTutorialFight = false;
    private bool isAIFight = false;
    private bool isDoorNoticeShowed = false;

    protected override void AfterStart()
    {
        Camera.main.farClipPlane = 500;
        for(int i = 0; i < AfterOneIterationObjects.Length; i++)
            AfterOneIterationObjects[i].SetActive(false);
        for(int i = 0; i < AfterSecondIterationObjects.Length; i++)
            AfterSecondIterationObjects[i].SetActive(false);

        StartCoroutine(AfterStartIE());
    }

    IEnumerator AfterStartIE()
    {
        StartSource.Play();
        mat1.SetFloat("_MainValue", 0);
        mat2.SetFloat("_MainValue", 0);
        mat1.DOFloat(23, "_MainValue", 5);
        for (int i = 0; i < particles2.Length; i++)
            particles2[i].SetActive(false);

        yield return null;
        Player.SetNotice("Перед подключением к боту, проверьте элементы управления!");
        Player.SetNotice("Загрузка сценария...");

        yield return new WaitForSeconds(10);

        mat1.DOFloat(100, "_MainValue", 5);
        mat2.DOFloat(6.5f, "_MainValue", 10);

        yield return new WaitForSeconds(5);

        wall1.SetActive(false);

        for (int i = 0; i < particles1.Length; i++)
            particles1[i].transform.DOMove(particles1[i].transform.position + new Vector3(0, 0, 22), 8);
        for(int i = 0; i < AfterOneIterationObjects.Length; i++)
        {
            Vector3 startPos = AfterOneIterationObjects[i].transform.position;
            AfterOneIterationObjects[i].transform.position += Vector3.down * 100;
            AfterOneIterationObjects[i].transform.DOMove(startPos, 5);
            AfterOneIterationObjects[i].SetActive(true);
        }

        yield return new WaitForSeconds(5);

        Player.SetNotice("Проверьте работоспособность меню!");
        Player.SetNotice($"Чтобы открыть меню, нажмите {AllKeys.Settings}!");
        isWaitingForButton = true;

        while (isWaitingForButton)
            yield return new WaitForSeconds(1);

        for (int i = 0; i < particles1.Length; i++)
            particles1[i].SetActive(false);
        for (int i = 0; i < particles2.Length; i++)
            particles2[i].SetActive(true);
        for (int i = 0; i < AfterSecondIterationObjects.Length; i++)
        {
            Vector3 startPos = AfterSecondIterationObjects[i].transform.position;
            AfterSecondIterationObjects[i].transform.position += Vector3.down * 100;
            AfterSecondIterationObjects[i].transform.DOMove(startPos, 5);
            AfterSecondIterationObjects[i].SetActive(true);
        }
        StartSource.Play();

        mat2.DOFloat(100, "_MainValue", 20);

        wall2.SetActive(false);

        yield return new WaitForSeconds(5);

        GlockGO.SetActive(true);
        Camera.main.farClipPlane = 100;
        SkyBoxGO.SetActive(false);

        yield return new WaitForSeconds(5);

        for (int i = 0; i < LoadSimulationShader.Length; i++)
            LoadSimulationShader[i].SetActive(false);
        for (int i = 0; i < DefaultShader.Length; i++)
            DefaultShader[i].SetActive(true);
        mat1.SetFloat("_MainValue", 0);
    }

    protected override void AfterUpdate()
    {
        if(isWaitingForButton && Player.isSettingsOpened)
            isWaitingForButton = false;
    }

    protected override void GameManagerTriggerEnterVirtual(string name)
    {
        if(name == "FightTutorial" && !isTutorialFight && Player.HasAnyWeapon)
        {
            StartSource.Play();
            isTutorialFight = true;
            FightWalls[0].SetActive(true);
            FightWalls[1].SetActive(true);
            FightWalls[2].SetActive(true);

            for (int i = 3; i < FightWalls.Length; i++)
            {
                if (Random.Range(0, 100) > 60)
                    FightWalls[i].SetActive(true);
            }

            mat1.DOFloat(40, "_MainValue", 3);
            for (int i = 0; i < particles2.Length; i++)
                particles2[i].SetActive(false);
            for(int i = 0; i < LastIterationObjects.Length; i++)
            {
                Vector3 startPos = LastIterationObjects[i].transform.position;
                LastIterationObjects[i].transform.position += Vector3.down * 100;
                LastIterationObjects[i].SetActive(true);
                LastIterationObjects[i].transform.DOMove(startPos, 3);
            }
            Player.SetNotice($"Прицеливание: {AllKeys.Target}, Огонь: {AllKeys.Fire}");

            for (int i = 0; i < Targets.Length; i++)
            {
                Vector3 targetPos = Targets[i].transform.position;
                Targets[i].transform.position += Vector3.down * 2;
                Targets[i].gameObject.SetActive(true);
                Targets[i].transform.DOMove(targetPos, .5f);
            }
        }
        else if(name == "Finish" && isAIFight)
        {
            isAIFight = false;
            StartCoroutine(FinishIE());
        }
        else if(name == "Door" && !isDoorNoticeShowed)
        {
            isDoorNoticeShowed = true;
            Player.SetNotice("Попробуйте открыть эту дверь");
            Player.SetNotice($"Чтобы убрать оружие, нажмите {AllKeys.ChooseFreeHand}");
            Player.SetNotice($"Наведитесь на ручку двери, зажмите {AllKeys.Fire} и проведите вверх");
        }
    }

    IEnumerator FinishIE()
    {
        Player.SetNotice("Подключение к роботу...");
        mat1.SetFloat("_MainValue", 100);
        mat2.SetFloat("_MainValue", 100);
        for (int i = 0; i < LoadSimulationShader.Length; i++)
            LoadSimulationShader[i].SetActive(true);
        for (int i = 0; i < DefaultShader.Length; i++)
            DefaultShader[i].SetActive(false);
        mat1.DOFloat(0, "_MainValue", 5);
        mat2.DOFloat(0, "_MainValue", 5);
        yield return new WaitForSeconds(5);
        MyDataStream.OpenWriteAndClose(AllSettings.SaveSlot, "2\nNone");
        SceneManager.LoadScene(2);
    }

    public override void ThingIsDestroyed(DestroyableByWeapon thing)
    {
        if (thing is BaseEnemy enemy)
            AI.OneEnemyDied(enemy);

        Instantiate(GlockAmmoPrefab, thing.transform.position, Quaternion.identity, null);
        bool isOK = true;
        for(int i = 0; i < Targets.Length; i++)
        {
            if (Targets[i] != null && !Targets[i].IsDead)
            {
                isOK = false;
                break;
            }
        }
        if(isOK && !isAIFight)
        {
            isAIFight = true;
            for (int i = 0; i < AI.Enemies.Length; i++)
                AI.Enemies[i].gameObject.SetActive(true);
            AI.SetEnemies(new List<BaseEnemy>(AI.Enemies));
            AI.StartWave();
        }
        else if(isOK && isAIFight)
        {
            FightWalls[2].transform.DOMoveY(-10, 5);
        }
    }

    public override void PlayerDead()
    {
        Player.Heal(100);
        Player.SetNotice("В реальном бою Вы бы были мертвы.");
    }
}
