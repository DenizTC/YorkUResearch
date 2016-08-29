using UnityEngine;
using System.Collections;

public class DisplayResult : MonoBehaviour {

    public Renderer _Screen;

	void Update () {

        if (FindLight._LightDetector != null)
        {
            Texture2D tex = FindLight._LightDetector._OutTexture;
            _Screen.material.mainTexture = tex;
        }
	}
}
