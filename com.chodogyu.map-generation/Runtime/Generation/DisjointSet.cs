using System;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Kruskal 알고리즘에서 연결된 Room 집합을 관리하는 Union-Find 자료구조입니다.
    /// Path Compression과 Union by Rank를 사용합니다.
    /// </summary>
    internal sealed class DisjointSet
    {
        private readonly int[] parents;
        private readonly int[] ranks;

        internal DisjointSet(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count는 음수일 수 없습니다.");
            }

            parents = new int[count];
            ranks = new int[count];

            for (int i = 0; i < count; i++)
            {
                parents[i] = i;
            }
        }

        internal int Find(int index)
        {
            ValidateIndex(index);

            if (parents[index] != index)
            {
                parents[index] = Find(parents[index]);
            }

            return parents[index];
        }

        internal bool Union(int first, int second)
        {
            int firstRoot = Find(first);
            int secondRoot = Find(second);

            if (firstRoot == secondRoot)
            {
                return false;
            }

            if (ranks[firstRoot] < ranks[secondRoot])
            {
                parents[firstRoot] = secondRoot;
            }
            else if (ranks[firstRoot] > ranks[secondRoot])
            {
                parents[secondRoot] = firstRoot;
            }
            else
            {
                parents[secondRoot] = firstRoot;
                ranks[firstRoot]++;
            }

            return true;
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= parents.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index가 Disjoint Set 범위를 벗어났습니다.");
            }
        }
    }
}