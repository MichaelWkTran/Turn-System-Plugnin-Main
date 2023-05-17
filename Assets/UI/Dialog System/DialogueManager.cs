using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    static DialogueManager m_dialogueManager;

    public TextAsset m_inkJSON;
    Story m_story;

    [Serializable] public struct DialogEvent
    {
        public string m_dialog;
        public List<string> m_tags;
        public List<Choice> m_choises;
        public uint m_selectedChoise;
    };
    [SerializeField] List<DialogEvent> m_history = new List<DialogEvent>();
    [SerializeField] uint m_historyIndex = 0U;
    bool m_nearStoryEnd = false;

    [Header("UI")]
    [SerializeField] Button m_dialogPannel;
    [SerializeField] TMP_Text m_dialogText;
    [SerializeField] HorizontalLayoutGroup m_choisesGroup;
    [SerializeField] Button m_choiseButtonPrefab;
    [SerializeField] GameObject m_namePannel;
    [SerializeField] float m_dialogLerpTime;
    [SerializeField] float m_charactersPerSecond;
    IEnumerator m_currentTypeDialog = null;

    void Start()
    {
        //Ensure this class is a singeton
        if (m_dialogueManager == null) m_dialogueManager = this;
        else if (m_dialogueManager != this) { Destroy(m_dialogueManager); return; }

        Open();
    }

    public void Open()
    {
        //If no text file is given, disable the manager
        if (m_inkJSON == null) { Close(); return; }

        //Move the dialog on screen when it is opened
        GetComponent<RectTransform>().anchoredPosition = Vector3.down * Screen.height;
        LeanTween.move(GetComponent<RectTransform>(), Vector3.zero, m_dialogLerpTime).
            setEase(LeanTweenType.easeOutExpo).setIgnoreTimeScale(true);

        //Create New Story
        m_story = new Story(m_inkJSON.text);

        //Get errors or warnings from the story
        m_story.onError += (msg, type) =>
        {
            if (type == Ink.ErrorType.Warning) Debug.LogWarning(msg);
            else Debug.LogError(msg);
        };

        //Initialise dialog history
        m_history = new List<DialogEvent>();
        m_historyIndex = 0U;

        Continue();
    }

    public void Close()
    {
        //Move the dialog off screen when it is closed
        LeanTween.move(GetComponent<RectTransform>(), Vector3.down * Screen.height, m_dialogLerpTime).
            setEase(LeanTweenType.easeOutExpo).setIgnoreTimeScale(true);
    }

    public void Continue(int _choise = 0)
    {
        //Skip to full dialog when the typing animation is still running
        if (m_currentTypeDialog != null) { UpdateUI(false); return; }

        //Close the dialog UI when the dialog has finished
        if (!m_nearStoryEnd && !m_story.canContinue) { m_nearStoryEnd = true; }
        else if (m_nearStoryEnd && !m_story.canContinue && m_historyIndex <= 0) { Close(); return; }

        //Update the dialog that is being shown
        if (m_historyIndex > 0)
        {
            //If a previously viewed dialog is shown then move to a more recent dialog
            m_historyIndex--;
        }
        else
        {
            if (m_story.currentChoices.Count > 0) m_story.ChooseChoiceIndex(_choise);

            //Add newly read dialog to the dialog history
            DialogEvent newDialogEvent;
            newDialogEvent.m_dialog = m_story.Continue();
            newDialogEvent.m_tags = m_story.currentTags;
            newDialogEvent.m_choises = m_story.currentChoices;
            newDialogEvent.m_selectedChoise = (uint)_choise;
            m_history.Add(newDialogEvent);
        }

        //Update the UI to match the viewed dialog
        UpdateUI(!(m_historyIndex > 0));
    }

    public void Back()
    {
        //Prevent historyIndex from going outside the dialog history range
        if (m_historyIndex >= m_history.Count-1) return;
        m_historyIndex++;
        //m_nearStoryEnd = false;
        UpdateUI(false);
    }

    void UpdateUI(bool _typeDialog = true)
    {
        DialogEvent dialogEvent = m_history[(m_history.Count - 1) - (int)m_historyIndex];

        //Set the text in the name pannel
        try
        {
            m_namePannel.SetActive(true);
            m_namePannel.GetComponentInChildren<TMP_Text>().text = dialogEvent.m_tags.Find(name => name.Contains("name:")).Substring(5);
        }
        catch
        {
            m_namePannel.SetActive(false);
        }

        //Set the dialog text
        m_dialogPannel.interactable = true;
        if (m_currentTypeDialog != null) { StopCoroutine(m_currentTypeDialog); m_currentTypeDialog = null; }
        
        if (_typeDialog) StartCoroutine(m_currentTypeDialog = TypeDialog(dialogEvent));
        else m_dialogText.text = dialogEvent.m_dialog;

        //Clear choise buttons
        foreach (Transform child in m_choisesGroup.transform) Destroy(child.gameObject);

        //Create choise buttons
        if (m_historyIndex == 0U &&  dialogEvent.m_choises.Count > 0) m_dialogPannel.interactable = false;

        foreach (Choice choice in dialogEvent.m_choises)
        {
            GameObject choiceButton = Instantiate(m_choiseButtonPrefab.gameObject, m_choisesGroup.transform);
            choiceButton.GetComponentInChildren<TMP_Text>().text = choice.text;

            if (m_historyIndex > 0U) choiceButton.GetComponent<Button>().interactable = false;
            choiceButton.GetComponent<Button>().onClick.AddListener(delegate { Continue(choice.index); });
        }
    }

    IEnumerator TypeDialog(DialogEvent _dialogEvent)
    {
        m_dialogText.text = "";

        for (int characterIndex = 0; characterIndex < _dialogEvent.m_dialog.Length; characterIndex++)
        {
            m_dialogText.text += _dialogEvent.m_dialog[characterIndex];
            yield return new WaitForSeconds(1.0f / m_charactersPerSecond);
        }

        m_currentTypeDialog = null;
    }
}
