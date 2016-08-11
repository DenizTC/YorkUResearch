//-----------------------------------------------------------------------
// <copyright file="ARGUIController.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Tango;
using UnityEngine;
using UnityEngine.UI;

public class ARObjectManager : MonoBehaviour, ITangoLifecycle, ITangoDepth
{

    private float forwardVelocity = 5.0f;
    public ARSelectable[] _Selectables;

    /// <summary>
    /// Index map for the _Selectables array.
    /// </summary>
    private Dictionary<Enums.SelectionType, int> _selectablesMap = new Dictionary<Enums.SelectionType, int>();

    /// <summary>
    /// The touch effect to place on taps.
    /// </summary>
    public RectTransform m_prefabTouchEffect;

    /// <summary>
    /// The canvas to place 2D game objects under.
    /// </summary>
    public Canvas m_canvas;

    /// <summary>
    /// The point cloud object in the scene.
    /// </summary>
    public TangoPointCloud m_pointCloud;

    public int _PrefabObjLayer;

    private TangoApplication m_tangoApplication;
    private TangoARPoseController m_tangoPose;
    private string m_tangoServiceVersion;

    /// <summary>
    /// If set, then the depth camera is on and we are waiting for the next depth update.
    /// </summary>
    private bool m_findPlaneWaitingForDepth;

    /// <summary>
    /// If set, this is the selected prefab.
    /// </summary>
    private ARSelectable m_selectedPrefab;

    /// <summary>
    /// If set, this is the rectangle bounding the selected prefab.
    /// </summary>
    private Rect m_selectedRect;

    public void Start()
    {
        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_tangoPose = FindObjectOfType<TangoARPoseController>();
        m_tangoServiceVersion = TangoApplication.GetTangoServiceVersion();

        m_tangoApplication.Register(this);

        for (int i = 0; i < _Selectables.Length; i++)
        {
            Debug.Log(_Selectables[i]._SelectableType.ToString() + " " + i);
            _selectablesMap.Add(_Selectables[i]._SelectableType, i);
        }

    }

    public void OnDestroy()
    {
        m_tangoApplication.Unregister(this);
    }

    public void Update()
    {
        _UpdateLocationMarker();
    }
    
    public void OnTangoPermissions(bool permissionsGranted)
    {
    }

    public void OnTangoServiceConnected()
    {
        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }
    
    public void OnTangoServiceDisconnected()
    {
    }

    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        // Don't handle depth here because the PointCloud may not have been updated yet.  Just
        // tell the coroutine it can continue.
        m_findPlaneWaitingForDepth = false;
    }

    /// <summary>
    /// Convert a 3D bounding box into a 2D Rect.
    /// </summary>
    /// <returns>The 2D Rect in Screen coordinates.</returns>
    /// <param name="cam">Camera to use.</param>
    /// <param name="bounds">3D bounding box.</param>
    private Rect WorldBoundsToScreen(Camera cam, Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        Bounds screenBounds = new Bounds(cam.WorldToScreenPoint(center), Vector3.zero);

        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, -extents.z)));
        return Rect.MinMaxRect(screenBounds.min.x, screenBounds.min.y, screenBounds.max.x, screenBounds.max.y);
    }

    /// <summary>
    /// Update location marker state.
    /// </summary>
    private void _UpdateLocationMarker()
    {

        if (Input.touchCount == 2)
        {
            return;
        }

        if (Input.touchCount == 1 || Input.GetMouseButtonDown(1))
        {
            // Single tap -- place new location or select existing location.

            Vector2 pos = new Vector2();
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                pos = t.position;
                if (t.phase != TouchPhase.Began ||
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    return;
                }
            }
            else
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;
                pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }

            if (GameGlobals.PropertiesPanelOpen) {
                GameGlobals.SetPropertiesOpen(false);
                return;
            }

            Vector2 guiPosition = new Vector2(pos.x, Screen.height - pos.y);
            Camera cam = Camera.main;
            RaycastHit hitInfo;

            if (Physics.Raycast(cam.ScreenPointToRay(pos), out hitInfo, 10, 1 << _PrefabObjLayer))
            {
                // Found a prefab, select it (so long as it isn't disappearing)!
                GameObject tapped = hitInfo.collider.transform.root.gameObject;
                //Debug.Log(tapped.name + " tapped!");
                m_selectedPrefab = tapped.GetComponent<ARSelectable>();
                m_selectedPrefab.MakeSelected();
            }
            else
            {
                // Place a new point at that location, clear selection
                m_selectedPrefab = null;
                GameGlobals.ChangeSelected(Enums.SelectionType.NONE);

                StartCoroutine(_WaitForDepthAndFindPlane(pos));

                // Because we may wait a small amount of time, this is a good place to play a small
                // animation so the user knows that their input was received.
                RectTransform touchEffectRectTransform = (RectTransform)Instantiate(m_prefabTouchEffect);
                touchEffectRectTransform.transform.SetParent(m_canvas.transform, false);
                Vector2 normalizedPosition = pos;
                normalizedPosition.x /= Screen.width;
                normalizedPosition.y /= Screen.height;
                touchEffectRectTransform.anchorMin = touchEffectRectTransform.anchorMax = normalizedPosition;
            }
        }

    }

    /// <summary>
    /// Wait for the next depth update, then find the plane at the touch position.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    /// <param name="touchPosition">Touch position to find a plane at.</param>
    private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition)
    {
        //Debug.Log("Waiting for new depth------------------------------");
        m_findPlaneWaitingForDepth = true;
        // Turn on the camera and wait for a single depth update.
        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
        while (m_findPlaneWaitingForDepth)
        {
            yield return null;
        }

        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
        //Debug.Log("------------------------------New depth available!");
        // Find the plane.
        Camera cam = Camera.main;
        Vector3 planeCenter;
        Plane plane;
        if (!m_pointCloud.FindPlane(cam, touchPosition, out planeCenter, out plane))
        {
            yield break;
        }

        // Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
        Vector3 up = plane.normal;
        Vector3 forward;
        if (Vector3.Angle(plane.normal, cam.transform.forward) < 175)
        {
            Vector3 right = Vector3.Cross(up, cam.transform.forward).normalized;
            forward = Vector3.Cross(right, up).normalized;
        }
        else
        {
            // Normal is nearly parallel to camera look direction, the cross product would have too much
            // floating point error in it.
            forward = Vector3.Cross(up, cam.transform.right);
        }

        InstantiateSelectable(planeCenter, Quaternion.LookRotation(forward, up));
        m_selectedPrefab = null;
    }

    private void InstantiateSelectable(Vector3 pos, Quaternion rot)
    {
        //ARSelectable ars = _Selectables[_selectablesMap[GameGlobals.CurrentDrawingSelection]];
        ARSelectable newARS = 
            GameObject.Instantiate(_Selectables[_selectablesMap[GameGlobals.CurrentDrawingSelection]], pos, rot) as ARSelectable;
        //Debug.Log(newARS.GetType().ToString());
        if (newARS._Projectile)
        {
            newARS.transform.position =
                Camera.main.transform.position - (Camera.main.transform.up * newARS.transform.localScale.y);
            newARS.transform.GetComponent<Rigidbody>().velocity =
                (Camera.main.transform.forward * forwardVelocity) + (Camera.main.transform.up * forwardVelocity / 2);
        }
        
    }

}
