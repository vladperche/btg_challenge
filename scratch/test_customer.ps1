$headers = @{ 
    "X-Api-Key" = "BTG_PROTOTYPING_SECRET_KEY_12345" 
}

$jsonBody = @'
{
  "name": "Alex Smith",
  "age": 30,
  "score": 750,
  "has_market_debt": false,
  "market_debt_types": [],
  "location": {
    "city": "Sao Paulo",
    "state": "SP",
    "region": "Sudeste"
  },
  "job_title": "Senior Software Engineer"
}
'@

$response = Invoke-RestMethod -Uri "http://localhost:8080/api/customers/classify" -Method Post -Headers $headers -Body $jsonBody -ContentType "application/json; charset=utf-8"
$response | ConvertTo-Json -Depth 5
