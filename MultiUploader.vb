Imports System.Threading
Imports System.Net
Imports System.IO

Public Class MultiUploader

    Public Login As WikiLogin
    Public Username, Password, Wiki As String
    Dim FileNames As String()
    Public Boundary As String = "------------------------------" + vbCrLf

    Private Sub LoginToWiki()
        Me.Login = New WikiLogin(Me.Username, Me.Password, Me.Wiki)
        Dim IsAutoConfirmed As Boolean = Me.Login.IsUserAutoConfirmed
        If Not IsAutoConfirmed Then
            Invoke(Sub()
                       LoginButton.Text = "Error: account is not autoconfirmed"
                       LoginButton.Enabled = True
                   End Sub)
            Return
        End If
        Dim IsLoggedIn As String = Me.Login.Login()
        If IsLoggedIn = "Success" Then
            Invoke(Sub()
                       LoginPanel.Visible = False
                   End Sub)
            ChangeTitle(Me.Wiki)
            ExpandWindow()
            If My.Settings.AutoLogIn Then
                My.Settings.Username = Me.Username
                My.Settings.Password = Me.Password
                My.Settings.Site = Me.Wiki
                My.Settings.Save()
            End If
        Else
            Invoke(Sub()
                       LoginButton.Text = "Login failure. Try again"
                       LoginButton.Enabled = True
                   End Sub)
        End If
    End Sub

    Private Sub UploadFiles()
        Dim Description As String
        Description = ""
        Invoke(Sub()
                   If DescriptionBox.Text <> Nothing Then
                       Description = DescriptionBox.Text
                   End If
                   UploadButton.Enabled = False
               End Sub)
        Dim Cookies As CookieContainer = Me.Login.GetCookies()
        Dim Upload As New FileUpload(Cookies, Me.Wiki, Me)
        If Me.FileNames IsNot Nothing Then
            For Each FileName In Me.FileNames
                AddLogMessage(Boundary)
                AddLogMessage("Uploading file: " + Path.GetFileName(FileName) + "...")
                Upload.UploadFiles(FileName, Description)
            Next
        Else : AddLogMessage("No files selected." + vbCrLf)
        End If
        Invoke(Sub()
                   UploadButton.Enabled = True
               End Sub)
    End Sub

    Public Sub AddLogMessage(Message As String)
        Invoke(Sub()
                   UploadLogger.Text = UploadLogger.Text + Message
               End Sub)
    End Sub

    Public Sub ChangeTitle(NewTitle As String)
        Invoke(Sub()
                   Me.Text = "MultiUploader - " + NewTitle
               End Sub)
    End Sub

    Private Sub ExpandWindow()
        For i As Integer = 1 To 606 Step 1
            Invoke(Sub()
                       Me.Width = Me.Width + 1
                       If i Mod 2 = 0 Then Me.Height = Me.Height + 1
                   End Sub)
        Next
        Invoke(Sub()
                   FileUploadPanel.Visible = True
               End Sub)
    End Sub

    Private Sub AddFileToList(File As String)
        FileList.Text = FileList.Text + File
    End Sub

    Public Sub AdjustFileList()
        Invoke(Sub()
                   Dim FileToRemove As String
                   Dim Files As String()
                   Files = FileList.Lines()
                   FileToRemove = Files(0)
                   Dim FilesList As List(Of String) = Files.Select(Function(File) File).ToList()
                   FilesList.Remove(FileToRemove)
                   FileList.Clear()
                   For Each File As String In FilesList
                       AddFileToList(File)
                   Next
               End Sub)
    End Sub

    Private Sub MultiUploader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoginPanel.Visible = True
        LoginButton.Enabled = True
        FileUploadPanel.Visible = False
        If My.Settings.AutoLogIn Then
            Me.Username = My.Settings.Username
            Me.Password = My.Settings.Password
            Me.Wiki = My.Settings.Site
            UsernameBox.Text = Me.Username
            PasswordBox.Text = Me.Password
            SiteBox.Text = Me.Wiki
            LoginButton.Text = "Logging in..."
            LoginButton.Enabled = False
            Dim LoginThread As New Thread(AddressOf LoginToWiki)
            LoginThread.Start()
        End If
    End Sub

    Private Sub LoginButton_Click(sender As Object, e As EventArgs) Handles LoginButton.Click
        Me.Username = UsernameBox.Text
        Me.Password = PasswordBox.Text
        Me.Wiki = SiteBox.Text
        If LoginCheckBox.Checked Then
            My.Settings.AutoLogIn = True
            My.Settings.Username = Me.Username
            My.Settings.Password = Me.Password
            My.Settings.Site = Me.Wiki
            My.Settings.Save()
        End If
        LoginButton.Text = "Logging in..."
        LoginButton.Enabled = False
        Dim LoginThread As New Thread(AddressOf LoginToWiki)
        LoginThread.Start()
    End Sub


    Private Sub ChooseFileButton_Click(sender As Object, e As EventArgs) Handles ChooseFileButton.Click
        ChooseFileDialog.Title = "Please choose the files to upload"
        ChooseFileDialog.Filter = "JPG|*jpg|PNG|*png|GIF|*gif"
        ChooseFileDialog.Multiselect = True
        ChooseFileDialog.ShowDialog()
    End Sub

    Private Sub ChooseFileDialog_FileOk(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ChooseFileDialog.FileOk
        Me.FileNames = ChooseFileDialog.FileNames
        If FileNames IsNot Nothing Then
            Dim NumberOfFiles As Integer = 0
            For Each FileName In Me.FileNames
                NumberOfFiles += 1
                AddFileToList(Path.GetFileName(FileName))
                If NumberOfFiles <> FileNames.Count() Then
                    AddFileToList(vbCrLf)
                End If
            Next
        End If
    End Sub

    Private Sub UploadButton_Click(sender As Object, e As EventArgs) Handles UploadButton.Click
        Dim UploadThread As New Thread(AddressOf UploadFiles)
        UploadThread.Start()
    End Sub

    Private Sub ClearAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClearAllToolStripMenuItem.Click
        UploadLogger.Text = ""
        DescriptionBox.Text = ""
        Me.FileNames = Nothing
        AddLogMessage("Cleared all" + vbCrLf)
    End Sub

    Private Sub ClearLogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClearLogToolStripMenuItem.Click
        UploadLogger.Text = ""
        AddLogMessage("Cleared log" + vbCrLf)
    End Sub

    Private Sub ClearSettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClearSettingsToolStripMenuItem.Click
        My.Settings.AutoLogIn = False
        My.Settings.Username = ""
        My.Settings.Password = ""
        My.Settings.Site = ""
        My.Settings.Save()
    End Sub

    Private Sub OpenSettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenSettingsToolStripMenuItem.Click
        Dim SettingsWindow As New OptionsWindow(Me)
        SettingsWindow.Show()
    End Sub

    Private Sub UploadLogger_LinkClicked(sender As Object, e As LinkClickedEventArgs) Handles UploadLogger.LinkClicked
        System.Diagnostics.Process.Start(e.LinkText)
    End Sub
End Class
