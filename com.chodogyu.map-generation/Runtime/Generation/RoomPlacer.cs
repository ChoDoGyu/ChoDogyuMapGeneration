using System;
using System.Collections.Generic;
using CDG.Core.Results;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 설정과 결정적 난수 생성기를 사용하여 겹치지 않는 직사각형 방들을 Grid에 배치합니다.
    /// </summary>
    internal static class RoomPlacer
    {
        internal static Result<IReadOnlyList<MapRoom>> PlaceRooms(MapGenerationSettings settings, DeterministicRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            Result validation = MapGenerationSettingsValidator.Validate(settings);

            if (validation.IsFailure)
            {
                return Result<IReadOnlyList<MapRoom>>.Failure(validation.Error);
            }

            List<MapRoom> rooms = new List<MapRoom>(settings.RoomCount);

            for (int roomId = 0; roomId < settings.RoomCount; roomId++)
            {
                bool placed = TryPlaceRoom(roomId, settings, random, rooms);

                if (!placed)
                {
                    string message = $"Room {roomId}을 {settings.MaxPlacementAttemptsPerRoom}회 시도했지만 배치하지 못했습니다.";
                    return Result<IReadOnlyList<MapRoom>>.Failure(
                        new ResultError(MapGenerationErrorCodes.RoomPlacementFailed, message));
                }
            }

            return Result<IReadOnlyList<MapRoom>>.Success(rooms.AsReadOnly());
        }

        private static bool TryPlaceRoom(int roomId, MapGenerationSettings settings, DeterministicRandom random,
            List<MapRoom> rooms)
        {
            for (int attempt = 0; attempt < settings.MaxPlacementAttemptsPerRoom; attempt++)
            {
                RectInt bounds = CreateRandomBounds(settings, random);

                if (OverlapsExistingRoom(bounds, rooms, settings.RoomPadding))
                {
                    continue;
                }

                rooms.Add(new MapRoom(roomId, bounds));
                return true;
            }

            return false;
        }

        private static RectInt CreateRandomBounds(MapGenerationSettings settings, DeterministicRandom random)
        {
            int widthRange = settings.MaxRoomSize.x - settings.MinRoomSize.x + 1;
            int heightRange = settings.MaxRoomSize.y - settings.MinRoomSize.y + 1;

            int width = settings.MinRoomSize.x + random.NextInt(0, widthRange);
            int height = settings.MinRoomSize.y + random.NextInt(0, heightRange);

            int maxX = settings.Width - settings.EdgePadding - width;
            int maxY = settings.Height - settings.EdgePadding - height;
            int availableX = maxX - settings.EdgePadding + 1;
            int availableY = maxY - settings.EdgePadding + 1;

            int x = settings.EdgePadding + random.NextInt(0, availableX);
            int y = settings.EdgePadding + random.NextInt(0, availableY);

            return new RectInt(x, y, width, height);
        }

        private static bool OverlapsExistingRoom(RectInt candidate, IReadOnlyList<MapRoom> rooms, int padding)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (OverlapsWithPadding(candidate, rooms[i].Bounds, padding))
                {
                    return true;
                }
            }

            return false;
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