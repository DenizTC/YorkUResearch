using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Testing : MonoBehaviour {

    public Renderer A;
    public Renderer B;
    public Renderer C;
    public Renderer D;

    public Button SwitchA;
    public Button SwitchB;
    public Button SwitchC;
    public Button SwitchD;

    // Use this for initialization
    void Start () {
        SwitchA.onClick.AddListener(onAClick);
        SwitchB.onClick.AddListener(onBClick);
        SwitchC.onClick.AddListener(onCClick);
        SwitchD.onClick.AddListener(onDClick);
    }

    private void onAClick() {
        Color randC = new Color(Random.value, Random.value, Random.value);
        A.material.color = randC;
    }

    private void onBClick()
    {
        Color randC = new Color(Random.value, Random.value, Random.value);
        B.material.color = randC;
    }

    private void onCClick()
    {
        Color randC = new Color(Random.value, Random.value, Random.value);
        C.material.color = randC;
    }

    private void onDClick()
    {
        Color randC = new Color(Random.value, Random.value, Random.value);
        D.material.color = randC;
    }


}
