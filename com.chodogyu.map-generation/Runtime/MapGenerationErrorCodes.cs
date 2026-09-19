namespace CDG.MapGeneration
{
    /// <summary>
    /// Map Generation Framework에서 사용하는 안정적인 오류 코드 모음입니다.
    /// 외부 코드에서는 오류 메시지보다 오류 코드를 기준으로 실패 원인을 구분할 수 있습니다.
    /// </summary>
    public static class MapGenerationErrorCodes
    {
        /// <summary>
        /// 맵 생성 설정에 유효하지 않은 값이 포함되어 있을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string InvalidSettings = "MAP_INVALID_SETTINGS";

        /// <summary>
        /// 제한된 배치 시도 횟수 안에 요청한 방을 모두 배치하지 못했을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string RoomPlacementFailed = "MAP_ROOM_PLACEMENT_FAILED";

        /// <summary>
        /// Visualization에 전달된 MapData가 유효하지 않을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string VisualizerInvalidData = "MAP_VISUALIZER_INVALID_DATA";

        /// <summary>
        /// PrefabMapVisualizer에 Theme이 지정되지 않았을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string VisualizerThemeMissing = "MAP_VISUALIZER_THEME_MISSING";

        /// <summary>
        /// Theme에 필요한 Prefab이 지정되지 않았을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string VisualizerInvalidTheme = "MAP_VISUALIZER_INVALID_THEME";
    }
}