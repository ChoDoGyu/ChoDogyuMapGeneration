using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class MapGenerationRuntimeTests
    {
        [Test]
        public void Generate_Default설정으로정상생성된다()
        {
            Result<MapData> result = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Rooms.Count, Is.EqualTo(MapGenerationSettings.Default.RoomCount));
        }

        [Test]
        public void Generate_Settings가null이면InvalidSettings로실패한다()
        {
            Result<MapData> result = RoomCorridorMapGenerator.Generate(null, 12345);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.InvalidSettings));
        }

        [Test]
        public void Generate_Room이하나이면Corridor를생성하지않는다()
        {
            MapGenerationSettings settings = new MapGenerationSettings(
                32, 32, 1, new Vector2Int(5, 5), new Vector2Int(8, 8), 1, 1, 64, 0);

            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 12345);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Rooms.Count, Is.EqualTo(1));
            Assert.That(result.Value.Corridors, Is.Empty);
        }

        [Test]
        public void Generate_Room이두개이면하나의Corridor로연결한다()
        {
            MapGenerationSettings settings = new MapGenerationSettings(
                48, 48, 2, new Vector2Int(5, 5), new Vector2Int(8, 8), 1, 1, 64, 0);

            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 54321);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Rooms.Count, Is.EqualTo(2));
            Assert.That(result.Value.Corridors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Generate_모든Room은MapBounds와Padding규칙을지킨다()
        {
            MapGenerationSettings settings = MapGenerationSettings.Default;
            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 777);

            Assert.That(result.IsSuccess, Is.True);

            for (int i = 0; i < result.Value.Rooms.Count; i++)
            {
                MapRoom room = result.Value.Rooms[i];

                Assert.That(room.Bounds.xMin, Is.GreaterThanOrEqualTo(settings.EdgePadding));
                Assert.That(room.Bounds.yMin, Is.GreaterThanOrEqualTo(settings.EdgePadding));
                Assert.That(room.Bounds.xMax, Is.LessThanOrEqualTo(settings.Width - settings.EdgePadding));
                Assert.That(room.Bounds.yMax, Is.LessThanOrEqualTo(settings.Height - settings.EdgePadding));

                for (int j = i + 1; j < result.Value.Rooms.Count; j++)
                {
                    Assert.That(OverlapsWithPadding(room.Bounds, result.Value.Rooms[j].Bounds, settings.RoomPadding), Is.False);
                }
            }
        }

        [Test]
        public void Generate_모든CorridorCell은MapBounds안의Floor이다()
        {
            Result<MapData> result = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 24680);

            Assert.That(result.IsSuccess, Is.True);

            for (int i = 0; i < result.Value.Corridors.Count; i++)
            {
                MapCorridor corridor = result.Value.Corridors[i];

                for (int j = 0; j < corridor.Cells.Count; j++)
                {
                    Assert.That(result.Value.IsInBounds(corridor.Cells[j]), Is.True);
                    Assert.That(result.Value.GetCell(corridor.Cells[j]), Is.EqualTo(MapCellType.Floor));
                }
            }
        }

        [Test]
        public void Generate_음수Seed도정상적으로사용할수있다()
        {
            Result<MapData> result = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, -12345);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Seed, Is.EqualTo(-12345));
        }

        [Test]
        public void Generate_여러Seed에서안정적으로Map을생성한다()
        {
            MapGenerationSettings settings = new MapGenerationSettings(
                96, 96, 16, new Vector2Int(4, 4), new Vector2Int(8, 8), 1, 1, 128, 3);

            for (int seed = -25; seed < 25; seed++)
            {
                Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, seed);

                Assert.That(result.IsSuccess, Is.True, $"Seed {seed}에서 생성에 실패했습니다.");
                Assert.That(result.Value.Rooms.Count, Is.EqualTo(settings.RoomCount));
                Assert.That(result.Value.Corridors.Count, Is.EqualTo(settings.RoomCount - 1 + settings.ExtraConnectionCount));
            }
        }

        private static bool OverlapsWithPadding(RectInt first, RectInt second, int padding)
        {
            long firstRight = (long)first.xMax - 1;
            long firstTop = (long)first.yMax - 1;
            long secondRight = (long)second.xMax - 1;
            long secondTop = (long)second.yMax - 1;

            bool separatedX = firstRight + padding < second.xMin || secondRight + padding < first.xMin;
            bool separatedY = firstTop + padding < second.yMin || secondTop + padding < first.yMin;

            return !separatedX && !separatedY;
        }
    }
}