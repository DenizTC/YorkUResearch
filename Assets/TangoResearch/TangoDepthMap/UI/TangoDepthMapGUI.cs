using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class TangoDepthMapGUI : MonoBehaviour {

    private Canvas _canvas;

    public Text _RadiusText;
    public Text _ResText;
    public Slider _RadiusSlider;
    public Slider _FillAmountSlider;

    public Toggle _NoFill;
    public Toggle _SimpleFill;
    public Toggle _MeanFill;

    public Toggle _DepthPrediction;
    public Toggle _DepthFull;
    public Toggle _DepthMasked;

    public Button _IncreaseRes;
    public Button _DecreaseRes;
    public Button _SwitchBGButton;

    public Button _NoFilter;
    public Button _Kuwahara;
    public Button _GuidedFilter;
    public Button _Gaussian;
    public Button _Median;
    public Button _MaskedMean;
    
    private Enums.DepthFilter _CurrentFilter = Enums.DepthFilter.NONE;
    public TangoOcclusion _TangoOcclusion;
    public TangoDepthGenerator _DepthMapGenerator;

    private int _resLevel = 1;
    private Text _switchBGText;
    private bool _rgbMode = true;
    private Image _curBtnImage;

    private void Start() {
        if (!_canvas)
            _canvas = GetComponent<Canvas>();

        _switchBGText = _SwitchBGButton.GetComponentInChildren<Text>();
        _SwitchBGButton.onClick.AddListener(onSwitchBGClick);
        _IncreaseRes.onClick.AddListener(onIncreaseResClick);
        _DecreaseRes.onClick.AddListener(onDecreaseResClick);
        _RadiusSlider.onValueChanged.AddListener(onRadiusChanged);
        _FillAmountSlider.onValueChanged.AddListener(onFillAmountChanged);

        _DepthPrediction.onValueChanged.AddListener(onDepthPredictionChanged);
        _NoFill.onValueChanged.AddListener(onNoFillChanged);
        _SimpleFill.onValueChanged.AddListener(onSimpleFillChanged);
        _MeanFill.onValueChanged.AddListener(onMeanFillChanged);

        _DepthFull.onValueChanged.AddListener(onDepthFullChanged);
        _DepthMasked.onValueChanged.AddListener(onDepthMaskedChanged);

        _NoFilter.onClick.AddListener(delegate { onFilterClick(_NoFilter); });
        _Kuwahara.onClick.AddListener(delegate { onFilterClick(_Kuwahara); });
        _GuidedFilter.onClick.AddListener(delegate { onFilterClick(_GuidedFilter); });
        _Gaussian.onClick.AddListener(delegate { onFilterClick(_Gaussian); });
        _Median.onClick.AddListener(delegate { onFilterClick(_Median); });
        _MaskedMean.onClick.AddListener(delegate { onFilterClick(_MaskedMean); });

        _curBtnImage = _NoFilter.GetComponent<Image>();
        _curBtnImage.color = Color.black;
    }


    #region ToggleEvents

    private void onDepthPredictionChanged(bool val) {
        _DepthMapGenerator._DepthPrediction = val;
    }

    private void onNoFillChanged(bool val) {
        if (val)
            _DepthMapGenerator._FillMode = Enums.FillHoleMode.NOFILL;

        //_FillAmountSlider.transform.parent.gameObject.SetActive(!val);
    }

    private void onSimpleFillChanged(bool val)
    {
        if (val)
            _DepthMapGenerator._FillMode = Enums.FillHoleMode.SIMPLE;
    }

    private void onMeanFillChanged(bool val)
    {
        if (val)
            _DepthMapGenerator._FillMode = Enums.FillHoleMode.MEAN;
    }

    private void onDepthFullChanged(bool val)
    {
        if (val) _DepthMapGenerator._DepthMapMode = Enums.DepthMapMode.FULL;
    }

    private void onDepthMaskedChanged(bool val)
    {
        if (val) _DepthMapGenerator._DepthMapMode = Enums.DepthMapMode.MASKED;
    }

    #endregion

    #region OnClickEvents

    private void onSwitchBGClick() {
        _rgbMode = !_rgbMode;
        float val = (_rgbMode) ? 1 : 0;
        string text = (_rgbMode) ? "Color" : "Depth";

        _switchBGText.text = text;

        if (_rgbMode) _TangoOcclusion.ChangeBackground(Enums.BackgroundMode.COLOR);
        else _TangoOcclusion.ChangeBackground(Enums.BackgroundMode.DEPTH);

    }

    private void onIncreaseResClick() {
        increaseRes();
    }

    private void onDecreaseResClick()
    {
        decreaseRes();
    }

    private void onFilterClick(Button btn) {

        switch (btn.name)
        {
            case "ButtonKuwahara":
                _CurrentFilter = Enums.DepthFilter.KUWAHARA;
                break;
            case "ButtonGuidedFilter":
                _CurrentFilter = Enums.DepthFilter.GUIDEDFILTER;
                break;
            case "ButtonGaussian":
                _CurrentFilter = Enums.DepthFilter.GAUSSIAN;
                break;
            case "ButtonMedian":
                _CurrentFilter = Enums.DepthFilter.MEDIAN;
                break;
            case "ButtonMaskedMean":
                _CurrentFilter = Enums.DepthFilter.MASKEDMEAN;
                break;
            default:
                _CurrentFilter = Enums.DepthFilter.NONE;
                break;
        }
        _curBtnImage.color = Color.white;
        _curBtnImage = btn.GetComponent<Image>();
        _curBtnImage.color = Color.black;
        _TangoOcclusion.ChangeFilter(_CurrentFilter);
    }

    private void increaseRes() {
        if (_resLevel + 1 > 4)
            return;

        _resLevel++;
        Vector2 newRes = currentRes();
        _ResText.text = newRes.x + " x " + newRes.y;

        _DepthMapGenerator.ChangeDepthMapResolution((int)newRes.x, (int)newRes.y);
    }

    private void decreaseRes()
    {
        if (_resLevel - 1 < 0)
            return;

        _resLevel--;
        Vector2 newRes = currentRes();
        _ResText.text = newRes.x + " x " + newRes.y;

        _DepthMapGenerator.ChangeDepthMapResolution((int)newRes.x, (int)newRes.y);
    }

    #endregion

    #region OnSliderChangedEvents

    private void onRadiusChanged(float value) {
        int rad = (int)value;
        _DepthMapGenerator._ScaleRadiusSize = rad;
        _RadiusText.text = rad.ToString();
    }

    private void onFillAmountChanged(float value) {
        _DepthMapGenerator._MaxEdgeIterations = (int)(value / 6);
    }

    #endregion



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
            case 4:
                return new Vector2(640, 360);
            default:
                return new Vector2(80, 45);
        }
    }

}
