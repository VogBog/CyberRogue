using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetLevelRoomSideWall : MonoBehaviour
{
    [System.Serializable]
    public class Way
    {
        public int FirstExit, SecondExit;

        public bool isEqualsTo(Way other)
        {
            if (FirstExit == other.FirstExit && SecondExit == other.SecondExit ||
                FirstExit == other.SecondExit && SecondExit == other.FirstExit)
                return true;
            return false;
        }
    }

    public Transform[] Rays;
    public LayerMask RayMask;
    public Way[] WaysExits;
    public GameObject[] AllVariations;

    public bool isCanStayHere()
    {
        foreach(Transform curRay in Rays)
        {
            if (Physics.CheckBox(curRay.position, new Vector3(.1f, 10, .1f), new Quaternion(), RayMask))
                return false;
            //Ray ray = new Ray(curRay.position, Vector3.down);
            //RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, 10, RayMask))
            //    return false;
        }
        return true;
    }
}
