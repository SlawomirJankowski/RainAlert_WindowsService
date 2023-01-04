# RainAlert_WindowsService

## OPIS

Rain Alert codziennie przed wyjściem do pracy/szkoły wyśle do użytkownika sms z informacją czy powinien zabrać ze sobą parasol, ponieważ w ciągu najbliższych  12 godzin w jego lokalizacji spodziewane są
opady deszczu, 
Usługa powinna zostać zainstalowana na komputerze/serwerze pracującym w trybie 24/7,

## Zasada działania:
1. Po uruchomieniu usługi pracę rozpoczyna TIMER, który w oparciu o aktualny czas i zaprogramowaną godzinę sprawdzenia prognozy pogody ustala "czas do pierwszego uruchomienia"
czyli za ile czasu będzie 6:00.
2. W oparciu o dane pobrane z ustawień aplikacji, generowane jest zapytanie do OpenWeather API.
3. W odpowiedzi z API, która otrzymujemy w formacie json, wyszukiwany jest kod pogody dla najbliższych 12 godzin.
4. Jeżeli kod jest mniejszy niż 700 to oznacza, że na tę godzinę prognozowane są opady. (szczegóły w dokumentacji OpenWeatherAPI)
5. W zależności od uzyskanych danych użytkownik otrzymuje sms ze stosową odpowiedzią.

## Ustawienia domyślne:
1. sprawdzanie prognozy pogody i wysyłka sms: codziennie o 6:00
2. lokalizacja: Warszawa

## Konfiguracja:
1. w pliku .config,
2. dodatkowo przy uruchamianiu usługi jest możliwość przekazania współrzędnych nowej lokalizacji, np. Twojej miejscowości bez potrzeby wprowadzania zmian w pliku .config *(W oknie usług Windows, klikamy prawym klawiszem na usłudze Rain Alert, dalej Właściwości --> w pole "Parametry uruchomienia" wprowadzamy nowe współrzędne w formacie {xx.xx yy.yy} (bez klamr)
--> klikamy Uruchom --> Ok}*

## Zastosowane biblioteki / żródła danych:
1. Obsługa logów - Serilog: https://github.com/serilog
2. Obsługa formatu json: Newtonsoft.Json: https://www.newtonsoft.com/json
3. Serwis SMS - Twilio: https://www.twilio.com/?g=%2F
4. Dane pogodowe - OpenWeather: https://openweathermap.org/
5. Współrzędne geograficzne - LatLong: https://www.latlong.net/


