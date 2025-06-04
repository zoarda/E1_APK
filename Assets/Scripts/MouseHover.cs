using Unity.VisualScripting;
using Naninovel;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool a = false;
    public void OnPointerEnter(PointerEventData eventData)
    {
        InputManager.Instance.ChangeToCustomCursor();
        // if (a)
        // {
        //     InputManager inputManager = InputManager.Instance;
        //     inputManager.SpawnLine(gameObject);
        // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InputManager.Instance.ChangeToDefaultCursor();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        InputManager.Instance.ChangeToDefaultCursor();
        AudioManager.Instance.PlaySFX("Click");
    }
}
