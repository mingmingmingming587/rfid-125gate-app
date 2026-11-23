using System;

namespace _125GateApp.Models
{
    /// <summary>
    /// 校园门禁卡片状态常量。
    /// </summary>
    public static class CardStatuses
    {
        public const string Normal = "正常";
        public const string Lost = "挂失";
        public const string Disabled = "禁用";

        public static readonly string[] All = new string[] { Normal, Lost, Disabled };
    }

    /// <summary>
    /// 校园人员身份类型常量。
    /// </summary>
    public static class CampusRoles
    {
        public const string Undergraduate = "本科生";
        public const string Postgraduate = "研究生";
        public const string Teacher = "教师";
        public const string Admin = "管理员";
        public const string Visitor = "访客";

        public static readonly string[] All = new string[]
        {
            Undergraduate, Postgraduate, Teacher, Admin, Visitor
        };
    }

    /// <summary>
    /// 校园门禁区域常量。
    /// </summary>
    public static class CampusZones
    {
        public static readonly string[] All = new string[]
        {
            "全区通行",
            "实训楼125K实验室",
            "教学楼A区",
            "图书馆通道",
            "教工办公区"
        };
    }

    /// <summary>
    /// 校园常用院系/单位列表。
    /// </summary>
    public static class CampusDepartments
    {
        public static readonly string[] All = new string[]
        {
            "物联网工程系",
            "计算机科学与技术学院",
            "电子信息工程学院",
            "人工智能与自动化学院",
            "校园信息中心",
            "校外访客"
        };
    }

    /// <summary>
    /// 门禁卡与校园持卡人信息模型（向后兼容原 db.json 字段）。
    /// </summary>
    public class CampusCardRecord
    {
        public string cardnum { get; set; }
        public string name { get; set; }
        public string stuid { get; set; }
        public string sex { get; set; }
        public string role { get; set; }
        public string department { get; set; }
        public string gateZone { get; set; }
        public string status { get; set; }
        public string validUntil { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }

        public CampusCardRecord()
        {
            cardnum = string.Empty;
            name = string.Empty;
            stuid = string.Empty;
            sex = "男";
            role = CampusRoles.Undergraduate;
            department = CampusDepartments.All[0];
            gateZone = CampusZones.All[0];
            status = CardStatuses.Normal;
            validUntil = "长期有效";
            createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            updatedAt = createdAt;
        }

        /// <summary>
        /// 补全旧版 JSON 缺失的校园扩展字段默认值。
        /// </summary>
        public void NormalizeDefaults()
        {
            if (cardnum == null) cardnum = string.Empty;
            cardnum = cardnum.Trim();
            if (name == null) name = string.Empty;
            if (stuid == null) stuid = string.Empty;
            if (string.IsNullOrEmpty(sex)) sex = "男";
            if (string.IsNullOrEmpty(role)) role = CampusRoles.Undergraduate;
            if (string.IsNullOrEmpty(department)) department = CampusDepartments.All[0];
            if (string.IsNullOrEmpty(gateZone)) gateZone = CampusZones.All[0];
            if (string.IsNullOrEmpty(status)) status = CardStatuses.Normal;
            if (string.IsNullOrEmpty(validUntil)) validUntil = "长期有效";
            if (string.IsNullOrEmpty(createdAt)) createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrEmpty(updatedAt)) updatedAt = createdAt;
        }

        /// <summary>
        /// 检查当前卡片是否已过有效期。
        /// </summary>
        public bool IsExpired()
        {
            if (string.IsNullOrEmpty(validUntil) || validUntil == "长期有效")
            {
                return false;
            }
            DateTime dt;
            if (DateTime.TryParse(validUntil, out dt))
            {
                return DateTime.Today > dt.Date;
            }
            return false;
        }
    }

    /// <summary>
    /// 门禁刷卡通行与控制日志条目。
    /// </summary>
    public class AccessLogEntry
    {
        public string id { get; set; }
        public string timestamp { get; set; }
        public string cardnum { get; set; }
        public string name { get; set; }
        public string stuid { get; set; }
        public string role { get; set; }
        public string department { get; set; }
        public string eventType { get; set; }
        public string result { get; set; }
        public string source { get; set; }
        public string detail { get; set; }

        public AccessLogEntry()
        {
            id = Guid.NewGuid().ToString("N").Substring(0, 8);
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            cardnum = string.Empty;
            name = "-";
            stuid = "-";
            role = "-";
            department = "-";
            eventType = string.Empty;
            result = string.Empty;
            source = string.Empty;
            detail = string.Empty;
        }
    }

    /// <summary>
    /// 串口连接配置模型（对应 GlobalCom.json）。
    /// </summary>
    public class ComConfig
    {
        public string COM { get; set; }
        public int BAUD { get; set; }

        public ComConfig()
        {
            COM = "COM3";
            BAUD = 57600;
        }
    }

    /// <summary>
    /// 门禁系统运行参数配置（对应 settings.json）。
    /// </summary>
    public class AppSettings
    {
        public int OpenAngle { get; set; }
        public bool AutoCloseEnabled { get; set; }
        public int AutoCloseSeconds { get; set; }
        public int PollIntervalMs { get; set; }
        public int DebounceSeconds { get; set; }
        public bool EnableAcoustoAlarm { get; set; }
        public bool EnableOnboardBuzzer { get; set; }
        public bool SimulationMode { get; set; }
        public string CurrentGateZone { get; set; }

        public AppSettings()
        {
            OpenAngle = 90;
            AutoCloseEnabled = true;
            AutoCloseSeconds = 5;
            PollIntervalMs = 800;
            DebounceSeconds = 3;
            EnableAcoustoAlarm = true;
            EnableOnboardBuzzer = true;
            SimulationMode = false;
            CurrentGateZone = "实训楼125K实验室";
        }
    }
}
