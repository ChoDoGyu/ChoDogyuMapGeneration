using CDG.Core.Results;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// MapData의 Floor와 Wall Cell을 Prefab GameObject로 Scene에 시각화합니다.
    /// 맵 생성 로직은 포함하지 않으며 전달받은 MapData만 표현합니다.
    /// </summary>
    public sealed class PrefabMapVisualizer : MonoBehaviour
    {
        private const string GeneratedRootName = "__CDG_GeneratedMap";

        [SerializeField] private PrefabMapTheme theme;
        [SerializeField] private MapProjectionPlane projectionPlane = MapProjectionPlane.XZ;
        [SerializeField, Min(0.01f)] private float cellSize = 1f;

        private Transform generatedRoot;

        /// <summary>
        /// Visualization에 사용할 Prefab Theme입니다.
        /// </summary>
        public PrefabMapTheme Theme
        {
            get => theme;
            set => theme = value;
        }

        /// <summary>
        /// Grid 좌표를 투영할 World 평면입니다.
        /// </summary>
        public MapProjectionPlane ProjectionPlane
        {
            get => projectionPlane;
            set => projectionPlane = value;
        }

        /// <summary>
        /// 인접한 Cell 사이의 World 간격입니다.
        /// 0보다 큰 값이어야 합니다.
        /// </summary>
        public float CellSize
        {
            get => cellSize;
            set => cellSize = value;
        }

        /// <summary>
        /// 지정한 MapData를 현재 Transform 아래에 Prefab으로 시각화합니다.
        /// 기존에 이 Visualizer가 생성한 결과는 먼저 제거됩니다.
        /// </summary>
        public Result Visualize(MapData mapData)
        {
            if (mapData == null)
            {
                return Failure(MapGenerationErrorCodes.VisualizerInvalidData, "MapData가 null입니다.");
            }

            MapValidationReport validation = MapValidator.Validate(mapData);

            if (!validation.IsValid)
            {
                return Failure(MapGenerationErrorCodes.VisualizerInvalidData,
                    $"MapData Validation에 실패했습니다. Error Count: {validation.ErrorCount}");
            }

            if (theme == null)
            {
                return Failure(MapGenerationErrorCodes.VisualizerThemeMissing, "Prefab Map Theme이 지정되지 않았습니다.");
            }

            if (!theme.IsValid)
            {
                return Failure(MapGenerationErrorCodes.VisualizerInvalidTheme,
                    "Prefab Map Theme에는 Floor Prefab과 Wall Prefab이 모두 필요합니다.");
            }

            if (cellSize <= 0f)
            {
                return Failure(MapGenerationErrorCodes.VisualizerInvalidTheme, "Cell Size는 0보다 커야 합니다.");
            }

            Clear();

            GameObject rootObject = new GameObject(GeneratedRootName);
            generatedRoot = rootObject.transform;
            generatedRoot.SetParent(transform, false);

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    MapCellType cellType = mapData.GetCell(x, y);

                    if (cellType == MapCellType.Empty)
                    {
                        continue;
                    }

                    GameObject prefab = cellType == MapCellType.Floor ? theme.FloorPrefab : theme.WallPrefab;
                    Vector3 position = ToLocalPosition(x, y);
                    GameObject instance = Instantiate(prefab, generatedRoot);
                    instance.transform.localPosition = position;
                }
            }

            return Result.Success();
        }

        /// <summary>
        /// 이 Visualizer가 생성한 Scene 표현을 제거합니다.
        /// MapData에는 영향을 주지 않습니다.
        /// </summary>
        public void Clear()
        {
            if (generatedRoot == null)
            {
                generatedRoot = FindGeneratedRoot();
            }

            if (generatedRoot == null)
            {
                return;
            }

            GameObject target = generatedRoot.gameObject;
            generatedRoot = null;

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private Vector3 ToLocalPosition(int x, int y)
        {
            if (projectionPlane == MapProjectionPlane.XY)
            {
                return new Vector3(x * cellSize, y * cellSize, 0f);
            }

            return new Vector3(x * cellSize, 0f, y * cellSize);
        }

        private Transform FindGeneratedRoot()
        {
            return transform.Find(GeneratedRootName);
        }

        private static Result Failure(string code, string message)
        {
            return Result.Failure(new ResultError(code, message));
        }
    }
}