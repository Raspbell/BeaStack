using UnityEngine;

namespace Model.Logic
{
    public struct TsumPhysicsData
    {
        public bool IsActive;
        public bool IsStatic;
        public float Radius;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public Vector2 RenderStartPosition;
        public Vector2 RenderEndPosition;
        public float Rotation;
        public float RenderStartRotation;
        public float RenderEndRotation;
    }
}