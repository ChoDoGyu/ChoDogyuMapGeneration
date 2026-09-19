namespace CDG.MapGeneration
{
    /// <summary>
    /// 생성된 맵의 한 Grid Cell이 가지는 논리적 상태를 나타냅니다.
    /// 실제 Prefab이나 Tile 표현과는 독립적으로 사용됩니다.
    /// </summary>
    public enum MapCellType
    {
        Empty = 0,
        Floor = 1,
        Wall = 2
    }
}