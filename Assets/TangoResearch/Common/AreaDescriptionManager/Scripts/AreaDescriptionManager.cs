using System.Collections;
using UnityEngine;
using Tango;
using System.Threading;
using UnityEngine.SceneManagement;

public class AreaDescriptionManager : MonoBehaviour, ITangoLifecycle, ITangoEvent
{
    private TangoApplication m_tangoApplication;

    public static AreaDescriptionManager _ADM;

    private Thread _saveADFThread;

    public void Start()
    {

        if (!_ADM)
            _ADM = this;

        m_tangoApplication = FindObjectOfType<TangoApplication>();
        if (m_tangoApplication != null)
        {
            m_tangoApplication.Register(this);
            m_tangoApplication.RequestPermissions();
        }

    }

    /// <summary>
    /// Populates the ADF selection list with all the ADFs found on the device.
    /// </summary>
    private void populateADFList() {
        AreaDescription[] list = AreaDescription.GetList();

        foreach (AreaDescription areaDescription in list)
        {
            AreaDescription.Metadata md = areaDescription.GetMetadata();
            bool isMeshed = System.IO.File.Exists("/sdcard/" + md.m_name + ".obj");
            GUIAreaDescription._GUIAreaDescription.AddADF(areaDescription.m_uuid, md.m_name, isMeshed);
        }
    }

    public void UseADF(string UUID) {
        AreaDescription ad = AreaDescription.ForUUID(UUID);
        GameGlobals.ActiveAreaDescription = ad.GetMetadata().m_name;
        //m_tangoApplication.Set3DReconstructionEnabled(true);
        m_tangoApplication.Startup(ad);
    }

    /// <summary>
    /// Start creation of new ADF.
    /// </summary>
    public void NewADF() {
        m_tangoApplication.Set3DReconstructionEnabled(false);
        m_tangoApplication.m_enableAreaLearning = true;
        m_tangoApplication.m_areaDescriptionLearningMode = true;
        m_tangoApplication.Startup(null);
    }

    /// <summary>
    /// Save the current ADF, and reload the scene.
    /// </summary>
    public void SaveADF() {

#if UNITY_EDITOR
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
#else
        StartCoroutine(saveCurrentAreaDescription());
#endif        
    }

    private IEnumerator saveCurrentAreaDescription()
    {

        if (TouchScreenKeyboard.visible || _saveADFThread != null)
        {
            yield break;
        }
        
        TouchScreenKeyboard kb = TouchScreenKeyboard.Open("Unnamed");
        while (!kb.done && !kb.wasCanceled)
        {
            yield return null;
        }
        bool saveConfirmed = kb.done;

        if (saveConfirmed)
        {
            if (m_tangoApplication.m_areaDescriptionLearningMode)
            {
                _saveADFThread = new Thread(delegate ()
                {
                    // Start saving process in another thread.
                    AreaDescription curAreaDescription = AreaDescription.SaveCurrent();
                    AreaDescription.Metadata metadata = curAreaDescription.GetMetadata();
                    metadata.m_name = kb.text;
                    curAreaDescription.SaveMetadata(metadata);
                });
                _saveADFThread.Start();
            }
            else
            {
                SceneManager.LoadScene(2, LoadSceneMode.Single);
            }

        }

        while (_saveADFThread != null && _saveADFThread.ThreadState != ThreadState.Running)
        {
            yield return null;
        }

        // Reload the scene to redisplay the ADF selection list with the newly saved ADF.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);

    }

    /// <summary>
    /// This is called each time a Tango event happens.
    /// </summary>
    public void OnTangoEventAvailableEventHandler(Tango.TangoEvent tangoEvent)
    {
        // We will not have the saving progress when the learning mode is off.
        if (!m_tangoApplication.m_areaDescriptionLearningMode)
        {
            return;
        }

        if (tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_AREA_LEARNING
            && tangoEvent.event_key == "AreaDescriptionSaveProgress")
        {
            string progress = "Saving ADF. " + (float.Parse(tangoEvent.event_value) * 100) + "%";
            MessageManager._MessageManager.PushMessage(progress, 0.5f);
        }
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        if (permissionsGranted)
        {

            //GUIAreaDescription._GUIAreaDescription.AddADF("uuid", "adfDummy", false);

#if UNITY_ANDROID && !UNITY_EDITOR
            populateADFList();
#endif
        }
    }

    public void OnTangoServiceConnected()
    {
    }

    public void OnTangoServiceDisconnected()
    {
    }
}