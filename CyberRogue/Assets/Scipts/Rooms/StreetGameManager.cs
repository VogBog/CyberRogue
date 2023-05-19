using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StreetGameManager : ProcedureGameManager
{
    public StreetSkyBoxHome[] SkyBoxes;
    public Transform SkyBoxHomesParent;
    public AudioSource AmbientAudioSource;

    protected override IEnumerator AfterBaseGenerate(List<LevelRoom> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i] is StreetLevelRoom street)
                street.GenerateSideWalls();
            if (i % 10 == 0)
                yield return null;
        }

        for(int i = 0; i < rooms.Count; i++)
        {
            for(int j = 0; j < 30; j++)
            {
                float randX = Random.Range(-300, 300);
                float randZ = Random.Range(-300, 300);
                Vector3 newPos = new Vector3(randX, 0, randZ) + rooms[i].transform.position;
                int randIndx = Random.Range(0, SkyBoxes.Length);
                if (SkyBoxes[randIndx].isFreeAtThatPos(newPos))
                {
                    StreetSkyBoxHome home = Instantiate(SkyBoxes[randIndx], newPos, new Quaternion(), SkyBoxHomesParent);
                    home.transform.Rotate(Vector3.up, Random.Range(0, 360));
                    if (!home.isFreeAtThatPos(home.transform.position))
                        home.transform.rotation = new Quaternion();
                    home.transform.position += Vector3.up * home.CheckBoxSize.y / 2;
                }
            }
            if (i % 10 == 0)
                yield return null;
        }

        yield return null;
        StartCoroutine(base.AfterBaseGenerate(rooms));
    }

    public override void BossIsDead()
    {
        StartCoroutine(BossIsDeadIE());
    }

    IEnumerator BossIsDeadIE()
    {
        yield return new WaitForSeconds(10);
        SceneManager.LoadScene(3);
    }

    protected override void GenerateDone()
    {
        base.GenerateDone();
        AmbientAudioSource.Play();
        StartCoroutine(GenerateDoneIE());
    }

    IEnumerator GenerateDoneIE()
    {
        yield return new WaitForSeconds(3);
        Player.SetNotice("Ваша цель: Путуридзе Зураб - работорговец");
        Player.SetNotice("Мы выслали Вас в приблизительное местоположение вашей цели");
    }
}
