az containerapp logs show -n ragsearch-api -g ragsearch --type system --tail 300 --format json > Tests/loadtest/Results/system-events.log

#$OutputFile = "Tests/loadtest/Results/containerlogs.log"
#$AppName = "ragsearch-api"
#
#try {
#  az containerapp logs show `
#    -n $AppName `
#    -g ragsearch `
#    -t system `
#    --query "[?contains(Log, 'Replica') || contains(Log, 'Scale') || contains(Log, 'scaling')]" `
#    --follow true `
#    --format json | ForEach-Object {
#
#    $line = $_
#    # Isolate lines belonging to your app and flag scaling keywords
#    # Keywords track: replica counts changing, scale actions, or KEDA triggers
#    if ($line -like "*$AppName*" -and ($line -match "scale|replica|replicaUpdated|keda|hpa|minimum|maximum")) {
#
#      # Print clearly to the window during the load test
#      Write-Host $line -ForegroundColor Green
#
#      # Write immediately to file
#      $line | Out-File -FilePath $OutputFile -Append -Encoding utf8
#    }
#  }
#}
#catch {
#  Write-Error "An error occurred during log collection: $_"
#}
#finally {
#  Write-Host "`n[-] Log streaming stopped. Final logs preserved in $OutputFile" -ForegroundColor Yellow
#}
