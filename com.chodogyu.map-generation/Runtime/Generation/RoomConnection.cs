namespace CDG.MapGeneration
{
    /// <summary>
    /// 두 Room 사이의 논리적 연결 후보를 나타냅니다.
    /// 실제 Corridor Cell 생성은 이후 단계에서 별도로 수행됩니다.
    /// </summary>
    internal sealed class RoomConnection
    {
        /// <summary>
        /// 연결되는 첫 번째 Room ID입니다.
        /// 항상 ToRoomId보다 작은 ID를 가집니다.
        /// </summary>
        internal int FromRoomId { get; }

        /// <summary>
        /// 연결되는 두 번째 Room ID입니다.
        /// 항상 FromRoomId보다 큰 ID를 가집니다.
        /// </summary>
        internal int ToRoomId { get; }

        /// <summary>
        /// 두 Room 중심 좌표 사이의 제곱 거리입니다.
        /// </summary>
        internal long DistanceSquared { get; }

        internal RoomConnection(int firstRoomId, int secondRoomId, long distanceSquared)
        {
            if (firstRoomId < secondRoomId)
            {
                FromRoomId = firstRoomId;
                ToRoomId = secondRoomId;
            }
            else
            {
                FromRoomId = secondRoomId;
                ToRoomId = firstRoomId;
            }

            DistanceSquared = distanceSquared;
        }
    }
}