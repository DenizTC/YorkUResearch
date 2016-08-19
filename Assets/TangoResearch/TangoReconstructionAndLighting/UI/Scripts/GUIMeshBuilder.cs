using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;
using System;

public class GUIMeshBuilder : MonoBehaviour
{

    public static GUIMeshBuilder _GUIMeshBuilder;

    private SaveData _worldMesh;
    public Button _ClearButton;
    public Button _PauseResumeButton;
    public Button _ButtonExport;
    public Button _ButtonNewMesh;
    public Button _ButtonLoad;


    private bool _isMeshing = true;
    private TangoApplication _tangoApplication;
    private TangoDynamicMesh _dynamicMesh;
    private Text _pauseResumeText;
    private WaypointManager _wpm;

    void Awake()
    {
        if (!_GUIMeshBuilder)
            _GUIMeshBuilder = this;

        _GUIMeshBuilder.gameObject.SetActive(true);

        
    }

    void Start()
    {
        _wpm = FindObjectOfType<WaypointManager>();

        _ClearButton.onClick.AddListener(onClearClick);
        _PauseResumeButton.onClick.AddListener(onPauseResumeClick);
        _ButtonExport.onClick.AddListener(onClickExport);
        _ButtonLoad.onClick.AddListener(onClickLoad);
        _ButtonNewMesh.onClick.AddListener(onClickNewMesh);

        _ClearButton.gameObject.SetActive(false);
        _PauseResumeButton.gameObject.SetActive(false);
        _ButtonExport.gameObject.SetActive(false);
        _ButtonNewMesh.gameObject.SetActive(true);
        _ButtonLoad.gameObject.SetActive(true);

        _pauseResumeText = _PauseResumeButton.transform.GetChild(0).GetComponent<Text>();
        _tangoApplication = FindObjectOfType<TangoApplication>();
        _dynamicMesh = FindObjectOfType<TangoDynamicMesh>();

        setMeshing(false);
    }

    void Update() {
        //Debug.Log(_tangoApplication.m_enable3DReconstruction);
    }

    public Material _worldMaterial;

    private void onClearClick()
    {
        clearMesh();
    }

    private void onClickNewMesh()
    {
        _ClearButton.gameObject.SetActive(true);
        _PauseResumeButton.gameObject.SetActive(true);
        _ButtonExport.gameObject.SetActive(true);

        _ButtonNewMesh.gameObject.SetActive(false);
        _ButtonLoad.gameObject.SetActive(false);

        clearMesh();
        setMeshing(true);
    }

    private void onClickLoad()
    {
        string fileName = "/sdcard/" + GameGlobals.ActiveAreaDescription + ".obj";
        //string fileName = @"C:\Users\Deniz\Desktop\Deniz\GameArt\3DModels\cube.obj";
        //string fileName = "/sdcard/" + "cube" + ".obj";
        if (System.IO.File.Exists(fileName))
        {
            GameObject go = OBJLoader.LoadOBJFile(fileName);
            go.transform.Rotate(Vector3.up, 180);
            go.transform.GetComponentInChildren<MeshFilter>().transform.gameObject.AddComponent<MeshCollider>();
            go.layer = GameGlobals.WalkableLayer;
            go.transform.GetComponentInChildren<Renderer>().material = _worldMaterial;
        }

        _ClearButton.gameObject.SetActive(false);
        _PauseResumeButton.gameObject.SetActive(false);
        _ButtonExport.gameObject.SetActive(false);

        _ButtonNewMesh.gameObject.SetActive(true);
        _ButtonLoad.gameObject.SetActive(false);

        clearMesh(false);
        setMeshing(false);
    }

    private void onClickExport() {
        onPauseResumeClick();
        _dynamicMesh.transform.GetComponent<Exporter>().ToOBJ(false, GameGlobals.ActiveAreaDescription);
        //_worldMesh = new SaveData();

        //_worldMesh["Position"] = _dynamicMesh.transform.position;
        //_worldMesh["Rotation"] = _dynamicMesh.transform.rotation;

        //_worldMesh.Save("/sdcard/" + GameGlobals.ActiveAreaDescription + ".uml");


        _ClearButton.gameObject.SetActive(false);
        _PauseResumeButton.gameObject.SetActive(false);
        _ButtonExport.gameObject.SetActive(false);

        _ButtonNewMesh.gameObject.SetActive(true);
        _ButtonLoad.gameObject.SetActive(false);

        setMeshing(false);
    }

    private void onPauseResumeClick()
    {
        setMeshing(!_isMeshing);
    }

    private void clearMesh(bool meshOnly = false) {
        _dynamicMesh.Clear();
        _tangoApplication.Tango3DRClear();

        if (!meshOnly)
        {
            _wpm.Clear();
        }
    }

    private void setMeshing(bool val) {
        _isMeshing = val;
        _pauseResumeText.text = _isMeshing ? "Pause" : "Resume";
        _tangoApplication.Set3DReconstructionEnabled(_isMeshing);
    }

}
