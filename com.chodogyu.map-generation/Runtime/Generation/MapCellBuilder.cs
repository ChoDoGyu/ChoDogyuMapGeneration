using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Room과 Corridor 정보를 기반으로 Empty, Floor, Wall 상태의 Grid Cell 배열을 생성합니다.
    /// Room과 Corridor를 먼저 Floor로 표시한 뒤 Floor 주변의 Empty Cell을 Wall로 변환합니다.
    /// </summary>
    internal static class MapCellBuilder
    {
        private static readonly Vector2Int[] WallDirections =
        {
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };

        internal static IReadOnlyList<MapCellType> Build(int width, int height, IReadOnlyList<MapRoom> rooms,
            IReadOnlyList<MapCorridor> corridors)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width는 0보다 커야 합니다.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height는 0보다 커야 합니다.");
            }

            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            if (corridors == null)
            {
                throw new ArgumentNullException(nameof(corridors));
            }

            MapCellType[] cells = new MapCellType[checked(width * height)];

            MarkRooms(cells, width, height, rooms);
            MarkCorridors(cells, width, height, corridors);
            MarkWalls(cells, width, height);

            return Array.AsReadOnly(cells);
        }

        private static void MarkRooms(MapCellType[] cells, int width, int height, IReadOnlyList<MapRoom> rooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                MapRoom room = rooms[i];

                if (room == null)
                {
                    throw new ArgumentException("Room 목록에는 null이 포함될 수 없습니다.", nameof(rooms));
                }

                for (int y = room.Bounds.yMin; y < room.Bounds.yMax; y++)
                {
                    for (int x = room.Bounds.xMin; x < room.Bounds.xMax; x++)
                    {
                        ValidatePosition(x, y, width, height, nameof(rooms));
                        cells[ToIndex(x, y, width)] = MapCellType.Floor;
                    }
                }
            }
        }

        private static void MarkCorridors(MapCellType[] cells, int width, int height, IReadOnlyList<MapCorridor> corridors)
        {
            for (int i = 0; i < corridors.Count; i++)
            {
                MapCorridor corridor = corridors[i];

                if (corridor == null)
                {
                    throw new ArgumentException("Corridor 목록에는 null이 포함될 수 없습니다.", nameof(corridors));
                }

                for (int j = 0; j < corridor.Cells.Count; j++)
                {
                    Vector2Int position = corridor.Cells[j];
                    ValidatePosition(position.x, position.y, width, height, nameof(corridors));
                    cells[ToIndex(position.x, position.y, width)] = MapCellType.Floor;
                }
            }
        }

        private static void MarkWalls(MapCellType[] cells, int width, int height)
        {
            List<int> wallIndices = new List<int>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cells[ToIndex(x, y, width)] != MapCellType.Floor)
                    {
                        continue;
                    }

                    for (int i = 0; i < WallDirections.Length; i++)
                    {
                        int wallX = x + WallDirections[i].x;
                        int wallY = y + WallDirections[i].y;

                        if (!IsInBounds(wallX, wallY, width, height))
                        {
                            continue;
                        }

                        int index = ToIndex(wallX, wallY, width);

                        if (cells[index] == MapCellType.Empty)
                        {
                            wallIndices.Add(index);
                        }
                    }
                }
            }

            for (int i = 0; i < wallIndices.Count; i++)
            {
                if (cells[wallIndices[i]] == MapCellType.Empty)
                {
                    cells[wallIndices[i]] = MapCellType.Wall;
                }
            }
        }

        private static void ValidatePosition(int x, int y, int width, int height, string parameterName)
        {
            if (!IsInBounds(x, y, width, height))
            {
                throw new ArgumentException($"Cell ({x}, {y})이 Map Bounds를 벗어났습니다.", parameterName);
            }
        }

        private static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private static int ToIndex(int x, int y, int width)
        {
            return y * width + x;
        }
    }
}