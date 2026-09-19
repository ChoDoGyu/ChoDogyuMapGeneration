using System;
using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class RoomPlacerTests
    {
        [Test]
        public void PlaceRooms_요청한개수의방을순차적인Id로배치한다()
        {
            MapGenerationSettings settings = CreateSettings(roomCount: 10);
            Result<IReadOnlyList<MapRoom>> result = RoomPlacer.PlaceRooms(settings, new DeterministicRandom(12345));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(10));

            for (int i = 0; i < result.Value.Count; i++)
            {
                Assert.That(result.Value[i].Id, Is.EqualTo(i));
            }
        }

        [Test]
        public void PlaceRooms_방크기와위치및Padding규칙을지킨다()
        {
            MapGenerationSettings settings = CreateSettings(roomCount: 12);
            Result<IReadOnlyList<MapRoom>> result = RoomPlacer.PlaceRooms(settings, new DeterministicRandom(54321));

            Assert.That(result.IsSuccess, Is.True);

            for (int i = 0; i < result.Value.Count; i++)
            {
                MapRoom room = result.Value[i];

                Assert.That(room.Bounds.width, Is.InRange(settings.MinRoomSize.x, settings.MaxRoomSize.x));
                Assert.That(room.Bounds.height, Is.InRange(settings.MinRoomSize.y, settings.MaxRoomSize.y));
                Assert.That(room.Bounds.xMin, Is.GreaterThanOrEqualTo(settings.EdgePadding));
                Assert.That(room.Bounds.yMin, Is.GreaterThanOrEqualTo(settings.EdgePadding));
                Assert.That(room.Bounds.xMax, Is.LessThanOrEqualTo(settings.Width - settings.EdgePadding));
                Assert.That(room.Bounds.yMax, Is.LessThanOrEqualTo(settings.Height - settings.EdgePadding));

                for (int j = i + 1; j < result.Value.Count; j++)
                {
                    Assert.That(OverlapsWithPadding(room.Bounds, result.Value[j].Bounds, settings.RoomPadding), Is.False);
                }
            }
        }

        [Test]
        public void PlaceRooms_동일한Seed는동일한방배치를생성한다()
        {
            MapGenerationSettings settings = CreateSettings();
            Result<IReadOnlyList<MapRoom>> first = RoomPlacer.PlaceRooms(settings, new DeterministicRandom(777));
            Result<IReadOnlyList<MapRoom>> second = RoomPlacer.PlaceRooms(settings, new DeterministicRandom(777));

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);
            Assert.That(first.Value.Count, Is.EqualTo(second.Value.Count));

            for (int i = 0; i < first.Value.Count; i++)
            {
                Assert.That(first.Value[i].Id, Is.EqualTo(second.Value[i].Id));
                Assert.That(first.Value[i].Bounds, Is.EqualTo(second.Value[i].Bounds));
                Assert.That(first.Value[i].Center, Is.EqualTo(second.Value[i].Center));
            }
        }

        [Test]
        public void PlaceRooms_제한된시도안에배치할수없으면실패한다()
        {
            MapGenerationSettings settings = new MapGenerationSettings(
                8, 8, 2, new Vector2Int(6, 6), new Vector2Int(6, 6), 1, 1, 8, 0);

            Result<IReadOnlyList<MapRoom>> result = RoomPlacer.PlaceRooms(settings, new DeterministicRandom(12345));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.RoomPlacementFailed));
        }

        [Test]
        public void PlaceRooms_잘못된설정이면InvalidSettings로실패한다()
        {
            MapGenerationSettings settings = CreateSettings(width: 0);
            Result<IReadOnlyList<MapRoom>> result = RoomPlacer.PlaceRooms(settings, new DeterministicRandom(12345));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.InvalidSettings));
        }

        [Test]
        public void PlaceRooms_Random이null이면예외가발생한다()
        {
            Assert.Throws<ArgumentNullException>(() => RoomPlacer.PlaceRooms(CreateSettings(), null));
        }

        private static MapGenerationSettings CreateSettings(int width = 64, int height = 64, int roomCount = 12,
            int roomPadding = 1, int edgePadding = 1)
        {
            return new MapGenerationSettings(width, height, roomCount, new Vector2Int(5, 5), new Vector2Int(10, 10),
                roomPadding, edgePadding, 64, 2);
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