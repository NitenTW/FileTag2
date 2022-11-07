Public Class RenameTag
    Private Sub RenameTag_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox1.Text = String.Empty
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Form1.CheckTagFormat(TextBox1.Text) Then
            Using sqliteConn As New SQLite.SQLiteConnection($"Data Source={Form1.DB_PATH};Version=3;")
                Dim tag As String = Form1.OldTagName

                sqliteConn.Open()
                Using cmd As New SQLite.SQLiteCommand(sqliteConn)
                    Try
                        cmd.CommandText = $"UPDATE Tags SET Name='{TextBox1.Text}' WHERE Name='{tag}'"
                        cmd.ExecuteNonQuery()
                    Catch sqliteEx As System.Data.SQLite.SQLiteException
                        If sqliteEx.ErrorCode = 19 Then
                            MsgBox("標籤已存在")
                        End If
                    End Try
                End Using
                sqliteConn.Close()
            End Using
        End If

        Me.Close()
    End Sub
End Class