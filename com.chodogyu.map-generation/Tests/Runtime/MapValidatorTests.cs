using System;
using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class MapValidatorTests
    {
        [Test]
        public void Validate_정상생성된Map은유효하다()
        {
            Result<MapData> generation = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);

            Assert.That(generation.IsSuccess, Is.True);

            MapValidationReport report = MapValidator.Validate(generation.Value);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void Validate_nullMapData는실패한다()
        {
            MapValidationReport report = MapValidator.Validate(null);

            AssertIssue(report, MapValidationIssueCodes.NullMapData);
        }

        [Test]
        public void Validate_유효하지않은CellType을검출한다()
        {
            MapCellType[] cells = { (MapCellType)999 };
            MapData mapData = new MapData(1, 1, 0, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.InvalidCellType);
        }

        [Test]
        public void Validate_Map밖의Room을검출한다()
        {
            MapCellType[] cells = new MapCellType[25];
            MapRoom[] rooms = { new MapRoom(0, new RectInt(4, 4, 2, 2)) };
            MapData mapData = new MapData(5, 5, 0, cells, rooms, Array.Empty<MapCorridor>());

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.RoomOutOfBounds);
        }

        [Test]
        public void Validate_RoomCenter가Floor가아니면검출한다()
        {
            MapCellType[] cells = new MapCellType[25];
            MapRoom[] rooms = { new MapRoom(0, new RectInt(1, 1, 2, 2)) };
            MapData mapData = new MapData(5, 5, 0, cells, rooms, Array.Empty<MapCorridor>());

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.RoomCenterNotFloor);
        }

        [Test]
        public void Validate_존재하지않는Room을참조하는Corridor를검출한다()
        {
            MapCellType[] cells = CreateFilledCells(5, 5, MapCellType.Floor);
            MapRoom[] rooms = { new MapRoom(0, new RectInt(0, 0, 2, 2)) };
            Vector2Int[] corridorCells = { new Vector2Int(1, 1), new Vector2Int(2, 1) };
            MapCorridor[] corridors = { new MapCorridor(0, 99, corridorCells) };
            MapData mapData = new MapData(5, 5, 0, cells, rooms, corridors);

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.InvalidCorridorRoomReference);
        }

        [Test]
        public void Validate_Map밖의CorridorCell을검출한다()
        {
            MapCellType[] cells = CreateFilledCells(5, 5, MapCellType.Floor);
            MapRoom[] rooms =
            {
                new MapRoom(0, new RectInt(0, 0, 2, 2)),
                new MapRoom(1, new RectInt(3, 3, 2, 2))
            };
            Vector2Int[] corridorCells = { new Vector2Int(1, 1), new Vector2Int(5, 1), new Vector2Int(4, 4) };
            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };
            MapData mapData = new MapData(5, 5, 0, cells, rooms, corridors);

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.CorridorOutOfBounds);
        }

        [Test]
        public void Validate_연속되지않은Corridor를검출한다()
        {
            MapCellType[] cells = CreateFilledCells(6, 6, MapCellType.Floor);
            MapRoom[] rooms =
            {
                new MapRoom(0, new RectInt(0, 0, 2, 2)),
                new MapRoom(1, new RectInt(4, 4, 2, 2))
            };
            Vector2Int[] corridorCells = { new Vector2Int(1, 1), new Vector2Int(3, 1), new Vector2Int(5, 5) };
            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };
            MapData mapData = new MapData(6, 6, 0, cells, rooms, corridors);

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.CorridorNotContinuous);
        }

        [Test]
        public void Validate_연결되지않은RoomGraph를검출한다()
        {
            MapCellType[] cells = CreateFilledCells(8, 8, MapCellType.Floor);
            MapRoom[] rooms =
            {
                new MapRoom(0, new RectInt(0, 0, 2, 2)),
                new MapRoom(1, new RectInt(3, 0, 2, 2)),
                new MapRoom(2, new RectInt(6, 6, 2, 2))
            };
            Vector2Int[] corridorCells = { new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(4, 1) };
            MapCorridor[] corridors = { new MapCorridor(0, 1, corridorCells) };
            MapData mapData = new MapData(8, 8, 0, cells, rooms, corridors);

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.RoomGraphDisconnected);
        }

        [Test]
        public void Validate_분리된Floor영역을검출한다()
        {
            MapCellType[] cells = new MapCellType[25];
            cells[0] = MapCellType.Floor;
            cells[24] = MapCellType.Floor;

            MapData mapData = new MapData(5, 5, 0, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());

            MapValidationReport report = MapValidator.Validate(mapData);

            AssertIssue(report, MapValidationIssueCodes.FloorDisconnected);
        }

        private static MapCellType[] CreateFilledCells(int width, int height, MapCellType type)
        {
            MapCellType[] cells = new MapCellType[width * height];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = type;
            }

            return cells;
        }

        private static void AssertIssue(MapValidationReport report, string expectedCode)
        {
            Assert.That(report.IsValid, Is.False);

            bool found = false;

            for (int i = 0; i < report.Issues.Count; i++)
            {
                if (report.Issues[i].Code == expectedCode)
                {
                    found = true;
                    break;
                }
            }

            Assert.That(found, Is.True, $"Expected Issue Code를 찾지 못했습니다: {expectedCode}");
        }
    }
}