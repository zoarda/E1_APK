using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Video;
using Naninovel;
public class ChapterPage : MonoBehaviour
{
    public GameObject C1, C2, C3, C4, C5;
    public Button C1_Return, C2_Return, C3_Return, C4_Return, C5_Return;
    public List<GameObject> Chapter1;
    public List<GameObject> Chapter2;
    public List<GameObject> Chapter3;
    public List<GameObject> Chapter4;
    public List<GameObject> Chapter5;

    void Awake()
    {
        InitChapterPage();
    }
    public void InitChapterPage()
    {
        StartNani startNani = StartNani.Instance;
        var sfx = Engine.GetService<IAudioManager>();
        bool isProcessing = false;

        // 初始化按钮绑定和返回按钮
        InitButtonChapter(Chapter1, C1, C1_Return, "C1_VB");
        InitButtonChapter(Chapter2, C2, C2_Return, "C2_VB");
        InitButtonChapter(Chapter3, C3, C3_Return, "C3_VB");
        InitButtonChapter(Chapter4, C4, C4_Return, "C4_VB");
        InitButtonChapter(Chapter5, C5, C5_Return, "C5_VB");

        // 初始化按钮绑定的方法
        void InitButtonChapter(
            List<GameObject> chapter,
            GameObject chapterPage,
            Button returnButton,
            string script
        )
        {
            foreach (GameObject i in chapter)
            {
                string Buttonlable = i.name;
                Button ChapterButton = i.GetComponent<Button>();

                // 绑定按钮点击事件
                ChapterButton.onClick.AddListener(async () =>
                {
                    if (isProcessing) return;
                    isProcessing = true;
                    startNani.CloseOption();
                    // NaniCommandManger.Instance.SpeedButtonClearSpawn();
                    Debug.Log($"{i.name}");
                    NaniCommandManger.Instance.SpeedButtonClearSpawn();
                    await startNani.PlayVideoAsync(script, Buttonlable);
                    await sfx.StopBgmAsync("GameStart", 0);
                    chapterPage.SetActive(false);
                    isProcessing = false;
                });
            }

            // 设置返回按钮
            SetReturnButton(returnButton, chapterPage);
        }

        void SetReturnButton(Button returnButton, GameObject chapterPage)
        {
            returnButton.onClick.AddListener(() =>
            {
                startNani.ChapterPage.SetActive(false);
                chapterPage.SetActive(false);
            });
        }
    }

    // 新增方法：更新按钮状态
    public async void UpdateChapterPage()
    {
        // 更新每一章节的按钮状态
        await UpdateButtonInteractable(Chapter1);
        await UpdateButtonInteractable(Chapter2);
        await UpdateButtonInteractable(Chapter3);
        await UpdateButtonInteractable(Chapter4);
        await UpdateButtonInteractable(Chapter5);
    }

    // 抽取独立方法更新按钮状态
    private async UniTask UpdateButtonInteractable(List<GameObject> chapter)
    {
        try
        {
            // 加载保存的数据
            // StartNani.SaveData saveData = await YamlLoader.LoadYaml<StartNani.SaveData>(Application.persistentDataPath + "/SaveData.yaml");
            ServerManager.SaveData saveData = await ServerManager.Instance.Load();
            foreach (GameObject obj in chapter)
            {
                if (obj.TryGetComponent<Button>(out Button chapterButton))
                {
                    bool isInteractable = saveData.scriptName.Contains(obj.name);
                    chapterButton.interactable = isInteractable;
                }
                else
                {
                    Debug.LogWarning($"GameObject {obj.name} does not have a Button component.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load YAML: {ex.Message}\n{ex.StackTrace}");

            // 如果加载失败，禁用所有按钮
            foreach (GameObject obj in chapter)
            {
                if (obj.TryGetComponent<Button>(out Button chapterButton))
                {
                    chapterButton.interactable = false;
                }
            }
        }
    }
}
