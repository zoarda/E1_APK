using UnityEngine;
using Naninovel;

public class AspectRatioControl : MonoBehaviour
{
    [SerializeField] internal bool isMainCamera = false;
    [SerializeField] bool debugMode,MainSceneUICamera;
    #region Pola
    private int ScreenSizeX = 0;
    private int ScreenSizeY = 0;
    #endregion

    #region metody

    #region rescale camera
    private void RescaleCamera()
    {

        if (Screen.width == ScreenSizeX && Screen.height == ScreenSizeY) return;
        
        float targetaspect = 16.0f / 9.0f;
        float windowaspect = (float)Screen.width / (float)Screen.height;
        float scaleheight = windowaspect / targetaspect;
        if(debugMode)
            Debug.Log("scaleheight:" + scaleheight);
        Camera camera = GetComponent<Camera>();

        if (scaleheight < 1.0f)
        {
            if (debugMode)
                Debug.Log("scaleheight < 1.0f");
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            camera.rect = rect;
        }
        else // add pillarbox
        {
            if (debugMode)
                Debug.Log("scaleheight > 1.0f");
            float scalewidth = 1.0f / scaleheight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }

        ScreenSizeX = Screen.width;
        ScreenSizeY = Screen.height;
    }
    #endregion

    #endregion
    #region metody unity
    private void Start()
    {

    }
    void OnPreCull()
    {
        // if (Application.isEditor) return;
        
        Camera camera;
        if((camera =Camera.main )!= null){
            camera = Camera.main;
        }
        else{
            camera = GameObject.Find("MainCamera").GetComponent<Camera>();
        }
        // Camera camera = Camera.main;
        Rect wp = camera.rect;
        Rect nr = new Rect(0, 0, 1, 1);

        camera.rect = nr;
        if (isMainCamera)
            GL.Clear(true, true, Color.black);

        camera.rect = wp;
    }
    void Update()
    {
        RescaleCamera();
    }
    #endregion
}
