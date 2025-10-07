#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Games.Coresdk.Unity.Editor
{
    public class SandboxLoginUI : MonoBehaviour
    {
        [SerializeField] GameObject loginPanel;
        [SerializeField] InputField accountInputField;
        [SerializeField] InputField passwordInputField;
        [SerializeField] Text tokenText;
        [SerializeField] Button loginButton;
        [SerializeField] Button closeButton;
        [SerializeField] Text registerLinkText;

        [SerializeField] GameObject selectionPanel;
        [SerializeField] Button selectionCloseButton;
        [SerializeField] Button selectionLoginButton;
        [SerializeField] Button selectionGuestButton;
        [SerializeField] Text selectionRegisterLinkText;


        private string game_id;

        public System.Action<string> ClickCallback { get; set; }

        private void Awake()
        {
            // Login Panel
            loginButton.onClick.AddListener(OnClickLoginButton);
            closeButton.onClick.AddListener(() => {
                GameObject.Destroy(this.gameObject);
            });

            SetRegisterTextLink(registerLinkText);

            // Selection Panel
            selectionCloseButton.onClick.AddListener(() => {
                GameObject.Destroy(this.gameObject);
            });
            selectionLoginButton.onClick.AddListener(() => {
                selectionPanel.SetActive(false);
                loginPanel.SetActive(true);
            });
            selectionGuestButton.onClick.AddListener(OnClickGuestButton);

            SetRegisterTextLink(selectionRegisterLinkText);
        }

        public void SetDeepLinkURL(string url)
        {
            var tokens = TokenCollection.Parse(url);
            this.game_id = tokens.GetValue("game_id");
        }

        private void SetRegisterTextLink(Text registerLinkText)
        {
            UnityAction<BaseEventData> click = new UnityAction<BaseEventData>(OnClickRegisterLink);
            EventTrigger.Entry eventEntry = new EventTrigger.Entry();
            eventEntry.eventID = EventTriggerType.PointerClick;
            eventEntry.callback.AddListener(click);
            var eventTrigger = registerLinkText.gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(eventEntry);
        }

        private void OnClickRegisterLink(BaseEventData p)
        {
            Application.OpenURL(Coresdk.GetLoginURL());
        }

        private void OnClickGuestButton()
        {
            EditorCoresdk.PostGuestLogin(_ =>
            {
                if (_.Exception != null)
                {
                    Debug.LogError(_.Exception.Message);
                    return;
                }

                if (_.Exception == null && _.Token != null)
                {
                    if (ClickCallback != null)
                        ClickCallback(_.Token);
                }
            });
        }

        private void OnClickLoginButton()
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

            StartLogin(account, password, game_id);
        }

        void StartLogin(string account, string password, string game_id)
        {
            EditorCoresdk.PostLogin(account, password, game_id, _ =>
            {
                if (_.Exception != null)
                {
                    tokenText.text = _.Exception.Message;
                    return;
                }

                if (_.Exception == null && _.Token != null)
                {
                    if (ClickCallback != null)
                        ClickCallback(_.Token);
                }
            });
        }
    }
}
#endif
