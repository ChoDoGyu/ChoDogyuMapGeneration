using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class RoomCorridorMapGeneratorTests
    {
        [Test]
        public void Generate_유효한설정이면MapData를생성한다()
        {
            MapGenerationSettings settings = CreateSettings();

            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 12345);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Width, Is.EqualTo(settings.Width));
            Assert.That(result.Value.Height, Is.EqualTo(settings.Height));
            Assert.That(result.Value.Seed, Is.EqualTo(12345));
            Assert.That(result.Value.Rooms.Count, Is.EqualTo(settings.RoomCount));
        }

        [Test]
        public void Generate_Corridor개수는MST와추가연결수를포함한다()
        {
            MapGenerationSettings settings = CreateSettings(roomCount: 10, extraConnectionCount: 2);

            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 54321);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Corridors.Count, Is.EqualTo(settings.RoomCount - 1 + settings.ExtraConnectionCount));
        }

        [Test]
        public void Generate_모든Room중심은Floor이다()
        {
            Result<MapData> result = RoomCorridorMapGenerator.Generate(CreateSettings(), 777);

            Assert.That(result.IsSuccess, Is.True);

            for (int i = 0; i < result.Value.Rooms.Count; i++)
            {
                Assert.That(result.Value.GetCell(result.Value.Rooms[i].Center), Is.EqualTo(MapCellType.Floor));
            }
        }

        [Test]
        public void Generate_모든CorridorCell은Floor이다()
        {
            Result<MapData> result = RoomCorridorMapGenerator.Generate(CreateSettings(), 13579);

            Assert.That(result.IsSuccess, Is.True);

            for (int i = 0; i < result.Value.Corridors.Count; i++)
            {
                MapCorridor corridor = result.Value.Corridors[i];

                for (int j = 0; j < corridor.Cells.Count; j++)
                {
                    Assert.That(result.Value.GetCell(corridor.Cells[j]), Is.EqualTo(MapCellType.Floor));
                }
            }
        }

        [Test]
        public void Generate_잘못된설정이면InvalidSettings로실패한다()
        {
            MapGenerationSettings settings = CreateSettings(width: 0);

            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 12345);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.InvalidSettings));
        }

        [Test]
        public void Generate_Room배치가불가능하면RoomPlacementFailed로실패한다()
        {
            MapGenerationSettings settings = new MapGenerationSettings(
                8, 8, 2, new Vector2Int(6, 6), new Vector2Int(6, 6), 1, 1, 8, 0);

            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, 12345);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.RoomPlacementFailed));
        }

        private static MapGenerationSettings CreateSettings(int width = 64, int height = 64, int roomCount = 12,
            int extraConnectionCount = 2)
        {
            return new MapGenerationSettings(width, height, roomCount, new Vector2Int(5, 5), new Vector2Int(10, 10),
                1, 1, 64, extraConnectionCount);
        }
    }
}