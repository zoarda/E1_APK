using UnityEngine;
using Naninovel;
using System.Linq;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    static Vector2 defaultCursorHotspot = new Vector2(6.24f, 6.82f);
    static Vector2 customCursorHotspot = new Vector2(6.72f, 53.94f);

    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D customCursor;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.Auto);
    }
    void Update()
    {
        // if(Input.GetMouseButtonDown(0))
        // {
        //     ChangeToCustomCursor();
        // }
        // if(Input.GetMouseButtonUp(0))
        // {
        //     ChangeToDefaultCursor();
        // }
    }
    public void ChangeToCustomCursor()
    {
        Cursor.SetCursor(customCursor, customCursorHotspot, CursorMode.Auto);
    }

    public void ChangeToDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.Auto);
    }
    // public async void SpawnLine(GameObject obj)
    // {
    //     var PreLine = Resources.Load<GameObject>("Prefabs/LinePage");
    //     GameObject Line = Instantiate(PreLine, obj.transform);
    //     // ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
    //     // var spawneObjects = Engine.GetService<ISpawnManager>().GetAllSpawned();
    //     // var LinePage = spawneObjects.FirstOrDefault(i => i.Path == "LinePage");
    //     // if (LinePage == null)
    //     //     await spawnManager.SpawnAsync("LinePage");
    // }
}
