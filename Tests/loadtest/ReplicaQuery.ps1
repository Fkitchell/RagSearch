& {
    while ($true) {
        $azLauncher = Get-Command az.cmd -ErrorAction Stop
        $python = Resolve-Path (Join-Path (Split-Path $azLauncher.Source) '..\python.exe')
        $replicaCount = & $python.Path -IBm azure.cli containerapp replica list `
            --name ragsearch-api `
            --resource-group ragsearch `
            --query 'length(@)' `
            --output tsv

        if ($LASTEXITCODE -ne 0) {
            throw "Azure CLI replica query failed with exit code $LASTEXITCODE."
        }

        "{0} {1}" -f (Get-Date -Format HH:mm:ss), $replicaCount.Trim()
        Start-Sleep -Seconds 10
    }
} | Tee-Object -FilePath Tests/loadtest/Results/replicas.log -Append