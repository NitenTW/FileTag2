Public Class AddFile
    Private Sub AddFile_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Form1.UpdateListBox(ListBox1)
        TextBox1.Text = String.Empty
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using selectFile As New OpenFileDialog
            If selectFile.ShowDialog = DialogResult.OK Then
                TextBox1.Text = selectFile.FileName
            End If
        End Using
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Using sqliteConn As New SQLite.SQLiteConnection($"Data Source={Form1.DB_PATH};Version=3;")
            sqliteConn.Open()
            Using cmd As New SQLite.SQLiteCommand(sqliteConn)
                For Each tag As String In ListBox1.SelectedItems
                    '取得標籤的OID
                    cmd.CommandText = $"SELECT OID FROM Tags WHERE Name='{tag}'"
                    Dim tagIndex As Integer = cmd.ExecuteScalar()

                    '無使用到重覆的標籤才加入檔案
                    cmd.CommandText = $"SELECT COUNT() FROM List WHERE Path='{TextBox1.Text}' AND TagID={tagIndex}"
                    If cmd.ExecuteScalar() = 0 Then
                        '加入檔案
                        cmd.CommandText = $"INSERT INTO List (Path, TagID) VALUES ('{TextBox1.Text}', {tagIndex})"
                        cmd.ExecuteNonQuery()
                    End If
                Next
            End Using
            sqliteConn.Close()
        End Using

        Me.Close()
    End Sub
End Class