using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TangoDepthMapGUIController : MonoBehaviour {

    public Text _DirtyRadiusText;
    public Text _ResText;
    public Slider _DirtyRadiusSlider;
    public Button _IncreaseRes;
    public Button _DecreaseRes;
    public Button _SwitchBGButton;
    public CustomZBufferEffect _CustorZBuffer;
    public PointCloudToDepthMap _DepthMapGenerator;

    private int _resLevel = 1;
    private Text _switchBGText;
    private bool _rgbMode = true;

    private void Start() {
        _switchBGText = _SwitchBGButton.GetComponentInChildren<Text>();
        _SwitchBGButton.onClick.AddListener(onSwitchBGClick);
        _IncreaseRes.onClick.AddListener(onIncreaseResClick);
        _DecreaseRes.onClick.AddListener(onDecreaseResClick);
        _DirtyRadiusSlider.onValueChanged.AddListener(onDirtyRadiusChanged);



    }

    private void onSwitchBGClick() {
        _rgbMode = !_rgbMode;
        float val = (_rgbMode) ? 1 : 0;
        string text = (_rgbMode) ? "Color" : "Depth";

        _switchBGText.text = text;
        _CustorZBuffer._material.SetFloat("_RGBMode", val);
    }

    private void onIncreaseResClick() {
        increaseRes();
    }

    private void onDecreaseResClick()
    {
        decreaseRes();
    }

    private void increaseRes() {
        if (_resLevel + 1 > 3)
            return;

        _resLevel++;
        Vector2 newRes = currentRes();
        _ResText.text = newRes.x + " x " + newRes.y;

        _DepthMapGenerator.ChangeDepthMapResolution((int)newRes.x, (int)newRes.y);
        _CustorZBuffer._material.SetFloat("_DepthWidth", _DepthMapGenerator._depthMapWidth);
        _CustorZBuffer._material.SetFloat("_DepthHeight", _DepthMapGenerator._depthMapHeight);
    }

    private void decreaseRes()
    {
        if (_resLevel - 1 < 0)
            return;

        _resLevel--;
        Vector2 newRes = currentRes();
        _ResText.text = newRes.x + " x " + newRes.y;

        _DepthMapGenerator.ChangeDepthMapResolution((int)newRes.x, (int)newRes.y);
        _CustorZBuffer._material.SetFloat("_DepthWidth", _DepthMapGenerator._depthMapWidth);
        _CustorZBuffer._material.SetFloat("_DepthHeight", _DepthMapGenerator._depthMapHeight);
    }

    private void onDirtyRadiusChanged(float value) {
        int rad = (int)value;
        _DepthMapGenerator._DirtyRadius = rad;
        _DirtyRadiusText.text = rad.ToString();
    }

    private Vector2 currentRes() {
        switch (_resLevel)
        {
            case 0:
                return new Vector2(80,45);
            case 1:
                return new Vector2(160, 90);
            case 2:
                return new Vector2(240, 135);
            case 3:
                return new Vector2(320, 180);
            default:
                return new Vector2(80, 45);
        }
    }

}
