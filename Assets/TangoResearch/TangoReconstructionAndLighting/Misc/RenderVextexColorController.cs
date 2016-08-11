using UnityEngine;
using System.Collections;

/// <summary>
/// Used for the TangoReconstructionAndLighting demo. Needed for the main camera to render
/// the live video over the reconstructed mesh, then render the vertex colors of the 
/// reconstructed mesh for the reflection probe.
/// </summary>
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
