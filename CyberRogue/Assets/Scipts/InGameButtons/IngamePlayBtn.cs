using UnityEngine;
using UnityEngine.SceneManagement;

public class IngamePlayBtn : InGameButton
{
    public GameObject SelectGO;

    private bool isPressed = false;

    public override void OnDraging(Vector3 dragPos) { }

    public override void OnClick(BasePlayer player)
    {
        if (!isPressed)
        {
            isPressed = true;

            MyDataStream stream = new MyDataStream(AllSettings.SaveSlot, MyDataStream.MyDataStreamType.Open);
            string line = stream.ReadLine();
            stream.Close();

            if (int.TryParse(line, out int result))
            {
                if (result != 0)
                    SceneManager.LoadScene(result);
                else SceneManager.LoadScene(1);
            }
            else SceneManager.LoadScene(1);
        }
    }

    public override void OnChoosen()
    {
        SelectGO.SetActive(true);
    }

    public override void OnUnchoosen()
    {
        SelectGO.SetActive(false);
    }

    public override void StopDraging() { }
}
