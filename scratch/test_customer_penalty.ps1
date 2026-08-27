$headers = @{ 
    "X-Api-Key" = "BTG_PROTOTYPING_SECRET_KEY_12345" 
}

$jsonBody = @'
{
  "name": "Maria Oliver",
  "age": 30,
  "score": 750,
  "has_market_debt": true,
  "market_debt_types": ["credit_default"],
  "location": {
    "city": "Rio de Janeiro",
    "state": "RJ",
    "region": "Sudeste"
  },
  "job_title": "Senior Software Engineer"
}
'@

$response = Invoke-RestMethod -Uri "http://localhost:8080/api/customers" -Method Post -Headers $headers -Body $jsonBody -ContentType "application/json; charset=utf-8"
$response | ConvertTo-Json -Depth 5
