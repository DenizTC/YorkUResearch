using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;

[RequireComponent(typeof(Button))]
public class ButtonADF : MonoBehaviour {

    public Text _TextADFName;
    public Text _TextADFMeshed;
    private string _ADFUUID;


    private void Start() {
        GetComponent<RectTransform>().localScale = Vector3.one;
        GetComponent<Button>().onClick.AddListener(onClickADF);
    }

    public void Init(string ADFUUID, string name, bool isMeshed) {
        _ADFUUID = ADFUUID;
        _TextADFName.text = name;
        _TextADFMeshed.enabled = isMeshed;
    }


    private void onClickADF() {
        GUIAreaDescription._GUIAreaDescription.gameObject.SetActive(false);
        AreaDescriptionManager._ADM.UseADF(_ADFUUID);
    }

}
