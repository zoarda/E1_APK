using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
public class GalleryPage : MonoBehaviour
{
    public GameObject BtnPage, GalleryButton;
    public Transform BtnPageParent, GalleryButtonParent;
    public Button Button_ALL, Button_C1, Button_C2, Button_C3, Button_C4, Button_C5;
    public Button SelectButton;
    public Sprite SelctSprite, Sprite, SelectPageButton, PageButton;

    public List<string> C1List, C2List, C3List, C4List, C5List;

    StartNani startNani;

    void Start()
    {
        var player = Engine.GetService<IScriptPlayer>();
        void Init()
        {
            SelectButton.image.sprite = SelctSprite;
            void SetPage(string[] galleryNames)
            {
                void SetGalleryPage(int page)
                {
                    foreach (Transform i in GalleryButtonParent.transform)
                        Destroy(i.gameObject);
                    var startIndex = (page - 1) * 6;
                    for (int i = startIndex; i < startIndex + 6; i++)
                    {
                        if (i >= galleryNames.Length) break;

                        GameObject go = Instantiate(GalleryButton, GalleryButtonParent);
                        var galleryButton = go.GetComponent<GalleryButton>();
                        var galleryName = galleryNames[i];
                        galleryButton.BtnGallery.onClick.AddListener(async () =>
                        {
                            await player.PreloadAndPlayAsync(galleryName);
                            startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
                            startNani.StartGamePage.SetActive(false);
                            startNani.GalleryPage.SetActive(false);
                            startNani.VideoImage.GetComponent<CanvasGroup>().alpha = 1;
                        });
                        galleryButton.ImaGallery.sprite = Resources.Load<Sprite>($"GalleryPage/{galleryName}");
                    }
                }

                foreach (Transform i in BtnPageParent.transform)
                    Destroy(i.gameObject);
                int pageCount = galleryNames.Length / 6 + 1;
                if (galleryNames.Length % 6 == 0)
                    pageCount--;
                // Button currentPageButton = null;
                Animator currentAnimator = null;
                for (int i = 0; i < pageCount; i++)
                {
                    GameObject go = Instantiate(BtnPage, BtnPageParent);
                    if (i == 0)
                    {
                        currentAnimator = go.GetComponent<Animator>();
                        currentAnimator.speed = 1;
                        currentAnimator.SetInteger("AnimationState", 0); // 初始按钮动画状态
                        // currentPageButton = go.GetComponent<Button>();
                        // currentPageButton.image.sprite = SelectPageButton;

                    }
                    else
                    {
                        Animator animator = go.GetComponent<Animator>();
                        animator.speed = 0;
                        animator.SetInteger("AnimationState", i % 3); // 根据索引设置不同动画状态
                        // go.GetComponent<Button>().image.sprite = PageButton;
                    }
                    var pageButton = go.GetComponent<PageButton>();
                    var page = i + 1;
                    pageButton.TxtPage.text = $"{page}";
                    pageButton.BtnPage.onClick.AddListener(() =>
                    {
                        if (currentAnimator != null)
                        {
                            // 停止当前按钮的动画
                            currentAnimator.SetInteger("AnimationState", -1); // 重置动画状态
                            currentAnimator.speed = 0;
                        }
                        currentAnimator = go.GetComponent<Animator>();
                        currentAnimator.speed = 1;
                        currentAnimator.SetInteger("AnimationState", page % 3); // 根据页面动态设置动画参数

                        // currentPageButton.image.sprite = PageButton;
                        // currentPageButton = go.GetComponent<Button>();
                        // currentPageButton.image.sprite = SelectPageButton;
                        SetGalleryPage(page);
                    });
                    // } SetGalleryPage(page));
                }
                SetGalleryPage(1);
            }
            void SetAllPage()
            {
                List<string> allList = new List<string>();
                allList.AddRange(C1List);
                allList.AddRange(C2List);
                allList.AddRange(C3List);
                allList.AddRange(C4List);
                allList.AddRange(C5List);
                SetPage(allList.ToArray());
            }
            void OnbuttonClick(Button button)
            {
                if (SelectButton != button)
                {
                    SelectButton.image.sprite = Sprite;
                }
                SelectButton = button;
                SelectButton.image.sprite = SelctSprite;
            }

            // Button_ALL.onClick.AddListener(SetAllPage);
            Button_ALL.onClick.AddListener(() =>
            {
                SetAllPage();
                OnbuttonClick(Button_ALL);
            });
            Button_C1.onClick.AddListener(() =>
            {
                SetPage(C1List.ToArray());
                OnbuttonClick(Button_C1);
            });
            Button_C2.onClick.AddListener(() =>
            {
                SetPage(C2List.ToArray());
                OnbuttonClick(Button_C2);
            });
            Button_C3.onClick.AddListener(() =>
            {
                SetPage(C3List.ToArray());
                OnbuttonClick(Button_C3);
            });
            Button_C4.onClick.AddListener(() =>
            {
                SetPage(C4List.ToArray());
                OnbuttonClick(Button_C4);
            });
            Button_C5.onClick.AddListener(() =>
            {
                SetPage(C5List.ToArray());
                OnbuttonClick(Button_C5);
            });
            // Button_C2.onClick.AddListener(() => SetPage(C2List.ToArray()));
            // Button_C3.onClick.AddListener(() => SetPage(C3List.ToArray()));
            // Button_C4.onClick.AddListener(() => SetPage(C4List.ToArray()));
            // Button_C5.onClick.AddListener(() => SetPage(C5List.ToArray()));

            SetAllPage();
        }

        Init();
    }
}
