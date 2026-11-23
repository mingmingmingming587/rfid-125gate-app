using _125GateApp.Models;
using System;

namespace _125GateApp.Services
{
    public class AccessDecision
    {
        public bool Granted { get; set; }
        public string EventType { get; set; }
        public string ResultText { get; set; }
        public string Reason { get; set; }
        public CampusCardRecord MatchedCard { get; set; }
    }

    /// <summary>
    /// 校园门禁权限校验规则引擎。
    /// </summary>
    public static class AccessEvaluator
    {
        public static AccessDecision Evaluate(string cardNumber, string currentGateZone)
        {
            CampusCardRecord record = JsonFormat.FindCard(cardNumber);
            if (record == null)
            {
                return new AccessDecision
                {
                    Granted = false,
                    EventType = "未注册拦截",
                    ResultText = "拒绝并报警",
                    Reason = string.Format("卡号 {0} 未在校园门禁库中注册", cardNumber),
                    MatchedCard = null
                };
            }

            if (record.status == CardStatuses.Lost)
            {
                return new AccessDecision
                {
                    Granted = false,
                    EventType = "挂失卡拦截",
                    ResultText = "拒绝并报警",
                    Reason = string.Format("{0}（{1}）的门禁卡已挂失，禁止通行", record.name, record.stuid),
                    MatchedCard = record
                };
            }

            if (record.status == CardStatuses.Disabled)
            {
                return new AccessDecision
                {
                    Granted = false,
                    EventType = "禁用卡拦截",
                    ResultText = "拒绝并报警",
                    Reason = string.Format("{0}（{1}）的门禁卡已被禁用", record.name, record.stuid),
                    MatchedCard = record
                };
            }

            if (record.IsExpired())
            {
                return new AccessDecision
                {
                    Granted = false,
                    EventType = "过期卡拦截",
                    ResultText = "拒绝并报警",
                    Reason = string.Format("{0} 的门禁卡已于 {1} 过期", record.name, record.validUntil),
                    MatchedCard = record
                };
            }

            if (!string.IsNullOrEmpty(currentGateZone) &&
                !string.IsNullOrEmpty(record.gateZone) &&
                record.gateZone != "全区通行" &&
                record.gateZone != currentGateZone)
            {
                return new AccessDecision
                {
                    Granted = false,
                    EventType = "越区卡拦截",
                    ResultText = "拒绝并报警",
                    Reason = string.Format("仅限 [{0}] 通行，无 [{1}] 权限", record.gateZone, currentGateZone),
                    MatchedCard = record
                };
            }

            return new AccessDecision
            {
                Granted = true,
                EventType = "合法通行",
                ResultText = "允许通行",
                Reason = string.Format("欢迎 {0}（{1} · {2}）通行", record.name, record.role, record.department),
                MatchedCard = record
            };
        }
    }
}
