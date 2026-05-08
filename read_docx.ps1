Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead('c:\subtrackers\project_ description.docx')
$entry = $zip.GetEntry('word/document.xml')
$reader = new-object System.IO.StreamReader($entry.Open())
$xml = $reader.ReadToEnd()
$reader.Close()
$zip.Dispose()
$xml -replace '<[^>]+>', ' '
