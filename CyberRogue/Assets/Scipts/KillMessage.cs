using System.Collections;
using UnityEngine;

public class KillMessage : MonoBehaviour
{
    public void Activate()
    {
        StartCoroutine(ActivateIE());
    }

    IEnumerator ActivateIE()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
