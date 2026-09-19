# Basic Usage Sample

ChoDogyu Procedural Map Generation Framework의 기본 Runtime 사용 흐름을 확인하는 Sample입니다.

## 확인할 수 있는 기능

- `RoomCorridorMapGenerator`를 통한 MapData 생성
- 동일한 Seed를 사용한 맵 재현
- `MapSeedUtility`를 통한 새로운 Seed 생성
- `MapValidator`를 통한 생성 결과 검증
- `PrefabMapVisualizer`를 통한 Scene Visualization
- 생성된 Visualization 제거

## 실행 방법

Package Manager에서 `Basic Usage` Sample을 Import한 뒤 `Scenes/BasicUsage.unity` Scene을 엽니다.

Play Mode에 진입하면 기본 Seed를 사용하여 맵이 자동 생성됩니다.

화면 왼쪽 위의 버튼을 통해 기본 동작을 확인할 수 있습니다.

### Generate Same Seed

현재 Seed를 그대로 사용하여 맵을 다시 생성합니다.

같은 설정과 같은 Seed를 사용하므로 동일한 맵 구조가 생성됩니다.

### Generate Random Seed

새로운 Seed를 생성한 뒤 새로운 맵을 생성합니다.

### Clear

현재 Scene에 생성된 맵 표현을 제거합니다.

## Sample 구성

`MapGeneratorSample` GameObject에는 다음 Component가 포함됩니다.

- `PrefabMapVisualizer`
- `BasicMapUsage`

`PrefabMapVisualizer`에는 Sample 전용 `BasicMapTheme`이 연결됩니다.

Floor와 Wall은 기본 기능 확인을 위한 단순 Cube 기반 Prefab입니다.
실제 프로젝트에서는 원하는 Prefab으로 교체하여 사용할 수 있습니다.