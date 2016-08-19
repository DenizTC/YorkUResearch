using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MainUI : MonoBehaviour {

    public string fileName;
    private SaveData _cubeData;

    public Button _ButtonLoad;
    public Button _ButtonSave;
    public Button _ButtonClear;
    public Button _ButtonCube;


    public Transform _PrefabCube;

    public Transform _Player;

    public Transform _ObjectContainer;

	void Start () {

        _cubeData = new SaveData(fileName);

        _ButtonSave.onClick.AddListener(onClickSave);
        _ButtonLoad.onClick.AddListener(onClickLoad);
        _ButtonClear.onClick.AddListener(onClickClear);
        _ButtonCube.onClick.AddListener(onClickCube);
	}

    private void onClickClear()
    {
        foreach (Transform item in _ObjectContainer)
        {
            Destroy(item.gameObject);
        }
    }

    private void onClickLoad() {
        onClickClear();

        //_cubeData = SaveData.Load(Application.streamingAssetsPath + "\\" + fileName + ".uml");
        _cubeData = SaveData.Load(Application.persistentDataPath + "/" + fileName + ".uml");

        int count = _cubeData.GetValue<int>("CubeCount");

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = _cubeData.GetValue<Vector3>("Position" + i);
            Transform obj = Instantiate(_PrefabCube, pos, Quaternion.identity) as Transform;
            obj.SetParent(_ObjectContainer);
        }

    }

    private void onClickSave() {

        _cubeData["CubeCount"] = _ObjectContainer.childCount;
        for(int i = 0; i < _ObjectContainer.childCount; i++)
        {
            _cubeData["Position" + i] = _ObjectContainer.GetChild(i).position;
        }

        _cubeData.Save(Application.persistentDataPath + "/" + fileName + ".uml");
    }

    private void onClickCube() {
        Transform obj = Instantiate(_PrefabCube, _Player.position + _Player.forward*0.5f, Quaternion.identity) as Transform;
        obj.SetParent(_ObjectContainer);
    }


}
