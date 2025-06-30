using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

namespace MiningSystem
{
    /// <summary>
    /// ระบบขุดแบบทดสอบสำหรับ Mixed Reality โดยใช้ XR Origin
    /// ยิงเลเซอร์ไปที่ Cube เพื่อทำการขุด
    /// </summary>
    public class MiningSystem : MonoBehaviour
    {
        [Header("Mining Settings")]
        [SerializeField]
        [Tooltip("ระยะของเลเซอร์ขุด")]
        float m_MiningRange = 10f;

        [SerializeField]
        [Tooltip("พลังในการขุดต่อวินาที")]
        float m_MiningPower = 1f;

        [SerializeField]
        [Tooltip("เลเยอร์ที่สามารถขุดได้")]
        LayerMask m_MineableLayer = 1;

        [Header("Laser Visual")]
        [SerializeField]
        [Tooltip("LineRenderer สำหรับแสดงเลเซอร์")]
        LineRenderer m_LaserRenderer;

        [SerializeField]
        [Tooltip("สีของเลเซอร์")]
        Color m_LaserColor = Color.red;

        [SerializeField]
        [Tooltip("ความกว้างของเลเซอร์")]
        float m_LaserWidth = 0.02f;

        [Header("XR Components")]
        [SerializeField]
        [Tooltip("Ray Interactor สำหรับตรวจจับการชี้")]
        XRRayInteractor m_RayInteractor;

        [Header("Input Actions")]
        [SerializeField]
        [Tooltip("Input Action สำหรับการขุด (Trigger/Mouse Click)")]
        InputActionReference m_MiningInputAction;

        [Header("Effects")]
        [SerializeField]
        [Tooltip("เอฟเฟกต์อนุภาคเมื่อขุด")]
        ParticleSystem m_MiningEffects;

        [SerializeField]
        [Tooltip("เสียงเมื่อขุด")]
        AudioSource m_MiningAudioSource;

        // Private variables
        MinableCube m_CurrentTarget;
        bool m_IsMining;
        Vector3 m_LaserStartPoint;
        Vector3 m_LaserEndPoint;
        
        // Input System variables
        bool m_TriggerPressed;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Start()
        {
            InitializeLaser();
            InitializeInputSystem();

            // ถ้าไม่มี Ray Interactor ให้ค้นหาใน GameObject
            if (m_RayInteractor == null)
                m_RayInteractor = GetComponentInChildren<XRRayInteractor>();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Update()
        {
            HandleMiningInput();
            UpdateLaser();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnEnable()
        {
            EnableInputActions();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDisable()
        {
            DisableInputActions();
        }

        void InitializeLaser()
        {
            if (m_LaserRenderer == null)
            {
                // สร้าง LineRenderer ใหม่ถ้าไม่มี
                GameObject laserObject = new GameObject("Mining Laser");
                laserObject.transform.SetParent(transform);
                m_LaserRenderer = laserObject.AddComponent<LineRenderer>();
            }

            // ตั้งค่า LineRenderer
            m_LaserRenderer.material = new Material(Shader.Find("Sprites/Default"));
            m_LaserRenderer.startColor = m_LaserColor;
            m_LaserRenderer.endColor = m_LaserColor;
            m_LaserRenderer.startWidth = m_LaserWidth;
            m_LaserRenderer.endWidth = m_LaserWidth;
            m_LaserRenderer.positionCount = 2;
            m_LaserRenderer.enabled = false;
        }

        void InitializeInputSystem()
        {
            // ถ้าไม่มี Mining Input Action ให้สร้างขึ้นมาใหม่
            if (m_MiningInputAction == null)
            {
                Debug.LogWarning("Mining Input Action not assigned. Using fallback method.");
            }
        }

        void EnableInputActions()
        {
            if (m_MiningInputAction != null)
            {
                m_MiningInputAction.action.Enable();
            }
        }

        void DisableInputActions()
        {
            if (m_MiningInputAction != null)
            {
                m_MiningInputAction.action.Disable();
            }
        }

        void HandleMiningInput()
        {
            // ตรวจสอบการกดปุ่ม Trigger หรือ Primary Button
            bool triggerPressed = false;

            // วิธีที่ 1: ใช้ Input Action Reference (แนะนำ)
            if (m_MiningInputAction != null)
            {
                triggerPressed = m_MiningInputAction.action.IsPressed();
            }
            else
            {
                // วิธีที่ 2: ใช้ XR Input Devices
                if (m_RayInteractor != null)
                {
                    var inputDevices = new List<UnityEngine.XR.InputDevice>();
                    UnityEngine.XR.InputDevices.GetDevices(inputDevices);

                    foreach (var device in inputDevices)
                    {
                        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerValue))
                        {
                            triggerPressed = triggerValue;
                            break;
                        }
                    }
                }

                // วิธีที่ 3: ใช้ New Input System สำหรับ Mouse (fallback สำหรับ Simulator)
                if (!triggerPressed)
                {
                    var mouse = Mouse.current;
                    if (mouse != null)
                    {
                        triggerPressed = mouse.leftButton.isPressed;
                    }
                }
            }

            if (triggerPressed)
                StartMining();
            else
                StopMining();
        }

        void StartMining()
        {
            // ยิง Raycast เพื่อหาเป้าหมาย
            RaycastHit hit;
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = transform.forward;

            if (m_RayInteractor != null)
            {
                rayOrigin = m_RayInteractor.rayOriginTransform.position;
                rayDirection = m_RayInteractor.rayOriginTransform.forward;
            }

            m_LaserStartPoint = rayOrigin;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, m_MiningRange, m_MineableLayer))
            {
                m_LaserEndPoint = hit.point;

                // ตรวจสอบว่าเป้าหมายเป็น MinableCube หรือไม่
                MinableCube minableCube = hit.collider.GetComponent<MinableCube>();
                if (minableCube != null)
                {
                    if (m_CurrentTarget != minableCube)
                    {
                        m_CurrentTarget = minableCube;
                        StartMiningEffects(hit.point);
                    }

                    // ทำการขุด
                    float damage = m_MiningPower * Time.deltaTime;
                    m_CurrentTarget.TakeDamage(damage);
                    m_IsMining = true;
                }
                else
                {
                    m_CurrentTarget = null;
                    m_IsMining = false;
                }
            }
            else
            {
                m_LaserEndPoint = rayOrigin + rayDirection * m_MiningRange;
                m_CurrentTarget = null;
                m_IsMining = false;
            }

            m_LaserRenderer.enabled = true;
        }

        void StopMining()
        {
            m_IsMining = false;
            m_CurrentTarget = null;
            m_LaserRenderer.enabled = false;
            StopMiningEffects();
        }

        void UpdateLaser()
        {
            if (m_LaserRenderer.enabled)
            {
                m_LaserRenderer.SetPosition(0, m_LaserStartPoint);
                m_LaserRenderer.SetPosition(1, m_LaserEndPoint);

                // เปลี่ยนสีเลเซอร์ตามสถานะ
                Color currentColor = m_IsMining ? Color.green : m_LaserColor;
                m_LaserRenderer.startColor = currentColor;
                m_LaserRenderer.endColor = currentColor;
            }
        }

        void StartMiningEffects(Vector3 position)
        {
            // เริ่มเอฟเฟกต์อนุภาค
            if (m_MiningEffects != null)
            {
                m_MiningEffects.transform.position = position;
                if (!m_MiningEffects.isPlaying)
                    m_MiningEffects.Play();
            }

            // เล่นเสียง
            if (m_MiningAudioSource != null && !m_MiningAudioSource.isPlaying)
                m_MiningAudioSource.Play();
        }

        void StopMiningEffects()
        {
            // หยุดเอฟเฟกต์อนุภาค
            if (m_MiningEffects != null && m_MiningEffects.isPlaying)
                m_MiningEffects.Stop();

            // หยุดเสียง
            if (m_MiningAudioSource != null && m_MiningAudioSource.isPlaying)
                m_MiningAudioSource.Stop();
        }
    }
}