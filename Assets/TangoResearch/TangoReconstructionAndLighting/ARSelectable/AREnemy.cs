using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AIAgent))]
public class AREnemy : ARSelectable
{

    private GUIAREnemy _ui;

    public float _Size = 0.7f;
    public float _Speed = 1f;

    private Animator _anim;

    public override void Start()
    {
        base.Start();
        this._SelectableType = Enums.SelectionType.ENEMY;

        _ui = GUIProperties._Properties._PanelEnemy;

        _anim = GetComponent<Animator>();

        _anim.speed = _Speed;
    }

    public override void MakeSelected()
    {
        base.MakeSelected();
        GameGlobals.ChangeSelected(Enums.SelectionType.ENEMY);

        _ui.Reset();
        _ui._DestroyButton.onClick.AddListener(delegate { this.OnDestoyClick(); });
        //_ui._ButtonMove.onClick.AddListener(this.OnClickMove);

        _ui._Size.onValueChanged.AddListener(onValueChangedSize);
        _ui._Speed.onValueChanged.AddListener(onValueChangedSpeed);

        _ui._Size.value = _Size;
        _ui._Speed.value = _Speed;

    }

    private void onValueChangedSpeed(float value)
    {
        _Speed = _ui._Speed.maxValue - value + _ui._Speed.minValue;
        //_Speed = value;
        transform.GetComponent<AIAgent>()._Speed = _Speed;

        _anim.speed = value;
    }

    private void onValueChangedSize(float value)
    {
        _Size = value;
        transform.localScale = new Vector3(_Size, _Size, _Size);
    }
}
