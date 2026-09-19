using CDG.Core.Results;
using CDG.MapGeneration;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class MapGenerationSettingsTests
    {
        [Test]
        public void Default_기본설정값을반환한다()
        {
            MapGenerationSettings settings = MapGenerationSettings.Default;

            Assert.That(settings.Width, Is.EqualTo(64));
            Assert.That(settings.Height, Is.EqualTo(64));
            Assert.That(settings.RoomCount, Is.EqualTo(12));
            Assert.That(settings.MinRoomSize, Is.EqualTo(new Vector2Int(5, 5)));
            Assert.That(settings.MaxRoomSize, Is.EqualTo(new Vector2Int(10, 10)));
            Assert.That(settings.RoomPadding, Is.EqualTo(1));
            Assert.That(settings.EdgePadding, Is.EqualTo(1));
            Assert.That(settings.MaxPlacementAttemptsPerRoom, Is.EqualTo(64));
            Assert.That(settings.ExtraConnectionCount, Is.EqualTo(2));
        }

        [Test]
        public void Validate_유효한설정이면성공한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings());

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Validate_null이면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(null);

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_Width가0이하면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(width: 0));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_Height가0이하면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(height: 0));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_RoomCount가0이하면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(roomCount: 0));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_MinRoomSize가유효하지않으면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(minRoomSize: new Vector2Int(0, 5)));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_MaxRoomSize가유효하지않으면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(maxRoomSize: new Vector2Int(10, 0)));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_MinRoomSize가MaxRoomSize보다크면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(minRoomSize: new Vector2Int(11, 5)));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_RoomPadding이음수이면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(roomPadding: -1));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_EdgePadding이음수이면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(edgePadding: -1));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_MaxPlacementAttempts가0이하면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(maxPlacementAttemptsPerRoom: 0));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_ExtraConnectionCount가음수이면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(extraConnectionCount: -1));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_MaxRoomSize와EdgePadding이Map에들어가지않으면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(
                CreateSettings(width: 11, maxRoomSize: new Vector2Int(10, 10), edgePadding: 1));

            AssertInvalidSettings(result);
        }

        [Test]
        public void Validate_ExtraConnectionCount가가능한수를초과하면실패한다()
        {
            Result result = MapGenerationSettingsValidator.Validate(CreateSettings(roomCount: 3, extraConnectionCount: 2));

            AssertInvalidSettings(result);
        }

        private static MapGenerationSettings CreateSettings(int width = 64, int height = 64, int roomCount = 12,
            Vector2Int? minRoomSize = null, Vector2Int? maxRoomSize = null, int roomPadding = 1, int edgePadding = 1,
            int maxPlacementAttemptsPerRoom = 64, int extraConnectionCount = 2)
        {
            return new MapGenerationSettings(width, height, roomCount, minRoomSize ?? new Vector2Int(5, 5),
                maxRoomSize ?? new Vector2Int(10, 10), roomPadding, edgePadding, maxPlacementAttemptsPerRoom, extraConnectionCount);
        }

        private static void AssertInvalidSettings(Result result)
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.InvalidSettings));
        }
    }
}