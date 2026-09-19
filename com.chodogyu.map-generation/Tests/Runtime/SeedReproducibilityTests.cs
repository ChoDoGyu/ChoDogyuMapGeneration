using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class SeedReproducibilityTests
    {
        [Test]
        public void Generate_동일한설정과Seed는완전히동일한Map을생성한다()
        {
            MapGenerationSettings settings = MapGenerationSettings.Default;

            Result<MapData> first = RoomCorridorMapGenerator.Generate(settings, 12345);
            Result<MapData> second = RoomCorridorMapGenerator.Generate(settings, 12345);

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);

            AssertMapsEqual(first.Value, second.Value);
        }

        [Test]
        public void Generate_Seed가0이어도동일한Map을재현한다()
        {
            Result<MapData> first = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 0);
            Result<MapData> second = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 0);

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);

            AssertMapsEqual(first.Value, second.Value);
        }

        [Test]
        public void Generate_음수Seed도동일한Map을재현한다()
        {
            Result<MapData> first = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, -98765);
            Result<MapData> second = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, -98765);

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);

            AssertMapsEqual(first.Value, second.Value);
        }

        [Test]
        public void Generate_다른Seed는다른Room배치를생성한다()
        {
            Result<MapData> first = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);
            Result<MapData> second = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 54321);

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);

            bool different = false;

            for (int i = 0; i < first.Value.Rooms.Count; i++)
            {
                if (first.Value.Rooms[i].Bounds != second.Value.Rooms[i].Bounds)
                {
                    different = true;
                    break;
                }
            }

            Assert.That(different, Is.True);
        }

        [Test]
        public void Generate_UnityRandom전역상태를변경하지않는다()
        {
            Random.InitState(24680);
            float expectedFirst = Random.value;
            float expectedSecond = Random.value;

            Random.InitState(24680);
            float actualFirst = Random.value;

            Result<MapData> result = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);

            float actualSecond = Random.value;

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(actualFirst, Is.EqualTo(expectedFirst));
            Assert.That(actualSecond, Is.EqualTo(expectedSecond));
        }

        private static void AssertMapsEqual(MapData first, MapData second)
        {
            Assert.That(first.Width, Is.EqualTo(second.Width));
            Assert.That(first.Height, Is.EqualTo(second.Height));
            Assert.That(first.Seed, Is.EqualTo(second.Seed));
            Assert.That(first.Rooms.Count, Is.EqualTo(second.Rooms.Count));
            Assert.That(first.Corridors.Count, Is.EqualTo(second.Corridors.Count));

            for (int i = 0; i < first.Rooms.Count; i++)
            {
                Assert.That(first.Rooms[i].Id, Is.EqualTo(second.Rooms[i].Id));
                Assert.That(first.Rooms[i].Bounds, Is.EqualTo(second.Rooms[i].Bounds));
                Assert.That(first.Rooms[i].Center, Is.EqualTo(second.Rooms[i].Center));
            }

            for (int i = 0; i < first.Corridors.Count; i++)
            {
                Assert.That(first.Corridors[i].FromRoomId, Is.EqualTo(second.Corridors[i].FromRoomId));
                Assert.That(first.Corridors[i].ToRoomId, Is.EqualTo(second.Corridors[i].ToRoomId));
                Assert.That(first.Corridors[i].Cells, Is.EqualTo(second.Corridors[i].Cells));
            }

            for (int y = 0; y < first.Height; y++)
            {
                for (int x = 0; x < first.Width; x++)
                {
                    Assert.That(first.GetCell(x, y), Is.EqualTo(second.GetCell(x, y)));
                }
            }
        }
    }
}