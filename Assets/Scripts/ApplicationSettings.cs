using UnityEngine;
using Singleton;

public class ApplicationSettings : Singleton<ApplicationSettings>
{
    [SerializeField, Header("Target frame rate")]
    private int frameRate;

    [SerializeField, Header("vSync")]
    private int vSync;

    [SerializeField, Header("Cursor lock mode")]
    private CursorLockMode cursorLockMode;
    [SerializeField]
    private bool isVisibleCursor;
    
    protected override void OnAwake()
    {
        Application.targetFrameRate = frameRate;
        QualitySettings.vSyncCount = vSync;

        Cursor.lockState = cursorLockMode;
        Cursor.visible = isVisibleCursor;
    }
}