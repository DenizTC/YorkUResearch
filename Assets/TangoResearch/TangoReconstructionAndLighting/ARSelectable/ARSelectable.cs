using UnityEngine;
using System.Collections;

public abstract class ARSelectable : MonoBehaviour {

    //protected Enums.SelectionType _SelectableType;

    /// <summary>
    /// The time in seconds before the object is destroyed. 
    /// Objects with non positive values will not be destroyed.
    /// </summary>
    public float _LifeTime = -1;
    public bool _Projectile = false;

    public Sprite _Icon;
    public Renderer _Gizmo;


    public virtual void Start() {
        if (_LifeTime > 0) {
            Destroy(transform.gameObject, _LifeTime);
        }
    }

    public abstract Enums.SelectionType GetSelectionType();

    public virtual void MakeSelected() {
        if (!GameGlobals.PropertiesPanelOpen)
            GameGlobals.SetPropertiesOpen(true);

    }

    protected void OnDestoyClick()
    {
        GameGlobals.ChangeSelected(Enums.SelectionType.NONE);
        Destroy(gameObject);
    }

    protected void OnClickMove() {
        StartCoroutine(doMoving());
    }

    private void Remove()
    {
        Destroy(gameObject);
    }

    protected virtual IEnumerator doMoving() {
        MessageManager._MessageManager.PushMessage("Tap screen to finish.", 2f);

        GameGlobals.MovingObject = true;
        GameGlobals.SetPropertiesOpen(false);
        transform.SetParent(Camera.main.transform);
        if (transform.GetComponent<Rigidbody>() != null)
            transform.GetComponent<Rigidbody>().isKinematic = true;

        while (GameGlobals.MovingObject)
        {
            yield return null;
        }

        GameGlobals.SetPropertiesOpen(true);
        transform.SetParent(ARObjectManager._AROBJManager.transform);

        if (transform.GetComponent<Rigidbody>() != null)
            transform.GetComponent<Rigidbody>().isKinematic = false;
    }

}
