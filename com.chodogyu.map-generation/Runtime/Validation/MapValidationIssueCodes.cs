namespace CDG.MapGeneration
{
    /// <summary>
    /// Map Validation에서 사용하는 안정적인 Issue Code 모음입니다.
    /// </summary>
    public static class MapValidationIssueCodes
    {
        public const string NullMapData = "MAP_VALIDATION_NULL_DATA";
        public const string InvalidCellType = "MAP_VALIDATION_INVALID_CELL_TYPE";
        public const string NullRoom = "MAP_VALIDATION_NULL_ROOM";
        public const string DuplicateRoomId = "MAP_VALIDATION_DUPLICATE_ROOM_ID";
        public const string RoomOutOfBounds = "MAP_VALIDATION_ROOM_OUT_OF_BOUNDS";
        public const string RoomCenterNotFloor = "MAP_VALIDATION_ROOM_CENTER_NOT_FLOOR";
        public const string NullCorridor = "MAP_VALIDATION_NULL_CORRIDOR";
        public const string InvalidCorridorRoomReference = "MAP_VALIDATION_INVALID_CORRIDOR_ROOM_REFERENCE";
        public const string CorridorOutOfBounds = "MAP_VALIDATION_CORRIDOR_OUT_OF_BOUNDS";
        public const string CorridorCellNotFloor = "MAP_VALIDATION_CORRIDOR_CELL_NOT_FLOOR";
        public const string CorridorNotContinuous = "MAP_VALIDATION_CORRIDOR_NOT_CONTINUOUS";
        public const string CorridorEndpointMismatch = "MAP_VALIDATION_CORRIDOR_ENDPOINT_MISMATCH";
        public const string RoomGraphDisconnected = "MAP_VALIDATION_ROOM_GRAPH_DISCONNECTED";
        public const string FloorDisconnected = "MAP_VALIDATION_FLOOR_DISCONNECTED";
    }
}