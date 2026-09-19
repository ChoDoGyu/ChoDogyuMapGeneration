using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class MapDataModelTests
    {
        [Test]
        public void MapRoom_유효한Bounds로생성하면중심좌표를계산한다()
        {
            MapRoom room = new MapRoom(0, new RectInt(10, 20, 6, 4));

            Assert.That(room.Id, Is.EqualTo(0));
            Assert.That(room.Bounds, Is.EqualTo(new RectInt(10, 20, 6, 4)));
            Assert.That(room.Center, Is.EqualTo(new Vector2Int(13, 22)));
        }

        [Test]
        public void MapRoom_음수Id를전달하면예외가발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MapRoom(-1, new RectInt(0, 0, 5, 5)));
        }

        [Test]
        public void MapRoom_유효하지않은크기를전달하면예외가발생한다()
        {
            Assert.Throws<ArgumentException>(() => new MapRoom(0, new RectInt(0, 0, 0, 5)));
        }

        [Test]
        public void MapCorridor_생성후원본Cell목록을변경해도내부데이터는변하지않는다()
        {
            List<Vector2Int> sourceCells = new List<Vector2Int>
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1)
            };

            MapCorridor corridor = new MapCorridor(0, 1, sourceCells);

            sourceCells.Add(new Vector2Int(3, 1));

            Assert.That(corridor.Cells.Count, Is.EqualTo(2));
            Assert.That(corridor.Cells[0], Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(corridor.Cells[1], Is.EqualTo(new Vector2Int(2, 1)));
        }

        [Test]
        public void MapCorridor_동일한Room을연결하면예외가발생한다()
        {
            Vector2Int[] cells =
            {
                new Vector2Int(1, 1)
            };

            Assert.Throws<ArgumentException>(() => new MapCorridor(0, 0, cells));
        }

        [Test]
        public void MapData_생성하면기본정보와Cell을조회할수있다()
        {
            MapCellType[] cells =
            {
                MapCellType.Empty,
                MapCellType.Floor,
                MapCellType.Wall,
                MapCellType.Floor,

                MapCellType.Wall,
                MapCellType.Empty,
                MapCellType.Floor,
                MapCellType.Wall
            };

            MapData mapData = new MapData(4, 2, 12345, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());

            Assert.That(mapData.Width, Is.EqualTo(4));
            Assert.That(mapData.Height, Is.EqualTo(2));
            Assert.That(mapData.Seed, Is.EqualTo(12345));

            Assert.That(mapData.GetCell(0, 0), Is.EqualTo(MapCellType.Empty));
            Assert.That(mapData.GetCell(1, 0), Is.EqualTo(MapCellType.Floor));
            Assert.That(mapData.GetCell(2, 0), Is.EqualTo(MapCellType.Wall));
            Assert.That(mapData.GetCell(2, 1), Is.EqualTo(MapCellType.Floor));
        }

        [Test]
        public void MapData_IsInBounds는Map경계를정확하게판단한다()
        {
            MapData mapData = CreateEmptyMap(4, 3);

            Assert.That(mapData.IsInBounds(new Vector2Int(0, 0)), Is.True);
            Assert.That(mapData.IsInBounds(new Vector2Int(3, 2)), Is.True);

            Assert.That(mapData.IsInBounds(new Vector2Int(-1, 0)), Is.False);
            Assert.That(mapData.IsInBounds(new Vector2Int(0, -1)), Is.False);
            Assert.That(mapData.IsInBounds(new Vector2Int(4, 0)), Is.False);
            Assert.That(mapData.IsInBounds(new Vector2Int(0, 3)), Is.False);
        }

        [Test]
        public void MapData_Map범위를벗어난Cell조회시예외가발생한다()
        {
            MapData mapData = CreateEmptyMap(4, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => mapData.GetCell(4, 0));
        }

        [Test]
        public void MapData_잘못된Cell개수를전달하면예외가발생한다()
        {
            MapCellType[] cells =
            {
                MapCellType.Empty
            };

            Assert.Throws<ArgumentException>(() => new MapData(2, 2, 0, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>()));
        }

        [Test]
        public void MapData_생성후원본Cell배열을변경해도MapData는변하지않는다()
        {
            MapCellType[] cells =
            {
                MapCellType.Empty,
                MapCellType.Empty,
                MapCellType.Empty,
                MapCellType.Empty
            };

            MapData mapData = new MapData(2, 2, 0, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());

            cells[0] = MapCellType.Floor;

            Assert.That(mapData.GetCell(0, 0), Is.EqualTo(MapCellType.Empty));
        }

        private static MapData CreateEmptyMap(int width, int height)
        {
            MapCellType[] cells = new MapCellType[width * height];

            return new MapData(width, height, 0, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());
        }
    }
}