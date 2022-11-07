Public Class NewTag

    Private Sub NewTag_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox1.Text = String.Empty
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Form1.CheckTagFormat(TextBox1.Text) Then
            '新增標籤
            Using sqliteConn As New SQLite.SQLiteConnection($"Data Source={Form1.DB_PATH};Version=3;")
                sqliteConn.Open()
                Using cmd As New SQLite.SQLiteCommand(sqliteConn)
                    Try
                        cmd.CommandText = $"INSERT INTO Tags (Name) VALUES ('{TextBox1.Text}')"
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