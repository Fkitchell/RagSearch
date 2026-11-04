k6 run -e BASE_URL=https://ragsearch-api.kindsand-3533e7f9.eastus.azurecontainerapps.io `
    --summary-export Tests/loadtest/results/summary.json `
    --out csv=Tests/loadtest/results/metrics.csv.gz `
    Tests/loadtest/http-scale.js