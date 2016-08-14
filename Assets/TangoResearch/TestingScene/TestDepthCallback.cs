using UnityEngine;
using System.Collections;
using Tango;
using System;
using UnityEngine.UI;

public class TestDepthCallback : MonoBehaviour, ITangoDepth
{

    private TangoApplication m_tangoApplication;

    public TangoPointCloud _tpc;

    public void Start()
    {
        m_tangoApplication = FindObjectOfType<TangoApplication>();

        m_tangoApplication.Register(this);

        
    }

    public void Update() {
        //_debugText.text = _tpc.m_depthDeltaTime.ToString();
    }

    /// <summary>
    /// Unity destroy function.
    /// </summary>
    public void OnDestroy()
    {
        m_tangoApplication.Unregister(this);
    }

    /// <summary>
    /// This is called when the permission granting process is finished.
    /// </summary>
    /// <param name="permissionsGranted"><c>true</c> if permissions were granted, otherwise <c>false</c>.</param>
    public void OnTangoPermissions(bool permissionsGranted)
    {
    }

    /// <summary>
    /// This is called when succesfully connected to the Tango service.
    /// </summary>
    public void OnTangoServiceConnected()
    {
        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }

    /// <summary>
    /// This is called when disconnected from the Tango service.
    /// </summary>
    public void OnTangoServiceDisconnected()
    {
    }

    public Text _debugText;
    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        _debugText.text = tangoDepth.m_timestamp.ToString();
    }
}
