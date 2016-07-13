using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;

public class GUIMeshBuilder : MonoBehaviour
{

    public static GUIMeshBuilder _GUIMeshBuilder;


    public Button _ClearButton;
    public Button _PauseResumeButton;

    private bool _isEnabled = true;
    private TangoApplication _tangoApplication;
    private TangoDynamicMesh _dynamicMesh;
    private Text _pauseResumeText;

    void Awake()
    {
        if (!_GUIMeshBuilder)
            _GUIMeshBuilder = this;

        _GUIMeshBuilder.gameObject.SetActive(true);
    }

    void Start()
    {
        _ClearButton.onClick.AddListener(OnClearClick);
        _PauseResumeButton.onClick.AddListener(OnPauseResumeClick);

        _pauseResumeText = _PauseResumeButton.transform.GetChild(0).GetComponent<Text>();
        _tangoApplication = FindObjectOfType<TangoApplication>();
        _dynamicMesh = FindObjectOfType<TangoDynamicMesh>();
    }

    private void OnClearClick()
    {
        _dynamicMesh.Clear();
        _tangoApplication.Tango3DRClear();
    }

    private void OnPauseResumeClick()
    {
        _isEnabled = !_isEnabled;
        _pauseResumeText.text = _isEnabled ? "Pause" : "Resume";
        _tangoApplication.Set3DReconstructionEnabled(_isEnabled);
    }


}
