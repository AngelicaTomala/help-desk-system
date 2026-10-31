# Exporta cada diapositiva de HelpDesk_Proyecto_Integrador.pptx a PNG en presentacion/preview/ con PowerPoint (COM).
# Uso: powershell -ExecutionPolicy Bypass -File presentacion/exportar-preview.ps1
$ErrorActionPreference = 'Stop'
$pptx = Join-Path $PSScriptRoot 'HelpDesk_Proyecto_Integrador.pptx'
$salida = Join-Path $PSScriptRoot 'preview'
New-Item -ItemType Directory -Force $salida | Out-Null
Get-ChildItem $salida -Filter *.png | Remove-Item -Force

$pp = New-Object -ComObject PowerPoint.Application
try {
    # Abrir sin ventana: ReadOnly, Untitled, WithWindow = false.
    $pres = $pp.Presentations.Open($pptx, $true, $false, $false)
    try {
        foreach ($slide in $pres.Slides) {
            $archivo = Join-Path $salida ('diapositiva-{0:D2}.png' -f ($slide.SlideIndex - 1))
            $slide.Export($archivo, 'PNG', 1600, 900)
        }
        "Exportadas $($pres.Slides.Count) diapositivas en $salida"
    }
    finally { $pres.Close() }
}
finally {
    $pp.Quit()
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($pp)
}
