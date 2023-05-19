using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PCPlayerController : PlayerController
{
    private Vector3 velocity = new Vector3(0, -.2f, 0);
    private float xRotation = 0;
    private float movementNoiseVector = .1f;
    private float accuracy = 0, stepTime = .5f;

    private Vector3 startLeftHandPos, mapLeftHandPos;
    private Quaternion startLeftHandRot;
    private Vector3 startRightHandPos, fightRightHandPos;
    private Quaternion startRightHandRot;
    private bool isRotateLocked = false;
    private bool isTargetAnim = false, isReloadAnim = false, isMoveAnim = false;

    private void Start()
    {
        player.MainCamera.SetParent(transform);
        player.MainCamera.position = player.CameraPos.position;

        startLeftHandPos = player.DefaultLeftHandPos.localPosition - player.CameraPos.localPosition;
        startLeftHandRot = player.LeftHand.transform.localRotation;
        mapLeftHandPos = player.MapLeftHandPos.localPosition - player.CameraPos.localPosition;

        startRightHandPos = player.DefaultRightHandPos.localPosition - player.CameraPos.localPosition;
        fightRightHandPos = player.FightRightHandPos.localPosition - player.CameraPos.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.RightHand.transform.SetParent(player.MainCamera);
        player.LeftHand.transform.SetParent(player.MainCamera);

        player.LeftHand.transform.localPosition = startLeftHandPos;
        player.RightHand.transform.localPosition = startRightHandPos;
    }

    private void Update()
    {
        Debug.DrawLine(player.MainCamera.position, player.MainCamera.position + player.MainCamera.forward * 10);

        //Movement
        float moveX = 0;
        if (Input.GetKey(AllKeys.Left)) moveX--;
        if (Input.GetKey(AllKeys.Right)) moveX++;
        float moveY = 0;
        if (Input.GetKey(AllKeys.Backward)) moveY--;
        if (Input.GetKey(AllKeys.Forward)) moveY++;

        if (stepTime > 0) stepTime -= Time.deltaTime;
        else if(moveX != 0 || moveY != 0)
        {
            stepTime = .5f;
            player.PlayStepSound();
        }

        Vector3 moveVector = transform.right * moveX + transform.forward * moveY;
        if (moveVector.magnitude > 0 && player.CurrentWeapon && !isReloadAnim)
        {
            isMoveAnim = true;
            Vector3 curVec = isTargetAnim ? player.CurrentWeapon.TargetingOffset : fightRightHandPos;

            if (Vector3.Distance(curVec, player.RightHand.transform.localPosition) >= .05f)
                movementNoiseVector *= -1;
            player.RightHand.transform.localPosition += Vector3.right * movementNoiseVector * Time.deltaTime;

            accuracy = Mathf.Clamp(accuracy + Time.deltaTime, 0, .1f);
        }
        else if(!isReloadAnim && isMoveAnim)
        {
            isMoveAnim = false;
            UpdateRightHandAnim(player.CurrentWeapon, .1f);
            accuracy = Mathf.Clamp(accuracy - Time.deltaTime, 0, .1f);
        }

        if (!Physics.CheckSphere(player.PhysiscSphere.position, .2f, player.GravityMask))
            velocity += Vector3.down * player.Gravity * Time.deltaTime;
        else velocity = new Vector3(0, -.2f, 0);

        moveVector = moveVector * player.CurSpeed + velocity;
        controller.Move(moveVector * Time.deltaTime);

        player.SettingsGO.transform.position = player.SettingsPos.position;
        player.SettingsGO.transform.rotation = player.SettingsPos.rotation;

        //Rotation
        if (!isRotateLocked)
        {
            float mouseX = Input.GetAxis("Mouse X") * AllSettings.Sensivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * AllSettings.Sensivity * Time.deltaTime;

            xRotation = Mathf.Clamp(xRotation - mouseY, -90, 90);

            player.MainCamera.localRotation = Quaternion.Euler(xRotation, 0, 0);
            transform.Rotate(Vector3.up * mouseX);
        }

        //Find ingame interactive buttons
        Ray ray = new Ray(player.MainCamera.position, player.MainCamera.forward);
        RaycastHit buttonHit;
        if (Physics.Raycast(ray, out buttonHit, player.ChooseInGameUIDistance, player.ChooseInGameUIMask) && buttonHit.collider.CompareTag("ingamebutton"))
        {
            if (player.ChoosenInGameButtonGO == null || player.ChoosenInGameButtonGO != buttonHit.collider.gameObject)
                player.ChooseInGameButton(buttonHit.collider.gameObject);
        }
        else if (player.ChoosenInGameButtonGO != null)
            player.ChooseInGameButton(null);

        //Check buttons
        if (Input.GetKeyDown(AllKeys.ActivateButton) && player.ChoosenInGameButtonGO != null && !isReloadAnim)
            player.ChoosenInGameButton.OnClick(player);
        if (Input.GetKeyDown(AllKeys.Settings))
        {
            player.OnSettingsButtonPressed();
            if (!player.isFighting || player.isSettingsOpened)
            {
                Cursor.lockState = player.isSettingsOpened ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = player.isSettingsOpened;
                isRotateLocked = player.isSettingsOpened;

                Vector3 movePos = startLeftHandPos;
                if (player.isSettingsOpened) movePos = mapLeftHandPos;
                player.LeftHand.transform.DOLocalMove(movePos, .3f);
                if (player.isSettingsOpened)
                    player.LeftHand.transform.DOLocalRotate(new Vector3(0, 160, 0), .3f);
                else player.LeftHand.transform.DOLocalRotateQuaternion(startLeftHandRot, .3f);

                if (!player.isSettingsOpened)
                    UpdateLeftHandAnim(player.CurrentWeapon && player.CurrentWeapon.HoldWithLeftHand, .3f);
            }
        }
        if (!player.isSettingsOpened)
        {
            if (Input.GetKey(AllKeys.Target) && !isTargetAnim && player.CurrentWeapon && !isReloadAnim)
            {
                isTargetAnim = true;
                UpdateRightHandAnim(true);
            }
            else if (!Input.GetKey(AllKeys.Target) && isTargetAnim)
            {
                isTargetAnim = false;
                UpdateRightHandAnim(player.CurrentWeapon);
            }
            else if (Input.GetKeyDown(AllKeys.DropWeapon) && player.CurrentWeapon)
            {
                isReloadAnim = false;
                player.DropCurrentWeapon();
                UpdateRightHandAnim(false);
            }
            else if (Input.GetKeyDown(AllKeys.ChangeWeapon))
            {
                isReloadAnim = false;
                player.SwapWeaponPC();
            }
            else if (player.CurrentWeapon && (player.CurrentWeapon.IsBurnType ? Input.GetKeyDown(AllKeys.Fire) : Input.GetKey(AllKeys.Fire)))
            {
                player.CurrentWeapon.Accuracy = player.CurrentWeapon.Accuracy / player.CurrentWeapon.AccuracyMultiplier + (isTargetAnim ? accuracy / 3 : accuracy + .01f);
                if (player.CurrentWeapon.Fire())
                {
                    player.RightHand.transform.DOComplete();
                    player.RightHand.transform.rotation = new Quaternion();
                    player.RightHand.transform.Rotate(new Vector3(-player.CurrentWeapon.FireCallbackAngle, 0, 0), Space.Self);
                    Vector3 movement = Vector3.back * player.CurrentWeapon.FireCallbackVector;
                    float randAngle = Random.Range(0, .5f);
                    float x = -movement.y * randAngle;
                    float y = movement.x * randAngle;
                    movement.x = x; movement.y = y;
                    player.RightHand.transform.localPosition += movement;

                    UpdateRightHandAnim(true, player.CurrentWeapon.FireCallbackTime);
                    player.RightHand.transform.DOLocalRotate(Vector3.zero, player.CurrentWeapon.FireCallbackTime);
                }
            }
            else if (Input.GetKeyDown(AllKeys.Reload) && player.CurrentWeapon && player.CurrentWeapon.CanReloadMe() && !isReloadAnim && !isTargetAnim)
            {
                isReloadAnim = true;
                player.LeftHandAnimator.SetInteger("State", 0);
                player.CurrentWeapon.ReloadAnimForPC(player.LeftHand.transform, player.RightHand.transform, player.AmmoPose, this);
            }
            else if (!player.CurrentWeapon && player.ChoosenInGameButton)
            {
                if (Input.GetKey(AllKeys.Fire))
                    player.ChoosenInGameButton.OnDraging(buttonHit.point);
                else player.ChoosenInGameButton.StopDraging();
            }
            else if (Input.GetKeyDown(AllKeys.ChooseFreeHand))
            {
                player.ChooseFreeHandForPC();
            }
            else if (Input.GetKeyDown(KeyCode.F1))
            {
                game.IsAutoSafe = !game.IsAutoSafe;
                string notice = game.IsAutoSafe ? "Автосохранения включены" : "Автосохранения отключены";
                player.SetNotice(notice);
            }
            else if (Input.GetKeyDown(KeyCode.O))
                player.TeleportPlayerTo(new Vector3(8.5f, 3, -2.5f));
        }
    }

    public void SetReloadAnim(bool set) => isReloadAnim = set;

    public override void UpdateRightHandAnim(bool isRightHandHoldWeapon, float time = .2f)
    {
        if (isRightHandHoldWeapon)
        {
            player.RightHand.transform.DOComplete();
            isReloadAnim = false;
            if(isTargetAnim)
                player.RightHand.transform.DOLocalMove(player.CurrentWeapon.TargetingOffset, time);
            else
                player.RightHand.transform.DOLocalMove(fightRightHandPos, time);
        }
        else player.RightHand.transform.DOLocalMove(startRightHandPos, time);
    }

    public override void UpdateLeftHandAnim(bool isLeftHandHoldWeapon, float time = 0.2f)
    {
        player.LeftHand.transform.SetParent(player.MainCamera.transform);
        player.LeftHand.transform.localRotation = startLeftHandRot;
        player.LeftHand.transform.localPosition = startLeftHandPos;
        if (player.isSettingsOpened) return;
        if (isLeftHandHoldWeapon && player.CurrentWeapon)
        {
            player.LeftHandAnimator.SetInteger("State", 3);
            player.LeftHand.transform.SetParent(player.CurrentWeapon.transform);
            player.LeftHand.transform.DOLocalRotate(new Vector3(0, 90, 0), time);
            player.LeftHand.transform.DOLocalMove(player.CurrentWeapon.LeftHandPos.localPosition, time);
        }
        else
            player.LeftHand.transform.DOLocalMove(startLeftHandPos, time);
    }
}
