using UnityEngine;
using UnityEngine.UI;

public class GUIAreaDescription : MonoBehaviour {

    // UI hierachy: this -> _PanelADFContainer -> _PanelADFList -> _ButtonADF

    public RectTransform _PanelADFContainer; // Container of the ADF button list
    public RectTransform _PanelADFList; // Parent for all the ADF buttons
    public ButtonADF _ButtonADF;

    public Button _ButtonNewADF;
    public Button _ButtonSaveADF;

    public Material _ARScreen;

    public static GUIAreaDescription _GUIAreaDescription;

	void Start () {
        if (!_GUIAreaDescription)
            _GUIAreaDescription = this;

        _ButtonNewADF.onClick.AddListener(onClickNewADF);
        _ButtonSaveADF.onClick.AddListener(onClickSaveADF);

    }

    private void onClickSaveADF()
    {
        GetComponent<Image>().material = null;
        AreaDescriptionManager._ADM.SaveADF();
    }

    public void AddADF(string ADFUUID, string name, bool isMeshed) {
        ButtonADF bADF = Instantiate(_ButtonADF) as ButtonADF;
        bADF.Init(ADFUUID, name, isMeshed);
        bADF.transform.parent = _PanelADFList;
    }

    private void onClickNewADF() {
        GetComponent<Image>().material = _ARScreen;
        _PanelADFContainer.gameObject.SetActive(false);
        AreaDescriptionManager._ADM.NewADF();
    }

}
