# MiniGames — Unity 미니게임 플랫폼

> 하나의 베이스 씬 위에서 미니게임을 동적으로 로드/언로드하는 **확장형 미니게임 플랫폼**입니다.
> 플랫폼(허브)과 콘텐츠(미니게임)를 구조적으로 분리해, 새 게임을 "폴더 하나 + 데이터 에셋 하나"로 추가할 수 있도록 설계했습니다.

**정휘현** · Unity 클라이언트 프로그래머 (7년차) · silsen@naver.com
📂 시스템/툴 포트폴리오: [frenil-portfolio](https://github.com/Frenil-client/frenil-portfolio)

`Unity` `C#` `UGUI` `TextMeshPro` `Input System` `ScriptableObject` `2D`

---

## 프로젝트 포지션

[기존 포트폴리오](https://github.com/Frenil-client/frenil-portfolio)가 **재사용 가능한 시스템 단위**(에디터 툴 · 스탯 · 레드닷 · MVVM)를 다룬다면,
이 저장소는 그 설계 철학을 **실제 게임플레이 콘텐츠**에 적용한 결과물입니다.

- 시스템 설계 → 플랫폼/콘텐츠 생명주기 계약, 데이터 드리븐 콘텐츠 등록
- 에디터 툴 경험 → `SceneReference` + 커스텀 PropertyDrawer로 씬 참조를 드래그&드롭으로 관리
- UI 구조 설계 → 이벤트 드리븐 UI, 재사용(Recycle) 스크롤, UI 스택 내비게이션

---

## 아키텍처 개요

```
MainScene (항상 상주하는 허브)
├── MainSceneController     ― 게임 목록 / 상세 페이지 / 뒤로 가기 흐름
├── MiniGameLauncher        ― 미니게임 씬 Additive 로드/언로드, 생명주기 제어
├── UIStackManager          ― 패널 스택 내비게이션 (뒤로 가기 일원화)
├── RecycleScrollView       ― 슬롯 재사용 스크롤 (Recycle / Loop 모드)
└── GameDetailPanel         ― 썸네일 · 최고 기록 · 난이도 선택(◀ N ▶) · 시작

MiniGames/ (콘텐츠 ― 게임당 폴더 1개, 네임스페이스/접두사 분리)
├── RandomDefence (RD_*)    ― 랜덤 타워 디펜스
└── TimerMatch    (TM_*)    ― 스톱워치 타이밍 게임
```

### 플랫폼 ↔ 콘텐츠 분리 원칙

| 장치 | 역할 |
|------|------|
| `IMiniGame` | 모든 미니게임 루트가 구현하는 생명주기 계약 (`OnMiniGameStart` / `OnMiniGameExit`) |
| `MiniGameData` (SO) | 게임 등록 메타데이터 — 표시 정보, 진입 씬, 난이도 수, 해금 키. **코드 수정 없이 게임 추가** |
| `MiniGameSession` | 정적 브리지 — 미니게임은 런처를 직접 참조하지 않고 `RequestExit()`만 호출. 난이도 선택값(`launchParameter`)도 이 통로로 전달 |
| `GameScoreManager` | 게임별 최고 기록 공용 저장소 |
| 공통 규칙 | 게임 종료 시 항상 `Time.timeScale` 복구 + **메인 씬의 해당 게임 상세 페이지로 복귀** |

미니게임 쪽 코드는 플랫폼 어셈블리에 역참조가 없어, 씬 단독 실행(개발용 `TM_TestLauncher`)과 플랫폼 경유 실행이 동일한 코드로 동작합니다.

---

## MainScene (허브)

| 기능 | 구현 포인트 |
|------|------------|
| 게임 목록 스크롤 | 슬롯 풀링 기반 `RecycleScrollView` — 슬롯 수가 늘어도 GC/인스턴스 부담 없음, Loop 모드 지원 |
| 상세 페이지 | `MiniGameData` 기반 데이터 바인딩 (썸네일 · 배경 · 최고 기록) |
| 난이도 선택 | `stageCount > 0`인 게임만 ◀ ▶ 셀렉터 노출. PlayerPrefs 해금 상태와 연동, 최소/최대 난이도에서 방향 버튼 자동 숨김 |
| 씬 전환 | 로딩 씬을 경유하는 Additive 로드 — 메인 씬 상주, 게임 씬만 교체 |
| 뒤로 가기 | UI 스택 pop → 게임 종료 → 앱 종료 순의 단일 진입점 |

---

## 미니게임

> 각 게임은 아래 공통 포맷으로 정리합니다. 새 게임 추가 시 이 포맷을 복사해 확장합니다.

### 🎮 RandomDefence — 랜덤 타워 디펜스

| 항목 | 내용 |
|------|------|
| 장르 | 2D 랜덤 타워 디펜스 (모바일) |
| 진행 | 총 50라운드, 보스 라운드(15/30/45/50), 배치 슬롯 12개 |
| 코어 루프 | 재화로 뽑기 → 동일 타입·학년 합성으로 강화 → 웨이브 방어 |

**핵심 시스템**
- **가챠**: 등급 가중치 테이블(`GachaTableSO`) 기반 확률 뽑기, 확률 공개 패널
- **드래그&드롭 인터랙션**: 슬롯 이동/교환, 동일 타워 머지, 하단 판매 존 드롭 — 단일 포인터 상태 머신으로 마우스/터치 동시 지원
- **판매**: 선택 시 판매가 표시 → 탭 판매 + 드래그 투 셀, 가격 공식 `15 × 학년`
- **CC(상태이상)**: 스턴 / 슬로우 / 방어 무시 — 타워 데이터(SO)로 외부화
- **타입 레벨업**: 타입 공유 데미지 배율 성장축
- **전투 연출**: 몬스터 HP바, `MaterialPropertyBlock` 히트 플래시(머티리얼 인스턴스화 없음), 데미지/재화 팝업, 머지 이펙트

**기술 포인트**
- 드래그 중 메인 캔버스를 `ScreenSpaceOverlay → ScreenSpaceCamera`로 런타임 전환해 **드래그 중인 캐릭터만 UI 위에 렌더링** (소팅 레이어 체계로 HUD < 캐릭터 순서 제어, 종료 시 복원)
- 판매 존은 별도 ScreenSpace-Camera 캔버스로 분리해 월드/UI 소팅을 한 체계에서 관리
- 모든 수치(타워/몬스터/가챠/웨이브)는 ScriptableObject로 외부화 — 밸런싱이 코드와 분리
- `TimeScaleController` 단일 창구로 배속/일시정지/드래그 슬로우 충돌 방지

### 🎮 TimerMatch — 스톱워치 타이밍 게임

| 항목 | 내용 |
|------|------|
| 장르 | 타이밍 액션 (모바일) |
| 진행 | 스테이지당 5라운드 고정, 난이도 4종 (클리어 시 다음 난이도 해금) |
| 코어 루프 | 목표 시간 확인 → 게이지 인디케이터가 중앙(목표)에 올 때 정지 → 오차 판정 |

**핵심 시스템**
- **라운드 랜덤 생성**: 목표 시간(3~12초) · 진행 방향(카운트업/다운) · 배속(1.0~1.3x)을 매 라운드 런타임 생성 — 스테이지 데이터는 난이도 파라미터만 보유
- **오차 누적 게임오버**: 라운드 오차 절대값 누적이 난이도별 한도(4/3/2/7초) 초과 시 즉시 게임오버
- **계단식 점수**: 오차 구간별 고정 점수(PERFECT 10,000 ~ 5초 이상 0점)
- **타이머 숨김(고난이도)**: 숨김 시점부터 숫자가 랜덤 스크램블되며 페이드아웃, 게이지 인디케이터도 동반 페이드 — 배경/중앙 마커는 유지해 감각만으로 플레이
- **연출**: 1라운드 슬롯머신식 숫자 스크램블 → 확정 바운스 → 출발, 라운드 결과(±오차 색상/점수) 자동 전환, SUCCESS/GAME OVER 배너
- **일시정지**: 타이머가 실제로 도는 동안에만 진입 가능(인트로/결과 연출 중 차단), `Time.timeScale` 기반 동결

**기술 포인트**
- 상태 머신(`StageSelect / Playing / RoundResult / StageResult`) + C# 이벤트로 게임 로직과 UI 완전 분리 — UI는 전부 구독자
- 탭 정지 입력이 UI 버튼과 충돌하지 않도록 `EventSystem.RaycastAll`로 Selectable 위 입력만 선별 차단
- 진행/연출 타이밍(스크램블 시간, 결과 노출 시간 등)은 전부 직렬화 필드로 노출해 에디터에서 튜닝

### ➕ 새 미니게임 추가 가이드 (확장 포맷)

```
1. Assets/MiniGames/{게임명}/ 폴더 생성
   └─ Scripts/{Data, Core, UI} + 접두사·네임스페이스 분리 (예: XX_*)
2. 루트 프리팹/씬에 IMiniGame 구현체 배치
   └─ 종료는 MiniGameSession.RequestExit() 호출 (공통 규칙)
3. MiniGameData 에셋 생성 → 씬 연결, 난이도 메타 입력
4. MainSceneController의 게임 목록에 에셋 추가 → 끝
```

README에는 위의 게임 포맷(장르/진행/코어 루프 → 핵심 시스템 → 기술 포인트)으로 섹션을 추가합니다.

---

## 폴더 구조

```
Assets/
├── _Main/            # 허브 — 컨트롤러, 런처, 상세 페이지, 게임 등록 데이터
├── Shared/           # 공용 — MiniGameSession, GameScoreManager, SceneReference,
│                     #        RecycleScrollView, 에디터 유틸(PropertyDrawer 등)
├── MiniGames/
│   ├── RandomDefence/  # RD_* (Data / Core / Tower / Monster / UI)
│   └── TimerMatch/     # TM_* (Data / Core / UI)
└── Scenes/           # MainScene, LoadingScene, 게임별 진입 씬
```

---

## 연관 포트폴리오

| 저장소 | 이 프로젝트와의 관계 |
|--------|---------------------|
| [frenil-portfolio](https://github.com/Frenil-client/frenil-portfolio) | 포트폴리오 허브 — 경력 및 시스템 프로젝트 개요 |
| unity-mvvm | 본 프로젝트의 이벤트 드리븐 UI 분리 원칙의 프레임워크화 버전 |
| unity-stat-system | 데이터 외부화(SO)·무결성 검증 설계의 시스템 단위 사례 |
| unity-reddot-system | UI 상태 전파 구조 설계 사례 |
| unity-maplightdata-tool | 에디터 자동화·렌더링 파이프라인 툴링 사례 |
