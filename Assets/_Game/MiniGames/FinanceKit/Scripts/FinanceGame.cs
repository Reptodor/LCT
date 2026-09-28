using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LCT.MiniGames.Finance
{
    public abstract class FinanceGame : MonoBehaviour
    {
        public const string TopicBudget = "Планирование бюджета";
        public const string TopicSavings = "Формирование сбережений";

        [SerializeField] protected FinanceTheme theme;

        protected Canvas RootCanvas { get; private set; }
        protected RectTransform Safe { get; private set; }
        protected RectTransform GameLayer { get; private set; }
        protected RectTransform DragLayer { get; private set; }
        protected RectTransform FxLayer { get; private set; }
        protected FinanceScreens Screens { get; private set; }

        protected virtual void Awake()
        {
            if (theme == null)
                theme = GetComponent<FinanceTheme>();
            FinanceUi.Theme = theme;

            EnsureEventSystem();
            BuildCanvas();
        }

        protected virtual void Start()
        {
            BuildGame();
            ShowIntro();
        }

        protected abstract void BuildGame();

        protected abstract void ShowIntro();

        protected ResultButton NextButton(System.Action onClick)
        {
            return new ResultButton
            {
                Sprite = theme != null ? theme.nextButton : null,
                Text = "ДАЛЕЕ",
                Color = FinanceUi.Leaf,
                Size = new Vector2(220f, 220f),
                OnClick = onClick
            };
        }

        protected ResultButton RetryButton(System.Action onClick)
        {
            return new ResultButton
            {
                Sprite = theme != null ? theme.restartButton : null,
                Text = "ЗАНОВО",
                Color = FinanceUi.Sky,
                Size = new Vector2(400f, 200f),
                OnClick = onClick
            };
        }

        protected ResultButton ExitButton()
        {
            return new ResultButton
            {
                Sprite = theme != null ? theme.exitButton : null,
                Text = "ВЫХОД",
                Color = FinanceUi.Orange,
                Size = new Vector2(220f, 220f),
                OnClick = ExitGame
            };
        }

        protected static int StarsByTime(float seconds, float threeStars, float twoStars)
        {
            if (seconds <= threeStars)
                return 3;
            return seconds <= twoStars ? 2 : 1;
        }

        protected void Float(string text, Color color, Transform source, Vector2 offset = default, int size = 60)
        {
            if (source == null)
                return;
            Vector3 world = source.position + (Vector3)(offset * RootCanvas.transform.lossyScale.x);
            FinanceTween.FloatText(this, FxLayer, text, color, world, size);
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        void BuildCanvas()
        {
            var go = new GameObject("FinanceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = 5;
            RootCanvas = go.GetComponent<Canvas>();
            RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var root = (RectTransform)go.transform;
            FinanceUi.Box(root, "BackgroundColor", new Color(0.3f, 0.5f, 0.34f, 1f), 0f).rectTransform.Stretch();
            if (theme != null && theme.background != null)
            {
                var background = FinanceUi.Picture(root, "Background", theme.background, false);
                var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = theme.background.rect.width / theme.background.rect.height;
            }

            Safe = FinanceUi.Node(root, "SafeArea").Stretch();
            Safe.gameObject.AddComponent<SafeAreaFitter>();
            GameLayer = FinanceUi.Node(Safe, "Game").Stretch();
            DragLayer = FinanceUi.Node(Safe, "Drag").Stretch();
            var screens = FinanceUi.Node(Safe, "Screens").Stretch();
            FxLayer = FinanceUi.Node(Safe, "Fx").Stretch();
            Screens = new FinanceScreens(this, screens, FxLayer, theme);
        }
    }
}
