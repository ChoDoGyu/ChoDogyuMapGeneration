using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class MapCellBuilderTests
    {
        [Test]
        public void Build_Room영역을Floor로생성한다()
        {
            MapRoom[] rooms = { new MapRoom(0, new RectInt(2, 2, 3, 3)) };

            IReadOnlyList<MapCellType> cells = MapCellBuilder.Build(8, 8, rooms, Array.Empty<MapCorridor>());

            Assert.That(GetCell(cells, 8, 2, 2), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 8, 3, 3), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 8, 4, 4), Is.EqualTo(MapCellType.Floor));
        }

        [Test]
        public void Build_Corridor영역을Floor로생성한다()
        {
            Vector2Int[] corridorCells =
            {
                new Vector2Int(1, 3),
                new Vector2Int(2, 3),
                new Vector2Int(3, 3)
            };

            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };

            IReadOnlyList<MapCellType> cells = MapCellBuilder.Build(6, 6, Array.Empty<MapRoom>(), corridors);

            Assert.That(GetCell(cells, 6, 1, 3), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 6, 2, 3), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 6, 3, 3), Is.EqualTo(MapCellType.Floor));
        }

        [Test]
        public void Build_Floor주변8방향EmptyCell을Wall로생성한다()
        {
            Vector2Int[] corridorCells = { new Vector2Int(2, 2) };
            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };

            IReadOnlyList<MapCellType> cells = MapCellBuilder.Build(5, 5, Array.Empty<MapRoom>(), corridors);

            Assert.That(GetCell(cells, 5, 2, 2), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 5, 1, 1), Is.EqualTo(MapCellType.Wall));
            Assert.That(GetCell(cells, 5, 2, 1), Is.EqualTo(MapCellType.Wall));
            Assert.That(GetCell(cells, 5, 3, 3), Is.EqualTo(MapCellType.Wall));
        }

        [Test]
        public void Build_Floor와Wall이아닌영역은Empty로유지한다()
        {
            MapRoom[] rooms = { new MapRoom(0, new RectInt(1, 1, 2, 2)) };

            IReadOnlyList<MapCellType> cells = MapCellBuilder.Build(8, 8, rooms, Array.Empty<MapCorridor>());

            Assert.That(GetCell(cells, 8, 7, 7), Is.EqualTo(MapCellType.Empty));
        }

        [Test]
        public void Build_Room과Corridor가겹쳐도Floor가유지된다()
        {
            MapRoom[] rooms = { new MapRoom(0, new RectInt(2, 2, 3, 3)) };
            Vector2Int[] corridorCells = { new Vector2Int(3, 3), new Vector2Int(4, 3), new Vector2Int(5, 3) };
            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };

            IReadOnlyList<MapCellType> cells = MapCellBuilder.Build(8, 8, rooms, corridors);

            Assert.That(GetCell(cells, 8, 3, 3), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 8, 4, 3), Is.EqualTo(MapCellType.Floor));
            Assert.That(GetCell(cells, 8, 5, 3), Is.EqualTo(MapCellType.Floor));
        }

        [Test]
        public void Build_MapBounds를벗어난Corridor가있으면예외가발생한다()
        {
            Vector2Int[] corridorCells = { new Vector2Int(5, 2) };
            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };

            Assert.Throws<ArgumentException>(() => MapCellBuilder.Build(5, 5, Array.Empty<MapRoom>(), corridors));
        }

        private static MapCellType GetCell(IReadOnlyList<MapCellType> cells, int width, int x, int y)
        {
            return cells[y * width + x];
        }
    }
}