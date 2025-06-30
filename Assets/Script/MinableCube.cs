using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR && PROBUILDER_4_0_OR_NEWER
using Unity.ProBuilder;
using Unity.ProBuilder.MeshOperations;
#endif

namespace MiningSystem
{
    /// <summary>
    /// Cube ที่สามารถขุดได้ แสดงรอยขุดบนพื้นผิวเมื่อถูกโจมตี
    /// </summary>
    public class MinableCube : MonoBehaviour
    {
        [Header("Mining Properties")]
        [SerializeField]
        [Tooltip("พลังชีวิตของ Cube")]
        float m_MaxHealth = 10f;

        [SerializeField]
        [Tooltip("ขนาดรอยขุดต่อครั้ง")]
        float m_MiningImpactSize = 0.25f;

        [SerializeField]
        [Tooltip("ความลึกของรอยขุด")]
        float m_MiningDepth = 0.2f;

        [SerializeField]
        [Tooltip("จำนวนรอยขุดสูงสุด")]
        int m_MaxMiningHits = 20;

        [SerializeField]
        [Tooltip("ความเร็วในการแตกตัว")]
        float m_CollapseSpeed = 2f;

        [SerializeField]
        [Tooltip("เอฟเฟกต์เมื่อถูกทำลาย")]
        ParticleSystem m_DestroyEffect;

        [SerializeField]
        [Tooltip("เอฟเฟกต์เมื่อถูกขุด")]
        ParticleSystem m_MiningHitEffect;

        [SerializeField]
        [Tooltip("เสียงเมื่อถูกทำลาย")]
        AudioClip m_DestroySound;

        [SerializeField]
        [Tooltip("เสียงเมื่อถูกขุด")]
        AudioClip m_MiningHitSound;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("วัสดุเมื่อถูกขุด")]
        Material m_DamagedMaterial;

        [SerializeField]
        [Tooltip("วัสดุสำหรับรอยขุด")]
        Material m_CraterMaterial;

        [SerializeField]
        [Tooltip("Prefab สำหรับรอยขุด")]
        GameObject m_CraterPrefab;

        // Private variables
        float m_CurrentHealth;
        bool m_IsDestroyed;
        Vector3 m_OriginalScale;
        Renderer m_Renderer;
        Material m_OriginalMaterial;
        AudioSource m_AudioSource;
        List<GameObject> m_MiningHits = new List<GameObject>();
        MeshFilter m_MeshFilter;

#if UNITY_EDITOR && PROBUILDER_4_0_OR_NEWER
        ProBuilderMesh m_ProBuilderMesh;
#endif

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            m_CurrentHealth = m_MaxHealth;
            m_OriginalScale = transform.localScale;
            m_Renderer = GetComponent<Renderer>();
            m_MeshFilter = GetComponent<MeshFilter>();
            m_AudioSource = GetComponent<AudioSource>();

            if (m_AudioSource == null)
                m_AudioSource = gameObject.AddComponent<AudioSource>();

            if (m_Renderer != null)
                m_OriginalMaterial = m_Renderer.material;

            // สร้าง Material สำหรับรอยขุดถ้าไม่ได้กำหนดไว้
            if (m_CraterMaterial == null)
            {
                m_CraterMaterial = new Material(Shader.Find("Standard"));
                m_CraterMaterial.color = new Color(0.3f, 0.3f, 0.3f);
            }

            // สร้าง Prefab สำหรับรอยขุดถ้าไม่ได้กำหนดไว้
            if (m_CraterPrefab == null)
            {
                m_CraterPrefab = Resources.Load<GameObject>("MiningCrater");
            }

#if UNITY_EDITOR && PROBUILDER_4_0_OR_NEWER
            m_ProBuilderMesh = GetComponent<ProBuilderMesh>();
            
            // ถ้าไม่มี ProBuilder Mesh ให้เพิ่ม
            if (m_ProBuilderMesh == null)
            {
                m_ProBuilderMesh = gameObject.AddComponent<ProBuilderMesh>();
                // สร้าง Cube ProBuilder
                var shape = ShapeGenerator.CreateShape(ShapeType.Cube);
                m_ProBuilderMesh.Clear();
                m_ProBuilderMesh.SetMesh(shape.mesh);
                m_ProBuilderMesh.Refresh();
            }
#endif
        }

        /// <summary>
        /// ทำความเสียหายต่อ Cube
        /// </summary>
        /// <param name="damage">ปริมาณความเสียหาย</param>
        public void TakeDamage(float damage)
        {
            if (m_IsDestroyed) return;

            m_CurrentHealth -= damage;

            // อัพเดตการแสดงผล
            UpdateVisualFeedback();

            // ตรวจสอบว่าถูกทำลายหรือไม่
            if (m_CurrentHealth <= 0)
            {
                StartDestroy();
            }
        }

        /// <summary>
        /// ทำความเสียหายต่อ Cube และสร้างรอยขุดที่ตำแหน่งที่กำหนด
        /// </summary>
        /// <param name="damage">ปริมาณความเสียหาย</param>
        /// <param name="hitPoint">ตำแหน่งที่ถูกขุด</param>
        /// <param name="hitNormal">ทิศทางปกติของพื้นผิวที่ถูกขุด</param>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (m_IsDestroyed) return;

            m_CurrentHealth -= damage;

            // สร้างรอยขุดที่ตำแหน่งที่กำหนด
            CreateMiningImpact(hitPoint, hitNormal);

            // อัพเดตการแสดงผล
            UpdateVisualFeedback();

            // แสดงเอฟเฟกต์การขุด
            ShowMiningEffect(hitPoint, hitNormal);

            // ตรวจสอบว่าถูกทำลายหรือไม่
            if (m_CurrentHealth <= 0)
            {
                StartDestroy();
            }
        }

        void CreateMiningImpact(Vector3 hitPoint, Vector3 hitNormal)
        {
            // จำกัดจำนวนรอยขุด
            if (m_MiningHits.Count >= m_MaxMiningHits)
            {
                // ลบรอยขุดเก่าสุด
                if (m_MiningHits.Count > 0)
                {
                    Destroy(m_MiningHits[0]);
                    m_MiningHits.RemoveAt(0);
                }
            }

            // สร้างรอยขุดด้วยวิธีที่เหมาะสม
#if UNITY_EDITOR && PROBUILDER_4_0_OR_NEWER
            if (m_ProBuilderMesh != null)
            {
                CreateProBuilderCrater(hitPoint, hitNormal);
            }
            else
            {
                CreateMeshDeformation(hitPoint, hitNormal);
            }
#else
            CreateDecalCrater(hitPoint, hitNormal);
#endif
        }

#if UNITY_EDITOR && PROBUILDER_4_0_OR_NEWER
        void CreateProBuilderCrater(Vector3 hitPoint, Vector3 hitNormal)
        {
            try
            {
                // แปลงตำแหน่งจาก world space เป็น local space
                Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
                
                // หาจุดที่ใกล้ที่สุดกับจุดที่ถูกขุด
                var positions = m_ProBuilderMesh.positions;
                var normals = m_ProBuilderMesh.normals;
                
                // กำหนดค่าสำหรับการดึงจุดเข้าไปด้านใน
                float impactRadius = m_MiningImpactSize;
                float impactDepth = m_MiningDepth;
                
                // วนลูปผ่านทุกจุดของ mesh
                for (int i = 0; i < positions.Count; i++)
                {
                    // คำนวณระยะห่างจากจุดที่ถูกขุด
                    float dist = Vector3.Distance(positions[i], localHitPoint);
                    
                    // ถ้าจุดอยู่ในรัศมีผลกระทบ
                    if (dist < impactRadius)
                    {
                        // คำนวณความลึกของผลกระทบ (จุดที่ใกล้ศูนย์กลางจะลึกกว่า)
                        float depthFactor = 1f - (dist / impactRadius);
                        float depth = impactDepth * depthFactor * depthFactor;
                        
                        // ดึงจุดเข้าไปตามทิศทางปกติของพื้นผิว
                        Vector3 hitNormalLocal = transform.InverseTransformDirection(hitNormal);
                        positions[i] += hitNormalLocal * depth;
                    }
                }
                
                // นำการเปลี่ยนแปลงไปใช้กับ mesh
                m_ProBuilderMesh.positions = positions;
                m_ProBuilderMesh.ToMesh();
                m_ProBuilderMesh.Refresh();
                
                // อัพเดต collider
                if (GetComponent<MeshCollider>() != null)
                {
                    GetComponent<MeshCollider>().sharedMesh = m_ProBuilderMesh.mesh;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error creating ProBuilder crater: {e.Message}");
                CreateDecalCrater(hitPoint, hitNormal);
            }
        }
#endif

        void CreateMeshDeformation(Vector3 hitPoint, Vector3 hitNormal)
        {
            if (m_MeshFilter != null && m_MeshFilter.mesh != null)
            {
                try
                {
                    // ทำสำเนา mesh เพื่อแก้ไข
                    Mesh mesh = Instantiate(m_MeshFilter.mesh);
                    
                    // แปลงตำแหน่งจาก world space เป็น local space
                    Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
                    Vector3 localHitNormal = transform.InverseTransformDirection(hitNormal);
                    
                    // ดึงข้อมูลของ mesh
                    Vector3[] vertices = mesh.vertices;
                    
                    // กำหนดค่าสำหรับการดึงจุดเข้าไปด้านใน
                    float impactRadius = m_MiningImpactSize;
                    float impactDepth = m_MiningDepth;
                    
                    // วนลูปผ่านทุกจุดของ mesh
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        // คำนวณระยะห่างจากจุดที่ถูกขุด
                        float dist = Vector3.Distance(vertices[i], localHitPoint);
                        
                        // ถ้าจุดอยู่ในรัศมีผลกระทบ
                        if (dist < impactRadius)
                        {
                            // คำนวณความลึกของผลกระทบ (จุดที่ใกล้ศูนย์กลางจะลึกกว่า)
                            float depthFactor = 1f - (dist / impactRadius);
                            float depth = impactDepth * depthFactor * depthFactor;
                            
                            // ดึงจุดเข้าไปตามทิศทางปกติของพื้นผิว
                            vertices[i] += localHitNormal * depth;
                        }
                    }
                    
                    // นำการเปลี่ยนแปลงไปใช้กับ mesh
                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    
                    // อัพเดต mesh และ collider
                    m_MeshFilter.mesh = mesh;
                    if (GetComponent<MeshCollider>() != null)
                    {
                        GetComponent<MeshCollider>().sharedMesh = mesh;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error deforming mesh: {e.Message}");
                    CreateDecalCrater(hitPoint, hitNormal);
                }
            }
            else
            {
                CreateDecalCrater(hitPoint, hitNormal);
            }
        }

        void CreateDecalCrater(Vector3 hitPoint, Vector3 hitNormal)
        {
            // วิธีที่ 3: สร้าง decal หรือ GameObject เล็กๆ ที่แสดงรอยขุด
            GameObject crater;
            
            if (m_CraterPrefab != null)
            {
                crater = Instantiate(m_CraterPrefab, hitPoint, Quaternion.identity);
            }
            else
            {
                // สร้าง primitive แบน (เช่น quad หรือ cylinder แบนๆ)
                crater = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(crater.GetComponent<Collider>());  // ลบ collider เพื่อไม่ให้รบกวน physics
                
                // ปรับขนาดให้เหมาะสม
                float craterSize = m_MiningImpactSize * 2f;
                crater.transform.localScale = new Vector3(craterSize, craterSize, 0.01f);
                
                // ตั้งค่าวัสดุให้รอยขุด
                Renderer craterRenderer = crater.GetComponent<Renderer>();
                craterRenderer.material = m_CraterMaterial != null ? m_CraterMaterial : m_DamagedMaterial;
                
                // ตั้งค่าให้อยู่ชั้นด้านหน้าของ Cube เล็กน้อย
                craterRenderer.material.renderQueue = 2900;  // เลขที่สูงกว่าจะอยู่ด้านหน้า
            }
            
            // ตั้งตำแหน่งและหมุนให้ตรงกับพื้นผิวที่ถูกขุด
            crater.transform.position = hitPoint + hitNormal * 0.01f;  // ออกมาจากพื้นผิวเล็กน้อยเพื่อป้องกัน z-fighting
            crater.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);
            
            // กำหนดให้เป็น child ของ cube
            crater.transform.SetParent(transform);
            
            // เพิ่มเข้าไปในรายการรอยขุด
            m_MiningHits.Add(crater);
        }

        void ShowMiningEffect(Vector3 hitPoint, Vector3 hitNormal)
        {
            // แสดงเอฟเฟกต์อนุภาคเมื่อขุด
            if (m_MiningHitEffect != null)
            {
                ParticleSystem effect = Instantiate(m_MiningHitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
                effect.Play();
                Destroy(effect.gameObject, 2f);  // ทำลายหลังจากเล่นเสร็จ
            }
            
            // เล่นเสียงเมื่อขุด
            if (m_MiningHitSound != null && m_AudioSource != null)
            {
                m_AudioSource.pitch = Random.Range(0.9f, 1.1f);  // เพิ่มความหลากหลายของเสียง
                m_AudioSource.PlayOneShot(m_MiningHitSound);
            }
        }

        void UpdateVisualFeedback()
        {
            // เปลี่ยนสีหรือวัสดุตามความเสียหาย
            float healthPercent = m_CurrentHealth / m_MaxHealth;

            if (m_Renderer != null && m_DamagedMaterial != null)
            {
                // ผสมวัสดุระหว่างต้นฉบับและวัสดุเสียหาย
                Color originalColor = m_OriginalMaterial.color;
                Color damagedColor = m_DamagedMaterial.color;
                Color currentColor = Color.Lerp(damagedColor, originalColor, healthPercent);
                m_Renderer.material.color = currentColor;
            }

            // ปรับขนาดเล็กลงเมื่อถูกขุด
            float scaleMultiplier = Mathf.Lerp(0.7f, 1f, healthPercent);
            transform.localScale = m_OriginalScale * scaleMultiplier;
        }

        void StartDestroy()
        {
            if (m_IsDestroyed) return;

            m_IsDestroyed = true;
            StartCoroutine(CollapseAndDestroy());
        }

        IEnumerator CollapseAndDestroy()
        {
            // เล่นเอฟเฟกต์การทำลาย
            if (m_DestroyEffect != null)
            {
                Instantiate(m_DestroyEffect, transform.position, transform.rotation);
            }

            // เล่นเสียง
            if (m_DestroySound != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(m_DestroySound);
            }

            // สร้างเศษซากเพื่อให้ดูเหมือนแตกออกเป็นชิ้นๆ
            CreateDebris();

            // ยุบตัวลงและหมุนอย่างช้าๆ
            Vector3 targetScale = Vector3.zero;
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < m_CollapseSpeed)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / m_CollapseSpeed;

                // ใช้ ease-in curve สำหรับการยุบ
                progress = 1f - Mathf.Pow(1f - progress, 3f);

                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);

                // หมุน Cube เล็กน้อยขณะยุบ
                transform.Rotate(Vector3.up, 90f * Time.deltaTime);

                yield return null;
            }

            // ทำลาย GameObject
            Destroy(gameObject);
        }

        void CreateDebris()
        {
            // สร้างชิ้นส่วนเล็กๆ เพื่อจำลองการแตกออกเป็นชิ้นๆ
            int debrisCount = Random.Range(5, 10);
            
            for (int i = 0; i < debrisCount; i++)
            {
                // สร้าง cube เล็กๆ
                GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                // ตั้งค่าขนาด
                float size = Random.Range(0.1f, 0.3f);
                debris.transform.localScale = new Vector3(size, size, size);
                
                // ตั้งค่าตำแหน่งเริ่มต้น (สุ่มรอบๆ cube)
                debris.transform.position = transform.position + Random.insideUnitSphere * 0.3f;
                
                // ตั้งค่าการหมุน
                debris.transform.rotation = Random.rotation;
                
                // ตั้งค่าวัสดุ
                if (m_Renderer != null)
                {
                    debris.GetComponent<Renderer>().material = m_Renderer.material;
                }
                
                // เพิ่ม Rigidbody เพื่อให้มีการเคลื่อนไหวทางฟิสิกส์
                Rigidbody rb = debris.AddComponent<Rigidbody>();
                
                // กำหนดแรงเริ่มต้น (ดันออกจากศูนย์กลางของ cube)
                Vector3 forceDir = (debris.transform.position - transform.position).normalized;
                rb.AddForce(forceDir * Random.Range(2f, 5f), ForceMode.Impulse);
                
                // เพิ่มแรงบิด
                rb.AddTorque(Random.insideUnitSphere * Random.Range(0.5f, 2f), ForceMode.Impulse);
                
                // ทำลายหลังจากเวลาผ่านไป
                Destroy(debris, 3f);
            }
        }

        /// <summary>
        /// รีเซ็ต Cube กลับสู่สถานะเริ่มต้น
        /// </summary>
        public void ResetCube()
        {
            m_CurrentHealth = m_MaxHealth;
            m_IsDestroyed = false;
            transform.localScale = m_OriginalScale;

            // ลบรอยขุดทั้งหมด
            foreach (var hit in m_MiningHits)
            {
                if (hit != null)
                    Destroy(hit);
            }
            m_MiningHits.Clear();

            if (m_Renderer != null && m_OriginalMaterial != null)
                m_Renderer.material = m_OriginalMaterial;

#if UNITY_EDITOR && PROBUILDER_4_0_OR_NEWER
            // รีเซ็ต mesh ถ้ามีการใช้ ProBuilder
            if (m_ProBuilderMesh != null)
            {
                var shape = ShapeGenerator.CreateShape(ShapeType.Cube);
                m_ProBuilderMesh.Clear();
                m_ProBuilderMesh.SetMesh(shape.mesh);
                m_ProBuilderMesh.Refresh();
            }
#endif
        }

        /// <summary>
        /// ได้รับความสุขภาพปัจจุบันเป็นเปอร์เซ็นต์
        /// </summary>
        public float GetHealthPercent()
        {
            return m_CurrentHealth / m_MaxHealth;
        }

        /// <summary>
        /// ตรวจสอบว่า Cube ถูกทำลายแล้วหรือไม่
        /// </summary>
        public bool IsDestroyed()
        {
            return m_IsDestroyed;
        }
    }
}