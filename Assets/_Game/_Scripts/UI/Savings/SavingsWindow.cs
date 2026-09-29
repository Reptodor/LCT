using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SavingsWindow : MonoBehaviour
{
    private const string CatalogResource = "SavingsGoals";

    [SerializeField] private CanvasGroup _rootGroup;
    [SerializeField] private RectTransform _sheet;
    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TMP_Text _balanceLabel;
    [SerializeField] private TMP_Text _goalTitle;
    [SerializeField] private TMP_Text _amountLabel;
    [SerializeField] private Image _fill;
    [SerializeField] private Button _depositButton;
    [SerializeField] private TMP_Text _depositLabel;
    [SerializeField] private TMP_Text _status;
    [SerializeField] private RectTransform _goalContent;
    [SerializeField] private SavingsGoalButton _goalTemplate;
    [SerializeField] private SavingsGoalCatalog _catalog;

    [SerializeField] private float _openDuration = 0.38f;
    [SerializeField] private float _closeDuration = 0.22f;
    [SerializeField] private float _sheetStartScale = 0.86f;
    [SerializeField] private float _sheetStartOffsetY = -56f;
    [SerializeField] private float _fillDuration = 0.28f;

    private readonly List<SavingsGoalButton> _rows = new List<SavingsGoalButton>();
    private bool _bound;
    private bool _closing;
    private Vector2 _sheetRest;
    private string _goalId = "";
    private Coroutine _fillRoutine;

    public event Action BalanceChanged;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (_sheet != null)
        {
            _sheetRest = _sheet.anchoredPosition;
        }

        if (_catalog == null)
        {
            _catalog = Resources.Load<SavingsGoalCatalog>(CatalogResource);
        }

        Bind();
        SnapHidden();
    }

    private void OnDestroy()
    {
        if (_dimButton != null)
        {
            _dimButton.onClick.RemoveListener(Close);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Close);
        }

        if (_depositButton != null)
        {
            _depositButton.onClick.RemoveListener(OnPrimary);
        }
    }

    public void Open()
    {
        if (_closing)
        {
            return;
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Bind();
        RestoreSelection();
        RebuildGoals();
        Refresh(false);
        StopAllCoroutines();
        _fillRoutine = null;
        StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (MustStay() || _closing || !gameObject.activeInHierarchy)
        {
            return;
        }

        StopAllCoroutines();
        _fillRoutine = null;
        StartCoroutine(CloseRoutine());
    }

    private void Bind()
    {
        if (_bound)
        {
            return;
        }

        _bound = true;
        if (_dimButton != null)
        {
            _dimButton.onClick.AddListener(Close);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
        }

        if (_depositButton != null)
        {
            _depositButton.onClick.AddListener(OnPrimary);
        }
    }

    private void RestoreSelection()
    {
        _goalId = "";
        if (_catalog == null || _catalog.Count == 0)
        {
            return;
        }

        string savedId = GameSession.IsReady ? GameSession.State.savingsGoalId : "";
        if (_catalog.TryGet(savedId, out SavingsGoalCatalog.Goal saved))
        {
            _goalId = saved.Id;
        }
    }

    private void RebuildGoals()
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            if (_rows[i] != null)
            {
                Destroy(_rows[i].gameObject);
            }
        }

        _rows.Clear();
        if (_catalog == null || _goalTemplate == null || _goalContent == null)
        {
            return;
        }

        for (int i = 0; i < _catalog.Count; i++)
        {
            SavingsGoalCatalog.Goal goal = _catalog.Get(i);
            SavingsGoalButton row = Instantiate(_goalTemplate, _goalContent);
            row.gameObject.SetActive(true);
            row.name = "Goal_" + goal.Id;
            string id = goal.Id;
            if (row.Button != null)
            {
                row.Button.onClick.AddListener(() => Choose(id));
            }

            _rows.Add(row);
        }
    }

    private void Choose(string goalId)
    {
        if (string.IsNullOrEmpty(goalId) || goalId == _goalId || !CanSwitch())
        {
            return;
        }

        if (_catalog == null || !_catalog.TryGet(goalId, out SavingsGoalCatalog.Goal goal))
        {
            return;
        }

        if (GameSession.IsReady && SavingsBank.IsFilled(GameSession.State, goal.Id, goal.Target))
        {
            return;
        }

        _goalId = goalId;
        Refresh(true);
    }

    private void OnPrimary()
    {
        if (IsChoosing())
        {
            Confirm();
            return;
        }

        Deposit();
    }

    private void Confirm()
    {
        if (!GameSession.IsReady || _catalog == null || !_catalog.TryGet(_goalId, out SavingsGoalCatalog.Goal goal))
        {
            SetStatus("Выбери цель");
            Refresh(false);
            return;
        }

        if (SavingsBank.IsFilled(GameSession.State, goal.Id, goal.Target))
        {
            SetStatus("Эта цель уже собрана");
            return;
        }

        SavingsGoalCatalog.Goal current = default;
        bool hasCurrent = _catalog.TryGet(GameSession.State.savingsGoalId, out current);
        if (hasCurrent && !SavingsBank.CanSwitchGoal(GameSession.State, current.Target) && current.Id != goal.Id)
        {
            return;
        }

        SavingsBank.Select(GameSession.State, goal.Id);
        GameSession.Persist();
        Refresh(false);
        SetStatus("Цель выбрана");
        SetInput(true);
    }

    private void Deposit()
    {
        if (_catalog == null || !GameSession.IsReady || !_catalog.TryGet(_goalId, out SavingsGoalCatalog.Goal goal))
        {
            SetStatus("Цель не выбрана");
            return;
        }

        int paid = SavingsBank.Deposit(GameSession.State, goal.Id, goal.Target, goal.Deposit);
        if (paid <= 0)
        {
            Refresh(false);
            int saved = SavingsBank.Saved(GameSession.State, goal.Id);
            SetStatus(saved >= goal.Target ? "Цель уже собрана" : "Не хватает монет");
            return;
        }

        GameSession.Persist();
        Refresh(true);
        SetStatus("В копилку +" + paid);
        BalanceChanged?.Invoke();
    }

    private void Refresh(bool animateFill)
    {
        int coins = GameSession.IsReady ? GameSession.State.coins : 0;
        if (_balanceLabel != null)
        {
            _balanceLabel.text = "На счету: " + coins;
        }

        SavingsGoalCatalog.Goal goal = default;
        bool hasGoal = _catalog != null && _catalog.TryGet(_goalId, out goal);
        bool choosing = IsChoosing();
        bool canSwitch = CanSwitch();
        int saved = hasGoal && GameSession.IsReady ? SavingsBank.Saved(GameSession.State, goal.Id) : 0;
        int target = hasGoal ? goal.Target : 1;
        int room = Mathf.Max(0, target - saved);
        int charge = hasGoal ? Mathf.Min(goal.Deposit, room) : 0;
        bool complete = hasGoal && room <= 0;
        bool canPay = !choosing && hasGoal && !complete && coins >= charge && charge > 0;
        bool canConfirm = choosing && hasGoal && !complete;

        if (_goalTitle != null)
        {
            _goalTitle.text = hasGoal ? goal.Title : "Выбери цель";
        }

        if (_amountLabel != null)
        {
            _amountLabel.text = hasGoal ? saved + " / " + target : "0 / 0";
        }

        float fraction = hasGoal ? Mathf.Clamp01(saved / (float)target) : 0f;
        if (_fill != null)
        {
            if (animateFill && gameObject.activeInHierarchy)
            {
                if (_fillRoutine != null)
                {
                    StopCoroutine(_fillRoutine);
                }

                _fillRoutine = StartCoroutine(AnimateFill(fraction));
            }
            else
            {
                _fill.fillAmount = fraction;
            }
        }

        if (_depositLabel != null)
        {
            if (choosing)
            {
                _depositLabel.text = "Подтвердить";
            }
            else if (!hasGoal || complete)
            {
                _depositLabel.text = "Цель собрана";
            }
            else
            {
                _depositLabel.text = "Пополнить · " + charge;
            }
        }

        if (_depositButton != null)
        {
            _depositButton.interactable = canPay || canConfirm;
        }

        if (MustStay() || choosing)
        {
            SetStatus(canConfirm ? "Нажми «Подтвердить»" : "Выбери цель и нажми «Подтвердить»");
        }
        else if (complete)
        {
            SetStatus("Цель собрана. Выбери следующую");
        }
        else
        {
            SetStatus("Новую цель можно выбрать после этой");
        }

        for (int i = 0; i < _rows.Count; i++)
        {
            if (_rows[i] == null || _catalog == null || i >= _catalog.Count)
            {
                continue;
            }

            SavingsGoalCatalog.Goal rowGoal = _catalog.Get(i);
            int rowSaved = GameSession.IsReady ? SavingsBank.Saved(GameSession.State, rowGoal.Id) : 0;
            bool rowFilled = GameSession.IsReady && SavingsBank.IsFilled(GameSession.State, rowGoal.Id, rowGoal.Target);
            _rows[i].Set(rowGoal.Title, rowSaved + " / " + rowGoal.Target, rowGoal.Id == _goalId);
            if (_rows[i].Button != null)
            {
                _rows[i].Button.interactable = canSwitch && !rowFilled;
            }
        }
    }

    private bool MustStay()
    {
        if (!GameSession.IsReady || SavingsBank.HasConfirmedGoal(GameSession.State))
        {
            return false;
        }

        return _catalog != null && _catalog.Count > 0;
    }

    private bool CanSwitch()
    {
        if (!GameSession.IsReady || _catalog == null)
        {
            return false;
        }

        if (!SavingsBank.HasConfirmedGoal(GameSession.State))
        {
            return true;
        }

        if (!_catalog.TryGet(GameSession.State.savingsGoalId, out SavingsGoalCatalog.Goal current))
        {
            return true;
        }

        return SavingsBank.CanSwitchGoal(GameSession.State, current.Target);
    }

    private bool IsChoosing()
    {
        if (!GameSession.IsReady || _catalog == null)
        {
            return false;
        }

        if (!SavingsBank.HasConfirmedGoal(GameSession.State))
        {
            return true;
        }

        if (!_catalog.TryGet(GameSession.State.savingsGoalId, out SavingsGoalCatalog.Goal current))
        {
            return true;
        }

        if (!SavingsBank.IsFilled(GameSession.State, current.Id, current.Target))
        {
            return false;
        }

        return !string.IsNullOrEmpty(_goalId) && _goalId != current.Id;
    }

    private void SetStatus(string text)
    {
        if (_status != null)
        {
            _status.text = text;
        }
    }

    private IEnumerator OpenRoutine()
    {
        IsOpen = true;
        _closing = false;
        SetInput(false);
        SnapHidden();
        float length = Mathf.Max(0.05f, _openDuration);
        float time = 0f;
        while (time < length)
        {
            time += Time.unscaledDeltaTime;
            ApplySheet(Mathf.Clamp01(time / length));
            yield return null;
        }

        ApplySheet(1f);
        SetInput(true);
        Refresh(false);
    }

    private IEnumerator CloseRoutine()
    {
        IsOpen = false;
        _closing = true;
        SetInput(false);
        float fromAlpha = _rootGroup != null ? _rootGroup.alpha : 1f;
        float fromScale = _sheet != null ? _sheet.localScale.x : 1f;
        Vector2 fromPos = _sheet != null ? _sheet.anchoredPosition : Vector2.zero;
        float length = Mathf.Max(0.05f, _closeDuration);
        float time = 0f;
        while (time < length)
        {
            time += Time.unscaledDeltaTime;
            float k = LevelSelectEase.InCubic(Mathf.Clamp01(time / length));
            if (_rootGroup != null)
            {
                _rootGroup.alpha = Mathf.Lerp(fromAlpha, 0f, k);
            }

            if (_sheet != null)
            {
                float scale = Mathf.Lerp(fromScale, _sheetStartScale, k);
                _sheet.localScale = new Vector3(scale, scale, 1f);
                _sheet.anchoredPosition = Vector2.Lerp(fromPos, _sheetRest + new Vector2(0f, _sheetStartOffsetY), k);
            }

            yield return null;
        }

        SnapHidden();
        _closing = false;
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateFill(float target)
    {
        float from = _fill != null ? _fill.fillAmount : 0f;
        float length = Mathf.Max(0.05f, _fillDuration);
        float time = 0f;
        while (time < length)
        {
            time += Time.unscaledDeltaTime;
            float k = LevelSelectEase.OutCubic(Mathf.Clamp01(time / length));
            if (_fill != null)
            {
                _fill.fillAmount = Mathf.Lerp(from, target, k);
            }

            yield return null;
        }

        if (_fill != null)
        {
            _fill.fillAmount = target;
        }

        _fillRoutine = null;
    }

    private void ApplySheet(float linear)
    {
        float scaleEase = LevelSelectEase.OutBack(linear);
        float fade = LevelSelectEase.OutCubic(linear);
        if (_rootGroup != null)
        {
            _rootGroup.alpha = fade;
        }

        if (_sheet == null)
        {
            return;
        }

        float scale = Mathf.LerpUnclamped(_sheetStartScale, 1f, scaleEase);
        _sheet.localScale = new Vector3(scale, scale, 1f);
        float y = Mathf.LerpUnclamped(_sheetStartOffsetY, 0f, scaleEase);
        _sheet.anchoredPosition = _sheetRest + new Vector2(0f, y);
    }

    private void SnapHidden()
    {
        if (_rootGroup != null)
        {
            _rootGroup.alpha = 0f;
            _rootGroup.blocksRaycasts = true;
            _rootGroup.interactable = true;
        }

        if (_sheet != null)
        {
            _sheet.localScale = new Vector3(_sheetStartScale, _sheetStartScale, 1f);
            _sheet.anchoredPosition = _sheetRest + new Vector2(0f, _sheetStartOffsetY);
        }

        if (_fill != null)
        {
            _fill.fillAmount = 0f;
        }
    }

    private void SetInput(bool enabled)
    {
        bool canLeave = enabled && !MustStay();
        if (_dimButton != null)
        {
            _dimButton.interactable = canLeave;
        }

        if (_closeButton != null)
        {
            _closeButton.interactable = canLeave;
        }

        if (!enabled && _depositButton != null)
        {
            _depositButton.interactable = false;
        }
    }
}
