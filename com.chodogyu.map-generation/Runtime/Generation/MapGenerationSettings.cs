using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Room + Corridor 맵 생성에 사용하는 설정값을 정의합니다.
    /// 설정의 유효성 검사는 생성 시작 전에 별도로 수행됩니다.
    /// </summary>
    public sealed class MapGenerationSettings
    {
        /// <summary>
        /// 기본 생성 설정의 새 인스턴스를 반환합니다.
        /// </summary>
        public static MapGenerationSettings Default =>
            new MapGenerationSettings(64, 64, 12, new Vector2Int(5, 5), new Vector2Int(10, 10), 1, 1, 64, 2);

        /// <summary>
        /// 생성할 Grid의 가로 Cell 개수입니다.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// 생성할 Grid의 세로 Cell 개수입니다.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 생성할 방의 개수입니다.
        /// </summary>
        public int RoomCount { get; }

        /// <summary>
        /// 방의 최소 가로 및 세로 크기입니다.
        /// </summary>
        public Vector2Int MinRoomSize { get; }

        /// <summary>
        /// 방의 최대 가로 및 세로 크기입니다.
        /// </summary>
        public Vector2Int MaxRoomSize { get; }

        /// <summary>
        /// 서로 다른 방 사이에 확보할 최소 Cell 간격입니다.
        /// </summary>
        public int RoomPadding { get; }

        /// <summary>
        /// 방과 맵 외곽 사이에 확보할 최소 Cell 간격입니다.
        /// </summary>
        public int EdgePadding { get; }

        /// <summary>
        /// 하나의 방을 배치하기 위해 허용하는 최대 위치 선택 시도 횟수입니다.
        /// </summary>
        public int MaxPlacementAttemptsPerRoom { get; }

        /// <summary>
        /// 모든 방의 기본 연결이 완료된 뒤 추가할 연결 개수입니다.
        /// </summary>
        public int ExtraConnectionCount { get; }

        /// <summary>
        /// 지정한 값으로 맵 생성 설정을 생성합니다.
        /// 잘못된 값 자체의 저장은 허용하며 실제 생성 전 Validation에서 검사합니다.
        /// </summary>
        public MapGenerationSettings(int width, int height, int roomCount, Vector2Int minRoomSize, Vector2Int maxRoomSize,
            int roomPadding, int edgePadding, int maxPlacementAttemptsPerRoom, int extraConnectionCount)
        {
            Width = width;
            Height = height;
            RoomCount = roomCount;
            MinRoomSize = minRoomSize;
            MaxRoomSize = maxRoomSize;
            RoomPadding = roomPadding;
            EdgePadding = edgePadding;
            MaxPlacementAttemptsPerRoom = maxPlacementAttemptsPerRoom;
            ExtraConnectionCount = extraConnectionCount;
        }
    }
}