﻿using UnityEngine;
using System.Collections;

public class Enums : MonoBehaviour {

    public enum SelectionType { NONE, POINT_LIGHT, DIRECTIONAL_LIGHT, PROP, AMBIENT_LIGHT }

    public enum DepthFilter { NONE, KUWAHARA, GUIDEDFILTER, GAUSSIAN, MEDIAN, MASKEDMEAN }

    public enum FillHoleMode { NOFILL, SIMPLE, MEAN };

    public enum DepthMapMode { FULL, MASKED };

}
