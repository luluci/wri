﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static wri.Interface.Json;

namespace wri.Interface
{
    static internal class Json
    {

        public class ConsoleExit
        {
            // メッセージタイプ
            [JsonPropertyName("type")]
            public string Type { get; set; } = "console";

            // 状態
            [JsonPropertyName("status")]
            public string Status { get; set; } = "exit";

            // 終了コード
            [JsonPropertyName("code")]
            public int Code { get; set; } = 0;
        }

        public class ConsoleStdout
        {
            // メッセージタイプ
            [JsonPropertyName("type")]
            public string Type { get; set; } = "console";

            // 状態
            [JsonPropertyName("status")]
            public string Status { get; set; } = "running";

            // 終了コード
            [JsonPropertyName("stdout")]
            public string Stdout { get; set; } = string.Empty;
        }

        static ConsoleExit TemplateConsoleExit = new ConsoleExit();
        static ConsoleStdout TemplateConsoleStdout = new ConsoleStdout();

        static public string MakeJsonStringConsoleExit(int code)
        {
            TemplateConsoleExit.Code = code;
            return JsonSerializer.Serialize(TemplateConsoleExit);
        }

        static public string MakeJsonStringConsoleStdout(string stdout)
        {
            TemplateConsoleStdout.Stdout = stdout;
            return JsonSerializer.Serialize(TemplateConsoleStdout);
        }
    }
}
