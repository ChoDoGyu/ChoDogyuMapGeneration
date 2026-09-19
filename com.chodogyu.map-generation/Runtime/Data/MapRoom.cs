using System;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 생성된 맵에 배치된 하나의 직사각형 방 정보를 나타냅니다.
    /// 방의 식별자와 Grid 영역, 대표 중심 좌표를 제공합니다.
    /// </summary>
    public sealed class MapRoom
    {
        /// <summary>
        /// 생성 과정에서 부여된 방의 고유 식별자입니다.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 방이 차지하는 Grid 영역입니다.
        /// RectInt의 xMax와 yMax는 포함되지 않습니다.
        /// </summary>
        public RectInt Bounds { get; }

        /// <summary>
        /// 방을 대표하는 중심 Cell 좌표입니다.
        /// 짝수 크기에서는 양의 축 방향에 가까운 Cell이 선택됩니다.
        /// </summary>
        public Vector2Int Center { get; }

        internal MapRoom(int id, RectInt bounds)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Room ID는 0 이상이어야 합니다.");
            }

            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentException("Room Bounds의 Width와 Height는 0보다 커야 합니다.", nameof(bounds));
            }

            Id = id;
            Bounds = bounds;
            Center = new Vector2Int(bounds.xMin + bounds.width / 2, bounds.yMin + bounds.height / 2);
        }
    }
}