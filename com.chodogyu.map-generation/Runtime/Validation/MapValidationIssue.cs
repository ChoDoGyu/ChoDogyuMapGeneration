namespace CDG.MapGeneration
{
    /// <summary>
    /// Map Validation 과정에서 발견된 하나의 문제를 나타냅니다.
    /// </summary>
    public sealed class MapValidationIssue
    {
        /// <summary>
        /// 문제를 식별하기 위한 안정적인 코드입니다.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// 문제에 대한 설명입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 문제의 심각도입니다.
        /// </summary>
        public MapValidationSeverity Severity { get; }

        internal MapValidationIssue(string code, string message, MapValidationSeverity severity)
        {
            Code = code;
            Message = message;
            Severity = severity;
        }
    }
}