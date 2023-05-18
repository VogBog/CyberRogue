using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHall : MonoBehaviour
{
    [HideInInspector] public bool isEnd = false;
    public BasePlayer Player;

    private string currentVersion = "1.0";

    private void Start()
    {
        StartCoroutine(TryFile());
    }

    private bool isFileExists()
    {
        string path = Application.persistentDataPath + "/SaveSlots/SaveSlot";
        for(int i = 1; i < 5; i++)
        {
            if (!File.Exists(path + i + ".dat"))
                return false;
        }
        return true;
    }

    IEnumerator TryFile()
    {
        int attempt = 0;
        yield return null;

        if (!isFileExists())
        {
            bool isOld = false;
            if (!Directory.Exists(Application.persistentDataPath + "/SaveSlots"))
                Directory.CreateDirectory(Application.persistentDataPath + "/SaveSlots");
            string dataPath = Application.persistentDataPath + "/SaveSlots/Version.dat";
            if (!File.Exists(dataPath))
            {
                FileStream file = File.Create(dataPath);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, currentVersion);
                file.Close();
            }
            else
            {
                FileStream file = File.OpenRead(dataPath);
                BinaryFormatter bf = new BinaryFormatter();
                string line = (string)bf.Deserialize(file);
                file.Close();
                if(line != currentVersion)
                {
                    Player.SetNotice("Вы используете старые файлы сохранения. Они будут удалены.");
                    file = File.OpenWrite(dataPath);
                    bf.Serialize(file, currentVersion);
                    file.Close();
                    isOld = true;
                }
            }

            for (int i = 1; i < 5; i++)
            {
                yield return null;
                string path = StaticSaveData.GetPath(i);
                try
                {
                    if (!File.Exists(path))
                    {
                        FileStream file = File.Create(path);
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(file, "0");
                        file.Close();
                    }
                    else if(isOld)
                    {
                        FileStream file = File.OpenWrite(path);
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(file, "0");
                        file.Close();
                    }
                }
                catch
                {
                    i--;
                    attempt++;
                }
                if (attempt > 1000) break;
            }

            if(attempt > 1000)
            {
                Player.SetNotice("Что-то не даёт открыть слот сохранения, попробуйте перезагрузить компьютер.");
            }
        }

        if (!File.Exists(StaticSaveData.SettingsDataPath))
        {
            FileStream file = File.Create(StaticSaveData.SettingsDataPath);
            BinaryFormatter bf = new BinaryFormatter();
            string res = $"{AllSettings.SaveSlot}\n{AllSettings.Sensivity}\n{AllSettings.SoundEffects}\n{AllSettings.Music}\n{AllSettings.Ambient}" +
                "\n" + QualitySettings.GetQualityLevel();
            bf.Serialize(file, res);
            file.Close();
        }
        else
        {
            StaticSaveData.LoadSettingsData();
        }

        isEnd = true;
    }
}
