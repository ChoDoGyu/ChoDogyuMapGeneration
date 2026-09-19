using System;
using CDG.Core.Results;
using UnityEditor.SceneManagement;

namespace CDG.MapGeneration.Editor
{
    /// <summary>
    /// Editor에서 생성된 MapData를 PrefabMapVisualizer를 통해 Scene에 적용하거나 제거합니다.
    /// Scene이 변경되면 Dirty 상태로 표시하여 저장할 수 있도록 합니다.
    /// </summary>
    internal static class MapSceneApplyUtility
    {
        internal static Result Apply(PrefabMapVisualizer visualizer, MapData mapData)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException(nameof(visualizer));
            }

            Result result = visualizer.Visualize(mapData);

            if (result.IsSuccess)
            {
                MarkSceneDirty(visualizer);
            }

            return result;
        }

        internal static void Clear(PrefabMapVisualizer visualizer)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException(nameof(visualizer));
            }

            visualizer.Clear();
            MarkSceneDirty(visualizer);
        }

        private static void MarkSceneDirty(PrefabMapVisualizer visualizer)
        {
            if (visualizer.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(visualizer.gameObject.scene);
            }
        }
    }
}