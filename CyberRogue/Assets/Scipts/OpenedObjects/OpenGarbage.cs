using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenGarbage : HandMoveObject
{
    public int Chance;
    public Transform[] SpawnTransformTrajectory;
    public GameObject[] SpawnObjects;

    private bool isOnceOpened = false;

    protected override void FullOpened()
    {
        if(!isOnceOpened)
        {
            isOnceOpened = true;
            if(Random.Range(0, 100) <= Chance)
            {
                Transform transf = Instantiate(SpawnObjects[Random.Range(0, SpawnObjects.Length)], SpawnTransformTrajectory[0].position, Quaternion.identity).transform;
                StartCoroutine(AnimIE(transf));
            }
        }
    }

    IEnumerator AnimIE(Transform transf)
    {
        foreach(Transform pos in SpawnTransformTrajectory)
        {
            transf.DOMove(pos.position, .2f);
            yield return new WaitForSeconds(.1f);
        }
    }
}
