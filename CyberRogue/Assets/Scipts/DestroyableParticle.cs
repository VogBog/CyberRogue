using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DestroyableParticle : MonoBehaviour
{
    public void StartTimer(float highPos, float lowPos)
    {
        StartCoroutine(StartTimerIE(highPos, lowPos));
    }

    private IEnumerator StartTimerIE(float highPos, float lowPos)
    {
        transform.DOMoveY(highPos, .25f);
        yield return new WaitForSeconds(.25f);
        transform.DOMoveY(lowPos, 1);
        yield return new WaitForSeconds(.2f);
        transform.DOComplete();
        yield return null;
        Destroy(gameObject);
    }
}
