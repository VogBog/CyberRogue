using UnityEngine;

public class StreetSkyBoxHome : MonoBehaviour
{
    public Vector3 CheckBoxSize;
    public LayerMask Mask;

    public bool isFreeAtThatPos(Vector3 pos)
    {
        return !Physics.CheckBox(pos, CheckBoxSize / 2, new Quaternion(), Mask);
    }
}
