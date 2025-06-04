using System.Collections;
using System.Collections.Generic;
using Naninovel;
using UnityEngine;

public class GalleryPageOpen : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await Open(asyncToken);
    }
    public static async UniTask Open(AsyncToken asyncToken)
    {
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        startNani.GalleryPage.SetActive(true);
        await UniTask.CompletedTask;
    }
}
