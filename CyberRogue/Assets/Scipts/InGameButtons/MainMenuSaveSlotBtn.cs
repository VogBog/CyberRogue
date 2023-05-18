using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class MainMenuSaveSlotBtn : InGameButton
{
    public int Slot;
    public GameObject MySlotIsChoosen, IAmChoosenByCursor;
    public TextMeshPro Text;
    public MainMenuHall Hall;

    private MainMenuSaveSlotBtn[] AllSlots;

    public override void OnChoosen()
    {
        IAmChoosenByCursor.SetActive(true);
    }

    public override void OnClick(BasePlayer player)
    {
        AllSettings.SaveSlot = Slot;
        MySlotIsChoosen.SetActive(true);
        for(int i = 0; i < AllSlots.Length; i++)
        {
            if (AllSlots[i].Slot != Slot)
                AllSlots[i].DeactivateMe();
        }
    }

    public void DeactivateMe()
    {
        MySlotIsChoosen.SetActive(false);
    }

    public override void OnDraging(Vector3 dragPos) { }

    public override void OnUnchoosen()
    {
        IAmChoosenByCursor.SetActive(false);
    }

    public override void StopDraging() { }

    private void Start()
    {
        StartCoroutine(TryStart());
    }

    IEnumerator TryStart()
    {
        while (!Hall.isEnd)
            yield return null;
        yield return new WaitForSeconds(1);
        AllSlots = FindObjectsOfType<MainMenuSaveSlotBtn>();
        FileStream file = File.OpenRead(StaticSaveData.GetPath(Slot));
        BinaryFormatter bf = new BinaryFormatter();
        string line = (string)bf.Deserialize(file);
        file.Close();
        Text.text = $"Lvl: {line[0]}\n";

        if (AllSettings.SaveSlot == Slot)
            MySlotIsChoosen.SetActive(true);
    }
}
