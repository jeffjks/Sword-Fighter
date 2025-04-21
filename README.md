[![Gameplay Demo](img/sample1.png)](https://www.youtube.com/watch?v=qITD623zyWI)
이미지 클릭 시 시연 유튜브 영상으로 이동

# 프로젝트 개요
- 유니티와 C# 데디케이티드 서버를 사용한 최대 4인 실시간 대전 게임.
- 다양한 동기화 기법을 적용하여 지연 환경에서도 일관되고 합리적인 플레이 경험을 제공

## 개발 정보
- 개발 인원: 1인
- 클라이언트: Unity
- 서버: C# .NET TCP 소켓 기반 데디케이티드 서버

## 주요 기능
### 🔧 네트워크 구조
- 클라이언트 예측 (Client-side Prediction)
- 서버 재조정 (Server Reconciliation)
- 지연 보상 (Lag Compensation)
- 데드 레커닝 기반 캐릭터 위치 보간

### 🧠 서버 아키텍처
- 권위적인(Authoritative) 서버 모델
- byte 배열 기반의 패킷 구조

### 🎮 게임 플레이
- 최대 4인 실시간 대전
- 평타 / 회피 / 방어 스킬 사용 가능


## TODO
- 피격 이펙트 등 그래픽 개선
- 다양한 스킬 추가

## 참고 자료
https://www.gabrielgambetta.com/client-server-game-architecture.html
https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking
https://noti.st/eiaserinnys/jCpSbp
https://blog.naver.com/linegamedev/221061964789
