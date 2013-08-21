'Copyright M.Kohli 2013

Imports System.Threading
Imports System.Net

Public Class OptionsWindow

    Dim LogForm As MultiUploader

    Sub New(ByVal LLogform As MultiUploader)
        InitializeComponent()
        Me.LogForm = LLogform
    End Sub

    Private Sub ReLogin(ByVal NewWiki As String)
        Invoke(Sub() SaveButton.Enabled = False)
        LogForm.Login = New WikiLogin(LogForm.Username, LogForm.Password, NewWiki)
        Dim IsLoggedIn As String = LogForm.Login.Login
        If IsLoggedIn = "Success" Then
            LogForm.ChangeTitle(NewWiki)
            LogForm.AddLogMessage(LogForm.Boundary)
            LogForm.AddLogMessage("Changed site to: " + NewWiki + vbCrLf)
            If My.Settings.AutoLogIn Then
                My.Settings.Site = NewWiki
                My.Settings.Save()
            End If
            Invoke(Sub()
                       Me.Close()
                   End Sub)
        Else
            Invoke(Sub()
                       SaveButton.Text = "Error logging in, please try again"
                       SaveButton.Enabled = True
                   End Sub)
        End If
    End Sub

    Private Sub OptionsWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UsernameBox.Text = My.Settings.Username
        PasswordBox.Text = My.Settings.Password
        SiteBox.Text = My.Settings.Site
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As EventArgs) Handles SaveButton.Click
        If (UsernameBox.Text <> Nothing) And (PasswordBox.Text <> Nothing) And (SiteBox.Text <> Nothing) And (ChangeSiteBox.Text = Nothing) Then
            My.Settings.Username = UsernameBox.Text
            My.Settings.Password = PasswordBox.Text
            My.Settings.Site = SiteBox.Text
            My.Settings.AutoLogIn = True
            My.Settings.Save()
            LogForm.AddLogMessage(LogForm.Boundary)
            LogForm.AddLogMessage("Updated autologin settings" + vbCrLf)
            Me.Close()
        End If
        If ChangeSiteBox.Text <> Nothing Then
            Dim NewWiki As String = ChangeSiteBox.Text
            LogForm.Wiki = NewWiki
            Dim LoginThread As New Thread(Sub()
                                              ReLogin(NewWiki)
                                          End Sub)
            LoginThread.Start()
        End If
    End Sub
End Class