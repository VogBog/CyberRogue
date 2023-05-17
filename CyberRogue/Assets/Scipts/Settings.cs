using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Settings : MonoBehaviour
{
    public LayerMask uiMask;
    public AudioMixer Audio;

    private Camera mainCamera;
    private Vector2 invalidPosition;
    private UIDocument ui;
    private VisualElement element;
    private BasePlayer player;

    //States
    #region States
    private int state = 0;
    private Button[] stateButtons = new Button[4];
    private GroupBox[] stateBoxes = new GroupBox[4];
    private GroupBox BigBoss;
    #endregion

    //Main
    #region Main
    private Slider SensivitySlider, AudioEffectsSlider, AmbientSlider, MusicSlider;
    private DropdownField GraphicField;
    private Button MainApplyBtn;
    private Label SensivityLabel, AudioEffectsLabel, AmbientLabel, MusicLabel;
    #endregion

    //Keys
    #region Keys
    private bool isWaitingForPressingKey = false;
    private int keyState = 0, waitingKeyGroup = 0, waitingKeyKey = 0;
    private Button[] ChangeKeysScreenBtns = new Button[2];
    private Label KeyScreenText;
    private Button[][] KeysButtons = new Button[3][];
    private GroupBox[] KeysGroups = new GroupBox[3];
    #endregion

    private Dictionary<string, int> qualityLevels = new Dictionary<string, int>()
    { {"Низкая", 0 }, {"Средняя", 1}, {"Высокая", 2} };

    private void OnMainApply()
    {
        AllSettings.Sensivity = SensivitySlider.value;
        AllSettings.SoundEffects = AudioEffectsSlider.value;
        AllSettings.Ambient = AmbientSlider.value;
        AllSettings.Music = MusicSlider.value;

        Audio.SetFloat("Effects", AllSettings.SoundEffects);
        Audio.SetFloat("Ambient", AllSettings.Ambient);
        Audio.SetFloat("Music", AllSettings.Music);

        QualitySettings.SetQualityLevel(qualityLevels[GraphicField.value]);
    }

    private void UpdateMainSlider()
    {
        SensivityLabel.text = $"Чувствительность: {SensivitySlider.value / 100f:f2}";
        AudioEffectsLabel.text = $"Аудио эффекты: {Mathf.FloorToInt((AudioEffectsSlider.value + 80) / .8f)}";
        AmbientLabel.text = $"Окружение: {Mathf.FloorToInt((AmbientSlider.value + 80) / .8f)}";
        MusicLabel.text = $"Музыка: {Mathf.FloorToInt((MusicSlider.value + 80) / .8f)}";
    }

    private void SelectState(int newState)
    {
        element.Clear();
        element.Add(BigBoss);
        element.Add(stateBoxes[newState]);
        state = newState;
        player.MiniMap.SetActive(state == 2);
    }

    private void OnEnable()
    {
        mainCamera = Camera.main;
        ui = GetComponent<UIDocument>();
        invalidPosition = new Vector2(float.NaN, float.NaN);
        element = ui.rootVisualElement;

        //States
        #region States
        string[] statesStr = new string[] { "Main", "Keys", "Map", "Inventory" };
        for (int i = 0; i < 4; i++)
        {
            stateButtons[i] = element.Q<Button>(statesStr[i] + "Btn");
            stateBoxes[i] = element.Q<GroupBox>(statesStr[i]);
            int j = i;
            stateButtons[i].clicked += () => SelectState(j);
        }
        BigBoss = element.Q<GroupBox>("BigBoss");
        #endregion

        //Main
        #region Main
        SensivitySlider = element.Q<Slider>("Sensivity");
        SensivityLabel = element.Q<Label>("SensivityLabel");

        AudioEffectsSlider = element.Q<Slider>("AudioEffects");
        AudioEffectsLabel = element.Q<Label>("AudioEffectsLabel");

        AmbientSlider = element.Q<Slider>("Ambient");
        AmbientLabel = element.Q<Label>("AmbientLabel");

        MusicSlider = element.Q<Slider>("Music");
        MusicLabel = element.Q<Label>("MusicLabel");

        GraphicField = element.Q<DropdownField>("Graphic");
        MainApplyBtn = element.Q<Button>("ApplyMain");
        MainApplyBtn.clicked += () => OnMainApply();
        element.Q<Button>("Quit").clicked += () =>
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
                Application.Quit();
            else SceneManager.LoadScene(0);
        };
        element.Q<Button>("SaveProgress").clicked += () => FindObjectOfType<GameManager>().SaveData();
        #endregion

        //Keys
        #region Keys
        ChangeKeysScreenBtns[0] = element.Q<Button>("KeysMenuLeft");
        ChangeKeysScreenBtns[1] = element.Q<Button>("KeysMenuRight");
        ChangeKeysScreenBtns[0].clicked += () => ChangeKeyScreenPressed(false);
        ChangeKeysScreenBtns[1].clicked += () => ChangeKeyScreenPressed(true);
        KeyScreenText = element.Q<Label>("KeysMenuLabel");
        KeyCode[][] keyCodes = new KeyCode[][] {
            new KeyCode[] { AllKeys.Forward, AllKeys.Backward, AllKeys.Left, AllKeys.Right },
            new KeyCode[] { AllKeys.ActivateButton, AllKeys.Settings, AllKeys.ChooseFreeHand },
            new KeyCode[] { AllKeys.Target, AllKeys.Fire, AllKeys.Super, AllKeys.ChangeWeapon, AllKeys.DropWeapon, AllKeys.Reload }
        };

        int[] lengths = new int[] { 4, 3, 6 };
        for (int group = 0; group < KeysGroups.Length; group++)
        {
            KeysGroups[group] = element.Q<GroupBox>("KeysGroup" + group);
            KeysButtons[group] = new Button[lengths[group]];
            for (int key = 0; key < lengths[group]; key++)
            {
                KeysButtons[group][key] = element.Q<Button>($"KeysBtn{group}{key}");
                int i1 = group;
                int i2 = key;
                KeysButtons[group][key].clicked += () => ChangeKeyBtnPressed(i1, i2);
                KeysButtons[group][key].text = keyCodes[group][key].ToString();
            }
        }

        for (int i = 1; i < KeysGroups.Length; i++)
            stateBoxes[1].Remove(KeysGroups[i]);
        #endregion

        //Inventory
        #region Inventory

        Label inventoryLabel = element.Q<Label>("InventoryText");
        string res = $"Здоровье: {player.MaxHealth}\nУрон: {player.DamageMultiply}\nСпособности: {player.AbilityMultiply}\n";
        foreach (string str in player.GetAbilitiesTexts())
            res += str + "\n";
        inventoryLabel.text = res;

        #endregion

        ui.panelSettings.SetScreenToPanelSpaceFunction((Vector2 screenPosition) =>
        {
            if (mainCamera == null) return invalidPosition;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, 100, uiMask))
                return invalidPosition;

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.y = 1 - pixelUV.y;
            pixelUV.x *= ui.panelSettings.targetTexture.width;
            pixelUV.y *= ui.panelSettings.targetTexture.height;

            return pixelUV;
        });

        for (int i = 0; i < KeysGroups.Length; i++)
        {
            if (stateBoxes[1].Contains(KeysGroups[i]))
                stateBoxes[1].Remove(KeysGroups[i]);
        }
        stateBoxes[1].Add(KeysGroups[keyState]);
        string[] texts = new string[] { "Движения", "Мир", "Боёвка" };
        KeyScreenText.text = texts[keyState];

        SelectState(state);
    }

    private void ChangeKeyBtnPressed(int group, int key)
    {
        player.SetNotice("Нажмите любую клавишу, чтобы назначить.");
        waitingKeyGroup = group;
        waitingKeyKey = key;
        isWaitingForPressingKey = true;
    }

    private void KeyIsChanged()
    {
        Array allKeys = Enum.GetValues(typeof(KeyCode));
        KeyCode key = KeyCode.None;
        foreach(KeyCode current in allKeys)
        {
            if(Input.GetKeyDown(current))
            {
                key = current;
                break;
            }
        }
        if (key == KeyCode.None)
        {
            player.SetNotice("Что-то пошло не так, может быть этой кнопки нет в списке всех возможных кнопок.");
            return;
        }

        isWaitingForPressingKey = false;
        for(int g = 0; g < KeysGroups.Length; g++)
        {
            for(int k = 0; k < KeysButtons[g].Length; k++)
            {
                if (KeysButtons[g][k].text == key.ToString())
                {
                    player.SetNotice("Такая кнопка уже назначена на другую команду или на эту же");
                    return;
                }
            }
        }

        #region MANY_IFS
        if (waitingKeyGroup == 0)
        {
            if (waitingKeyKey == 0) AllKeys.Forward = key;
            else if (waitingKeyKey == 1) AllKeys.Backward = key;
            else if (waitingKeyKey == 2) AllKeys.Left = key;
            else if (waitingKeyKey == 3) AllKeys.Right = key;
            else return;
        }
        else if (waitingKeyGroup == 1)
        {
            if (waitingKeyKey == 0) AllKeys.ActivateButton = key;
            else if (waitingKeyKey == 1) AllKeys.Settings = key;
            else if (waitingKeyKey == 2) AllKeys.ChooseFreeHand = key;
            else return;
        }
        else if (waitingKeyGroup == 2)
        {
            if (waitingKeyKey == 0) AllKeys.Target = key;
            else if (waitingKeyKey == 1) AllKeys.Fire = key;
            else if (waitingKeyKey == 2) AllKeys.Super = key;
            else if (waitingKeyKey == 3) AllKeys.ChangeWeapon = key;
            else if (waitingKeyKey == 4) AllKeys.DropWeapon = key;
            else if (waitingKeyKey == 5) AllKeys.Reload = key;
            else return;
        }
        else return;
        #endregion

        KeysButtons[waitingKeyGroup][waitingKeyKey].text = key.ToString();
    }

    private void ChangeKeyScreenPressed(bool isRight)
    {
        stateBoxes[1].Remove(KeysGroups[keyState]);
        keyState = (keyState + (isRight ? 1 : KeysGroups.Length - 1)) % KeysGroups.Length;
        stateBoxes[1].Add(KeysGroups[keyState]);
        string[] texts = new string[] { "Движение", "Мир", "Боёвка" };
        KeyScreenText.text = texts[keyState];
    }

    public void UpdateInfo()
    {
        SensivitySlider.value = AllSettings.Sensivity;
        AudioEffectsSlider.value = AllSettings.SoundEffects;
        AmbientSlider.value = AllSettings.Ambient;
        MusicSlider.value = AllSettings.Music;
        UpdateMainSlider();
        foreach(string key in qualityLevels.Keys)
        {
            if (qualityLevels[key] == QualitySettings.GetQualityLevel())
            {
                GraphicField.value = key;
                break;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) UpdateMainSlider();
        if (isWaitingForPressingKey && Input.anyKeyDown)
            KeyIsChanged();
    }

    public void SetPlayer(BasePlayer player)
    {
        this.player = player;
    }
}
