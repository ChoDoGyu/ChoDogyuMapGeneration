using System.Collections.Generic;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 생성된 MapData의 Room, Corridor, Cell 구조와 연결 상태를 검증합니다.
    /// 검증 과정에서는 MapData를 변경하지 않습니다.
    /// </summary>
    public static class MapValidator
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up
        };

        /// <summary>
        /// 지정한 MapData를 검증하고 발견된 문제를 Report로 반환합니다.
        /// null도 예외 대신 Validation Issue로 반환합니다.
        /// </summary>
        public static MapValidationReport Validate(MapData mapData)
        {
            List<MapValidationIssue> issues = new List<MapValidationIssue>();

            if (mapData == null)
            {
                AddError(issues, MapValidationIssueCodes.NullMapData, "MapData가 null입니다.");
                return new MapValidationReport(issues);
            }

            ValidateCellTypes(mapData, issues);

            HashSet<int> roomIds = new HashSet<int>();
            Dictionary<int, MapRoom> roomLookup = new Dictionary<int, MapRoom>();
            bool roomIdentityValid = ValidateRooms(mapData, issues, roomIds, roomLookup);

            ValidateCorridors(mapData, issues, roomLookup);

            if (roomIdentityValid)
            {
                ValidateRoomConnectivity(mapData, issues, roomIds);
            }

            ValidateFloorConnectivity(mapData, issues);

            return new MapValidationReport(issues);
        }

        private static void ValidateCellTypes(MapData mapData, List<MapValidationIssue> issues)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    MapCellType cell = mapData.GetCell(x, y);

                    if (cell != MapCellType.Empty && cell != MapCellType.Floor && cell != MapCellType.Wall)
                    {
                        AddError(issues, MapValidationIssueCodes.InvalidCellType,
                            $"Cell ({x}, {y})에 유효하지 않은 Cell Type 값 {(int)cell}이 있습니다.");
                    }
                }
            }
        }

        private static bool ValidateRooms(MapData mapData, List<MapValidationIssue> issues, HashSet<int> roomIds,
            Dictionary<int, MapRoom> roomLookup)
        {
            bool identityValid = true;

            for (int i = 0; i < mapData.Rooms.Count; i++)
            {
                MapRoom room = mapData.Rooms[i];

                if (room == null)
                {
                    AddError(issues, MapValidationIssueCodes.NullRoom, $"Room 목록의 Index {i}가 null입니다.");
                    identityValid = false;
                    continue;
                }

                if (!roomIds.Add(room.Id))
                {
                    AddError(issues, MapValidationIssueCodes.DuplicateRoomId, $"중복된 Room ID가 존재합니다. ID: {room.Id}");
                    identityValid = false;
                    continue;
                }

                roomLookup.Add(room.Id, room);

                if (!IsRoomInBounds(mapData, room))
                {
                    AddError(issues, MapValidationIssueCodes.RoomOutOfBounds,
                        $"Room {room.Id}의 Bounds가 Map Bounds를 벗어났습니다.");
                    continue;
                }

                if (mapData.GetCell(room.Center) != MapCellType.Floor)
                {
                    AddError(issues, MapValidationIssueCodes.RoomCenterNotFloor,
                        $"Room {room.Id}의 Center {room.Center}가 Floor가 아닙니다.");
                }
            }

            return identityValid;
        }

        private static void ValidateCorridors(MapData mapData, List<MapValidationIssue> issues,
            Dictionary<int, MapRoom> roomLookup)
        {
            for (int i = 0; i < mapData.Corridors.Count; i++)
            {
                MapCorridor corridor = mapData.Corridors[i];

                if (corridor == null)
                {
                    AddError(issues, MapValidationIssueCodes.NullCorridor, $"Corridor 목록의 Index {i}가 null입니다.");
                    continue;
                }

                bool hasFromRoom = roomLookup.TryGetValue(corridor.FromRoomId, out MapRoom fromRoom);
                bool hasToRoom = roomLookup.TryGetValue(corridor.ToRoomId, out MapRoom toRoom);

                if (!hasFromRoom || !hasToRoom)
                {
                    AddError(issues, MapValidationIssueCodes.InvalidCorridorRoomReference,
                        $"Corridor {corridor.FromRoomId} → {corridor.ToRoomId}가 존재하지 않는 Room을 참조합니다.");
                }
                else if (corridor.Cells[0] != fromRoom.Center ||
                    corridor.Cells[corridor.Cells.Count - 1] != toRoom.Center)
                {
                    AddError(issues, MapValidationIssueCodes.CorridorEndpointMismatch,
                        $"Corridor {corridor.FromRoomId} → {corridor.ToRoomId}의 시작 또는 끝 Cell이 Room Center와 일치하지 않습니다.");
                }

                ValidateCorridorCells(mapData, issues, corridor);
            }
        }

        private static void ValidateCorridorCells(MapData mapData, List<MapValidationIssue> issues, MapCorridor corridor)
        {
            for (int i = 0; i < corridor.Cells.Count; i++)
            {
                Vector2Int position = corridor.Cells[i];

                if (!mapData.IsInBounds(position))
                {
                    AddError(issues, MapValidationIssueCodes.CorridorOutOfBounds,
                        $"Corridor {corridor.FromRoomId} → {corridor.ToRoomId}의 Cell {position}이 Map Bounds를 벗어났습니다.");
                    continue;
                }

                if (mapData.GetCell(position) != MapCellType.Floor)
                {
                    AddError(issues, MapValidationIssueCodes.CorridorCellNotFloor,
                        $"Corridor {corridor.FromRoomId} → {corridor.ToRoomId}의 Cell {position}이 Floor가 아닙니다.");
                }

                if (i == 0)
                {
                    continue;
                }

                Vector2Int previous = corridor.Cells[i - 1];
                int distance = Mathf.Abs(position.x - previous.x) + Mathf.Abs(position.y - previous.y);

                if (distance != 1)
                {
                    AddError(issues, MapValidationIssueCodes.CorridorNotContinuous,
                        $"Corridor {corridor.FromRoomId} → {corridor.ToRoomId}의 Cell이 연속되지 않습니다.");
                    break;
                }
            }
        }

        private static void ValidateRoomConnectivity(MapData mapData, List<MapValidationIssue> issues, HashSet<int> roomIds)
        {
            if (roomIds.Count <= 1)
            {
                return;
            }

            Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();

            foreach (int roomId in roomIds)
            {
                adjacency.Add(roomId, new List<int>());
            }

            for (int i = 0; i < mapData.Corridors.Count; i++)
            {
                MapCorridor corridor = mapData.Corridors[i];

                if (corridor == null || !adjacency.ContainsKey(corridor.FromRoomId) || !adjacency.ContainsKey(corridor.ToRoomId))
                {
                    continue;
                }

                adjacency[corridor.FromRoomId].Add(corridor.ToRoomId);
                adjacency[corridor.ToRoomId].Add(corridor.FromRoomId);
            }

            Queue<int> queue = new Queue<int>();
            HashSet<int> visited = new HashSet<int>();

            foreach (int roomId in roomIds)
            {
                queue.Enqueue(roomId);
                visited.Add(roomId);
                break;
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                List<int> neighbors = adjacency[current];

                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (visited.Add(neighbors[i]))
                    {
                        queue.Enqueue(neighbors[i]);
                    }
                }
            }

            if (visited.Count != roomIds.Count)
            {
                AddError(issues, MapValidationIssueCodes.RoomGraphDisconnected,
                    $"모든 Room이 연결되어 있지 않습니다. Connected: {visited.Count}, Total: {roomIds.Count}");
            }
        }

        private static void ValidateFloorConnectivity(MapData mapData, List<MapValidationIssue> issues)
        {
            Vector2Int start = default;
            int floorCount = 0;
            bool hasStart = false;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    if (mapData.GetCell(x, y) != MapCellType.Floor)
                    {
                        continue;
                    }

                    floorCount++;

                    if (!hasStart)
                    {
                        start = new Vector2Int(x, y);
                        hasStart = true;
                    }
                }
            }

            if (floorCount <= 1)
            {
                return;
            }

            bool[] visited = new bool[checked(mapData.Width * mapData.Height)];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            int visitedFloorCount = 0;

            queue.Enqueue(start);
            visited[ToIndex(start.x, start.y, mapData.Width)] = true;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                visitedFloorCount++;

                for (int i = 0; i < CardinalDirections.Length; i++)
                {
                    Vector2Int next = current + CardinalDirections[i];

                    if (!mapData.IsInBounds(next) || mapData.GetCell(next) != MapCellType.Floor)
                    {
                        continue;
                    }

                    int index = ToIndex(next.x, next.y, mapData.Width);

                    if (visited[index])
                    {
                        continue;
                    }

                    visited[index] = true;
                    queue.Enqueue(next);
                }
            }

            if (visitedFloorCount != floorCount)
            {
                AddError(issues, MapValidationIssueCodes.FloorDisconnected,
                    $"Floor 영역이 하나로 연결되어 있지 않습니다. Connected: {visitedFloorCount}, Total: {floorCount}");
            }
        }

        private static bool IsRoomInBounds(MapData mapData, MapRoom room)
        {
            return room.Bounds.xMin >= 0 && room.Bounds.yMin >= 0 &&
                room.Bounds.xMax <= mapData.Width && room.Bounds.yMax <= mapData.Height;
        }

        private static int ToIndex(int x, int y, int width)
        {
            return y * width + x;
        }

        private static void AddError(List<MapValidationIssue> issues, string code, string message)
        {
            issues.Add(new MapValidationIssue(code, message, MapValidationSeverity.Error));
        }
    }
}