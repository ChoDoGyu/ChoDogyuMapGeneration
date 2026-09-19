using CDG.Core.Results;
using UnityEditor;
using UnityEngine;

namespace CDG.MapGeneration.Editor
{
    /// <summary>
    /// Room + Corridor 맵의 생성 설정과 Seed를 입력하고 생성 결과와 논리적 Grid Preview를 확인할 수 있는 Editor Window입니다.
    /// 생성된 MapData는 PrefabMapVisualizer를 통해 Scene에 적용할 수 있습니다.
    /// </summary>
    public sealed class MapGenerationEditorWindow : EditorWindow
    {
        private const float PreviewHeight = 360f;
        private const float PreviewPadding = 8f;

        private static readonly Color PreviewBackgroundColor = new Color(0.12f, 0.12f, 0.12f);
        private static readonly Color EmptyColor = new Color(0.16f, 0.16f, 0.16f);
        private static readonly Color FloorColor = new Color(0.78f, 0.78f, 0.78f);
        private static readonly Color WallColor = new Color(0.35f, 0.35f, 0.35f);

        [SerializeField] private bool initialized;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int roomCount;
        [SerializeField] private Vector2Int minRoomSize;
        [SerializeField] private Vector2Int maxRoomSize;
        [SerializeField] private int roomPadding;
        [SerializeField] private int edgePadding;
        [SerializeField] private int maxPlacementAttemptsPerRoom;
        [SerializeField] private int extraConnectionCount;
        [SerializeField] private int seed;
        [SerializeField] private PrefabMapVisualizer sceneVisualizer;

        private MapData currentMapData;
        private MapValidationReport currentValidation;
        private string statusMessage;
        private MessageType statusType = MessageType.Info;
        private Vector2 scrollPosition;

        [MenuItem("Tools/CDG/Map Generation/Map Generator")]
        private static void OpenWindow()
        {
            MapGenerationEditorWindow window = GetWindow<MapGenerationEditorWindow>("Map Generator");
            window.minSize = new Vector2(420f, 640f);
            window.Show();
        }

        private void OnEnable()
        {
            if (initialized)
            {
                return;
            }

            ApplyDefaultSettings();
            seed = MapSeedUtility.CreateRandomSeed();
            initialized = true;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(8f);

            DrawGenerationSettings();
            EditorGUILayout.Space(8f);

            DrawSeedSettings();
            EditorGUILayout.Space(8f);

            DrawActions();
            EditorGUILayout.Space(8f);

            DrawSceneVisualization();
            EditorGUILayout.Space(8f);

            DrawStatus();
            DrawSummary();

            if (currentMapData != null)
            {
                EditorGUILayout.Space(10f);
                DrawPreview();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Procedural Map Generator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Room + Corridor 구조의 논리적 MapData를 설정과 Seed를 기반으로 생성합니다.",
                EditorStyles.wordWrappedLabel);
        }

        private void DrawGenerationSettings()
        {
            EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);

            width = EditorGUILayout.IntField("Width", width);
            height = EditorGUILayout.IntField("Height", height);
            roomCount = EditorGUILayout.IntField("Room Count", roomCount);
            minRoomSize = EditorGUILayout.Vector2IntField("Min Room Size", minRoomSize);
            maxRoomSize = EditorGUILayout.Vector2IntField("Max Room Size", maxRoomSize);
            roomPadding = EditorGUILayout.IntField("Room Padding", roomPadding);
            edgePadding = EditorGUILayout.IntField("Edge Padding", edgePadding);
            maxPlacementAttemptsPerRoom = EditorGUILayout.IntField(
                "Max Placement Attempts", maxPlacementAttemptsPerRoom);
            extraConnectionCount = EditorGUILayout.IntField("Extra Connections", extraConnectionCount);

            if (GUILayout.Button("Reset Defaults"))
            {
                ApplyDefaultSettings();
                SetStatus("기본 Generation Settings를 적용했습니다.", MessageType.Info);
            }
        }

        private void DrawSeedSettings()
        {
            EditorGUILayout.LabelField("Seed", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            seed = EditorGUILayout.IntField(seed);

            if (GUILayout.Button("Randomize", GUILayout.Width(100f)))
            {
                seed = MapSeedUtility.CreateRandomSeed();
                SetStatus($"새 Seed를 생성했습니다: {seed}", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate", GUILayout.Height(28f)))
            {
                GenerateMap();
            }

            EditorGUI.BeginDisabledGroup(currentMapData == null);

            if (GUILayout.Button("Regenerate", GUILayout.Height(28f)))
            {
                GenerateMap();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(currentMapData == null);

            if (GUILayout.Button("Validate"))
            {
                ValidateCurrentMap();
            }

            if (GUILayout.Button("Clear"))
            {
                ClearCurrentMap();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSceneVisualization()
        {
            EditorGUILayout.LabelField("Scene Visualization", EditorStyles.boldLabel);

            sceneVisualizer = (PrefabMapVisualizer)EditorGUILayout.ObjectField(
                "Scene Visualizer", sceneVisualizer, typeof(PrefabMapVisualizer), true);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(currentMapData == null || sceneVisualizer == null);

            if (GUILayout.Button("Apply to Scene"))
            {
                ApplyCurrentMapToScene();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(sceneVisualizer == null);

            if (GUILayout.Button("Clear Scene"))
            {
                ClearSceneVisualization();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            if (string.IsNullOrEmpty(statusMessage))
            {
                return;
            }

            EditorGUILayout.HelpBox(statusMessage, statusType);
            EditorGUILayout.Space(4f);
        }

        private void DrawSummary()
        {
            if (currentMapData == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Generated Map Summary", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Size", $"{currentMapData.Width} x {currentMapData.Height}");
            EditorGUILayout.LabelField("Seed", currentMapData.Seed.ToString());
            EditorGUILayout.LabelField("Rooms", currentMapData.Rooms.Count.ToString());
            EditorGUILayout.LabelField("Corridors", currentMapData.Corridors.Count.ToString());

            CountCells(currentMapData, out int floorCount, out int wallCount, out int emptyCount);

            EditorGUILayout.LabelField("Floor Cells", floorCount.ToString());
            EditorGUILayout.LabelField("Wall Cells", wallCount.ToString());
            EditorGUILayout.LabelField("Empty Cells", emptyCount.ToString());

            if (currentValidation != null)
            {
                EditorGUILayout.LabelField("Validation", currentValidation.IsValid ? "Valid" : "Invalid");
                EditorGUILayout.LabelField("Errors", currentValidation.ErrorCount.ToString());
                EditorGUILayout.LabelField("Warnings", currentValidation.WarningCount.ToString());
            }
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Map Preview", EditorStyles.boldLabel);

            Rect previewRect = GUILayoutUtility.GetRect(
                100f, PreviewHeight, GUILayout.ExpandWidth(true), GUILayout.Height(PreviewHeight));

            EditorGUI.DrawRect(previewRect, PreviewBackgroundColor);

            Rect contentRect = new Rect(
                previewRect.x + PreviewPadding,
                previewRect.y + PreviewPadding,
                previewRect.width - PreviewPadding * 2f,
                previewRect.height - PreviewPadding * 2f);

            Rect mapRect = CalculateMapRect(contentRect);

            for (int y = 0; y < currentMapData.Height; y++)
            {
                for (int x = 0; x < currentMapData.Width; x++)
                {
                    DrawPreviewCell(mapRect, x, y);
                }
            }
        }

        private Rect CalculateMapRect(Rect availableRect)
        {
            float cellWidth = availableRect.width / currentMapData.Width;
            float cellHeight = availableRect.height / currentMapData.Height;
            float cellSize = Mathf.Min(cellWidth, cellHeight);

            float mapWidth = currentMapData.Width * cellSize;
            float mapHeight = currentMapData.Height * cellSize;
            float x = availableRect.x + (availableRect.width - mapWidth) * 0.5f;
            float y = availableRect.y + (availableRect.height - mapHeight) * 0.5f;

            return new Rect(x, y, mapWidth, mapHeight);
        }

        private void DrawPreviewCell(Rect mapRect, int x, int y)
        {
            float cellWidth = mapRect.width / currentMapData.Width;
            float cellHeight = mapRect.height / currentMapData.Height;
            float drawX = mapRect.x + x * cellWidth;
            float drawY = mapRect.yMax - (y + 1) * cellHeight;

            Rect cellRect = new Rect(drawX, drawY, cellWidth, cellHeight);
            EditorGUI.DrawRect(cellRect, GetCellColor(currentMapData.GetCell(x, y)));
        }

        private static Color GetCellColor(MapCellType cellType)
        {
            switch (cellType)
            {
                case MapCellType.Floor:
                    return FloorColor;

                case MapCellType.Wall:
                    return WallColor;

                default:
                    return EmptyColor;
            }
        }

        private void GenerateMap()
        {
            MapGenerationSettings settings = CreateSettings();
            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, seed);

            if (result.IsFailure)
            {
                currentMapData = null;
                currentValidation = null;
                SetStatus($"{result.Error.Code}\n{result.Error.Message}", MessageType.Error);
                return;
            }

            currentMapData = result.Value;
            currentValidation = MapValidator.Validate(currentMapData);

            if (!currentValidation.IsValid)
            {
                SetStatus(
                    $"맵은 생성되었지만 Validation에 실패했습니다. Error Count: {currentValidation.ErrorCount}",
                    MessageType.Error);
                return;
            }

            SetStatus(
                $"맵 생성 완료 - Seed: {currentMapData.Seed}, Rooms: {currentMapData.Rooms.Count}, " +
                $"Corridors: {currentMapData.Corridors.Count}",
                MessageType.Info);
        }

        private void ValidateCurrentMap()
        {
            if (currentMapData == null)
            {
                return;
            }

            currentValidation = MapValidator.Validate(currentMapData);

            if (currentValidation.IsValid)
            {
                SetStatus("Map Validation 통과 - 발견된 Error가 없습니다.", MessageType.Info);
                return;
            }

            SetStatus($"Map Validation 실패 - Error Count: {currentValidation.ErrorCount}", MessageType.Error);
        }

        private void ClearCurrentMap()
        {
            currentMapData = null;
            currentValidation = null;
            SetStatus("현재 생성된 MapData를 제거했습니다.", MessageType.Info);
        }

        private void ApplyCurrentMapToScene()
        {
            if (currentMapData == null || sceneVisualizer == null)
            {
                return;
            }

            Result result = MapSceneApplyUtility.Apply(sceneVisualizer, currentMapData);

            if (result.IsFailure)
            {
                SetStatus($"{result.Error.Code}\n{result.Error.Message}", MessageType.Error);
                return;
            }

            SetStatus($"Scene 적용 완료 - Visualizer: {sceneVisualizer.name}", MessageType.Info);
        }

        private void ClearSceneVisualization()
        {
            if (sceneVisualizer == null)
            {
                return;
            }

            MapSceneApplyUtility.Clear(sceneVisualizer);
            SetStatus($"Scene Visualization 제거 완료 - Visualizer: {sceneVisualizer.name}", MessageType.Info);
        }

        private MapGenerationSettings CreateSettings()
        {
            return new MapGenerationSettings(width, height, roomCount, minRoomSize, maxRoomSize, roomPadding,
                edgePadding, maxPlacementAttemptsPerRoom, extraConnectionCount);
        }

        private void ApplyDefaultSettings()
        {
            MapGenerationSettings defaults = MapGenerationSettings.Default;

            width = defaults.Width;
            height = defaults.Height;
            roomCount = defaults.RoomCount;
            minRoomSize = defaults.MinRoomSize;
            maxRoomSize = defaults.MaxRoomSize;
            roomPadding = defaults.RoomPadding;
            edgePadding = defaults.EdgePadding;
            maxPlacementAttemptsPerRoom = defaults.MaxPlacementAttemptsPerRoom;
            extraConnectionCount = defaults.ExtraConnectionCount;
        }

        private void SetStatus(string message, MessageType type)
        {
            statusMessage = message;
            statusType = type;
            Repaint();
        }

        private static void CountCells(MapData mapData, out int floorCount, out int wallCount, out int emptyCount)
        {
            floorCount = 0;
            wallCount = 0;
            emptyCount = 0;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    switch (mapData.GetCell(x, y))
                    {
                        case MapCellType.Floor:
                            floorCount++;
                            break;

                        case MapCellType.Wall:
                            wallCount++;
                            break;

                        default:
                            emptyCount++;
                            break;
                    }
                }
            }
        }
    }
}