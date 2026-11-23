using _125GateApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace _125GateApp
{
    /// <summary>
    /// 门禁本地 JSON 数据存储与 CSV 导出工具（兼容原 ReadSensorJson / AddJson 接口）。
    /// </summary>
    public class JsonFormat
    {
        private static readonly object _fileLock = new object();

        /// <summary>
        /// 解析数据根目录：依次检查当前工作目录、应用程序基目录以及程序集所在目录。
        /// </summary>
        public static string GetAppBaseDirectory()
        {
            string currentDir = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(currentDir) &&
                Directory.Exists(Path.Combine(currentDir, "plugin", "125GateApp")))
            {
                return currentDir;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir) &&
                Directory.Exists(Path.Combine(baseDir, "plugin", "125GateApp")))
            {
                return baseDir;
            }

            string asmDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(asmDir))
            {
                return asmDir;
            }

            return string.IsNullOrEmpty(baseDir) ? currentDir : baseDir;
        }

        public static string GetPluginDirectory()
        {
            string dir = Path.Combine(GetAppBaseDirectory(), "plugin", "125GateApp");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static string GetDbPath()
        {
            return Path.Combine(GetPluginDirectory(), "db.json");
        }

        public static string GetLogsPath()
        {
            return Path.Combine(GetPluginDirectory(), "logs.json");
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(GetPluginDirectory(), "settings.json");
        }

        public static string GetComConfigPath()
        {
            return Path.Combine(GetAppBaseDirectory(), "GlobalCom.json");
        }

        /// <summary>
        /// 确保运行时所需基础文件存在（例如在全新 Release 目录首次运行）。
        /// </summary>
        public static void EnsureInitialized()
        {
            lock (_fileLock)
            {
                string comPath = GetComConfigPath();
                if (!File.Exists(comPath))
                {
                    ComConfig defCom = new ComConfig();
                    File.WriteAllText(comPath, JsonConvert.SerializeObject(defCom, Formatting.Indented), Encoding.UTF8);
                }

                string dbPath = GetDbPath();
                if (!File.Exists(dbPath))
                {
                    List<CampusCardRecord> seed = CreateInitialDemoCards();
                    File.WriteAllText(dbPath, JsonConvert.SerializeObject(seed, Formatting.Indented), Encoding.UTF8);
                }

                string logsPath = GetLogsPath();
                if (!File.Exists(logsPath))
                {
                    File.WriteAllText(logsPath, "[]", Encoding.UTF8);
                }

                string settingsPath = GetSettingsPath();
                if (!File.Exists(settingsPath))
                {
                    AppSettings defSettings = new AppSettings();
                    File.WriteAllText(settingsPath, JsonConvert.SerializeObject(defSettings, Formatting.Indented), Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// 兼容原工程接口：读取 db.json 为 JArray。
        /// </summary>
        public static JArray ReadSensorJson()
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                string jsonfile = GetDbPath();
                if (!File.Exists(jsonfile))
                {
                    return new JArray();
                }

                string text = File.ReadAllText(jsonfile, Encoding.UTF8).Trim();
                if (string.IsNullOrEmpty(text))
                {
                    return new JArray();
                }

                return JArray.Parse(text);
            }
        }

        /// <summary>
        /// 兼容原工程接口：按卡号新增或更新基础人员信息。
        /// </summary>
        public static bool AddJson(string cardnum, string name, string stuid, string sex)
        {
            CampusCardRecord existing = FindCard(cardnum);
            CampusCardRecord record = existing ?? new CampusCardRecord();
            record.cardnum = cardnum == null ? string.Empty : cardnum.Trim();
            record.name = name == null ? string.Empty : name.Trim();
            record.stuid = stuid == null ? string.Empty : stuid.Trim();
            record.sex = string.IsNullOrEmpty(sex) ? "男" : sex;
            if (existing == null)
            {
                record.role = CampusRoles.Undergraduate;
                record.department = CampusDepartments.All[0];
                record.gateZone = CampusZones.All[0];
                record.status = CardStatuses.Normal;
                record.validUntil = "长期有效";
            }
            record.updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return UpsertCard(record);
        }

        /// <summary>
        /// 加载全部校园门禁卡记录并补全扩展字段。
        /// </summary>
        public static List<CampusCardRecord> LoadCards()
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    string jsonfile = GetDbPath();
                    if (!File.Exists(jsonfile))
                    {
                        return new List<CampusCardRecord>();
                    }

                    string text = File.ReadAllText(jsonfile, Encoding.UTF8).Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        return new List<CampusCardRecord>();
                    }

                    List<CampusCardRecord> list = JsonConvert.DeserializeObject<List<CampusCardRecord>>(text);
                    if (list == null)
                    {
                        list = new List<CampusCardRecord>();
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null)
                        {
                            list[i].NormalizeDefaults();
                        }
                    }
                    return list;
                }
                catch
                {
                    return new List<CampusCardRecord>();
                }
            }
        }

        public static bool SaveCards(List<CampusCardRecord> cards)
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    string jsonfile = GetDbPath();
                    List<CampusCardRecord> safeList = cards ?? new List<CampusCardRecord>();
                    for (int i = 0; i < safeList.Count; i++)
                    {
                        safeList[i].NormalizeDefaults();
                    }
                    string content = JsonConvert.SerializeObject(safeList, Formatting.Indented);
                    File.WriteAllText(jsonfile, content, Encoding.UTF8);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static CampusCardRecord FindCard(string cardnum)
        {
            if (string.IsNullOrEmpty(cardnum))
            {
                return null;
            }
            string target = cardnum.Trim();
            List<CampusCardRecord> cards = LoadCards();
            for (int i = 0; i < cards.Count; i++)
            {
                if (string.Equals(cards[i].cardnum, target, StringComparison.OrdinalIgnoreCase))
                {
                    return cards[i];
                }
            }
            return null;
        }

        public static bool UpsertCard(CampusCardRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.cardnum))
            {
                return false;
            }

            record.NormalizeDefaults();
            record.updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            List<CampusCardRecord> cards = LoadCards();
            bool found = false;
            for (int i = 0; i < cards.Count; i++)
            {
                if (string.Equals(cards[i].cardnum, record.cardnum, StringComparison.OrdinalIgnoreCase))
                {
                    record.createdAt = cards[i].createdAt;
                    cards[i] = record;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                if (string.IsNullOrEmpty(record.createdAt))
                {
                    record.createdAt = record.updatedAt;
                }
                cards.Add(record);
            }

            return SaveCards(cards);
        }

        public static bool DeleteCard(string cardnum)
        {
            if (string.IsNullOrEmpty(cardnum))
            {
                return false;
            }

            string target = cardnum.Trim();
            List<CampusCardRecord> cards = LoadCards();
            int removed = cards.RemoveAll(delegate(CampusCardRecord c)
            {
                return string.Equals(c.cardnum, target, StringComparison.OrdinalIgnoreCase);
            });

            if (removed > 0)
            {
                return SaveCards(cards);
            }
            return false;
        }

        public static bool UpdateCardStatus(string cardnum, string newStatus)
        {
            CampusCardRecord card = FindCard(cardnum);
            if (card == null)
            {
                return false;
            }
            card.status = newStatus;
            return UpsertCard(card);
        }

        /// <summary>
        /// 导入校园演示样本数据（不覆盖已有卡号，返回新增条数）。
        /// </summary>
        public static int ImportDemoCampusCards()
        {
            List<CampusCardRecord> existing = LoadCards();
            List<CampusCardRecord> demos = CreateInitialDemoCards();
            int added = 0;

            for (int i = 0; i < demos.Count; i++)
            {
                CampusCardRecord demo = demos[i];
                bool exists = false;
                for (int j = 0; j < existing.Count; j++)
                {
                    if (string.Equals(existing[j].cardnum, demo.cardnum, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    existing.Add(demo);
                    added++;
                }
            }

            if (added > 0)
            {
                SaveCards(existing);
            }
            return added;
        }

        private static List<CampusCardRecord> CreateInitialDemoCards()
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return new List<CampusCardRecord>
            {
                new CampusCardRecord
                {
                    cardnum = "0006849456",
                    name = "张明远",
                    stuid = "2023010101",
                    sex = "男",
                    role = CampusRoles.Undergraduate,
                    department = "物联网工程系",
                    gateZone = "全区通行",
                    status = CardStatuses.Normal,
                    validUntil = "2027-06-30",
                    createdAt = now,
                    updatedAt = now
                },
                new CampusCardRecord
                {
                    cardnum = "0005860000",
                    name = "林晓晴",
                    stuid = "2023010218",
                    sex = "女",
                    role = CampusRoles.Postgraduate,
                    department = "电子信息工程学院",
                    gateZone = "实训楼125K实验室",
                    status = CardStatuses.Normal,
                    validUntil = "2027-06-30",
                    createdAt = now,
                    updatedAt = now
                },
                new CampusCardRecord
                {
                    cardnum = "0001202659",
                    name = "王建国",
                    stuid = "T10086",
                    sex = "男",
                    role = CampusRoles.Teacher,
                    department = "物联网工程系",
                    gateZone = "全区通行",
                    status = CardStatuses.Normal,
                    validUntil = "长期有效",
                    createdAt = now,
                    updatedAt = now
                },
                new CampusCardRecord
                {
                    cardnum = "0008812345",
                    name = "陈思雨",
                    stuid = "2022040312",
                    sex = "女",
                    role = CampusRoles.Undergraduate,
                    department = "计算机科学与技术学院",
                    gateZone = "实训楼125K实验室",
                    status = CardStatuses.Lost,
                    validUntil = "2027-06-30",
                    createdAt = now,
                    updatedAt = now
                },
                new CampusCardRecord
                {
                    cardnum = "0007765432",
                    name = "临时访客01",
                    stuid = "V2026001",
                    sex = "男",
                    role = CampusRoles.Visitor,
                    department = "校外访客",
                    gateZone = "实训楼125K实验室",
                    status = CardStatuses.Disabled,
                    validUntil = "2026-01-01",
                    createdAt = now,
                    updatedAt = now
                }
            };
        }

        public static List<AccessLogEntry> LoadLogs()
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    string path = GetLogsPath();
                    if (!File.Exists(path))
                    {
                        return new List<AccessLogEntry>();
                    }
                    string text = File.ReadAllText(path, Encoding.UTF8).Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        return new List<AccessLogEntry>();
                    }
                    List<AccessLogEntry> list = JsonConvert.DeserializeObject<List<AccessLogEntry>>(text);
                    return list ?? new List<AccessLogEntry>();
                }
                catch
                {
                    return new List<AccessLogEntry>();
                }
            }
        }

        public static bool AppendLog(AccessLogEntry entry)
        {
            if (entry == null)
            {
                return false;
            }
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    List<AccessLogEntry> logs = LoadLogs();
                    logs.Insert(0, entry);
                    if (logs.Count > 2000)
                    {
                        logs.RemoveRange(2000, logs.Count - 2000);
                    }
                    File.WriteAllText(GetLogsPath(), JsonConvert.SerializeObject(logs, Formatting.Indented), Encoding.UTF8);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool ClearLogs()
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    File.WriteAllText(GetLogsPath(), "[]", Encoding.UTF8);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static AppSettings LoadSettings()
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    string path = GetSettingsPath();
                    if (!File.Exists(path))
                    {
                        return new AppSettings();
                    }
                    string text = File.ReadAllText(path, Encoding.UTF8).Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        return new AppSettings();
                    }
                    AppSettings settings = JsonConvert.DeserializeObject<AppSettings>(text);
                    return settings ?? new AppSettings();
                }
                catch
                {
                    return new AppSettings();
                }
            }
        }

        public static bool SaveSettings(AppSettings settings)
        {
            if (settings == null)
            {
                return false;
            }
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    File.WriteAllText(GetSettingsPath(), JsonConvert.SerializeObject(settings, Formatting.Indented), Encoding.UTF8);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static ComConfig LoadComConfig()
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    string path = GetComConfigPath();
                    if (!File.Exists(path))
                    {
                        return new ComConfig();
                    }
                    string text = File.ReadAllText(path, Encoding.UTF8).Trim();
                    ComConfig cfg = JsonConvert.DeserializeObject<ComConfig>(text);
                    if (cfg == null)
                    {
                        return new ComConfig();
                    }
                    if (string.IsNullOrEmpty(cfg.COM)) cfg.COM = "COM3";
                    if (cfg.BAUD <= 0) cfg.BAUD = 57600;
                    return cfg;
                }
                catch
                {
                    return new ComConfig();
                }
            }
        }

        public static bool SaveComConfig(string comName, int baud)
        {
            EnsureInitialized();
            lock (_fileLock)
            {
                try
                {
                    ComConfig cfg = new ComConfig
                    {
                        COM = string.IsNullOrEmpty(comName) ? "COM3" : comName.Trim(),
                        BAUD = baud > 0 ? baud : 57600
                    };
                    File.WriteAllText(GetComConfigPath(), JsonConvert.SerializeObject(cfg, Formatting.Indented), Encoding.UTF8);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void ExportCardsCsv(string filePath, IEnumerable<CampusCardRecord> cards)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("卡号,姓名,学号/工号,性别,身份类型,所属院系/单位,授权门区,卡状态,有效期,注册时间,更新时间");
            foreach (CampusCardRecord c in cards)
            {
                sb.AppendLine(string.Join(",", new string[]
                {
                    CsvEscape(c.cardnum),
                    CsvEscape(c.name),
                    CsvEscape(c.stuid),
                    CsvEscape(c.sex),
                    CsvEscape(c.role),
                    CsvEscape(c.department),
                    CsvEscape(c.gateZone),
                    CsvEscape(c.status),
                    CsvEscape(c.validUntil),
                    CsvEscape(c.createdAt),
                    CsvEscape(c.updatedAt)
                }));
            }
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        public static void ExportLogsCsv(string filePath, IEnumerable<AccessLogEntry> logs)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("日志ID,刷卡时间,卡号,姓名,学号/工号,身份类型,所属院系,事件类型,判定结果,触发来源,详情备注");
            foreach (AccessLogEntry log in logs)
            {
                sb.AppendLine(string.Join(",", new string[]
                {
                    CsvEscape(log.id),
                    CsvEscape(log.timestamp),
                    CsvEscape(log.cardnum),
                    CsvEscape(log.name),
                    CsvEscape(log.stuid),
                    CsvEscape(log.role),
                    CsvEscape(log.department),
                    CsvEscape(log.eventType),
                    CsvEscape(log.result),
                    CsvEscape(log.source),
                    CsvEscape(log.detail)
                }));
            }
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static string CsvEscape(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }
            string escaped = value.Replace("\"", "\"\"");
            return "\"" + escaped + "\"";
        }
    }
}
