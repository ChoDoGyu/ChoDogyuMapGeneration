using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 한 번의 맵 생성 결과를 표현하는 논리적 데이터입니다.
    /// Grid Cell 상태와 Room, Corridor 정보를 포함하며 Unity Scene 표현과는 독립적입니다.
    /// </summary>
    public sealed class MapData
    {
        private readonly MapCellType[] cells;
        private readonly ReadOnlyCollection<MapRoom> rooms;
        private readonly ReadOnlyCollection<MapCorridor> corridors;

        /// <summary>
        /// Grid의 가로 Cell 개수입니다.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Grid의 세로 Cell 개수입니다.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 이 맵을 생성할 때 사용된 Seed입니다.
        /// </summary>
        public int Seed { get; }

        /// <summary>
        /// 생성된 방의 읽기 전용 목록입니다.
        /// </summary>
        public IReadOnlyList<MapRoom> Rooms => rooms;

        /// <summary>
        /// 생성된 복도의 읽기 전용 목록입니다.
        /// </summary>
        public IReadOnlyList<MapCorridor> Corridors => corridors;

        internal MapData(int width, int height, int seed, IReadOnlyList<MapCellType> cells, IReadOnlyList<MapRoom> rooms, IReadOnlyList<MapCorridor> corridors)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Map Width는 0보다 커야 합니다.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Map Height는 0보다 커야 합니다.");
            }

            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            if (corridors == null)
            {
                throw new ArgumentNullException(nameof(corridors));
            }

            int expectedCellCount = checked(width * height);

            if (cells.Count != expectedCellCount)
            {
                throw new ArgumentException(
                    $"Cell 개수는 Width × Height와 같아야 합니다. Expected: {expectedCellCount}, Actual: {cells.Count}",
                    nameof(cells));
            }

            Width = width;
            Height = height;
            Seed = seed;

            this.cells = new MapCellType[expectedCellCount];

            for (int i = 0; i < expectedCellCount; i++)
            {
                this.cells[i] = cells[i];
            }

            this.rooms = new List<MapRoom>(rooms).AsReadOnly();
            this.corridors = new List<MapCorridor>(corridors).AsReadOnly();
        }

        /// <summary>
        /// 지정한 Grid 좌표가 현재 맵 범위 안에 포함되는지 확인합니다.
        /// </summary>
        public bool IsInBounds(Vector2Int position)
        {
            return position.x >= 0
                && position.x < Width
                && position.y >= 0
                && position.y < Height;
        }

        /// <summary>
        /// 지정한 Grid 좌표의 Cell 상태를 반환합니다.
        /// 맵 범위를 벗어난 좌표를 전달하면 예외가 발생합니다.
        /// </summary>
        public MapCellType GetCell(Vector2Int position)
        {
            if (!IsInBounds(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "지정한 좌표가 Map Bounds를 벗어났습니다.");
            }

            return cells[ToIndex(position.x, position.y)];
        }

        /// <summary>
        /// 지정한 Grid 좌표의 Cell 상태를 반환합니다.
        /// 맵 범위를 벗어난 좌표를 전달하면 예외가 발생합니다.
        /// </summary>
        public MapCellType GetCell(int x, int y)
        {
            return GetCell(new Vector2Int(x, y));
        }

        private int ToIndex(int x, int y)
        {
            return y * Width + x;
        }
    }
}