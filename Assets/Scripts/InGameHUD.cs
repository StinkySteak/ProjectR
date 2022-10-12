using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUD : SceneSingleton<InGameHUD>
{
    public GameObject DespawnedPanel;
    public GameObject SpawnedPanel;
    public GameObject PausePanel;
    public GameObject UniversalPanel;
    public GameObject EndGamePanel;

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text AmmoText;
    [SerializeField] private Image CrosshairOuterCircle;
    [SerializeField] private TMP_Text CartDistanceText;
    [SerializeField] private TMP_Text ContestedText;
    [SerializeField] private TMP_Text TimeRemainingText;

    [Header("EndGame")]
    public TMP_Text EndGameText;
    [Space]

    public float MinCroshairOuterCircle = 80;
    public float MaxCrosshairOuterCircle = 300;

    public float CrosshairLerpSpeed = 10;

    public float CrosshairOuterCircleMultiplier = 50;

    public bool OnPauseMenu => PausePanel.activeInHierarchy;

    public override void Awake()
    {
        base.Awake();

        LevelManager.OnRender += OnRender;
    }

    private void OnDestroy()
    {
        LevelManager.OnRender -= OnRender;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();
    }

    void TogglePauseMenu()
    {
        bool value = !PausePanel.activeInHierarchy;

        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = value;

        PausePanel.SetActive(value);
    }

    private void OnRender()
    {
        UpdateHealth();
        UpdateWeapon();
        UpdateCartDistance();
        UpdateTimer();
    }
    void UpdateTimer()
    {
        TimeRemainingText.text = TimeSpan.FromSeconds((double)LevelManager.Instance.DefendingDuration).ToString(@"mm\:ss");
    }
    void UpdateHealth()
    {
        if (PlayerHealth.LocalPlayer == null || PlayerHealth.LocalPlayer.Object == null || !PlayerHealth.LocalPlayer.Object.IsValid)
            return;

        slider.maxValue = PlayerHealth.LocalPlayer.MaxHealth;
        slider.value = PlayerHealth.LocalPlayer.Health;
    }
    /// <summary>
    /// Update Ammo & Crosshair
    /// </summary>
    void UpdateWeapon()
    {
        if (PlayerHealth.LocalPlayer == null || PlayerHealth.LocalPlayer.Object == null || !PlayerHealth.LocalPlayer.Object.IsValid)
            return;

        if (PlayerWeaponManager.LocalPlayer.ActiveWeapon == null)
        {
            AmmoText.text = string.Empty;
            return;
        }

        var weapon = PlayerWeaponManager.LocalPlayer.ActiveWeapon;

        UpdateWeaponText(weapon);
        UpdateWeaponCrosshair(weapon);
    }
    void UpdateWeaponText(Weapon weapon)
    {
        AmmoText.text = $"{weapon.CurrentBullet} / {weapon.TotalAmmo}";
    }
    void UpdateWeaponCrosshair(Weapon weapon)
    {
        var size = weapon.InAccuracy * CrosshairOuterCircleMultiplier;

        size = Mathf.Clamp(size, MinCroshairOuterCircle, MaxCrosshairOuterCircle);

        var prevSize = CrosshairOuterCircle.rectTransform.sizeDelta;

        CrosshairOuterCircle.rectTransform.sizeDelta = Vector2.Lerp(prevSize, Vector2.one * size,Time.deltaTime * CrosshairLerpSpeed);

   //     print(size);
    }
    void UpdateCartDistance()
    {
        ContestedText.gameObject.SetActive(Cart.Instance.IsContested);

        if (Plug.Instance.TryGetDistance(out float distance))
        {
            var text = distance < 5 ? distance.ToString() : Mathf.RoundToInt(distance).ToString();

            CartDistanceText.text = text;
        }
    }
}
