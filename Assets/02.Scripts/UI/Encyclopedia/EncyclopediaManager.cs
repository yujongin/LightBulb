using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;


public class EncyclopediaManager : MonoBehaviour
{
    [SerializeField] private TMP_Text CharacterTitleText;
    [SerializeField] private TMP_Text ObservationTitleText;
    [SerializeField] private Button closeButton;

    [Header("Character Panel")]
    [SerializeField] private Image contentImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private GameObject pageObject;

    [Header("Description Panel")]
    [SerializeField] private GameObject lookDescription;
    [SerializeField] private Toggle lookToggle;

    [Header("Life Style Panel")]
    [SerializeField] private GameObject lifeStyleDescription;
    [SerializeField] private Toggle lifeStyleToggle;

    [Header("Feature Toggles")]
    [SerializeField] private Toggle characterToggle;
    [SerializeField] private Toggle observationToggle;

    [Header("Source Link")]
    [SerializeField] private GameObject sourceObject;

    private EncyclopediaDataParser dataParser;


    // UI 패널 정보를 캡슐화하는 클래스
    [System.Serializable]
    private class UIPanel
    {
        public Image[] iconImages;
        public TMP_Text[] texts;
        public Toggle toggle;

        public void ResetAllElements()
        {
            if (iconImages == null || texts == null) return;

            for (int i = 0; i < iconImages.Length && i < texts.Length; i++)
            {
                if (iconImages[i] != null) iconImages[i].gameObject.SetActive(false);
                if (texts[i] != null) texts[i].text = "";
            }
        }

        public void UpdateElements(string[] data)
        {
            if (data == null || iconImages == null || texts == null) return;

            for (int i = 0; i < data.Length && i < iconImages.Length && i < texts.Length; i++)
            {
                if (iconImages[i] != null) iconImages[i].gameObject.SetActive(true);
                if (texts[i] != null) texts[i].text = data[i];
            }
        }
    }

    private UIPanel lookPanel;
    private UIPanel lifeStylePanel;

    private GameObject currentPageObject;
    private EncyclopediaData currentEncyclopediaInfo;
    private int currentImageIndex;
    private bool isInitialized = false;
    private string currentInfoName;
    private int CurrentImageIndex
    {
        get => currentImageIndex;
        set
        {
            currentImageIndex = value;
            UpdateNavigationButtons();
            UpdateSourceLink();
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        InitializePageObject();
        InitializeButtons();
        InitializePanels();
        InitializeSourceLink();
        dataParser = FindFirstObjectByType<EncyclopediaDataParser>(FindObjectsInactive.Include);
    }

    private void InitializePageObject()
    {
        if (pageObject != null && pageObject.transform.childCount > 0)
        {
            currentPageObject = pageObject.transform.GetChild(0).gameObject;
        }
        else
        {
            
        }
    }

    private void InitializeButtons()
    {
        closeButton.onClick.AddListener(CloseEncyclopedia);
        nextButton.onClick.AddListener(() => NextImage(currentEncyclopediaInfo.itemKey));
        previousButton.onClick.AddListener(() => PreviousImage(currentEncyclopediaInfo.itemKey));
    }

    private void InitializePanels()
    {
        lookPanel = new UIPanel
        {
            iconImages = GetComponentsInChildrenOnly<Image>(lookDescription),
            texts = GetComponentsInChildrenOnly<TMP_Text>(lookDescription),
            toggle = lookToggle
        };

        lifeStylePanel = new UIPanel
        {
            iconImages = GetComponentsInChildrenOnly<Image>(lifeStyleDescription),
            texts = GetComponentsInChildrenOnly<TMP_Text>(lifeStyleDescription),
            toggle = lifeStyleToggle
        };
    }
    private void InitializeSourceLink()
    {
        sourceObject.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
        {
            Managers.System.NotifyUrlLink(SystemManager.LinkType.SourceLink, currentEncyclopediaInfo.itemImageData[CurrentImageIndex].sourceLink);
        });
        sourceObject.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
        {
            Managers.System.NotifyUrlLink(SystemManager.LinkType.CCBYLink, currentEncyclopediaInfo.itemImageData[CurrentImageIndex].CCBYLink);
        });
    }

    private T[] GetComponentsInChildrenOnly<T>(GameObject parent) where T : Component
    {
        var components = new System.Collections.Generic.List<T>();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            components.AddRange(child.GetComponentsInChildren<T>());
        }

        return components.ToArray();
    }

    public void InitEncyclopediaInfo(EncyclopediaData info)
    {
        if (info == null) return;

        Initialize();
        currentEncyclopediaInfo = info;

        SetupTitles(info);
        SetupPageObjects(info);
        SetupDescriptionPanels(info);
        SetupImages(info);
    }

    private void SetupTitles(EncyclopediaData info)
    {
        CharacterTitleText.text = info.itemName;
        ObservationTitleText.text = info.itemName;
        currentInfoName = info.itemName;
    }

    private void SetupPageObjects(EncyclopediaData info)
    {
        CurrentImageIndex = 0;
        for (int i = 0; i < pageObject.transform.childCount; i++)
        {
            bool shouldActivate = i < info.itemImageData.Length;
            pageObject.transform.GetChild(i).gameObject.SetActive(shouldActivate);
        }
    }

    private void SetupDescriptionPanels(EncyclopediaData info)
    {
        // Look Panel 설정
        lookPanel.toggle.isOn = true;
        lookPanel.ResetAllElements();
        lookPanel.UpdateElements(info.itemDescription);

        // Life Style Panel 설정
        bool hasLifeStyleData = info.lifeStyle != null && info.lifeStyle.Length > 0;
        lifeStylePanel.toggle.interactable = hasLifeStyleData;
        lifeStylePanel.ResetAllElements();

        if (hasLifeStyleData)
        {
            lifeStylePanel.UpdateElements(info.lifeStyle);
        }
    }

    private void SetupImages(EncyclopediaData info)
    {
        if (info.itemImageData != null && info.itemImageData.Length > 0)
        {
            contentImage.sprite = dataParser.GetCachedImages(info.itemKey)[0];
            CurrentImageIndex = 0;
            nextButton.gameObject.SetActive(info.itemImageData.Length > 1);
            previousButton.gameObject.SetActive(info.itemImageData.Length > 1);
            pageObject.SetActive(info.itemImageData.Length > 1);
        }
    }

    public void NextImage(string key)
    {
        if (currentEncyclopediaInfo?.itemImageData == null) return;

        if (CurrentImageIndex < currentEncyclopediaInfo.itemImageData.Length - 1)
        {
            CurrentImageIndex++;

            contentImage.sprite = dataParser.GetCachedImages(key)[CurrentImageIndex];
        }
    }

    public void PreviousImage(string key)
    {
        if (currentEncyclopediaInfo?.itemImageData == null) return;

        if (CurrentImageIndex > 0)
        {
            CurrentImageIndex--;
            contentImage.sprite = dataParser.GetCachedImages(key)[CurrentImageIndex];
        }
    }
    private void UpdateSourceLink()
    {
        ImageData currentImageData = currentEncyclopediaInfo.itemImageData[CurrentImageIndex];
        if (currentImageData != null)
        {
            bool hasSourceLink = currentImageData.sourceLink != null && currentImageData.sourceLink != "";
            sourceObject.SetActive(hasSourceLink);
            if (!hasSourceLink) return;
            sourceObject.transform.GetChild(2).GetComponent<TMP_Text>().text = currentEncyclopediaInfo.itemImageData[CurrentImageIndex].authorship;
        }
    }
    private void UpdateNavigationButtons()
    {
        if (currentEncyclopediaInfo?.itemImageData == null) return;

        previousButton.interactable = CurrentImageIndex > 0;
        nextButton.interactable = CurrentImageIndex < currentEncyclopediaInfo.itemImageData.Length - 1;

        if (currentPageObject != null)
            currentPageObject.transform.SetSiblingIndex(currentImageIndex);
    }

    public void CloseEncyclopedia()
    {
        gameObject.SetActive(false);
        if (!Managers.System.GetCondition("encyclopedia_" + currentEncyclopediaInfo.itemKey))
        {
            Managers.System.UpdateCondition("encyclopedia_" + currentEncyclopediaInfo.itemKey, true);
        }
    }

    public void ActiveObservationPanel(bool isActive)
    {
        if (isActive)
        {
            observationToggle.gameObject.SetActive(true);
            observationToggle.isOn = true;
        }
        else
        {
            characterToggle.isOn = true;
            observationToggle.gameObject.SetActive(false);
        }
    }
}
