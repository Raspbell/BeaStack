using UnityEngine;
using System.Collections.Generic;
using Model.Data;

namespace Model.Logic
{
    public class TsumPhysicsManager
    {
        private TsumPhysicsData[] _tsumPhysicsData;
        private Stack<int> _availableIndices;
        private PhysicsData _physicsData;

        public TsumPhysicsManager(int capacity, PhysicsData physicsData)
        {
            _physicsData = physicsData;
            _tsumPhysicsData = new TsumPhysicsData[capacity];

            _availableIndices = new Stack<int>(capacity);
            for (int i = capacity - 1; i >= 0; i--)
            {
                _availableIndices.Push(i);
            }
        }

        public int AllocatePhysicsIndex()
        {
            if (_availableIndices.Count > 0)
            {
                int index = _availableIndices.Pop();
                return index;
            }
            return -1;
        }

        public void InitializeTsum(int physicsIndex, Vector2 position, float radius)
        {
            _tsumPhysicsData[physicsIndex].IsActive = true;
            _tsumPhysicsData[physicsIndex].IsStatic = false;
            _tsumPhysicsData[physicsIndex].IsGameoverTarget = false;
            _tsumPhysicsData[physicsIndex].Radius = radius;
            _tsumPhysicsData[physicsIndex].Position = position;
            _tsumPhysicsData[physicsIndex].PreviousPosition = position;
            _tsumPhysicsData[physicsIndex].RenderStartPosition = position;
            _tsumPhysicsData[physicsIndex].RenderEndPosition = position;
            _tsumPhysicsData[physicsIndex].Rotation = 0f;
            _tsumPhysicsData[physicsIndex].RenderStartRotation = 0f;
            _tsumPhysicsData[physicsIndex].RenderEndRotation = 0f;
        }

        public void ReleasePhysicsIndex(int physicsIndex)
        {
            _availableIndices.Push(physicsIndex);
            _tsumPhysicsData[physicsIndex].IsActive = false;
        }

        public bool ExistTsumAboveDeadLine(float height)
        {
            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (_tsumPhysicsData[i].IsActive && _tsumPhysicsData[i].IsGameoverTarget && _tsumPhysicsData[i].Position.y + _tsumPhysicsData[i].Radius > height)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetGameoverTargetByHeight(float height)
        {
            // 重力による1フレーム分の移動量を計算 (だいたい)
            float gravityStep = _physicsData.Gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;

            // その移動量の 1/10 以下しか動いていないなら何かに載って止まっているとみなす
            float sleepThresholdSqr = (gravityStep * gravityStep) * 0.1f;

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (!_tsumPhysicsData[i].IsActive || _tsumPhysicsData[i].IsGameoverTarget)
                {
                    continue;
                }

                Vector2 movement = _tsumPhysicsData[i].Position - _tsumPhysicsData[i].PreviousPosition;

                // 移動量が閾値より小さいなら着地して安定したとみなす
                if (movement.sqrMagnitude < sleepThresholdSqr)
                {
                    _tsumPhysicsData[i].IsGameoverTarget = true;
                }
            }
        }

        public int GetGameoverTargetCount()
        {
            int count = 0;
            if (_tsumPhysicsData == null) return 0;

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (_tsumPhysicsData[i].IsActive && _tsumPhysicsData[i].IsGameoverTarget)
                {
                    count++;
                }
            }
            return count;
        }

        public void GetTsumsInRadius(Vector2 center, float radius, List<int> resultIndices)
        {
            resultIndices.Clear();
            float radiusSqr = radius * radius;

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (!_tsumPhysicsData[i].IsActive) continue;

                Vector2 diff = _tsumPhysicsData[i].Position - center;
                if (diff.sqrMagnitude <= radiusSqr)
                {
                    resultIndices.Add(i);
                }
            }
        }

        public Vector2 GetTsumPosition(int physicsIndex)
        {
            return _tsumPhysicsData[physicsIndex].Position;
        }

        public Vector2 GetInterpolatedPosition(int physicsIndex, float alpha)
        {
            return Vector2.Lerp(_tsumPhysicsData[physicsIndex].RenderStartPosition, _tsumPhysicsData[physicsIndex].RenderEndPosition, alpha);
        }

        public float GetInterpolatedRotation(int physicsIndex, float alpha)
        {
            return Mathf.Lerp(_tsumPhysicsData[physicsIndex].RenderStartRotation, _tsumPhysicsData[physicsIndex].RenderEndRotation, alpha);
        }

        public float GetTsumRadius(int physicsIndex)
        {
            return _tsumPhysicsData[physicsIndex].Radius;
        }

        public void SetRadius(int physicsIndex, float radius)
        {
            _tsumPhysicsData[physicsIndex].Radius = radius;
        }

        public void SetStatic(int physicsIndex, bool isStatic)
        {
            _tsumPhysicsData[physicsIndex].IsStatic = isStatic;
        }

        public void SetGameOverTarget(int physicsIndex, bool isGameOverTarget)
        {
            _tsumPhysicsData[physicsIndex].IsGameoverTarget = isGameOverTarget;
        }

        public void UpdateAllTsumPosition(float deltaTime, float leftX, float rightX, float bottomY, float topY)
        {
            Vector2 gravity = new Vector2(0, _physicsData.Gravity);

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (_tsumPhysicsData[i].IsActive)
                {
                    _tsumPhysicsData[i].RenderStartPosition = _tsumPhysicsData[i].Position;
                    _tsumPhysicsData[i].RenderStartRotation = _tsumPhysicsData[i].Rotation;
                }
            }

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (!_tsumPhysicsData[i].IsActive)
                {
                    continue;
                }

                if (_tsumPhysicsData[i].IsStatic)
                {
                    continue;
                }

                Vector2 currentPosition = _tsumPhysicsData[i].Position;
                Vector2 velocity = (currentPosition - _tsumPhysicsData[i].PreviousPosition) * _physicsData.Friction;
                Vector2 nextPosition = currentPosition + velocity + gravity * (deltaTime * deltaTime);

                _tsumPhysicsData[i].PreviousPosition = currentPosition;
                _tsumPhysicsData[i].Position = nextPosition;
            }

            for (int iter = 0; iter < _physicsData.ConstraintIterations; iter++)
            {
                for (int i = 0; i < _tsumPhysicsData.Length; i++)
                {
                    if (!_tsumPhysicsData[i].IsActive)
                    {
                        continue;
                    }

                    if (_tsumPhysicsData[i].IsStatic)
                    {
                        continue;
                    }

                    if (_tsumPhysicsData[i].Position.y - _tsumPhysicsData[i].Radius < bottomY)
                    {
                        _tsumPhysicsData[i].Position.y = bottomY + _tsumPhysicsData[i].Radius;
                    }

                    if (_tsumPhysicsData[i].Position.y + _tsumPhysicsData[i].Radius > topY)
                    {
                        _tsumPhysicsData[i].Position.y = topY - _tsumPhysicsData[i].Radius;
                    }

                    if (_tsumPhysicsData[i].Position.x - _tsumPhysicsData[i].Radius < leftX)
                    {
                        _tsumPhysicsData[i].Position.x = leftX + _tsumPhysicsData[i].Radius;
                    }

                    if (_tsumPhysicsData[i].Position.x + _tsumPhysicsData[i].Radius > rightX)
                    {
                        _tsumPhysicsData[i].Position.x = rightX - _tsumPhysicsData[i].Radius;
                    }
                }

                for (int i = 0; i < _tsumPhysicsData.Length; i++)
                {
                    if (!_tsumPhysicsData[i].IsActive)
                    {
                        continue;
                    }

                    for (int j = i + 1; j < _tsumPhysicsData.Length; j++)
                    {
                        if (!_tsumPhysicsData[j].IsActive)
                        {
                            continue;
                        }

                        if (_tsumPhysicsData[i].IsStatic && _tsumPhysicsData[j].IsStatic)
                        {
                            continue;
                        }

                        Vector2 diff = _tsumPhysicsData[i].Position - _tsumPhysicsData[j].Position;
                        float distanceSqr = diff.sqrMagnitude;
                        float minDistance = _tsumPhysicsData[i].Radius + _tsumPhysicsData[j].Radius;

                        if (distanceSqr < minDistance * minDistance && distanceSqr > 0.0001f)
                        {
                            float distance = Mathf.Sqrt(distanceSqr);
                            Vector2 normal = diff / distance;
                            float overlap = minDistance - distance;

                            float pushMagnitude = overlap * _physicsData.PushFactor;

                            if (pushMagnitude > _physicsData.MaxPushDistance)
                            {
                                pushMagnitude = _physicsData.MaxPushDistance;
                            }

                            Vector2 correction = normal * pushMagnitude;

                            if (!_tsumPhysicsData[i].IsStatic && !_tsumPhysicsData[j].IsStatic)
                            {
                                _tsumPhysicsData[i].Position += correction * 0.5f;
                                _tsumPhysicsData[j].Position -= correction * 0.5f;
                            }
                            else if (!_tsumPhysicsData[i].IsStatic && _tsumPhysicsData[j].IsStatic)
                            {
                                _tsumPhysicsData[i].Position += correction;
                            }
                            else if (_tsumPhysicsData[i].IsStatic && !_tsumPhysicsData[j].IsStatic)
                            {
                                _tsumPhysicsData[j].Position -= correction;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (!_tsumPhysicsData[i].IsActive)
                {
                    continue;
                }

                if (_tsumPhysicsData[i].IsStatic)
                {
                    continue;
                }

                float deltaX = _tsumPhysicsData[i].Position.x - _tsumPhysicsData[i].PreviousPosition.x;
                float angleChange = -(deltaX / _tsumPhysicsData[i].Radius) * Mathf.Rad2Deg;
                _tsumPhysicsData[i].Rotation += angleChange;
            }

            for (int i = 0; i < _tsumPhysicsData.Length; i++)
            {
                if (_tsumPhysicsData[i].IsActive)
                {
                    _tsumPhysicsData[i].RenderEndPosition = _tsumPhysicsData[i].Position;
                    _tsumPhysicsData[i].RenderEndRotation = _tsumPhysicsData[i].Rotation;
                }
            }
        }
    }
}