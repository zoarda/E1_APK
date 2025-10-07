#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Games.Coresdk.Unity.Editor
{
    public class SandboxBindUI : MonoBehaviour
    {
        [SerializeField] InputField accountInputField;
        [SerializeField] InputField passwordInputField;
        [SerializeField] Button bindButton;
        [SerializeField] Button closeButton;
        [SerializeField] Text registerLinkText;

        public System.Action<AccountLoginBindGameResult> ClickCallback { get; set; }

        private string game_id;
        private string game_account;

        private void Awake()
        {
            bindButton.onClick.AddListener(OnClickBindButton);
            closeButton.onClick.AddListener(() => {
                GameObject.Destroy(this.gameObject);
            });

            UnityAction<BaseEventData> click = new UnityAction<BaseEventData>(OnClickRegisterLink);
            EventTrigger.Entry eventEntry = new EventTrigger.Entry();
            eventEntry.eventID = EventTriggerType.PointerClick;
            eventEntry.callback.AddListener(click);
            var eventTrigger = registerLinkText.gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(eventEntry);
        }

        public void SetDeepLinkURL(string url)
        {
            var tokens = TokenCollection.Parse(url);
            this.game_id = tokens.GetValue("game_id");
            this.game_account = tokens.GetValue("game_account");
        }

        private void OnClickRegisterLink(BaseEventData p)
        {
            Application.OpenURL(Coresdk.GetLoginURL());
        }

        private void OnClickBindButton()
        {
            var account = accountInputField.text;
            var password = passwordInputField.text;

            var isError = false;
            if (string.IsNullOrEmpty(account))
            {
                accountInputField.text = "";
                accountInputField.placeholder.GetComponent<Text>().text = "<color=red>信箱未填寫</color>";
                isError = true;
            }

            if (string.IsNullOrEmpty(password))
            {
                passwordInputField.text = "";
                passwordInputField.placeholder.GetComponent<Text>().text = "<color=red>請輸入密碼</color>";
                isError = true;
            }

            if (isError)
                return;

            StartBind(account, password);
        }

        void StartBind(string account, string password)
        {
            EditorCoresdk.PostAccountBindGame(account, password, game_id, game_account, _ =>
            {
                if (ClickCallback != null)
                    ClickCallback(_);
            });
        }
    }
}
#endif
