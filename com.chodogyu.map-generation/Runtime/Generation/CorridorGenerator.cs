using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Room 연결 정보를 기반으로 수평과 수직 이동만 사용하는 직교 복도를 생성합니다.
    /// 각 복도는 연결된 두 Room의 중심 Cell을 포함하며 결정적 난수로 이동 축의 우선순위를 선택합니다.
    /// </summary>
    internal static class CorridorGenerator
    {
        internal static IReadOnlyList<MapCorridor> Generate(IReadOnlyList<MapRoom> rooms,
            IReadOnlyList<RoomConnection> connections, DeterministicRandom random)
        {
            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            Dictionary<int, MapRoom> roomLookup = CreateRoomLookup(rooms);
            List<MapCorridor> corridors = new List<MapCorridor>(connections.Count);

            for (int i = 0; i < connections.Count; i++)
            {
                RoomConnection connection = connections[i];

                if (connection == null)
                {
                    throw new ArgumentException("Connection 목록에는 null이 포함될 수 없습니다.", nameof(connections));
                }

                if (!roomLookup.TryGetValue(connection.FromRoomId, out MapRoom fromRoom) ||
                    !roomLookup.TryGetValue(connection.ToRoomId, out MapRoom toRoom))
                {
                    throw new ArgumentException("Connection이 존재하지 않는 Room ID를 참조합니다.", nameof(connections));
                }

                List<Vector2Int> cells = CreateCorridorCells(fromRoom.Center, toRoom.Center, random);
                corridors.Add(new MapCorridor(connection.FromRoomId, connection.ToRoomId, cells));
            }

            return corridors.AsReadOnly();
        }

        private static Dictionary<int, MapRoom> CreateRoomLookup(IReadOnlyList<MapRoom> rooms)
        {
            Dictionary<int, MapRoom> lookup = new Dictionary<int, MapRoom>(rooms.Count);

            for (int i = 0; i < rooms.Count; i++)
            {
                MapRoom room = rooms[i];

                if (room == null)
                {
                    throw new ArgumentException("Room 목록에는 null이 포함될 수 없습니다.", nameof(rooms));
                }

                if (!lookup.TryAdd(room.Id, room))
                {
                    throw new ArgumentException($"중복된 Room ID가 존재합니다. ID: {room.Id}", nameof(rooms));
                }
            }

            return lookup;
        }

        private static List<Vector2Int> CreateCorridorCells(Vector2Int start, Vector2Int end, DeterministicRandom random)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            cells.Add(start);

            bool horizontalFirst = random.NextInt(0, 2) == 0;

            if (horizontalFirst)
            {
                AppendHorizontal(cells, start.x, end.x, start.y);
                AppendVertical(cells, start.y, end.y, end.x);
            }
            else
            {
                AppendVertical(cells, start.y, end.y, start.x);
                AppendHorizontal(cells, start.x, end.x, end.y);
            }

            return cells;
        }

        private static void AppendHorizontal(List<Vector2Int> cells, int startX, int endX, int y)
        {
            int direction = Math.Sign(endX - startX);

            for (int x = startX + direction; direction != 0 && x != endX + direction; x += direction)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }

        private static void AppendVertical(List<Vector2Int> cells, int startY, int endY, int x)
        {
            int direction = Math.Sign(endY - startY);

            for (int y = startY + direction; direction != 0 && y != endY + direction; y += direction)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }
    }
}