using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// PrefabMapVisualizer가 Floor와 Wall Cell을 표현할 때 사용할 Prefab 구성을 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabMapTheme", menuName = "CDG/Map Generation/Prefab Map Theme")]
    public sealed class PrefabMapTheme : ScriptableObject
    {
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject wallPrefab;

        /// <summary>
        /// Floor Cell을 표현할 Prefab입니다.
        /// </summary>
        public GameObject FloorPrefab
        {
            get => floorPrefab;
            set => floorPrefab = value;
        }

        /// <summary>
        /// Wall Cell을 표현할 Prefab입니다.
        /// </summary>
        public GameObject WallPrefab
        {
            get => wallPrefab;
            set => wallPrefab = value;
        }

        /// <summary>
        /// Floor와 Wall Prefab이 모두 지정되어 있는지를 반환합니다.
        /// </summary>
        public bool IsValid => floorPrefab != null && wallPrefab != null;
    }
}