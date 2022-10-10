using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUD : SceneSingleton<InGameHUD>
{
    public GameObject DespawnedPanel;
    public GameObject SpawnedPanel;

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text AmmoText;
    [SerializeField] private Image CrosshairOuterCircle;

    public float MinCroshairOuterCircle = 80;
    public float MaxCrosshairOuterCircle = 300;

    public float CrosshairLerpSpeed = 10;

    public float CrosshairOuterCircleMultiplier = 50;

    public override void Awake()
    {
        base.Awake();

        LevelManager.OnRender += OnRender;
    }

    private void OnRender()
    {
        UpdateHealth();
        UpdateWeapon();
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
}
