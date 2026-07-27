# DefCity / ElementalDef

DefCity의 기반 시스템을 활용해 더 작은 범위의 완성된 게임을 만드는 프로토타입입니다. 개발 코드명은 **ElementalDef**이며, 현재 테스트의 핵심은 물·불·흙 속성 상성, 속성 지형, 타워 배치·재배치, 자동 전투와 웨이브 진행입니다.

- 문서 기준일: 2026-07-27
- 권장 Unity 버전: `6000.3.9f1`
- 테스트 씬: `Assets/Scenes/ElementalDefMain.unity`
- 현재 테스트 방식: Unity Editor의 Play Mode

> 현재 버전은 목표 사양의 일부를 검증하는 개발용 프로토타입입니다. 최종 UI, 스테이지 선택, 결과 화면, 스킬·필살기 등은 아직 없거나 임시 상태입니다.

## 기준 기획안

ElementalDef는 [Elemental Defense Design_20260721_v2](<Documents/Elemental Defense Design_20260721_v2.docx>)를 기준으로 작업합니다. 문서 간 내용이 충돌하면 이 개정 기획안의 2026-07-21 기준을 우선합니다.

기획안은 **목표 사양**, 이 README의 조작·설정 안내는 **현재 구현 상태**를 설명합니다. 따라서 기획안에 있지만 현재 테스트 버전에서 조작할 수 없는 기능이 있을 수 있습니다.

| 범위 | 현재 테스트 가능 여부 |
| --- | --- |
| 물·불·흙 속성 상성과 피해 배율 | 가능 |
| 속성 지형의 공격·방어 보정 | 가능 |
| 3속성 타워 건설·재배치 | 가능 |
| 적 이동, 자동 탐색·전투, 5개 웨이브 | 가능 |
| 10스테이지 평가·자동 생성 구조 | 미구현 |
| 스킬, 필살기, 연계 스킬 | 미구현 |
| 로비, 스테이지 선택, 정식 결과 화면 | 미구현 |
| 일반·중간·보스 9종의 최종 몬스터 구성 | 미구현 — 현재는 속성별 Small 적 3종 |

## 1. 프로젝트 받기

### 추천: GitHub Desktop으로 Git 저장소 복제

현재 저장소는 Git과 Unity Version Control(UVCS)을 함께 사용하지만, 기획자 테스트에는 GitHub Desktop을 추천합니다. 

준비물:

- 프로젝트의 비공개 GitHub 저장소에 접근 가능한 GitHub 계정
- [GitHub Desktop](https://desktop.github.com/)
- Unity Hub
- Unity Editor `6000.3.9f1`
- 에셋 복제와 Unity `Library` 임포트를 위한 25GB 이상의 여유 공간 권장

처음 한 번만 다음 순서로 진행합니다.

1. GitHub Desktop에 해당 계정으로 로그인합니다.
2. `File > Clone Repository...`를 선택합니다.
3. `GitHub.com` 탭에서 `ysb06/defcity-private`를 선택합니다. 목록에 없다면 접근 권한과 로그인 계정을 확인합니다.
4. `Local Path`에 충분한 여유 공간이 있는 폴더를 지정하고 `Clone`을 누릅니다. GitHub의 `Download ZIP`은 사용하지 않습니다.
5. 복제가 끝나면 상단 `Current Branch`에서 공유 담당자가 지정한 **정확한 테스트 브랜치**를 선택합니다. 브랜치 안내가 없다면 `main`이나 `dev`를 임의로 고르지 말고 먼저 확인합니다.
6. `Fetch origin`을 누르고, `Pull origin`이 표시되면 한 번 더 눌러 최신 파일을 받습니다.
7. Git LFS 초기화 안내가 나타나면 허용하고 다운로드가 끝날 때까지 기다립니다.

공식 참고 자료: [GitHub Desktop에서 저장소 복제](https://docs.github.com/en/desktop/adding-and-cloning-repositories/cloning-and-forking-repositories-from-github-desktop), [GitHub Desktop과 Git LFS](https://docs.github.com/en/desktop/configuring-and-customizing-github-desktop/about-git-large-file-storage-and-github-desktop)

### Unity Hub에서 프로젝트 열기

저장소 최상위 폴더가 아니라, 그 안의 **Unity 프로젝트 폴더 `DefCity`**를 열어야 합니다.

```text
복제한 폴더/
├── README.md
├── Documents/
└── DefCity/          ← Unity Hub에서 이 폴더 선택
    ├── Assets/
    ├── Packages/
    └── ProjectSettings/
```

1. Unity Hub에서 `Add > Add project from disk`를 선택합니다.
2. 위 구조에서 `Assets`, `Packages`, `ProjectSettings`가 들어 있는 내부 `DefCity` 폴더를 선택합니다.
3. Editor 버전으로 `6000.3.9f1`을 선택해 엽니다.
4. 첫 실행의 에셋 임포트와 패키지 설치가 끝날 때까지 기다립니다. 시간이 걸려도 Unity를 강제 종료하지 않습니다.

정확한 Editor 버전이 없다면 Unity Hub에서 먼저 설치합니다. 다른 버전으로 열어 프로젝트 업그레이드 안내가 나오면 진행하지 말고 개발 담당자에게 확인합니다. `Safe Mode`나 컴파일 오류가 뜨면 임의로 수정하지 말고 화면과 Console 오류를 공유합니다.

### Unity Version Control을 쓰는 경우

UVCS는 **공유 담당자가 “이번 테스트는 UVCS의 특정 브랜치/변경집합을 사용한다”고 안내한 경우에만** 사용합니다.

1. 팀의 Unity Organization/DevOps 프로젝트 초대를 받은 Unity ID로 Unity Hub에 로그인합니다.
2. Unity Hub에서 `Add > Add from repository`를 선택합니다.
3. 담당자가 지정한 DefCity 저장소를 새 로컬 위치에 내려받습니다.
4. 내려받은 폴더에서 `Assets`, `Packages`, `ProjectSettings`가 바로 보이는 Unity 프로젝트 폴더를 엽니다.
5. Unity Editor의 `Window > Unity Version Control` 또는 UVCS 데스크톱 앱에서 담당자가 지정한 브랜치·변경집합인지 확인하고 `Update workspace`로 최신 파일을 받습니다.

공식 참고 자료: [Unity Hub에서 UVCS 프로젝트 추가](https://docs.unity.com/en-us/unity-version-control/get-started-vcs-hub), [UVCS Workspace 설정](https://docs.unity.com/en-us/unity-version-control/workflow/create-workspace)

> Git과 UVCS를 **같은 로컬 폴더에서 번갈아 사용하지 마세요.** 두 방식을 모두 확인해야 한다면 서로 다른 폴더에 각각 새로 받습니다. 또한 Git 브랜치와 UVCS 변경집합은 자동으로 같은 버전이 되지 않으므로, 테스트 출처를 반드시 기록합니다.

### 이후 최신 버전 받기

1. Play Mode를 종료하고 Unity를 닫습니다.
2. 변경한 테스트 값을 먼저 기록합니다.
3. GitHub Desktop의 `Changes` 탭에 로컬 변경이 남아 있다면 업데이트 전에 개발 담당자에게 확인합니다.
4. `Fetch origin > Pull origin`으로 최신 파일을 받습니다.
5. Unity를 다시 열고 테스트 브랜치와 씬을 확인합니다.

기획 테스트에서 바꾼 `.asset`이나 씬 파일은 별도 요청이 없다면 `Commit` 또는 `Push`하지 않습니다. 기준값으로 되돌릴 때 GitHub Desktop의 `Discard Changes`를 사용할 수 있지만, 기록하지 않은 변경은 영구 삭제되므로 주의합니다.

## 2. 실행하기

현재 `ElementalDefMain`은 Build Settings에 등록되어 있지 않습니다. 실행 파일을 만들거나 `Build And Run`하지 말고 다음처럼 Editor에서 직접 실행합니다.

1. Unity의 Project 창에서 `Assets/Scenes/ElementalDefMain.unity`를 더블클릭합니다.
2. Scene 탭 위쪽 이름이 `ElementalDefMain`인지 확인합니다.
3. Unity 상단의 ▶ Play 버튼을 누릅니다.
4. Game 탭을 한 번 클릭해 키보드와 마우스 포커스를 줍니다.

Play를 누르면 준비 버튼 없이 첫 웨이브가 바로 시작됩니다. 현재 한 Turn은 1초이며 첫 적은 약 5초 뒤에 등장합니다. 선배치된 타워가 없으므로 바로 타워를 건설해야 합니다.

다시 시작하려면 상단 Play 버튼을 눌러 Play Mode를 종료한 뒤 다시 누릅니다. 인게임 재시작 버튼은 아직 없습니다.

## 3. 현재 게임 흐름과 조작

### 기본 진행

1. Play 직후 Wave 01이 시작됩니다.
2. 화면 아래의 속성별 건설 버튼으로 타워를 배치합니다.
3. 적은 정해진 경로를 따라 본영으로 이동하고, 타워와 적은 범위 안의 상대를 자동으로 찾아 공격합니다.
4. 현재 웨이브에서 예약된 적을 모두 소환하고 처치하면 다음 웨이브가 바로 시작됩니다. 웨이브 사이 준비 시간은 없습니다.
5. 5개 웨이브의 적을 모두 처치하면 승리, 본영이 파괴되면 패배입니다.

현재는 자원, 건설 비용, 타워 수 제한이 없습니다. 수동 공격, 타깃 지정, 캐릭터 직접 이동도 없습니다.

### 카메라와 선택

| 기능 | 조작 | 알아둘 점 |
| --- | --- | --- |
| 카메라 이동 | `WASD` 또는 방향키 | 현재 카메라 방향을 기준으로 이동 |
| 화면 가장자리 이동 | 포인터를 Game 화면 가장자리에 둠 | 카메라가 뜻하지 않게 흐르면 포인터를 중앙으로 옮기거나 `R` 사용 |
| 확대·축소 | 마우스 휠 | UI 위에서는 작동하지 않음 |
| 드래그 이동 | 마우스 가운데 버튼을 누른 채 드래그 | UI 위에서는 작동하지 않음 |
| 카메라 회전 | `Q`, `E` | 좌우 회전 |
| 카메라 초기화 | `R` | 시작 위치·회전·줌으로 복귀 |
| 대상 선택 | 타워, 적, 본영을 좌클릭 | 한 번에 하나만 선택 |
| 선택 해제 | 빈 지형 좌클릭 | 선택 표시 제거 |
| 배치·이동 취소 | `Esc` | 우클릭은 취소가 아님 |

선택한 대상 주변의 초록색 원은 **공격 범위가 아니라 선택 표시**입니다. 실제 공격 범위 원은 아직 표시되지 않습니다.

화면 위쪽의 `Debug Text`에는 선택 대상의 이름, 공격 속성, 방어 속성·방어력, 현재 지형 속성, 현재/최대 체력, 최근 피해량이 표시됩니다. 상성·지형 보정이 적용되는지 볼 때 이 정보를 활용합니다.

### 타워 건설

1. 화면 아래의 `불 타워 건설`, `물 타워 건설`, `흙 타워 건설` 중 하나를 누릅니다.
2. 지형 위로 포인터를 움직입니다.
3. 셀 표시가 **파란색이면 설치 가능**, **빨간색이면 설치 불가**입니다.
4. 파란 셀을 좌클릭하면 타워 한 개가 설치되고 건설 모드가 끝납니다.
5. 여러 개를 놓으려면 매번 건설 버튼을 다시 누릅니다.
6. 설치하지 않고 취소하려면 `Esc`를 누릅니다.

현재 적 이동 경로이거나 다른 타워·적·본영의 충돌체와 겹치는 셀에는 설치할 수 없습니다. 빨간 셀을 클릭하면 건설 모드는 유지되며, 실패 이유는 화면이 아니라 Console에만 경고로 남습니다.

타워는 현재 약 3셀 이내의 적을 탐색합니다. 경로에서 너무 멀리 배치하면 공격 사거리 수치가 더 크더라도 적을 발견하지 못할 수 있습니다.

### 타워 재배치

1. 이미 건설한 아군 타워를 좌클릭합니다.
2. 초록색 선택 표시가 생겼는지 확인합니다.
3. 화면 아래의 `타워 재배치`를 누릅니다.
4. 파란색으로 표시되는 새 셀을 좌클릭합니다. 타워는 그 위치로 즉시 이동합니다.
5. 취소하려면 `Esc`를 누릅니다. 원래 위치는 유지됩니다.

재배치 버튼은 선택 상태에 따라 자동으로 비활성화되지 않습니다. 아무 대상도 선택하지 않았거나 적·본영을 선택한 상태에서 누르면 화면 반응 없이 Console에만 경고가 나올 수 있습니다.

### 현재 보이는 버튼과 미구현 기능

- `타워 철거` 버튼은 보이지만 아직 작동하지 않습니다.
- 스킬, 필살기, 연계 스킬 조작은 없습니다.
- 인게임 일시정지, 배속, 설정 메뉴가 없습니다. 멈춰 확인하려면 Unity 상단의 Pause 버튼을 사용합니다.
- 체력 바와 공격 범위 표시는 아직 없습니다.
- 승리·패배 전용 화면은 없습니다. 위쪽 Debug Text에 `Game Victory!` 또는 `Game Defeat!`가 표시된 뒤 게임 상호작용이 멈춥니다.
- 승패 문구는 이후 지형 클릭 메시지로 덮일 수 있으므로 나타났을 때 바로 기록합니다. 승패 뒤에도 카메라 조작은 가능합니다.
- 유효하지 않은 배치·선택 안내 대부분은 Console에만 표시됩니다.

Console은 `Window > General > Console`에서 열 수 있습니다. 테스트 시작 전에 `Clear`로 이전 메시지를 지우고, 문제가 생기면 빨간 Error의 전체 문구와 첫 번째 관련 Stack Trace를 함께 남깁니다.

## 4. ScriptableObject 설정 바꾸기

### 공통 수정 절차

ElementalDef 전투에서 직접 사용하는 설정은 Project 창의 두 위치에 있습니다.

- `Assets/Settings/ElementalDef/Combat`
- `Assets/Settings/Wave Schedule`

수정 순서:

1. **Play Mode를 먼저 종료**합니다. ScriptableObject는 Play Mode 중 바꾼 값도 자산에 남을 수 있습니다.
2. Project 창에서 아래 설명에 적힌 `.asset` 파일을 선택합니다.
3. Inspector에서 값을 수정합니다. Project 창에 보이는 파일 이름으로 검색해도 됩니다.
4. 배열은 왼쪽 삼각형으로 펼치고 `+`, `-` 또는 `Size`로 항목을 관리합니다.
5. `Entity` 같은 자산 참조는 오른쪽의 작은 원형 선택 버튼을 누르거나 Project 창의 프리팹을 해당 칸에 드래그합니다.
6. `File > Save Project`로 저장한 뒤 Play Mode에서 확인합니다.
7. 한 번에 한 종류의 값만 바꾸고, 변경 전·후 값과 결과를 기록합니다.

`.asset`과 `.meta` 파일을 Finder/Explorer나 텍스트 편집기에서 직접 수정·이동·삭제하지 않습니다. 설정 자산을 Duplicate하거나 새로 만들어도 현재 씬이 기존 원본을 계속 참조하므로 자동으로 적용되지 않습니다.

### 4-1. 속성 상성 배율

자산: `Assets/Settings/ElementalDef/Combat/ElementalDef Base Elemental Affinity Settings.asset`

상성 순환은 **물 → 불 → 흙 → 물**입니다. 공격 속성과 대상의 방어 속성을 비교합니다.

| Inspector 필드 | 현재 값 | 설명 |
| --- | ---: | --- |
| `Advantage Multiplier` | `1.30` | 유리한 상성의 공격 배율. 130%, 즉 30% 증가 |
| `Neutral Multiplier` | `1.00` | 같은 속성이거나 어느 한쪽이 무속성일 때의 배율 |
| `Disadvantage Multiplier` | `0.80` | 불리한 상성의 공격 배율. 80%, 즉 20% 감소 |

값은 0 이상이며 상한이 따로 없습니다. 비교 테스트는 한 번에 `0.05`~`0.10` 정도씩 바꾸는 것을 권장합니다.

### 4-2. 속성 지형 보정

자산: `Assets/Settings/ElementalDef/Combat/ElementalDef Base Terrain Modifier.asset`

공격자는 자신이 서 있는 지형에서 공격 보정을, 방어자는 자신이 서 있는 지형에서 방어력 보정을 받습니다.

| Inspector 필드 | 현재 값 | 설명 |
| --- | ---: | --- |
| `Same Element Attack Multiplier` | `1.15` | 공격 속성과 공격자 지형이 같을 때 공격력 115% |
| `Same Element Defense Multiplier` | `1.10` | 방어 속성과 방어자 지형이 같을 때 방어력 110% |
| `Neutral Attack Multiplier` | `1.00` | 무속성 또는 지형상 불이익이 없을 때 공격력 100% |
| `Neutral Defense Multiplier` | `1.00` | 무속성 또는 지형상 불이익이 없을 때 방어력 100% |
| `Disadvantage Attack Multiplier` | `0.85` | 지형 속성이 유닛 속성을 이길 때 공격력 85% |
| `Disadvantage Defense Multiplier` | `0.90` | 지형 속성이 유닛 속성을 이길 때 방어력 90% |

유닛 속성이 지형 속성에 유리한 경우에는 추가 보너스가 없고 `Neutral`이 적용됩니다. `Defense Multiplier`는 받는 피해 전체가 아니라 **방어력 수치**에 곱해집니다. 현재 적의 기본 방어력은 0이므로 적에게 지형 방어 배율만 바꾸어도 차이가 나지 않습니다.

현재 일반 공격의 피해 계산은 다음 구조입니다.

```text
최종 피해 = max(0,
    기본 공격력 × 스킬 배율 × 속성 상성 배율 × 공격자 지형 공격 배율
    - 방어력 × 방어자 지형 방어 배율)
```

현재 자동 일반 공격의 스킬 배율은 `1.0`입니다.

### 4-3. 웨이브 순서

자산: `Assets/Settings/Wave Schedule/ElementalDef Base Wave Bundle.asset`

`Waves` 배열의 **위에서 아래 순서**가 실제 플레이 순서입니다. 각 칸은 아래의 `ElementalDef Wave 01`~`05` 자산을 참조합니다. 순서를 바꾸면 게임의 웨이브 순서도 바뀝니다.

### 4-4. 웨이브별 적 소환

자산 위치: `Assets/Settings/Wave Schedule/ElementalDef Wave 01.asset` ~ `ElementalDef Wave 05.asset`

| Inspector 필드 | 설명 |
| --- | --- |
| `Entries` | 해당 웨이브에서 소환할 항목 목록 |
| `Turn` | 웨이브 시작 후 몇 번째 Turn에 소환할지 지정. 현재 1 Turn은 1초 |
| `Entity` | 해당 Turn에 한 개 소환할 적 프리팹 |

현재 기본 구성:

| 웨이브 | 소환 Turn | 적 |
| --- | --- | --- |
| Wave 01 | 5 | Earth Small |
| Wave 02 | 5 | Fire Small |
| Wave 03 | 5 | Water Small |
| Wave 04 | 3 / 6 / 9 | Earth / Fire / Water |
| Wave 05 | 3 / 5 / 7 | Water / Fire / Earth |

웨이브 설정에는 현재 구현상 중요한 제약이 있습니다.

- `Turn`은 Inspector에서 0을 넣을 수 있어도 **반드시 1 이상**을 사용합니다. 0 Turn의 적은 소환되지 않습니다.
- 같은 Wave 안의 `Turn`은 **서로 달라야 합니다**. 같은 Turn에 여러 항목을 넣으면 첫 적만 소환되고 웨이브가 끝나지 않을 수 있습니다.
- `Entity`를 `None`으로 두지 않습니다.
- 각 `WaveSchedule`의 `Entries`와 `WaveBundle`의 `Waves`를 비워 두지 않습니다.
- 같은 시점에 여러 적을 내보내고 싶다면 현재는 같은 Turn을 중복하지 말고 인접한 Turn으로 나눕니다.
- 한 웨이브는 예약된 적을 모두 소환하고 살아 있는 적을 모두 처치해야 종료됩니다.

게임이 다음 웨이브로 넘어가지 않으면 먼저 위 조건과 Console 오류를 확인합니다.

### ScriptableObject 밖에 있는 전투 값

아래 값은 아직 ScriptableObject로 분리되지 않았습니다. 일반 밸런스 테스트에서는 위치만 확인하고, 수정이 필요하면 개발 담당자와 범위를 맞춘 뒤 프리팹을 편집합니다.

| 대상 | 위치 | 현재 주요 값 |
| --- | --- | --- |
| 불·물·흙 타워 | `Assets/Prefabs/ElementalDef/Towers` | HP 100, 공격력 10, 사거리 5, 공격 간격 1초, 탐색 반경 3, 방어력 1 |
| 불·물·흙 Small 적 | `Assets/Prefabs/ElementalDef/Enemies` | HP 100, 공격력 5, 사거리 5, 공격 간격 1초, 탐색 반경 2, 방어력 0, 이동 속도 1 |
| 본영 | `ElementalDefMain` 씬의 `Headquarters` | HP 100 |

`MapGenerator` Inspector의 `Generate`, `Generate Demo`, `Clear` 버튼은 일반 전투 테스트에서 누르지 않습니다. 타일맵을 즉시 덮어쓰지만 적 경로와 NavMesh를 함께 갱신하지 않아 현재 씬을 실행할 수 없는 상태로 만들 수 있습니다.

## 5. 권장 테스트 항목

한 세션에서 모든 것을 보려 하기보다 아래 항목을 나누어 확인하는 편이 좋습니다.

1. **실행 안정성**: 첫 웨이브 자동 시작, 5개 웨이브 전환, 승리·패배 시 정지
2. **배치**: 세 속성 타워 건설, 경로·겹침 위치 차단, 빨강/파랑 셀 표시
3. **재배치**: 아군 타워만 이동 가능, 취소 시 원위치 유지, 이동 후 자동 전투 재개
4. **속성 상성**: 물→불, 불→흙, 흙→물의 피해 증가와 역상성 피해 감소
5. **지형 보정**: 동일 속성 지형 보너스와 불리한 지형 페널티
6. **설정 반영**: 상성·지형 배율 또는 Wave 자산을 한 항목만 바꾼 뒤 재실행하여 차이 확인
7. **오류 확인**: Console의 Warning/Error, 멈춘 웨이브, 공격하지 않는 유닛, 잘못된 배치 상태

### 피드백에 함께 적을 정보

```text
[테스트 환경]
- 받은 경로: Git / UVCS
- Git 브랜치와 커밋 또는 UVCS 브랜치와 변경집합:
- Unity 버전:
- 테스트한 날짜:

[변경한 설정]
- 자산 이름:
- 변경 전 값 → 변경 후 값:

[결과]
- 재현 순서:
- 기대 결과:
- 실제 결과:
- 재현 빈도:
- 스크린샷 또는 영상:
- Console Warning/Error 전체 문구:
```

카메라 각도와 타워 위치가 중요하므로, 문제 상황은 가능하면 Game 뷰 전체가 보이는 스크린샷이나 짧은 영상으로 남깁니다.

## 6. 문제 해결

| 증상 | 확인할 내용 |
| --- | --- |
| Unity Hub에서 프로젝트로 인식하지 못함 | 저장소 루트가 아니라 내부 `DefCity` 폴더를 선택했는지 확인 |
| 다른 장면이 실행됨 | `Assets/Scenes/ElementalDefMain.unity`를 직접 열고 Editor Play 했는지 확인 |
| 키보드·마우스가 작동하지 않음 | Play 중 Game 탭을 한 번 클릭하고 포커스 확인 |
| 재질이 분홍색이거나 모델·텍스처가 비어 있음 | 임포트 완료 여부와 Git LFS 다운로드 여부 확인 후 화면 공유 |
| 타워가 공격하지 않음 | 적 탐색 반경 약 3셀 안에 경로가 있는지, 게임이 이미 종료됐는지 확인 |
| 건설·재배치가 되지 않음 | 파란 셀인지, 아군 타워가 선택됐는지 확인. 자세한 이유는 Console 확인 |
| 웨이브가 끝나지 않음 | Turn 0·중복 Turn·`Entity=None`·빈 배열 여부와 살아 있는 적 확인 |
| 승패 화면이 나오지 않음 | 현재는 정식 결과 화면이 없으며 Debug Text의 메시지만 확인 |
| Safe Mode 또는 빨간 컴파일 Error | 임의 수정하지 말고 브랜치/변경집합과 첫 Error를 개발 담당자에게 전달 |

## 7. 폴더 구조

```text
저장소 루트/
├── README.md                 # 현재 안내 문서
├── Documents/                # 기획·설계 문서
│   └── Elemental Defense Design_20260721_v2.docx
└── DefCity/                  # Unity 프로젝트 본체
    ├── Assets/
    │   ├── Scenes/           # 테스트 씬
    │   ├── Scripts/          # DefCore, DefCity, ElementalDef 코드
    │   ├── Settings/         # ScriptableObject 설정
    │   └── Prefabs/          # 타워·적·지형 프리팹
    ├── Packages/
    └── ProjectSettings/
```

## 8. 개발·공유 담당자 확인 사항

기획자에게 링크를 보내기 전에 다음을 확인합니다.

- 테스트에 필요한 씬, 코드, 프리팹, 설정과 `README.md`를 같은 원격 버전에 반영했는가
- 기준 문서 `Documents/Elemental Defense Design_20260721_v2.docx`가 실제로 버전 관리에 포함되었는가
- Git을 쓴다면 기획자에게 **브랜치명과 커밋 ID**, UVCS를 쓴다면 **브랜치와 변경집합 번호**를 전달했는가
- Git LFS 에셋이 원격에 모두 올라갔는가
- 깨끗한 새 폴더에 직접 받아 `6000.3.9f1`로 `ElementalDefMain`을 실행해 보았는가
- Git과 UVCS 중 어느 쪽을 이번 테스트의 기준으로 사용할지 하나만 지정했는가

Git 커밋 메시지는 다음 형식을 권장합니다.

```text
<type>: <English summary>

- 한국어 변경 사항
- 한국어 변경 사항
- 필요한 경우 변경 이유나 주의사항
```
