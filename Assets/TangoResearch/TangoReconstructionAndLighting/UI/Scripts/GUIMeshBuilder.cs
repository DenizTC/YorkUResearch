using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;
using System;
using System.Threading;

public class GUIMeshBuilder : MonoBehaviour
{

    public static GUIMeshBuilder _GUIMeshBuilder;

    private SaveData _worldMesh;
    public Button _ClearButton;
    public Button _PauseResumeButton;
    public Button _ButtonExport;
    public Button _ButtonNewMesh;
    public Button _ButtonLoad;
    public Material _worldMaterial;

    private bool _isMeshing = true;
    private TangoApplication _tangoApplication;
    private TangoDynamicMesh _dynamicMesh;
    private Text _pauseResumeText;
    private WaypointManager _wpm;

    private GameObject _loadedMesh;

    private Thread _saveWorldThread;
    private Thread _loadWorldThread;

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

    #region Button Events

    private void onClearClick()
    {
        StartCoroutine(doClearMesh());
    }

    private void onClickNewMesh()
    {
        StartCoroutine(doNewMesh());
    }

    private void onClickLoad()
    {
        StartCoroutine(doLoad());
    }

    private void onClickExport() {
        StartCoroutine(doExport());
    }

    private void onPauseResumeClick()
    {
        setMeshing(!_isMeshing);
    }

    #endregion

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

    private IEnumerator doNewMesh() {

        MessageOkCancel oc;
        MessageManager._MessageManager
            .PushMessageOKCancel("Are you sure you want to continue? All unsaved work will be lost.", out oc);

        while (oc._WaitingForResponse)
        {
            yield return null;
        }

        MessageManager.Response r;
        oc.GetResponse(out r);

        if (r == MessageManager.Response.OK) {
            if (_loadedMesh != null)
                Destroy(_loadedMesh);

            _ClearButton.gameObject.SetActive(true);
            _PauseResumeButton.gameObject.SetActive(true);
            _ButtonExport.gameObject.SetActive(true);

            _ButtonNewMesh.gameObject.SetActive(false);
            _ButtonLoad.gameObject.SetActive(false);

            clearMesh();
            setMeshing(true);
        }

    }

    private IEnumerator doClearMesh() {
        MessageOkCancel oc;
        MessageManager._MessageManager
            .PushMessageOKCancel("Are you sure you want to continue? All unsaved work will be lost.", out oc);

        while (oc._WaitingForResponse)
        {
            yield return null;
        }

        MessageManager.Response r;
        oc.GetResponse(out r);

        if (r == MessageManager.Response.OK)
        {
            clearMesh();
        }
    }

    private IEnumerator doExport() {
        MessageOkCancel oc;
        MessageManager._MessageManager
            .PushMessageOKCancel("Exporting mesh may take a few minutes. Continue?", out oc);

        while (oc._WaitingForResponse)
        {
            yield return null;
        }

        MessageManager.Response r;
        oc.GetResponse(out r);

        if (r == MessageManager.Response.OK)
        {
            onPauseResumeClick();

            _saveWorldThread = new Thread(delegate ()
            {
                _dynamicMesh.transform.GetComponent<Exporter>().ToOBJ(false, GameGlobals.ActiveAreaDescription);
            });
            _saveWorldThread.Start();

            
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
    }

    private IEnumerator doLoad()
    {
        MessageOkCancel oc;
        MessageManager._MessageManager
            .PushMessageOKCancel("Loading mesh may take a few minutes. Continue?", out oc);

        while (oc._WaitingForResponse)
        {
            yield return null;
        }

        MessageManager.Response r;
        oc.GetResponse(out r);

        if (r == MessageManager.Response.OK)
        {
            
            _loadedMesh = new GameObject();
            _loadWorldThread = new Thread(delegate ()
            {
                string fileName = "/sdcard/" + GameGlobals.ActiveAreaDescription + ".obj";

                if (System.IO.File.Exists(fileName))
                {
                    _loadedMesh = OBJLoader.LoadOBJFile(fileName);
                    
                }
            });
            _loadWorldThread.Start();

            while (_loadWorldThread != null && _loadWorldThread.ThreadState == ThreadState.Running)
            {
                yield return null;
            }

            _loadedMesh.transform.Rotate(Vector3.up, 180);
            _loadedMesh.transform.GetComponentInChildren<MeshFilter>().transform.gameObject.AddComponent<MeshCollider>();
            _loadedMesh.layer = GameGlobals.WalkableLayer;
            _loadedMesh.transform.GetComponentInChildren<Renderer>().material = _worldMaterial;

            _ClearButton.gameObject.SetActive(false);
            _PauseResumeButton.gameObject.SetActive(false);
            _ButtonExport.gameObject.SetActive(false);

            _ButtonNewMesh.gameObject.SetActive(true);
            _ButtonLoad.gameObject.SetActive(false);

            clearMesh(false);
            setMeshing(false);
        }
    }

}
