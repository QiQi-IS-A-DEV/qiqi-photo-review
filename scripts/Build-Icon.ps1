param([string]$Source = (Join-Path $PSScriptRoot '..\Assets\qiqistudio_nobackground.png'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$taskImage = [System.Drawing.Image]::FromFile((Resolve-Path -LiteralPath $Source).Path)
$taskFrames = @()
try {
    foreach ($taskSize in @(16, 24, 32, 48, 64, 128, 256)) {
        $taskBitmap = [System.Drawing.Bitmap]::new($taskSize, $taskSize)
        $taskGraphics = [System.Drawing.Graphics]::FromImage($taskBitmap)
        $taskStream = [System.IO.MemoryStream]::new()
        try {
            $taskGraphics.Clear([System.Drawing.Color]::Transparent)
            $taskGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $taskGraphics.DrawImage($taskImage, 0, 0, $taskSize, $taskSize)
            $taskBitmap.Save($taskStream, [System.Drawing.Imaging.ImageFormat]::Png)
            $taskFrames += [pscustomobject]@{ Size = $taskSize; Bytes = $taskStream.ToArray() }
        } finally { $taskGraphics.Dispose(); $taskBitmap.Dispose(); $taskStream.Dispose() }
    }
    $taskOutput = [System.IO.File]::Create((Join-Path $PSScriptRoot '..\Assets\qiqistudio.ico'))
    $taskWriter = [System.IO.BinaryWriter]::new($taskOutput)
    try {
        $taskWriter.Write([uint16]0); $taskWriter.Write([uint16]1); $taskWriter.Write([uint16]$taskFrames.Count)
        $taskOffset = 6 + 16 * $taskFrames.Count
        foreach ($taskFrame in $taskFrames) {
            $taskDimension = if ($taskFrame.Size -eq 256) { 0 } else { $taskFrame.Size }
            $taskWriter.Write([byte]$taskDimension); $taskWriter.Write([byte]$taskDimension)
            $taskWriter.Write([byte]0); $taskWriter.Write([byte]0)
            $taskWriter.Write([uint16]1); $taskWriter.Write([uint16]32)
            $taskWriter.Write([uint32]$taskFrame.Bytes.Length); $taskWriter.Write([uint32]$taskOffset)
            $taskOffset += $taskFrame.Bytes.Length
        }
        foreach ($taskFrame in $taskFrames) { $taskWriter.Write([byte[]]$taskFrame.Bytes) }
    } finally { $taskWriter.Dispose(); $taskOutput.Dispose() }
} finally { $taskImage.Dispose() }
