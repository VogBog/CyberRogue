using UnityEngine;

public class NearPlayerSpot : MonoBehaviour
{
    public LayerMask Mask;

    public bool IsFree
    {
        get
        {
            return !Physics.CheckBox(transform.position, Vector3.one / 8, new Quaternion(), Mask);
        }
    }
}
