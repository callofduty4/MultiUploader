'Copyright M.Kohli 2013

Imports System.Threading
Imports System.Net
Imports System.Web
Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class FileUpload
    Dim Site, EditToken As String
    Dim Cookies As CookieContainer
    Dim LogForm As MultiUploader

    Sub New(ByVal Cookies As CookieContainer, ByVal Site As String, ByVal LLogForm As MultiUploader)
        Me.Site = Site
        Me.Cookies = Cookies
        Me.LogForm = LLogForm
    End Sub

    Private Sub GetEditToken()
        Dim URL As String = Me.Site + "/api.php?action=query&prop=info&intoken=edit&titles=Foo&format=json&indexpageids=1"
        Dim GetTokenFromAPI As HttpWebRequest = WebRequest.Create(URL)
        GetTokenFromAPI.UserAgent = "MultiUploader 0.1"
        GetTokenFromAPI.Method = "GET"
        GetTokenFromAPI.CookieContainer = Me.Cookies
        Try
            Dim APIResponse As HttpWebResponse = GetTokenFromAPI.GetResponse()
            Dim Response As Stream = APIResponse.GetResponseStream()
            Dim ResponseReader As StreamReader = New StreamReader(Response, Encoding.UTF8)
            Dim JSONResponseString As String = ResponseReader.ReadToEnd()
            Dim DecodedResponse As JObject = JObject.Parse(JSONResponseString)
            Dim PageID As String = DecodedResponse("query")("pageids")(0).ToString()
            Me.EditToken = DecodedResponse("query")("pages")(PageID)("edittoken").ToString()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    'NOTE TO SELF: DO NOT EVER USE VB.NET FOR SOMETHING LIKE THIS AGAIN
    Private Sub Upload(ByVal FilePath As String, ByVal FileDescription As String) 'Manually build the request
        Dim URL As String = Me.Site + "/api.php" 'Point data to /api.php
        Dim Boundary As String = "---------------------------" + DateTime.Now.Ticks.ToString("x") 'Generate boundary
        Dim APIParams As New Dictionary(Of String, String) 'Dictionary of API parameters
        Dim FileTitle As String = Path.GetFileName(FilePath) 'Get the file name
        APIParams.Add("action", "upload") 'Add API parameters
        APIParams.Add("filename", FileTitle)
        APIParams.Add("comment", FileDescription)
        APIParams.Add("token", Me.EditToken)
        APIParams.Add("ignorewarnings", "true")
        APIParams.Add("format", "json")
        Dim UploadImageRequest As HttpWebRequest = WebRequest.Create(URL) 'Setup request
        UploadImageRequest.Method = "POST"
        UploadImageRequest.UserAgent = "MultiUploader 0.1"
        UploadImageRequest.CookieContainer = Me.Cookies 'Set cookies to those received at login
        UploadImageRequest.ContentType = "multipart/form-data; boundary=" + Boundary 'I hate this.
        Dim UploadImageRequestStream As Stream = UploadImageRequest.GetRequestStream() 'Begin writing to request stream
        For Each Entry As KeyValuePair(Of String, String) In APIParams 'Generate binary stream for API parameters
            Dim Data As String = String.Format("--{0}" + vbCrLf + "Content-Disposition: form-data; name=""{1}""" + vbCrLf + vbCrLf + "{2}" + vbCrLf, Boundary, Entry.Key, Entry.Value)
            Dim DataBytes As Byte() = Encoding.UTF8.GetBytes(Data) 'Get binary stream for API parameter
            UploadImageRequestStream.Write(DataBytes, 0, DataBytes.Length) 'Write API parameter to the request stream
        Next
        Dim FileType As String
        Select Case Path.GetExtension(FilePath).ToUpper() 'Set correct MIME type
            Case ".JPG"
                FileType = "image/jpg"
            Case ".PNG"
                FileType = "image/png"
            Case ".GIF"
                FileType = "image/gif"
        End Select
        Dim FileStr As String = String.Format("--{0}" + vbCrLf + "Content-Disposition: form-data; name=""{1}""; filename=""{2}""" + vbCrLf + "Content-Type: {3}" + vbCrLf + vbCrLf, Boundary, "file", FileTitle, FileType) 'Header for imagedata
        Dim FileStrBytes As Byte() = Encoding.UTF8.GetBytes(FileStr) 'Get binary stream for image data header
        UploadImageRequestStream.Write(FileStrBytes, 0, FileStrBytes.Length) 'Write imagedata header to the request stream
        Dim Image As System.Drawing.Image = System.Drawing.Image.FromFile(FilePath) 'Create image object for the image
        Dim ImageStream As New MemoryStream() 'Create binary stream for the image
        Select Case Path.GetExtension(FilePath).ToUpper 'Generate correct image data
            Case ".JPG"
                Image.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Jpeg)
            Case ".PNG"
                Image.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Png)
            Case ".GIF"
                Image.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Gif)
        End Select
        FileStrBytes = ImageStream.ToArray() 'Get binary stream for image
        UploadImageRequestStream.Write(FileStrBytes, 0, FileStrBytes.Length) 'Write binary stream for image to request stream
        Dim Ending As String = vbCrLf + vbCrLf + "--" + Boundary + "--" 'Create end boundary
        Dim EndingBytes As Byte() = Encoding.UTF8.GetBytes(Ending) 'Get binary stream for end boundary
        UploadImageRequestStream.Write(EndingBytes, 0, EndingBytes.Length) 'Write binary stream for end boundary to request stream
        UploadImageRequestStream.Close() 'Finished creating request stream
        Dim UploadImageRequestResponse As WebResponse = Nothing 'Setup response
        Try
            UploadImageRequestResponse = UploadImageRequest.GetResponse() 'Get response
            LogForm.AddLogMessage("OK" + vbCrLf) 'Add confirming log message
            LogForm.AddLogMessage(Me.Site + "/w/File:" + FileTitle + vbCrLf)
            LogForm.AdjustFileList()
            Dim UploadImageRequestStreamReader As StreamReader = New StreamReader(UploadImageRequestResponse.GetResponseStream())
            Dim UploadImageRequestResponseString As String = UploadImageRequestStreamReader.ReadToEnd
            UploadImageRequestStreamReader.Close()
            UploadImageRequestResponse.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Sub UploadFiles(ByVal FileName As String, ByVal FileDescription As String)
        GetEditToken()
        Upload(FileName, FileDescription)
    End Sub

End Class
