using CDG.Core.Results;
using UnityEngine;

namespace CDG.MapGeneration.Samples.BasicUsage
{
    /// <summary>
    /// Procedural Map Generation Framework의 기본 생성, 재생성 및 시각화 흐름을 보여주는 Sample입니다.
    /// </summary>
    [RequireComponent(typeof(PrefabMapVisualizer))]
    public sealed class BasicMapUsage : MonoBehaviour
    {
        [SerializeField] private int seed = 12345;
        [SerializeField] private bool generateOnStart = true;

        private PrefabMapVisualizer visualizer;
        private MapData currentMapData;
        private string statusMessage = "Generate 버튼을 눌러 맵을 생성할 수 있습니다.";

        private void Awake()
        {
            visualizer = GetComponent<PrefabMapVisualizer>();
        }

        private void Start()
        {
            if (generateOnStart)
            {
                Generate();
            }
        }

        /// <summary>
        /// 현재 Seed를 사용하여 맵을 생성하고 Scene에 시각화합니다.
        /// </summary>
        public void Generate()
        {
            Result<MapData> generation = RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, seed);

            if (generation.IsFailure)
            {
                currentMapData = null;
                statusMessage = $"생성 실패: {generation.Error.Code}";
                Debug.LogError($"맵 생성 실패: {generation.Error.Code} / {generation.Error.Message}", this);
                return;
            }

            MapValidationReport validation = MapValidator.Validate(generation.Value);

            if (!validation.IsValid)
            {
                currentMapData = null;
                statusMessage = $"검증 실패: Error {validation.ErrorCount}";
                Debug.LogError($"맵 검증 실패: Error Count {validation.ErrorCount}", this);
                return;
            }

            Result visualization = visualizer.Visualize(generation.Value);

            if (visualization.IsFailure)
            {
                currentMapData = null;
                statusMessage = $"시각화 실패: {visualization.Error.Code}";
                Debug.LogError($"맵 시각화 실패: {visualization.Error.Code} / {visualization.Error.Message}", this);
                return;
            }

            currentMapData = generation.Value;
            statusMessage =
                $"생성 완료 | Seed: {currentMapData.Seed} | Rooms: {currentMapData.Rooms.Count} | " +
                $"Corridors: {currentMapData.Corridors.Count}";

            Debug.Log(statusMessage, this);
        }

        /// <summary>
        /// 새로운 Seed를 만든 뒤 새로운 맵을 생성합니다.
        /// </summary>
        public void GenerateRandomSeed()
        {
            seed = MapSeedUtility.CreateRandomSeed();
            Generate();
        }

        /// <summary>
        /// 현재 Scene Visualization을 제거합니다.
        /// </summary>
        public void Clear()
        {
            visualizer.Clear();
            currentMapData = null;
            statusMessage = "Scene Visualization을 제거했습니다.";

            Debug.Log(statusMessage, this);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16f, 16f, 360f, 180f), GUI.skin.box);

            GUILayout.Label("CDG Map Generation - Basic Usage");
            GUILayout.Label($"Current Seed: {seed}");
            GUILayout.Space(6f);

            if (GUILayout.Button("Generate Same Seed"))
            {
                Generate();
            }

            if (GUILayout.Button("Generate Random Seed"))
            {
                GenerateRandomSeed();
            }

            if (GUILayout.Button("Clear"))
            {
                Clear();
            }

            GUILayout.Space(6f);
            GUILayout.Label(statusMessage);

            GUILayout.EndArea();
        }
    }
}