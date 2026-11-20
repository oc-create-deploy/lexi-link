using System;
using TMPro;
using UnityEngine;



public class LetterInput : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputTextComp;
    [SerializeField] private RectTransform _panelRectTransform;

    private string _inputText;
    // Optional accessible input: tap distinct letters, then explicitly submit.
    // The existing continuous drag path remains the default.
    private bool tapMode;
    private void OnGUI()
    {
        if(!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
        float scale=Screen.width/430f;
        var style=new GUIStyle(GUI.skin.button);style.fontSize=Mathf.RoundToInt(16*scale);
        float top=Screen.height-Screen.safeArea.yMax+8*scale;
        if(GUI.Button(new Rect(12*scale,top,190*scale,38*scale),tapMode?"INPUT: TAP LETTERS":"INPUT: DRAG LETTERS",style))
        { tapMode=!tapMode;Reset();OnWordEnterEnd?.Invoke(); }
        if(tapMode)
        {
            if(GUI.Button(new Rect(218*scale,top,98*scale,38*scale),"SUBMIT",style)) SubmitWord();
            if(GUI.Button(new Rect(322*scale,top,96*scale,38*scale),"CLEAR",style)) {Reset();OnWordEnterEnd?.Invoke();}
        }
    }
    private void SubmitWord()
    {
        if(string.IsNullOrEmpty(_inputText)) return;
        OnWordEnterEndWord?.Invoke(_inputText);
        OnWordEnterEnd?.Invoke();
    }

    

    public static Action<string> OnWordEnterEndWord;
    public static Action OnWordEnterEnd;

    [SerializeField] private float _pitchMultiplierStep=0.1f;
    private float _pitchMultiplier = 1f;
    

    private void Start()
    {
        Reset();
    }

    void OnEnable()
    {
        LetterButton.OnLetterButtonPressed += AddChar;
        SquareManager.OnWrongWordEntered += ShowWrongWordMessage;
        SquareManager.OnExistingWordEntered += ShowExistingWordMessage;
        SquareManager.OnCorrectWordEntered += Reset;
        ShutterPanel.OnHintCanceled += Reset;
        CanvasEventBus.OnGameLoaded += Reset;

        HintButton.OnHintLetterUsed += ShowLetterHintMessage;
        Square.OnSquareClickedAfterHintUsed += Reset;
    }

    void OnDisable()
    {
        LetterButton.OnLetterButtonPressed -= AddChar;
        SquareManager.OnWrongWordEntered -= ShowWrongWordMessage;
        SquareManager.OnExistingWordEntered -= ShowExistingWordMessage;
        SquareManager.OnCorrectWordEntered -= Reset;
        ShutterPanel.OnHintCanceled -= Reset;
        CanvasEventBus.OnGameLoaded -= Reset;

        HintButton.OnHintLetterUsed -= ShowLetterHintMessage;
        Square.OnSquareClickedAfterHintUsed -= Reset;
    }

    public void AddChar(char c)
    {
        _inputText = _inputText + c;

        _inputTextComp.text = _inputText;

        SoundManager.Instance.Play(SoundManager.SoundInfoName.letterButtonClicked, _pitchMultiplier);
        _pitchMultiplier += _pitchMultiplierStep;

        if (_inputText.Length == 1)
        {
            _panelRectTransform.sizeDelta = new Vector2(110f, _panelRectTransform.sizeDelta.y);
        }
        else
        {
            _panelRectTransform.sizeDelta = new Vector2(110f + _inputTextComp.textBounds.size.x, _panelRectTransform.sizeDelta.y);
        }
    }


    public void Reset()
    {
        _inputText = "";
        _inputTextComp.text = _inputText;
        _pitchMultiplier = 1f;
        
        _panelRectTransform.sizeDelta = new Vector2(0f, _panelRectTransform.sizeDelta.y);
    }

    private void ShowWrongWordMessage(string word)
    {
        if (word.Length < 3)
        {
            Reset();
            return;
        }
        _inputText = "";
        _pitchMultiplier = 1f;
        
        _inputTextComp.text = $"“{word}” is not in this puzzle";
        _inputTextComp.ForceMeshUpdate();
        _panelRectTransform.sizeDelta = new Vector2(110f + _inputTextComp.textBounds.size.x, _panelRectTransform.sizeDelta.y);

    }

    private void ShowExistingWordMessage(string word)
    {
        _inputText = "";
        _pitchMultiplier = 1f;
         _inputTextComp.text = $"“{word}” is already found";
         _inputTextComp.ForceMeshUpdate();
        _panelRectTransform.sizeDelta = new Vector2(110f + _inputTextComp.textBounds.size.x, _panelRectTransform.sizeDelta.y);
    }

    private void ShowLetterHintMessage()
    {
        _inputText = "";
        _pitchMultiplier = 1f;

        _inputTextComp.text = $"Choose a letter to reveal";
        _inputTextComp.ForceMeshUpdate();
        _panelRectTransform.sizeDelta = new Vector2(110f + _inputTextComp.textBounds.size.x, _panelRectTransform.sizeDelta.y);
    }


    private void Update()
    {
        if (!tapMode && Input.GetMouseButtonUp(0) && _inputText != "")
        {
            // check if word is valid then reset letter buttons
            OnWordEnterEndWord?.Invoke(_inputText);
            OnWordEnterEnd?.Invoke();
        }

    }
}
