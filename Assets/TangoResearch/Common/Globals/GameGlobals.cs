public static class GameGlobals {

    public static System.Random Rand = new System.Random();

    public static int WaypointLayer = 9;

    public static int WalkableLayer = 10;

    //public static bool SpaceClearing3DR = false;

    public static Enums.SelectionType CurrentDrawingSelection = Enums.SelectionType.NONE;

    public static Enums.SelectionType CurrentSelection = Enums.SelectionType.NONE;

    public static bool DrawWireframe = true;

    public static bool EnvironmentShadows = false;

    public static bool PropertiesPanelOpen = false;

    public static bool SelectablesPanelOpen = true;

    public static bool IsAimingSun = false;

    public static bool CanvasEnabled = true;

    private static GUIProperties _properties = GUIProperties._Properties;
    private static GUISelectables _selectables = GUISelectables._GUISelectables;

    public static void SetPropertiesOpen(bool value) {
        PropertiesPanelOpen = value;
        _properties.gameObject.SetActive(value);

        SelectablesPanelOpen = !PropertiesPanelOpen;
        _selectables.gameObject.SetActive(SelectablesPanelOpen);
    }

    public static void ChangeSelected(Enums.SelectionType selection) {
        if(selection != Enums.SelectionType.DIRECTIONAL_LIGHT ||
            selection != Enums.SelectionType.AMBIENT_LIGHT)
            CurrentSelection = selection;

        switch (selection)
        {
            case Enums.SelectionType.POINT_LIGHT:
                _properties._PanelARAmbientLight.gameObject.SetActive(false);
                _properties._PanelARDirectionalLight.gameObject.SetActive(false);
                _properties._PanelARPointLight.gameObject.SetActive(true);
                _properties._PanelProp.gameObject.SetActive(false);
                break;
            case Enums.SelectionType.DIRECTIONAL_LIGHT:
                _properties._PanelARAmbientLight.gameObject.SetActive(false);
                _properties._PanelARDirectionalLight.gameObject.SetActive(true);
                _properties._PanelARPointLight.gameObject.SetActive(false);
                _properties._PanelProp.gameObject.SetActive(false);
                break;
            case Enums.SelectionType.AMBIENT_LIGHT:
                _properties._PanelARAmbientLight.gameObject.SetActive(true);
                _properties._PanelARDirectionalLight.gameObject.SetActive(false);
                _properties._PanelARPointLight.gameObject.SetActive(false);
                _properties._PanelProp.gameObject.SetActive(false);
                break;
            case Enums.SelectionType.PROP:
                _properties._PanelARAmbientLight.gameObject.SetActive(false);
                _properties._PanelARDirectionalLight.gameObject.SetActive(false);
                _properties._PanelARPointLight.gameObject.SetActive(false);
                _properties._PanelProp.gameObject.SetActive(true);
                break;
            default:
                SetPropertiesOpen(false);
                break;
        }

    }

    public static void ChangeDrawingSelection(Enums.SelectionType selection)
    {
        CurrentDrawingSelection = selection;
    }

}
