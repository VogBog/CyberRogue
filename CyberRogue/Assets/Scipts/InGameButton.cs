using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InGameButton : MonoBehaviour
{
    public abstract void OnChoosen();

    public abstract void OnUnchoosen();

    public abstract void OnClick(BasePlayer player);

    public abstract void OnDraging(Vector3 dragPos);

    public abstract void StopDraging();
}
