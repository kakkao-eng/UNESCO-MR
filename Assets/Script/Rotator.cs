using UnityEngine;

public class Rotator : MonoBehaviour
{
    // กำหนดความเร็วในการหมุนในแต่ละแกน
    public float rotationSpeedX = 0f;
    public float rotationSpeedY = -40f; // เช่น หมุนรอบแกน Y -40 องศาต่อวินาที
    public float rotationSpeedZ = 0f;

    // ฟังก์ชัน Update จะถูกเรียกทุกเฟรม
    void Update()
    {
        // คำนวณปริมาณการหมุนสำหรับเฟรมนี้
        float xRotation = rotationSpeedX * Time.deltaTime;
        float yRotation = rotationSpeedY * Time.deltaTime;
        float zRotation = rotationSpeedZ * Time.deltaTime;

        // ใช้ transform.Rotate เพื่อหมุนวัตถุ
        // Space.Self คือการหมุนรอบแกนของวัตถุเอง (Local Space)
        transform.Rotate(xRotation, yRotation, zRotation, Space.Self);

        // หากต้องการหมุนรอบแกนของโลก (World Space) ให้เปลี่ยนเป็น Space.World
        // transform.Rotate(xRotation, yRotation, zRotation, Space.World);
    }
}