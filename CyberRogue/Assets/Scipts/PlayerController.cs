using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    protected BasePlayer player;
    protected CharacterController controller;
    protected GameManager game;

    public void SetData(BasePlayer player)
    {
        this.player = player;
        controller = player.CharacterController;
        game = FindObjectOfType<GameManager>();
    }

    public abstract void UpdateRightHandAnim(bool isRightHandHoldWeapon, float time = .2f);

    public abstract void UpdateLeftHandAnim(bool isLeftHandHoldWeapon, float time = .2f);
}
