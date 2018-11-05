﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GroundPlaneUI : MonoBehaviour
{
    #region PUBLIC_MEMBERS
    [Header("UI Buttons")]
    public Button m_ResetButton;
    public Button m_ConfirmButton;
    public Button m_BackButton;
    public Button m_CaptureButton;
    public Button m_ListUpDown;
    public Button m_ShoeLeftRightTextButton;
    public Button m_SceneChangeButton;
    public Button m_HeartButton;
    public Button m_SocialShareButton;
    public Button m_BuyButton;

    [Header("UI Panels")]
    public RectTransform m_CustomListRectTransform;
    public RectTransform m_MidToolbarTectTrnasform;
    #endregion // PUBLIC_MEMBERS


    #region PRIVATE_MEMBERS
    ShoeController m_ShoeController;
    AudioSource shoePuttingSound;
    GraphicRaycaster[] m_GraphicRayCasters;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    GameObject m_CustomScrollView;
    GameObject shopWebView;
    float customizingListSize = 670f;
    #endregion // PRIVATE_MEMBERS

    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        m_ShoeController = FindObjectOfType<ShoeController>();
        m_GraphicRayCasters = FindObjectsOfType<GraphicRaycaster>();
        m_EventSystem = FindObjectOfType<EventSystem>();
        InitializeButtons();
        ChangeButtonStatus();
        SetCustomScrollView();
    }

    void Update()
    {
        ChangeButtonStatus();
        // If user click android back button, then call click back button method.
        if (Application.platform == RuntimePlatform.Android && Input.GetKey(KeyCode.Escape))
        {
            if (!ScreenshotPreview.previewGameObject.activeSelf && shopWebView==null)
            {
                ClickBackButton();
            }
            else if (ScreenshotPreview.previewGameObject.activeSelf)
            {
                ScreenshotPreview.previewGameObject.SetActive(false);
            }
            else if(shopWebView!=null) 
            {
                Destroy(shopWebView);
            }
        }
    }

    /// <summary>
    /// Sets the custom scroll view into Canvas.
    /// </summary>
    private void SetCustomScrollView()
    {
        m_CustomScrollView = Instantiate(UIManager.Instance.customizePanel.customize.gameObject);
        m_CustomScrollView.transform.SetParent(m_CustomListRectTransform.gameObject.transform);
        RectTransform customScrollViewRectTransform = m_CustomScrollView.GetComponent<RectTransform>();
        customScrollViewRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        customScrollViewRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        customScrollViewRectTransform.anchoredPosition = new Vector2(0f, 0f);
    }

    /// <summary>
    /// Add listener to each button.
    /// </summary>
    void InitializeButtons()
    {
        m_BackButton.onClick.AddListener(ClickBackButton);
        m_ResetButton.onClick.AddListener(ClickResetButton);
        m_CaptureButton.onClick.AddListener(ClickCaptureButton);
        m_ListUpDown.onClick.AddListener(ClickListUpDownButton);
        m_ShoeLeftRightTextButton.onClick.AddListener(ClickShoeLeftRightTextButton);
        m_SceneChangeButton.onClick.AddListener(ClickSceneChangeButton);
        m_HeartButton.onClick.AddListener(ClickHeartButton);
        m_SocialShareButton.onClick.AddListener(ClickSocialShareButton);
        m_BuyButton.onClick.AddListener(ClickBuyButton);
        if (SceneManager.GetActiveScene().name.Equals("WatchingShoes"))
        {
            m_ConfirmButton.onClick.AddListener(ClickConfirmButton);
        }
    }

    void ClickBackButton()
    {
        CurrentCustomShoe.shoes.GetComponent<Swiper>().enabled = true;
        SceneChanger.ChangeToShoeListScene();
    }

    void ClickConfirmButton()
    {
        if (m_ConfirmButton.image.enabled)
        {
            SetShoeStopped();
        }
    }

    private void SetShoeStopped()
    {
        m_ShoeController.IsPlaced = true;
        m_ShoeController.PlaceShoe();
        m_ConfirmButton.image.enabled = false;
        ChangeButtonStatus();
    }

    void ClickResetButton()
    {
        m_ShoeController.ResetAR();
        ChangeButtonStatus();
    }

    /// <summary>
    /// Capture and save image, the image can be shared optionally.
    /// </summary>
    void ClickCaptureButton()
    {
        StartCoroutine(ScreenshotPreview.CaptureAndShowPreviewImage()); // Start coroutine for screenshot function.
    }

    /// <summary>
    /// Clicks the list up and down button, then UIs are changed.
    /// </summary>
    void ClickListUpDownButton()
    {
        Vector2 originalPanelVector = m_CustomListRectTransform.anchoredPosition;
        Vector2 goalPanelVector;
        Vector2 originalToolbarVector = m_MidToolbarTectTrnasform.anchoredPosition;
        Vector2 goalToolbarVector;
        if (m_ListUpDown.image.sprite.name.Equals("arrow_up"))
        {
            m_ListUpDown.image.sprite = Resources.Load<Sprite>("Sprites/Arshoe/arrow_down");
            goalPanelVector = new Vector2(m_CustomListRectTransform.anchoredPosition.x, 0f);
            goalToolbarVector = new Vector2(m_MidToolbarTectTrnasform.anchoredPosition.x, m_MidToolbarTectTrnasform.anchoredPosition.y + customizingListSize);
            StartCoroutine(ListUpOrDownAnimation(originalPanelVector, goalPanelVector, originalToolbarVector, goalToolbarVector));
        }
        else
        {
            m_ListUpDown.image.sprite = Resources.Load<Sprite>("Sprites/Arshoe/arrow_up");
            goalPanelVector = new Vector2(m_CustomListRectTransform.anchoredPosition.x, -customizingListSize);
            goalToolbarVector = new Vector2(m_MidToolbarTectTrnasform.anchoredPosition.x, m_MidToolbarTectTrnasform.anchoredPosition.y - customizingListSize);
            StartCoroutine(ListUpOrDownAnimation(originalPanelVector, goalPanelVector, originalToolbarVector, goalToolbarVector));
        }
    }

    /// <summary>
    /// Extends or shrink the Panel height, change toolbar's anchor y position.
    /// </summary>
    /// <returns>The or shrink height.</returns>
    /// <param name="originalPanelVector">Original panel vector.</param>
    /// <param name="goalPanelVector">Goal vector.</param>
    /// <param name="originalToolbarVector">Original toolbar vector.</param>
    /// <param name="goalToolbarVector">Goal toolbar vector.</param>
    IEnumerator<RectTransform> ListUpOrDownAnimation(Vector2 originalPanelVector, Vector2 goalPanelVector, Vector2 originalToolbarVector, Vector2 goalToolbarVector)
    {
        float currentTime = 0f;
        float timeOver = 0.3f;

        while (currentTime < timeOver)
        {
            currentTime += Time.deltaTime;
            float normalizedValue = currentTime / timeOver; // we normalize our time 

            m_CustomListRectTransform.anchoredPosition = Vector2.Lerp(originalPanelVector, goalPanelVector, normalizedValue);
            m_MidToolbarTectTrnasform.anchoredPosition = Vector2.Lerp(originalToolbarVector, goalToolbarVector, normalizedValue);
            yield return null;
        }
    }

    /// <summary>
    /// Change shoe object to left or right
    /// </summary>
    void ClickShoeLeftRightTextButton()
    {
        if (m_ShoeLeftRightTextButton.image.sprite.name.Equals("right"))
        {
            m_ShoeLeftRightTextButton.image.sprite = Resources.Load<Sprite>("Sprites/Arshoe/left");
        }
        else
        {
            m_ShoeLeftRightTextButton.image.sprite = Resources.Load<Sprite>("Sprites/Arshoe/right");
        }
        m_ShoeController.ChangeLeftRight();
    }

    void ClickSceneChangeButton()
    {
        if (SceneManager.GetActiveScene().name.Equals("WatchingShoes"))
        {
            SceneChanger.ChangeToAttachShoes();
        }
        else
        {
            SceneChanger.ChangeToWatchingShoes();
        }
    }

    void ClickHeartButton()
    {
        if (m_HeartButton.image.sprite.name.Equals("UI_Icon_HeartEmpty"))
        {
            m_HeartButton.image.sprite = Resources.Load<Sprite>("Sprites/Icons/UI_Icon_Heart");
            ColorBlock colorBlock = m_HeartButton.colors;
            colorBlock.highlightedColor = new Color32(0, 164, 255, 255);
            colorBlock.normalColor = new Color32(0, 164, 255, 255);
            m_HeartButton.colors = colorBlock;
            // ToDo: Save Changed info.
        }
        else
        {
            m_HeartButton.image.sprite = Resources.Load<Sprite>("Sprites/Icons/UI_Icon_HeartEmpty");
            ColorBlock colorBlock = m_HeartButton.colors;
            colorBlock.highlightedColor = new Color32(0, 0, 0, 255);
            colorBlock.normalColor = new Color32(0, 0, 0, 255);
            m_HeartButton.colors = colorBlock;
            // ToDo: Save Changed info.
        }
    }

    /// <summary>
    /// Share link or itme info, etc.
    /// </summary>
    void ClickSocialShareButton()
    {
        // ToDo: Get url of shop.
        #if UNITY_ANDROID
        new NativeShare().SetText("text").Share();
        #elif UNITY_IOS
        new NativeShare().SetText("text").Share();
        #endif
    }

    void ClickBuyButton() {
        shopWebView = Instantiate(UIManager.Instance.shopPanel.gameObject);
        shopWebView.transform.SetParent(GameObject.Find("Canvas").transform);
    }

    public void SetShoeMovable()
    {
        m_ShoeController.IsPlaced = false;
        m_ShoeController.MoveShoe();
        m_ConfirmButton.image.enabled = true;
        ChangeButtonStatus();
    }

    // Change button's clickability and visualization.
    // Return true: If shoe object does not placed and vuforia detect floor, or shoe object placed.
    public void ChangeButtonStatus() {
        m_ResetButton.interactable = m_CaptureButton.interactable = m_ConfirmButton.interactable = m_ShoeController.DoesShoeActive;
        m_ConfirmButton.image.enabled = m_ShoeController.DoesShoeActive && !m_ShoeController.IsPlaced;
    }
#endregion // MONOBEHAVIOUR_METHODS
}
