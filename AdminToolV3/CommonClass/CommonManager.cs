namespace AdminToolV3.CommonClass
{
    public class CommonManager : Singleton<CommonManager>
    {
        // 프로세스가 시작된다음 한번은 꼭 실행하도록 사용하는 변수
        public bool IsExecutedDAUAfterStart { get; set; } = false;
    }
}
