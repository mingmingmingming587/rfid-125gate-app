using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace _125GateApp
{
    public class JsonFormat
    {

        public static JArray ReadSensorJson()
        {
            //string jsonfile = System.Environment.CurrentDirectory + "/Configs.json";
            string jsonfile = System.Environment.CurrentDirectory + "/plugin/125GateApp/db.json";
            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonfile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JArray o = (JArray)JToken.ReadFrom(reader);
                    reader.Close();
                    return o;
                }

            }

        }


        /// <summary>
        /// 写入串口信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="baud"></param>
        public static bool AddJson(string cardnum, string name, string stuid,string sex)
        {
            bool OK = true;
            try
            {
                JArray ja = null;
                bool b = false;
                string jsonfile = System.Environment.CurrentDirectory + "/plugin/125GateApp/db.json";
                string json = File.ReadAllText(jsonfile);
                if (!json.Equals(""))

                    ja = JArray.Parse(json);
                else
                    ja = new JArray();

                foreach (JObject j in ja)
                {

                    if (j["cardnum"].ToString().Equals(cardnum))
                    {
                        j["name"] = name;
                        j["stuid"] = stuid;
                        j["sex"] = sex;
                        b = true;
                        break;
                    }
                }
                if (!b)
                {
                    JObject stu = new JObject();
                    stu["cardnum"] = cardnum;
                    stu["name"] = name;
                    stu["stuid"] = stuid;
                    stu["sex"] = sex;    
                    ja.Add(stu);
                }
                File.WriteAllText(jsonfile, JsonConvert.SerializeObject(ja));
            }
            catch {
                OK = false;
            }
            return OK;
        }



    }
}
