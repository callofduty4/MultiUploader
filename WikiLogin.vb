Imports System.Threading
Imports System.Net
Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class WikiLogin

    Dim Username, Password, Site, LoginToken As String
    Dim Cookies As CookieContainer

    Sub New(ByVal Username As String, ByVal Password As String, ByVal Site As String)
        Me.Username = Username
        Me.Password = Password
        Me.Site = Site
    End Sub

    Public Function Login() As String
        Dim LoginViaAPI As HttpWebRequest
        Dim URL As String = Me.Site + "/api.php?action=login&lgname=" + Me.Username + "&lgpassword=" + Me.Password + "&format=json"
        Try
            LoginViaAPI = WebRequest.Create(URL)
            LoginViaAPI.Method = "POST"
            LoginViaAPI.UserAgent = "MultiUploader 0.1"
            Me.Cookies = New CookieContainer()
            LoginViaAPI.CookieContainer = Me.Cookies
            Dim APIResponse As HttpWebResponse = LoginViaAPI.GetResponse()
            Dim Response As Stream = APIResponse.GetResponseStream()
            Dim ResponseReader As New StreamReader(Response, Encoding.UTF8)
            Dim JSONResponseString As String = ResponseReader.ReadToEnd
            Dim DecodedResponse As JObject = JObject.Parse(JSONResponseString)
            Me.LoginToken = DecodedResponse("login")("token").ToString()
        Catch ex As Exception
            Return "Failure"
        End Try
        URL = Me.Site + "/api.php?action=login&lgname=" + Me.Username + "&lgpassword=" + Me.Password + "&lgtoken=" + Me.LoginToken + "&format=json"
        LoginViaAPI = WebRequest.Create(URL)
        LoginViaAPI.Method = "POST"
        LoginViaAPI.UserAgent = "MultiUploader 0.1"
        LoginViaAPI.CookieContainer = Me.Cookies
        Try
            Dim APIResponse As HttpWebResponse = LoginViaAPI.GetResponse()
            Dim Response As Stream = APIResponse.GetResponseStream()
            Dim ResponseReader As New StreamReader(Response, Encoding.UTF8)
            Dim JSONResponseString As String = ResponseReader.ReadToEnd
            Dim DecodedResponse As JObject = JObject.Parse(JSONResponseString)
            Dim ResponseText As String = DecodedResponse("login")("result").ToString()
            Return ResponseText
        Catch ex As Exception
            Return "Failure"
        End Try
    End Function

    Public Function GetCookies() As CookieContainer
        Return Me.Cookies
    End Function
End Class
