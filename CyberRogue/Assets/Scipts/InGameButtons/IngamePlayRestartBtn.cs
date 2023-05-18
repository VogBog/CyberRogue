using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IngamePlayRestartBtn : IngamePlayBtn
{
    private bool is_Pressed = false;

    public override void OnClick(BasePlayer player)
    {
        if(!is_Pressed)
        {
            is_Pressed = true;
            MyDataStream stream = new MyDataStream(AllSettings.SaveSlot, MyDataStream.MyDataStreamType.Write);
            stream.WriteLine("0");
            stream.Close();
            SceneManager.LoadScene(1);
        }
    }
}
