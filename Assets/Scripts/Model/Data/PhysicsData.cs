using UnityEngine;

namespace Model.Data
{
    [CreateAssetMenu(fileName = "PhysicsData", menuName = "Game/PhysicsData")]
    public class PhysicsData : ScriptableObject
    {
        [Header("重力加速度")] public float Gravity = -9.8f;
        [Header("摩擦・空気抵抗係数")] public float Friction = 0.95f;
        [Header("反復処理回数")] public int ConstraintIterations = 2;
        [Header("押し出し係数")] public float PushFactor = 0.6f;
        [Header("最大押し出し量")] public float MaxPushDistance = 0.2f;
    }
}