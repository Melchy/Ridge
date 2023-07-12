## RequestResponseLogging

[See documentation for more information.](https://github.com/Melchy/Ridge/wiki/4.-Request-response-logging)

Logger generates following output for this example:

```text
Request:
Time when request was sent: 22:20:38:164
Method: GET, RequestUri: '/WeatherForecast', Version: 1.1, Content: <null>, Headers:
{
  ridgeCallId: 6ffa2832-238d-4486-a3a6-07eb885c5add
}
Body:


Response:
Time when response was received: 22:20:38:233
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
{
  Content-Type: application/json; charset=utf-8
}
Body:
[{"date":"2023-07-12","temperatureC":1,"summary":"Cool"}]

```