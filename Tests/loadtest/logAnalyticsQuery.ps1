$ws = az monitor log-analytics workspace show -g ragsearch -n RagSearchLogAnalyticsWorkspace --query customerId -o tsv

az monitor log-analytics query -w $ws -o json --analytics-query @"
ContainerAppSystemLogs_CL
| where ContainerAppName_s == 'ragsearch-api'
| where TimeGenerated between (datetime(2026-11-03T22:00:00Z) .. datetime(2026-11-03T22:20:00Z))
| project TimeGenerated, Type_s, Reason_s, EventSource_s, ReplicaName_s, Log_s
| order by TimeGenerated asc
"@ | Tee-Object -FilePath Tests/loadtest/Results/system-events.json