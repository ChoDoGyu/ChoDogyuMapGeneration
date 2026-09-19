using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 두 방을 연결하는 하나의 복도 정보를 나타냅니다.
    /// 연결 대상 Room ID와 복도가 통과하는 Grid Cell 목록을 제공합니다.
    /// </summary>
    public sealed class MapCorridor
    {
        private readonly ReadOnlyCollection<Vector2Int> cells;

        /// <summary>
        /// 복도가 시작되는 방의 식별자입니다.
        /// </summary>
        public int FromRoomId { get; }

        /// <summary>
        /// 복도가 연결되는 방의 식별자입니다.
        /// </summary>
        public int ToRoomId { get; }

        /// <summary>
        /// 복도가 통과하는 Cell 좌표의 읽기 전용 목록입니다.
        /// </summary>
        public IReadOnlyList<Vector2Int> Cells => cells;

        internal MapCorridor(int fromRoomId, int toRoomId, IReadOnlyList<Vector2Int> cells)
        {
            if (fromRoomId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fromRoomId), fromRoomId, "From Room ID는 0 이상이어야 합니다.");
            }

            if (toRoomId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(toRoomId), toRoomId, "To Room ID는 0 이상이어야 합니다.");
            }

            if (fromRoomId == toRoomId)
            {
                throw new ArgumentException("복도는 서로 다른 두 방을 연결해야 합니다.");
            }

            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            if (cells.Count == 0)
            {
                throw new ArgumentException("복도에는 최소 하나 이상의 Cell이 필요합니다.", nameof(cells));
            }

            FromRoomId = fromRoomId;
            ToRoomId = toRoomId;
            this.cells = new List<Vector2Int>(cells).AsReadOnly();
        }
    }
}