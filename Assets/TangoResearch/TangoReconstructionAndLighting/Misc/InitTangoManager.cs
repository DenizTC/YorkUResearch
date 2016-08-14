using UnityEngine;
using System.Collections;
using Tango;

[RequireComponent(typeof(TangoApplication))]
public class InitTangoManager : MonoBehaviour {

    void Awake() {
        GetComponent<TangoApplication>().m_3drSpaceClearing = SceneLoader.SpaceClearing3DR;
    }
}
