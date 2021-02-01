namespace Ridge.Interceptor.ActionInfo.Dtos
{
    public class ActionInfoDto
    {
        public ActionInfoDto(string url, string httpMethod, ActionArgumentsInfo actionArgumentsInfo)
        {
            Url = url;
            HttpMethod = httpMethod;
            ActionArgumentsInfo = actionArgumentsInfo;
        }

        public string Url { get; }
        public string HttpMethod { get; }
        public ActionArgumentsInfo ActionArgumentsInfo { get; }
    }
}
