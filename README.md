# defcity-private

Base Unity game for defense game

## 폴더 구조
```text
Root Folder (DefCity)/
	- 리포지토리의 최상위 루트입니다.
	- `defcity.code-workspace`, 솔루션/프로젝트 파일, 공통 설정 파일처럼 프로젝트 전체에 영향을 주는 파일을 둡니다.

DefCity/
	- 유니티 프로젝트 본체입니다.
	- 실제 게임 실행에 필요한 소스코드, Unity 관련 프로젝트 파일, 패키지/설정 파일이 위치합니다.

Docs/
	- 현재 프로젝트와 관련된 문서를 보관하는 폴더입니다.
	- 기획, 설계, 규칙, 운영 메모처럼 프로젝트 진행에 필요한 문서를 여기에 둡니다.
	- main 브랜치의 `Docs`에는 가능하면 이미 확정된 내용, 검토를 마친 문서만 넣는 것을 권장합니다.
```

## Git Commit 메시지 가이드라인 제안

```
<type>: <English summary>

- 한국어 변경 사항
- 한국어 변경 사항
- 필요한 경우 변경 이유나 주의사항
```

## 주의 사항

- 현재 Git 저장소는 Git LFS를 사용하고 있습니다. Git LFS를 설치하지 않은 상태에서 커밋을 시도하면 오류가 발생할 수 있습니다. Git LFS를 설치한 후 커밋을 진행하시기 바랍니다.