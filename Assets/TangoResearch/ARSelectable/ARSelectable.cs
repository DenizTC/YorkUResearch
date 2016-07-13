using UnityEngine;
using System.Collections;

public abstract class ARSelectable : MonoBehaviour {

    public Enums.SelectionType _SelectableType;
    public float _LifeTime = -1;
    public bool _Projectile = false;

    void Start() {
        if (_LifeTime > 0) {
            Destroy(this, _LifeTime);
        }
    }

    public virtual void MakeSelected() {
        if (!GameGlobals.PropertiesPanelOpen)
            GameGlobals.SetPropertiesOpen(true);

    }

    protected void OnDestoyClick()
    {
        GameGlobals.ChangeSelected(Enums.SelectionType.NONE);
        Destroy(gameObject);
    }

    /// <summary>
    /// The Tango time stamp when this objec is created
    /// 
    /// This field is used in the Area Learning example, the timestamp is save for the position adjustment when the
    /// loop closure happens.
    /// </summary>
    public float m_timestamp = -1.0f;

    /// <summary>
    /// The marker's transformation with respect to the device frame.
    /// </summary>
    public Matrix4x4 m_deviceTMarker = new Matrix4x4();

    public Renderer _Gizmo;

    private void Remove()
    {
        Destroy(gameObject);
    }

}
