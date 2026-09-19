using System.Collections.Generic;
using CDG.Core.Results;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 설정과 Seed를 기반으로 Room + Corridor 구조의 MapData를 생성합니다.
    /// 동일한 설정과 Seed를 사용하면 동일한 생성 결과를 반환합니다.
    /// </summary>
    public static class RoomCorridorMapGenerator
    {
        /// <summary>
        /// 지정한 설정과 Seed를 사용하여 새로운 MapData를 생성합니다.
        /// 잘못된 설정이나 Room 배치 실패는 Result 실패로 반환됩니다.
        /// </summary>
        public static Result<MapData> Generate(MapGenerationSettings settings, int seed)
        {
            Result validation = MapGenerationSettingsValidator.Validate(settings);

            if (validation.IsFailure)
            {
                return Result<MapData>.Failure(validation.Error);
            }

            DeterministicRandom random = new DeterministicRandom(seed);
            Result<IReadOnlyList<MapRoom>> roomResult = RoomPlacer.PlaceRooms(settings, random);

            if (roomResult.IsFailure)
            {
                return Result<MapData>.Failure(roomResult.Error);
            }

            IReadOnlyList<MapRoom> rooms = roomResult.Value;
            IReadOnlyList<RoomConnection> connections =
                RoomConnectionGraphBuilder.BuildConnections(rooms, settings.ExtraConnectionCount);
            IReadOnlyList<MapCorridor> corridors = CorridorGenerator.Generate(rooms, connections, random);
            IReadOnlyList<MapCellType> cells = MapCellBuilder.Build(settings.Width, settings.Height, rooms, corridors);

            MapData mapData = new MapData(settings.Width, settings.Height, seed, cells, rooms, corridors);
            return Result<MapData>.Success(mapData);
        }
    }
}