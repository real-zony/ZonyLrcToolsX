using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZonyLrcTools.Common.Infrastructure.Exceptions
{
    /// <summary>
    /// 错误码相关的帮助类。
    /// </summary>
    public static class ErrorCodeHelper
    {
        public static Dictionary<int, string> ErrorMessages { get; }

        static ErrorCodeHelper()
        {
            ErrorMessages = new Dictionary<int, string>();
        }

        /// <summary>
        /// 从 err_msg.json 文件加载错误信息。
        /// </summary>
        public static void LoadErrorMessage()
        {
            // 防止重复加载。
            if (ErrorMessages.Count != 0)
            {
                return;
            }

            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "error_msg.json");
            using var jsonReader = new JsonTextReader(File.OpenText(jsonPath));
            var jsonObj = JObject.Load(jsonReader);

            var errors = jsonObj.SelectTokens("$.Error.*");
            var warnings = jsonObj.SelectTokens("$.Warning.*");
            errors.Union(warnings).Select(m => m.Parent).OfType<JProperty>().ToList()
                .ForEach(m => ErrorMessages.Add(int.Parse(m.Name), m.Value.Value<string>() ?? string.Empty));
        }

        public static string GetMessage(int errorCode) => ErrorMessages[errorCode];
    }
}