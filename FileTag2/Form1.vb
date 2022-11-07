Public Class Form1
    Friend Const DB_PATH = "FileTag.db"

    'Pooling=ture時，SQL連線將從連線池獲得。預設為 True
    'FailIfMissing=true 時，資料庫不存在拋出例外，而不是建立資料庫。預設為 False
    Private sqliteConn As New SQLite.SQLiteConnection($"Data Source={DB_PATH};Version=3;")

    Friend ReadOnly Property OldTagName As String
        Get
            Return ListBox1.SelectedItem
        End Get
    End Property

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'conn.SetPassword("password")    'Data Source=test3.db;Version=3;Password=password;
        'conn.SetPassword(String.Empty)  '清除密碼
        '需有 SQLite.SEE.License 才能為資料庫加上密碼。2000美金…囧

        If My.Computer.FileSystem.FileExists(DB_PATH) AndAlso Not My.Computer.FileSystem.GetFileInfo(DB_PATH).Length = 0 Then
            UpdateListBox(ListBox1)
        Else
            DbInitilize()
        End If
    End Sub

    ''' <summary>
    ''' 更新 ListBox 的標籤
    ''' </summary>
    Friend Sub UpdateListBox(ByRef list As ListBox)
        list.Items.Clear()

        '顕示標籤清單到 ListBox
        sqliteConn.Open()
        Using cmd As New SQLite.SQLiteCommand(sqliteConn)
            cmd.CommandText = "SELECT Name FROM Tags"
            Using tagList As New SQLite.SQLiteDataAdapter(cmd)
                Dim ds As New DataTable
                tagList.Fill(ds)
                'ListBox1.DataSource = ds   '不能用在ListBox的様子
                For Each i As System.Data.DataRow In ds.Rows
                    list.Items.Add(i.Item(0))
                Next
            End Using
            sqliteConn.Close()
        End Using
    End Sub

    ''' <summary>
    ''' 資料庫初始化
    ''' </summary>
    Private Sub DbInitilize()
        '加入表單
        sqliteConn.Open()
        Using cmd As New SQLite.SQLiteCommand(sqliteConn)
            cmd.CommandText = "CREATE TABLE List (OID INTEGER PRIMARY KEY AUTOINCREMENT, Path TEXT NOT NULL, TagID INTEGER NOT NULL)"
            cmd.ExecuteNonQuery()
            cmd.CommandText = "CREATE TABLE Tags (OID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT Not NULL, UNIQUE(Name))"
            cmd.ExecuteNonQuery()

            '建立刪除觸發器。刪除Tag時，也一併刪除List裡的Tag
            cmd.CommandText = "CREATE TRIGGER aferDelTag AFTER DELETE ON Tags BEGIN DELETE FROM List WHERE List.TagID = old.OID; END;"
            cmd.ExecuteNonQuery()

            '建立視圖
            cmd.CommandText = "CREATE VIEW viewAll AS SELECT List.OID, List.Path, Tags.Name FROM List INNER JOIN Tags ON List.TagID = Tags.OID"
            cmd.ExecuteNonQuery()
        End Using
        sqliteConn.Close()
    End Sub

    ''' <summary>
    ''' 更新 DataGridView 的檔案清單
    ''' </summary>
    Private Sub UpdateDataGridView(ByVal searchWord As String)
        sqliteConn.Open()
        Using cmd As New SQLite.SQLiteCommand(sqliteConn)
            '顕示檔案清單到 DataGridView
            cmd.CommandText = $"SELECT * FROM viewAll {searchWord}"
            Using da As New SQLite.SQLiteDataAdapter(cmd)
                Dim ds As New DataTable
                da.Fill(ds)
                DataGridView1.DataSource = ds
            End Using
        End Using
        sqliteConn.Close()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub NewTagToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles NewTagToolStripMenuItem1.Click
        NewTag.ShowDialog() '新增標籤
        UpdateListBox(ListBox1)
    End Sub

    Private Sub RenameTagToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RenameTagToolStripMenuItem.Click
        If Not CheckSelectedItemIsOnly() Then Exit Sub

        RenameTag.ShowDialog()
        UpdateListBox(ListBox1)
    End Sub

    ''' <summary>
    ''' 刪除標籤
    ''' </summary>
    Private Sub DeleteTagToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteTagToolStripMenuItem.Click
        sqliteConn.Open()
        Using cmd As New SQLite.SQLiteCommand(sqliteConn)
            For Each tag As String In ListBox1.SelectedItems
                cmd.CommandText = $"DELETE FROM Tags WHERE Name='{tag}'"
                cmd.ExecuteNonQuery()
            Next
            'Dim tag As String = ListBox1.SelectedItem
        End Using
        sqliteConn.Close()

        UpdateListBox(ListBox1)
    End Sub

    ''' <summary>
    ''' 確認是否只選擇一個標籤
    ''' </summary>
    ''' <returns></returns>
    Private Function CheckSelectedItemIsOnly() As Boolean
        If ListBox1.SelectedIndices.Count = 0 Then
            MsgBox("必需選擇一個標籤")
            Return False
        End If

        If Not ListBox1.SelectedIndices.Count = 1 Then
            MsgBox("只能選擇一個標籤")
            Return False
        End If

        Return True
    End Function

    Private Sub AddFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AddFileToolStripMenuItem.Click
        AddFile.ShowDialog()
    End Sub

    Private Sub DeleteFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteFileToolStripMenuItem.Click
        If DataGridView1.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        sqliteConn.Open()
        Using cmd As New SQLite.SQLiteCommand(sqliteConn)
            For i As Integer = 0 To DataGridView1.SelectedRows.Count - 1
                Dim selectedIndex As Integer = DataGridView1.SelectedRows(i).Index
                If selectedIndex = DataGridView1.NewRowIndex Then Continue For

                Dim path As String = DataGridView1.Item(1, selectedIndex).Value
                Dim tagName As String = DataGridView1.Item(2, selectedIndex).Value

                cmd.CommandText = $"SELECT OID FROM Tags WHERE Name='{tagName}'"    '取得TagID
                Dim tagID As Integer = cmd.ExecuteScalar()

                cmd.CommandText = $"DELETE FROM List WHERE Path='{path}' AND TagID='{tagID}'"
                cmd.ExecuteNonQuery()
            Next
        End Using
        sqliteConn.Close()

        'Todo: 刪除和重新命名後刷新 DataGridView 或是清除？
    End Sub

    Friend Function CheckTagFormat(ByVal text As String) As Boolean
        If text = String.Empty Then
            MsgBox("尚未輸入標籤名稱")
            Return False
        End If

        If Not text.IndexOf(" ") = -1 Then
            MsgBox("標籤不得有空白")
            Return False
        End If

        If Not text.IndexOf(",") = -1 Then
            MsgBox("標籤不得有逗號")
            Return False
        End If

        Return True
    End Function

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If ListBox1.SelectedItems.Count = 0 Then Exit Sub

        Dim searchWord As String = " WHERE "

        For Each tagName In ListBox1.SelectedItems
            searchWord &= "Name='" & tagName & "' OR "
        Next
        searchWord = Strings.Left(searchWord, searchWord.Length - 4)

        UpdateDataGridView(searchWord)
    End Sub

    Private Sub FileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FileToolStripMenuItem.Click
        sqliteConn.Open()
        Using cmd As New SQLite.SQLiteCommand(sqliteConn)
            '顕示檔案清單到 DataGridView
            'cmd.CommandText = "SELECT List.OID, List.Path, Tags.Name FROM List INNER JOIN Tags ON List.TagID = Tags.OID WHERE List.TagID = 0 OR List.TagID = 2"
            cmd.CommandText = $"SELECT * FROM viewAll"   'WHERE Name='Test2' OR Name='Test3'
            Using da As New SQLite.SQLiteDataAdapter(cmd)
                Dim ds As New DataTable
                da.Fill(ds)
                DataGridView1.DataSource = ds
            End Using
        End Using
        sqliteConn.Close()
    End Sub
End Class
