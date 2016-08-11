using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class HiddenUI : MonoBehaviour {

    public Button _ButtonShowHideMainUI;
    public Canvas _MainUI;

    private Canvas _canvas;


	void Start () {
        _canvas = GetComponent<Canvas>();

        if(_ButtonShowHideMainUI)
            _ButtonShowHideMainUI.onClick.AddListener(onClickShowHideMainUI);
	}

    private void Update()
    {
        if (Input.touchCount == 3)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            Touch t2 = Input.GetTouch(2);
            if (t0.phase == TouchPhase.Began &&
                t1.phase == TouchPhase.Began &&
                t2.phase == TouchPhase.Began)
            {
                toggleCanvas();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            toggleCanvas();
        }

    }

    private void onClickShowHideMainUI() {
        toggleCanvas();
        
    }

    private void toggleCanvas()
    {
        GameGlobals.CanvasEnabled = !GameGlobals.CanvasEnabled;
        _MainUI.enabled = GameGlobals.CanvasEnabled;

        Camera.main.cullingMask = (GameGlobals.CanvasEnabled) ?
            Camera.main.cullingMask + (1 << GameGlobals.WaypointLayer) :
            Camera.main.cullingMask - (1 << GameGlobals.WaypointLayer);
    }


}
