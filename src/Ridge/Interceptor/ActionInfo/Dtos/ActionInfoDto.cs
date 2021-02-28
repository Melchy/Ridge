namespace Ridge.Interceptor.ActionInfo.Dtos
{
    internal class ActionInfoDto
    {
        public ActionInfoDto(string url, ActionArgumentsInfo actionArgumentsInfo)
        {
            Url = url;
            ActionArgumentsInfo = actionArgumentsInfo;
        }

        public string Url { get; }
        public ActionArgumentsInfo ActionArgumentsInfo { get; }
    }
}
