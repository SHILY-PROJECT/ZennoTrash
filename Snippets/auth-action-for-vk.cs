#region [NEEDED PARAMETERS] =============================================
var login = "";
var pass = "";

var proxy = "";
var userAgent = project.Profile.UserAgent;

var deviceId = TextProcessing.RandomText(21, "dc");
#endregion ==============================================================


#region [AUTHORIZATION CHECK & COOKIE COLLECTION]========================
if (MakeHomePageRequest(out var _))
{
	project.SendToLog($"{login} | Account is authorized.", ZennoLab.InterfacesLibrary.Enums.Log.LogType.Info, true, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Green);
	return "Exit on the green line.";
}
#endregion =============================================================


#region [COLLECTING TOKENS]=============================================
var uuid = TextProcessing.RandomText(21, "dc");

var resp = ZennoPoster.HTTP.Request
(
    method: ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.GET,
    UserAgent: userAgent,
    proxy: proxy,
    url: $"https://id.vk.com/auth?app_id=7913379&response_type=silent_token&v=1.46.0&redirect_uri=https%3A%2F%2Fvk.com%2Ffeed&uuid={uuid}",
    Encoding: "utf-8",
    respType: ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
    Timeout: 30000,
    UseRedirect: true, MaxRedirectCount: 5,
    AdditionalHeaders: new[]{ "Referer: https://vk.com/", "Connection: keep-alive" },
    cookieContainer: project.Profile.CookieContainer
);

var authToken = Regex.Match(resp, "(?<=\"auth_token\":\").*?(?=\")").Value;
var anonymousToken = Regex.Match(resp, "(?<=\"anonymous_token\":\").*?(?=\")").Value;

if (string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(anonymousToken))
{
	project.SendToLog("Authorization tokens not found... | Resp: " + resp, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Warning, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Yellow);
	throw new Exception("Exit by red line...");
}

project.SendToLog("Auth token: " + authToken, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Info, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Blue);
project.SendToLog("Anonymous token: " + anonymousToken, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Info, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Blue);
#endregion =============================================================


#region [JUST REGISTERING THE EVENT]====================================
var rEvent = new Random().Next(500000, 7000000);
var time = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString().Replace(".", "").Substring(0, 13);

var content = $"auth_token={authToken}&anonymous_token={anonymousToken}&device_id={deviceId}&service_group=&external_device_id=&source_app_id=&flow_type=auth_without_password&events=%5B%7B%22id%22%3A{rEvent}%2C%22prev_event_id%22%3A0%2C%22prev_nav_id%22%3A0%2C%22screen%22%3A%22registration_phone%22%2C%22timestamp%22%3A%22{time}%22%2C%22type%22%3A%22type_action%22%2C%22type_action%22%3A%7B%22type%22%3A%22type_registration_item%22%2C%22type_registration_item%22%3A%7B%22event_type%22%3A%22auth_start%22%7D%7D%7D%5D&access_token=";

resp = ZennoPoster.HTTP.Request
(
    method: ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST,
    UserAgent: userAgent,
    proxy: proxy,
    url: "https://api.vk.com/method/statEvents.addAnonymously?v=5.123",
    content: content,
    contentPostingType: "application/x-www-form-urlencoded",
    Encoding: "utf-8",
    respType: ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.HeaderAndBody,
    Timeout: 30000,
    UseRedirect: true, MaxRedirectCount: 5,
    AdditionalHeaders: new[] { "Referer: https://id.vk.com/", "Origin: https://id.vk.com", "Connection: keep-alive" },
    cookieContainer: project.Profile.CookieContainer
);
#endregion =============================================================


#region [VALIDATE ACCOUNT]==============================================
content = $"login={login}&sid=&client_id=7913379&auth_token={authToken}&access_token=";

resp = ZennoPoster.HTTP.Request
(
    method: ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST,
    UserAgent: userAgent,
    proxy: proxy,
    url: "https://api.vk.com/method/auth.validateAccount?v=5.174&client_id=7913379",
    content: content,
    contentPostingType: "application/x-www-form-urlencoded",
    Encoding: "utf-8",
    respType: ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
    Timeout: 30000,
    UseRedirect: true, MaxRedirectCount: 5,
    AdditionalHeaders: new[] { "Referer: https://id.vk.com/", "Origin: https://id.vk.com", "Connection: keep-alive" },
    cookieContainer: project.Profile.CookieContainer
);

var sid = Regex.Match(resp, "(?<=\"sid\":\").*?(?=\")").Value;

if (string.IsNullOrWhiteSpace(sid))
{
	project.SendToLog("'{nameof(sid)}' - not found... | Resp: " + resp, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Warning, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Yellow);
	throw new Exception("Exit by red line...");
}
else project.SendToLog("Sid: " + sid, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Info, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Blue);
#endregion =============================================================


#region [AUTHORIZATION] ================================================
content = $"username={login}&password={ZennoLab.Macros.TextProcessing.UrlEncode(pass)}&auth_token={authToken}&sid=&uuid={uuid}&v=5.174&device_id={deviceId}&service_group=&version=1&app_id=7913379&access_token=";

resp = ZennoPoster.HTTP.Request
(
    method: ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST,
    UserAgent: userAgent,
    proxy: proxy,
    url: "https://login.vk.com/?act=connect_authorize",
    content: content,
    contentPostingType: "application/x-www-form-urlencoded",
    Encoding: "utf-8",
    respType: ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
    Timeout: 30000,
    UseRedirect: true, MaxRedirectCount: 5,
    AdditionalHeaders: new[] { "Referer: https://id.vk.com/", "Origin: https://id.vk.com", "Connection: keep-alive" },
    cookieContainer: project.Profile.CookieContainer
);

var accessToken = Regex.Match(resp, "(?<=\"access_token\":\").*?(?=\")").Value;
	
if (string.IsNullOrWhiteSpace(accessToken))
{
	project.SendToLog("'{nameof(accessToken)}' - not found... | Resp: " + resp, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Warning, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Yellow);
	throw new Exception("Exit by red line...");
}
else project.SendToLog("Access token: " + accessToken, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Info, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Blue);
#endregion =============================================================


#region [AUTHORIZATION CHECK & COOKIE COLLECTION]=======================
if (!MakeHomePageRequest(out var homePage))
{
	project.SendToLog("UserID not found... | Resp: " + homePage, ZennoLab.InterfacesLibrary.Enums.Log.LogType.Warning, false, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Yellow);
	throw new Exception("Exit by red line...");	
}
else
{
	project.SendToLog($"{login} | Account successfully authorized.", ZennoLab.InterfacesLibrary.Enums.Log.LogType.Info, true, ZennoLab.InterfacesLibrary.Enums.Log.LogColor.Green);
	return "Exit on the green line.";
}
#endregion =============================================================


#region [LOCAL METHOD FOR CHECKING AUTHORIZATION]=======================
bool MakeHomePageRequest(out string respHomePage)
{
	respHomePage = ZennoPoster.HTTP.Request
	(
	    method: ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.GET,
	    UserAgent: userAgent,
	    proxy: proxy,
	    url: "https://vk.com/",
	    Encoding: "utf-8",
	    respType: ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.HeaderAndBody,
	    Timeout: 60000,
	    UseRedirect: true, MaxRedirectCount: 10,
	    AdditionalHeaders: null,
	    cookieContainer: project.Profile.CookieContainer
	);
	
	return Regex.IsMatch(respHomePage, "(?<=\"user_id\":).*?(?=,)");
};
#endregion =============================================================