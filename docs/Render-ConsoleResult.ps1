param(
    [string]$LogPath = "$PSScriptRoot/examples/console-output.txt",
    [string]$OutputPath = "$PSScriptRoot/images/console-result.png"
)

# Render the saved, unmodified execution log; this is not a window screenshot.
Add-Type -AssemblyName System.Drawing
$lines = Get-Content -LiteralPath $LogPath -Encoding UTF8
$font = [System.Drawing.Font]::new('Malgun Gothic', 17, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
$titleFont = [System.Drawing.Font]::new('Malgun Gothic', 23, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$measure = [System.Drawing.Bitmap]::new(1, 1)
$measureGraphics = [System.Drawing.Graphics]::FromImage($measure)
$maxWidth = 0
foreach ($line in $lines) {
    $maxWidth = [Math]::Max($maxWidth, $measureGraphics.MeasureString($line, $font).Width)
}
$width = [int][Math]::Ceiling([Math]::Max(1100, $maxWidth + 80))
$height = 125 + $lines.Count * 29 + 30
$bitmap = [System.Drawing.Bitmap]::new($width, $height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#101827'))
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$white = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#dce5f2'))
$accent = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#70dfbb'))
$muted = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#94a8c4'))
try {
    $graphics.DrawString('LOTTO / CONSOLE RESULT', $titleFont, $white, 32, 22)
    $graphics.DrawString('실제 실행 로그 · 기존 캐시 사용 · 1223~1248회차 분석', $font, $muted, 32, 62)
    $y = 112
    foreach ($line in $lines) {
        $brush = if ($line.StartsWith('[') -or $line -match '^\d게임:') { $accent } else { $white }
        $graphics.DrawString($line, $font, $brush, 32, $y)
        $y += 29
    }
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
} finally {
    $graphics.Dispose()
    $bitmap.Dispose()
    $measureGraphics.Dispose()
    $measure.Dispose()
    $font.Dispose()
    $titleFont.Dispose()
    $white.Dispose()
    $accent.Dispose()
    $muted.Dispose()
}
