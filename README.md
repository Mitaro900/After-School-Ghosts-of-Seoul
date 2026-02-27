# After-School-Ghosts-of-Seoul
Unity 기반 추리 어드벤처 게임입니다. \
NPC 대화 시스템은 별도의 Node.js 프록시 서버를 통해 OpenAI API와 통신합니다. \
제작자: 김재현, 박민규 \
개발 기간: 2026-02-02 ~ 2026-02-27

# 실행 방법
서버 저장소에서 먼저 서버를 실행해주세요: \
https://github.com/Mitaro900/OpenAI-Proxy

Unity에서 다음을 확인하세요:

🔹 OpenAIManager 설정 \
API_URL = http://localhost:3000/chat


필요 시: \
AppToken = (서버에서 사용하는 경우 동일 값 입력)

플레이 테스트:
1. Unity에서 Play ▶
2. 채팅창에서 메시지 입력
3. NPC 응답 출력 확인

🛠 요구사항

- Unity 2022 LTS 이상

- TextMeshPro

- Input System (New)

- Node.js 18+ (서버 실행 시)

# 기능 명세서

## 1. 플레이어 이동 & 입력

- 방향키 / 조이스틱 기반 이동
- E 키 상호작용: NPC, 아이템, 퀘스트 오브젝트, 텔레포트
- 이동, 대화, 전투, 컷씬 중 입력 차단
- 마지막 이동 방향 기록

## 2. NPC & 대화 시스템

- NPC 정보: 이름, 관련 퀘스트
- E 키 → 대화 UI 오픈
- ESC → 대화 종료
- 대화 기반 Step 적용 (퀘스트 연동)
- 대화 로그 기록 유지

## 3. 퀘스트 & 조사 시스템

- 퀘스트 ID, 이름, 관련 NPC, 완료 조건 관리
- Step 기반 진행
- 완료 조건: 아이템 소지, NPC 대화, 플래그
- 완료 시 플래그 설정 및 퀘스트 완료 처리
- 필수 조사 체크 → 포탈 이동 제한 가능

## 4. 포탈 & 맵 전환

- 플레이어 충돌 감지 → 위치 이동
- 카메라 스냅 처리
- 현재 TeleportZone 기록 관리

## 5. 컷씬

- PlayableDirector 기반 재생
- 재생 중 입력 차단
- 종료 후 씬 전환
- Intro / Ending 컷씬 지원

## 6. UI

- 대화 말풍선 관리
- 채팅 로그 기록
- 입력창 / ESC 처리

## 7. 사용자 설정

- 게임 설정 저장/로드 (BGM, SFX 음소거)
- 기본값 초기화 및 파일 저장

## 8. 상호작용 흐름 요약

| 대상 | 키 | 동작 |
| --- | --- | --- |
| NPC | E | 대화 UI 열기, 퀘스트 연동 |
| Item | E | 메시지 / 아이템 사용 |
| QuestObject | E | 퀘스트 메시지 |
| TeleportZone | E | 순간이동, 카메라 이동 |

## 9. 구조/비기능

- 싱글턴 기반 매니저: PlayerInputManager, AudioManager, GameManager, UserDataManager, CutsceneController
- ScriptableObject 기반 데이터 관리: NPC, 퀘스트, 아이템
- Null 참조 방지 & UI 재바인딩
- 대화 / 퀘스트 / 컷씬 / 전투 간 입력 관리
