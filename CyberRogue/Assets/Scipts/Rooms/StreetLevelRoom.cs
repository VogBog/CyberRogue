using System.Collections.Generic;
using UnityEngine;

public class StreetLevelRoom : LevelRoom
{
    public StreetLevelRoomSideWall[] SideWalls;
    public int MaxWay;

    private bool[][] wallsActives;

    private void SetAllActive(bool isActive)
    {
        if (!isActive)
            wallsActives = new bool[SideWalls.Length][];
        for(int i = 0; i < SideWalls.Length; i++)
        {
            if (!isActive)
                wallsActives[i] = new bool[SideWalls[i].AllVariations.Length];
            for(int j = 0; j < SideWalls[i].AllVariations.Length; j++)
            {
                if (!isActive)
                {
                    wallsActives[i][j] = SideWalls[i].AllVariations[j].activeSelf;
                    SideWalls[i].AllVariations[j].SetActive(false);
                }
                else SideWalls[i].AllVariations[j].SetActive(wallsActives[i][j]);
            }
        }
    }

    public void GenerateSideWalls()
    {
        List<StreetLevelRoomSideWall.Way> ways = new List<StreetLevelRoomSideWall.Way>();
        for (int i = 0; i <= MaxWay; i++)
        {
            StreetLevelRoomSideWall.Way newWay = new StreetLevelRoomSideWall.Way();
            newWay.FirstExit = i;
            newWay.SecondExit = (i + 1) % (MaxWay + 1);
            ways.Add(newWay);
        }
        Coll.enabled = false;
        SetAllActive(false);

        for (int i = 0; i < SideWalls.Length; i++)
        {
            bool isAllContain = true;
            for(int j = 0; j < SideWalls[i].WaysExits.Length; j++)
            {
                bool isFinded = false;
                for(int k = 0; k < ways.Count; k++)
                {
                    if (SideWalls[i].WaysExits[j].isEqualsTo(ways[k]))
                    {
                        isFinded = true;
                        break;
                    }
                }
                if(!isFinded)
                {
                    isAllContain = false;
                    break;
                }
            }
            if(isAllContain  && SideWalls[i].isCanStayHere())
            {
                for(int j = 0; j < SideWalls[i].WaysExits.Length; j++)
                {
                    for(int k = 0; k < ways.Count; k++)
                    {
                        if (ways[k].isEqualsTo(SideWalls[i].WaysExits[j]))
                        {
                            ways.RemoveAt(k);
                            break;
                        }
                    }
                }

                int indx = Random.Range(0, SideWalls[i].AllVariations.Length);
                wallsActives[i][indx] = true;
                //SideWalls[i].AllVariations[Random.Range(0, SideWalls[i].AllVariations.Length)].gameObject.SetActive(true);
            }
        }
        SetAllActive(true);
        Coll.enabled = true;
    }

    public override void DeleteAllUnused()
    {
        base.DeleteAllUnused();
        for(int i = 0; i < SideWalls.Length; i++)
        {
            bool isUsed = false;
            for(int j = 0; j < SideWalls[i].AllVariations.Length; j++)
            {
                if (!SideWalls[i].AllVariations[j].activeSelf)
                    Destroy(SideWalls[i].AllVariations[j]);
                else isUsed = true;
            }
            if (!isUsed)
                Destroy(SideWalls[i].gameObject);
        }
    }
}
