using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMaterial : MonoBehaviour
{
    public Renderer Render;
    public Material[] Materials;
    public Renderer OtherObjectRendererForLink;

    private void Start()
    {
        if (OtherObjectRendererForLink == null)
            Render.material = Materials[Random.Range(0, Materials.Length)];
        else 
        {
            Render.material = OtherObjectRendererForLink.material;
        }
    }
}
