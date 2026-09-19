using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Map Validation 수행 결과와 발견된 Issue 목록을 제공합니다.
    /// Error가 하나도 없으면 유효한 Map으로 판단합니다.
    /// </summary>
    public sealed class MapValidationReport
    {
        private readonly ReadOnlyCollection<MapValidationIssue> issues;

        /// <summary>
        /// 발견된 모든 Validation Issue입니다.
        /// </summary>
        public IReadOnlyList<MapValidationIssue> Issues => issues;

        /// <summary>
        /// Error가 존재하지 않는지를 나타냅니다.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 발견된 Error 개수입니다.
        /// </summary>
        public int ErrorCount { get; }

        /// <summary>
        /// 발견된 Warning 개수입니다.
        /// </summary>
        public int WarningCount { get; }

        internal MapValidationReport(IReadOnlyList<MapValidationIssue> issues)
        {
            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }

            this.issues = new List<MapValidationIssue>(issues).AsReadOnly();

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i].Severity == MapValidationSeverity.Error)
                {
                    ErrorCount++;
                }
                else if (issues[i].Severity == MapValidationSeverity.Warning)
                {
                    WarningCount++;
                }
            }

            IsValid = ErrorCount == 0;
        }
    }
}