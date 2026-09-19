using System;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Map Generation에서 사용할 새로운 Seed 값을 생성하는 기능을 제공합니다.
    /// UnityEngine.Random의 전역 상태에는 영향을 주지 않습니다.
    /// </summary>
    public static class MapSeedUtility
    {
        /// <summary>
        /// 새로운 임의 Seed 값을 생성합니다.
        /// 생성된 값은 RoomCorridorMapGenerator의 Seed로 바로 사용할 수 있습니다.
        /// </summary>
        public static int CreateRandomSeed()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}