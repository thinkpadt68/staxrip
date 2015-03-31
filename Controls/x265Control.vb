Imports StaxRip.UI
Imports StaxRip.x265

Class x265Control
    Inherits UserControl

#Region " Designer "
    <DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If

        MyBase.Dispose(disposing)
    End Sub

    Friend WithEvents lv As StaxRip.UI.ListViewEx
    Friend WithEvents llConfigCodec As System.Windows.Forms.LinkLabel
    Friend WithEvents llConfigContainer As System.Windows.Forms.LinkLabel
    Friend WithEvents llCompCheck As System.Windows.Forms.LinkLabel

    Private components As System.ComponentModel.IContainer

    <DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.llConfigCodec = New System.Windows.Forms.LinkLabel()
        Me.llConfigContainer = New System.Windows.Forms.LinkLabel()
        Me.llCompCheck = New System.Windows.Forms.LinkLabel()
        Me.lv = New StaxRip.UI.ListViewEx()
        Me.SuspendLayout()
        '
        'llConfigCodec
        '
        Me.llConfigCodec.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.llConfigCodec.AutoSize = True
        Me.llConfigCodec.BackColor = System.Drawing.SystemColors.Window
        Me.llConfigCodec.LinkColor = System.Drawing.Color.DimGray
        Me.llConfigCodec.Location = New System.Drawing.Point(3, 185)
        Me.llConfigCodec.Margin = New System.Windows.Forms.Padding(3)
        Me.llConfigCodec.Name = "llConfigCodec"
        Me.llConfigCodec.Size = New System.Drawing.Size(120, 25)
        Me.llConfigCodec.TabIndex = 1
        Me.llConfigCodec.TabStop = True
        Me.llConfigCodec.Text = "Options"
        '
        'llConfigContainer
        '
        Me.llConfigContainer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.llConfigContainer.AutoSize = True
        Me.llConfigContainer.BackColor = System.Drawing.SystemColors.Window
        Me.llConfigContainer.LinkColor = System.Drawing.Color.DimGray
        Me.llConfigContainer.Location = New System.Drawing.Point(218, 185)
        Me.llConfigContainer.Margin = New System.Windows.Forms.Padding(3)
        Me.llConfigContainer.Name = "llConfigContainer"
        Me.llConfigContainer.Size = New System.Drawing.Size(146, 25)
        Me.llConfigContainer.TabIndex = 2
        Me.llConfigContainer.TabStop = True
        Me.llConfigContainer.Text = "Container Options"
        '
        'llCompCheck
        '
        Me.llCompCheck.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.llCompCheck.AutoSize = True
        Me.llCompCheck.BackColor = System.Drawing.SystemColors.Window
        Me.llCompCheck.LinkColor = System.Drawing.Color.DimGray
        Me.llCompCheck.Location = New System.Drawing.Point(3, 154)
        Me.llCompCheck.Margin = New System.Windows.Forms.Padding(3)
        Me.llCompCheck.Name = "llCompCheck"
        Me.llCompCheck.Size = New System.Drawing.Size(222, 25)
        Me.llCompCheck.TabIndex = 3
        Me.llCompCheck.TabStop = True
        Me.llCompCheck.Text = "Run Compressibility Check"
        '
        'lv
        '
        Me.lv.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lv.Location = New System.Drawing.Point(0, 0)
        Me.lv.Name = "lv"
        Me.lv.Size = New System.Drawing.Size(367, 213)
        Me.lv.TabIndex = 0
        Me.lv.UseCompatibleStateImageBehavior = False
        '
        'x265Control
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.llConfigContainer)
        Me.Controls.Add(Me.llConfigCodec)
        Me.Controls.Add(Me.llCompCheck)
        Me.Controls.Add(Me.lv)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "x265Control"
        Me.Size = New System.Drawing.Size(367, 213)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private Encoder As x265Encoder
    Private Params As x265Params

    Private cms As ContextMenuStrip
    Private QualityDefinitions As List(Of QualityItem)

    Sub New(enc As x265Encoder)
        MyBase.New()
        InitializeComponent()

        components = New System.ComponentModel.Container()

        QualityDefinitions = New List(Of QualityItem) From {
            New QualityItem(18, "Super High", "Super high quality and file size (-crf 18)"),
            New QualityItem(19, "Very High", "Very high quality and file size (-crf 19)"),
            New QualityItem(20, "Higher", "Higher quality and file size (-crf 20)"),
            New QualityItem(21, "High", "High quality and file size (-crf 21)"),
            New QualityItem(22, "Medium", "Medium quality and file size (-crf 22)"),
            New QualityItem(23, "Low", "Low quality and file size (-crf 23)"),
            New QualityItem(24, "Lower", "Lower quality and file size (-crf 24)"),
            New QualityItem(25, "Very Low", "Very low quality and file size (-crf 25)"),
            New QualityItem(26, "Super Low", "Super low quality and file size (-rf 26)")}

        Encoder = enc
        Params = Encoder.Params

        cms = New ContextMenuStrip(components)

        lv.View = View.Details
        lv.HeaderStyle = ColumnHeaderStyle.None
        lv.FullRowSelect = True
        lv.MultiSelect = False
        lv.ContextMenuStrip = cms
        lv.ShowContextMenuOnLeftClick = True

        UpdateControls()

        AddHandler lv.UpdateContextMenu, AddressOf UpdateMenu
    End Sub

    Protected Overrides Sub OnLayout(e As LayoutEventArgs)
        MyBase.OnLayout(e)

        If lv.Columns.Count = 0 Then
            lv.Columns.AddRange({New ColumnHeader, New ColumnHeader})
        End If

        lv.Columns(0).Width = CInt(Width * (32 / 100))
        lv.Columns(1).Width = CInt(Width * (66 / 100))

        'couldn't get scaling to work trying everything
        llConfigCodec.Left = 5
        llConfigCodec.Top = Height - llConfigCodec.Height - 5

        llCompCheck.Left = 5
        llCompCheck.Top = Height - llConfigCodec.Height - llCompCheck.Height - 10

        llConfigContainer.Left = Width - llConfigContainer.Width - 5
        llConfigContainer.Top = Height - llConfigContainer.Height - 5
    End Sub

    Sub UpdateMenu()
        cms.Items.Clear()

        Dim offset = If(Params.Mode.Value = RateMode.SingleCRF, 0, 1)

        If lv.SelectedItems.Count > 0 Then
            Select Case lv.SelectedIndices(0)
                Case 0 - offset
                    For Each i In QualityDefinitions
                        cms.Items.Add(New ActionMenuItem(Of Single)(i.Value & " - " + i.Text, AddressOf SetQuality, i.Value, i.Tooltip) With {.Font = If(Params.Quant.Value = i.Value, New Font(.Font, FontStyle.Bold), .Font)})
                    Next
                Case 1 - offset
                    For x = 0 To Params.Preset.Options.Length - 1
                        cms.Items.Add(New ActionMenuItem(Of Integer)(
                                      Params.Preset.Options(x), AddressOf SetPreset, x,
                                      "Use values between Fast and Slower otherwise the quality and compression will either be poor or the encoding will be painful slow. Slower is three times slower than Medium, Veryslow is 6 times slower than Medium with little gains compared to Slower.") With {.Font = If(Params.Preset.Value = x, New Font(.Font, FontStyle.Bold), .Font)})
                    Next
                Case 2 - offset
                    For x = 0 To Params.Tune.Options.Length - 1
                        cms.Items.Add(New ActionMenuItem(Of Integer)(
                                      Params.Tune.Options(x), AddressOf SetTune, x) With {.Font = If(Params.Tune.Value = x, New Font(.Font, FontStyle.Bold), .Font)})
                    Next
            End Select
        End If
    End Sub

    Sub SetQuality(v As Single)
        Params.Quant.Value = v
        lv.Items(0).SubItems(1).Text = GetQualityCaption(v)
        lv.Items(0).Selected = False
        UpdateControls()
    End Sub

    Sub SetPreset(value As Integer)
        Dim offset = If(Params.Mode.Value = RateMode.SingleCRF, 0, 1)

        Params.Preset.Value = value
        Params.ApplyPresetValues()

        lv.Items(1 - offset).SubItems(1).Text = value.ToString
        lv.Items(1 - offset).Selected = False

        UpdateControls()
    End Sub

    Sub SetTune(value As Integer)
        Dim offset = If(Params.Mode.Value = RateMode.SingleCRF, 0, 1)

        Params.Tune.Value = value
        Params.ApplyTuneValues()

        lv.Items(2 - offset).SubItems(1).Text = value.ToString
        lv.Items(2 - offset).Selected = False

        UpdateControls()
    End Sub

    Function GetQualityCaption(value As Single) As String
        For Each i In QualityDefinitions
            If i.Value = value Then
                Return value & " - " + i.Text
            End If
        Next

        Return value.ToString
    End Function

    Sub UpdateControls()
        If Params.Mode.Value = RateMode.SingleCRF AndAlso lv.Items.Count < 4 Then
            lv.Items.Clear()
            lv.Items.Add(New ListViewItem({"Quality", GetQualityCaption(Params.Quant.Value)}))
            lv.Items.Add(New ListViewItem({"Preset", Params.Preset.OptionText}))
            lv.Items.Add(New ListViewItem({"Tune", Params.Tune.OptionText}))
        ElseIf Params.Mode.Value <> 2 AndAlso lv.Items.Count <> 3 Then
            lv.Items.Clear()
            lv.Items.Add(New ListViewItem({"Preset", Params.Preset.OptionText}))
            lv.Items.Add(New ListViewItem({"Tune", Params.Tune.OptionText}))
        End If

        Dim offset = If(Params.Mode.Value = RateMode.SingleCRF, 0, 1)
        llCompCheck.Visible = Params.Mode.Value = RateMode.TwoPass Or Params.Mode.Value = RateMode.ThreePass
    End Sub

    Private Sub llAdvanced_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llConfigCodec.LinkClicked
        Encoder.ShowConfigDialog()
    End Sub

    Private Sub llConfigContainer_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llConfigContainer.LinkClicked
        Encoder.OpenMuxerConfigDialog()
    End Sub

    Private Sub llCompCheck_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llCompCheck.LinkClicked
        Encoder.RunCompCheck()
    End Sub

    Class QualityItem
        Property Value As Single
        Property Text As String
        Property Tooltip As String

        Sub New(value As Single, text As String, tooltip As String)
            Me.Value = value
            Me.Text = text
            Me.Tooltip = tooltip
        End Sub
    End Class
End Class