using CDG.Core.Results;

namespace CDG.MapGeneration
{
    internal static class MapGenerationSettingsValidator
    {
        internal static Result Validate(MapGenerationSettings settings)
        {
            if (settings == null)
            {
                return Failure("Map Generation Settings가 null입니다.");
            }

            if (settings.Width <= 0)
            {
                return Failure("Map Width는 0보다 커야 합니다.");
            }

            if (settings.Height <= 0)
            {
                return Failure("Map Height는 0보다 커야 합니다.");
            }

            if (settings.RoomCount <= 0)
            {
                return Failure("Room Count는 0보다 커야 합니다.");
            }

            if (settings.MinRoomSize.x <= 0 || settings.MinRoomSize.y <= 0)
            {
                return Failure("Min Room Size의 Width와 Height는 0보다 커야 합니다.");
            }

            if (settings.MaxRoomSize.x <= 0 || settings.MaxRoomSize.y <= 0)
            {
                return Failure("Max Room Size의 Width와 Height는 0보다 커야 합니다.");
            }

            if (settings.MinRoomSize.x > settings.MaxRoomSize.x || settings.MinRoomSize.y > settings.MaxRoomSize.y)
            {
                return Failure("Min Room Size는 Max Room Size보다 클 수 없습니다.");
            }

            if (settings.RoomPadding < 0)
            {
                return Failure("Room Padding은 음수일 수 없습니다.");
            }

            if (settings.EdgePadding < 0)
            {
                return Failure("Edge Padding은 음수일 수 없습니다.");
            }

            if (settings.MaxPlacementAttemptsPerRoom <= 0)
            {
                return Failure("Max Placement Attempts Per Room은 0보다 커야 합니다.");
            }

            if (settings.ExtraConnectionCount < 0)
            {
                return Failure("Extra Connection Count는 음수일 수 없습니다.");
            }

            long requiredWidth = settings.MaxRoomSize.x + settings.EdgePadding * 2L;
            long requiredHeight = settings.MaxRoomSize.y + settings.EdgePadding * 2L;

            if (requiredWidth > settings.Width || requiredHeight > settings.Height)
            {
                return Failure("Max Room Size와 Edge Padding을 현재 Map Bounds 안에 배치할 수 없습니다.");
            }

            long maxExtraConnectionCount = settings.RoomCount <= 2
                ? 0L
                : (long)(settings.RoomCount - 1) * (settings.RoomCount - 2) / 2L;

            if (settings.ExtraConnectionCount > maxExtraConnectionCount)
            {
                return Failure($"Extra Connection Count가 가능한 최대 추가 연결 수를 초과했습니다. Maximum: {maxExtraConnectionCount}");
            }

            return Result.Success();
        }

        private static Result Failure(string message)
        {
            return Result.Failure(new ResultError(MapGenerationErrorCodes.InvalidSettings, message));
        }
    }
}