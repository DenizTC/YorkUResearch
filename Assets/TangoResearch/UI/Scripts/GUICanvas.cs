using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class GUICanvas : MonoBehaviour {

    public static Canvas _canvas;

	void Start () {
        if(!_canvas)
            _canvas = GetComponent<Canvas>();
	}
	
	void Update () {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            if (t0.phase == TouchPhase.Began && t1.phase == TouchPhase.Began) {
                toggleCanvas();
                Camera.main.cullingMask = (GameGlobals.CanvasEnabled) ?
                    Camera.main.cullingMask + (1 << GameGlobals.WaypointLayer) :
                    Camera.main.cullingMask - (1 << GameGlobals.WaypointLayer);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab)) {
            toggleCanvas();
        }
    }

    private void toggleCanvas()
    {
        GameGlobals.CanvasEnabled = !GameGlobals.CanvasEnabled;
        _canvas.enabled = GameGlobals.CanvasEnabled;
    }

}
