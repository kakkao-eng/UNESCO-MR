using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// เล่นเสียงเมื่อกดปุ่ม (รองรับทั้ง UI ปกติ และ XR Interactable เช่น PICO Controller)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class UIButtonSound : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private SoundType soundType = SoundType.ClickButton; // ประเภทเสียง
    private Button uiButton;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable xrInteractable;

    void Awake()
    {
        // หา UI Button
        uiButton = GetComponent<Button>();

        // หา XR Interactable (ใช้กับวัตถุใน MR เช่น PICO)
        xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        // ถ้าเป็น UI ปุ่มปกติ
        if (uiButton != null)
        {
            uiButton.onClick.AddListener(PlaySound);
        }

        // ถ้าเป็น XR ปุ่ม (เช่น Object ที่กดด้วย Controller)
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.AddListener(OnXRSelected);
        }
    }

    private void PlaySound()
    {
        SoundManager.PlaySound(soundType);
    }

    private void OnXRSelected(SelectEnterEventArgs args)
    {
        PlaySound();
    }

    private void OnDestroy()
    {
        if (uiButton != null)
            uiButton.onClick.RemoveListener(PlaySound);

        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnXRSelected);
    }
}
