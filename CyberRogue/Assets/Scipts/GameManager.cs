using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Camera MainCamera;
    public Camera MiniMapCamera;
    public BasePlayer Player;
    public GameObject PCCursor, PCChooseCursor;
    public Light SunLight;
    public Transform PlayerDeadPosition;
    public AudioClip WinSound;

    [HideInInspector] public bool IsAutoSafe = false;

    protected AIManager localAIManager;

    private Bloom bloomEffect;
    private float startBloomIntensity, bloomEffectInstensitySpeed, startSunLightIntensity;
    private Color startBloomColor, bloomEffectColorSpeed, startSunLightColor;
    private Coroutine bloomCor;
    private bool isBloomEffectGoing = false, isDead = false;

    public GameObject CameraGO { get; private set; }


    private void Start()
    {
        List<VolumeComponent> volume = FindObjectOfType<Volume>().profile.components;
        for(int i = 0; i < volume.Count; i++)
        {
            if (volume[i] is Bloom bloom)
            {
                bloomEffect = bloom;
                break;
            }
        }
        if (bloomEffect != null)
        {
            startBloomIntensity = bloomEffect.intensity.value;
            startBloomColor = bloomEffect.tint.value;
        }
        startSunLightColor = SunLight.color;
        startSunLightIntensity = SunLight.intensity;
        CameraGO = MainCamera.gameObject;

        AfterStart();
    }

    public void GetBlindEffect(float time, float intensity)
    {
        if (Player.HasThisAbility(Ability.AbilityType.NoFlash))
        {
            intensity /= 2 * Player.AbilityMultiply;
            time /= 2 * Player.AbilityMultiply;
        }

        if(Player.HasThisAbility(Ability.AbilityType.Morningstar))
            Player.SetUndead(time * Player.AbilityMultiply * .3f);

        if (bloomEffect == null) return;

        bloomEffectInstensitySpeed = (intensity - startBloomIntensity) / time;
        bloomEffectColorSpeed = (Color.white - startBloomColor) / time;

        if(bloomCor != null) StopCoroutine(bloomCor);
        bloomCor = StartCoroutine(GetBlindEffectIE(time, intensity));

        Player.BlindAudioSource.Play();
    }

    IEnumerator GetBlindEffectIE(float time, float intensity)
    {
        bloomEffect.intensity.value = intensity;
        bloomEffect.tint.value = Color.white;
        SunLight.DOColor(Color.white, .1f);
        SunLight.DOIntensity(50, .1f);

        yield return new WaitForSeconds(time / 2);

        isBloomEffectGoing = true;
        SunLight.DOColor(startSunLightColor, time / 2);
        SunLight.DOIntensity(startSunLightIntensity, time / 2);

        yield return new WaitForSeconds(time / 2);

        isBloomEffectGoing = false;

        yield return null;

        bloomEffect.intensity.value = startBloomIntensity;
        bloomEffect.tint.value = startBloomColor;
    }

    protected virtual void AfterStart() { }

    private void Update()
    {
        if(isBloomEffectGoing)
        {
            bloomEffect.intensity.value -= bloomEffectInstensitySpeed * Time.deltaTime;
            bloomEffect.tint.value -= bloomEffectColorSpeed * Time.deltaTime;
        }

        AfterUpdate();
    }

    protected virtual void AfterUpdate() { }

    public void GameManagerTriggerIsEnter(string name)
    {
        GameManagerTriggerEnterVirtual(name);
    }

    protected virtual void GameManagerTriggerEnterVirtual(string name) { }

    public virtual void ThingIsDestroyed(DestroyableByWeapon thing) { }

    public virtual void UpdateLocalAIManager(AIManager ai) => localAIManager = ai;

    public virtual void PlayerDead()
    {
        if (isDead) return;
        isDead = true;
        Player.TeleportPlayerTo(PlayerDeadPosition.position);
        StartCoroutine(PlayerDeadIE());
    }

    IEnumerator PlayerDeadIE()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(0);
    }

    public virtual void SaveData() { }

    public virtual void LoadData() { }

    public virtual void BossIsDead() { }

    public virtual void EndWave(LevelRoom whatRoom)
    {
        Player.PlayMusic(WinSound, false);
    }
}
