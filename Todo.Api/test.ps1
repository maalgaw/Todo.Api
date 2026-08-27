try {
    Invoke-WebRequest -Uri "http://localhost:5001/api/auth/register" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"username":"testuser2","password":"password123"}'
} catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $reader.ReadToEnd()
}
