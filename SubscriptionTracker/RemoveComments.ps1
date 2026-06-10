$path = "x:\PROJEKT WPF\subtrackers\SubscriptionTracker"

Write-Host "Processing CS files..."
$csFiles = Get-ChildItem -Path $path -Filter *.cs -Recurse
foreach ($f in $csFiles) {
    $text = [System.IO.File]::ReadAllText($f.FullName)
    
    # XML comments
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, '(?m)^\s*///.*$\r?\n', '')
    
    # Block comments
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, '(?s)/\*.*?\*/', '')
    
    # Single line comments (skip if preceded by : e.g., http://)
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, '(?m)[ \t]*(?<!:)//.*$', '')
    
    [System.IO.File]::WriteAllText($f.FullName, $text, [System.Text.Encoding]::UTF8)
}

Write-Host "Processing XAML files..."
$xamlFiles = Get-ChildItem -Path $path -Filter *.xaml -Recurse
foreach ($f in $xamlFiles) {
    $text = [System.IO.File]::ReadAllText($f.FullName)
    
    # Remove HTML/XAML comments
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, '(?s)[ \t]*<!--.*?-->\r?\n?', '')
    
    [System.IO.File]::WriteAllText($f.FullName, $text, [System.Text.Encoding]::UTF8)
}
Write-Host "Done."
