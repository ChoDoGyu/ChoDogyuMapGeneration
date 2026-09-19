using System;

namespace CDG.MapGeneration
{
    /// <summary>
    /// 동일한 Seed에서 항상 동일한 난수 순서를 생성하는 Xorshift32 기반 난수 생성기입니다.
    /// UnityEngine.Random의 전역 상태를 사용하거나 변경하지 않습니다.
    /// </summary>
    internal sealed class DeterministicRandom
    {
        private const uint ZeroSeedState = 0x6D2B79F5u;

        private uint state;

        /// <summary>
        /// 지정한 Seed를 사용하여 새로운 난수 생성기를 초기화합니다.
        /// Seed가 0인 경우 Xorshift32의 고정 상태를 피하기 위해 내부 대체 상태를 사용합니다.
        /// </summary>
        internal DeterministicRandom(int seed)
        {
            state = seed == 0 ? ZeroSeedState : unchecked((uint)seed);
        }

        /// <summary>
        /// 다음 32비트 부호 없는 난수를 반환합니다.
        /// </summary>
        internal uint NextUInt()
        {
            uint value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;
            return value;
        }

        /// <summary>
        /// minInclusive 이상 maxExclusive 미만 범위의 정수를 반환합니다.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// maxExclusive가 minInclusive보다 작거나 같은 경우 발생합니다.
        /// </exception>
        internal int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "maxExclusive는 minInclusive보다 커야 합니다.");
            }

            uint range = (uint)((long)maxExclusive - minInclusive);
            uint threshold = unchecked(0u - range) % range;
            uint value;

            do
            {
                value = NextUInt();
            }
            while (value < threshold);

            return (int)((long)minInclusive + value % range);
        }
    }
}