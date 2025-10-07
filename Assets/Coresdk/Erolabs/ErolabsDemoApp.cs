using UnityEngine;

public class ErolabsDemoApp : MonoBehaviour
{
    void Start()
    {
        var original = Resources.Load<GameObject>("CoresdkDemo");
        var go = GameObject.Instantiate(original);
        go.AddComponent<ErolabsDemoScript>();
    }
}
