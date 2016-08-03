using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RenderVextexColorController : MonoBehaviour {

    public Material material;

    void OnPreRender()
    {
        material.SetFloat("_RenderVertexColor", 0);
    }

    void OnPostRender()
    {
        material.SetFloat("_RenderVertexColor", 1);
    }
}
