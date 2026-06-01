# Detectar GUIDs referenciados que no existen en .meta
$metaGuids = Get-ChildItem -Path .\Assets -Recurse -Filter *.meta | ForEach-Object {
    $m = Select-String -Path $_.FullName -Pattern '^guid: ([0-9a-fA-F]+)' -ErrorAction SilentlyContinue
    if ($m) { $m.Matches[0].Groups[1].Value }
}
$metaGuids = $metaGuids | Where-Object { $_ } | Sort-Object -Unique

$refs = @()
$files = Get-ChildItem -Path .\Assets -Recurse -Include *.prefab,*.unity,*.asset,*.renderTexture,*.mat,*.controller,*.anim -File
foreach ($f in $files) {
    $matches = Select-String -Path $f.FullName -Pattern 'guid: ([0-9a-fA-F]+)' -AllMatches -ErrorAction SilentlyContinue
    foreach ($m in $matches) {
        foreach ($match in $m.Matches) {
            $g = $match.Groups[1].Value
            if (-not ($metaGuids -contains $g)) {
                $refs += [PSCustomObject]@{ File = $f.FullName; Line = $m.LineNumber; Guid = $g }
            }
        }
    }
}

$out = '.\verify_missing_guids.txt'
if ($refs.Count -eq 0) {
    'No missing GUID references found.' | Out-File -FilePath $out -Encoding utf8
} else {
    $refs | Format-Table | Out-String | Out-File -FilePath $out -Encoding utf8
}
Get-Content $out -Tail 200
