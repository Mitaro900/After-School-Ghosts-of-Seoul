# After-School-Ghosts-of-Seoul
Unity 기반 추리 어드벤처 게임입니다. \
NPC 대화 시스템은 별도의 Node.js 프록시 서버를 통해 OpenAI API와 통신합니다. \
제작자: 김재현, 박민규

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
