using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Titles : MonoBehaviour
{
    public Transform TitlesTransform;
    public Transform EndPos;
    public float AllTime;

    private float speed;

    IEnumerator Start()
    {
        speed = (EndPos.position - TitlesTransform.position).y / AllTime;
        yield return new WaitForSeconds(AllTime);
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        TitlesTransform.position += Vector3.up * speed * Time.deltaTime;
    }
}
