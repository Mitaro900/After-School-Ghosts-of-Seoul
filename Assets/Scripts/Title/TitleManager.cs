using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 타이틀 화면을 관리하는 클래스
public class TitleManager : MonoBehaviour
{
    // 로고 애니메이션 컴포넌트
    public Animation LogoAnim;
    // 로고 텍스트 UI 컴포넌트
    public TextMeshProUGUI LogoTxt;

    // 타이틀 화면 게임오브젝트
    public GameObject Title;
    // 로딩 슬라이더 UI 컴포넌트
    public Slider LoadingSlider;
    // 로딩 진행률 텍스트 UI 컴포넌트
    public TextMeshProUGUI LoadingProgressTxt;

    // 비동기 씬 로딩 작업을 저장하는 변수
    private AsyncOperation m_AsyncOperation;

    // 오브젝트 활성화시 호출되는 메서드
    private void Awake()
    {
        // 로고 애니메이션 게임오브젝트 활성화
        LogoAnim.gameObject.SetActive(true);
        // 타이틀 화면 비활성화
        Title.SetActive(false);
    }

    // 첫 프레임 시작시 호출되는 메서드
    private void Start()
    {
        // 유저 데이터 매니저를 통한 데이터 로드
        UserDataManager.Instance.LoadUserData();

        AudioManager.Instance.OnLoadUserData(); // 오디오 매니저에 유저 데이터 로드

        // 게임 로딩 코루틴 시작
        StartCoroutine(LoadGameCo());
    }

    // 게임 로딩 코루틴 메서드
    private IEnumerator LoadGameCo()
    {
        // 게임 로딩 코루틴 시작 로그 출력
        Debug.Log($"{GetType()}::LoadGameCo");

        // 로고 애니메이션 재생
        LogoAnim.Play();
        // 로고 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(LogoAnim.clip.length);

        // 로고 애니메이션 게임오브젝트 비활성화
        LogoAnim.gameObject.SetActive(false);
        // 타이틀 화면 활성화
        Title.SetActive(true);

        // 로비 로딩 코루틴 시작 후 완료까지 대기
        yield return StartCoroutine(LoadLobbyCo());
    }

    // 로비 씬 로딩 코루틴 메서드
    private IEnumerator LoadLobbyCo()
    {
        // 로비 씬 비동기 로딩 시작
        m_AsyncOperation = SceneLoader.Instance.LoadSceneAsync(SceneType.Lobby);
        // 비동기 작업이 null인 경우
        if (m_AsyncOperation == null)
        {
            // 로비 비동기 로딩 에러 로그 출력
            Debug.LogError("Lobby async loading error.");
            // 코루틴 종료
            yield break;
        }

        // 씬 자동 활성화 비활성화 (수동으로 활성화하기 위해)
        m_AsyncOperation.allowSceneActivation = false;

        /*
         * 로드 시간이 짧은 경우 로딩 슬라이더 변화가 너무 빨리 보일 수 있다.
         * 일부로 첫 시작 50%로 설정하므로써 시간여유를 더 자연스럽게 처리한다.
         */
        // 로딩 슬라이더 값을 50%로 설정
        LoadingSlider.value = 0.5f;
        // 로딩 진행률 텍스트 업데이트
        LoadingProgressTxt.text = $"{(int)(LoadingSlider.value * 100)}%";
        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 로딩이 완료 될때까지 반복 주석
        while (!m_AsyncOperation.isDone) //로딩이 완료 될때까지 반복
        {
            // 로딩 진행률이 50% 미만이면 50%로, 아니면 실제 진행률로 설정
            LoadingSlider.value = m_AsyncOperation.progress < 0.5f ? 0.5f : m_AsyncOperation.progress;
            // 로딩 진행률 텍스트 업데이트
            LoadingProgressTxt.text = $"{(int)(LoadingSlider.value * 100)}%";

            // 씬 로딩이 완료되었다면 로비로 전환 처리하고 코루틴 종료
            // Unity에서 씬 로딩이 90% 완료된 상태를 나타냄
            // https://docs.unity3d.com/ScriptReference/AsyncOperation-progress.html
            // 로딩 진행률이 90%인 경우
            if (m_AsyncOperation.progress == 0.9f)
            {
                LoadingSlider.value = 1f;
                LoadingProgressTxt.text = "100%";

                // 씬 자동 활성화 허용
                yield return new WaitForSeconds(0.4f);

                yield return SceneLoader.Instance.FadeSceneOut(0.5f);

                m_AsyncOperation.allowSceneActivation = true;
                // 코루틴 종료
                yield break;
            }

            // 다음 프레임까지 대기
            yield return null;
        }
    }
}
