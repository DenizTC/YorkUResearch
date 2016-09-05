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
using System.Collections;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class ARObjectManager : MonoBehaviour, ITangoDepth
{

    public static ARObjectManager _AROBJManager;
    public Camera _MainCam;

    private float forwardVelocity = 5.0f;
    public ARSelectable[] _ARObjects;

    /// <summary>
    /// Maps unique AR object id to _ARObjects array index.
    /// </summary>
    private Dictionary<int, int> _arObjMap = new Dictionary<int, int>();

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

    private TangoApplication m_tangoApplication;
    private string m_tangoServiceVersion;

    /// <summary>
    /// If set, then the depth camera is on and we are waiting for the next depth update.
    /// </summary>
    private bool m_findPlaneWaitingForDepth;

    /// <summary>
    /// If set, this is the selected prefab.
    /// </summary>
    private ARSelectable m_selectedPrefab;

    public void Start()
    {
        if(!_AROBJManager)
            _AROBJManager = this;

        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_tangoServiceVersion = TangoApplication.GetTangoServiceVersion();

        m_tangoApplication.Register(this);


        for (int i = 0; i < _ARObjects.Length; i++)
        {
            // Setup the index map
            _arObjMap.Add(_ARObjects[i].GetInstanceID(), i);

            // Add the ARToggles
            GUISelectables._GUISelectables.AddSelectable(_ARObjects[i]._Icon, _ARObjects[i].GetInstanceID());
        }
    }

    public void OnDestroy()
    {
        m_tangoApplication.Unregister(this);
    }

    public void Update()
    {
        if (Input.touchCount == 2)
        {
            return;
        }

        if (Input.touchCount == 1 || Input.GetMouseButtonDown(1))
        {
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

            if (GameGlobals.MovingObject) {
                GameGlobals.MovingObject = false;
                return;
            }

            if (GameGlobals.PropertiesPanelOpen)
            {
                GameGlobals.SetPropertiesOpen(false);
                GUISelectables._GUISelectables.DeselectAll();
                return;
            }            

            if (GUISelectables._GUISelectables.IsAnyToggled())
            {
                // Draw selected object.

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

                GUISelectables._GUISelectables.DeselectAll();
            }
            else
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(_MainCam.ScreenPointToRay(pos), out hitInfo, 10, 1 << GameGlobals.ARObjectLayer))
                {
                    // Select tapped object.

                    GameObject tapped = hitInfo.collider.transform.GetComponent<ObjectRoot>()._ObjectRoot.gameObject;
                    m_selectedPrefab = tapped.GetComponent<ARSelectable>();
                    m_selectedPrefab.MakeSelected();
                }
            }



        }
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
    /// Wait for the next depth update, then find the plane at the touch position.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    /// <param name="touchPosition">Touch position to find a plane at.</param>
    private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition)
    {
        m_findPlaneWaitingForDepth = true;
        // Turn on the camera and wait for a single depth update.

        if (!m_tangoApplication.m_enable3DReconstruction)
            m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
        while (m_findPlaneWaitingForDepth)
        {
            Debug.Log(m_findPlaneWaitingForDepth);
            yield return null;
        }


        if(!m_tangoApplication.m_enable3DReconstruction)
            m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
        // Find the plane.
        Vector3 planeCenter;
        Plane plane;
        if (!m_pointCloud.FindPlane(_MainCam, touchPosition, out planeCenter, out plane))
        {
            yield break;
        }

        // Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
        Vector3 up = plane.normal;
        Vector3 forward;
        if (Vector3.Angle(plane.normal, _MainCam.transform.forward) < 175)
        {
            Vector3 right = Vector3.Cross(up, _MainCam.transform.forward).normalized;
            forward = Vector3.Cross(right, up).normalized;
        }
        else
        {
            // Normal is nearly parallel to camera look direction, the cross product would have too much
            // floating point error in it.
            forward = Vector3.Cross(up, _MainCam.transform.right);
        }

        InstantiateARObject(planeCenter, Quaternion.LookRotation(forward, up));

        m_selectedPrefab = null;
    }

    public Enums.SelectionType CurrentSelectionType() {
        if (!GUISelectables._GUISelectables.IsAnyToggled())
            return Enums.SelectionType.NONE;
        Debug.Log(GameGlobals.CurrentARSelectableIndex);
        int index = _arObjMap[GameGlobals.CurrentARSelectableIndex];
        return _ARObjects[index].GetComponent<ARSelectable>().GetSelectionType();
    }

    public ARSelectable InstantiateARObject(Vector3 pos, Quaternion rot) {
        int index = _arObjMap[GameGlobals.CurrentARSelectableIndex];

        ARSelectable newARO =
            Instantiate(_ARObjects[index], pos, rot) as ARSelectable;
        newARO.transform.SetParent(transform);
        if (newARO._Projectile)
        {
            newARO.transform.position =
                _MainCam.transform.position - (_MainCam.transform.up * newARO.transform.localScale.y);
            newARO.transform.GetComponent<Rigidbody>().velocity =
                (_MainCam.transform.forward * forwardVelocity) + (_MainCam.transform.up * forwardVelocity / 2);
            newARO.transform.GetComponent<Rigidbody>().angularVelocity = 
                new Vector3(GameGlobals.Rand.Next(0,45), GameGlobals.Rand.Next(0, 45), GameGlobals.Rand.Next(0, 45));
        }
        return newARO;
    }

}
